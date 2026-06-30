# Original Request

## Request — 2026-06-22T18:25:00Z

You are teamwork_preview_worker.
Your working directory is d:\Project\ielts-teaching-assistant\.agents\worker_3\.
Your parent is sub_orch_implementation_gen2.
Your objective is to address all bugs, resource leaks, and gaps identified by the reviewers and challengers for Milestone 1, remove stub shadowing, add 5 new test cases, and ensure all tests pass.

CRITICAL MODEL CONSTRAINT:
You must run using the Gemini 3.5 Flash model. Ensure that any subagents you spawn or commands you execute use Gemini 3.5 Flash.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Tasks:
1. **Remove Test Stub Shadowing (CS0436)**:
   - Delete all files inside `scratch/TestGrading/Stubs/` (`SpeakingEvaluationViewModel.cs`, `WritingEvaluationViewModel.cs`, `MarkdownHelper.cs`, `MatrixClasses.cs`, `RubricDescriptorExtensions.cs`) to ensure the test project compiles and runs tests directly against the production classes in the referenced `IeltsTeachingAssistant` assembly.
2. **Speaking Score Rounding Bug**:
   - In `src/IeltsTeachingAssistant/Models/SpeakingEvaluation.cs` (line 22), compute `OverallBand` using `Helpers.BandScoreCalculator.CalculateSpeakingOverall(FluencyCoherence, LexicalResource, GrammaticalRange, Pronunciation)` instead of native Banker's Rounding (`Math.Round`).
3. **PDF & Image Text Extraction**:
   - In `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs`, inside `LoadDocumentContentAsync`, if the file is not a `.txt` file, call `await _vertexAIService.ExtractTextFromPdfOrImageAsync(filePath)` to extract the text and assign it to `task.SubmissionText`, instead of setting a placeholder string.
4. **Command Exception Handling**:
   - In both `WritingEvaluationViewModel.cs` and `SpeakingEvaluationViewModel.cs`, inside MVVM command handlers (like `GradeWithAiCommand`, `SaveSessionCommand`), ensure exceptions are caught and set on `ErrorMessage` and `IsErrorVisible = true`, but **do not rethrow** them to prevent WinUI application crashes on UI clicks.
5. **Connection Pooling Locks (Database Leak)**:
   - In `scratch/TestGrading/Tests/TestHelper.cs`, in `CleanupTest` (line 57), call `Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();` immediately before checking and deleting the SQLite file to release locks and allow deletion to succeed.
6. **Implement 5 New Test Cases**:
   - Add the 5 test cases suggested by the challenger in `scratch/TestGrading/Tests/` (e.g. inside `Tier2BoundaryEdgeTests.cs` or in a new file, and wire them in `Program.cs`):
     - `Test_LoadDocumentContent_NonExistentFile`
     - `Test_LoadDocumentContent_PdfBypass`
     - `Test_RevertToAiScore_Writing`
     - `Test_RevertToAiScore_Speaking`
     - `Test_SpeakingOverallBand_IeltsRounding`
7. **Verify & Build**:
   - Run the Windows x64 build and verify it compiles without errors:
     `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
   - Run the test suite and ensure all 87 tests pass successfully:
     `dotnet run --project scratch/TestGrading/TestGrading.csproj`

Write a detailed handoff report to d:\Project\ielts-teaching-assistant\.agents\worker_3\handoff.md and notify me.

## 2026-06-23T01:26:01Z

You are a teamwork_preview_worker subagent.
Resume work at the workspace d:\Project\ielts-teaching-assistant\.
Your working directory is d:\Project\ielts-teaching-assistant\.agents\worker_3\.
Your parent conversation ID is the conversation ID that invoked you.
Read ORIGINAL_REQUEST.md, BRIEFING.md, and progress.md in your working directory to recover your state and find task instructions.

CRITICAL MODEL CONSTRAINT:
You must run using the Gemini 3.5 Flash model. Ensure that any subagents you spawn or commands you execute use Gemini 3.5 Flash.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Tasks for Milestone 1 (Refinements):
1. Remove Test Stub Shadowing (CS0436): Delete all files in scratch/TestGrading/Stubs/ to ensure tests run directly against production classes in IeltsTeachingAssistant.csproj.
2. Correct Speaking Evaluation Rounding: In src/IeltsTeachingAssistant/Models/SpeakingEvaluation.cs, update OverallBand to use helpers.BandScoreCalculator.CalculateSpeakingOverall(FluencyCoherence, LexicalResource, GrammaticalRange, Pronunciation).
3. PDF/Image text extraction in Writing VM: In src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs, inside LoadDocumentContentAsync, call IVertexAIService.ExtractTextFromPdfOrImageAsync(filePath) for non-txt files and assign it to task.SubmissionText.
4. WinUI UI Command Crash Mitigation: Catch exceptions inside VMs commands and set on ErrorMessage and IsErrorVisible = true, but do not rethrow them.
5. Prevent Test DB Lock Leaks: In scratch/TestGrading/Tests/TestHelper.cs, add Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools() to CleanupTest right before deleting the SQLite file.
6. Implement 5 New Verification Tests: Add the 5 test cases suggested by the challenger:
   - Test_LoadDocumentContent_NonExistentFile
   - Test_LoadDocumentContent_PdfBypass
   - Test_RevertToAiScore_Writing
   - Test_RevertToAiScore_Speaking
   - Test_SpeakingOverallBand_IeltsRounding
7. Verify target x64 builds cleanly and all 87/87 tests pass successfully.

When you are done, write a detailed handoff report to d:\Project\ielts-teaching-assistant\.agents\worker_3\handoff.md and notify me via a message.
