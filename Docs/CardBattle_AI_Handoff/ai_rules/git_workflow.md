# Git Workflow Rules

## General Policy

Keep changes small, reviewable, and tied to a clear gameplay or architecture goal.

## Before Editing

Before making changes, the AI should check or report:

- Current goal
- Expected files to change
- Related rule files
- Risk of touching unrelated systems

## Branch Naming

Recommended branch names:

```text
feature/battle-flow
feature/card-effects
feature/enemy-ai
feature/battle-ui
fix/card-refill
refactor/battle-architecture
```

## Commit Message Style

Use concise commit messages.

Recommended format:

```text
feat: add player card selection flow
fix: prevent dead cards from being targeted
refactor: separate card model from card view
ui: add turn status text
```

## Diff Rule

Before final reporting, check changed files.
The AI should summarize only actual changes.
Do not claim a file was changed if it was not changed.

## Review Rule

After each task, report:

- Changed files
- What changed
- Manual test steps
- Known issues
- Suggested next task

## Forbidden Git Actions

The AI must not run or suggest destructive commands without explicit approval.

Forbidden unless approved:

```bash
git reset --hard
git clean -fd
git rebase
git push --force
rm -rf
```

## Safe Commands

These are generally safe for inspection:

```bash
git status
git diff
git diff --stat
git log --oneline -5
```

## Submission Rule

Before preparing a submission, verify:

- Source code matches APK build
- README is updated
- AI usage is disclosed
- YouTube video link is included if required
- Build target is Android 64-bit if required
