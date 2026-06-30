# Integration & E2E Testing Plan

This plan details the setup and execution of the E2E Testing Track for the IELTS Teaching Assistant project.

## Architecture
The test runner is a console application located in `scratch/TestGrading`.
We will:
1. Create stub implementations of the missing ViewModels `SpeakingEvaluationViewModel` and `WritingEvaluationViewModel` in `scratch/TestGrading/Stubs/` under the namespace `IeltsTeachingAssistant.ViewModels` so that the project compiles successfully.
2. Structure the test suite into a series of logical modules under `scratch/TestGrading/Tests/`:
   - `Tier1FeatureCoverageTests.cs` (35 test cases)
   - `Tier2BoundaryEdgeTests.cs` (35 test cases)
   - `Tier3CrossFeatureTests.cs` (7 test cases)
   - `Tier4RealWorldScenarioTests.cs` (5 test cases)
3. Set up a test registry or runner wrapper in `scratch/TestGrading/Program.cs` to execute all 82+ tests, print their status, track statistics, and return exit code 0 if they execute (some might fail as expected due to missing viewmodel business logic, but the runner itself should execute the test code and exit properly).

## Detailed Test Cases Mapping

### Feature Coverage (Tier 1 - 35 Tests)
- **Feature 1: Input Prompt & Work**
  - F1_T1_1: Create WritingTask, verify prompt and submission text set/get.
  - F1_T1_2: Verify WritingEvaluation initializes with empty Tasks.
  - F1_T1_3: Verify SpeakingPart properties (PartNumber, Prompt, Transcript) set/get.
  - F1_T1_4: Verify audio file path property on SpeakingPart set/get.
  - F1_T1_5: Verify original file path set/get on WritingTask.
- **Feature 2: Auto Grading Trigger**
  - F2_T1_1: Trigger AI grading on WritingTask via mock IVertexAIService and verify call.
  - F2_T1_2: Trigger AI grading on SpeakingPart via mock service and verify call.
  - F2_T1_3: Verify band score properties updated on VM after AI grading.
  - F2_T1_4: Verify IsLoading state sets during AI call.
  - F2_T1_5: Verify ErrorMessage cleared on new grading request.
- **Feature 3: Marking Grid Binding**
  - F3_T1_1: Verify MatrixDescriptors contains standard IELTS criteria.
  - F3_T1_2: Verify descriptor selection updates individual band score.
  - F3_T1_3: Verify overall band score updates automatically on individual score change.
  - F3_T1_4: Verify matched_descriptor_ids JSON parsing into descriptors.
  - F3_T1_5: Verify property changed events for overall band scores.
- **Feature 4: Teacher Review Override**
  - F4_T1_1: Override AI score with manual score on WritingTask and verify update.
  - F4_T1_2: Add teacher comment on WritingTask and verify update.
  - F4_T1_3: Add teacher comment on SpeakingPart and verify update.
  - F4_T1_4: Verify manual override of criteria updates overall score.
  - F4_T1_5: Verify clear override reverts to AI score.
- **Feature 5: Detailed Comment Board**
  - F5_T1_1: Verify AI criteria comments are populated for WritingTask.
  - F5_T1_2: Verify AI criteria comments are populated for SpeakingPart.
  - F5_T1_3: Verify strengths, weaknesses, and actionable practice fields.
  - F5_T1_4: Verify markdown helper parses comments correctly.
  - F5_T1_5: Verify ViewModel holds the parsed comment entities.
- **Feature 6: Database Persistence**
  - F6_T1_1: Save WritingEvaluation to DB and verify record is inserted.
  - F6_T1_2: Save SpeakingEvaluation to DB and verify record is inserted.
  - F6_T1_3: Load saved WritingEvaluation and verify scores.
  - F6_T1_4: Load saved SpeakingEvaluation and verify transcripts.
  - F6_T1_5: Save evaluation with new Student and verify relationship.
- **Feature 7: Workspace Reset**
  - F7_T1_1: Clear session on WritingEvaluationViewModel and verify reset.
  - F7_T1_2: Clear session on SpeakingEvaluationViewModel and verify reset.
  - F7_T1_3: Verify Student is null after reset.
  - F7_T1_4: Verify ErrorMessage is cleared after reset.
  - F7_T1_5: Verify Task collection is reinitialized after reset.

