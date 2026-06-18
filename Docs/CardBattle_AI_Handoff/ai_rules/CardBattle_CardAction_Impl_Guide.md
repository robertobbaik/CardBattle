# CardBattle 카드 행동 / 스킬 구현 지시서

## 1. 문서 목적

이 문서는 Unity 카드 배틀 프로젝트에서 카드별 공격, 스킬, 패시브, 반응 효과를 구현하기 위한 AI 작업 지시서다.
작업 대상 AI는 gpt-5.4-mini를 기준으로 한다.

본 문서의 핵심 목적은 다음과 같다.

- 모든 카드 클래스가 `BaseCard.cs`를 상속받도록 한다.
- 기본 공격과 자동 카드 효과를 명확히 분리한다.
- 한 턴에 하나의 카드가 하나의 행동만 수행하도록 한다.
- 카드 공개 효과, 턴 시작 효과, 패시브, 반격, 사망 효과는 자동 효과로 처리한다.
- 카드별 능력을 코드 구조에 맞게 구현할 수 있도록 정리한다.

---

## 1.1 최신 카드 행동 지침

이 섹션은 기존 문서의 수동 스킬 관련 설명보다 우선한다.

- 카드 액티브 스킬은 구현하지 않는다.
- 플레이어가 직접 선택하는 카드 행동은 기본 공격만 둔다.
- `스킬` 버튼, 스킬 대상 선택, `UseSkill` 기반 수동 스킬 흐름은 제거하거나 사용하지 않는다.
- 카드 고유 효과는 자동 효과로만 발동한다.
- 자동 효과 발동 시점은 카드 공개 시점 또는 해당 카드 소유자의 턴 시작 시점으로 제한한다.
- 카드 공개 시점 효과는 대기 카드가 전장에 배치되어 앞면으로 공개될 때 1회 발동한다.
- 턴 시작 효과는 해당 카드가 살아 있고 전장에 공개된 상태일 때만 발동한다.
- 자동 효과는 플레이어의 수동 행동 횟수를 소모하지 않는다.
- 향후 카드 움직임 / 연출 작업은 기본 공격, 카드 공개 자동 효과, 턴 시작 자동 효과를 기준으로 설계한다.

## 2. 전제 조건

### 2.1 프로젝트 전제

- Unity 기반 모바일 세로형 턴제 카드 배틀 게임이다.
- 전투는 플레이어와 상대 AI가 번갈아 턴을 진행한다.
- 각 진영은 전투 덱 6장을 사용한다.
- 전투 시작 시 각 진영은 전장 카드 3장과 대기 카드 3장을 가진다.
- 전장 카드가 제거되면 대기 카드가 자동으로 전장에 배치된다.

### 2.1.1 Enemy 덱 구성 규칙

- Player 덱은 `UserInfoManager`에서 가져온 유저 보유 / 선택 덱 데이터를 사용한다.
- Player 덱 생성에는 Enemy 랜덤 덱 생성 로직을 사용하지 않는다.
- Player와 Enemy가 같은 `List`, 같은 카드 인스턴스, 같은 셔플 결과를 공유하면 안 된다.
- Enemy 덱은 사용 가능한 전체 카드 풀을 활용해 전투 시작 시 자동 구성한다.
- 전체 카드 풀을 무작위로 섞은 뒤 전투 덱 크기만큼 앞에서부터 선택한다.
- 한 전투의 Enemy 덱 안에는 같은 카드가 중복으로 들어가면 안 된다.
- 사용 가능한 카드 수가 전투 덱 크기보다 적으면, 사용 가능한 모든 카드를 1장씩만 넣고 설정 경고 로그를 남긴다.
- 선택된 Enemy 덱의 앞 3장은 전장 카드로 배치한다.
- 나머지 카드는 섞인 순서를 유지한 채 대기 카드로 둔다.

### 2.2 기본 카드 룰

기본 과제 문서의 카드 종류는 다음 4종이다.

- 일반
- 원거리
- 무쌍
- 힐러

