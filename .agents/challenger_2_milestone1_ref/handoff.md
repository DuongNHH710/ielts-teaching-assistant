# Handoff Report: Adversarial Review of Milestone 1 Refinements

## Observation
During independent verification and adversarial inspection of the codebase in `src/IeltsTeachingAssistant/` and the integration tests in `scratch/TestGrading/`, the following facts were observed:

### 1. Inconsistent and Incorrect Rounding Implementations
While `BandScoreCalculator.RoundToHalfBand` correctly implements official IELTS rounding (rounding up `.25` to `.5` and `.75` to the next whole band), multiple parts of the application bypass this helper and use the standard `.NET` Bankers rounding:
- **`src/IeltsTeachingAssistant/Models/Student.cs` (lines 29-33, 46)**:
  ```csharp
  public double AverageReadingBand => ReadingEvaluations.Any() ? Math.Round(ReadingEvaluations.Average(e => e.BandScore) * 2) / 2.0 : 0;
  public double AverageListeningBand => ListeningEvaluations.Any() ? Math.Round(ListeningEvaluations.Average(e => e.BandScore) * 2) / 2.0 : 0;
  public double AverageSpeakingBand => SpeakingEvaluations.Any() ? Math.Round(SpeakingEvaluations.Average(e => e.OverallBand) * 2) / 2.0 : 0;
  public double AverageWritingBand => WritingEvaluations.Any() ? Math.Round(WritingEvaluations.Average(e => e.OverallBand) * 2) / 2.0 : 0;
  ...
  return Math.Round(bands.Average() * 2) / 2.0;
  ```
- **`src/IeltsTeachingAssistant/Models/ClassEntity.cs` (lines 65, 68, 71, 74, 77)**:
  ```csharp
  public double AverageReadingBand => Students.Any() ? Math.Round(Students.Where(s => s.AverageReadingBand > 0).Select(s => s.AverageReadingBand).DefaultIfEmpty(0).Average() * 2) / 2.0 : 0;
  ...
  public double OverallBand => Students.Any() ? Math.Round(Students.Where(s => s.OverallBand > 0).Select(s => s.OverallBand).DefaultIfEmpty(0).Average() * 2) / 2.0 : 0;
  ```
- **`src/IeltsTeachingAssistant/Controls/InteractiveRubricGrid.xaml.cs` (line 65)**:
  ```csharp
  grid.OverallBand = System.Math.Round((grid.TaskAchievement + grid.CoherenceCohesion + grid.LexicalResource + grid.GrammaticalRange) / 4.0 * 2.0) / 2.0;
  ```
- **`src/IeltsTeachingAssistant/Controls/InteractiveSpeakingRubricGrid.xaml.cs` (line 62)**:
  ```csharp
  grid.OverallBand = System.Math.Round((grid.FluencyCoherence + grid.LexicalResource + grid.GrammaticalRange + grid.Pronunciation) / 4.0 * 2.0) / 2.0;
  ```

