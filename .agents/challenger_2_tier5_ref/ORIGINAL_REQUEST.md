# Request for challenger_2_tier5_ref

## Objective
Empirically verify the correctness, check boundary/corner cases, and run adversarial tests on the fixes for the 9 gaps/vulnerabilities.

## Gaps to Challenge:
1. **Speaking Recording Temp Path Collision**
2. **Cross-Student AI Score Backup Leakage**
3. **Incomplete Writing Evaluation overall band**
4. **DbContext Lifetime Race on Page Unload**
5. **DbContext Concurrency Risks**
6. **SpeakingPart.IsGrading State**
7. **SpeakingPart.IsPlaying State**
8. **Concurrent VM IsLoading**
9. **Global Loader Visual Feedback**

## Verification Instructions
- Compile the application using: `dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
- Run the full test suite: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
- Confirm all 97 tests pass.
- Review `scratch/TestGrading/Tests/ChallengerAdversarialTests.cs` to ensure the adversarial tests are valid, comprehensive, and check correct fixed behavior.
- Validate that the adversarial test cases cover boundary conditions, parallel execution concurrency, and edge states.
- You must run using Gemini 3.5 Flash.
- Write your challenge report to: `d:\Project\ielts-teaching-assistant\.agents\challenger_2_tier5_ref\handoff.md`.

## 2026-06-25T09:00:57Z
You are challenger_2_tier5_ref. Your task is to perform the empirical and adversarial verification of the fixes.
Read your instructions at: d:\Project\ielts-teaching-assistant\.agents\challenger_2_tier5_ref\ORIGINAL_REQUEST.md
Compile the application using:
`dotnet build src/IeltsTeachingAssistant/IeltsTeachingAssistant.csproj -p:Platform=x64`
and run tests:
`dotnet run --project scratch/TestGrading/TestGrading.csproj`
Verify that all 97 tests pass and check boundary/concurrency/race conditions.
You must run using Gemini 3.5 Flash.
Write your report to: d:\Project\ielts-teaching-assistant\.agents\challenger_2_tier5_ref\handoff.md
Send a message when you are done.