이 프로젝트에서는 확장 카드까지 포함해 총 10종을 구현 대상으로 둔다.

---

## 3. 핵심 행동 규칙

### 3.1 한 턴 하나의 행동 원칙

플레이어는 자신의 턴에 아군 카드 1장을 선택한다.
선택한 카드는 기본 공격만 수행할 수 있다.
행동이 끝나면 턴이 종료된다.

```text
카드 선택
↓
공격 선택
↓
대상 선택
↓
효과 적용
↓
턴 종료
```

### 3.2 공격과 스킬의 관계

액티브 스킬은 없다.
카드 고유 효과는 수동 행동이 아니라 자동 효과다.

```text
수동 행동: 기본 공격
자동 행동: 카드 공개 효과 / 턴 시작 효과
```

### 3.3 자동 효과의 예외

아래 효과는 행동 횟수를 소모하지 않는다.

- 패시브 효과
- 카드 공개 효과
- 턴 시작 효과
- 반격 효과
- 사망 시 효과
- 피해 감소 효과
- 상태 이상 만료 처리

예시:

```text
힐러의 턴 시작 회복은 자동 효과다.
힐러가 자동 회복을 발동했더라도, 플레이어는 그 턴에 카드 1장의 기본 공격을 사용할 수 있다.
```

---

## 4. 용어 정의

| 용어 | 의미 |
|---|---|
| 기본 공격 | 모든 카드가 기본적으로 사용할 수 있는 공격 행동 |
| 액티브 스킬 | 플레이어가 직접 선택해 사용하는 특수 행동. 현재 지침에서는 사용하지 않음 |
| 카드 공개 효과 | 카드가 전장에 공개될 때 자동으로 1회 발동하는 효과 |
| 턴 시작 효과 | 카드 소유자의 턴 시작 시 자동으로 발동하는 효과 |
| 패시브 | 자동으로 적용되는 지속 또는 조건부 효과 |
| 반격 | 일반 공격 등 특정 공격 후 대상이 자동으로 되돌려주는 피해 |
| 사망 효과 | 카드 HP가 0 이하가 되어 제거될 때 발동하는 효과 |
| 상태 효과 | 약화, 격려 등 일정 조건이나 턴 동안 유지되는 효과 |

---

## 5. 추천 폴더 구조

```text
Assets/
  _Project/
    Scripts/
      Battle/
        BattleManager.cs
        BattleState.cs
        BattleSide.cs
        BattleActionType.cs
        DamageContext.cs
      Cards/
        BaseCard.cs
        CardType.cs
        NormalCard.cs
        RangedCard.cs
        MusouCard.cs
        HealerCard.cs
        GuardianCard.cs
        AssassinCard.cs
        BerserkerCard.cs
        ShamanCard.cs
        CommanderCard.cs
        BomberCard.cs
        CardFactory.cs
        CardRuntimeData.cs
      StatusEffects/
        StatusEffectType.cs
        StatusEffect.cs
        StatusEffectController.cs
      UI/
        CardView.cs
        BattleCommandPanel.cs
```

필요 없는 파일은 현재 프로젝트 구조에 맞게 조정해도 된다.
단, `BaseCard.cs`와 파생 카드 클래스 구조는 유지한다.

---

## 6. enum 정의

### 6.1 CardType.cs

```csharp
public enum CardType
{
    Normal = 1,
    Ranged = 2,
    Musou = 3,
    Healer = 4,
    Guardian = 5,
    Assassin = 6,
    Berserker = 7,
    Shaman = 8,
    Commander = 9,
    Bomber = 10
}
```

### 6.2 BattleActionType.cs

```csharp
public enum BattleActionType
{
    None = 0,
    Attack = 1,
    Skill = 2
}
```

### 6.3 StatusEffectType.cs

```csharp
public enum StatusEffectType
{
    None = 0,
    Weaken = 1,
    Inspired = 2,
    Guard = 3
}
```

---

## 7. BaseCard.cs 설계

모든 카드 클래스는 `BaseCard`를 상속한다.
`BaseCard`는 공통 상태와 기본 행동 흐름을 제공한다.

