#  Jungle Endless Run
An endless runner game created with Unity, where players switch lanes while running. It includes power-ups and a smart enemy that chases you. Players control a character running through randomly generated jungle landscapes, collecting coins, dodging obstacles, and trying to escape from the enemy behind them.
## Overview
Jungle Endless Run is an exciting endless runner game made with Unity and C#. In this game, players run through levels that are created on the spot. They can move between three lanes, jump over obstacles, and pick up power-ups to stay alive longer and score as many points as possible.

The game features smooth movement, different types of power-ups, smart enemy AI that follows and mimics the player's actions, and a system that generates new levels every time you play, ensuring that the game never gets boring.
## Features
* Infinite procedurally generated levels
* Multiple Power-Up Systems
  * Speed Boost - Temporarily increases running speed
  * Magnet - Attracts nearby coins automatically
* Intelligent Enemy Chaser
  * Matches player's lane position
  * Mimics player jumps
  * Adapts speed during player power-ups
  * Teleports when falling too far behind
* Score tracking and collection mechanics
* Smooth camera tracking
* Professional menu navigation
* Sound effects for collisions and coin collection
* Touch swipes and keyboard controls
## Technologies Used
* Unity Game Engine
* C# Programming Language
* Unity UI System
* Object-Oriented Programming (OOP)
## Gameplay Mechanics
### Player Abilities
- **Move Left/Right** - Switch between three lanes (positions: -2, 0, 2)
- **Jump** - Avoid obstacles and enemy attacks
- **Invincibility Frames** - Temporary protection during boost power-ups
- **Auto-Dodge** - Automatic obstacle avoidance during boost
### Enemy AI (Chaser)
- **Lane Matching** - Enemy follows player's lane position
- **Jump Synchronization** - Enemy jumps when player jumps
- **Boost Adaptation** - Enemy speeds up during player power-ups
- **Distance Management** - Teleports forward if falling too far behind
- **Collision Detection** - Stops player on contact
### Power-Up System

#### Speed Boost Power-Up
- Drastically increases movement speed
- Provides invincibility during boost
- Auto-dodges obstacles automatically
- Gradual acceleration and deceleration
- Travels fixed distance or duration

#### Magnet Power-Up
- Attracts all nearby coins
- Configurable radius and duration
- Smooth magnetic pull effect
- Automatic coin collection

### Level Generation
- Procedural section spawning
- Dynamic section pooling
- Optimized memory management
- Configurable section length
- Random section selection

## Control
| Input Method | Action |
|--------------|--------|
| A / Left Arrow | Move Left |
| D / Right Arrow | Move Right |
| W / Up Arrow / Space | Jump |
| Touch Swipe Left | Move Left (Mobile) |
| Touch Swipe Right | Move Right (Mobile) |
| Touch Swipe Up/Tap | Jump (Mobile) |

## Project Structure

```bash
JungleEndlessRun/
├── Scripts/
│ ├── Player/
│ │ ├── PlayerMove.cs
│ │ ├── PlayerBoost.cs
│ │ └── PlayerMagnet.cs
│ ├── PowerUps/
│ │ ├── BoostPowerup.cs
│ │ └── MagnetPowerup.cs
│ ├── Collectibles/
│ │ └── CollectCoin.cs
│ ├── Enemies/
│ │ └── EnemyChaser.cs
│ ├── Level/
│ │ └── GenerateLevel.cs
│ ├── Camera/
│ │ └── CameraFollow.cs
│ └── UI/
│ ├── MainMenu.cs
│ └── GameOver.cs
├── Prefabs/
│ ├── Sections/
│ ├── PowerUps/
│ ├── Obstacles/
│ └── Collectibles/
├── Audio/
│ ├── SoundEffects/
│ └── Music/
└── README.md
```

## Installation
1. Open the project using Unity (Version 2020.3 or later recommended).
2. For PC: File → Build Settings → PC, Mac & Linux Standalone → Build
3. For Mobile: File → Build Settings → Android/iOS → Build
## Screenshots
### Main Menu
<img width="285" height="515" alt="image" src="https://github.com/user-attachments/assets/bb4766c3-e18a-4462-bbba-d9ef57608869" />

### GamePlay
<img width="284" height="507" alt="image" src="https://github.com/user-attachments/assets/5eb154f2-1b54-4b12-94f3-26833dcbf1b4" />



## Authors
* LAM MING KANG
* WONG WEI LUN
* FATIN AQILAH BINTI MOHAMAD FADILAH
* MUHAMMAD SAIFFUDDIN BIN AHMAD FAUZI

## License
This project is developed for educational purposes.
