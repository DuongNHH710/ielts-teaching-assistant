# Handoff Report — Adversarial Challenger Report (Milestone 1)

## 1. Observations

### 1.1 Shadow VM Testing (Type Conflicts)
During the build of `scratch/TestGrading/TestGrading.csproj`, the compiler emitted 49 type conflict warnings (CS0436) for `SelectedRubricDescriptorsExtensions`, `SpeakingEvaluationViewModel`, `WritingEvaluationViewModel`, and `MarkdownHelper`.
```
D:\Project\ielts-teaching-assistant\scratch\TestGrading\Program.cs(30,23): warning CS0436: The type 'SpeakingEvaluationViewModel' in 'D:\Project\ielts-teaching-assistant\scratch\TestGrading\Stubs\SpeakingEvaluationViewModel.cs' conflicts with the imported type 'SpeakingEvaluationViewModel' in 'IeltsTeachingAssistant, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'. Using the type defined in 'D:\Project\ielts-teaching-assistant\scratch\TestGrading\Stubs\SpeakingEvaluationViewModel.cs'. [D:\Project\ielts-teaching-assistant\scratch\TestGrading\TestGrading.csproj]
D:\Project\ielts-teaching-assistant\scratch\TestGrading\Program.cs(31,23): warning CS0436: The type 'WritingEvaluationViewModel' in 'D:\Project\ielts-teaching-assistant\scratch\TestGrading\Stubs\WritingEvaluationViewModel.cs' conflicts with the imported type 'WritingEvaluationViewModel' in 'IeltsTeachingAssistant, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'. Using the type defined in 'D:\Project\ielts-teaching-assistant\scratch\TestGrading\Stubs\WritingEvaluationViewModel.cs'. [D:\Project\ielts-teaching-assistant\scratch\TestGrading\TestGrading.csproj]
```

### 1.2 Banker's Rounding in Speaking Evaluation
In `src/IeltsTeachingAssistant/Models/SpeakingEvaluation.cs`, the overall band score is calculated at line 22 as:
```csharp
public double OverallBand => Parts.Any(p => p.FluencyCoherence > 0) ? Math.Round(Parts.Where(p => p.FluencyCoherence > 0).Average(p => (p.FluencyCoherence + p.LexicalResource + p.GrammaticalRange + p.Pronunciation) / 4.0) * 2) / 2.0 : 0;
```
This is in contrast to `src/IeltsTeachingAssistant/Helpers/BandScoreCalculator.cs` (lines 16-27), which defines the correct IELTS rounding logic (`RoundToHalfBand`):
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

### 1.3 Silent Database Migration Failures
In `src/IeltsTeachingAssistant/App.xaml.cs` (lines 111-206), the automatic column-level schema migrator catches and swallows exceptions:
```csharp
try
{
    // Loops through tables, runs ALTER TABLE ADD COLUMN ...
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Error during automatic migration: {ex}");
}
```

### 1.4 Test Database Resource Leak
In `scratch/TestGrading/Tests/TestHelper.cs` (lines 57-68), the `CleanupTest` method disposes the context and swallows file deletion errors:
```csharp
public static void CleanupTest(AppDbContext db, string dbName)
{
    db.Dispose();
    try
    {
        if (System.IO.File.Exists(dbName))
        {
            System.IO.File.Delete(dbName);
        }
    }
    catch {}
}
```
As a consequence, dozens of `test_temp_*.db` files remain in the root directory.

### 1.5 Dead Conflict Resolution Logic
In `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (line 278) and `SpeakingEvaluationViewModel.cs` (line 357), `CheckForConflicts` is defined, but a codebase search shows it is never called by the view code-behind files.

---

## 2. Logic Chain

1. **Test Shadowing**:
   - The warnings under **1.1** show the compiler resolved VM references to classes inside the `Stubs/` folder rather than `IeltsTeachingAssistant.dll`.
   - The stubs have different line lengths (e.g., `WritingEvaluationViewModel.cs` stub is 381 lines, while the production version is 459 lines) and omit WinUI types like `Microsoft.UI.Xaml.Visibility`.
   - Therefore, the test suite executes successfully (82/82 passing) but does not actually cover the production ViewModels.

2. **Algorithmic Rounding Bug**:
   - Standard IELTS grading specifies that `.25` boundaries round up to `.5` (e.g., `6.25 -> 6.5`).
   - Standard .NET `Math.Round` uses Banker's Rounding (round to nearest even) by default. For example, `6.25 * 2 = 12.5`, which `Math.Round` rounds down to `12.0`, resulting in `6.0`.
   - Because `SpeakingEvaluation` calculates its overall band via inline `Math.Round` rather than calling `RoundToHalfBand`, it yields incorrect bands (e.g., `6.0` instead of `6.5`) on `.25` average boundaries.

3. **Database & Resource Integrity**:
   - Swallowing exceptions inside the automatic migrator (as seen in **1.3**) prevents schema failures from halting startup, resulting in silent failures that manifest as downstream crashes during runtime CRUD operations.
   - EF Core's SQLite connection pool locks database files. Disposing the context does not release the file locks immediately, meaning the `File.Delete` in `CleanupTest` fails silently. This causes disk clutter with transient test databases.

---

## 3. Caveats

- We assumed the test suite is intentionally designed to run console-based integration tests, which necessitated the VM stubs to bypass WinUI 3 AppSDK dependencies. 
- However, we did not investigate if a proper WinUI 3 unit testing framework (like `Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer`) could be configured to run tests against the real production assemblies without stubs.

---

## 4. Conclusion

While the integration test suite reports `100% pass (82/82)` and the project compiles cleanly for Windows x64:
- The tests run against **shadow ViewModel stubs**, leaving production ViewModels unverified.
- `SpeakingEvaluation.OverallBand` has a ** Banker's Rounding Bug** that rounds `.25` boundaries down.
- Database migration failures are **swallowed silently** and test cleanups **leak SQLite file locks**.
- The **Conflict Resolution** feature is dead code not wired into the UI.

---

## 5. Verification Method

### 5.1 Run Test Suite
Run the test runner via:
```powershell
dotnet run --project scratch/TestGrading/TestGrading.csproj
```
Observe that the 82 tests pass, but note the CS0436 warnings indicating stub overshadowing.

### 5.2 Rounding Bug Verification
Evaluate a Speaking Part session with average criteria score of `6.25` (e.g. Part 1 has FC=6.0, LR=6.5, GRA=6.0, PR=6.5). Confirm that the UI or DB entity records it as `6.0` rather than the correct IELTS score of `6.5`.
