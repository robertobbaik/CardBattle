# Asset and Scene Rules

## General Policy

Keep assets simple and lightweight.
The prototype should be judged mainly on gameplay, structure, and playability.

## Folder Structure

Recommended Unity folder structure:

```text
Assets/
  01.Scenes/
  02.Scripts/
    Battle/
    Cards/
    UI/
    AI/
    Data/
  03.Prefabs/
    Cards/
    UI/
  04.ScriptableObjects/
    Cards/
  05.Art/
    Cards/
    Icons/
    FX/
  06.Audio/
  07.Settings/
```

Use numbering only for the top-level folders under `Assets`.
Nested folders should use clear feature or asset names without numbering unless there is a strong ordering reason.

## Scene Rules

Use three scenes for the project flow:

- `LoadingScene` = build index `0`
- `LobbyScene` = build index `1`
- `GameScene` = build index `2`

Keep the MVP battle loop in `GameScene`.
Do not add extra scenes until the loading, lobby, and game flow are stable.
Load scenes by build index through `SceneType`, not by hardcoded scene name strings.

Core and manager scripts placed in a scene should use one GameObject per script.
For example, `BattleManager`, `BoardManager`, `PlayerInputController`, and `EnemyAIController` should be separate scene objects.

`GameScene` should contain:

- BattleManager
- BoardManager
- PlayerInputController
- EnemyAIController
- Canvas
- Player card slot parent
- Enemy card slot parent
- Action buttons
- Turn text
- Result panel

## Prefab Rules

Card prefab should include:

- Root GameObject
- CardView script
- Button or click handler
- HP text
- Card name/type text
- Selection highlight
- Optional illustration image

Do not make separate prefabs for every card type unless visuals are significantly different.
Prefer one card prefab driven by data.

## UI Rules

The game is vertical.
Design UI for portrait orientation.

Priority UI elements:

1. Enemy cards
2. Turn/status text
3. Player cards
4. Attack/skill buttons
5. Result panel

The player must always understand the current required action.

## Art Rules

AI-generated or temporary art may be used.
If AI-generated art is used, record it in README.

Temporary placeholders are acceptable if:

- Card type is readable
- HP is readable
- Selection state is clear
- Win/lose screen is visible

## Animation and FX Rules

Simple feedback is better than complex unfinished effects.
Recommended polish order:

1. Selection highlight
2. HP change animation
3. Damage text
4. Simple attack movement
5. Hit flash
6. Particle FX

## Resource Restrictions

- Do not import large asset packs unless approved.
- Do not add unused resources to the submission.
- Do not depend on paid or licensed assets unless the license is clear.
- Do not include unnecessary generated images if source-code-only submission is required.
