# C# Style Rules

## General Rules

- Use clear, explicit names.
- Prefer simple code over clever code.
- Keep classes focused on one responsibility.
- Avoid unnecessary abstraction before the core gameplay is stable.
- Do not write large methods that mix UI, data, and battle logic.

## Naming

Use standard C# naming conventions:

- Classes: `PascalCase`
- Interfaces: `IInterfaceName`
- Public properties: `PascalCase`
- Public methods: `PascalCase`
- Private fields: `_camelCase`
- Local variables: `camelCase`
- Constants: `PascalCase` or `UpperSnakeCase` only if already used in the project

Example:

```csharp
private int _currentHp;
public int CurrentHp => _currentHp;
public void ApplyDamage(int amount) { }
```

## Unity Serialized Fields

Prefer private serialized fields over public fields.

```csharp
[SerializeField] private TextMeshProUGUI _hpText;
```

Do not expose fields publicly unless another system genuinely needs write access.

## Method Structure

Methods should do one thing.
If a method needs comments to explain multiple phases, split it.

Preferred:

```csharp
public void ResolveAttack(CardModel attacker, CardModel target)
{
    ApplyPrimaryDamage(attacker, target);
    ApplyCounterDamageIfNeeded(attacker, target);
    RemoveDeadCards();
    RefillEmptySlots();
    CheckBattleResult();
}
```

## Unity Lifecycle Methods

- Avoid using `Awake` and `Start` in non-Core and non-Manager scripts unless there is a clear Unity lifecycle reason.
- Prefer explicit setup methods such as `Initialize`, `Bind`, or `Refresh` called by the owning manager.
- Core and Manager scripts may use `Awake` or `Start` for singleton setup, scene flow, and high-level initialization.

## Null Handling

- Validate serialized references in `Awake` or `Start` when useful.
- Avoid silent null failures.
- Use early returns for invalid player input.

## Exception Handling

- Do not add excessive defensive exception handling for simple prototype flow.
- Prefer clear control flow, early returns, and Unity console errors for expected invalid states.
- Use `try` / `catch` only when there is a real recoverable failure or an external API boundary that can reasonably throw.

## Events and Callbacks

Use events for UI updates when it keeps dependencies clean.
Avoid event chains that are difficult to trace.

Good candidates for events:

- Card HP changed
- Card removed
- Turn changed
- Battle ended

## Comments

Use comments to explain intent, not obvious syntax.

Bad:

```csharp
// subtract damage
hp -= damage;
```

Good:

```csharp
// HP also acts as attack power, so damage taken weakens future attacks.
hp -= damage;
```

## Temporary Code

Temporary shortcuts are allowed only if marked clearly.

```csharp
// TODO: Replace hardcoded test deck with card data table.
```

## Forbidden Patterns

- Do not put all battle logic inside button callbacks.
- Do not directly manipulate card HP from UI classes.
- Do not use magic numbers without explanation.
- Do not create circular dependencies between managers.
- Do not mix player input handling with enemy AI decision logic.
