# Handoff Report: Codebase Analysis for AI-Assisted Grading Integration

This report provides a detailed analysis of the IELTS Teaching Assistant codebase, focusing on models, services, UI components, and navigation structure, along with an actionable plan to integrate the AI-assisted grading workflow.

---

## 1. Observation

### Target Models

*   **`src/IeltsTeachingAssistant/Models/WritingTask.cs`**
    *   **Fields/Properties**:
        *   `Id` (int), `EvaluationId` (int), `Evaluation` (`WritingEvaluation?`)
        *   `TaskNumber` (int, e.g. 1 or 2), `TaskType` (string), `Prompt` (string?), `SubmissionText` (string?), `SubmissionRichText` (string?), `SelectedRubricDescriptors` (string?), `OriginalFilePath` (string?)
        *   `TaskAchievement` (double), `CoherenceCohesion` (double), `LexicalResource` (double), `GrammaticalRange` (double)
        *   `OverallBand` (double, computed): `Math.Round((TaskAchievement + CoherenceCohesion + LexicalResource + GrammaticalRange) / 4.0 * 2) / 2.0`
        *   AI / Teacher comments for each criterion:
            *   `TaskAchievementAIComment` / `TaskAchievementTeacherComment`
            *   `CoherenceCohesionAIComment` / `CoherenceCohesionTeacherComment`
            *   `LexicalResourceAIComment` / `LexicalResourceTeacherComment`
            *   `GrammaticalRangeAIComment` / `GrammaticalRangeTeacherComment`
        *   AI detailed analysis outputs:
            *   Justification: `TaskAchievementJustification`, `CoherenceCohesionJustification`, `LexicalResourceJustification`, `GrammaticalRangeJustification`
            *   Evidence quotes: `TaskAchievementEvidence`, `CoherenceCohesionEvidence`, `LexicalResourceEvidence`, `GrammaticalRangeEvidence`
            *   Limiting factors: `TaskAchievementLimitingFactors`, `CoherenceCohesionLimitingFactors`, `LexicalResourceLimitingFactors`, `GrammaticalRangeLimitingFactors`
        *   Pedagogical coaching (prose): `CoreStrengths` (string?), `PrimaryWeakness` (string?), `ActionablePractice` (string?)

*   **`src/IeltsTeachingAssistant/Models/SpeakingPart.cs`**
    *   **Fields/Properties**:
        *   `Id` (int), `EvaluationId` (int), `Evaluation` (`SpeakingEvaluation?`)
        *   `PartNumber` (int), `AudioFilePath` (string?), `Topic` (string?), `CueCard` (string?), `Transcript` (string?), `TranscriptRichText` (string?), `SelectedRubricDescriptors` (string?)
        *   `FluencyCoherence` (double), `LexicalResource` (double), `GrammaticalRange` (double), `Pronunciation` (double)
        *   `OverallBand` (double, computed): `Math.Round((FluencyCoherence + LexicalResource + GrammaticalRange + Pronunciation) / 4.0 * 2) / 2.0`
        *   AI / Teacher comments for each criterion:
            *   `FluencyCoherenceAIComment` / `FluencyCoherenceTeacherComment`
            *   `LexicalResourceAIComment` / `LexicalResourceTeacherComment`
            *   `GrammaticalRangeAIComment` / `GrammaticalRangeTeacherComment`
            *   `PronunciationAIComment` / `PronunciationTeacherComment`
        *   AI detailed analysis outputs:
            *   Justification: `FluencyCoherenceJustification`, `LexicalResourceJustification`, `GrammaticalRangeJustification`, `PronunciationJustification`
            *   Evidence quotes: `FluencyCoherenceEvidence`, `LexicalResourceEvidence`, `GrammaticalRangeEvidence`, `PronunciationEvidence`
            *   Limiting factors: `FluencyCoherenceLimitingFactors`, `LexicalResourceLimitingFactors`, `GrammaticalRangeLimitingFactors`, `PronunciationLimitingFactors`
        *   Pedagogical coaching (prose): `CoreStrengths` (string?), `PrimaryWeakness` (string?), `ActionablePractice` (string?)
        *   NotMapped state variables: `IsTranscribing`, `IsRecording`, `IsPlaying`, `IsGrading` (bool)

