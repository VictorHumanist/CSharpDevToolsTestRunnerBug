# C# Dev Tools test runner bug reproduction

Standalone .NET 10 solution with one xUnit fact and one theory containing two InlineData cases. All three tests pass. Its only direct package dependency is xunit.v3 4.0.3, which brings Microsoft.Testing.Platform 2.5.2 transitively.

## Reproduce in VS Code

1. Install C# Dev Tools (jakubkozera.csharp-dev-tools) **1.16.2**, the version investigated, and the .NET 10 SDK.
2. Open **this folder** as a separate VS Code workspace and select CSharpDevToolsTestRunnerBug.sln if prompted.
3. Run the terminal command below once to restore and build the tests.
4. Refresh C# Dev Tools Test Explorer, then choose its **Run C# Tests** profile.
5. Run all tests. The fact receives a result, while the theory cases remain without actual results or appear skipped.
6. Run either individual theory case. Test Explorer reports:

~~~text
No test results received - test may have been skipped or failed to execute
~~~

Expected: the fact and both theory cases display Passed.

## Verify in the terminal

Run these commands from this folder:

~~~powershell
dotnet test
~~~

Expected summary: **total 3, succeeded 3, failed 0, skipped 0**.

The extension's theory command also executes both cases successfully:

~~~powershell
dotnet test --project CSharpDevToolsTestRunnerBug.Tests/CSharpDevToolsTestRunnerBug.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName=CSharpDevToolsTestRunnerBug.Tests.TestRunnerTests.Theory_Passes" -c Debug --verbosity normal
~~~

Expected summary: **total 2, succeeded 2, failed 0, skipped 0**.

To inspect actual per-case output, append --output Detailed to that command.

## Cause

In MTP mode, C# Dev Tools 1.16.2 reads console output without requesting per-test results or a structured report. It converts the assembly summary into an __mtp_placeholder__ result. Fact reporting accepts this fallback; theory-case matching cannot associate it with either InlineData case.

Detailed output also exposes a parser mismatch: MTP emits lowercase, parenthesized results such as passed TestName (1ms), while the extension's passing-result parser expects Passed TestName [1ms]. A structured TRX report provides actual per-case results.
