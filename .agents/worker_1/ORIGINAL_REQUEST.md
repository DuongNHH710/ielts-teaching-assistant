## 2026-06-22T11:14:04Z

Please perform the following tasks:
1. Copy the following cached XAML views:
   From: d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\obj\Debug\net8.0-windows10.0.22621.0\win-x64\Views\
   To: d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\Views\
   - EvaluationsPage.xaml
   - WritingEvaluationPage.xaml
   - SpeakingEvaluationPage.xaml
   - ListeningEvaluationPage.xaml
   - ReadingEvaluationPage.xaml

2. Create the code-behind files for these views in d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\Views\:
   - EvaluationsPage.xaml.cs: Needs to navigate the internal frames (ReadingFrame, ListeningFrame, WritingFrame, SpeakingFrame) to their corresponding page types, and implement the EvalPivot_SelectionChanged event handler.
   - WritingEvaluationPage.xaml.cs: Needs to resolve the ViewModel property of type WritingEvaluationViewModel from App.Services, bind it to DataContext, and provide the static Task1Visibility and FormatTaskTitle helpers.
   - SpeakingEvaluationPage.xaml.cs: Needs to resolve the ViewModel property of type SpeakingEvaluationViewModel from App.Services, bind it to DataContext, and provide the various static helpers expected by the bindings (Part2Visibility, GradingProgressVisibility, CanGrade, RecordingButtonVisibility, CanRecord, StopRecordingButtonVisibility, CanUpload, PlaybackControlsVisibility, CanPlay, StopPlaybackButtonVisibility, CanTranscribe, GetAudioStatusText).
   - ListeningEvaluationPage.xaml.cs: Needs to resolve IVertexAIService and IEvaluationService, populate StudentCombo, handle AnalyzeButton_Click, SaveButton_Click, and ClearButton_Click.
   - ReadingEvaluationPage.xaml.cs: Same as ListeningEvaluationPage.xaml.cs but for Reading evaluations.

3. Copy the ViewModel stubs:
   From: d:\Project\ielts-teaching-assistant\scratch\TestGrading\Stubs\
   To: d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\ViewModels\
   - WritingEvaluationViewModel.cs
   - SpeakingEvaluationViewModel.cs
   Also copy RubricDescriptorExtensions.cs from d:\Project\ielts-teaching-assistant\scratch\TestGrading\Stubs\ to d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\Models\RubricDescriptorExtensions.cs.

4. Register the ViewModels:
   Modify d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\App.xaml.cs's ConfigureServices method to register:
   - WritingEvaluationViewModel (transient)
   - SpeakingEvaluationViewModel (transient)

5. Update Navigation:
   - Modify d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\MainWindow.xaml.cs to route the "Marking" navigation to EvaluationsPage instead of MarkingPage.
   - Modify d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\Views\DashboardPage.xaml.cs to route click handlers (NewSpeakingEval_Click, NewWritingEval_Click, EvaluateNewTask_Click) to navigate to EvaluationsPage instead of MarkingPage.

6. Build and Run the E2E Test Suite:
   Propose running the command: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
   Verify that all basic ViewModel setup checks and Tier 1 features pass cleanly (and report on any other tiers that pass).

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Provide a handoff report documenting the file edits made, the commands executed, and the test results.
