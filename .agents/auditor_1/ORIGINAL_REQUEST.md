## 2026-06-22T08:47:29Z
You are teamwork_preview_auditor.
Your working directory is d:\Project\ielts-teaching-assistant\.agents\auditor_1\.
Your parent conversation ID is c30702e0-862c-4320-bc10-45980deea088.

CRITICAL MODEL CONSTRAINT:
You must run using the Gemini 3.5 Flash model. Ensure that any subagents you spawn or commands you execute use Gemini 3.5 Flash.

Task:
Perform a Forensic Integrity Audit of the implementation of the AI-Assisted Grading Workflow.
1. Inspect the newly implemented Views, ViewModels, Helpers, and Services under:
   - `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs`
   - `src/IeltsTeachingAssistant/ViewModels/SpeakingEvaluationViewModel.cs`
   - `src/IeltsTeachingAssistant/ViewModels/MatrixClasses.cs`
   - `src/IeltsTeachingAssistant/Models/RubricDescriptorExtensions.cs`
   - `src/IeltsTeachingAssistant/Helpers/MarkdownHelper.cs`
   - `src/IeltsTeachingAssistant/Views/EvaluationsPage.xaml.cs`
   - `src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml.cs`
   - `src/IeltsTeachingAssistant/Views/SpeakingEvaluationPage.xaml.cs`
   - `src/IeltsTeachingAssistant/Views/ListeningEvaluationPage.xaml.cs`
   - `src/IeltsTeachingAssistant/Views/ReadingEvaluationPage.xaml.cs`
2. Verify that:
   - No test cases, expected outputs, or grading strings are hardcoded in the codebase to cheat the test suite.
   - All ViewModel bindings, automated grading requests, DB persistence logic (EF Core/SQLite), and UI navigation handlers are fully, authentically implemented.
   - Any mathematical rounding of bands matches IELTS official standards (no banker's rounding for bands).
3. Produce a structured forensic audit report detailing:
   - **Audit Verdict**: CLEAN or INTEGRITY VIOLATION / CHEATING DETECTED.
   - **Findings**: Detailed analysis of each module.
   - **Checklist**: Results of static analysis, database transaction logging, and validation checks.

Write your report to d:\Project\ielts-teaching-assistant\.agents\auditor_1\handoff.md and notify me.