### 7.1 BaseCard 책임

`BaseCard`는 다음 책임을 가진다.

- 카드 ID 보관
- 카드 타입 보관
- 현재 HP / 최대 HP 관리
- 생존 여부 관리
- 소유 진영 관리
- 전장 슬롯 인덱스 관리
- 공개 여부 관리
- 행동 가능 여부 관리
- 기본 공격 흐름 제공
- 카드 공개 / 턴 시작 / 사망 / 피해 처리 훅 제공

### 7.2 BaseCard 예시 구조

```csharp
public abstract class BaseCard
{
    public int CardId { get; protected set; }
    public CardType CardType { get; protected set; }

    public int MaxHp { get; protected set; }
    public int CurrentHp { get; protected set; }

    public bool IsAlive => CurrentHp > 0;
    public bool IsRevealed { get; private set; }
    public bool HasActedThisTurn { get; private set; }

    public int OwnerSide { get; private set; }
    public int SlotIndex { get; private set; }

    public virtual bool CanAttack => IsAlive && !HasActedThisTurn;

    public virtual void Initialize(CardRuntimeData data, int ownerSide)
    {
        CardId = data.CardId;
        CardType = (CardType)data.CardType;
        MaxHp = data.MaxHp;
        CurrentHp = data.StartHp;
        OwnerSide = ownerSide;
        IsRevealed = false;
        HasActedThisTurn = false;
        SlotIndex = -1;
    }

    public void Reveal()
    {
        IsRevealed = true;
        OnReveal();
    }

    public void SetSlotIndex(int slotIndex)
    {
        SlotIndex = slotIndex;
    }

    public void ResetTurnAction()
    {
        HasActedThisTurn = false;
    }

    public void MarkAsActed()
    {
        HasActedThisTurn = true;
    }

    public virtual void OnTurnStart(BattleContext context)
    {
    }

    public virtual void OnReveal()
    {
    }

    public virtual void Attack(BaseCard target, BattleContext context)
    {
        if (!CanAttack)
            return;

        int damage = CalculateAttackDamage(target, context);
        target.TakeDamage(damage, this, context);

        if (ShouldReceiveCounter(target, context))
        {
            int counterDamage = CalculateCounterDamage(target, context);
            TakeDamage(counterDamage, target, context);
        }

        MarkAsActed();
    }

    protected virtual int CalculateAttackDamage(BaseCard target, BattleContext context)
    {
        return CurrentHp;
    }

    protected virtual bool ShouldReceiveCounter(BaseCard target, BattleContext context)
    {
        return target != null && target.IsAlive;
    }

    protected virtual int CalculateCounterDamage(BaseCard target, BattleContext context)
    {
        return target.CurrentHp;
    }

    public virtual void TakeDamage(int amount, BaseCard attacker, BattleContext context)
    {
        int finalDamage = Math.Max(1, amount);
        CurrentHp -= finalDamage;

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            OnDeath(attacker, context);
        }
    }

    public virtual void Heal(int amount)
    {
        if (!IsAlive)
            return;

        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
    }

    public virtual void OnDeath(BaseCard killer, BattleContext context)
    {
    }
}
```

주의:
위 코드는 구현 방향 예시다.
프로젝트의 실제 네임스페이스, 자료형, BattleContext 구조에 맞춰 조정한다.

---

## 8. 카드별 능력 정의

### 8.0 카드별 능력 최신 해석 규칙

아래 카드별 설명 중 `스킬`, `CanUseSkill`, `UseSkill`, `스킬 사용`이라고 적힌 과거 문구는 더 이상 액티브 스킬로 구현하지 않는다.
해당 효과를 유지해야 한다면 다음 중 하나로 재설계한다.

- 카드 공개 시 1회 자동 발동하는 `OnReveal` 효과
- 카드 소유자의 턴 시작 시 자동 발동하는 `OnTurnStart` 효과
- 기본 공격의 피해 계산 / 반격 여부 / 사망 반응에 포함되는 자동 효과

