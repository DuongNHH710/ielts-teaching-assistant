# BRIEFING — 2026-06-22T11:08:50+07:00

## Mission
Execute the Implementation Track to pass all 82 E2E tests and perform adversarial hardening.

## 🔒 My Identity
- Archetype: teamwork_preview_worker (but operating as the Sub-Orchestrator for Implementation Track)
- Roles: orchestrator, user_liaison, human_reporter, successor
- Working directory: d:\Project\ielts-teaching-assistant\.agents\sub_orch_implementation
- Original parent: main agent
- Original parent conversation ID: 44f49f85-6c4c-401d-b4ce-8c085d3437dc

## 🔒 My Workflow
- **Pattern**: Project Pattern (Implementation Track Sub-Orchestrator)
- **Scope document**: d:\Project\ielts-teaching-assistant\PROJECT.md
1. **Decompose**: Decompose by E2E test tier (Tiers 1, 2, 3, 4) followed by Adversarial Hardening (Tier 5).
2. **Dispatch & Execute**:
   - **Delegate**: Delegate code changes, tests running, and validation checks to worker, reviewer, challenger, and auditor subagents.
3. **On failure** (in this order):
   - Retry: nudge stuck agent or re-send task
   - Replace: spawn fresh agent with partial progress
   - Skip: proceed without (only if non-critical)
   - Redistribute: split stuck agent's remaining work
   - Redesign: re-partition decomposition
   - Escalate: report to parent (as a last resort)
4. **Succession**: Self-succeed at 16 spawns, write handoff.md, spawn successor.
- **Work items**:
  1. Milestone 1: Tier 1 Feature Coverage [pending]
  2. Milestone 2: Tier 2 Boundary & Edge Cases [pending]
  3. Milestone 3: Tier 3 Cross-Feature [pending]
  4. Milestone 4: Tier 4 Real-World Scenarios [pending]
  5. Milestone 5: Adversarial Hardening [pending]
- **Current phase**: 1
- **Current focus**: Milestone 1: Tier 1 Feature Coverage

## 🔒 Key Constraints
- NEVER write, modify, or create source code files directly.
- NEVER run build/test commands yourself.
- Delegate all engineering actions to workers.
- Include the integrity warning in all workers' prompts.

## Current Parent
- Conversation ID: 44f49f85-6c4c-401d-b4ce-8c085d3437dc
- Updated: not yet

## Key Decisions Made
- Use the complete ViewModel implementations from the Stubs folder as the reference/direct implementation source for WritingEvaluationViewModel and SpeakingEvaluationViewModel.
- Use the cached XAML files from `obj/` folder directly to restore Views in `src/IeltsTeachingAssistant/Views/`.

## Team Roster
| Agent | Type | Work Item | Status | Conv ID |
|-------|------|-----------|--------|---------|
| worker_1 | teamwork_preview_worker | Restoring Views, VMs, registering DI, updating navigation and running test suite | in-progress | 9641d1b2-4c38-4944-8c9d-55c2844f78a0 |

## Succession Status
- Succession required: no
- Spawn count: 1 / 16
- Pending subagents: 9641d1b2-4c38-4944-8c9d-55c2844f78a0
- Predecessor: none
- Successor: not yet spawned

## Active Timers
- Heartbeat cron: task-105
- Safety timer: none

## Artifact Index
- d:\Project\ielts-teaching-assistant\.agents\sub_orch_implementation\progress.md — Liveness and task checklist status
- d:\Project\ielts-teaching-assistant\.agents\sub_orch_implementation\ORIGINAL_REQUEST.md — Verbatim user instructions
