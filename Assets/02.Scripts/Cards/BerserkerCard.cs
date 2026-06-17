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

        target.TakeDamage(GetAttackDamage(), this);
        MarkAsActed();
    }

    public override void ReflectDamage(BaseCard target, int targetHpBeforeDamage)
    {
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

        target.TakeDamage(GetAttackDamage() + 2, this);
        TakeDamage(1, this);
        MarkAsActed();
    }

}
