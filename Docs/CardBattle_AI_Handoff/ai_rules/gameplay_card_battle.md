# Gameplay Rules

## Current Project Gameplay Direction

The current project is a vertical turn-based card battle prototype.
Treat this document as the gameplay-specific rule file for the card battle project.

## Core Battle Rules

- Player and opponent each start with 6 cards.
- Each side places 3 cards on the battlefield.
- Remaining cards stay as standby cards.
- When a battlefield card dies, refill the empty slot from standby cards if available.
- If no standby card remains, the slot stays empty.
- The player wins when all enemy cards are removed.
- The player loses when all player cards are removed.

## Deck Setup Rules

- The player deck must be loaded from `UserInfoManager`.
- Do not generate the player deck from the enemy random deck generator.
- Do not reuse the same deck instance or same selected card list for both sides.
- The enemy deck must be generated from the full available card pool.
- Shuffle all available card definitions, then take cards until the enemy battle deck is full.
- The enemy deck must not contain duplicate cards in a single battle.
- If the available card pool is smaller than the required enemy deck size, use every available card once and log a setup warning.
- The first 3 enemy cards become battlefield cards.
- The remaining enemy cards become standby cards in shuffled order.

## Turn Flow

Player turn:

1. Trigger automatic turn-start effects for revealed friendly cards.
2. Select friendly card.
3. Select attack.
4. Select valid target.
5. Apply effect.
6. Remove dead cards.
7. Refill empty slots.
8. Reveal refilled cards and trigger automatic reveal effects.
9. Check win/lose.
10. End turn.

Enemy turn:

1. Trigger automatic turn-start effects for revealed enemy cards.
2. AI selects attacker.
3. AI selects target.
4. Apply default attack.
5. Remove dead cards.
6. Refill empty slots.
7. Reveal refilled cards and trigger automatic reveal effects.
8. Check win/lose.
9. Return to player turn.

## Card Skill Policy

- Cards do not have active skills.
- Do not implement a manual skill button, manual skill target selection, or player-selected `UseSkill` flow.
- Card-specific abilities must be automatic.
- Automatic abilities may trigger only when a card is revealed or at the start of that card owner's turn.
- Reveal effects trigger once when a standby card enters the battlefield and is flipped face-up.
- Turn-start effects trigger only for alive, revealed battlefield cards.
- Automatic abilities do not consume the player's manual action for the turn.

## Card Types

### Normal

- Deals damage equal to its current HP to the selected enemy card.
- Receives counter damage equal to the target card's current HP.

### Ranged

- Deals damage equal to its current HP to the selected enemy card.
- Does not receive counter damage.

### Peerless

- Deals damage equal to 100% of current HP to the selected enemy card.
- Additionally deals 50% of current HP to one random enemy card adjacent to the selected target.
- If there is no adjacent enemy card, no additional damage occurs.

### Healer

- At the start of its owner's turn, heals allied cards by 1 HP.
- The healer does not heal itself.
- Healing cannot increase a card's HP above its maximum HP.
- Its attack behaves like a Normal card.

## Targeting Rules

- Player can select only alive friendly cards as attackers.
- Player can select only alive enemy cards as targets.
- Empty slots are not valid targets.
- Dead cards cannot act or be targeted.

## AI Rules

Current Enemy AI:

- Build all possible attacker and target pairs from alive, open, attack-capable enemy cards and alive, open player cards.
- Score every possible action pair, then execute one of the highest-scoring pairs.
- If multiple action pairs have the same best score, choose randomly among those tied best actions.
- Prioritize targets that can be killed.
- Prefer lower HP targets.
- Prefer removing support or control cards first: Healer, Commander, and Shaman.
- Treat Assassin, Peerless, and Bomber as high-threat targets.
- Prefer using Ranged attackers because they do not receive reflected damage.
- Avoid choices with high expected reflected damage when a safer useful action is available.
- Account for Berserker self-damage when choosing an attacker.
- Give extra value to Peerless and Bomber attacks when their splash damage can hit additional cards.

## Balance Rule

Avoid overcomplicating balance in the first version.
The important part is that the battle can finish and the player can understand why cards died.

## Feedback Rule

Every action should provide visible feedback:

- Selected card highlight
- Target highlight
- HP update
- Damage text or simple log
- Turn text update
- Result panel
