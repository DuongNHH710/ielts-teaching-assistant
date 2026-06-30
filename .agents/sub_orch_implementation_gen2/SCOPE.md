# Scope: AI-Assisted Grading Workflow Implementation

## Architecture
- **Views**: EvaluationsPage, WritingEvaluationPage, SpeakingEvaluationPage, ListeningEvaluationPage, ReadingEvaluationPage under `src/IeltsTeachingAssistant/Views/`. Restored from pre-compiled cached XAML in `obj/`.
- **ViewModels**: WritingEvaluationViewModel, SpeakingEvaluationViewModel under `src/IeltsTeachingAssistant/ViewModels/`. Copied/adapted from `scratch/TestGrading/Stubs/`.
- **DI Registration**: In `App.xaml.cs`, register both viewmodels to the service collection.
- **Database Persistence**: SQLite with EF Core (`AppDbContext`), configured in `EvaluationService`.
- **Navigation Routing**: Sidebar click handlers in `MainWindow.xaml.cs` and dashboard card clicks in `DashboardPage.xaml.cs` routed to `EvaluationsPage` instead of `MarkingPage`.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Tier 1 Feature Coverage | Implement all required Views, VM stubs, DI registration, DB saving, and navigate click handlers. Run test suite to verify 35 Tier 1 feature coverage tests pass. | None | DONE |
| 2 | Tier 2 Boundary & Edge Cases | Review VM logic and Views for limits, error handling, null inputs. Run test suite to verify 35 Tier 2 tests pass. | Milestone 1 | DONE |
| 3 | Tier 3 Cross-Feature | Address interaction between multiple concurrent tasks, overrides, and saves. Run test suite to verify 7 Tier 3 tests pass. | Milestone 2 | DONE |
| 4 | Tier 4 Real-World Scenarios | Verify multi-step E2E grading sessions for both Writing & Speaking tracks. Run test suite to verify 5 Tier 4 tests pass. | Milestone 3 | DONE |
| 5 | Phase 2 Adversarial Hardening | Generate and pass Tier 5 tests to close any code coverage gaps. | Milestone 4 | IN_PROGRESS |
