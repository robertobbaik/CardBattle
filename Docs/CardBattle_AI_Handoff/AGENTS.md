# AGENTS.md

## Purpose
This file is the entry point for AI agents working on this Unity project.
All agents must read this file first, then open only the detailed rule files needed for the current task.

The project is a Unity vertical turn-based card battle prototype for a client developer assignment.
The goal is not to build a large commercial game, but to deliver a playable, understandable, and well-structured prototype.

## Required Workflow

1. The developer gives the AI the implementation goal, restrictions, and priorities.
2. The AI starts from `AGENTS.md` and checks the project instructions.
3. The AI reads the detailed rule files relevant to the current request.
4. Before implementation, the AI explains the intended structure, scope, and risks.
5. After writing code, the AI checks build impact, test coverage, and changed files.
6. The developer reviews the result and updates these rules when needed.

## Rule Files

Read these files depending on the task:

- `ai_rules/project_overview.md`  
  Project goal, development direction, scope, and priorities.

- `ai_rules/csharp_style.md`  
  C# code style, naming rules, class structure, and readability rules.

- `ai_rules/unity_architecture.md`  
  Unity scene structure, MonoBehaviour responsibilities, data/view separation, and manager rules.

- `ai_rules/gameplay_card_battle.md`  
  Card battle gameplay rules, turn flow, card types, targeting, AI behavior, and battle feedback.

- `ai_rules/asset_scene_rules.md`  
  Asset, prefab, scene, UI, and resource management rules.

- `ai_rules/git_workflow.md`  
  Git branch, commit, diff, and review rules.

## Default Development Priorities

When priorities conflict, follow this order:

1. A playable battle loop
2. Correct battle rules
3. Clear code structure
4. Simple and understandable UI
5. Build stability
6. Visual polish
7. Optional bonus features

## Mandatory Before Coding

Before editing or creating code, the AI must state:

- Which files or systems will be touched
- Which rule files were used
- Expected behavior after implementation
- Risks or assumptions

## Mandatory After Coding

After implementation, the AI must report:

- Changed files
- Main implementation summary
- How to test manually
- Build or compile status if available
- Remaining risks or TODOs

Do not repeatedly report routine forbidden-pattern checks in every completion message.
Follow the rule files while working, and mention those checks only when a violation, exception, or relevant risk exists.

## Restrictions

- Do not silently change unrelated systems.
- Do not introduce large frameworks without approval.
- Do not replace existing architecture without explaining why.
- Do not add bonus systems before the core battle loop works.
- Do not hardcode behavior that should belong to card data or battle rules unless it is temporary and clearly marked.

## AI Usage Disclosure

When preparing README or submission documents, clearly state which AI tools were used and for what purpose, such as:

- Initial architecture planning
- Code review and refactoring suggestions
- README drafting
- UI/UX idea generation
- Temporary art/resource generation
