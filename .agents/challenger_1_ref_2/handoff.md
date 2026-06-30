# Handoff Report

## 1. Observation
1. **Tests Execution**: Run commands:
   - Build: `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
   - Run Tests: `dotnet run --project scratch/TestGrading/TestGrading.csproj --no-build`
   - Output:
     ```
     Test Suite Summary: Passed=87, Failed=0
     ==========================================
     [SUCCESS] All integration tests passed successfully!
     ```
2. **Page Caching and Scope Disposal**:
   - Files: `src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml` (line 11), `SpeakingEvaluationPage.xaml`, `StudentPerformancePage.xaml`, `ListeningEvaluationPage.xaml`, `ReadingEvaluationPage.xaml`.
   - Observation: All these XAML files contain `NavigationCacheMode="Enabled"`.
   - In code-behinds (e.g. `WritingEvaluationPage.xaml.cs` lines 16-20), a scoped container is created and disposed:
     ```csharp
     _scope = App.Services.CreateScope();
     ViewModel = _scope.ServiceProvider.GetRequiredService<WritingEvaluationViewModel>();
     this.InitializeComponent();
     DataContext = ViewModel;
     this.Unloaded += (s, e) => _scope.Dispose();
     ```
3. **Docx Parser Silent Failure**:
   - File: `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (lines 443-471).
   - Code snippet:
     ```csharp
     var entry = archive.GetEntry("word/document.xml");
     if (entry != null)
     {
         using (var entryStream = entry.Open())
         {
             var doc = System.Xml.Linq.XDocument.Load(entryStream);
             ...
         }
     }
     ```
     If `entry` is null, the block is skipped and `task.OriginalFilePath = filePath` is set, returning with no error raised or displayed.
4. **Docx Paragraph Break Handling**:
   - File: `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (lines 455-464).
   - Code snippet:
     ```csharp
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
     ```
     Text elements are concatenated using `string.Concat` without considering other sibling tags like `<w:br/>` (line breaks) or `<w:tab/>`.
5. **InfoBar UI Bindings**:
   - Files: `src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml` (line 177) and `SpeakingEvaluationPage.xaml` (line 279).
   - Code snippet:
     ```xml
     <InfoBar Grid.Row="1" IsOpen="{x:Bind ViewModel.IsErrorVisible, Mode=OneWay}" 
              Severity="{x:Bind ViewModel.InfoBarSeverity, Mode=OneWay}" 
              Message="{x:Bind ViewModel.ErrorMessage, Mode=OneWay}" />
     ```
     In contrast, `StudentPerformancePage.xaml` (line 26) uses `Mode=TwoWay`.

---

## 2. Logic Chain
1. **Critical Connection/Disposal Crash**:
   - When a page is navigated away from in WinUI, it is removed from the visual tree, causing the `Unloaded` event to fire and disposing of `_scope` (Observation 2).
   - Since `NavigationCacheMode="Enabled"` is set on these pages, when the user navigates back to them, the frame reuses the cached page instance instead of calling the constructor.
   - Therefore, the page uses the existing `ViewModel` resolved from the previously disposed `_scope`.
   - When any database operation (saving, loading, deleting) is triggered on this reused ViewModel, the injected `AppDbContext` (or scoped services) will be in a disposed state, throwing `ObjectDisposedException` and crashing the application.
   - **Conclusion**: Page-scoped DI combined with page caching causes a fatal runtime crash upon second navigation.
2. **Missing docx content warning**:
   - If a corrupted or malformed `.docx` file lacks `word/document.xml`, `archive.GetEntry` returns null (Observation 3).
   - The method returns successfully without setting any `SubmissionText` or raising an error in `ErrorMessage` or `IsErrorVisible`.
   - **Conclusion**: Users will experience a silent failure where no text is loaded and no error feedback is shown.
3. **Format merging in docx parsing**:
   - Reading only `<w:t>` elements inside `<w:p>` and concatenating them with `string.Concat` (Observation 4) means `<w:br/>` line breaks or `<w:tab/>` elements are omitted.
   - **Conclusion**: A sentence like `"Hello<w:br/>World"` will merge into `"HelloWorld"`, skewing word counts and degrading readability.
4. **InfoBar stuck state**:
   - When `IsOpen` is bound with `Mode=OneWay` (Observation 5), closing the InfoBar in the UI does not write back a `false` value to the ViewModel's `IsErrorVisible` property.
   - Consequently, the property stays `true` inside the ViewModel. If a subsequent error occurs, setting `IsErrorVisible = true` does not raise the `PropertyChanged` event because the value does not transition.
   - **Conclusion**: Subsequent errors will not make the InfoBar open again after the user closes it once.

---

## 3. Caveats
- Testing was conducted in a headless/console integration test runner. Headless execution does not trigger the WinUI `Unloaded` or `Loaded` visual tree lifecycle events, meaning the DI connection crash does not manifest in the test suite itself but affects the actual desktop application runtime.
- No other pages besides the five modified pages were analyzed for scoped DI.

---

## 4. Conclusion
While all 87 integration tests pass cleanly and the rounding logic works as expected, worker_4's implementation contains several critical defects:
1. **Critical Defect**: A fatal `ObjectDisposedException` crash occurs when users navigate back to cached evaluation pages because page-scoped scopes are disposed on `Unloaded` while page caching keeps the page instances alive.
2. **UI/UX Defect**: OneWay binding on InfoBars on Writing and Speaking pages prevents errors from displaying again after the user manually closes the InfoBar once.
3. **Parsing Defect**: Silent failure on docx files lacking `word/document.xml`, and incorrect paragraph layout formatting where `<w:br/>` tags are ignored.

*Recommendation*: Disable page caching (`NavigationCacheMode="None"`) on pages with scoped DI lifetimes since page sessions are cleared upon entry anyway. Change InfoBar `IsOpen` bindings to `TwoWay` on Writing and Speaking pages. Throw an exception in `LoadDocumentContentAsync` when `entry == null` to trigger the error UI.

---

## 5. Verification Method
- **Run build**: `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
- **Run tests**: `dotnet run --project scratch/TestGrading/TestGrading.csproj --no-build`
- **Inspect code files**:
  - `src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml` (line 11 and 177) to inspect `NavigationCacheMode` and `InfoBar` binding mode.
  - `src/IeltsTeachingAssistant/Views/SpeakingEvaluationPage.xaml` (line 279) to check the InfoBar binding.
  - `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs` (lines 448-468) to verify text extraction logic and silent docx failures.
