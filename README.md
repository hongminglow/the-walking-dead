# 🧟 The Walking Dead — Indie Survival Horror Game

> A 3rd-person survival horror game inspired by **Resident Evil 4** and **The Last of Us**, built with Unity 6.
> Navigate a zombie-infested small town, scavenge for supplies, solve puzzles, and fight your way to escape.

<br>

## 📖 Game Overview

**Setting**: Millbrook — a small rural American town, population ~2,000.

**Story**: You play as **Alex Chen**, a field paramedic passing through Millbrook when a sudden, unexplained outbreak turns the townspeople into aggressive undead. You wake up in an abandoned house — no memory of the last 12 hours, a dead phone, and a locked front door. Escape Millbrook alive.

**Genre**: 3rd-Person Survival Horror
**Platform**: PC (Windows)
**Rating Target**: Mature (M) — violence, horror themes
**Camera**: Over-the-shoulder (Resident Evil 4–style)
**Tone**: Tense, atmospheric, resource-scarce

<br>

## 🗺️ Game Flow — 5 Acts

```
 MAIN MENU
     │
     ▼
 LEVEL 1: ABANDONED HOUSE  ──── Tutorial area, learn controls, find first key
     │
     ▼
 LEVEL 2: TOWN STREETS  ──────── First combat, find Hospital Keycard
     │
     ▼
 LEVEL 3: HOSPITAL  ─────────── Puzzle-heavy, find Sewer Map, get Shotgun
     │
     ▼
 LEVEL 4: UNDERGROUND SEWERS ── Survival gauntlet, heavy zombie density
     │
     ▼
 LEVEL 5: FINALE  ───────────── Boss fight (Zombie Brute) → escape cutscene
```

<br>

## 🎮 Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left Stick |
| Sprint | Left Shift (hold) | L3 |
| Crouch | C (toggle) | B |
| Interact | E | X / Square |
| Aim | Right Click (hold) | LT |
| Shoot | Left Click | RT |
| Reload | R | X (while aiming) |
| Melee | V | RB |
| Inventory | Tab / I | Select |
| Pause | Escape | Start |

**Movement Values**: Walk `3.5 m/s` · Sprint `5.5 m/s` · Crouch `1.8 m/s`

<br>

## ⚙️ Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| **Unity** | 6000.4.0f1 | Game engine |
| **Universal Render Pipeline (URP)** | 17.4.0 | Rendering |
| **C#** | .NET Standard 2.1 | Game logic |
| **New Input System** | 1.19.0 | Keyboard, mouse & gamepad |
| **AI Navigation** | 2.0.11 | Zombie pathfinding (NavMesh) |
| **Cinemachine** | 3.1.6 | Over-the-shoulder camera |
| **ProBuilder** | 6.0.9 | Level greyboxing |
| **TextMeshPro** | 3.2.0 | UI text rendering |
| **Timeline** | 1.8.11 | Cutscenes |

<br>

## 🏗️ Architecture

The project uses a clean, decoupled architecture based on three pillars:

### 1. Singleton Managers (always alive, DontDestroyOnLoad)
```
[Managers] GameObject
├── GameManager      — game state machine (Menu / Playing / Paused / Dead / Loading)
├── AudioManager     — 5-channel audio (music, SFX, ambience, voice, UI)
├── SceneLoader      — async scene transitions with loading screen
├── SaveManager      — 3-slot JSON save system with XOR encryption
└── InventoryManager — 24-slot grid inventory (4×6)
```

### 2. EventBus — Decoupled Communication
All systems talk to each other through a **static event bus** so nothing has hard references:
```csharp
// Example: Enemy killed → HUD updates, SaveManager records it
EventBus.EnemyKilled("zombie_basic_42");

// Subscribe anywhere without caring who fires it
EventBus.OnEnemyKilled += (id) => { killedEnemies.Add(id); };
```

### 3. ScriptableObjects — Data-Driven Design
All game stats live in `.asset` files — no hardcoded numbers in scripts:
```
ScriptableObjects/
├── Weapons/   SO_WeaponKnife, SO_WeaponPistol, SO_WeaponShotgun, SO_WeaponWrench
├── Enemies/   SO_Zombie_Basic, SO_Zombie_Crawler, SO_Zombie_Brute
└── Items/     SO_HealthPack, SO_Ammo_Pistol, SO_Ammo_Shotgun, SO_Key_House, SO_Key_Hospital
```

<br>

## 🧟 Enemies

| Enemy | HP | Damage | Speed | Special |
|-------|----|--------|-------|---------|
| Zombie Basic | 80 | 15/hit | 2.0 m/s | Standard chase & swipe |
| Zombie Crawler | 40 | 10/hit | 1.5 m/s | Low-profile, ankle grab |
| Zombie Brute | 300 | 40/hit | 1.2 m/s | Charge attack, AoE smash, mini-boss |

**AI State Machine**: `Idle → Wander → Chase → Attack → Search → Stagger → Dead`
- Detection based on **sight** (field of view cone) and **sound** (gunshots alert nearby zombies)
- Headshots deal **2.5× damage**

<br>

## 🔫 Weapons

| Weapon | Damage | Fire Rate | Range | Ammo |
|--------|--------|-----------|-------|------|
| Kitchen Knife | 15 | 0.8s | 1.5m | Unlimited |
| Pistol (9mm) | 25 | 0.4s | 30m | Pistol Ammo (scarce) |
| Shotgun | 60 | 1.2s | 10m | Shotgun Shells (very scarce) |
| Pipe Wrench | 35 | 1.0s | 2.0m | Unlimited |

> **Design philosophy**: Ammo is **intentionally scarce**. Average 6–10 pistol rounds per area. Melee is viable but risky — enemies can grab during swing recovery.

