using UnityEngine;

public class PeerlessCard : BaseCard
{
    public override CardType CardType => CardType.Peerless;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
