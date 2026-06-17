using UnityEngine;

public class ShamanCard : BaseCard
{
    public override CardType CardType => CardType.Shaman;

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

        int damage = GetAttackDamage() / 2;
        if (damage < 1)
        {
            damage = 1;
        }

        AttackWithCounter(target, damage);
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

        target.ApplyWeaken();
        MarkAsActed();
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
