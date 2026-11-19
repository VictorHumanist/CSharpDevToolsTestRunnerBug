using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace CSharpDevToolsTestRunnerBug.Tests;

[TestCaseOrderer(typeof(UrlCaseOrderer))]
public class TestRunnerTests
{
    #region Test result reporting

    [Fact]
    public void Fact_Passes()
    {
        // Arrange:
        var value = 1;
        var expectedValue = 2;

        // Act:
        var actualValue = value + 1;

        // Assert:
        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("first")]
    [InlineData("second")]
    public void Theory_Passes(string value)
    {
        // Arrange:
        var expectedHasValue = true;

        // Act:
        var actualHasValue = !string.IsNullOrWhiteSpace(value);

        // Assert:
        Assert.Equal(expectedHasValue, actualHasValue);
    }

    #endregion

    #region Long URL argument reporting

    // These six rows reproduced the 1.16.3 truncation bug.
    // They match successfully in 1.17.2 and remain as regression controls.
    [Theory]
    [InlineData("https://sample.example.com{1}/tutorial-navigator.html")] // Case 1: 53 characters.
    [InlineData("https://sample.example.com{1}{2}/tutorial-navigator.html")] // Case 2: 56 characters.
    [InlineData("https://sample.example.com/tutorial{1}.html")] // Case 3: 43 characters.
    [InlineData("https://sample.example.com{q1}/tutorial-navigator.html")] // Case 4: 54 characters.
    [InlineData("https://sample.example.com{q1}{q2}/tutorial-navigator.html")] // Case 5: 58 characters.
    [InlineData("https://sample.example.com/tutorial{q1}.html")] // Case 6: 44 characters.
    public void Theory_LongUrlArguments_Passes(string to)
    {
        // Arrange:
        var expectedHasValue = true;

        // Act:
        var actualHasValue = !string.IsNullOrWhiteSpace(to);

        // Assert:
        Assert.Equal(expectedHasValue, actualHasValue);
    }

    #endregion

    #region Empty and whitespace arguments

    [Theory]
    [InlineData(null)] // Case 1.
    [InlineData("")] // Case 2.
    [InlineData("   ")] // Case 3: three spaces, a different value from case 2.
    public void Theory_EmptyAndWhitespaceArguments_Passes(string value)
    {
        // Arrange:
        var expectedIsBlank = true;

        // Act:
        var actualIsBlank = string.IsNullOrWhiteSpace(value);

        // Assert:
        Assert.Equal(expectedIsBlank, actualIsBlank);
    }

    #endregion

    #region URL schemes

    [Theory]
    [InlineData("//example.com/key.pem")] // Case 1.
    [InlineData("https://example.com/key.pem")] // Case 2: preserve the scheme.
    [InlineData("/key.pem?redirect=https://example.com")] // Case 3: control.
    public void Theory_UrlSchemeArguments_Passes(string value)
    {
        // Arrange:
        var expectedHasValue = true;

        // Act:
        var actualHasValue = !string.IsNullOrWhiteSpace(value);

        // Assert:
        Assert.Equal(expectedHasValue, actualHasValue);
    }

    #endregion

    #region C# string literal representations

    [Theory]
    [InlineData("/path")] // Case 1.
    [InlineData(@"/path\?")] // Case 2: verbatim source syntax is not part of the value.
    public void Theory_VerbatimPathArguments_Passes(string from)
    {
        // Arrange:
        var expectedHasValue = true;

        // Act:
        var actualHasValue = !string.IsNullOrWhiteSpace(from);

        // Assert:
        Assert.Equal(expectedHasValue, actualHasValue);
    }

    [Theory]
    [InlineData("/(alpha|beta)/old", "/(alpha|beta)/new")] // Case 1.
    [InlineData(@"/\(ignored\)/(alpha|beta)/old", "/(alpha|beta)/new")] // Case 2.
    public void Theory_VerbatimRegexArguments_Passes(string from, string expectedFrom)
    {
        // Arrange:
        var expectedHasValidSuffixes = true;

        // Act:
        var actualHasValidSuffixes = from.EndsWith("/old") && expectedFrom.EndsWith("/new");

        // Assert:
        Assert.Equal(expectedHasValidSuffixes, actualHasValidSuffixes);
    }

    #endregion

    #region Colliding JSON display names

    // Cases 3, 4, and 5 produce the same truncated runtime display name.
    // Case 2 also exposes whitespace removal in the truncation matcher.
    [Theory]
    [InlineData("""{"type":"example.object.created","data":{"path":123,"user":{"id":"user"}}}""")] // Case 1.
    [InlineData("""{"type":"example.object.created","data":{"path":" ","user":{"id":"user"}}}""")] // Case 2.
    [InlineData("""{"type":"example.object.created","data":{"path":"/path","user":{"id":123}}}""")] // Case 3.
    [InlineData("""{"type":"example.object.created","data":{"path":"/path","user":{"id":" "}}}""")] // Case 4.
    [InlineData("""{"type":"example.object.created","data":{"path":"/path"}}""")] // Case 5.
    public void Theory_JsonArgumentsWithSharedPrefix_Passes(string payload)
    {
        // Arrange:
        var expectedType = "example.object.created";

        // Act:
        using var document = JsonDocument.Parse(payload);
        var actualType = document.RootElement.GetProperty("type").GetString();

        // Assert:
        Assert.Equal(expectedType, actualType);
    }

    #endregion
}

// Preserves the earlier URL regression's result arrival order.
// All other cases retain their discovery order; every assertion passes.
public class UrlCaseOrderer : ITestCaseOrderer
{
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        return testCases.OrderBy(testCase => GetOrder(testCase)).ToArray();
    }

    private static int GetOrder(ITestCase testCase)
    {
        if (testCase is XunitTestCase xunitTestCase &&
            xunitTestCase.TestMethodArguments is [string to])
        {
            return to switch
            {
                "https://sample.example.com{1}{2}/tutorial-navigator.html" => 0, // Case 2.
                "https://sample.example.com/tutorial{q1}.html" => 1, // Case 6.
                "https://sample.example.com{q1}/tutorial-navigator.html" => 2, // Case 4.
                "https://sample.example.com{1}/tutorial-navigator.html" => 3, // Case 1.
                "https://sample.example.com/tutorial{1}.html" => 4, // Case 3.
                "https://sample.example.com{q1}{q2}/tutorial-navigator.html" => 5, // Case 5.
                _ => 6,
            };
        }

        return 6;
    }
}
