# Challenger Handoff Report — Milestone 1 Refinements

## 1. Observation

### Observation A: Correctness of Compilation and Tests
Running the test suite via the command:
`dotnet run --project scratch/TestGrading/TestGrading.csproj -r win-x64`
yields the following successful output:
```
==========================================
Test Suite Summary: Passed=87, Failed=0
==========================================

[SUCCESS] All integration tests passed successfully!
```

### Observation B: Rounding Math in Interactive Controls
In `src/IeltsTeachingAssistant/Controls/InteractiveRubricGrid.xaml.cs` (Line 65):
```csharp
grid.OverallBand = System.Math.Round((grid.TaskAchievement + grid.CoherenceCohesion + grid.LexicalResource + grid.GrammaticalRange) / 4.0 * 2.0) / 2.0;
```
Similarly, in `src/IeltsTeachingAssistant/Controls/InteractiveSpeakingRubricGrid.xaml.cs` (Line 62):
```csharp
grid.OverallBand = System.Math.Round((grid.FluencyCoherence + grid.LexicalResource + grid.GrammaticalRange + grid.Pronunciation) / 4.0 * 2.0) / 2.0;
```
Both of these rely on standard C# `System.Math.Round` (which uses Banker's Rounding) instead of the project's customized IELTS-compliant helper: `Helpers.BandScoreCalculator.RoundToHalfBand(double score)`.

### Observation C: Rounding Math in Student and Class Models
In `src/IeltsTeachingAssistant/Models/Student.cs` (Lines 29-33, 46):
```csharp
public double AverageReadingBand => ReadingEvaluations.Any() ? Math.Round(ReadingEvaluations.Average(e => e.BandScore) * 2) / 2.0 : 0;
public double AverageListeningBand => ListeningEvaluations.Any() ? Math.Round(ListeningEvaluations.Average(e => e.BandScore) * 2) / 2.0 : 0;
public double AverageSpeakingBand => SpeakingEvaluations.Any() ? Math.Round(SpeakingEvaluations.Average(e => e.OverallBand) * 2) / 2.0 : 0;
public double AverageWritingBand => WritingEvaluations.Any() ? Math.Round(WritingEvaluations.Average(e => e.OverallBand) * 2) / 2.0 : 0;
...
return Math.Round(bands.Average() * 2) / 2.0;
```
And in `src/IeltsTeachingAssistant/Models/ClassEntity.cs` (Lines 65-77), the same Banker's Rounding expression `Math.Round(... * 2) / 2.0` is used for all section averages and overall class band score averages.

### Observation D: Database Connection Resource Leak
In `src/IeltsTeachingAssistant/App.xaml.cs` (Lines 77-100), `AppDbContext` is registered with the default Scoped lifetime:
```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
```
While all ViewModels are registered with Transient lifetime:
```csharp
services.AddTransient<WritingEvaluationViewModel>();
services.AddTransient<SpeakingEvaluationViewModel>();
```
In the Page views (e.g. `WritingEvaluationPage.xaml.cs` Line 16, `SpeakingEvaluationPage.xaml.cs` Line 16), these transient ViewModels are resolved directly from the root ServiceProvider:
```csharp
ViewModel = App.Services.GetRequiredService<WritingEvaluationViewModel>();
```

