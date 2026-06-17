using UnityEngine;

public class GuardianCard : BaseCard
{
    public override CardType CardType => CardType.Guardian;

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
        if (HasActedThisTurn)
        {
            return;
        }

        if (Owner == CardOwner.Player)
        {
            PlayerController.Instance?.ApplyGuard();
        }
        else if (Owner == CardOwner.Enemy)
        {
            EnemyController.Instance?.ApplyGuard();
        }

        MarkAsActed();
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
