using System.Collections.Generic;
using System.Linq;
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

    // Case numbers follow InlineData order in Test Explorer.
    // In C# Dev Tools 1.16.3, Run All leaves cases 2 and 5 unchecked.
    // Individual runs fail to match cases 1, 2, 4, and 5; cases 3 and 6 pass.
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
}

// Replays the original result arrival order, making the incorrect row-order
// fallback deterministic. This does not skip tests or change display names.
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
