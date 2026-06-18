using System.Collections.Generic;
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

    protected override void OnEnterField()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsOpponentRevealDebuffSuppressed(Owner))
        {
            return;
        }

        List<BaseCard> cards = GetEnemyBattlefieldCards();
        for (int i = 0; i < cards.Count; i++)
        {
            BaseCard card = cards[i];
            if (card == null || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            card.ReduceCurrentHealthAboveInitial(this);
        }
    }

    private List<BaseCard> GetEnemyBattlefieldCards()
    {
        if (Owner == CardOwner.Player && EnemyController.Instance != null)
        {
            return EnemyController.Instance.Cards;
        }

        if (Owner == CardOwner.Enemy && PlayerController.Instance != null)
        {
            return PlayerController.Instance.Cards;
        }

        return new List<BaseCard>();
    }
}
