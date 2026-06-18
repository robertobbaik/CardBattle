# Project Overview

## Project Goal

Build a Unity vertical turn-based card battle prototype.
The game must be playable from start to finish, including card placement, card selection, turn progression, basic attack execution, automatic reveal or turn-start card effects, card removal, automatic card refill, and win/lose result handling.

## Assignment Context

This project is for a junior Unity client developer assignment.
The evaluation focuses on:

- Playability
- Unity client development skill
- Game system design
- Code architecture
- UI/UX implementation
- Ability to make a small game feel fun
- Efficient AI tool usage

## Core Game Concept

The player and opponent each start with 6 cards.

- 3 cards are placed on the battlefield.
- Remaining cards stay as face-down standby cards.
- The player deck is loaded from `UserInfoManager`.
- The opponent deck is generated randomly from the full available card pool.
- The opponent deck must not include duplicate cards within the same battle.
- The player and opponent must not share the same deck list, card instances, or deck generation path.
- When a battlefield card is removed, a standby card is automatically placed if available.
- The player wins when all opponent cards are removed.
- The player loses when all player cards are removed.

## Minimum Playable Scope

The minimum version must include:

- Battle start
- Player card selection
- Action selection
- Target selection
- Damage calculation
- Automatic card effects on card reveal or turn start
- Card death/removal
- Automatic battlefield refill
- Enemy AI turn
- Turn display
- Card HP display
- Victory and defeat screen

## Development Direction

Prefer a small but complete game loop over many unfinished features.
Do not implement deck editing, card growth, collection, or advanced effects before the core battle loop is complete.
Do not implement active card skills. Card abilities should be automatic effects triggered only on card reveal or turn start.

## Recommended MVP Order

1. Static test battle setup
2. Card data and HP system
3. Enemy random non-duplicate deck setup
4. Battlefield slots and standby cards
5. Player turn flow
6. Basic attack resolution
7. Card removal and refill
8. Enemy AI turn
9. Win/lose check
10. Card type effects
11. UI feedback and polish

## Bonus Features

Only consider these after the required game loop is stable:

- Additional card types
- Simple attack / hit animation
- Damage number feedback
- Card illustration
- Simple VFX
- Deck construction
- Card growth
- Card collection

## Design Principle

The game should be easy to understand at a glance.
The player should always know:

- Whose turn it is
- Which card is selected
- Which attack action is available
- Which target can be selected
- What happened after an action
- Whether the game was won or lost
