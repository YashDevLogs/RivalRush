# 🎮 Rival Rush

A fast-paced multiplayer competitive runner built in Unity, inspired by Fun Run.

Players race through obstacle-filled levels while using offensive and defensive powerups to sabotage opponents and secure first place. Every match is designed to be short, chaotic, and highly replayable.

---

## 🚀 Overview

Rival Rush is a production-focused multiplayer game project built with:

- Unity 6
- Netcode for GameObjects (NGO)
- Unity Transport (UTP)
- ScriptableObject-driven gameplay systems
- Event-driven architecture
- AI fallback players
- Object pooling for gameplay performance

The goal is to create a highly replayable competitive racing experience where movement skill, timing, and powerup usage determine the winner.

---

## 🎯 Core Features

### Multiplayer Racing

- 2–4 players per match
- Host-client networking architecture
- Lobby system with ready checks
- Scene synchronization handshake
- Race countdown synchronization
- Finish order tracking
- AI fills empty player slots

### Movement System

Players continuously auto-run forward and control:

- Jump
- Double Jump
- Slide
- Wall Cling
- Wall Jump
- Air Dive

Movement includes:

- Tick-based timing
- Jump buffering
- Coyote time
- Ground grace periods
- Physics-driven traversal

---

## ⚔️ Combat & Powerups

Combat is entirely powerup-driven.

Current powerups include:

| Powerup | Type | Description |
|----------|--------|------------|
| 🚀 Rocket | Offensive | Homing missile with AoE explosion |
| 🔫 Revolver | Offensive | Bullet rain attack |
| 🪚 Sawblade | Offensive | Bouncing projectile |
| ⚡ Shocker | Offensive | Area-of-effect electrical pulse |
| 🪤 Trap | Offensive | Ground trap placed behind player |
| 🛡 Shield | Defensive | Temporary invincibility |
| 💨 Speed Boost | Utility | Temporary speed increase |

Design goals:

- Instant readability
- Chaotic interactions
- Fair targeting rules
- Server-authoritative kill validation
- Shared kill feed across players

---

## 🤖 AI Opponents

Rival Rush includes AI players that can replace empty lobby slots.

### AI Personality Types

- Balanced
- Aggressive
- Defensive
- Risky

### AI Architecture

Perception → Context → Strategy → Action

AI uses:

- Hazard raycasts
- Wall detection
- Slide obstacle detection
- Powerup decision scoring
- Personality-driven behaviors

The AI shares the exact same input pipeline as human players through a common input interface.

---

## 🌐 Multiplayer Architecture

Networking is built using Unity Netcode for GameObjects.

### Authority Model

- Server-authoritative race state
- Server-authoritative finish order
- Server-authoritative combat validation
- Owner-driven player input
- NetworkVariables for replicated state
- RPCs for gameplay synchronization

### Networking Systems

- LobbyManager
- MultiplayerManager
- RaceManager
- NetworkPlayerSpawner
- TickManager

### Synchronization Features

- Scene-ready handshake system
- Countdown synchronization
- Finish state replication
- Lobby player replication
- Networked race lifecycle

---

## 🏗 Architecture

The project follows a hybrid architecture combining several patterns.

### Patterns Used

#### Observer Pattern

GameEvents act as the central event bus.

Examples:

- Race Started
- Race Finished
- Player Killed
- Powerup Picked
- Powerup Activated

---

#### MVC Separation

Player logic is split into:

- PlayerController
- PlayerModel
- PlayerView

This keeps gameplay logic separate from state and presentation.

---

#### Strategy Pattern

AI personalities are implemented using strategy classes:

- AggressiveStrategy
- DefensiveStrategy
- BalancedStrategy
- RiskyStrategy

---

#### Factory Pattern

Powerups are created using:

```csharp
PowerUpDefinition -> CreateEffect()
```

Each powerup defines its own effect implementation.

---

#### Object Pooling

No runtime Instantiate/Destroy during gameplay.

Pools include:

- ProjectilePool
- VFXPool
- KillFeed Pool
- Lobby UI Pool

---

