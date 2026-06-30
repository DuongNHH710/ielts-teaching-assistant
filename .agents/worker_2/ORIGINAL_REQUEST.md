## 2026-06-22T08:27:01Z
You are a teamwork_preview_worker subagent.
Resume work at the workspace d:\Project\ielts-teaching-assistant\.
Your working directory is d:\Project\ielts-teaching-assistant\.agents\worker_2\.
Your parent conversation ID is the conversation ID that invoked you.
Read ORIGINAL_REQUEST.md, BRIEFING.md, and progress.md in your working directory to recover your state and find detailed task instructions.

CRITICAL MODEL CONSTRAINT:
You must run using the Gemini 3.5 Flash model. Ensure that any subagents you spawn or commands you execute use Gemini 3.5 Flash.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Tasks for Milestone 1:
1. Copy/move ViewModel and helper stub files from scratch/TestGrading/Stubs/ to their respective source locations (ViewModels, Models, Helpers).
2. Restore XAML view files from compiler cache obj/ folder to src/IeltsTeachingAssistant/Views/.
3. Implement code-behind files (.xaml.cs) for all 5 evaluation pages, matching constructor signatures, fields, and event handlers.
4. Register the new ViewModels in App.xaml.cs ConfigureServices.
5. Update sidebar navigation (MainWindow.xaml.cs) and dashboard (DashboardPage.xaml.cs) to navigate to EvaluationsPage.
6. Verify and compile the solution (make sure there are no XAML compile errors).
7. Run the test suite: `dotnet run --project scratch/TestGrading/TestGrading.csproj` and ensure all 35 Tier 1 integration tests pass.

When you are done, write a detailed handoff report to d:\Project\ielts-teaching-assistant\.agents\worker_2\handoff.md and notify me via a message.
