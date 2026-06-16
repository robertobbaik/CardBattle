using UnityEngine;

public class BerserkerCard : BaseCard
{
    public override CardType CardType => CardType.Berserker;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
