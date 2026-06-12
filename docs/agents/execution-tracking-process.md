# Implementation & Progress Tracking Process

This document defines the standardized process for implementing, refining, and tracking progress on any software development plan. It ensures consistency, accountability, and high-quality output across all coding tasks, regardless of the specific project domain.

## 1. Guiding Principles

- **Atomic Tasks**: Break down all work into the smallest possible independently verifiable units. Each task should be completable within a single execution session.
- **Test-Driven Validation**: No task is considered complete without explicit validation (passing unit tests, successful builds, or defined manual verification steps).
- **Strict Context Adherence**: All changes must strictly follow existing architectural patterns, coding standards, naming conventions, and project dependencies.
- **No Silent Scope Expansion**: If a task requires work beyond its defined boundaries, execution must pause to update the plan and seek approval.

## 2. Task Lifecycle

Every atomic task follows a strict, repeatable lifecycle:

1. **Preparation**: 
   - Read the specific task definition.
   - Locate and read all relevant existing files to understand the surrounding context and conventions.
   - Identify required dependencies, test fixtures, or configuration changes.
2. **Drafting**: 
   - Implement the code changes.
   - Ensure the code compiles/builds successfully before proceeding.
3. **Validation**: 
   - Run all relevant linters, type-checkers, and build commands.
   - Execute the specific test suite for the modified components.
   - Run the full project test suite to ensure no regressions were introduced.
   - Resolve any failures immediately before moving forward.
4. **Finalization**: 
   - Update relevant documentation (code comments, README, architecture docs).
   - Mark the task as complete in the tracking log.
   - Create a distinct, logically grouped git commit with a descriptive message referencing the task.

## 3. Progress Tracking Mechanism

To maintain visibility, continuity, and state across multiple sessions:

- **Centralized Checklist**: Maintain a Markdown checklist (`- [ ] Task Name`) within the primary plan document or a dedicated `TRACKING.md` file. Checkboxes are updated in real-time as tasks transition through the lifecycle.
- **Incremental Commits**: Each completed task results in a distinct git commit. Commit messages must reference the specific task or iteration (e.g., `feat: implement wall corner generation (Task 2.2)`).
- **Session Boundaries**: At the end of every agent session, provide a concise summary detailing:
  - Tasks completed.
  - Tasks partially completed (and current state).
  - Any encountered blockers.
  - The exact next task to resume work.

## 4. Handling Roadblocks & Scope Changes

- **Blockers**: If a task cannot be completed due to missing information, unexpected complexity, or tooling failures, halt execution. Formulate a specific, targeted question for the user or create a sub-task to investigate the blocker.
- **Scope Adjustment**: If architectural discoveries during implementation invalidate a future step, update the plan document *before* proceeding. Propose the adjustment to the user for explicit approval.
- **Plan Refinement**: The plan is a living document. It must be updated to reflect the actual state of the codebase if deviations from the original plan are necessary and approved.

## 5. Definition of Done (DoD)

A task is only marked as completed (`- [x]`) when **ALL** of the following criteria are met:
1. Code compiles without errors or unintended warnings.
2. All automated tests (both new and existing) pass successfully.
3. No linter, formatter, or static analysis violations are introduced.
4. Relevant documentation has been updated to reflect the changes.
5. The change has been successfully committed to version control.
6. The corresponding checklist item is marked as complete.

## 6. Execution Workflow Summary

1. Load the current plan and identify the next unchecked task.
2. Execute the **Task Lifecycle** (Preparation -> Drafting -> Validation -> Finalization).
3. Update the **Progress Tracking Mechanism** (Checklist, Commit, Session Summary).
4. Repeat until the plan is complete or a blocker requires user intervention.
