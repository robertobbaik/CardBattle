# Unity Architecture Rules

## Core Principle

Separate battle data, battle rules, UI views, and Unity scene objects as much as possible.
The prototype can be simple, but it should still be easy to explain and modify.

## Recommended Responsibilities

### BattleManager

Responsible for the overall battle flow.

- Initialize battle
- Start and end turns
- Track current battle state
- Request win/lose checks
- Coordinate player and enemy turns

### BoardManager

Responsible for battlefield and standby card placement.

- Manage player slots
- Manage enemy slots
- Track standby cards
- Remove dead cards from slots
- Refill empty slots from standby cards

### CardModel

Responsible for pure card data.

- Card type
- Owner
- Current HP
- Max HP
- Alive/dead state

CardModel should not depend on Unity UI.

### CardView

Responsible for visual representation.

- Show HP
- Show card type/name
- Show selection state
- Play simple animation or feedback

CardView should not directly calculate battle results.

### CardEffect / CardEffectResolver

Responsible for card effect execution.

- Normal attack
- Ranged attack
- Peerless attack
- Healer passive
- Future card effects

### PlayerInputController

Responsible for player selection flow.

- Select friendly card
- Select action
- Select target
- Send confirmed action to BattleManager

### EnemyAIController

Responsible for enemy turn decisions.

- Select attacker
- Select action
- Select target
- Request battle resolution

## Battle State

Use an explicit battle state enum.

```csharp
public enum BattleState
{
    None,
    Initializing,
    PlayerSelectCard,
    PlayerSelectAction,
    PlayerSelectTarget,
    Resolving,
    EnemyTurn,
    GameOver
}
```

Do not rely only on booleans such as `isPlayerTurn`, `isSelecting`, `isGameOver`.

## UI Rule

UI should display state, not own state.

Good:

- BattleManager changes state.
- UI receives update and changes text/buttons.

Bad:

- UI button directly changes turn and resolves damage by itself.

## Scene Rule

Keep each scene simple.
Use `GameScene` as the main battle scene until the full battle loop is complete.

Core and manager scripts should follow a one GameObject, one script rule when placed in a scene.
Do not stack multiple manager or core controller scripts on the same GameObject unless the developer explicitly approves it.

Recommended `GameScene` objects:

- BattleManager
- BoardManager
- PlayerInputController
- EnemyAIController
- Canvas
- PlayerCardSlots
- EnemyCardSlots
- ResultPanel

## Prefab Rule

Cards should be prefab-based.
A card prefab should include:

- CardView script
- Button or click handler
- HP text
- Card name/type text
- Selection highlight object

## ScriptableObject Rule

Use ScriptableObject for card definitions if time allows.
For the MVP, hardcoded test card creation is acceptable if marked as temporary.

Recommended card definition fields:

- Card ID
- Display name
- Card type
- Max HP
- Description
- Sprite reference

## Dependency Rule

Avoid every class referencing every manager.
Prefer one-direction flow:

```text
Input/UI -> BattleManager -> BoardManager / EffectResolver -> CardModel -> CardView Update
```

## Coroutine Rule

Coroutines may be used for simple AI delay or animation timing.
Do not hide core battle logic inside long coroutines.

## Testing Rule

Every completed battle system change should be manually tested with:

- Player card attack
- Enemy card attack
- Card death
- Slot refill
- Victory
- Defeat
