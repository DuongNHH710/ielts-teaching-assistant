# AI-Assisted Grading Workflow — Implementation Walkthrough

> **Build status**: SUCCESS (0 errors, 12 pre-existing warnings)
> **Test status**: 97/97 tests passing (integration + adversarial)
> **Target**: Windows x64, WinUI 3, .NET 8

---

## Overview

Teachers can now input a student's work, have it automatically graded by Vertex AI, review and adjust results in the marking grid, generate per-criterion AI comments, and save the full session to the SQLite database.

---

## End-to-End Workflow

1. Teacher enters Prompt + pastes/uploads Student Work
2. GradeWithAiAsync calls IVertexAIService
3. Band scores, AI comments, matched descriptor IDs auto-populated
4. LoadDescriptorsFromTask auto-selects rubric items in marking grid
5. Teacher manually adjusts any selections/scores
6. GoToNext shows per-criterion AI comments screen
7. SaveSessionAsync persists to SQLite via EF Core
8. ClearSession resets student fields, retains Prompt and skill

---

## Key Files Modified

- WritingEvaluationViewModel.cs: GradeWithAiAsync, LoadDescriptorsFromTask, SaveSessionAsync, ClearSession, concurrency hardening
- SpeakingEvaluationViewModel.cs: Same pattern for speaking + audio recording fixes
- WritingEvaluationPage.xaml/.cs: Input fields, file upload picker, loader overlay, unsaved-changes guard
- SpeakingEvaluationPage.xaml/.cs: Speaking-specific UI with audio controls

---

## Adversarial Hardening (9 Patches)

1. Unique audio recording paths (timestamp + GUID)
2. AI backup clearing on student change
3. Weighted overall band for partial tasks
4. DbContext scope safe disposal after IsLoading=false
5. SemaphoreSlim(1,1) preventing concurrent saves
6. IsGrading reset in finally block
7. IsPlaying reset on natural MediaEnded
8. Interlocked ref-count for concurrent loader tasks
9. Dynamic progress indicator bound to IsLoading

---

## Build Command

dotnet build src\IeltsTeachingAssistant\IeltsTeachingAssistant.csproj --configuration Release -p:Platform=x64

---

## Test Command

dotnet run --project scratch\TestGrading\TestGrading.csproj
# Expected: 97 passed, 0 failed