### Observation E: Visual Feedback Bug on Document Load Exception
In `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (Lines 438-457):
```csharp
public async Task LoadDocumentContentAsync(WritingTask task, string filePath)
{
    if (task == null || string.IsNullOrEmpty(filePath)) return;
    try
    {
        ...
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Failed to load document: {ex.Message}";
    }
}
```
No assignment is made to toggle `IsErrorVisible` to `true` or set `InfoBarSeverity` to `Error`.

---

## 2. Logic Chain

### Logic Link A: Incorrect Rounding in UI & Models
1. **Official IELTS Rule**: Average scores must be rounded up to the nearest half or whole band on the `.25` and `.75` boundaries (e.g. `6.25` rounds to `6.5`, and `6.75` rounds to `7.0`).
2. **Observation B & C**: Custom controls and `Student` / `ClassEntity` calculate averages using `Math.Round(val * 2) / 2.0`.
3. **Execution Behavior**: In C#, `Math.Round(12.5)` uses midpoint rounding to nearest even number, yielding `12.0`. Thus, `Math.Round(6.25 * 2.0) / 2.0` results in `6.0` instead of `6.5`.
4. **Conclusion**: When interactive control inputs average to `6.25` or `5.25`, the control displays `6.0` or `5.0`. Likewise, when student averages across sections or class performance averages across students evaluate to `.25` or `.75` boundaries, the model computes and displays incorrect bands.

### Logic Link B: SQLite Database Connection Leak
1. **Observation D**: ViewModels and `AppDbContext` are resolved directly from `App.Services` (the root provider).
2. **DI Lifetime Specification**: In `Microsoft.Extensions.DependencyInjection`, resolving Transient or Scoped dependencies implementing `IDisposable` from the root ServiceProvider causes the provider to keep a reference to them in memory for lifetime tracking. They are never disposed until the root provider itself is disposed (which only occurs at application shutdown).
3. **Execution Behavior**: Navigating between pages/views multiple times instantiates new `AppDbContext` objects. These instances remain tracked in memory, leaking memory and leaving their SQLite database connection pools active.
4. **Conclusion**: Long-term application use leads to connection exhaustions, file locks, and sharing/access violations on `ielts_assistant.db`.

### Logic Link C: Silent Error Swallow
1. **Observation E**: `LoadDocumentContentAsync` catches exceptions and sets `ErrorMessage`, but does not set `IsErrorVisible = true`.
2. **Conclusion**: If a file read or vertex AI PDF text extraction fails, the view's InfoBar will not show, leaving the user with a silent load failure and no feedback.

---

## 3. Caveats
- No caveats. We did not perform automated stress testing under heavy load, but the logic issues identified are deterministic.

---

## 4. Conclusion
Adversarial testing of the refinement changes is complete. All 87 integration tests compile and run successfully. However, the following critical bugs/gaps remain:
1. **Banker's Rounding in Interactive UI**: `InteractiveRubricGrid` and `InteractiveSpeakingRubricGrid` round scores incorrectly on `.25` and `.75` boundaries.
2. **Banker's Rounding in Student & Class Performance**: Overall and sub-band aggregate scores in `Student.cs` and `ClassEntity.cs` violate IELTS rounding rules.
3. **AppDbContext Memory/Connection Leak**: ViewModels resolved from the root provider hold undisposed SQLite database connections.
4. **Silent Document Load Failures**: UI error feedback is never displayed in `LoadDocumentContentAsync` when exceptions are caught.

---

## 5. Verification Method

- **Compilation Command (Target x64)**:
  `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -r win-x64`
- **Execution Command for Integration Tests (87 Tests)**:
  `dotnet run --project scratch/TestGrading/TestGrading.csproj -r win-x64`
- **Verification of Rounding Math**:
  Verify the code lines at:
  - `src/IeltsTeachingAssistant/Controls/InteractiveRubricGrid.xaml.cs` (Line 65)
  - `src/IeltsTeachingAssistant/Controls/InteractiveSpeakingRubricGrid.xaml.cs` (Line 62)
  - `src/IeltsTeachingAssistant/Models/Student.cs` (Lines 29-33, 46)
  - `src/IeltsTeachingAssistant/Models/ClassEntity.cs` (Lines 65-77)
- **Verification of connection leak**:
  Verify dependency resolution from `App.Services` without scope block in views under `src/IeltsTeachingAssistant/Views/`.
- **Verification of silent error visibility**:
  Verify catch block in `WritingEvaluationViewModel.cs` (Line 453-456).
