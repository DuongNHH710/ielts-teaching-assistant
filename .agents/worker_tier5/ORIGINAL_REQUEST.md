## 2026-06-23T04:29:41Z
Objective:
Implement code fixes for the 9 gaps/vulnerabilities identified by the Challengers during Tier 5 white-box analysis, and update/integrate the adversarial tests to verify the corrected behavior.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Gaps to Address:
1. Speaking Recording Temp Path Collision: Prevent different Speaking Parts from writing to the same hardcoded "temp_audio.wav" file. Use a unique path per part.
2. Cross-Student AI Score Backup Leakage: Clear or isolate the AI score backup dictionaries in the ViewModels so that selecting a different student does not leak previous student scores.
3. Incomplete Writing Evaluation overall band: Ensure that when only one task is submitted, the missing task is weighted as 0.0 (overall writing band is Task 1 * 1/3 + Task 2 * 2/3), rather than returning the submitted task's score directly.
4. DbContext Lifetime Race on Page Unload: Ensure that database operations running asynchronously do not crash the app with ObjectDisposedException when page scope is disposed on Unload.
5. DbContext Concurrency Risks: Prevent concurrent executions of database-saving commands (SaveSessionCommand) using locking/semaphore or checking if an operation is already in progress.
6. SpeakingPart.IsGrading State: Set part.IsGrading to true during AI grading, and reset it to false in finally.
7. SpeakingPart.IsPlaying State: Reset part.IsPlaying to false in finally after audio playback finishes naturally.
8. Concurrent VM IsLoading: Implement a reference counter or safe check for concurrent async operations so that interleaved operations do not reset IsLoading to false prematurely.
9. Global Loader Visual Feedback: In WritingEvaluationPage.xaml and SpeakingEvaluationPage.xaml, bind a progress indicator or visual busy overlay to ViewModel.IsLoading so users receive visual feedback during slow database/AI operations.

Test Updates:
- Update the assertions in scratch/TestGrading/Tests/ChallengerAdversarialTests.cs to check for the CORRECT (fixed) behaviors, rather than asserting the bugs' presence.
- Ensure that:
  - Test_SpeakingRecording_SharedTempPath_OverwritesPreviousPartAudio asserts that the paths are NOT equal.
  - Test_RevertToAiScore_CrossStudentLeak asserts that Student B's task does NOT leak Student A's scores.
  - Test_WritingOverallBand_Task2Only_Rounding asserts that overall score is weighted correctly (e.g. 4.5 instead of 7.0).
  - Test_SpeakingPart_IsGrading_NeverSetToTrue asserts that wasIsGradingSetToTrue is TRUE.
  - Test_SpeakingPart_IsPlaying_StuckAtTrueAfterPlayback asserts that part.IsPlaying is FALSE.
  - Test_VM_IsLoading_InterleavedConcurrency_PrematureReset asserts that IsLoading stays TRUE until both operations finish.

Verification:
- Compile using: dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64
- Run all tests: dotnet run --project scratch/TestGrading/TestGrading.csproj
- Write your handoff report to d:\Project\ielts-teaching-assistant\.agents\worker_tier5\handoff.md. Once done, notify me via send_message.
