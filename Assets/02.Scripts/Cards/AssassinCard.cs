public class AssassinCard : BaseCard
{
    public override CardType CardType => CardType.Assassin;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        UnityEngine.Object.Destroy(gameObject);
    }
}