*   **`src/IeltsTeachingAssistant/Models/IeltsDescriptors.cs`**
    *   Defines models for descriptors: `DescriptorPoint` (with unique `Id` and `Text`), `BandDescriptor` (with `Band` and `Points`), and `CriterionDescriptor`.
    *   Provides static collections `SpeakingDescriptors` and `WritingDescriptors` with hardcoded public descriptors, e.g.:
        *   Speaking FC Band 9 point 1: `Id = "S_FC_9_1"`, `Text = "speaks fluently..."`
        *   Writing TR Band 7 point 2: `Id = "W_TR_7_2"`, `Text = "presents a clear position throughout..."`

---

### AI & Audio Services

*   **`src/IeltsTeachingAssistant/Services/IVertexAIService.cs` & `VertexAIService.cs`**
    *   **Method Signatures**:
        *   `Task<string> TranscribeAudioAsync(string audioFilePath)`: Transcribes recorded audio via Gemini.
        *   `Task<SpeakingGradingResult> GradeSpeakingAsync(string transcript, int partNumber, string? audioFilePath, int wpmRate = 0, int longPauseCount = 0)`: Grades speaking with explicit rubric guidelines and returns structured JSON parsed into `SpeakingGradingResult`.
        *   `Task<WritingGradingResult> GradeWritingAsync(string promptText, string text, int taskNumber, string testType, string taskType)`: Grades writing essays and returns structured JSON parsed into `WritingGradingResult`.
        *   `Task<string> ExtractTextFromPdfOrImageAsync(string filePath)`: Extracts essay text from uploaded PDFs/images.

*   **`src/IeltsTeachingAssistant/Services/AudioService.cs`**
    *   Handles NAudio recording (`StartRecordingAsync`, `StopRecording`) and playback (`PlayAudioAsync`, `StopAudio`).
    *   `int EstimateLongPauses(string filePath, double silenceThresholdDb = -40, double minimumSilenceDurationSeconds = 2.0)`: Calculates the number of long pauses (>2.0s) in speaking audio files to feed into Vertex AI grading telemetry.

---

### Marking Grid & UI Components

*   **Active Reference Components**:
    *   `src/IeltsTeachingAssistant/Views/MarkingPage.xaml`: Shell page selecting exam and skill. Swaps between subjective and objective views.
    *   `src/IeltsTeachingAssistant/Controls/SubjectiveGridControl.xaml` & `.cs`: Dynamically generates a WinUI 3 `Grid` in code-behind mapping standard rubrics.
    *   `src/IeltsTeachingAssistant/Controls/MarkingGridCell.xaml` & `.cs`: Checkbox list of descriptors for a specific band/criterion with click event tracking.
    *   `src/IeltsTeachingAssistant/Helpers/RichTextParser.cs`: Highlight keywords in descriptors using tags like `<positive>` or `<limitation>`.
*   **Cached Evaluation Views in `obj/Debug/net8.0-windows10.0.22621.0/win-x64/Views/`**:
    *   *Note: These files exist in build cache but are deleted/missing from the `src/Views` folder.*
    *   `EvaluationsPage.xaml`: Pivot control hosting frames for all four IELTS skills.
    *   `WritingEvaluationPage.xaml`: Configures student, mode (simultaneous or focus), inputs prompt & text, uploads documents, calls Vertex AI auto-grade, displays interactive checkbox matrix for Bands 9-5, lists justification/limiting factors, and displays coaching cards.
    *   `SpeakingEvaluationPage.xaml`: Audio control center (record, stop, upload, play, transcribe), transcript input, AI auto-grade button, interactive matrix for Bands 9-5, AI details, and coaching cards.

---

### Navigation and Structure

*   **`src/IeltsTeachingAssistant/MainWindow.xaml.cs`**
    *   Main navigation shell with custom sidebar tags: `Dashboard`, `Classes`, `Students`, `Marking`, `Settings`.
    *   `NavigateTo(Type pageType, object? parameter)`: Navigates `ContentFrame` to selected page and manages active sidebar button highlight.
*   **`src/IeltsTeachingAssistant/Views/StudentPerformancePage.xaml.cs`**
    *   Displays student metrics and historical timeline of evaluations.
    *   Receives `studentId` on navigation: `Frame.Navigate(typeof(StudentPerformancePage), studentId)`.
    *   Loads history mapping SpeakingPart and WritingTask into VM models `EvaluationHistoryItem` and `EvaluationSubItem` (defined in `StudentPerformanceViewModel.cs`).

