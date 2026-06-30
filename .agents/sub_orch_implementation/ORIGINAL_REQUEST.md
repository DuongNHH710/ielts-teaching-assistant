# Original User Request

## Initial Request — 2026-06-22T11:08:50+07:00

You are the Implementation Track Orchestrator for the IELTS Teaching Assistant project.
Your workspace is d:\Project\ielts-teaching-assistant.
Your working directory is d:\Project\ielts-teaching-assistant\.agents\sub_orch_implementation.
Your parent conversation ID is 44f49f85-6c4c-401d-b4ce-8c085d3437dc.
Your objective is to execute the Implementation Track of the AI-assisted grading workflow based on PROJECT.md, TEST_INFRA.md, and TEST_READY.md.
Please execute the following steps:
1. Phase 1: Pass 100% of the E2E test suite (82 test cases).
   - Decompose by test tier as sequential sub-milestones (Tier 1 -> Tier 2 -> Tier 3 -> Tier 4).
   - For each sub-milestone, delegate implementation of views, viewmodels, and DB logic to a worker subagent.
   - Run the test suite: `dotnet run --project scratch/TestGrading/TestGrading.csproj`. Verify all tests in the current tier and previous tiers pass cleanly.
   - You must write the Views (EvaluationsPage.xaml, WritingEvaluationPage.xaml, SpeakingEvaluationPage.xaml, ListeningEvaluationPage.xaml, ReadingEvaluationPage.xaml) and code-behinds under `src/IeltsTeachingAssistant/Views/`, using the XAML cached files in `obj/` folder as references.
   - You must write WritingEvaluationViewModel and SpeakingEvaluationViewModel under `src/IeltsTeachingAssistant/ViewModels/` (and register them in DI in `App.xaml.cs`).
   - You must implement DB persistence using Entity Framework Core (IEvaluationService/AppDbContext).
   - You must wire up sidebar and dashboard page click handlers to navigate to EvaluationsPage instead of MarkingPage.
2. Phase 2: Adversarial Coverage Hardening (Tier 5).
   - Spawn challengers to analyze the implementation code and existing tests for coverage gaps and edge cases.
   - Generate additional tests (Tier 5) and implement code fixes to close those gaps.
   - Gate Phase 2: complete when no remaining gaps are reported or iteration limit is reached.
Please create BRIEFING.md and progress.md in your working directory. Update progress.md as your heartbeat.
Ensure that all coordination files created are inside your working directory d:\Project\ielts-teaching-assistant\.agents\sub_orch_implementation.
Report your success or failure. You must delegate code changes and tests running to workers. DO NOT write code or run commands yourself.
Include the integrity warning in all workers' prompts.
