# Handoff Report: Milestone 1 Adversarial Review

## 1. Observation
We examined the codebase in `src/IeltsTeachingAssistant/` and the E2E/integration test suite in `scratch/TestGrading/`.
We compiled the solution (targeting Windows x64) and executed the integration tests.

### Build and Test Execution
- **Command Run**: `dotnet build IeltsTeachingAssistant.sln`
- **Result**: Build Succeeded with 11 warnings and 0 errors.
- **Command Run**: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
- **Result**: Compilation and execution succeeded. The test runner executed 82 test cases across 4 tiers.
  - **Stdout**:
    ```
    Test Suite Summary: Passed=82, Failed=0
    [SUCCESS] All integration tests passed successfully!
    ```

### Rounding Calculation Bug in SpeakingEvaluation
In `src/IeltsTeachingAssistant/Models/SpeakingEvaluation.cs` (lines 22-27), the overall band is computed as:
```csharp
public double OverallBand => Parts.Any(p => p.FluencyCoherence > 0) ? Math.Round(Parts.Where(p => p.FluencyCoherence > 0).Average(p => (p.FluencyCoherence + p.LexicalResource + p.GrammaticalRange + p.Pronunciation) / 4.0) * 2) / 2.0 : 0;
```
Meanwhile, in `src/IeltsTeachingAssistant/Helpers/BandScoreCalculator.cs` (lines 16-27), the official IELTS rounding is defined:
```csharp
public static double RoundToHalfBand(double score)
{
    double floor = Math.Floor(score);
    double fraction = score - floor;

    if (fraction >= 0.75)
        return floor + 1.0;
    if (fraction >= 0.25)
        return floor + 0.5;

    return floor;
}
```

