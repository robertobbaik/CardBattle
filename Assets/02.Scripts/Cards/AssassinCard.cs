using UnityEngine;

public class AssassinCard : BaseCard
{
    public override CardType CardType => CardType.Assassin;

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

        int damage = GetAttackDamage();
        if (target.Hp <= 3)
        {
            damage += 2;
        }

        target.TakeDamage(damage, this);
        MarkAsActed();
    }

    protected override bool ShouldReceiveCounter(BaseCard target)
    {
        return false;
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
