using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Xperience.Relay.Kentico.Internal;

internal sealed class SqlStatementAnalysis
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> StatementSummaries { get; init; } = [];
}

/// <summary>
/// Parses and validates T-SQL using the ScriptDom AST. Two modes:
/// read-only (SELECT/DECLARE only) and read-write (also INSERT/UPDATE/DELETE/
/// CREATE/ALTER/DROP/EXECUTE). Dangerous functions are blocked in both modes.
/// </summary>
internal static class SqlStatementAnalyzer
{
    private static readonly TSql170Parser Parser = new(true);

    private static readonly HashSet<string> DangerousTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "XP_CMDSHELL", "OPENROWSET", "OPENDATASOURCE", "OPENQUERY", "OPENXML", "SP_CONFIGURE",
    };

    public static SqlStatementAnalysis Analyze(string sql, bool readOnly)
    {
        using var reader = new StringReader(sql);
        var fragment = Parser.Parse(reader, out var errors) as TSqlScript;

        if (errors.Count > 0)
        {
            return Fail($"SQL parsing error: {string.Join("; ", errors.Select(e => e.Message))}");
        }

        // Token-level scan blocks dangerous names regardless of syntactic context
        // (e.g. EXEC xp_cmdshell '...', OPENDATASOURCE workaround for parser gap).
        foreach (var token in fragment!.ScriptTokenStream)
        {
            if (!string.IsNullOrEmpty(token.Text) && DangerousTokens.Contains(token.Text))
            {
                return Fail($"'{token.Text.ToUpperInvariant()}' is not allowed.");
            }
        }

        var summaries = new List<string>();

        foreach (var batch in fragment.Batches)
        {
            foreach (var statement in batch.Statements)
            {
                if (!IsAllowed(statement, readOnly))
                {
                    return Fail($"'{GetVerb(statement)}' statements are not allowed in this context.");
                }

                summaries.Add(BuildSummary(statement));
            }
        }

        return new SqlStatementAnalysis { IsValid = true, StatementSummaries = summaries };
    }

    private static bool IsAllowed(TSqlStatement statement, bool readOnly)
    {
        if (statement is SelectStatement or DeclareVariableStatement)
        {
            return true;
        }

        if (readOnly)
        {
            return false;
        }

        return statement is InsertStatement or UpdateStatement or DeleteStatement
            or TruncateTableStatement or ExecuteStatement
            || statement.GetType().Name.StartsWith("Create", StringComparison.OrdinalIgnoreCase)
            || statement.GetType().Name.StartsWith("Drop", StringComparison.OrdinalIgnoreCase)
            || statement.GetType().Name.StartsWith("Alter", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildSummary(TSqlStatement statement)
    {
        var verb = GetVerb(statement);
        var tables = CollectTableNames(statement);
        return tables.Count > 0
            ? $"{verb} [{string.Join(", ", tables)}]"
            : verb;
    }

    private static string GetVerb(TSqlStatement statement) => statement switch
    {
        SelectStatement => "SELECT",
        InsertStatement => "INSERT",
        UpdateStatement => "UPDATE",
        DeleteStatement => "DELETE",
        TruncateTableStatement => "TRUNCATE",
        ExecuteStatement => "EXECUTE",
        DeclareVariableStatement => "DECLARE",
        _ => statement.GetType().Name
                .Replace("Statement", "", StringComparison.OrdinalIgnoreCase)
                .ToUpperInvariant(),
    };

    private static List<string> CollectTableNames(TSqlStatement statement)
    {
        var collector = new TableNameCollector();
        statement.Accept(collector);
        return [.. collector.TableNames];
    }

    private static SqlStatementAnalysis Fail(string message) =>
        new() { IsValid = false, ErrorMessage = message };

    private sealed class TableNameCollector : TSqlFragmentVisitor
    {
        public HashSet<string> TableNames { get; } = new(StringComparer.OrdinalIgnoreCase);

        public override void Visit(NamedTableReference node)
        {
            var name = node.SchemaObject?.BaseIdentifier?.Value;
            if (name is not null)
            {
                TableNames.Add(name);
            }
        }

        public override void Visit(CreateTableStatement node)
        {
            var name = node.SchemaObjectName?.BaseIdentifier?.Value;
            if (name is not null)
            {
                TableNames.Add(name);
            }
        }

        public override void Visit(DropTableStatement node)
        {
            foreach (var obj in node.Objects)
            {
                var name = obj.BaseIdentifier?.Value;
                if (name is not null)
                {
                    TableNames.Add(name);
                }
            }
        }
    }
}