플레이어가 직접 스킬 버튼을 누르거나 스킬 대상을 선택하는 흐름은 만들지 않는다.

---

# 8.1 NormalCard

## 역할

기본 공격형 카드.

## 행동

- 공격 가능
- 스킬 없음

## 공격

```text
선택한 적 카드에게 자신의 현재 HP만큼 피해를 준다.
공격한 카드는 대상 카드의 현재 HP만큼 반격 피해를 받는다.
```

## 구현 규칙

`BaseCard`의 기본 공격 로직을 그대로 사용한다.

```csharp
public sealed class NormalCard : BaseCard
{
    public override bool CanUseSkill => false;
}
```

---

# 8.2 RangedCard

## 역할

반격 피해를 받지 않는 안정 공격형 카드.

## 행동

- 공격 가능
- 스킬 없음

## 공격

```text
선택한 적 카드에게 자신의 현재 HP만큼 피해를 준다.
반격 피해를 받지 않는다.
```

## 구현 규칙

`ShouldReceiveCounter`를 false로 오버라이드한다.

```csharp
public sealed class RangedCard : BaseCard
{
    protected override bool ShouldReceiveCounter(BaseCard target, BattleContext context)
    {
        return false;
    }
}
```

---

# 8.3 MusouCard

## 역할

광역 공격형 카드.

## 행동

- 기본 공격 가능
- 스킬 가능
- 공격 또는 스킬 중 하나만 사용 가능

## 기본 공격

일반 카드와 동일하다.

## 스킬: 무쌍 베기

```text
선택한 적 카드에게 자신의 현재 HP 100% 피해를 준다.
추가로 선택한 카드와 인접한 적 카드 중 무작위 1장에게 자신의 현재 HP 50% 피해를 준다.
인접한 적 카드가 없다면 추가 피해는 발생하지 않는다.
```

## 구현 규칙

- `CanUseSkill`은 true.
- `UseSkill`에서 대상에게 100% 피해 적용.
- `BattleContext` 또는 `BattleSide`를 통해 대상의 인접 슬롯을 찾는다.
- 살아있는 인접 적 카드 중 무작위 1장에게 추가 피해 적용.
- 추가 피해는 `CurrentHp / 2`로 계산한다.
- 스킬 사용 후 `MarkAsActed()` 호출.

---

# 8.4 HealerCard

## 역할

회복 지원형 카드.

## 행동

- 기본 공격 가능
- 스킬 없음 권장
- 턴 시작 패시브 있음

## 기본 공격

일반 카드와 동일하다.

## 패시브: 치유의 기도

```text
자신의 턴 시작 시 자신을 제외한 아군 전장 카드들의 HP를 1 회복한다.
```

## 구현 규칙

- `OnTurnStart`에서 아군 전장 카드 목록을 순회한다.
- 자신은 회복하지 않는다.
- 죽은 카드는 회복하지 않는다.
- `MaxHp`를 초과하지 않는다.
- 패시브는 행동 횟수를 소모하지 않는다.

---

# 8.5 GuardianCard

## 역할

방어 / 탱커 카드.

## 행동

- 기본 공격 가능
- 스킬 가능
- 공격 또는 스킬 중 하나만 사용 가능

## 기본 공격

```text
자신의 현재 HP 기준으로 피해를 준다.
반격 피해를 받는다.
```

## 스킬: 방어 태세

```text
이번 상대 턴이 끝날 때까지 아군 전장 카드가 받는 피해를 1 감소시킨다.
```

## 구현 규칙

- `CanUseSkill`은 true.
- `UseSkill`은 대상 없이 사용할 수 있어도 된다.
- 스킬 사용 시 아군 진영에 `Guard` 상태를 부여한다.
- 피해 감소 후 최종 피해는 최소 1 이상이어야 한다.
- 스킬 사용 후 `MarkAsActed()` 호출.

---

# 8.6 AssassinCard

## 역할

저격 / 처형 카드.

## 행동

- 기본 공격 가능
- 스킬 가능
- 공격 또는 스킬 중 하나만 사용 가능

## 기본 공격

