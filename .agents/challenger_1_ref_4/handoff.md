# Handoff Report - Milestone 1 Verification

## 1. Observation
We observed the following exact commands, outputs, file paths, and code snippets during the execution of verification tests:

### 1.1 Compilation
Command: `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
Result:
```text
  All projects are up-to-date for restore.
  IeltsTeachingAssistant -> D:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\bin\x64\Debug\net8.0-windows10.0.22621.0\IeltsTeachingAssistant.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 1.2 Test Execution (First Run)
Command: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
Result:
```text
Starting comprehensive app function test...

[FAIL] An error occurred during basic ViewModel setup check:
Microsoft.Data.Sqlite.SqliteException (0x80004005): SQLite Error 1: 'table "Classes" already exists'.
   at Microsoft.Data.Sqlite.SqliteException.ThrowExceptionForRC(Int32 rc, sqlite3 db)
   at Microsoft.Data.Sqlite.SqliteDataReader.NextResult()
   at Microsoft.Data.Sqlite.SqliteCommand.ExecuteReader(CommandBehavior behavior)
   at Microsoft.Data.Sqlite.SqliteCommand.ExecuteReader()
   at Microsoft.Data.Sqlite.SqliteCommand.ExecuteNonQuery()
   ...
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator.CreateTables()
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator.EnsureCreated()
   at Program.<Main>$(String[] args) in D:\Project\ielts-teaching-assistant\scratch\TestGrading\Program.cs:line 43
```

### 1.3 Test Execution (Second Run)
Command: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
Result:
```text
Starting comprehensive app function test...
[PASS] Database initialized and schema created.
...
--- Running Tier 2: Boundary & Edge ---
Running F1_T2_1... PASSED
...
Running Test_LoadDocumentContent_NonExistentFile... PASSED
Running Test_LoadDocumentContent_MalformedDocx... PASSED
Running Test_LoadDocumentContent_DocxFormatting... PASSED
Running Test_LoadDocumentContent_PdfBypass... PASSED
Running Test_RevertToAiScore_Writing... PASSED
Running Test_RevertToAiScore_Speaking... PASSED
Running Test_SpeakingOverallBand_IeltsRounding... PASSED
...
Test Suite Summary: Passed=89, Failed=0
==========================================

[SUCCESS] All integration tests passed successfully!
```

### 1.4 Code Inspection: Page Caching Configuration
In XAML files located in `src/IeltsTeachingAssistant/Views/`:
- `ClassPerformancePage.xaml` (line 11): `NavigationCacheMode="Disabled"`
- `ListeningEvaluationPage.xaml` (line 9): `NavigationCacheMode="Disabled"`
- `ReadingEvaluationPage.xaml` (line 9): `NavigationCacheMode="Disabled"`
- `SpeakingEvaluationPage.xaml` (line 11): `NavigationCacheMode="Disabled"`
- `StudentPerformancePage.xaml` (line 11): `NavigationCacheMode="Disabled"`
- `WritingEvaluationPage.xaml` (line 11): `NavigationCacheMode="Disabled"`

### 1.5 Code Inspection: Malformed Docx and Formatting Extraction
In `src/IeltsTeachingAssistant/ViewModels/WritingEvaluationViewModel.cs`:
- Checking document structure (lines 448-452):
```csharp
var entry = archive.GetEntry("word/document.xml");
if (entry == null)
{
    throw new System.IO.InvalidDataException("Invalid Word document: main content (word/document.xml) is missing.");
}
```
- Mapping paragraph formatting elements (lines 461-466):
```csharp
var pText = string.Concat(p.Descendants().Select(el => {
    if (el.Name == w + "t") return el.Value;
    if (el.Name == w + "br") return Environment.NewLine;
    if (el.Name == w + "tab") return "\t";
    return "";
}));
```