---

## 2. Logic Chain

1.  **Backend Capability is Ready**:
    *   `VertexAIService` fully supports AI grading of writing (via text) and speaking (via transcript + audio telemetry).
    *   `AudioService` provides speech telemetry (long pauses, duration) and saves files to disk.
    *   `EvaluationService` has database persistence methods for writing and speaking evaluations.
    *   `WritingTask` and `SpeakingPart` database models already possess fields for storing detailed AI justifications, evidence quotes, and coaching advice.

2.  **UI is Currently Omitted**:
    *   The active `MarkingPage` only displays the static subjective/objective grids. It lacks fields for prompts, submission text, and save functionality.
    *   However, the cached `WritingEvaluationPage.xaml` and `SpeakingEvaluationPage.xaml` contain the complete layout for these student input fields, AI grading buttons, and interactive descriptor check grids.

3.  **UI Restoration & ViewModel Creation**:
    *   To implement the AI-assisted grading workflow, we must restore/write `WritingEvaluationPage` and `SpeakingEvaluationPage` in the `src/Views` folder, along with the wrapper `EvaluationsPage`.
    *   We need to implement `WritingEvaluationViewModel` and `SpeakingEvaluationViewModel`.
    *   **Interactive Rubric Synced with AI**:
        *   In the ViewModels, we initialize a grid structure (`MatrixDescriptors`) loaded from `IeltsDescriptors.cs`.
        *   When Vertex AI returns `matched_descriptor_ids`, we search `MatrixDescriptors` for matching IDs and set their `IsSelected` property to `true`.
        *   This checks the checkboxes in the XAML grid, visualizing the AI's analytical justification.
        *   The teacher can manually adjust sliders or check additional descriptors, and then click **Save Session** which saves the records to SQLite.

4.  **Navigation Integration**:
    *   Register `EvaluationsPage` in `MainWindow.xaml.cs`'s navigation tags (e.g. replace `MarkingPage` navigation with `EvaluationsPage`).
    *   Update quick actions on `DashboardPage.xaml.cs` (e.g. `NewSpeakingEval_Click`, `NewWritingEval_Click`) to navigate to `EvaluationsPage`.
    *   Add a "New Evaluation" button in `StudentPerformancePage.xaml` that navigates to `EvaluationsPage`, pre-selecting the active student.

---

## 3. Caveats

*   **ViewModel Code is Missing**: The original viewmodel files for evaluations (`WritingEvaluationViewModel` and `SpeakingEvaluationViewModel`) are not present in the codebase. They must be rewritten from scratch using MVVM CommunityToolkit patterns based on the bindings used in the recovered XAML files.
*   **Binding Mismatches**: In `WritingEvaluationPage.xaml`, the descriptors checkbox binds to `IsSelected`. In the existing `DescriptorViewModel.cs`, the property is named `IsChecked`. We recommend defining dedicated matrix models in the new ViewModels (e.g., `MatrixPoint` containing `IsSelected`) to avoid this mismatch.

---

## 4. Conclusion

The IELTS Teaching Assistant project is fully architected for AI-assisted grading. Integrating the workflow requires:
1.  Creating `WritingEvaluationViewModel` and `SpeakingEvaluationViewModel` implementing file uploading, NAudio recording, telemetry estimation, Vertex AI calls, and database saving.
2.  Restoring `EvaluationsPage`, `WritingEvaluationPage`, and `SpeakingEvaluationPage` views in `src/Views/` using the cached XAML files recovered from the `obj/` folder.
3.  Updating DI registrations in `App.xaml.cs`.
4.  Linking the Views in navigation (Sidebar, Dashboard, Student Performance).

---

## 5. Verification Method

1.  **Build Verification**:
    *   Run `dotnet build` from powershell to ensure the project compiles cleanly under standard x64 configurations.
2.  **Navigation Check**:
    *   Launch the app and check that clicking "Marking" or quick actions navigates to the newly restored evaluation screens.
3.  **Grading Test**:
    *   Input a writing task prompt and essay text. Click "Auto-Grade with AI". Ensure it successfully calls `IVertexAIService` (or gracefully fails with an InfoBar if GCP credentials are unconfigured) and populates the sliders, comments, and checkbox matrix.
