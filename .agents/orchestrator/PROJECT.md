# Project: IELTS Teaching Assistant AI-Assisted Grading Workflow

## Architecture
- **Views (UI)**: Restored pages in `src/IeltsTeachingAssistant/Views/`:
  - `EvaluationsPage.xaml` & `.xaml.cs`: Tab container.
  - `WritingEvaluationPage.xaml` & `.xaml.cs`: Input prompts/text, displays rubric grid checklist, sliders, AI justifications, pedagogical suggestions.
  - `SpeakingEvaluationPage.xaml` & `.xaml.cs`: Audio recording/upload tools, transcript text, rubric grid, AI comments.
  - `ListeningEvaluationPage.xaml` & `.xaml.cs`: Simple transcript & student answer audit.
  - `ReadingEvaluationPage.xaml` & `.xaml.cs`: Simple reading passage & student answer audit.
- **ViewModels (Logic)**: Managed state in `src/IeltsTeachingAssistant/ViewModels/`:
  - `WritingEvaluationViewModel.cs`: Manages multiple WritingTasks (Task 1 & Task 2), handles AI grading call via `IVertexAIService`, parses and maps `matched_descriptor_ids` to UI model, handles DB save and session reset.
  - `SpeakingEvaluationViewModel.cs`: Manages SpeakingParts, handles audio playback/recording, audio transcription, AI grading call, rubric matching, DB save and session reset.
- **Models**:
  - `WritingTask` and `SpeakingPart` classes.
  - ViewModel matrix classes: `MatrixPoint`, `MatrixBand`, `MatrixCriterion` mapping IELTS public band descriptors to view bindings.
- **Services & DB**:
  - `IVertexAIService`: Triggers AI content generation.
  - `IEvaluationService`: Commits database evaluations.
  - `AppDbContext`: Entity Framework Core SQLite database context.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Test Infra Setup | Design and set up the integration test runner in `scratch/TestGrading` | None | PLANNED |
| 2 | E2E Test Suite | Implement Tier 1-4 tests verifying VMs, services, and db persistence | M1 | PLANNED |
| 3 | ViewModels & Views | Restoring XAML and writing code-behind files, implementing VMs and binding structures | M2 | PLANNED |
| 4 | DB Persistence & Nav | Wire UI navigation in sidebar, dashboards, and profile pages, and complete database commit logic | M3 | PLANNED |
| 5 | E2E Testing Verification | Pass 100% of integration test suite, fix any issues found | M4 | PLANNED |
| 6 | Adversarial & Auditing | Perform adversarial gap closing and Forensic Integrity Audit verification | M5 | PLANNED |

## Interface Contracts
### ViewModel ↔ View
- `WritingEvaluationViewModel` provides:
  - `Students`: ObservableCollection of Student
  - `SelectedStudent`: Student
  - `EvaluationModes`: Array of string
  - `SelectedEvaluationMode`: string
  - `WritingTasks`: ObservableCollection of WritingTask
  - `MatrixDescriptors`: List of MatrixCriterion
  - `GradeWithAiCommand`: AsyncRelayCommand<WritingTask>
  - `SaveSessionCommand`: AsyncRelayCommand
  - `ClearSessionCommand`: RelayCommand
  - `GoToNextCommand`: RelayCommand
- `SpeakingEvaluationViewModel` provides:
  - `Students`: ObservableCollection of Student
  - `SelectedStudent`: Student
  - `EvaluationModes`: Array of string
  - `SelectedEvaluationMode`: string
  - `SpeakingParts`: ObservableCollection of SpeakingPart
  - `MatrixDescriptors`: List of MatrixCriterion
  - `GradeWithAiCommand`: AsyncRelayCommand<SpeakingPart>
  - `StartRecordingCommand`: AsyncRelayCommand<SpeakingPart>
  - `StopRecordingCommand`: AsyncRelayCommand<SpeakingPart>
  - `UploadAudioCommand`: AsyncRelayCommand<SpeakingPart>
  - `PlayAudioCommand`: AsyncRelayCommand<SpeakingPart>
  - `StopAudioCommand`: AsyncRelayCommand<SpeakingPart>
  - `TranscribeAudioCommand`: AsyncRelayCommand<SpeakingPart>
  - `SaveSessionCommand`: AsyncRelayCommand
  - `ClearSessionCommand`: RelayCommand
  - `GoToNextCommand`: RelayCommand

## Code Layout
- Views: `src/IeltsTeachingAssistant/Views/`
- ViewModels: `src/IeltsTeachingAssistant/ViewModels/`
- Test Runner: `scratch/TestGrading/`
