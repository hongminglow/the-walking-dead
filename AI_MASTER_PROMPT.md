# 🧟 THE WALKING DEAD — AI MASTER PROMPT

> **PURPOSE**: Attach this document to every AI conversation during development.
> It gives the AI full context about the project so it can generate accurate,
> consistent, and production-ready code, assets, and design decisions.

---

## TABLE OF CONTENTS

1. [Project Identity](#1-project-identity)
2. [Technology Stack](#2-technology-stack)
3. [Folder Structure](#3-folder-structure)
4. [Game Design Overview](#4-game-design-overview)
5. [Game Flow & Scene Map](#5-game-flow--scene-map)
6. [Core Mechanics](#6-core-mechanics)
7. [Enemy & AI Design](#7-enemy--ai-design)
8. [Inventory & Item System](#8-inventory--item-system)
9. [Asset Specifications](#9-asset-specifications)
10. [C# Coding Standards](#10-c-coding-standards)
11. [Architecture & Patterns](#11-architecture--patterns)
12. [UI/UX Guidelines](#12-uiux-guidelines)
13. [Audio Design](#13-audio-design)
14. [Lighting & Post-Processing](#14-lighting--post-processing)
15. [Performance Budgets](#15-performance-budgets)
16. [Save/Load System](#16-saveload-system)
17. [Development Phases](#17-development-phases)
18. [AI Prompt Templates](#18-ai-prompt-templates)
19. [Agent Git Workflow](#19-agent-git-workflow)

---

## 1. PROJECT IDENTITY

| Field              | Value                                          |
|--------------------|------------------------------------------------|
| **Project Name**   | The Walking Dead                               |
| **Genre**          | 3rd-Person Survival Horror (Resident Evil-like) |
| **Engine**         | Unity 6 (6000.4.0f1)                           |
| **Render Pipeline**| Universal Render Pipeline (URP 17.4.0)         |
| **Platform**       | PC (Windows) — primary target                  |
| **Art Style**      | Semi-realistic, dark & gritty                  |
| **Camera**         | Over-the-shoulder 3rd person (RE4-style)       |
| **Tone**           | Tense, atmospheric, resource-scarce            |
| **Rating Target**  | Mature (M) — violence, horror themes           |
| **Solo Dev?**      | Yes — scope everything for a solo beginner     |

### Elevator Pitch
> A lone survivor navigates through a zombie-infested small town after a
> mysterious outbreak. With limited ammo, scarce healing items, and locked
> doors requiring keys/puzzles, the player must scavenge, fight, and solve
> their way to escape. Think **Resident Evil 4** meets **The Last of Us**,
> but scoped down for a solo indie developer.

---

## 19. AGENT GIT WORKFLOW

> **RULE FOR ALL AI AGENTS WORKING ON THIS PROJECT**
>
> Before any `git commit` or `git push`, the agent must:
>
> 1. **Auto-generate a suggested commit message** based on the actual work completed.
> 2. **Show that suggested commit message to the user clearly**.
> 3. **Ask for permission first** before running `git commit`.
> 4. **Ask for permission first** before running `git push`, even if commit permission was already granted.
>
> Additional rules:
> - Do **not** commit silently.
> - Do **not** push silently.
> - Prefer concise Conventional Commit style messages such as:
>   - `fix: restore Level 03 hospital navmesh and fuse-door progression`
>   - `scene: retune Level 02 streets spawn and lighting`
>   - `docs: update AI master prompt with git approval workflow`
> - If multiple unrelated changes were made, the agent should suggest either:
>   - one combined commit message, or
>   - a short list of recommended commit messages grouped by change area.
> - If the user says no, the agent must stop before committing or pushing.

---

## 2. TECHNOLOGY STACK

### Engine & Packages (Already Installed)

| Package                       | Version   | Purpose                        |
|-------------------------------|-----------|--------------------------------|
| Unity 6                       | 6000.4.0f1| Game engine                    |
| URP                           | 17.4.0    | Rendering pipeline             |
| Input System                  | 1.19.0    | Player input (gamepad + K&M)   |
| AI Navigation                 | 2.0.11    | Enemy pathfinding (NavMesh)    |
| Timeline                      | 1.8.11    | Cutscenes & scripted sequences |
| Visual Scripting              | 1.9.10    | Prototyping (optional)         |
| uGUI                          | 2.0.0     | UI system                      |

### Packages to Add Later (When Needed)

| Package                       | Purpose                                |
|-------------------------------|----------------------------------------|
| Cinemachine                   | Camera system (over-the-shoulder cam)  |
| ProBuilder                    | Level greyboxing & prototyping         |
| TextMeshPro                   | Better UI text rendering               |
| Unity Addressables            | Async asset loading (optimization)     |
| DOTween (Asset Store)         | UI animations & tweening               |

### Language & Runtime
- **Language**: C# (.NET Standard 2.1 / .NET 6+ managed by Unity 6)
- **IDE**: Visual Studio / Rider (already configured)

---

## 3. FOLDER STRUCTURE

> **RULE**: Always follow this folder structure. When generating scripts or
> assets, place them in the correct folder. Ask if unsure.

```
Assets/
├── _Project/                     # ← ALL game-specific content lives here
│   ├── Animations/
│   │   ├── Player/
│   │   │   ├── Locomotion/       # Walk, Run, Idle, Crouch
│   │   │   ├── Combat/           # Aim, Shoot, Melee, Reload
│   │   │   ├── Interaction/      # PickUp, OpenDoor, PushObject
│   │   │   └── AnimatorControllers/
│   │   ├── Enemies/
│   │   │   ├── Zombie_Basic/
│   │   │   ├── Zombie_Crawler/
│   │   │   └── Zombie_Brute/
│   │   └── Environment/          # Door open/close, traps, etc.
│   │
│   ├── Art/
│   │   ├── Models/
│   │   │   ├── Characters/
│   │   │   │   ├── Player/
│   │   │   │   └── Enemies/
│   │   │   ├── Weapons/
│   │   │   ├── Props/            # Tables, chairs, barricades
│   │   │   └── Environment/      # Buildings, walls, doors
│   │   ├── Textures/
│   │   │   ├── Characters/
│   │   │   ├── Environment/
│   │   │   ├── UI/
│   │   │   └── Effects/
│   │   ├── Materials/
│   │   │   ├── Characters/
│   │   │   ├── Environment/
│   │   │   └── Effects/
│   │   └── Shaders/              # Custom URP shaders if needed
│   │
│   ├── Audio/
│   │   ├── Music/
│   │   │   ├── Ambient/          # Eerie drones, wind, silence
│   │   │   ├── Combat/           # Tension music during fights
│   │   │   └── Cutscene/
│   │   ├── SFX/
│   │   │   ├── Player/           # Footsteps, breathing, heartbeat
│   │   │   ├── Weapons/          # Gunshots, reload, melee impact
│   │   │   ├── Enemies/          # Groans, screams, attack sounds
│   │   │   ├── Environment/      # Door creaks, glass breaking
│   │   │   └── UI/               # Button clicks, inventory sounds
│   │   └── Voice/                # Dialogue lines (if any)
│   │
│   ├── Prefabs/
│   │   ├── Characters/
│   │   │   ├── Player.prefab
│   │   │   └── Enemies/
│   │   ├── Weapons/
│   │   ├── Items/                # Pickupable items
│   │   ├── UI/                   # UI element prefabs
│   │   ├── Environment/          # Doors, barricades, traps
│   │   └── VFX/                  # Muzzle flash, blood splatter
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Loading.unity
│   │   ├── Level_01_House.unity        # Starting location
│   │   ├── Level_02_Streets.unity      # Town streets
│   │   ├── Level_03_Hospital.unity     # Mid-game area
│   │   ├── Level_04_Underground.unity  # Sewer / tunnels
│   │   ├── Level_05_Finale.unity       # Final escape
│   │   └── _TestScene.unity            # Dev sandbox
│   │
│   ├── Scripts/
│   │   ├── Core/                 # Singletons, managers, bootstrapping
│   │   │   ├── GameManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   ├── SceneLoader.cs
│   │   │   ├── SaveManager.cs
│   │   │   └── EventBus.cs       # Global event system
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerHealth.cs
│   │   │   ├── PlayerCombat.cs
│   │   │   ├── PlayerInteraction.cs
│   │   │   ├── PlayerAnimator.cs
│   │   │   └── PlayerStateMachine.cs
│   │   ├── Camera/
│   │   │   ├── ThirdPersonCamera.cs
│   │   │   └── CameraShake.cs
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs      # Abstract base class
│   │   │   ├── ZombieBasic.cs
│   │   │   ├── ZombieCrawler.cs
│   │   │   ├── ZombieBrute.cs
│   │   │   ├── EnemyStateMachine.cs
│   │   │   ├── EnemyHealth.cs
│   │   │   └── EnemySpawner.cs
│   │   ├── Combat/
│   │   │   ├── WeaponBase.cs     # Abstract base class
│   │   │   ├── Pistol.cs
│   │   │   ├── Shotgun.cs
│   │   │   ├── MeleeWeapon.cs
│   │   │   ├── DamageSystem.cs
│   │   │   └── Projectile.cs
│   │   ├── Inventory/
│   │   │   ├── InventoryManager.cs
│   │   │   ├── InventorySlot.cs
│   │   │   ├── ItemBase.cs       # ScriptableObject
│   │   │   └── ItemPickup.cs
│   │   ├── Puzzle/
│   │   │   ├── PuzzleBase.cs
│   │   │   ├── KeyLockPuzzle.cs
│   │   │   ├── SequencePuzzle.cs
│   │   │   └── PushBlockPuzzle.cs
│   │   ├── Environment/
│   │   │   ├── Door.cs
│   │   │   ├── Trap.cs
│   │   │   ├── Destructible.cs
│   │   │   └── InteractableBase.cs
│   │   ├── UI/
│   │   │   ├── HUDController.cs
│   │   │   ├── PauseMenu.cs
│   │   │   ├── InventoryUI.cs
│   │   │   ├── DialogueUI.cs
│   │   │   └── MainMenuUI.cs
│   │   └── Utilities/
│   │       ├── Singleton.cs      # Generic singleton base
│   │       ├── ObjectPool.cs
│   │       ├── Extensions.cs     # Extension methods
│   │       └── Constants.cs      # Global constants & tags
│   │
│   ├── ScriptableObjects/
│   │   ├── Items/
│   │   │   ├── SO_Pistol.asset
│   │   │   ├── SO_Shotgun.asset
│   │   │   ├── SO_HealthPack.asset
│   │   │   ├── SO_Ammo_Pistol.asset
│   │   │   └── SO_Key_Hospital.asset
│   │   ├── Enemies/
│   │   │   ├── SO_Zombie_Basic.asset
│   │   │   ├── SO_Zombie_Crawler.asset
│   │   │   └── SO_Zombie_Brute.asset
│   │   └── Weapons/
│   │       ├── SO_WeaponPistol.asset
│   │       └── SO_WeaponShotgun.asset
│   │
│   ├── Input/
│   │   └── PlayerInputActions.inputactions
│   │
│   ├── Settings/                 # URP settings, quality profiles
│   │   ├── URP_HighQuality.asset
│   │   ├── URP_LowQuality.asset
│   │   └── PostProcessing/
│   │
│   └── Timeline/                 # Cutscene timelines
│       ├── Intro_Cutscene.playable
│       └── Ending_Cutscene.playable
│
├── Plugins/                      # 3rd-party plugins (DOTween, etc.)
├── StreamingAssets/               # Runtime-loaded files
│
├── Scenes/                       # (default — move to _Project/Scenes/)
└── Settings/                     # (default — move to _Project/Settings/)
```

### Folder Rules
1. **Never** put game content directly in `Assets/` root
2. **Always** prefix the main project folder with `_` so it sorts to the top
3. Group by **feature** first, then by **type** within feature scripts
4. Keep **ScriptableObject definitions** (C# classes) in `Scripts/` and **instances** (.asset files) in `ScriptableObjects/`
5. **Prefabs** should be self-contained — drag into scene and they work

---

## 4. GAME DESIGN OVERVIEW

### Story Synopsis

**Setting**: Millbrook — a small rural American town, population ~2,000.

**Premise**: The player is **Alex Chen**, a field paramedic who was passing
through Millbrook when a sudden, unexplained outbreak turned most of the
townspeople into aggressive undead. Alex wakes up in an abandoned house with
no memory of the last 12 hours, a dead phone, and a locked front door.

**Goal**: Escape Millbrook alive by navigating through 5 connected areas,
solving puzzles to unlock paths forward, scavenging for supplies, and
surviving increasingly dangerous zombie encounters.

### Story Beats (5-Act Structure)

| Act | Location        | Story Beat                                        | Key Mechanic Introduced |
|-----|-----------------|---------------------------------------------------|-------------------------|
| 1   | Abandoned House | Wake up, learn controls, find first key, escape   | Movement, Interaction   |
| 2   | Town Streets    | Navigate to hospital, first real combat encounters | Combat, Inventory       |
| 3   | Hospital        | Search for radio to call for help, puzzle-heavy    | Puzzles, New Weapon     |
| 4   | Underground     | Sewers/tunnels shortcut, heavy zombie density     | Stealth, Resource Mgmt  |
| 5   | Town Exit       | Final escape, mini-boss encounter                 | Boss Fight, Climax      |

### Characters

| Character     | Role           | Description                                    |
|---------------|----------------|------------------------------------------------|
| Alex Chen     | Protagonist    | 28, paramedic, resourceful, no combat training |
| Radio Voice   | Guide (NPC)    | Mysterious voice on walkie-talkie, gives hints  |
| The Brute     | Mini-Boss      | Massive mutated zombie, final obstacle          |

---

## 5. GAME FLOW & SCENE MAP

```
┌──────────────┐
│  MAIN MENU   │
│  - New Game  │
│  - Continue  │
│  - Settings  │
│  - Quit      │
└──────┬───────┘
       │
       ▼
┌──────────────┐    ┌───────────────────┐
│   LOADING    │───▶│  LEVEL 1: HOUSE   │
│   SCREEN     │    │  (Tutorial Area)  │
└──────────────┘    └────────┬──────────┘
                             │ Find House Key
                             ▼
                    ┌───────────────────┐
                    │ LEVEL 2: STREETS  │
                    │ (Open Area)       │
                    └────────┬──────────┘
                             │ Find Hospital Keycard
                             ▼
                    ┌───────────────────┐
                    │ LEVEL 3: HOSPITAL │
                    │ (Puzzle Heavy)    │
                    └────────┬──────────┘
                             │ Find Sewer Map
                             ▼
                    ┌───────────────────┐
                    │ LEVEL 4: SEWERS   │
                    │ (Survival Gauntlet│
                    └────────┬──────────┘
                             │ Reach town exit
                             ▼
                    ┌───────────────────┐
                    │ LEVEL 5: FINALE   │
                    │ (Boss + Escape)   │
                    └────────┬──────────┘
                             │
                             ▼
                    ┌───────────────────┐
                    │   ENDING SCENE    │
                    │   (Cutscene)      │
                    └───────────────────┘

ACCESSIBLE ANYTIME:
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  PAUSE MENU  │  │  INVENTORY   │  │   MAP VIEW   │
│  - Resume    │  │  - Grid View │  │  (Optional)  │
│  - Settings  │  │  - Combine   │  │              │
│  - Save      │  │  - Use/Drop  │  │              │
│  - Load      │  │              │  │              │
│  - Quit      │  └──────────────┘  └──────────────┘
└──────────────┘
```

### Scene Transition Rules
- **Loading Screen** between every level transition
- Save checkpoints at **level entrances** and **before boss**
- Player **cannot backtrack** to previous levels (keeps scope small)
- Each level is a **separate Unity scene** for memory management

---

## 6. CORE MECHANICS

### 6.1 Player Movement (PlayerController.cs)

| Action    | Input (Keyboard) | Input (Gamepad) | Details                       |
|-----------|-------------------|-----------------|-------------------------------|
| Move      | WASD              | Left Stick      | Relative to camera forward    |
| Sprint    | Left Shift (hold) | L3 (click)      | 1.5x speed, drains stamina    |
| Crouch    | C (toggle)        | B               | Slower, quieter, smaller hitbox|
| Interact  | E                 | X / Square      | Context-sensitive              |
| Aim       | Right Click (hold)| LT (hold)       | Over-shoulder zoom            |
| Shoot     | Left Click        | RT              | Only while aiming             |
| Reload    | R                 | X (while aiming)| Manual reload                 |
| Melee     | V                 | RB              | Emergency close-range attack  |
| Inventory | Tab / I           | Select          | Opens inventory overlay       |
| Pause     | Escape            | Start           | Opens pause menu              |

**Movement Specs**:
- Walk speed: `3.5 m/s`
- Sprint speed: `5.5 m/s`
- Crouch speed: `1.8 m/s`
- Rotation speed: `10` (smooth rotation toward movement direction)
- Stamina: `100` points, drains `15/sec` while sprinting, regens `8/sec`

### 6.2 Health System (PlayerHealth.cs)

| Stat       | Value | Notes                                    |
|------------|-------|------------------------------------------|
| Max Health | 100   | Displayed as health bar on HUD           |
| Fine       | 71-100| Normal screen, normal speed              |
| Hurt       | 31-70 | Screen edges tint red, slight limp       |
| Critical   | 1-30  | Heavy vignette, heartbeat SFX, slow move |
| Dead       | 0     | Death screen → reload checkpoint         |

**Visual Feedback**:
- Screen **vignette** increases as health decreases
- **Heartbeat** audio plays below 30 HP
- **Blood splatter** on screen when taking damage (fades after 2 sec)

### 6.3 Combat System (PlayerCombat.cs)

| Weapon        | Damage | Fire Rate   | Ammo Type     | Range  | Notes                  |
|---------------|--------|-------------|---------------|--------|------------------------|
| Kitchen Knife | 15     | 0.8s swing  | Unlimited     | 1.5m   | Starting weapon        |
| Pistol (9mm)  | 25     | 0.4s        | Pistol Ammo   | 30m    | Found in Level 1       |
| Shotgun       | 60     | 1.2s        | Shotgun Shells| 10m    | Found in Level 3       |
| Pipe Wrench   | 35     | 1.0s swing  | Unlimited     | 2.0m   | Found in Level 2       |

**Combat Rules**:
- Headshots deal **2.5x** damage
- Player **cannot move while aiming** (tank-like tension, RE-style)
- **Ammo is scarce** — average 6-10 pistol rounds per area
- Melee is **viable but risky** — enemies can grab during swing recovery
- **No auto-aim** — manual aiming with subtle crosshair magnetism

### 6.4 Interaction System (PlayerInteraction.cs)

```
Raycast from camera center → distance 3m → LayerMask "Interactable"
    ├── HIT → Show UI prompt "[E] Examine" / "[E] Pick Up" / "[E] Open"
    │         ├── ItemPickup   → Add to inventory
    │         ├── Door         → Check if unlocked/has key → open/locked msg
    │         ├── Puzzle       → Enter puzzle mode
    │         ├── Note/File    → Show readable document UI
    │         └── Switch       → Toggle state
    └── MISS → Hide UI prompt
```

---

## 7. ENEMY & AI DESIGN

### Enemy Types

| Enemy          | Health | Damage | Speed   | Behavior                                    |
|----------------|--------|--------|---------|---------------------------------------------|
| Zombie Basic   | 80     | 15/hit | 2.0 m/s | Wanders → detect player → chase → attack    |
| Zombie Crawler | 40     | 10/hit | 1.5 m/s | Low-profile, harder to spot, grabs ankles   |
| Zombie Brute   | 300    | 40/hit | 1.2 m/s | Slow but tanky, charge attack, mini-boss    |

### AI State Machine (EnemyStateMachine.cs)

```
┌──────────┐  detect player   ┌──────────┐
│   IDLE   │ ───────────────▶ │  CHASE   │
│ (Wander) │                  │          │
└────┬─────┘                  └────┬─────┘
     │                             │ reach attack range
     │  lose player                ▼
     │  (after 8s)           ┌──────────┐
     ◀──────────────────────│  ATTACK  │
                             └────┬─────┘
                                  │ player dies / flees
                                  ▼
                             ┌──────────┐
                             │  SEARCH  │ → return to IDLE after 5s
                             └──────────┘
```

### AI Specs
- **Detection**: Sight cone (60°, 15m range) + Noise radius (sound triggers within 8m)
- **Pathfinding**: Unity NavMesh (AI Navigation package)
- **Attack Cooldown**: 1.5s between attacks
- **Stagger**: Enemies stagger after taking 30+ damage in 1 hit (shotgun always staggers)
- **Death**: Ragdoll physics on death (keep corpse for 30s, then fade & destroy)

### Spawn Rules
- Max **8 active enemies** per scene (performance)
- Use **EnemySpawner.cs** with trigger zones
- Spawners **disable** after their wave is cleared (no infinite respawns)
- Some enemies are **pre-placed** in scenes for scripted scares

---

## 8. INVENTORY & ITEM SYSTEM

### Inventory Specs
- **Grid-based** inventory (RE4-style), **4 columns × 6 rows = 24 slots**
- Each item occupies **1 slot** (keep it simple)
- Items can be: **Used, Examined, Dropped, Combined**

### Item Types (ScriptableObject-based)

```csharp
// ItemBase.cs — ScriptableObject definition
[CreateAssetMenu(fileName = "New Item", menuName = "TWD/Item")]
public class ItemBase : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;      // Weapon, Ammo, Healing, Key, Document
    public bool isStackable;
    public int maxStack;
    public GameObject worldPrefab; // 3D model for pickup in world
}
```

### Key Items Per Level

| Level         | Key Items Found                                           |
|---------------|-----------------------------------------------------------|
| 1 - House     | House Key, Pistol, Pistol Ammo x12, Health Pack x1        |
| 2 - Streets   | Hospital Keycard, Pipe Wrench, Ammo x8, Health Pack x2    |
| 3 - Hospital  | Shotgun, Sewer Map, Fuse (puzzle), Shells x6, Pack x2     |
| 4 - Sewers    | Gate Valve Handle, Ammo x15, Shells x4, Pack x3           |
| 5 - Finale    | Ammo x10, Shells x6, Health Pack x2                       |

---

## 9. ASSET SPECIFICATIONS

### 3D Models — Polygon Budget

| Asset Category    | Target Poly Count | LOD Levels | Notes                       |
|-------------------|--------------------|------------|-----------------------------|
| Player Character  | 8,000 - 12,000     | 2          | Most detailed model in game |
| Zombie Basic      | 4,000 - 6,000      | 2          | Shared base mesh, variants  |
| Zombie Crawler    | 3,000 - 5,000      | 2          | Modified basic mesh         |
| Zombie Brute      | 6,000 - 8,000      | 2          | Unique bulkier mesh         |
| Weapons           | 1,500 - 3,000      | 1          | First person not needed     |
| Props (small)     | 200 - 800          | 0          | Chairs, bottles, boxes      |
| Props (large)     | 1,000 - 3,000      | 1          | Tables, cars, dumpsters     |
| Buildings         | 3,000 - 8,000      | 2          | Modular pieces preferred    |

### Textures

| Type               | Resolution | Format | Notes                          |
|--------------------|------------|--------|--------------------------------|
| Character Albedo   | 2048×2048  | PNG    | Diffuse color map              |
| Character Normal   | 2048×2048  | PNG    | Normal map for detail          |
| Environment Albedo | 1024×1024  | PNG    | Tiling textures                |
| Environment Normal | 1024×1024  | PNG    |                                |
| Props              | 512×512    | PNG    | Small objects                  |
| UI Icons           | 128×128    | PNG    | Inventory item icons           |
| UI Backgrounds     | 1920×1080  | PNG    | Full-screen menu backgrounds   |

### Animations Required

| Category     | Animations Needed                                                  |
|--------------|--------------------------------------------------------------------|
| Player       | Idle, Walk, Run, Crouch_Idle, Crouch_Walk, Aim_Idle, Aim_Walk      |
|              | Shoot_Pistol, Shoot_Shotgun, Melee_Knife, Melee_Wrench, Reload    |
|              | TakeDamage, Death, PickUp, OpenDoor, PushObject, Stagger           |
| Zombie Basic | Idle, Walk_Shamble, Run_Charge, Attack_Swipe, Attack_Grab          |
|              | TakeHit, Stagger, Death_Forward, Death_Backward, Crawl_Transition |
| Zombie Crawl | Crawl_Idle, Crawl_Move, Crawl_Attack, Death                       |
| Zombie Brute | Idle, Walk_Heavy, Charge, Attack_Smash, Roar, TakeHit, Death      |

---

## 10. C# CODING STANDARDS

### Naming Conventions

| Element            | Convention          | Example                          |
|--------------------|---------------------|----------------------------------|
| Namespace          | PascalCase          | `namespace TWD.Player`           |
| Class              | PascalCase          | `public class PlayerController`  |
| Interface          | I + PascalCase      | `public interface IDamageable`   |
| Method             | PascalCase          | `public void TakeDamage()`       |
| Public Field       | camelCase           | `public float moveSpeed`         |
| Private Field      | _camelCase          | `private float _currentHealth`   |
| Serialized Private | _camelCase + [SF]   | `[SerializeField] private float _maxHealth` |
| Constant           | UPPER_SNAKE_CASE    | `public const int MAX_AMMO = 30` |
| Property           | PascalCase          | `public float Health { get; }`   |
| Enum               | PascalCase          | `public enum WeaponType`         |
| Enum Values        | PascalCase          | `WeaponType.Pistol`              |
| ScriptableObject   | SO_ prefix (asset)  | `SO_Pistol.asset`                |
| Event              | On + PascalCase     | `public event Action OnDeath`    |

### Code Rules

```csharp
// ✅ DO — Use [SerializeField] instead of public fields
[SerializeField] private float _moveSpeed = 3.5f;

// ❌ DON'T — Expose fields publicly for Inspector access
public float moveSpeed = 3.5f;

// ✅ DO — Use namespaces for all scripts
namespace TWD.Player
{
    public class PlayerController : MonoBehaviour { }
}

// ✅ DO — Use regions for organization in larger files
#region Movement
    private void HandleMovement() { }
    private void HandleSprint() { }
#endregion

// ✅ DO — Summary comments on public methods
/// <summary>
/// Deals damage to this entity and updates health UI.
/// </summary>
/// <param name="amount">Damage amount before armor reduction.</param>
public void TakeDamage(float amount) { }

// ✅ DO — Use TryGetComponent over GetComponent
if (collision.TryGetComponent<IDamageable>(out var damageable))
{
    damageable.TakeDamage(damage);
}

// ❌ DON'T — Use Find() or GetComponent() in Update()
void Update()
{
    // BAD: var player = GameObject.Find("Player");
    // BAD: var rb = GetComponent<Rigidbody>();
}
```

### File Header Template

```csharp
// ============================================================
// File:        {FileName}.cs
// Namespace:   TWD.{Feature}
// Description: {Brief description of what this script does}
// Author:      The Walking Dead Team
// Created:     {Date}
// ============================================================
```

---

## 11. ARCHITECTURE & PATTERNS

### Core Architecture

```
                    ┌─────────────────┐
                    │   GameManager   │  (Singleton, DontDestroyOnLoad)
                    │   - Game State  │
                    │   - Scene Flow  │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
      ┌──────────────┐ ┌──────────┐ ┌──────────────┐
      │ AudioManager │ │SaveManager│ │ SceneLoader  │
      │ (Singleton)  │ │(Singleton)│ │ (Singleton)  │
      └──────────────┘ └──────────┘ └──────────────┘

      ┌─────────────────────────────────────────────┐
      │                 EventBus                     │
      │  (Static class — decoupled communication)    │
      │  OnPlayerDeath, OnItemPickup, OnEnemyKill,   │
      │  OnLevelComplete, OnPauseToggle              │
      └─────────────────────────────────────────────┘
```

### Design Patterns to Use

| Pattern            | Where to Use                           | Why                                  |
|--------------------|----------------------------------------|--------------------------------------|
| **Singleton**      | GameManager, AudioManager, SaveManager | One instance, global access          |
| **State Machine**  | Player states, Enemy AI states         | Clean state transitions              |
| **Observer/Event** | EventBus for cross-system communication| Decoupled architecture               |
| **ScriptableObject** | Item data, Enemy stats, Weapon data | Data-driven design, easy to tweak    |
| **Object Pool**    | Bullets, VFX, enemy corpses            | Avoid GC spikes from Instantiate     |
| **Strategy**       | Weapon behaviors, AI behaviors         | Swap behaviors without inheritance   |
| **Component**      | Unity's ECS-lite: one script per role  | Modular, reusable MonoBehaviours     |

### Generic Singleton Base Class

```csharp
namespace TWD.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        Debug.LogError($"[Singleton] No instance of {typeof(T)} found!");
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
```

### EventBus Pattern

```csharp
namespace TWD.Core
{
    public static class EventBus
    {
        // Player Events
        public static event Action<float> OnPlayerHealthChanged;
        public static event Action OnPlayerDeath;
        public static event Action<int> OnAmmoChanged;

        // Item Events
        public static event Action<ItemBase> OnItemPickedUp;
        public static event Action<ItemBase> OnItemUsed;

        // Enemy Events
        public static event Action<EnemyBase> OnEnemyKilled;

        // Game Events
        public static event Action OnLevelComplete;
        public static event Action<bool> OnPauseToggle;

        // Fire methods
        public static void PlayerHealthChanged(float hp) => OnPlayerHealthChanged?.Invoke(hp);
        public static void PlayerDied() => OnPlayerDeath?.Invoke();
        // ... etc.
    }
}
```

---

## 12. UI/UX GUIDELINES

### HUD Layout

```
┌─────────────────────────────────────────────────┐
│ [Health Bar]                     [Ammo: 12/30]  │
│ ██████████░░░░                                  │
│                                                 │
│                                                 │
│                                                 │
│                    +  (crosshair)               │
│                                                 │
│                                                 │
│                        [E] Pick Up  ← prompt    │
│                                                 │
│ [Weapon Icon]              [Mini Status Icons]  │
└─────────────────────────────────────────────────┘
```

### UI Design Rules
- **Font**: Use a single, readable font family (e.g., `Roboto Condensed` or `Oswald`)
- **Color Palette**:
  - Background: `#0D0D0D` (near black)
  - Primary text: `#E8E8E8` (off-white)
  - Health bar: `#8B0000` (dark red) → `#FF4444` (bright red when low)
  - Ammo text: `#C0C0C0` (silver)
  - Interactive prompts: `#FFD700` (gold)
  - Danger/critical: `#FF0000` (red)
- **HUD should be minimal** — horror games need immersion, not clutter
- **Inventory uses diegetic-style UI** — dark overlay, grid layout
- **No minimap** — adds to fear of the unknown

---

## 13. AUDIO DESIGN

### Audio Layers

| Layer         | Description                             | Volume Default |
|---------------|-----------------------------------------|----------------|
| Music         | Background ambient/combat music         | 0.3            |
| SFX           | Gameplay sounds (guns, footsteps)       | 0.7            |
| Voice         | Character dialogue / walkie-talkie      | 0.8            |
| Ambient       | Environmental atmosphere loops          | 0.4            |
| UI            | Menu clicks, inventory sounds           | 0.5            |

### Key Sound Design Principles
1. **Silence is powerful** — don't fill every moment with music
2. **Distant zombie groans** should be audible to build tension
3. **Footstep sounds** change with surface (wood, concrete, gravel, water)
4. **Spatial audio**: 3D sound on all SFX — hear enemies from direction
5. **Jump scare stingers**: Short, sharp orchestral hits for scripted scares
6. **Heartbeat**: Plays when health < 30%, intensifies toward 0%

### Audio File Specs

| Type           | Format | Sample Rate | Bit Depth | Notes                     |
|----------------|--------|-------------|-----------|---------------------------|
| Music tracks   | OGG    | 44100 Hz    | 16-bit    | Loopable, ~2-3 min each   |
| SFX            | WAV    | 44100 Hz    | 16-bit    | Short clips, < 3 seconds  |
| Ambient loops  | OGG    | 44100 Hz    | 16-bit    | Seamless loop, 30-60 sec  |
| Voice lines    | WAV    | 22050 Hz    | 16-bit    | Mono, normalized           |

---

## 14. LIGHTING & POST-PROCESSING

### Lighting Strategy (URP)
- **Primary light**: Single directional light (moonlight, cold blue-white)
- **Fill lights**: Sparse point lights (warm orange — lamps, fires)
- **Darkness is a mechanic** — player has a **flashlight** (spotlight, limited cone)
- **Baked lighting** for static environments (save performance)
- **Real-time** only for player flashlight + dynamic enemies

### Post-Processing Stack (URP Volume)

| Effect              | Settings                           | Purpose                     |
|---------------------|------------------------------------|-----------------------------|
| Bloom               | Threshold 1.2, Intensity 0.3      | Subtle glow on lights       |
| Vignette            | Intensity 0.3 → 0.6 (low HP)      | Focus + health feedback     |
| Color Grading       | Desaturated, slight teal shadows   | Gritty horror atmosphere    |
| Film Grain          | Intensity 0.15                     | Cinematic grit              |
| Motion Blur         | Intensity 0.2                      | Smooth camera movement      |
| Chromatic Aberration | Intensity 0.05 → 0.3 (damage)    | Disorientation on hit       |
| Depth of Field      | Bokeh, f/2.8 (only in cutscenes)  | Cinematic focus             |
| Ambient Occlusion   | SSAO, Medium quality               | Ground shadowing            |

---

## 15. PERFORMANCE BUDGETS

### Target Specs

| Metric                | Target                | Hard Limit    |
|-----------------------|-----------------------|---------------|
| **FPS**               | 60 FPS                | Min 30 FPS    |
| **Draw Calls**        | < 200 per frame       | < 400         |
| **Triangles/Frame**   | < 300K                | < 500K        |
| **Texture Memory**    | < 512 MB              | < 1 GB        |
| **RAM Usage**         | < 2 GB                | < 4 GB        |
| **Scene Load Time**   | < 5 seconds           | < 10 seconds  |
| **Active Enemies**    | 8 max                 | 12 absolute   |

### Optimization Rules
1. **Object Pooling** for bullets, VFX, and frequently spawned objects
2. **LOD Groups** on all characters and large props
3. **Occlusion Culling** enabled on all levels
4. **Static Batching** for all non-moving environment objects
5. **Texture Streaming** enabled in Quality Settings
6. **NavMesh baking** per-level (not runtime)
7. **Avoid `Update()`** for things that don't change every frame — use coroutines or event-driven
8. **Profile regularly** with Unity Profiler — check for GC spikes

---

## 16. SAVE/LOAD SYSTEM

### Save Data Structure

```csharp
[System.Serializable]
public class SaveData
{
    // Player State
    public float playerHealth;
    public Vector3Serializable playerPosition;
    public QuaternionSerializable playerRotation;
    public string currentScene;

    // Inventory
    public List<SavedItem> inventoryItems;
    public int currentWeaponIndex;
    public Dictionary<string, int> ammoCounts;

    // World State
    public List<string> unlockedDoors;       // Door IDs
    public List<string> collectedItems;      // Item IDs (don't respawn)
    public List<string> killedEnemies;       // Enemy IDs (don't respawn)
    public List<string> completedPuzzles;    // Puzzle IDs
    public List<string> triggeredEvents;     // One-time events

    // Meta
    public string saveTimestamp;
    public float playTime;
    public int saveSlot;
}
```

### Save/Load Rules
- **Save to**: `Application.persistentDataPath + "/saves/slot_{n}.json"`
- **3 save slots** available
- **Auto-save** at level transitions + before boss fights
- **Manual save** only at specific "safe room" locations (typewriter mechanic from RE)
- **Encrypt** save files with simple XOR to prevent casual editing
- Use **JsonUtility** or **Newtonsoft.Json** for serialization

---

## 17. DEVELOPMENT PHASES

### Phase 1: PROTOTYPE (Weeks 1-4)
> **Goal**: Playable character in a greybox level

- [ ] Set up folder structure (as above)
- [ ] Player movement (WASD + camera)
- [ ] Third-person camera (Cinemachine)
- [ ] Basic interaction system (raycast + prompt)
- [ ] One greybox room with a locked door + key
- [ ] **Milestone**: Walk around, pick up key, unlock door

### Phase 2: COMBAT (Weeks 5-8)
> **Goal**: Player can fight zombies

- [ ] Weapon system (pistol + knife)
- [ ] Aiming + shooting mechanics
- [ ] Zombie Basic AI (idle → chase → attack)
- [ ] Health system + damage
- [ ] Death / game over screen
- [ ] Basic HUD (health bar, ammo count)
- [ ] **Milestone**: Fight and kill 3 zombies in test arena

### Phase 3: SYSTEMS (Weeks 9-12)
> **Goal**: Core systems working together

- [ ] Inventory system (grid UI)
- [ ] Item pickups (ammo, health packs, keys)
- [ ] Door/lock system with key items
- [ ] Save/Load system
- [ ] Pause menu
- [ ] Audio Manager + basic SFX
- [ ] **Milestone**: Complete gameplay loop in greybox Level 1

### Phase 4: CONTENT (Weeks 13-20)
> **Goal**: All 5 levels playable

- [ ] Build Level 1-5 layouts (greybox first, then art)
- [ ] Place enemies, items, puzzles per level
- [ ] Add all weapon types (shotgun, pipe wrench)
- [ ] Add Zombie Crawler + Zombie Brute
- [ ] Puzzle implementations
- [ ] Scene transitions + loading screen
- [ ] **Milestone**: Play from Level 1 to Level 5

### Phase 5: POLISH (Weeks 21-26)
> **Goal**: Looks and sounds like a real game

- [ ] Replace greybox with actual 3D art
- [ ] Lighting passes on all levels
- [ ] Post-processing setup
- [ ] All animations integrated
- [ ] Full SFX + ambient sound
- [ ] Music tracks for each area
- [ ] UI polish (menus, fonts, animations)
- [ ] **Milestone**: Full visual/audio experience

### Phase 6: TESTING & SHIP (Weeks 27-30)
> **Goal**: Bug-free, playable build

- [ ] Playtesting (get 3-5 people to play)
- [ ] Bug fixing
- [ ] Performance optimization passes
- [ ] Build for Windows (PC)
- [ ] Create trailer / screenshots
- [ ] **Milestone**: v1.0 release build

---

## 18. AI PROMPT TEMPLATES

> Copy-paste these templates when asking AI for specific tasks.
> Always attach this master prompt as context.

### Template 1: New Script

```
CONTEXT: I'm building "The Walking Dead" (see attached master prompt).

TASK: Write the {ScriptName}.cs script.

REQUIREMENTS:
- Namespace: TWD.{Feature}
- Follow coding standards from Section 10
- Use [SerializeField] for all inspector fields
- Include XML summary comments on public methods
- Place in: Assets/_Project/Scripts/{Feature}/{ScriptName}.cs

DEPENDENCIES: {List any scripts it needs to interact with}

BEHAVIOR:
{Describe what the script should do in detail}
```

### Template 2: Enemy AI

```
CONTEXT: I'm building "The Walking Dead" (see attached master prompt).

TASK: Implement AI for {EnemyType} using the state machine pattern.

ENEMY STATS (from Section 7):
- Health: {X}
- Damage: {X}
- Speed: {X}

STATES NEEDED: Idle, Chase, Attack, Search, Death
USE: NavMeshAgent for pathfinding
FOLLOW: Architecture patterns from Section 11
```

### Template 3: Level Design Brief

```
CONTEXT: I'm building "The Walking Dead" (see attached master prompt).

TASK: Design the layout for Level {N}: {LevelName}

INCLUDE:
- Room/area descriptions with dimensions
- Enemy placement (types + counts from Section 7)
- Item placement (from Section 8 key items table)
- Puzzle description and solution
- Key progression path (how player moves through)
- Atmosphere notes (lighting, sounds)
- Estimated play time: {X} minutes

CONSTRAINTS:
- Max 8 active enemies
- Player cannot backtrack to previous levels
```

### Template 4: Bug Fix

```
CONTEXT: I'm building "The Walking Dead" (see attached master prompt).

BUG: {Describe what's going wrong}
EXPECTED: {What should happen}
SCRIPT: {Paste the relevant code}
UNITY VERSION: 6000.4.0f1
PIPELINE: URP 17.4.0

Please diagnose and provide the fix.
```

### Template 5: UI Implementation

```
CONTEXT: I'm building "The Walking Dead" (see attached master prompt).

TASK: Create the {UIElement} using Unity's uGUI system.

DESIGN: Follow UI guidelines from Section 12
- Font: {as specified}
- Color palette: {from Section 12}
- Layout: {describe layout}

BEHAVIOR:
- {What it shows/does}
- {How it updates}
- {What events it listens to from EventBus}
```

---

## QUICK REFERENCE CARD

```
PROJECT:        The Walking Dead
ENGINE:         Unity 6 (6000.4.0f1) + URP 17.4.0
LANGUAGE:       C# (TWD.* namespaces)
INPUT:          New Input System 1.19.0
AI NAV:         AI Navigation 2.0.11
CAMERA:         Cinemachine (to be added)
ROOT FOLDER:    Assets/_Project/
SCRIPTS IN:     Assets/_Project/Scripts/{Feature}/
PREFABS IN:     Assets/_Project/Prefabs/{Category}/
SCENES IN:      Assets/_Project/Scenes/
MAX ENEMIES:    8 per scene
TARGET FPS:     60
SAVE FORMAT:    JSON → Application.persistentDataPath
```

---

> **Last Updated**: 2026-03-29
> **Version**: 1.0
> **Next Review**: After Phase 1 completion
