using UnityEngine;

public class BomberCard : BaseCard
{
    public override CardType CardType => CardType.Bomber;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
