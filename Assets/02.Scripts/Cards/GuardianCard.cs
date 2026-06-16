using UnityEngine;

public class GuardianCard : BaseCard
{
    public override CardType CardType => CardType.Guardian;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
