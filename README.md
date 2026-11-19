# C# Dev Tools 1.17.2: remaining theory result bugs

Standalone .NET 10 / Microsoft.Testing.Platform reproduction with **24 passing test cases**. The only direct package dependency is xunit.v3 4.0.3, which brings Microsoft.Testing.Platform 2.5.2 transitively.

The original fact, short-string theory, and six long-URL cases are controls. Five additional theories reproduce the remaining case-identity and reconciliation problems.

## Reproduce in VS Code

1. Install the .NET 10 SDK and C# Dev Tools (jakubkozera.csharp-dev-tools) **1.17.2**.
2. Open this folder as a separate VS Code workspace and select CSharpDevToolsTestRunnerBug.sln if prompted.
3. Run **dotnet test --output Detailed** from this folder to restore and build. All **24** tests pass, with zero failed and zero skipped.
4. Refresh C# Dev Tools Test Explorer. Select **Run C# Tests** (standard mode) and the **Project > Namespace > Class** grouping. MTP is enabled by global.json and UseMicrosoftTestingPlatformRunner in the project.
5. Run all tests. The 1.17.2 code produces **22** extension console results and **17** green case rows, despite 24 actual passing executions.
6. Refresh Test Explorer again before testing individual rows, to restore the original source labels. Run the cases listed below; they report:

~~~text
No test results received - test may have been skipped or failed to execute
~~~

These counts and selections were verified by replaying the installed extension's unchanged parser, streaming-result callbacks, and controller methods against captured CLI output and an in-memory Project > Namespace > Class > Method > Case tree. The VS Code UI was not automated.

## Cases

Row numbers refer to InlineData declaration order within each theory:

| Method | Cases | Unchecked rows after Run All | Individual rows that fail to report a result |
| --- | --- | --- | --- |
| Fact_Passes | 1 | None | None |
| Theory_Passes | 2 | None | None |
| Theory_LongUrlArguments_Passes | 6 | None | None |
| Theory_EmptyAndWhitespaceArguments_Passes | 3 | 3 | 2, 3 |
| Theory_UrlSchemeArguments_Passes | 3 | 2 | 1, 2 |
| Theory_VerbatimPathArguments_Passes | 2 | 2 | 2 |
| Theory_VerbatimRegexArguments_Passes | 2 | 2 | 2 |
| Theory_JsonArgumentsWithSharedPrefix_Passes | 5 | 2, 4, 5 | 1, 3, 4, 5 |

Run All can assign a result to the wrong row and replace that row's label. This is why the individual-run check starts with a refresh.

UrlCaseOrderer preserves the earlier long-URL theory's execution order, **2, 6, 4, 1, 3, 5**. Those six cases now match successfully in 1.17.2 and remain as controls. The new theories use generic example.com URLs, paths, and JSON data.

## Terminal verification

Run from this folder:

~~~powershell
dotnet test --output Detailed
~~~

Verified summary: **total 24, succeeded 24, failed 0, skipped 0**.

For example, run only the empty/whitespace theory with:

~~~powershell
dotnet test --project CSharpDevToolsTestRunnerBug.Tests/CSharpDevToolsTestRunnerBug.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName=CSharpDevToolsTestRunnerBug.Tests.TestRunnerTests.Theory_EmptyAndWhitespaceArguments_Passes" --output Detailed
~~~

All three underlying cases pass. The extension's individual-case error comes from selecting the returned results.

## Remaining defects

1. **Display-name deduplication:** JSON cases 3, 4, and 5 have the same truncated runtime display name. The runner keeps only one result per fullName, reducing 24 executions to 22 result records.
2. **Destructive normalization:** empty strings and three spaces become identical. Protocol-relative and HTTPS URLs also become identical when the URL scheme is removed. The JSON truncation fallback can match more than one distinct payload.
3. **C# literal representation:** discovered verbatim literals keep their source @ prefix, while xUnit reports evaluated strings with escaped backslashes. Comparing source syntax with formatted values fails to match those cases.
4. **Recursive reconciliation:** applyResultsToChildren drops the handled-results set when descending into namespace/class nodes. Its positional fallback can then assign an unmatched result to an already green row, leaving the intended case unchecked.

The relevant matching and reconciliation methods remain unchanged from 1.16.4. Correct behavior is **24** results attached to the appropriate cases, with every case also passing individually. Result tracking should use stable case identities and preserve actual argument values.