```text
선택한 적 카드에게 자신의 현재 HP만큼 피해를 준다.
반격 피해를 받지 않는다.
```

## 스킬: 처형

```text
대상 카드의 현재 HP가 기준 이하이면 추가 피해를 준다.
```

기본 기준:

```text
대상 HP가 3 이하이면 추가 피해 +2
```

## 구현 규칙

- 기본 공격은 `RangedCard`처럼 반격을 받지 않도록 구현한다.
- `UseSkill`에서 대상 HP 조건을 확인한다.
- 조건을 만족하면 추가 피해를 적용한다.
- 조건을 만족하지 않아도 기본 피해는 적용할지 여부를 결정해야 한다.
- 추천: 조건을 만족하지 않아도 `CurrentHp` 피해는 주고, 조건 만족 시 +2를 더한다.
- 스킬 사용 후 `MarkAsActed()` 호출.

---

# 8.7 BerserkerCard

## 역할

고위험 고화력 카드.

## 행동

- 기본 공격 가능
- 스킬 가능
- 공격 또는 스킬 중 하나만 사용 가능

## 기본 공격

일반 카드와 동일하다.

## 스킬: 광폭 일격

```text
대상에게 자신의 현재 HP + 2 피해를 준다.
공격 후 자신에게 1 피해를 준다.
반격 피해도 정상적으로 받는다.
```

## 구현 규칙

스킬 처리 순서:

1. 대상에게 피해 적용
2. 대상이 살아있으면 반격 피해 적용
3. 자신에게 자해 피해 1 적용
4. 사망 처리
5. `MarkAsActed()` 호출

---

# 8.8 ShamanCard

## 역할

디버프 / 약화 카드.

## 행동

- 기본 공격 가능
- 스킬 가능
- 공격 또는 스킬 중 하나만 사용 가능

## 기본 공격

```text
자신의 현재 HP 50% 피해를 준다.
반격 피해를 받는다.
```

## 스킬: 약화 저주

```text
선택한 적 카드에게 약화 상태를 부여한다.
약화된 카드는 다음 공격 피해량이 2 감소한다.
지속은 1회 또는 1턴으로 처리한다.
```

## 구현 규칙

- `CanUseSkill`은 true.
- `UseSkill`에서 대상에게 `Weaken` 상태를 부여한다.
- 약화 상태는 다음 피해 계산 시 적용한다.
- 피해 감소 후 최종 피해는 최소 1 이상이어야 한다.
- 스킬 사용 후 `MarkAsActed()` 호출.

---

# 8.9 CommanderCard

## 역할

버프 / 전술 지원 카드.

## 행동

- 기본 공격 가능
- 스킬 가능
- 공격 또는 스킬 중 하나만 사용 가능

## 기본 공격

```text
자신의 현재 HP 기준으로 피해를 준다.
반격 피해를 받는다.
```

## 스킬: 전술 지휘

```text
선택한 아군 카드 1장의 다음 공격 피해를 2 증가시킨다.
```

## 구현 규칙

- `UseSkill`의 대상은 적이 아니라 아군 카드다.
- 대상 아군에게 `Inspired` 상태를 부여한다.
- `Inspired` 상태는 다음 공격 피해 계산 시 +2 적용 후 제거한다.
- 스킬 사용 후 `MarkAsActed()` 호출.

---

# 8.10 BomberCard

## 역할

자폭 / 사망 피해 카드.

## 행동

- 기본 공격 가능
- 스킬 가능
- 사망 효과 있음
- 공격 또는 스킬 중 하나만 사용 가능

## 기본 공격

```text
선택한 적 카드에게 자신의 현재 HP만큼 피해를 준다.
반격 피해를 받지 않는다.
```

## 스킬: 폭탄 투척

```text
대상에게 고정 피해 3을 준다.
대상과 인접한 적 카드 1장에게 피해 1을 준다.
```

## 사망 효과: 최후의 폭발

```text
폭탄병이 사망하면 자신을 처치한 카드에게 2 피해를 준다.
```

## 구현 규칙

