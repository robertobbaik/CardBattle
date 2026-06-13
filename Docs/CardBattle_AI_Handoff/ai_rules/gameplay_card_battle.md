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

## Turn Flow

Player turn:

1. Select friendly card.
2. Select attack or skill.
3. Select valid target.
4. Apply effect.
5. Remove dead cards.
6. Refill empty slots.
7. Check win/lose.
8. End turn.

Enemy turn:

1. AI selects attacker.
2. AI selects action.
3. AI selects target.
4. Apply effect.
5. Remove dead cards.
6. Refill empty slots.
7. Check win/lose.
8. Return to player turn.

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
- Its attack behaves like a Normal card.

## Targeting Rules

- Player can select only alive friendly cards as attackers.
- Player can select only alive enemy cards as targets.
- Empty slots are not valid targets.
- Dead cards cannot act or be targeted.

## AI Rules

MVP AI:

- Select a random alive attacker.
- Select a random alive target.
- Use the selected card's default action.

Improved AI, if time allows:

- Prioritize targets that can be killed.
- Prefer Peerless targets with adjacent enemies.
- Avoid suicidal Normal attacks when a safer Ranged card is available.

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