## 📦 Gameplay Systems

### Race System

State Flow:

```text
Waiting
   ↓
Countdown
   ↓
Race
   ↓
Finished
```

Managed by:

- RaceManager
- TickManager

---

### Powerup System

Built using:

```text
PowerUpController
        ↓
PowerUpDefinition
        ↓
IPowerUpEffect
```

Benefits:

- Easy expansion
- Designer-friendly
- ScriptableObject workflow

---

### Audio System

Custom audio architecture includes:

### Sound Manager

Supports:

- Local sounds
- World-space sounds
- Attached looping sounds
- Audio pooling

Current SFX:

- Jump
- Landing
- Explosion
- Rocket Launch
- Rocket Loop
- Rocket Hit
- Gunshot
- Trap Place
- Trap Hit
- Sawblade Throw
- Sawblade Hit
- Bullet Hit
- Shield Pop
- Countdown
- Race Start
- Finish

### Music Manager

Supports:

- Persistent music playback
- Volume saving
- Runtime adjustment
- Settings menu integration

---

### VFX System

Pooled visual effects include:

- Death Smoke
- Shield Effects
- Explosion Effects

Features:

- Pre-warmed pools
- Automatic recycling
- Zero-allocation runtime usage

---

## 📊 Technical Highlights

### Performance Focus

- Object pooling throughout gameplay
- Cached AI raycasts
- Tick-based gameplay timing
- No gameplay coroutines in critical systems
- Minimal runtime allocations

### Scalability

Systems are designed to support:

- Additional powerups
- Additional AI personalities
- Multiple race maps
- Cosmetics
- Ranked progression
- Future matchmaking

---

## 🎮 Planned Features

### High Priority

- Client-side prediction
- Server reconciliation
- Lobby code sharing
- Matchmaking queue

### Medium Priority

- Cosmetic shop
- Character skins
- Trails
- Emotes
- Additional maps

### Low Priority

- Tournament mode
- Global leaderboards
- Mobile version

---

## 🧠 What I Learned

This project has been my primary sandbox for learning and applying:

- Multiplayer architecture
- Unity Netcode for GameObjects
- Network synchronization
- Server authority models
- AI architecture
- Design patterns
- Object pooling
- Production-level code organization
- Game system architecture

The goal was not simply to make a playable game, but to build systems that are scalable, maintainable, and representative of real-world game development practices.

---

## 📷 Screenshots
<img width="1912" height="993" alt="MainMenu" src="https://github.com/user-attachments/assets/10932cc9-ec3c-4a12-9fd6-8ba8fc47f323" />
<img width="1672" height="941" alt="COverImage" src="https://github.com/user-attachments/assets/9f67d5e4-3311-47b7-968a-05053ff730a2" />
<img width="1915" height="973" alt="LevelPrototype2" src="https://github.com/user-attachments/assets/db40ad47-7fcd-4e85-8d9b-1088996640fc" />
<img width="1875" height="956" alt="LevrlPrototype" src="https://github.com/user-attachments/assets/18b06b78-ea69-415b-a5fc-00aa2c938ce4" />
<img width="1897" height="966" alt="JoinLobbyPanel" src="https://github.com/user-attachments/assets/608601f9-f10e-4dde-97ac-343ceedf2679" />
<img width="1910" height="978" alt="LobbyPanel" src="https://github.com/user-attachments/assets/f2b9ef2e-ec71-44fd-bb44-a56efa2ed95a" />

---

## 🛠 Tech Stack

- Unity 6
- C#
- Netcode for GameObjects
- Unity Transport
- ScriptableObjects
- Unity Input System
- TextMeshPro

---

## 👨‍💻 Developer

**Yash Londhe**

Unity Game Developer

- Multiplayer Systems
- Gameplay Programming
- AI Systems
- Tools & Architecture

Portfolio:
https://13augyash.wixsite.com/gamedevportfolio

---

## ⭐ Project Status

Current Status:

**Prototype / Vertical Slice**

Core gameplay, multiplayer, AI, powerups, audio systems, VFX systems, and race flow are implemented and actively being expanded.
