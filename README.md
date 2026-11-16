# C# Dev Tools 1.16.3: long theory argument reproduction

Standalone .NET 10 / Microsoft.Testing.Platform solution with nine passing test cases: one fact, a two-case short-string theory, and a six-case URL theory. Its only direct package dependency is xunit.v3 4.0.3, which brings Microsoft.Testing.Platform 2.5.2 transitively.

## Reproduce in VS Code

1. Install the .NET 10 SDK and C# Dev Tools (jakubkozera.csharp-dev-tools) **1.16.3**.
2. Open this folder as a separate VS Code workspace and select CSharpDevToolsTestRunnerBug.sln if prompted.
3. Run **dotnet test** from this folder to restore and build the project.
4. Refresh C# Dev Tools Test Explorer and select its **Run C# Tests** profile (standard mode). MTP is enabled by global.json and UseMicrosoftTestingPlatformRunner in the project.
5. Run all tests. Under **Theory_LongUrlArguments_Passes**, cases **2 and 5** remain unchecked, even though the runner reports all nine tests passed.
6. Run each URL case individually. Cases **1, 2, 4, and 5** report the error below. Cases **3 and 6** pass.

~~~text
No test results received - test may have been skipped or failed to execute
~~~

The original Fact_Passes and short-string Theory_Passes remain controls and pass in both modes.

## URL cases

Case numbers refer to InlineData declaration order in TestRunnerTests.cs:

| Case | Argument | Length | Run All view in 1.16.3 | Individual run in 1.16.3 |
| --- | --- | --- | --- | --- |
| 1 | https://sample.example.com{1}/tutorial-navigator.html | 53 | Green check | Error |
| 2 | https://sample.example.com{1}{2}/tutorial-navigator.html | 56 | Unchecked | Error |
| 3 | https://sample.example.com/tutorial{1}.html | 43 | Green check | Pass |
| 4 | https://sample.example.com{q1}/tutorial-navigator.html | 54 | Green check | Error |
| 5 | https://sample.example.com{q1}{q2}/tutorial-navigator.html | 58 | Unchecked | Error |
| 6 | https://sample.example.com/tutorial{q1}.html | 44 | Green check | Pass |

UrlCaseOrderer fixes the execution order to **2, 6, 4, 1, 3, 5**, replaying the result arrival order that exposed the original bug. This makes the unchecked row numbers reproducible. It leaves the discovered arguments, display names, and passing assertions intact.

## Terminal verification

Run from this folder:

~~~powershell
dotnet test --output Detailed
~~~

Expected: **total 9, succeeded 9, failed 0, skipped 0**.

To run only the six URL cases:

~~~powershell
dotnet test --project CSharpDevToolsTestRunnerBug.Tests/CSharpDevToolsTestRunnerBug.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName=CSharpDevToolsTestRunnerBug.Tests.TestRunnerTests.Theory_LongUrlArguments_Passes" --output Detailed
~~~

Expected: **total 6, succeeded 6, failed 0, skipped 0**. The per-case output appears in the fixed execution order above.

## Remaining extension defect

xUnit truncates arguments longer than 50 characters in the runtime test display name. Discovery supplies the full InlineData strings. C# Dev Tools 1.16.3 parses the runtime results, but its individual theory-case matcher compares the complete discovered arguments against the truncated runtime text, so cases 1, 2, 4, and 5 do not match.

During Run All, the controller falls back to row order when matching fails. Some green checks are assigned to the wrong cases, and cases 2 and 5 receive no result. A subsequent reconciliation skips results already considered handled, preserving the incorrect mapping.

Expected after a fix: all nine tests have correct passing results in Run All, and all six URL cases also pass individually.
