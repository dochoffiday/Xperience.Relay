using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class FormFilterCheckerTests
{
    private const string Form = "BizForm.ContactUs";

    private static readonly string[] Columns =
        ["ContactUsID", "FormInserted", "FormUpdated", "UserFirstName", "UserLastName", "UserEmail", "UserMessage", "Age"];

    private static CheckedSql Condition(string condition) => FormFilterChecker.CheckCondition(condition, Form, Columns);

    private static CheckedSql OrderBy(string orderBy) => FormFilterChecker.CheckOrderBy(orderBy, Form, Columns);

    [Theory]
    [InlineData("UserEmail LIKE 'grace@%'")]
    [InlineData("FormInserted > '2026-10-01' AND (UserEmail IS NOT NULL OR UserLastName = 'O''Hara')")]
    [InlineData("Age BETWEEN 18 AND 65")]
    [InlineData("UserLastName IN ('Hopper', 'Lovelace')")]
    [InlineData("NOT (Age < -1) AND Age >= 1.5")]
    [InlineData("Age <> 3 OR Age != 4 OR Age !< 5")]
    [InlineData("[UserEmail] = N'grace@example.com'")]
    [InlineData("UserMessage NOT LIKE '50!%%' ESCAPE '!'")]
    [InlineData("UserEmail = UserMessage")]
    public void OrdinaryConditions_Pass(string condition)
    {
        var result = Condition(condition);

        Assert.Null(result.Error);
        Assert.NotNull(result.Sql);
    }

    [Fact]
    public void AColumn_IsNamedAsTheFormNamesIt_InBrackets()
    {
        var result = Condition("useremail like 'grace@%' and AGE > 3");

        Assert.Null(result.Error);
        Assert.Equal("[UserEmail] LIKE 'grace@%' AND [Age] > 3", result.Sql);
    }

    [Fact]
    public void AValueWithAQuote_StaysOneValue()
    {
        var result = Condition("UserLastName = 'O''Hara'");

        Assert.Equal("[UserLastName] = 'O''Hara'", result.Sql);
    }

    [Fact]
    public void SayingWhatTheValueIs_ThatMakesAConditionTrue_IsStillJustACondition()
    {
        // The check is about what a condition can contain, not about what it matches.
        Assert.Null(Condition("UserEmail = 'x' OR 1 = 1").Error);
    }

    [Theory]
    [InlineData("LEN(UserEmail) > 3", "LEN(UserEmail)")]
    [InlineData("UserEmail = GETDATE()", "GETDATE()")]
    [InlineData("UserEmail = SYSTEM_USER", "SYSTEM_USER")]
    [InlineData("FormInserted < CURRENT_TIMESTAMP", "CURRENT_TIMESTAMP")]
    [InlineData("UserEmail = DEFAULT", "DEFAULT")]
    [InlineData("Age + 1 > 2", "'+'")]
    [InlineData("Age - 1 > 2", "Age - 1")]
    [InlineData("UserEmail IN (SELECT 1)", "'SELECT'")]
    [InlineData("EXISTS (SELECT 1)", "'EXISTS'")]
    [InlineData("UserEmail = 'x'; Age = 1", "';'")]
    [InlineData("UserEmail = @value", "'@value'")]
    [InlineData("UserEmail = @@VERSION", "'@@VERSION'")]
    [InlineData("dbo.UserEmail = 'x'", "'.'")]
    [InlineData("\"UserEmail\" = 'x'", "'\"UserEmail\"'")]
    [InlineData("Age * 2 > 2", "'*'")]
    [InlineData("UserEmail = 0x41", "'0x41'")]
    [InlineData("UserEmail = CASE WHEN Age > 1 THEN 'a' END", "'CASE'")]
    [InlineData("UserEmail = 'x' UNION SELECT 1", "'UNION'")]
    [InlineData("1 = (SELECT 1)", "'SELECT'")]
    public void AnythingButComparisonsOfColumnsAndValues_IsRefused(string condition, string whatIsNamed)
    {
        var result = Condition(condition);

        Assert.Null(result.Sql);
        Assert.NotNull(result.Error);
        Assert.Contains(whatIsNamed, result.Error);
        Assert.Contains("A condition can compare columns with values", result.Error);
    }

    [Theory]
    [InlineData("UserEmail = 'x' -- the rest")]
    [InlineData("UserEmail = 'x' /* note */")]
    [InlineData("/* note */ UserEmail = 'x'")]
    [InlineData("UserEmail = /* note */ 'x'")]
    public void Comments_AreRefused(string condition)
    {
        var result = Condition(condition);

        Assert.Equal("WhereClause can't contain comments.", result.Error);
    }

    [Fact]
    public void WhitespaceThatIsNotPlain_IsRefused()
    {
        Assert.Equal("WhereClause contains a character that isn't allowed.", Condition("UserEmail = 'x'").Error);
        Assert.Equal("WhereClause contains a character that isn't allowed.", Condition("UserEmail = 'x'").Error);
        Assert.Null(Condition("UserEmail\t=\r\n'x'").Error);
    }

    [Theory]
    [InlineData("UserEmail = 'unterminated")]
    [InlineData("UserEmail = ")]
    [InlineData("UserEmail = 'x' Age = 1")]
    [InlineData("(UserEmail = 'x'")]
    [InlineData("UserEmail = 'x')")]
    [InlineData("UserEmail LIKE")]
    [InlineData("AND UserEmail = 'x'")]
    public void ASentenceThatIsNotACondition_IsRefused(string condition)
    {
        var result = Condition(condition);

        Assert.Null(result.Sql);
        Assert.NotNull(result.Error);
        Assert.StartsWith("WhereClause isn't ", result.Error);
    }

    [Fact]
    public void AColumnTheFormLacks_IsNamedWithTheForm()
    {
        Assert.Equal("Column 'Nope' was not found on form 'BizForm.ContactUs'.", Condition("Nope = 1").Error);
    }

    [Fact]
    public void SeveralMissingColumns_AreNamedTogether_OnceEach()
    {
        var error = Condition("Nope = 1 AND nope = 2 AND Never IS NULL").Error;

        Assert.Equal("Columns 'Nope', 'Never' were not found on form 'BizForm.ContactUs'.", error);
    }

    [Fact]
    public void ALongCondition_IsRefused()
    {
        var condition = string.Join(" OR ", Enumerable.Repeat("Age = 1", 400));

        Assert.StartsWith("WhereClause can be at most 2000 characters.", Condition(condition).Error);
    }

    [Fact]
    public void AConditionWithManyParts_IsRefused()
    {
        var condition = string.Join(" OR ", Enumerable.Repeat("Age=1", 110));

        Assert.True(condition.Length < FormFilterChecker.MaxConditionLength);
        Assert.Equal("WhereClause is too long.", Condition(condition).Error);
    }

    [Fact]
    public void DeeplyNestedParentheses_AreRefused_BeforeTheyAreParsed()
    {
        var nested = new string('(', 9) + "Age = 1" + new string(')', 9);

        Assert.Equal("WhereClause nests parentheses more than 8 deep.", Condition(nested).Error);
        Assert.Null(Condition(new string('(', 8) + "Age = 1" + new string(')', 8)).Error);
    }

    [Fact]
    public void TooLongAList_InACondition_IsRefused()
    {
        var values = string.Join(", ", Enumerable.Range(1, 101));

        var result = Condition($"Age IN ({values})");

        Assert.NotNull(result.Error);
        Assert.Contains("can't contain", result.Error);
    }

    [Theory]
    [InlineData("FormInserted DESC", "[FormInserted] DESC")]
    [InlineData("FormInserted", "[FormInserted]")]
    [InlineData("userlastname, FormInserted desc", "[UserLastName], [FormInserted] DESC")]
    [InlineData("[UserEmail] ASC", "[UserEmail] ASC")]
    [InlineData("  Age   asc ,\r\n UserEmail ", "[Age] ASC, [UserEmail]")]
    public void AnOrdering_OfTheFormsColumns_Passes(string orderBy, string expected)
    {
        var result = OrderBy(orderBy);

        Assert.Null(result.Error);
        Assert.Equal(expected, result.Sql);
    }

    [Theory]
    [InlineData("FormInserted DESC DESC")]
    [InlineData("FormInserted,")]
    [InlineData(",FormInserted")]
    [InlineData("FormInserted,,Age")]
    [InlineData("DESC")]
    [InlineData("FormInserted Age")]
    [InlineData("LEN(UserEmail)")]
    [InlineData("1")]
    [InlineData("Age; DROP")]
    [InlineData("Age -- note")]
    [InlineData("Age COLLATE Latin1_General_BIN")]
    [InlineData("dbo.Age")]
    [InlineData("\"Age\"")]
    [InlineData("(SELECT 1)")]
    public void AnythingButColumnsAndDirections_IsRefused(string orderBy)
    {
        var result = OrderBy(orderBy);

        Assert.Null(result.Sql);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void AnOrderingColumnTheFormLacks_IsNamedWithTheForm()
    {
        Assert.Equal("Column 'Nope' was not found on form 'BizForm.ContactUs'.", OrderBy("Age, Nope").Error);
    }

    [Fact]
    public void TooManyOrderingColumns_AreRefused()
    {
        var orderBy = string.Join(", ", Enumerable.Repeat("Age", 11));

        Assert.StartsWith("OrderBy isn't a list of up to 10 columns.", OrderBy(orderBy).Error);
    }

    [Fact]
    public void ALongOrdering_IsRefused()
    {
        Assert.Equal("OrderBy can be at most 500 characters.", OrderBy(new string('a', 501)).Error);
    }
}
