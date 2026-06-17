using UnityEngine;

public class BerserkerCard : BaseCard
{
    public override CardType CardType => CardType.Berserker;

    public override void Attack(BaseCard target)
    {
        if (target == null)
        {
            return;
        }

        if (!CanAttack)
        {
            return;
        }

        AttackWithCounter(target, GetAttackDamage());
        MarkAsActed();
    }

    public override bool CanUseSkill => true;

    public override void UseSkill(BaseCard target = null)
    {
        if (target == null)
        {
            return;
        }

        if (HasActedThisTurn)
        {
            return;
        }

        AttackWithCounter(target, GetAttackDamage() + 2);
        TakeDamage(1, this);
        MarkAsActed();
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