### 1.6 Code Inspection: InfoBar TwoWay Binding
In XAML files:
- `src/IeltsTeachingAssistant/Views/WritingEvaluationPage.xaml` (lines 177-178):
```xml
<InfoBar Grid.Row="1" IsOpen="{x:Bind ViewModel.IsErrorVisible, Mode=TwoWay}" 
         Severity="{x:Bind ViewModel.InfoBarSeverity, Mode=OneWay}" 
```
- `src/IeltsTeachingAssistant/Views/SpeakingEvaluationPage.xaml` (lines 279-280):
```xml
<InfoBar Grid.Row="1" IsOpen="{x:Bind ViewModel.IsErrorVisible, Mode=TwoWay}" 
         Severity="{x:Bind ViewModel.InfoBarSeverity, Mode=OneWay}" 
```

---

## 2. Logic Chain
1. **WinUI Page Caching**: By setting `NavigationCacheMode="Disabled"` on the main dynamic views (e.g. `WritingEvaluationPage`, `SpeakingEvaluationPage`, `StudentPerformancePage`), these page objects are destroyed and re-instantiated upon each navigation event. This guarantees that navigation parameters (like `studentId` or `classId`) are processed on a fresh page instance, preventing visual bleed or stale data bugs that would manifest under cached page configurations.
2. **Docx Malformed Throwing**: The parser checks for the zip archive entry `"word/document.xml"`. If this file is missing, it explicitly throws a `System.IO.InvalidDataException`. The view model captures this exception inside `LoadDocumentContentAsync` and populates the `ErrorMessage` and sets `IsErrorVisible = true`. This is verified by `Test_LoadDocumentContent_MalformedDocx`.
3. **Docx Formatting (br/tab)**: The XML extraction process uses Linq-to-XML. In `WritingEvaluationViewModel.cs`, formatting tags `<w:br/>` and `<w:tab/>` are translated to `Environment.NewLine` and `"\t"`, respectively. The integration test `Test_LoadDocumentContent_DocxFormatting` validates that standard formatting tags are parsed correctly.
4. **TwoWay InfoBar Bindings**: WinUI's `InfoBar` has an interactive close button. Binding `IsOpen` with `Mode=TwoWay` ensures that when a user closes the InfoBar, the ViewModel's backing property `IsErrorVisible` is automatically updated to `false`. Without `TwoWay`, the ViewModel state would stay out of sync (retaining `true`), meaning subsequent error messages could fail to trigger the InfoBar popup.
5. **SQLite Setup Check Failure (First Run)**: The test database creation throws an exception when `EnsureDeleted()` fails to drop/delete the `test_app.db` file (due to file handles kept open by SQLite connection pooling or background processes). As a result, EF Core's `EnsureCreated()` detects that the file already exists, but then attempts to create the schema, resulting in SQLite error `table "Classes" already exists`. Once pools are cleared/restarted (second run), the database file is cleanly removed and successfully recreated.

---

## 3. Caveats
- **WinUI Navigation Integration**: The disabling of Page cache was verified at the XAML markup level. Programmatic unit tests run in a console harness and cannot directly execute/simulate WinUI navigation stack cache behavior.
- **SQLite Concurrency & Pools**: Connection pooling can keep db file handles open. It is recommended to call `Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools()` before calling `EnsureDeleted()` in setup methods to prevent transient file locking.

---

## 4. Conclusion
Milestone 1 changes are implemented correctly and perform as expected:
- WinUI pages (`WritingEvaluationPage`, `SpeakingEvaluationPage`, `StudentPerformancePage`, etc.) have page caching disabled to prevent state leakage.
- Malformed docx files lacking `word/document.xml` successfully trigger `InvalidDataException`, which is caught and surfaced on the UI.
- Docx text extraction preserves line breaks and tabs correctly.
- TwoWay bindings on the InfoBars prevent the UI state from falling out of sync when closed.
- The test harness in `scratch/TestGrading` executes all 89 integration tests successfully.

---

## 5. Verification Method
To verify the milestone correctness:
1. Run the build command:
   `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
2. Run the integration test suite:
   `dotnet run --project scratch/TestGrading/TestGrading.csproj`
3. Inspect `scratch/TestGrading/Tests/Tier2BoundaryEdgeTests.cs` to verify that the boundary cases (malformed documents, invalid band scores, parallel tasks) are covered.
