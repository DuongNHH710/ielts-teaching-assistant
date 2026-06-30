## 2026-06-23T06:19:45Z
You are a teamwork_preview_worker subagent.
Resume work at the workspace d:\Project\ielts-teaching-assistant\.
Your working directory is d:\Project\ielts-teaching-assistant\.agents\worker_4\.
Your parent conversation ID is the conversation ID that invoked you.

CRITICAL MODEL CONSTRAINT:
You must run using the Gemini 3.5 Flash model. Ensure that any subagents you spawn or commands you execute use Gemini 3.5 Flash.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Tasks to implement in this iteration:
1. Fix Banker's Rounding in UI Controls:
   - In `src/IeltsTeachingAssistant/Controls/InteractiveRubricGrid.xaml.cs` (line 65): Replace standard `System.Math.Round(...)` with `Helpers.BandScoreCalculator.RoundToHalfBand((grid.TaskAchievement + grid.CoherenceCohesion + grid.LexicalResource + grid.GrammaticalRange) / 4.0)`.
   - In `src/IeltsTeachingAssistant/Controls/InteractiveSpeakingRubricGrid.xaml.cs` (line 62): Replace standard `System.Math.Round(...)` with `Helpers.BandScoreCalculator.RoundToHalfBand((grid.FluencyCoherence + grid.LexicalResource + grid.GrammaticalRange + grid.Pronunciation) / 4.0)`.

2. Fix Banker's Rounding in Student & Class Aggregate Models:
   - In `src/IeltsTeachingAssistant/Models/Student.cs`:
     - Line 29: Replace `Math.Round(ReadingEvaluations.Average(e => e.BandScore) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(ReadingEvaluations.Average(e => e.BandScore))` (where `ReadingEvaluations.Any()`).
     - Line 30: Replace `Math.Round(ListeningEvaluations.Average(e => e.BandScore) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(ListeningEvaluations.Average(e => e.BandScore))` (where `ListeningEvaluations.Any()`).
     - Line 32: Replace `Math.Round(SpeakingEvaluations.Average(e => e.OverallBand) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(SpeakingEvaluations.Average(e => e.OverallBand))` (where `SpeakingEvaluations.Any()`).
     - Line 33: Replace `Math.Round(WritingEvaluations.Average(e => e.OverallBand) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(WritingEvaluations.Average(e => e.OverallBand))` (where `WritingEvaluations.Any()`).
     - Line 46: Replace `Math.Round(bands.Average() * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(bands.Average())`.
   - In `src/IeltsTeachingAssistant/Models/ClassEntity.cs`:
     - Line 65: Replace `Math.Round(Students.Where(...) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(Students.Where(s => s.AverageReadingBand > 0).Select(s => s.AverageReadingBand).DefaultIfEmpty(0).Average())`.
     - Line 68: Replace `Math.Round(Students.Where(...) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(Students.Where(s => s.AverageListeningBand > 0).Select(s => s.AverageListeningBand).DefaultIfEmpty(0).Average())`.
     - Line 71: Replace `Math.Round(Students.Where(...) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(Students.Where(s => s.AverageSpeakingBand > 0).Select(s => s.AverageSpeakingBand).DefaultIfEmpty(0).Average())`.
     - Line 74: Replace `Math.Round(Students.Where(...) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(Students.Where(s => s.AverageWritingBand > 0).Select(s => s.AverageWritingBand).DefaultIfEmpty(0).Average())`.
     - Line 77: Replace `Math.Round(Students.Where(...) * 2) / 2.0` with `Helpers.BandScoreCalculator.RoundToHalfBand(Students.Where(s => s.OverallBand > 0).Select(s => s.OverallBand).DefaultIfEmpty(0).Average())`.

3. Fix Silent Document Load Failures:
   - In `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs`, inside `LoadDocumentContentAsync`, make sure that the `catch (Exception ex)` block sets:
     `ErrorMessage = $"Failed to load document: {ex.Message}";`
     `IsErrorVisible = true;`
     `InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;`

4. Fix Swallowed Deletion Exceptions and Add User Feedback:
   - In `src/IeltsTeachingAssistant/ViewModels/StudentPerformanceViewModel.cs`, add `[ObservableProperty] private string? _errorMessage;` and `[ObservableProperty] private bool _isErrorVisible;` properties.
   - Inside `DeleteEvaluationAsync`, in the `catch (Exception ex)` block, set `ErrorMessage = $"Error deleting evaluation: {ex.Message}";` and `IsErrorVisible = true;`. Reset them to `null` and `false` in `InitializeAsync`.
   - In `src/IeltsTeachingAssistant/Views/StudentPerformancePage.xaml`, add a visual `<InfoBar IsOpen="{x:Bind ViewModel.IsErrorVisible, Mode=TwoWay}" Severity="Error" Title="Error" Message="{x:Bind ViewModel.ErrorMessage, Mode=OneWay}" Margin="0,0,0,16" />` control at an appropriate place near the top of the content Grid (e.g. inside the Grid at Row 0 or Row 1) to notify the user.

5. Fix Scoped DbContext Connection / Memory Leak in Pages:
   - In Page code-behinds: `src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml.cs`, `SpeakingEvaluationPage.xaml.cs`, `StudentPerformancePage.xaml.cs`, `ListeningEvaluationPage.xaml.cs`, `ReadingEvaluationPage.xaml.cs`:
     - Avoid resolving viewmodels/contexts directly from the root `App.Services` without scope.
     - Store a `private readonly IServiceScope _scope;` field in the class.
     - In the constructor, create a scope and resolve views' dependencies from it, then register an handler for the Page's `Unloaded` event to dispose the scope.
     For example:
     ```csharp
     private readonly IServiceScope _scope;
     public WritingEvaluationPage()
     {
         _scope = App.Services.CreateScope();
         ViewModel = _scope.ServiceProvider.GetRequiredService<WritingEvaluationViewModel>();
         this.InitializeComponent();
         DataContext = ViewModel;
         this.Unloaded += (s, e) => _scope.Dispose();
     }
     ```
     Do the same for `SpeakingEvaluationPage`, `StudentPerformancePage`.
     For `ListeningEvaluationPage` and `ReadingEvaluationPage`, resolve their DbContext and other services from the scope and dispose the scope on `Unloaded`.

6. Correct docx support:
   - In `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs`, inside `LoadDocumentContentAsync`, write a lightweight pure C# zip-based XML text parser to extract text from `.docx` files:
     ```csharp
     if (filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
     {
         using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
         using (var archive = new System.IO.Compression.ZipArchive(fileStream))
         {
             var entry = archive.GetEntry("word/document.xml");
             if (entry != null)
             {
                 using (var entryStream = entry.Open())
                 {
                     var doc = System.Xml.Linq.XDocument.Load(entryStream);
                     var w = (System.Xml.Linq.XNamespace)"http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                     var paragraphs = doc.Descendants(w + "p");
                     var paragraphTexts = new List<string>();
                     foreach (var p in paragraphs)
                     {
                         var pText = string.Concat(p.Descendants(w + "t").Select(t => t.Value));
                         if (!string.IsNullOrEmpty(pText))
                         {
                             paragraphTexts.Add(pText);
                         }
                     }
                     task.SubmissionText = string.Join(Environment.NewLine, paragraphTexts);
                 }
             }
         }
         task.OriginalFilePath = filePath;
         return;
     }
     ```

7. Build and Verification:
   - Build the solution using `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
   - Run the integration test suite: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
   - Ensure all 87/87 tests pass successfully.
