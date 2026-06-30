# E2E Test Infra: IELTS Teaching Assistant AI-Assisted Grading Workflow

## Test Philosophy
- Opaque-box, requirement-driven. Evaluates logic correctness of ViewModels, AI integration, and database operations.
- Methodology: Category-Partition + BVA + Pairwise + Workload Testing.

## Feature Inventory
| # | Feature | Source (requirement) | Tier 1 | Tier 2 | Tier 3 |
|---|---------|---------------------|:------:|:------:|:------:|
| 1 | Input Prompt & Work | ORIGINAL_REQUEST §R1 | 5 | 5 | ✓ |
| 2 | Auto Grading Trigger | ORIGINAL_REQUEST §R1 | 5 | 5 | ✓ |
| 3 | Marking Grid Binding | ORIGINAL_REQUEST §R1 | 5 | 5 | ✓ |
| 4 | Teacher Review Override | ORIGINAL_REQUEST §R2 | 5 | 5 | ✓ |
| 5 | Detailed Comment Board | ORIGINAL_REQUEST §R3 | 5 | 5 | ✓ |
| 6 | Database Persistence | ORIGINAL_REQUEST §R4 | 5 | 5 | ✓ |
| 7 | Workspace Reset | ORIGINAL_REQUEST §R4 | 5 | 5 | ✓ |

## Test Architecture
- **Test Runner**: Console application `scratch/TestGrading`
- **Invocation**: `dotnet run --project scratch/TestGrading/TestGrading.csproj`
- **Pass/Fail Semantics**: Standard exit codes (0 for pass, non-zero for fail). Outputs results in clear test execution logs.
- **Directory Layout**: Test classes implemented under `scratch/TestGrading/` directory.

## Real-World Application Scenarios (Tier 4)
| # | Scenario | Features Exercised | Complexity |
|---|----------|--------------------|------------|
| 1 | Full Writing Grading Session | F1, F2, F3, F4, F5, F6, F7 | High |
| 2 | Full Speaking Grading Session | F1, F2, F3, F4, F5, F6, F7 | High |
| 3 | Multiple Sequential Writing Tasks | F1, F2, F3, F6, F7 | High |
| 4 | Telemetry-Based Speaking Evaluation | F1, F2, F3, F6 | High |
| 5 | Conflict Resolution & Override Save | F4, F5, F6 | Medium |

## Coverage Thresholds
- Tier 1: ≥35 test cases (5 per feature)
- Tier 2: ≥35 test cases (5 per feature)
- Tier 3: ≥7 test cases (covering major feature pairs)
- Tier 4: ≥5 realistic application scenarios
