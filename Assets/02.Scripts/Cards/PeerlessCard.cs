using System.Collections.Generic;
using UnityEngine;

public class PeerlessCard : BaseCard
{
    public override CardType CardType => CardType.Peerless;

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

        ResolvePeerlessHit(target, GetAttackDamage());
        MarkAsActed();
    }

    public override void ReflectDamage(BaseCard target, int targetHpBeforeDamage)
    {
        if (target == null)
        {
            return;
        }

        int reflectedDamage = Mathf.FloorToInt(targetHpBeforeDamage * 1.2f);
        if (reflectedDamage <= 0)
        {
            return;
        }

        TakeReflectDamage(reflectedDamage, target);
    }

    private void ResolvePeerlessHit(BaseCard target, int mainDamage)
    {
        if (target == null)
        {
            return;
        }

        target.TakeDamage(mainDamage, this);

        BaseCard adjacentCard = GetRandomOpenAdjacentEnemyCard(target);
        if (adjacentCard == null)
        {
            return;
        }

        int splashDamage = Mathf.FloorToInt(mainDamage * 0.5f);
        if (splashDamage <= 0)
        {
            return;
        }

        adjacentCard.TakeEffectDamage(splashDamage, this);
    }

    private BaseCard GetRandomOpenAdjacentEnemyCard(BaseCard target)
    {
        if (target == null)
        {
            return null;
        }

        List<BaseCard> candidates = new List<BaseCard>(2);

        if (target.Owner == CardOwner.Player && PlayerController.Instance != null)
        {
            AddOpenAdjacentCards(PlayerController.Instance.Cards, target.SlotIndex, candidates);
        }
        else if (target.Owner == CardOwner.Enemy && EnemyController.Instance != null)
        {
            AddOpenAdjacentCards(EnemyController.Instance.Cards, target.SlotIndex, candidates);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    private void AddOpenAdjacentCards(List<BaseCard> cards, int slotIndex, List<BaseCard> candidates)
    {
        if (cards == null || candidates == null)
        {
            return;
        }

        TryAddOpenCard(cards, slotIndex - 1, candidates);
        TryAddOpenCard(cards, slotIndex + 1, candidates);
    }

    private void TryAddOpenCard(List<BaseCard> cards, int slotIndex, List<BaseCard> candidates)
    {
        if (slotIndex < 0 || slotIndex >= cards.Count)
        {
            return;
        }

        BaseCard card = cards[slotIndex];
        if (card == null || !card.IsAlive || !card.IsOpen)
        {
            return;
        }

        candidates.Add(card);
    }
}
