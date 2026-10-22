using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>The SQL a check let through, or why it didn't.</summary>
internal sealed record CheckedSql(string? Sql, string? Error)
{
    public static CheckedSql Valid(string sql) => new(sql, null);

    public static CheckedSql Invalid(string error) => new(null, error);
}

/// <summary>
/// Checks the two pieces of SQL that <c>get-form-submissions</c> takes, a condition and an ordering, which Kentico would
/// otherwise run as written. A condition may compare the form's columns with values and combine the comparisons; an
/// ordering may list the form's columns. Everything else, such as a function call, a subquery, a comment or a second
/// statement, is refused. What comes back is not the text that was sent but SQL generated from what was checked, so
/// nothing that wasn't looked at can reach the database.
/// </summary>
internal static class FormFilterChecker
{
    public const int MaxConditionLength = 2000;
    public const int MaxOrderByLength = 500;

    private const int MaxTokens = 400;
    private const int MaxNesting = 8;
    private const int MaxListValues = 100;
    private const int MaxOrderColumns = 10;
    private const int MaxRefusedShown = 3;

    private const string ConditionHint =
        "A condition can compare columns with values (=, <>, !=, <, >, <=, >=, LIKE, IN, BETWEEN, IS NULL) and combine them with AND, OR, NOT and parentheses.";

    private const string OrderByHint = "OrderBy takes columns separated by commas, each optionally followed by ASC or DESC.";

    private static readonly HashSet<TSqlTokenType> ConditionTokens =
    [
        TSqlTokenType.Identifier, TSqlTokenType.QuotedIdentifier,
        TSqlTokenType.And, TSqlTokenType.Or, TSqlTokenType.Not, TSqlTokenType.Like, TSqlTokenType.Escape, TSqlTokenType.In,
        TSqlTokenType.Is, TSqlTokenType.Null, TSqlTokenType.Between,
        TSqlTokenType.EqualsSign, TSqlTokenType.LessThan, TSqlTokenType.GreaterThan, TSqlTokenType.Bang,
        TSqlTokenType.LeftParenthesis, TSqlTokenType.RightParenthesis, TSqlTokenType.Comma, TSqlTokenType.Minus,
        TSqlTokenType.Integer, TSqlTokenType.Numeric, TSqlTokenType.Real,
        TSqlTokenType.AsciiStringLiteral, TSqlTokenType.UnicodeStringLiteral,
    ];

    private static readonly HashSet<TSqlTokenType> OrderByTokens =
    [
        TSqlTokenType.Identifier, TSqlTokenType.QuotedIdentifier, TSqlTokenType.Asc, TSqlTokenType.Desc, TSqlTokenType.Comma,
    ];

    private static readonly HashSet<BooleanComparisonType> Comparisons =
    [
        BooleanComparisonType.Equals, BooleanComparisonType.NotEqualToBrackets, BooleanComparisonType.NotEqualToExclamation,
        BooleanComparisonType.LessThan, BooleanComparisonType.GreaterThan, BooleanComparisonType.LessThanOrEqualTo,
        BooleanComparisonType.GreaterThanOrEqualTo, BooleanComparisonType.NotLessThan, BooleanComparisonType.NotGreaterThan,
    ];

