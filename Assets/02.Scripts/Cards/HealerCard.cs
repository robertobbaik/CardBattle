using UnityEngine;

public class HealerCard : BaseCard
{
    public override CardType CardType => CardType.Healer;

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
        if (target == null)
        {
            return;
        }

        TakeReflectDamage(targetHpBeforeDamage, target);
    }

    public override void OnTurnStart()
    {
        HealAllies();
    }

    private void HealAllies()
    {
        if (Owner == CardOwner.Player)
        {
            HealAlliedCards(PlayerController.Instance == null ? null : PlayerController.Instance.Cards);
            return;
        }

        HealAlliedCards(EnemyController.Instance == null ? null : EnemyController.Instance.Cards);
    }

    private void HealAlliedCards(System.Collections.Generic.List<BaseCard> cards)
    {
        if (cards == null)
        {
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            if (i >= cards.Count)
            {
                break;
            }

            BaseCard card = cards[i];
            if (card == null || card == this)
            {
                continue;
            }

            card.Heal(1);
        }
    }
}
