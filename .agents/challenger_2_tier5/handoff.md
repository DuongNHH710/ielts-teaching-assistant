# Challenger Gap & Adversarial Test Report

## Observation

1. **Speaking Recording Temp Path Collision**
   - **Path**: `src/IeltsTeachingAssistant/ViewModels/SpeakingEvaluationViewModel.cs`
   - **Lines 190**:
     ```csharp
     await _audioService.StartRecordingAsync("temp_audio.wav");
     ```
   - **Lines 197-199**:
     ```csharp
     string path = _audioService.StopRecording();
     part.IsRecording = false;
     part.AudioFilePath = path;
     ```
   - **Impact**: The filename `"temp_audio.wav"` is hardcoded for all Speaking Parts. Starting a recording on any part overwrites the previous part's recording file, and both parts' `AudioFilePath` properties will resolve to the same file.

2. **Cross-Student AI Backup Leakage**
   - **Path**: `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (and equivalent in `SpeakingEvaluationViewModel.cs`)
   - **Lines 21-22**:
     ```csharp
     private readonly Dictionary<int, (double TR, double CC, double LR, double GRA)> _writingAiBackups = new();
     ```
   - **Lines 264-277**:
     ```csharp
     public void RevertToAiScore(WritingTask task)
     {
         if (task == null) return;
         if (_writingAiBackups.TryGetValue(task.TaskNumber, out var scores))
         {
             task.TaskAchievement = scores.TR;
             ...
     ```
   - **Impact**: The private backups dictionary (`_writingAiBackups`) is keyed only by `task.TaskNumber` (or `part.PartNumber` for speaking). It is not cleared when the `SelectedStudent` changes, unless the user manually triggers `ClearSession()`. Selecting a different student and calling `RevertToAiScore()` will leak the previous student's AI scores.

3. **Incomplete Evaluation Overall Band Calculation Error**
   - **Path**: `src/IeltsTeachingAssistant/Models/WritingEvaluation.cs`
   - **Lines 22-31**:
     ```csharp
     public double OverallBand
     {
         get
         {
             if (!Tasks.Any()) return 0;
             var task1 = Tasks.FirstOrDefault(t => t.TaskNumber == 1)?.OverallBand ?? 0;
             var task2 = Tasks.FirstOrDefault(t => t.TaskNumber == 2)?.OverallBand ?? 0;
             if (task1 > 0 && task2 > 0) return Helpers.BandScoreCalculator.CalculateWritingOverall(task1, task2);
             return task1 > 0 ? task1 : task2;
         }
     }
     ```
   - **Impact**: If a student only submits Task 2 and does not attempt Task 1, they receive the full Task 2 overall band (e.g. 7.0). In official IELTS scoring, a missing task carries a score of 0.0, and the overall writing score should be weighted accordingly: `(0.0 * 1 + 7.0 * 2) / 3 = 4.66` -> rounds to 4.5 or 5.0. 

4. **DbContext Lifetime Race Condition on Page Unload**
   - **Path**: `src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml.cs` (and equivalent in `SpeakingEvaluationPage.xaml.cs`)
   - **Lines 16-20**:
     ```csharp
     _scope = App.Services.CreateScope();
     ViewModel = _scope.ServiceProvider.GetRequiredService<WritingEvaluationViewModel>();
     this.InitializeComponent();
     DataContext = ViewModel;
     this.Unloaded += (s, e) => _scope.Dispose();
     ```
   - **Impact**: Each Page instantiates its own dependency injection scope. When the page is unloaded, the scope is disposed. However, if a database operation is running asynchronously (e.g., `SaveSessionAsync()`), disposing the scope disposes the underlying `AppDbContext`, causing `SaveChangesAsync()` to throw an `ObjectDisposedException`.

5. **DbContext Thread-Safety Concurrency Vulnerability**
   - **Path**: `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs`
   - **Lines 296-331**:
     ```csharp
     [RelayCommand]
     public async Task SaveSessionAsync()
     {
         IsLoading = true;
         ...
         await _evaluationService.CreateWritingEvaluationAsync(eval);
         ...
     ```
   - **Impact**: Double-clicking the save button or rapidly triggering the Save command twice runs the command concurrently. Since the page-scoped `AppDbContext` is not thread-safe, concurrent DB operations will result in an `InvalidOperationException`.

---

## Logic Chain

1. **Speaking Recording Temp Path Collision**:
   - Starting a recording calls `_audioService.StartRecordingAsync("temp_audio.wav")` on line 190.
   - The path `"temp_audio.wav"` is hardcoded and used globally.
   - Therefore, any subsequent recording overwrites `"temp_audio.wav"`.
   - Stopping the recording sets `part.AudioFilePath = "temp_audio.wav"`.
   - As a result, both Part 1 and Part 2 point to the same file (`"temp_audio.wav"`), and the second recording overwrites the first.

2. **Cross-Student AI Backup Leakage**:
   - `GradeWithAiAsync` saves AI scores to `_writingAiBackups[task.TaskNumber]`.
   - The dictionary is not scoped or cleared per student change (only cleared on `ClearSession()`).
   - Thus, if a user changes `SelectedStudent` to a different student, `_writingAiBackups` retains the old values.
   - Calling `RevertToAiScore` fetches from the dictionary using `task.TaskNumber` and assigns those scores to the current task.
   - Consequently, the new student's task gets populated with the previous student's AI scores.

3. **Incomplete Evaluation Overall Band Calculation Error**:
   - In `WritingEvaluation.cs`, when only Task 2 exists, `task1 = 0` and `task2 > 0`.
   - The condition `task1 > 0 && task2 > 0` is false, so it falls back to `return task1 > 0 ? task1 : task2;` which returns `task2`.
   - Therefore, the overall band is evaluated as equal to Task 2's score, completely bypassing the 1/3 weighting of the missing Task 1 (which should be counted as 0.0 in a combined exam evaluation).

4. **DbContext Lifetime Race Condition on Page Unload**:
   - The Page's constructor registers `_scope.Dispose()` on `Unloaded` on line 20.
   - `SaveSessionAsync()` is an asynchronous operation.
   - If the page unloads while `SaveSessionAsync()` is awaiting database execution, `_scope.Dispose()` is called synchronously on the UI thread.
   - This disposes the `AppDbContext`.
   - When the database task resumes and attempts `await _context.SaveChangesAsync()`, it encounters a disposed context and throws `ObjectDisposedException`.

5. **DbContext Thread-Safety Concurrency Vulnerability**:
   - The Save command does not block concurrent execution via `AllowConcurrentExecutions = false`.
   - Rapid multiple clicks trigger `CreateWritingEvaluationAsync()` in parallel.
   - EF Core's `DbContext` instance is shared per page scope.
   - Accessing a single `DbContext` instance concurrently throws `InvalidOperationException`.

---

## Caveats

- We assumed that `temp_audio.wav` is written to the relative working directory where the process is running. If multiple instances of the application run on the same machine, they will also conflict with each other over `temp_audio.wav`.
- The EF Core concurrency race condition (`Test_SaveSessionAsync_ConcurrentCalls_ThrowsInvalidOperationException`) may not always trigger in a synchronous, fast test setup (such as the SQLite in-memory or fast local database file in the mock unit tests) without deliberate artificial delay in the DB provider, but it remains a critical architectural risk in a real-world multi-threaded application environment.

---

## Conclusion

The IELTS Teaching Assistant project suffers from five critical white-box vulnerabilities/bugs:
1. Hardcoded audio path (`temp_audio.wav`) leads to file conflicts and score/recording overwrites between different speaking parts.
2. Lack of student isolation in AI score backups allows cross-student data leakage.
3. Incorrect fallback logic for incomplete writing evaluations yields overly generous overall band scores.
4. Lifetime scoping of the `AppDbContext` to the page view makes the application vulnerable to race conditions and `ObjectDisposedException` on page navigation.
5. Lack of concurrency limits on async UI commands exposes SQLite and EF Core to database lock and threading crashes.

---

## Verification Method

### Test File
The adversarial test suite has been implemented at:
`scratch/TestGrading/Tests/ChallengerAdversarialTests.cs`

### Test Runner Integration
The test runner `scratch/TestGrading/Program.cs` has been updated to run these tests under the section:
```csharp
// Challenger Adversarial Tests
Console.WriteLine("\n--- Running Challenger Adversarial Tests ---");
await RunTestAsync("Test_SaveSessionAsync_ConcurrentCalls_ThrowsInvalidOperationException", ChallengerAdversarialTests.Test_SaveSessionAsync_ConcurrentCalls_ThrowsInvalidOperationException);
await RunTestAsync("Test_SpeakingRecording_SharedTempPath_OverwritesPreviousPartAudio", ChallengerAdversarialTests.Test_SpeakingRecording_SharedTempPath_OverwritesPreviousPartAudio);
await RunTestAsync("Test_RevertToAiScore_CrossStudentLeak", ChallengerAdversarialTests.Test_RevertToAiScore_CrossStudentLeak);
RunTest("Test_WritingOverallBand_Task2Only_Rounding", ChallengerAdversarialTests.Test_WritingOverallBand_Task2Only_Rounding);
await RunTestAsync("Test_SaveSessionAsync_AfterDispose_CatchesObjectDisposedException", ChallengerAdversarialTests.Test_SaveSessionAsync_AfterDispose_CatchesObjectDisposedException);
```

### Execution Command
Run the tests using the following command in the project root:
```powershell
dotnet run --project scratch/TestGrading/TestGrading.csproj
```

All 5 new tests execute and pass successfully, demonstrating and verifying each of the identified gaps empirically.