    /// <param name="condition">The condition, without the <c>WHERE</c> keyword.</param>
    /// <param name="formName">Used in the messages.</param>
    /// <param name="knownColumns">The columns of the form's table.</param>
    public static CheckedSql CheckCondition(string condition, string formName, IReadOnlyCollection<string> knownColumns)
    {
        var scan = Scan(condition, "WhereClause", ConditionTokens, MaxConditionLength, ConditionHint);

        if (scan.Error is not null)
        {
            return CheckedSql.Invalid(scan.Error);
        }

        var depth = 0;

        foreach (var token in scan.Tokens)
        {
            depth += token.TokenType switch
            {
                TSqlTokenType.LeftParenthesis => 1,
                TSqlTokenType.RightParenthesis => -1,
                _ => 0,
            };

            if (depth > MaxNesting)
            {
                return CheckedSql.Invalid($"WhereClause nests parentheses more than {MaxNesting} deep.");
            }
        }

        var parser = new TSql170Parser(true);
        using var reader = new StringReader(condition);
        var expression = parser.ParseBooleanExpression(reader, out var errors);

        if (errors.Count > 0 || expression is null)
        {
            return CheckedSql.Invalid($"WhereClause isn't a valid condition: {errors.FirstOrDefault()?.Message ?? "it can't be read"} {ConditionHint}");
        }

        var validator = new ConditionValidator(Lookup(knownColumns), condition);
        validator.Check(expression);

        var problems = new List<string>();

        if (validator.Refused.Count > 0)
        {
            var shown = validator.Refused.Distinct().Take(MaxRefusedShown).Select(text => $"'{QueryInputChecker.Printable(text)}'");
            problems.Add($"WhereClause can't contain {string.Join(", ", shown)}. {ConditionHint}");
        }

        if (validator.Unknown.Count > 0)
        {
            problems.Add(ObjectColumnChecker.Check(formName, knownColumns, validator.Unknown, "form")
                ?? $"Column '{validator.Unknown[0]}' was not found on form '{formName}'.");
        }

        if (problems.Count > 0)
        {
            return CheckedSql.Invalid(string.Join(" ", problems));
        }

        var generator = new Sql170ScriptGenerator(new SqlScriptGeneratorOptions
        {
            IncludeSemicolons = false,
            KeywordCasing = KeywordCasing.Uppercase,
            MultilineWherePredicatesList = false,
        });

        generator.GenerateScript(expression, out var sql);

        return CheckedSql.Valid(sql);
    }

    /// <param name="orderBy">The ordering, without the <c>ORDER BY</c> keywords.</param>
    /// <param name="formName">Used in the messages.</param>
    /// <param name="knownColumns">The columns of the form's table.</param>
    public static CheckedSql CheckOrderBy(string orderBy, string formName, IReadOnlyCollection<string> knownColumns)
    {
        var scan = Scan(orderBy, "OrderBy", OrderByTokens, MaxOrderByLength, OrderByHint);

        if (scan.Error is not null)
        {
            return CheckedSql.Invalid(scan.Error);
        }

        var items = new List<List<TSqlParserToken>> { new() };

        foreach (var token in scan.Tokens)
        {
            if (token.TokenType == TSqlTokenType.Comma)
            {
                items.Add([]);
            }
            else
            {
                items[^1].Add(token);
            }
        }

        var shapeIsRight = items.Count <= MaxOrderColumns
            && items.All(item => item.Count is 1 or 2
                && item[0].TokenType is TSqlTokenType.Identifier or TSqlTokenType.QuotedIdentifier
                && (item.Count == 1 || item[1].TokenType is TSqlTokenType.Asc or TSqlTokenType.Desc));

        if (!shapeIsRight)
        {
            return CheckedSql.Invalid($"OrderBy isn't a list of up to {MaxOrderColumns} columns. {OrderByHint}");
        }

        var known = Lookup(knownColumns);
        var unknown = new List<string>();
        var parts = new List<string>();

        foreach (var item in items)
        {
            var name = ColumnName(item[0]);

            if (!known.TryGetValue(name, out var canonical))
            {
                unknown.Add(name);
                continue;
            }

            parts.Add(item.Count == 2 ? $"[{canonical}] {item[1].Text.ToUpperInvariant()}" : $"[{canonical}]");
        }

        if (unknown.Count > 0)
        {
            return CheckedSql.Invalid(ObjectColumnChecker.Check(formName, knownColumns, unknown, "form")
                ?? $"Column '{unknown[0]}' was not found on form '{formName}'.");
        }

        return CheckedSql.Valid(string.Join(", ", parts));
    }

    private sealed record ScanResult(List<TSqlParserToken> Tokens, string? Error);

    /// <summary>
    /// Reads the text as T-SQL tokens and refuses any that aren't in <paramref name="allowed"/>. Comments are refused, and
    /// so is any whitespace other than the plain kind, since a different reading of it by the database would change what
    /// the text means.
    /// </summary>
    private static ScanResult Scan(string text, string what, HashSet<TSqlTokenType> allowed, int maxLength, string hint)
    {
        if (text.Length > maxLength)
        {
            return new ScanResult([], $"{what} can be at most {maxLength} characters.");
        }

        var parser = new TSql170Parser(true);
        using var reader = new StringReader(text);
        var tokens = parser.GetTokenStream(reader, out var errors);

        if (errors.Count > 0)
        {
            return new ScanResult([], $"{what} isn't valid: {errors[0].Message}");
        }

        var significant = new List<TSqlParserToken>();

        foreach (var token in tokens)
        {
            switch (token.TokenType)
            {
                case TSqlTokenType.EndOfFile:
                    continue;

                case TSqlTokenType.WhiteSpace:
                    if (token.Text.Any(c => c is not (' ' or '\t' or '\r' or '\n')))
                    {
                        return new ScanResult([], $"{what} contains a character that isn't allowed.");
                    }

                    continue;

                case TSqlTokenType.SingleLineComment or TSqlTokenType.MultilineComment:
                    return new ScanResult([], $"{what} can't contain comments.");
            }

            var bracketOnly = token.TokenType == TSqlTokenType.QuotedIdentifier && !IsBracketed(token.Text);

            if (!allowed.Contains(token.TokenType) || bracketOnly)
            {
                return new ScanResult([], $"{what} can't contain '{QueryInputChecker.Printable(token.Text)}'. {hint}");
            }

            significant.Add(token);

            if (significant.Count > MaxTokens)
            {
                return new ScanResult([], $"{what} is too long.");
            }
        }

        return new ScanResult(significant, null);
    }

    private static bool IsBracketed(string text) => text.Length >= 2 && text[0] == '[' && text[^1] == ']';

    private static string ColumnName(TSqlParserToken token) =>
        token.TokenType == TSqlTokenType.QuotedIdentifier ? token.Text[1..^1] : token.Text;

    private static Dictionary<string, string> Lookup(IEnumerable<string> columns)
    {
        var known = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in columns)
        {
            known.TryAdd(column, column);
        }

        return known;
    }

