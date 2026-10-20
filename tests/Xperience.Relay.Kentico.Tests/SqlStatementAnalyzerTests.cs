using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class SqlStatementAnalyzerTests
{
    [Theory]
    [InlineData("SELECT * FROM Relay_ApiKey")]
    [InlineData("select ApiKeyHash from relay_apikey where ApiKeyID = 1")]
    [InlineData("SELECT * FROM [dbo].[Relay_ApiKey]")]
    [InlineData("EXEC('SELECT * FROM Relay_ApiKey')")]
    public void TheApiKeyTable_IsRejectedInBothModes(string sql)
    {
        foreach (var readOnly in new[] { true, false })
        {
            var analysis = SqlStatementAnalyzer.Analyze(sql, readOnly);

            Assert.False(analysis.IsValid);
            Assert.Contains("protected", analysis.ErrorMessage);
        }
    }

    [Theory]
    [InlineData("UPDATE Relay_ApiKey SET ApiKeyAccess = 3")]
    [InlineData("DELETE FROM Relay_ApiKey")]
    [InlineData("INSERT INTO Relay_ApiKey (ApiKeyAccess) VALUES (3)")]
    [InlineData("DROP TABLE Relay_ApiKey")]
    public void ChangingTheApiKeyTable_IsRejectedEvenWhenWritesAreAllowed(string sql)
    {
        var analysis = SqlStatementAnalyzer.Analyze(sql, readOnly: false);

        Assert.False(analysis.IsValid);
        Assert.Contains("protected", analysis.ErrorMessage);
    }

    [Theory]
    [InlineData("SELECT 1")]
    [InlineData("SELECT UserName FROM CMS_User WHERE UserID = 1")]
    public void OrdinaryReads_StillPass(string sql)
    {
        Assert.True(SqlStatementAnalyzer.Analyze(sql, readOnly: true).IsValid);
        Assert.True(SqlStatementAnalyzer.Analyze(sql, readOnly: false).IsValid);
    }

    [Fact]
    public void Writes_AreOnlyAllowedWhenNotReadOnly()
    {
        const string sql = "UPDATE CMS_User SET UserEnabled = 1 WHERE UserID = 1";

        Assert.False(SqlStatementAnalyzer.Analyze(sql, readOnly: true).IsValid);
        Assert.True(SqlStatementAnalyzer.Analyze(sql, readOnly: false).IsValid);
    }

    [Theory]
    [InlineData("SELECT TagID INTO Docs_Copy FROM CMS_Tag")]
    [InlineData("SELECT * INTO [dbo].[Docs_Copy] FROM CMS_Tag")]
    public void SelectInto_CreatesATable_SoItIsNotReadOnly(string sql)
    {
        var readOnly = SqlStatementAnalyzer.Analyze(sql, readOnly: true);

        Assert.False(readOnly.IsValid);
        Assert.Contains("INTO", readOnly.ErrorMessage);
        Assert.True(SqlStatementAnalyzer.Analyze(sql, readOnly: false).IsValid);
    }

    [Fact]
    public void SelectInto_ATemporaryTable_IsStillFine()
    {
        Assert.True(SqlStatementAnalyzer.Analyze("SELECT TagID INTO #tags FROM CMS_Tag; SELECT * FROM #tags", readOnly: true).IsValid);
    }

    [Fact]
    public void DangerousFunctions_AreBlockedInBothModes()
    {
        foreach (var readOnly in new[] { true, false })
        {
            Assert.False(SqlStatementAnalyzer.Analyze("EXEC xp_cmdshell 'dir'", readOnly).IsValid);
        }
    }

    [Fact]
    public void Summaries_NameTheStatementAndTable()
    {
        var analysis = SqlStatementAnalyzer.Analyze("UPDATE CMS_User SET UserEnabled = 1", readOnly: false);

        Assert.Equal(["UPDATE [CMS_User]"], analysis.StatementSummaries);
    }
}