<br>

## 💾 Save System

Progress is saved to `AppData/LocalLow/.../saves/slot_N.json` (XOR encrypted).

**What gets saved:**
- Player position, health, stamina, current scene
- Full inventory (items, equipped weapon, ammo counts)
- World state: unlocked doors, collected items, killed enemies, solved puzzles
- Play time and save timestamp

**Save triggers:**
- Manual save via Pause Menu (3 slots)  
- Auto-save at level transitions (slot 0)
- Auto-save at safe rooms

<br>

## 📁 Project Structure

```
Assets/
└── _Project/
    ├── Animations/       Player, Enemies, Environment
    ├── Art/              Models, Textures, Materials, Shaders
    ├── Audio/            Music (Ambient/Combat/Cutscene), SFX, Voice
    ├── Input/            PlayerInputActions.inputactions
    ├── Prefabs/          Characters, Weapons, Items, Environment, VFX, UI
    ├── Scenes/           MainMenu, Loading, Level_01–05, _TestScene
    ├── ScriptableObjects/ Weapons, Enemies, Items data assets
    └── Scripts/
        ├── Core/         GameManager, EventBus, AudioManager, SaveManager, SceneLoader
        ├── Player/       Controller, Health, Combat, Animator, Interaction
        ├── Camera/       ThirdPersonCamera
        ├── Enemies/      EnemyBase (FSM), ZombieBasic/Crawler/Brute, EnemySpawner
        ├── Combat/       DamageSystem, WeaponData, IDamageable
        ├── Inventory/    InventoryManager, InventorySlot, ItemPickup, ItemData
        ├── Environment/  Door, Destructible, Trap, IInteractable
        ├── Puzzle/       PuzzleBase, KeyLockPuzzle, SequencePuzzle
        ├── UI/           HUDController, PauseMenu, MainMenuUI
        └── Utilities/    Singleton<T>, ObjectPool<T>, Constants, Enums, Extensions
```

<br>

## 🗂️ C# Coding Standards

```csharp
namespace TWD.Player          // Namespace: TWD.<Subsystem>
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 100f;  // _camelCase private fields
        public float CurrentHealth { get; private set; }  // PascalCase properties
        
        private void Awake() { ... }
        
        public void TakeDamage(float amount, bool isHeadshot = false)  // PascalCase methods
        {
            // Use EventBus for cross-system communication — no direct references
            EventBus.PlayerDamaged(amount);
        }
    }
}
```

- **Namespace**: `TWD.*` on every script
- **Private fields**: `_camelCase` with `[SerializeField]`
- **No magic numbers** — all constants in `Constants.cs` and `Enums.cs`
- **Interfaces**: `IDamageable`, `IInteractable` for polymorphism

<br>

## 🚀 Development Roadmap

| Stage | Milestone | Status |
|-------|-----------|--------|
| Stage 0 | 40 core C# scripts (all systems) | ✅ Complete |
| Stage 1 | Unity Editor Setup (tags, layers, packages, scenes) | 🚧 In Progress |
| Stage 2 | Phase 1 Prototype — walk, pick up key, unlock door | 🔲 Next |
| Stage 3 | Phase 2 Combat — fight and kill zombies | 🔲 |
| Stage 4 | Phase 3 Systems — inventory UI, save/load | 🔲 |
| Stage 5 | Phase 4 Content — build all 5 levels | 🔲 |
| Stage 6 | Phase 5 Polish — art, audio, VFX | 🔲 |
| Stage 7 | Phase 6 Testing & Ship | 🔲 |

> See [DEVELOPMENT_CHECKLIST.md](./DEVELOPMENT_CHECKLIST.md) for the full stage-by-stage task list with progress tracking.

<br>

## 🔀 Git Workflow

```bash
# Daily commands
git status                              # see what changed
git add -A                              # stage all changes
git commit -m "feat: add zombie prefab" # commit with conventional message
git push origin main                    # push to GitHub
```

**Commit types**: `feat:` `fix:` `asset:` `scene:` `config:` `docs:` `refactor:`

**Large assets** (FBX, PNG, WAV, MP3) are tracked with **Git LFS** — already configured.

<br>

## 🛠️ Getting Started (For Developers)

```bash
# 1. Clone the repository
git clone https://github.com/hongminglow/the-walking-dead.git
cd the-walking-dead

# 2. Open in Unity Hub → Add project from disk
#    Select: d:\Game Project\The Walking Dead
#    Unity version: 6000.4.0f1

# 3. Unity will auto-import packages from manifest.json
#    (Cinemachine, ProBuilder, TextMeshPro, Input System, AI Navigation)

# 4. When TextMeshPro imports → click "Import TMP Essential Resources"

# 5. Open _TestScene to start testing
#    Assets/_Project/Scenes/_TestScene.unity
```

**Required Unity Setup** (one-time, automated via ProjectSettings):
- Tags: `Enemy`, `Interactable`, `ItemPickup`, `Door`, `Puzzle`, `SavePoint`, `DamageZone`, `Headshot`
- Layers: `Player(6)`, `Enemy(7)`, `Interactable(8)`, `Ground(9)`, `Obstacle(10)`, `Projectile(11)`

<br>

## 📦 Free Resources Used

| Resource | What For |
|----------|----------|
| [Mixamo](https://www.mixamo.com) | Character models + animations (FBX) |
| [Freesound](https://freesound.org) | SFX — footsteps, groans, gunshots |
| [ZapSplat](https://www.zapsplat.com) | Horror ambience and atmospheric audio |
| [Kenney](https://kenney.nl) | Prototype art and UI assets |

<br>

---

*Built with Unity 6 · URP · C# · New Input System · NavMesh AI*
