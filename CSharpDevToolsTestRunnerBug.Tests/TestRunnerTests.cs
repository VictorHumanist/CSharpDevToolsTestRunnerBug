using Xunit;

namespace CSharpDevToolsTestRunnerBug.Tests;

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
}