- 기본 공격은 반격을 받지 않는 것으로 처리한다.
- `UseSkill`에서 대상에게 3 피해 적용.
- 인접한 적 카드 중 살아있는 카드 1장에게 1 피해 적용.
- `OnDeath`에서 killer가 null이 아니고 살아있으면 killer에게 2 피해 적용.
- 사망 효과는 행동 횟수를 소모하지 않는다.

---

## 9. 상태 효과 구현 규칙

### 9.1 Weaken

```text
다음 공격 피해량 -2
적용 후 제거
최종 피해는 최소 1 이상
```

### 9.2 Inspired

```text
다음 공격 피해량 +2
적용 후 제거
```

### 9.3 Guard

```text
아군이 받는 피해 -1
상대 턴 종료 시 제거
최종 피해는 최소 1 이상
```

---

## 10. UI 행동 규칙

카드를 선택하면 명령 패널을 표시한다.

```text
[공격] [취소]
```

### 10.1 액티브 스킬 버튼 금지

액티브 스킬은 없으므로 `스킬` 버튼을 만들지 않는다.
카드 고유 효과는 카드 공개 시점 또는 턴 시작 시점에 자동으로 발동한다.

```text
[공격] [취소]
```

카드마다 UI 레이아웃이 바뀌지 않도록 공격 / 취소 버튼 위치는 유지한다.

### 10.2 대상 규칙

- 공격 대상: 적 카드
- 자동 효과 대상: 효과별 규칙에 따라 코드가 자동 선택하거나 전체 대상에 적용한다.
- 자동 효과 때문에 플레이어에게 별도 대상 선택을 요구하지 않는다.

### 10.3 행동 확정 후

공격이 확정되면 다음을 수행한다.

```text
효과 적용
↓
반격 처리
↓
사망 처리
↓
대기 카드 자동 배치
↓
승패 체크
↓
턴 종료
```

---

## 11. BattleManager 처리 흐름

### 11.1 플레이어 턴

```text
PlayerTurnStart
↓
아군 카드들의 OnTurnStart 호출
↓
카드 선택 상태 진입
↓
공격 선택
↓
대상 선택
↓
공격 실행
↓
카드 제거 처리
↓
대기 카드 자동 배치
↓
새로 공개된 카드들의 OnReveal 호출
↓
승패 체크
↓
EnemyTurn으로 전환
```

### 11.2 상대 AI 턴

```text
EnemyTurnStart
↓
상대 카드들의 OnTurnStart 호출
↓
행동 가능한 카드 선택
↓
대상 선택
↓
공격 실행
↓
카드 제거 처리
↓
대기 카드 자동 배치
↓
새로 공개된 카드들의 OnReveal 호출
↓
승패 체크
↓
PlayerTurn으로 전환
```

---

## 12. AI 구현 우선순위

### 12.1 MVP AI

처음에는 단순하게 구현한다.

```text
1. 살아있는 카드 중 랜덤 선택
2. 공격 가능한 적 대상을 랜덤 선택
3. 기본 공격 실행
```

### 12.2 개선 AI

추후 아래 조건을 추가할 수 있다.

- 킬 가능한 대상 우선 공격
- 힐러 / 지휘관 / 주술사 우선 제거
- 무쌍은 인접 카드가 있는 대상을 우선 선택
- 턴 시작 자동 효과를 고려해 위험도가 높은 카드를 우선 제거
- 카드 공개 자동 효과가 강한 카드는 대기 카드 공개 타이밍을 고려

---

## 13. 구현 금지 사항

아래는 이번 작업에서 피한다.

- 액티브 스킬을 구현하지 말 것
- 스킬 버튼, 스킬 대상 선택, 수동 `UseSkill` 흐름을 만들지 말 것
- 카드별 행동 로직을 UI 버튼 안에 직접 작성하지 말 것
- 카드 HP를 UI 클래스에서 직접 변경하지 말 것
- 패시브 효과를 수동 행동으로 처리하지 말 것
- BattleManager에 모든 카드별 효과를 하드코딩하지 말 것
- 덱 편성, 카드 성장, 도감은 이번 작업 범위에 포함하지 말 것