### 2. Broken `.docx` Text Extraction Path
- **`src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (lines 443-450)**:
  ```csharp
  if (filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
  {
      task.SubmissionText = await System.IO.File.ReadAllTextAsync(filePath);
  }
  else
  {
      task.SubmissionText = await _vertexAIService.ExtractTextFromPdfOrImageAsync(filePath);
  }
  ```
- **`src/IeltsTeachingAssistant/Services/VertexAIService.cs` (lines 611-618)**:
  ```csharp
  string extension = Path.GetExtension(filePath).ToLower();
  string mimeType = extension switch
  {
      ".pdf" => "application/pdf",
      ".png" => "image/png",
      ".jpg" => "image/jpeg",
      ".jpeg" => "image/jpeg",
      _ => throw new NotSupportedException($"File extension {extension} is not supported for extraction.")
  };
  ```
- **`src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml.cs` (line 51)**:
  ```csharp
  picker.FileTypeFilter.Add(".docx");
  ```
Selecting a `.docx` file in the file picker triggers a `NotSupportedException` in the extraction service instead of running the previously supported Word document parser.

### 3. Improper VM Command Exception Handling and Swallowing
- **`src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (lines 453-456)**:
  ```csharp
  catch (Exception ex)
  {
      ErrorMessage = $"Failed to load document: {ex.Message}";
  }
  ```
  Here, `IsErrorVisible` is not set to `true`, preventing the error `InfoBar` in XAML from popping up when text extraction/file loading fails.
- **`src/IeltsTeachingAssistant/ViewModels/StudentPerformanceViewModel.cs` (lines 406-409)**:
  ```csharp
  catch (Exception ex)
  {
      System.Diagnostics.Debug.WriteLine($"Error deleting evaluation: {ex}");
  }
  ```
  If database commands fail during evaluation deletion, it is completely swallowed and printed to the debug stream only, leaving the user with no visual indication of the failure.

### 4. Build and Test Verification
- Building the solution using `dotnet build -p:Platform=x64` succeeds cleanly (0 errors, 0 warnings).
- Building the test runner using `dotnet build scratch/TestGrading/TestGrading.csproj -p:Platform=x64` completes successfully with a single CS8602 warning and 0 errors.
- Running the test suite using `dotnet run --project scratch/TestGrading/TestGrading.csproj` reports:
  ```
  Test Suite Summary: Passed=87, Failed=0
  [SUCCESS] All integration tests passed successfully!
  ```

---

## Logic Chain
1. **Rounding Bug**:
   - Standard `Math.Round(val * 2) / 2.0` in .NET uses **Bankers Rounding (Round to Even)**.
   - For an average ending in `.25` or `.75` (e.g. `6.25`), `Math.Round(6.25 * 2) / 2.0 = Math.Round(12.5) / 2.0 = 12.0 / 2.0 = 6.0`.
   - The official IELTS rules dictate that `.25` rounds UP to `.5` (resulting in `6.5` overall band), and `.75` rounds UP to next whole band.
   - Therefore, properties like `Student.OverallBand` and `ClassEntity.OverallBand` calculate incorrect bands (e.g., `6.0` instead of `6.5` for a student with sub-scores `6.0, 6.0, 6.5, 6.5`).
   
2. **Text Extraction Bug**:
   - `LoadDocumentContentAsync` sends non-`.txt` files to `_vertexAIService.ExtractTextFromPdfOrImageAsync`.
   - `ExtractTextFromPdfOrImageAsync` has a hardcoded switch block supporting only `.pdf`, `.png`, `.jpg`, and `.jpeg`.
   - Any `.docx` file will fall into the default switch arm and throw a `NotSupportedException`.
   - Because the UI file picker explicitly allows `.docx`, users will experience crashes/errors when they pick a valid Word document.

3. **Exception Handling / UI Bug**:
   - XAML binding for the Error InfoBar uses `IsOpen="{x:Bind ViewModel.IsErrorVisible, Mode=OneWay}"`.
   - In `LoadDocumentContentAsync`, `IsErrorVisible` is never set to `true` when an exception is caught.
   - Therefore, if file loading or PDF/Image extraction fails, the UI will not display the error details to the user.

---

## Caveats
- No caveats. The identified bugs are verifiable directly via inspection of the source code.

---

## Conclusion
- The target x64 builds compile cleanly and all 87 tests in the integration suite pass successfully.
- However, major bugs exist in the current refinement code:
  1. **Student & Class Rounding**: Non-compliance with IELTS rounding rules on `.25` and `.75` boundaries due to standard Bankers Rounding fallback in `Student` and `ClassEntity` models, and custom rubric grid controls.
  2. **Broken `.docx` Support**: Incomplete text extraction routing causing `NotSupportedException` on Word document files.
  3. **Exception UI Blind Spot**: Missing `IsErrorVisible` activation in file loader exceptions, and swallowed database exceptions in the delete evaluation command.

---

## Verification Method
To independently verify the bugs:
1. **Verify Rounding Bug**:
   - Create a test class or database entries with a student having sub-scores `6.0`, `6.0`, `6.5`, `6.5` (average 6.25).
   - Assert `Student.OverallBand`. It will return `6.0` (Bankers Rounding) instead of the IELTS expected `6.5`.
2. **Verify `.docx` Bug**:
   - Call `LoadDocumentContentAsync(task, "essay.docx")` from a unit test or standard run. It will throw `NotSupportedException` in `VertexAIService`.
3. **Verify Build & Run**:
   - Build command: `dotnet build -p:Platform=x64`
   - Test command: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