    /// <summary>
    /// Walks a parsed condition and refuses every part of it that isn't a comparison of columns and values. A part that
    /// isn't known is refused, not allowed: the list of what is accepted is the whole of the check.
    /// </summary>
    private sealed class ConditionValidator(Dictionary<string, string> known, string source)
    {
        /// <summary>The parts of the condition that aren't allowed, as they were written.</summary>
        public List<string> Refused { get; } = [];

        /// <summary>The columns the condition names that the form doesn't have.</summary>
        public List<string> Unknown { get; } = [];

        public void Check(BooleanExpression expression)
        {
            switch (expression)
            {
                case BooleanBinaryExpression binary:
                    Check(binary.FirstExpression);
                    Check(binary.SecondExpression);
                    break;

                case BooleanNotExpression not:
                    Check(not.Expression);
                    break;

                case BooleanParenthesisExpression parenthesis:
                    Check(parenthesis.Expression);
                    break;

                case BooleanComparisonExpression comparison when Comparisons.Contains(comparison.ComparisonType):
                    Check(comparison.FirstExpression);
                    Check(comparison.SecondExpression);
                    break;

                case BooleanIsNullExpression isNull:
                    Check(isNull.Expression);
                    break;

                case BooleanTernaryExpression between:
                    Check(between.FirstExpression);
                    Check(between.SecondExpression);
                    Check(between.ThirdExpression);
                    break;

                case LikePredicate like:
                    Check(like.FirstExpression);
                    Check(like.SecondExpression);

                    if (like.EscapeExpression is { } escape)
                    {
                        Check(escape);
                    }

                    break;

                case InPredicate { Subquery: null } inList when inList.Values.Count <= MaxListValues:
                    Check(inList.Expression);

                    foreach (var value in inList.Values)
                    {
                        Check(value);
                    }

                    break;

                default:
                    Refuse(expression);
                    break;
            }
        }

        private void Check(ScalarExpression expression)
        {
            switch (expression)
            {
                case ColumnReferenceExpression column:
                    CheckColumn(column);
                    break;

                case StringLiteral or IntegerLiteral or NumericLiteral or RealLiteral or NullLiteral:
                    break;

                case UnaryExpression
                {
                    UnaryExpressionType: UnaryExpressionType.Negative,
                    Expression: IntegerLiteral or NumericLiteral or RealLiteral,
                }:
                    break;

                case ParenthesisExpression parenthesis:
                    Check(parenthesis.Expression);
                    break;

                default:
                    Refuse(expression);
                    break;
            }
        }

        private void CheckColumn(ColumnReferenceExpression column)
        {
            var identifiers = column.MultiPartIdentifier?.Identifiers;

            if (column.ColumnType != ColumnType.Regular || column.Collation is not null || identifiers is not { Count: 1 })
            {
                Refuse(column);
                return;
            }

            var identifier = identifiers[0];

            if (identifier.QuoteType == QuoteType.DoubleQuote)
            {
                Refuse(column);
            }
            else if (known.TryGetValue(identifier.Value, out var canonical))
            {
                // What is generated afterwards names the column as the form does, in brackets.
                identifier.Value = canonical;
                identifier.QuoteType = QuoteType.SquareBracket;
            }
            else
            {
                Unknown.Add(identifier.Value);
            }
        }

        private void Refuse(TSqlFragment fragment)
        {
            var inRange = fragment.StartOffset >= 0 && fragment.FragmentLength > 0 && fragment.StartOffset + fragment.FragmentLength <= source.Length;
            Refused.Add(inRange ? source.Substring(fragment.StartOffset, fragment.FragmentLength) : fragment.GetType().Name);
        }
    }
}
