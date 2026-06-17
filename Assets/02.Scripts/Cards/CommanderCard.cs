using UnityEngine;

public class CommanderCard : BaseCard
{
    public override CardType CardType => CardType.Commander;

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

        target.ApplyInspired();
        MarkAsActed();
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