---

## 14. 구현 완료 기준

아래 조건을 만족하면 구현 완료로 본다.

### 14.1 카드 행동

- 일반 카드가 기본 공격을 수행한다.
- 원거리 카드가 반격 없이 공격한다.
- 카드 공개 시 자동 효과가 필요한 카드는 공개 직후 1회 효과를 발동한다.
- 턴 시작 자동 효과가 필요한 카드는 소유자 턴 시작 시 효과를 발동한다.
- 힐러가 턴 시작 시 아군을 회복한다.
- 폭탄병이 사망 폭발을 처리한다.

### 14.2 행동 제한

- 한 턴에 선택한 카드는 기본 공격만 사용한다.
- 행동 후 턴이 종료된다.
- 카드 공개 효과, 턴 시작 효과, 패시브, 반격, 사망 효과는 행동 횟수를 소모하지 않는다.

### 14.3 UI

- 카드 선택 시 공격 / 취소 버튼이 표시된다.
- 스킬 버튼은 표시하지 않는다.
- 공격 대상은 적 카드만 선택된다.
- 행동 후 UI 상태가 초기화된다.

### 14.4 전투 결과

- HP가 0 이하인 카드는 제거된다.
- 빈 슬롯이 생기면 대기 카드가 자동 배치된다.
- 상대 카드가 모두 사라지면 승리한다.
- 플레이어 카드가 모두 사라지면 패배한다.

---

## 15. 수동 테스트 체크리스트

### 기본 공격 테스트

```text
일반 카드로 적을 공격한다.
적 HP가 감소한다.
일반 카드는 반격 피해를 받는다.
```

### 원거리 테스트

```text
원거리 카드로 적을 공격한다.
적 HP가 감소한다.
원거리 카드는 반격 피해를 받지 않는다.
```

### 카드 공개 자동 효과 테스트

```text
대기 카드가 전장에 배치되어 공개된다.
해당 카드에 공개 효과가 있으면 자동으로 1회 발동한다.
플레이어에게 스킬 버튼이나 대상 선택을 요구하지 않는다.
```

### 힐러 테스트

```text
아군 카드 HP가 감소한 상태에서 내 턴을 시작한다.
힐러 자신을 제외한 아군 카드 HP가 1 회복된다.
회복 후 HP는 각 카드의 MaxHp를 초과하지 않는다.
```

### 행동 제한 테스트

```text
카드가 공격을 사용한다.
행동 후 턴이 종료된다.
스킬 버튼은 표시되지 않는다.
```

```text
카드가 스킬을 사용한다.
같은 턴에 공격을 다시 사용할 수 없어야 한다.
```

### 사망 / 대기 카드 테스트

```text
카드 HP가 0 이하가 된다.
해당 카드가 제거된다.
대기 카드가 남아 있으면 빈 슬롯에 자동 배치된다.
```

---

## 16. gpt-5.4-mini 작업 지시 요약

작업 시작 시 다음 순서로 진행한다.

```text
1. AGENTS.md를 먼저 읽는다.
2. 카드 / 전투 / UI 관련 세부 지침을 확인한다.
3. 기존 코드 구조를 확인한다.
4. BaseCard.cs 중심의 상속 구조를 설계한다.
5. CardType enum을 정수값 기준으로 정의한다.
6. 공격 / 스킬 / 패시브 / 반응 효과를 분리한다.
7. 한 턴 하나의 행동 규칙을 구현한다.
8. 카드별 파생 클래스를 구현한다.
9. UI 명령 패널과 연결한다.
10. 수동 테스트를 진행한다.
11. 변경 파일과 테스트 결과를 보고한다.
```

작업 후 반드시 보고할 내용:

```text
- 변경한 파일 목록
- 구현한 카드 클래스 목록
- 공격 / 스킬 / 패시브 처리 방식
- 한 턴 하나의 행동 제한이 적용된 위치
- 수동 테스트 결과
- 남은 TODO
```