### Boundary & Edge Cases (Tier 2 - 35 Tests)
- **Feature 1: Input Prompt & Work**
  - F1_T2_1: Set prompt/submission to extremely long strings (10k+ chars).
  - F1_T2_2: Set prompt/submission to null or empty.
  - F1_T2_3: Set invalid task numbers (e.g., negative).
  - F1_T2_4: Set speaking part number to invalid values.
  - F1_T2_5: Set non-existent file path.
- **Feature 2: Auto Grading Trigger**
  - F2_T2_1: Handle IVertexAIService exception gracefully (sets error message).
  - F2_T2_2: Handle invalid JSON response from AI service.
  - F2_T2_3: Grade empty text/prompt (check validation).
  - F2_T2_4: Trigger parallel grading calls.
  - F2_T2_5: Trigger grading with null student/task.
- **Feature 3: Marking Grid Binding**
  - F3_T2_1: Set scores to extreme boundaries (0.0, 1.0, 9.0).
  - F3_T2_2: Set scores to invalid values (< 0 or > 9) and check clamping/validation.
  - F3_T2_3: Bind empty descriptor array.
  - F3_T2_4: Bind mismatched descriptor criteria/bands.
  - F3_T2_5: Check standard IELTS rounding rules (half-bands).
- **Feature 4: Teacher Review Override**
  - F4_T2_1: Override score with non-half-band value (e.g., 5.3) and check validation.
  - F4_T2_2: Pass rich text/HTML to teacher comments and verify.
  - F4_T2_3: Override to 0.0 or 9.5 and check limits.
  - F4_T2_4: Override before AI grading runs.
  - F4_T2_5: Save with empty comments after manual override.
- **Feature 5: Detailed Comment Board**
  - F5_T2_1: Handle malformed markdown comments in parser.
  - F5_T2_2: Handle missing/null criteria comments.
  - F5_T2_3: Parse extremely large comment strings.
  - F5_T2_4: Parse comment strings with special unicode/emojis.
  - F5_T2_5: Parse empty comment string.
- **Feature 6: Database Persistence**
  - F6_T2_1: Save with null StudentId (FK check).
  - F6_T2_2: Save with duplicate task numbers in WritingEvaluation.
  - F6_T2_3: Save with max string values.
  - F6_T2_4: Save empty evaluation (no tasks).
  - F6_T2_5: Update existing evaluation and verify db updates without duplicate records.
- **Feature 7: Workspace Reset**
  - F7_T2_1: Reset workspace during active AI grading.
  - F7_T2_2: Reset workspace after error state.
  - F7_T2_3: Reset workspace multiple times sequentially.
  - F7_T2_4: Reset workspace with disposed db context.
  - F7_T2_5: Check VM dirty flag warning logic before reset.

### Cross-Feature (Tier 3 - 7 Tests)
- T3_1: AI grade Task 1 + manual override Task 2 + db save + combined overall score verification.
- T3_2: Run AI grading, manually edit descriptors, verify VM comments/scores update, save, reload.
- T3_3: Select student, run AI grading, override score, reset workspace, verify DB unmodified.
- T3_4: Add student, select student, run AI grading for speaking parts, save, verify student performance updates.
- T3_5: Run AI grading, force network failure on next grading part, check first part remains graded and second shows error.
- T3_6: Change evaluation mode, run grading, check descriptor mapping.
- T3_7: Save evaluation, retrieve, edit scores, save again, verify update.

### Real-World Scenarios (Tier 4 - 5 Tests)
- T4_1: Full Writing Grading Session: Create student, load Tasks 1 & 2, perform mock AI grading, map justifications, override score, save to DB, verify overall band.
- T4_2: Full Speaking Grading Session: Create student, load Speaking Parts, upload audio, transcribe, perform mock AI grading, select rubric cells manually, save to DB, verify.
- T4_3: Multiple Sequential Writing Tasks: Run grading for Student A, save. Reset. Run grading for Student B, save. Verify separate records.
- T4_4: Telemetry-Based Speaking Evaluation: Verify flow of speech rate estimation, pause count telemetry, mock grading, and mapping.
- T4_5: Conflict Resolution & Override Save: Load completed evaluation, mock teacher conflict (severe override), check validation messages, resolve, save, verify.

## Verification
- We will delegate to a worker subagent to set up `scratch/TestGrading/Stubs/` and the tests under `scratch/TestGrading/Tests/`.
- The worker will compile the project and execute it via `dotnet run --project scratch/TestGrading/TestGrading.csproj`.
- Once compilation and run pass, we will generate the `TEST_READY.md` file.