### PDF & Image File Text Extraction Bypass
In `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (lines 436-456), the file loading logic is:
```csharp
public async Task LoadDocumentContentAsync(WritingTask task, string filePath)
{
    if (task == null || string.IsNullOrEmpty(filePath)) return;
    try
    {
        if (filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            task.SubmissionText = await System.IO.File.ReadAllTextAsync(filePath);
        }
        else
        {
            // Simple placeholder for other formats if unsupported or mock
            task.SubmissionText = $"[Loaded content from {System.IO.Path.GetFileName(filePath)}]";
        }
        task.OriginalFilePath = filePath;
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Failed to load document: {ex.Message}";
    }
}
```
However, `src/IeltsTeachingAssistant/Services/VertexAIService.cs` implements an actual PDF/image text extraction method:
```csharp
public async Task<string> ExtractTextFromPdfOrImageAsync(string filePath)
```
This service method is never called in `WritingEvaluationViewModel`.

### ViewModel Command Exception Handling (UI Crash Potential)
In both `WritingEvaluationViewModel.cs` and `SpeakingEvaluationViewModel.cs`, commands catch and rethrow exceptions. For example, in `WritingEvaluationViewModel.cs` (lines 252-256):
```csharp
catch (Exception ex)
{
    ErrorMessage = ex.Message;
    throw;
}
```
In WinUI 3, commands bound directly to UI controls (like `Button.Command`) that throw unhandled exceptions will crash the application when triggered via user click.

---

## 2. Logic Chain

1. **Bug 1: Banker's Rounding vs. IELTS Rounding**
   - *Observation*: `SpeakingEvaluation.OverallBand` uses C#'s native `Math.Round` (which defaults to banker's rounding, i.e., rounding to the nearest even number).
   - *Reasoning*: If a student receives scores with an average of `6.25`, the calculation is `Math.Round(6.25 * 2) / 2.0 = Math.Round(12.5) / 2.0 = 12 / 2.0 = 6.0`. However, according to the official IELTS guidelines (implemented in `BandScoreCalculator.RoundToHalfBand`), any score ending in `.25` must round up to the next half band (i.e., `6.5`). This introduces a systematic scoring discrepancy.
   - *Conclusion*: There is a critical calculation bug in `SpeakingEvaluation.OverallBand`.

2. **Bug 2: Bypassed PDF Text Extraction**
   - *Observation*: `WritingEvaluationViewModel.LoadDocumentContentAsync` stubs out non-TXT files (like `.pdf` or `.png`) with a dummy string `"[Loaded content from filename.pdf]"`.
   - *Reasoning*: The implementation fails to invoke the `IVertexAIService.ExtractTextFromPdfOrImageAsync` service. This means teachers attempting to load student essays in PDF or image formats will have their tasks populated with a literal placeholder message rather than the essay's contents, and subsequent grading will grade the placeholder text.
   - *Conclusion*: Document file extraction is broken and bypassed in the ViewModel layer.

3. **Bug 3: Uncaught Command Exceptions**
   - *Observation*: The asynchronous event commands (e.g. `GradeWithAiAsync`, `SaveSessionAsync`) rethrow caught exceptions.
   - *Reasoning*: Rethrown exceptions on async commands execute directly on the UI dispatcher, resulting in unhandled application crashes in production.
   - *Conclusion*: Exception propagation in MVVM commands poses a stability risk.

---

## 3. Caveats
- We did not perform dynamic manual testing of the UI pages in a running app instance, since our execution environment is headless.
- We assumed that `IVertexAIService.ExtractTextFromPdfOrImageAsync` functions correctly under GCP authentication. We did not verify its API connectivity due to networking restrictions, but mocked behavior is covered.
- No other areas (like `ReadingEvaluation` and `ListeningEvaluation`) were evaluated for rounding behavior since they do not aggregate multiple sub-parts in the same way.

---

## 4. Conclusion
While the existing 82 integration tests compile and pass cleanly, they fail to cover several critical business logic edges. Specifically, we have identified a **rounding calculation bug** in speaking evaluations, a **stubbed-out document content loader** bypassing Vertex AI text extraction, and a **command exception rethrow pattern** that poses application crash risks.

---

## 5. Verification Method

### Test Cases to Formulate & Add

To close these coverage gaps and verify the bugs, the following 5 test cases should be added to the test suite:

#### Test Case 1: Null or Non-existent File Loading (VM Level)
```csharp
public static async Task Test_LoadDocumentContent_NonExistentFile()
{
    var (db, ai, eval, audio, writingVM, speakingVM, dbName) = TestHelper.SetupTest();
    try
    {
        var task = writingVM.WritingTasks.First();
        await writingVM.LoadDocumentContentAsync(task, "nonexistent_essay.txt");
        Assert.IsNotNull(writingVM.ErrorMessage, "Error message should be set for non-existent file");
        Assert.IsTrue(writingVM.ErrorMessage.Contains("Failed to load document"), "Error message should indicate load failure");
    }
    finally
    {
        TestHelper.CleanupTest(db, dbName);
    }
}
```

#### Test Case 2: PDF File Content Loading (VM Level)
```csharp
public static async Task Test_LoadDocumentContent_PdfBypass()
{
    var (db, ai, eval, audio, writingVM, speakingVM, dbName) = TestHelper.SetupTest();
    try
    {
        var task = writingVM.WritingTasks.First();
        await writingVM.LoadDocumentContentAsync(task, "test_essay.pdf");
        // This fails currently because task.SubmissionText ends up containing the placeholder
        Assert.IsFalse(task.SubmissionText.Contains("[Loaded content from"), "VM should extract actual text instead of setting a placeholder.");
    }
    finally
    {
        TestHelper.CleanupTest(db, dbName);
    }
}
```

#### Test Case 3: Revert to AI Score (Writing VM)
```csharp
public static async Task Test_RevertToAiScore_Writing()
{
    var (db, ai, eval, audio, writingVM, speakingVM, dbName) = TestHelper.SetupTest();
    try
    {
        var task = writingVM.WritingTasks.First();
        task.Prompt = "Prompt";
        task.SubmissionText = "Submission";
        await writingVM.GradeWithAiAsync(task); // Grades to 7.0
        
        task.TaskAchievement = 9.0;
        writingVM.RevertToAiScore(task);
        
        Assert.AreEqual(7.0, task.TaskAchievement, "Score should revert to original AI score");
    }
    finally
    {
        TestHelper.CleanupTest(db, dbName);
    }
}
```

#### Test Case 4: Revert to AI Score (Speaking VM)
```csharp
public static async Task Test_RevertToAiScore_Speaking()
{
    var (db, ai, eval, audio, writingVM, speakingVM, dbName) = TestHelper.SetupTest();
    try
    {
        var part = speakingVM.SpeakingParts.First();
        part.Transcript = "Transcript";
        await speakingVM.GradeWithAiAsync(part); // Grades to 7.0
        
        part.FluencyCoherence = 9.0;
        speakingVM.RevertToAiScore(part);
        
        Assert.AreEqual(7.0, part.FluencyCoherence, "Score should revert to original AI score");
    }
    finally
    {
        TestHelper.CleanupTest(db, dbName);
    }
}
```

#### Test Case 5: Banker's Rounding Discrepancy in Speaking Evaluation
```csharp
public static void Test_SpeakingOverallBand_IeltsRounding()
{
    var eval = new SpeakingEvaluation();
    // Avg of FC=6, LR=6, GRA=6, PR=7 is 6.25. 
    // In banker's rounding, this evaluates to 6.0. 
    // Under IELTS rules, it must round to 6.5.
    eval.Parts.Add(new SpeakingPart
    {
        PartNumber = 1,
        FluencyCoherence = 6.0,
        LexicalResource = 6.0,
        GrammaticalRange = 6.0,
        Pronunciation = 7.0
    });
    
    // This will FAIL on current codebase (evaluates to 6.0)
    Assert.AreEqual(6.5, eval.OverallBand, "Speaking overall band must round 6.25 UP to 6.5 according to official IELTS rules");
}
```

### Verification Command
To verify after implementation:
1. Run `dotnet build IeltsTeachingAssistant.sln` to compile.
2. Run `dotnet run --project scratch/TestGrading/TestGrading.csproj` to execute tests.
