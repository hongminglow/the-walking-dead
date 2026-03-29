# 🧟 THE WALKING DEAD — DEVELOPMENT CHECKLIST

> **HOW TO USE**: This is the single source of truth for development progress.
> - ✅ = Done | 🔲 = Not started | 🚧 = In progress
> - Update this file after each work session.
> - When resuming with AI, say: _"Continue from Stage X, Step Y"_

> **LAST UPDATED**: 2026-03-29 (Session 3)
> **CURRENT STAGE**: Stage 1 — Steps 1.1-1.5 DONE ✅ | Step 1.6 onward 🚧

---

## 📍 QUICK STATUS

| Stage | Name | Status | Notes |
|-------|------|--------|-------|
| 0 | Project Foundation (Scripts) | ✅ COMPLETE | 40 scripts created |
| 1 | Unity Editor Setup | 🚧 IN PROGRESS | Steps 1.1-1.5 auto-done by AI, Step 1.6+ next |
| 2 | Phase 1: Prototype | 🔲 Not started | Walk + pickup + door |
| 3 | Phase 2: Combat | 🔲 Not started | Fight zombies |
| 4 | Phase 3: Systems | 🔲 Not started | Inventory UI + Save/Load |
| 5 | Phase 4: Content | 🔲 Not started | Build 5 levels |
| 6 | Phase 5: Polish | 🔲 Not started | Art, audio, VFX |
| 7 | Phase 6: Testing & Ship | 🔲 Not started | Bug fixes + build |

---

## STAGE 0: PROJECT FOUNDATION (SCRIPTS) ✅ COMPLETE

> 40 C# scripts written. All compile-ready. Architecture is production-grade.

### Scripts Created (40 total)

<details>
<summary>Click to expand full script list</summary>

**Core (5):** `GameManager.cs`, `EventBus.cs`, `SceneLoader.cs`, `SaveData.cs`, `AudioManager.cs`, `SaveManager.cs`

**Utilities (5):** `Singleton.cs`, `Constants.cs`, `Enums.cs`, `Extensions.cs`, `ObjectPool.cs`

**Player (5):** `PlayerController.cs`, `PlayerHealth.cs`, `PlayerCombat.cs`, `PlayerAnimator.cs`, `PlayerInteraction.cs`

**Camera (1):** `ThirdPersonCamera.cs`

**Combat (3):** `IDamageable.cs`, `WeaponData.cs`, `DamageSystem.cs`

**Enemies (6):** `EnemyData.cs`, `EnemyBase.cs`, `ZombieBasic.cs`, `ZombieCrawler.cs`, `ZombieBrute.cs`, `EnemySpawner.cs`

**Inventory (4):** `ItemData.cs`, `InventoryManager.cs`, `InventorySlot.cs`, `ItemPickup.cs`

**Environment (4):** `IInteractable.cs`, `Door.cs`, `Destructible.cs`, `Trap.cs`

**Puzzle (3):** `PuzzleBase.cs`, `KeyLockPuzzle.cs`, `SequencePuzzle.cs`

**UI (3):** `HUDController.cs`, `PauseMenu.cs`, `MainMenuUI.cs`

**Input (1):** `PlayerInputActions.inputactions`

</details>

---

## STAGE 1: UNITY EDITOR SETUP 🚧 START HERE

> These are things that CANNOT be done via code — you must do them manually in the Unity Editor.

---

### Step 1.1 — Install Unity Packages ✅ AUTO-DONE BY AI

> **AI automated this!** Checked your `Packages/manifest.json` — Cinemachine and ProBuilder were already installed.
> TextMeshPro was added by editing manifest.json. Unity will auto-download it when you next open the Editor.

| # | Package | Search For | Status | Why Needed |
|---|---------|-----------|--------|------------|
| 1 | **Cinemachine** | `Cinemachine` | ✅ Already installed (v3.1.6) | Camera system |
| 2 | **ProBuilder** | `ProBuilder` | ✅ Already installed (v6.0.9) | Greybox room building |
| 3 | **TextMeshPro** | `TextMeshPro` | ✅ Added to manifest.json | Better UI text |

> After installing TextMeshPro, Unity will ask "Import TMP Essential Resources?" — click **Import**!

**LATER (don't install yet):**
- DOTween → Get from Asset Store when you need UI animations (Phase 3)
- Addressables → Only for optimization (Phase 5)

---

### Step 1.2 — Setup Tags ✅ AUTO-DONE BY AI

> **AI automated this!** Edited `ProjectSettings/TagManager.asset` directly.

| # | Tag Name | Status | Used By |
|---|----------|--------|---------|
| 1 | `Player` | ✅ (Unity default) | Player prefab |
| 2 | `Enemy` | ✅ | All zombie prefabs |
| 3 | `Interactable` | ✅ | Doors, pickups, puzzles, switches |
| 4 | `ItemPickup` | ✅ | Item pickup objects in the world |
| 5 | `Door` | ✅ | Door objects |
| 6 | `Puzzle` | ✅ | Puzzle objects |
| 7 | `SavePoint` | ✅ | Safe room save typewriters |
| 8 | `DamageZone` | ✅ | Traps and environmental hazards |
| 9 | `Headshot` | ✅ | Enemy head colliders (for 2.5× damage) |

---

### Step 1.3 — Setup Layers ✅ AUTO-DONE BY AI

> **AI automated this!** Edited `ProjectSettings/TagManager.asset` directly.

| # | Index | Layer Name | Status | Used For |
|---|-------|-----------|--------|----------|
| 1 | 6 | `Player` | ✅ | Player collision & detection |
| 2 | 7 | `Enemy` | ✅ | Enemy collision & detection |
| 3 | 8 | `Interactable` | ✅ | Raycast filtering for interactions |
| 4 | 9 | `Ground` | ✅ | Ground check raycasts |
| 5 | 10 | `Obstacle` | ✅ | Camera collision, line-of-sight |
| 6 | 11 | `Projectile` | ✅ | Bullet collision filtering |

---

### Step 1.4 — Setup Physics Collision Matrix ✅ AUTO-DONE BY AI

> **AI automated this!** Edited `ProjectSettings/DynamicsManager.asset` m_LayerCollisionMatrix directly.

| Disable collision between: | Why | Status |
|---------------------------|-----|--------|
| Player ↔ Player | Player can't collide with self | ✅ |
| Enemy ↔ Enemy | Enemies shouldn't block each other | ✅ |
| Enemy ↔ Interactable | Enemies don't interact with objects | ✅ |
| Projectile ↔ Player | Player bullets shouldn't hit player | ✅ |
| Projectile ↔ Interactable | Bullets pass through interactables | ✅ |
| Projectile ↔ Projectile | Bullets don't hit each other | ✅ |
| Projectile ↔ Ground | Bullets don't hit ground (simplification) | ✅ |

---

### Step 1.5 — Create Scenes ✅ AUTO-DONE BY AI

> **AI automated this!** Created all 8 `.unity` scene files and updated `EditorBuildSettings.asset`.
> `_TestScene` has a ground plane (50x50, Layer 9) + dim directional light + fog for horror vibe.

| # | Scene Name | Build Index | Status |
|---|-----------|-------------|--------|
| 1 | `MainMenu` | 0 | ✅ Created (empty placeholder) |
| 2 | `Loading` | 1 | ✅ Created (empty placeholder) |
| 3 | `Level_01_House` | 2 | ✅ Created (empty placeholder) |
| 4 | `Level_02_Streets` | 3 | ✅ Created (empty placeholder) |
| 5 | `Level_03_Hospital` | 4 | ✅ Created (empty placeholder) |
| 6 | `Level_04_Underground` | 5 | ✅ Created (empty placeholder) |
| 7 | `Level_05_Finale` | 6 | ✅ Created (empty placeholder) |
| 8 | `_TestScene` | 7 | ✅ Created (with ground + light + fog) |

---

### Step 1.6 — Create ScriptableObject Data Assets

> **How:** In Unity → navigate to the folder → right-click → **Create → TWD → [category]**
> Fill in the values shown below in the Inspector panel.

#### Weapons (right-click → Create → TWD → Weapons → Weapon Data)

| # | Asset Name | Folder | Key Values | Status |
|---|-----------|--------|------------|--------|
| 1 | `SO_WeaponKnife` | ScriptableObjects/Weapons/ | Melee, 15 dmg, 0.8s rate, 1.5m range | 🔲 |
| 2 | `SO_WeaponPistol` | ScriptableObjects/Weapons/ | Pistol, 25 dmg, 0.4s rate, 30m, PistolAmmo, mag:12 | 🔲 |
| 3 | `SO_WeaponShotgun` | ScriptableObjects/Weapons/ | Shotgun, 60 dmg, 1.2s rate, 10m, ShotgunShells, mag:6 | 🔲 |
| 4 | `SO_WeaponWrench` | ScriptableObjects/Weapons/ | Melee, 35 dmg, 1.0s rate, 2.0m range | 🔲 |

#### Enemies (right-click → Create → TWD → Enemies → Enemy Data)

| # | Asset Name | Folder | Key Values | Status |
|---|-----------|--------|------------|--------|
| 1 | `SO_Zombie_Basic` | ScriptableObjects/Enemies/ | 80 HP, 15 dmg, walk:1.2, chase:2.0 | 🔲 |
| 2 | `SO_Zombie_Crawler` | ScriptableObjects/Enemies/ | 40 HP, 10 dmg, walk:0.8, chase:1.5 | 🔲 |
| 3 | `SO_Zombie_Brute` | ScriptableObjects/Enemies/ | 300 HP, 40 dmg, walk:0.6, chase:1.2 | 🔲 |

#### Items (right-click → Create → TWD → Items → Item Data)

| # | Asset Name | Folder | Key Values | Status |
|---|-----------|--------|------------|--------|
| 1 | `SO_HealthPack` | ScriptableObjects/Items/ | Healing, heal:50, stackable, maxStack:5 | 🔲 |
| 2 | `SO_Ammo_Pistol` | ScriptableObjects/Items/ | Ammo, PistolAmmo, amount:6, stackable | 🔲 |
| 3 | `SO_Ammo_Shotgun` | ScriptableObjects/Items/ | Ammo, ShotgunShells, amount:4, stackable | 🔲 |
| 4 | `SO_Key_House` | ScriptableObjects/Items/ | Key, isKeyItem:true, itemId:"key_house" | 🔲 |
| 5 | `SO_Key_Hospital` | ScriptableObjects/Items/ | Key, isKeyItem:true, itemId:"key_hospital" | 🔲 |

---

### Step 1.7 — Build Manager Prefab

> **How:** In `_TestScene` or any scene:
> 1. Create Empty GameObject → name it `[Managers]`
> 2. Add components one by one
> 3. Drag it into `Assets/_Project/Prefabs/` to make it a prefab

```
[Managers]  (place in every scene or use DontDestroyOnLoad)
├── Add Component: GameManager
├── Add Component: AudioManager
├── Add Component: SceneLoader
├── Add Component: SaveManager
└── Add Component: InventoryManager
```

> AudioManager auto-creates its 5 AudioSource children at runtime — no manual setup needed!

Status: 🔲

---

### Step 1.8 — Build Player Prefab

> **How:** 
> 1. For quick start: use a **Capsule** as placeholder (GameObject → 3D Object → Capsule)
> 2. Or download a character from [Mixamo](https://www.mixamo.com) for proper animations

```
Player  (tag: "Player", layer: 6-Player)
├── Add Component: CharacterController  (height:1.8, radius:0.3, center: 0, 0.9, 0)
├── Add Component: PlayerInput          (Actions: drag PlayerInputActions asset, Behavior: "Send Messages")
├── Add Component: PlayerController     (assign camera transform reference)
├── Add Component: PlayerHealth
├── Add Component: PlayerCombat         (assign weapon data SOs)
├── Add Component: PlayerAnimator       (needs Animator component too)
├── Add Component: PlayerInteraction    (assign camera + interactable layer mask)
├── Add Component: AudioSource
├── Add Component: Animator             (need AnimatorController — create later)
└── Create child empty: "CameraTarget"  (position: 0, 1.6, 0 — head height)
```

Status: 🔲

---

## STAGE 2: PHASE 1 PROTOTYPE 🔲

> **MILESTONE**: Walk around, pick up a key, unlock a door

| # | Task | Status | Details |
|---|------|--------|---------|
| 2.1 | Create `_TestScene` with ground plane | 🔲 | Use ProBuilder or a simple Plane |
| 2.2 | Place `[Managers]` prefab in scene | 🔲 | Drag from Prefabs folder |
| 2.3 | Place Player prefab in scene | 🔲 | Position at (0, 1, 0) |
| 2.4 | Setup camera | 🔲 | Add ThirdPersonCamera to Main Camera, assign player as follow target |
| 2.5 | Build greybox room with ProBuilder | 🔲 | Simple walls + floor + ceiling |
| 2.6 | Create a Door object | 🔲 | Box collider + Door.cs, set requiredKeyId:"key_house" |
| 2.7 | Create a key pickup | 🔲 | Cube/Capsule + ItemPickup.cs, assign SO_Key_House |
| 2.8 | Setup Interactable layer mask | 🔲 | Set door + pickup to Layer 8 (Interactable) |
| 2.9 | Setup Ground layer | 🔲 | Set floor objects to Layer 9 (Ground) |
| 2.10 | Bake NavMesh | 🔲 | Window → AI → Navigation → Bake |
| 2.11 | **TEST**: Walk around ✅ | 🔲 | WASD movement works, camera follows |
| 2.12 | **TEST**: Pick up key ✅ | 🔲 | Press E near key, appears in inventory |
| 2.13 | **TEST**: Unlock door ✅ | 🔲 | Press E on door, it opens |

---

## STAGE 3: PHASE 2 COMBAT 🔲

> **MILESTONE**: Fight and kill 3 zombies in test arena

| # | Task | Status | Details |
|---|------|--------|---------|
| 3.1 | Download zombie model from Mixamo | 🔲 | Search "zombie" → download FBX for Unity |
| 3.2 | Create Zombie Basic prefab | 🔲 | Model + NavMeshAgent + ZombieBasic.cs + assign SO_Zombie_Basic |
| 3.3 | Create weapon SO instances | 🔲 | SO_WeaponKnife + SO_WeaponPistol (if not done in 1.6) |
| 3.4 | Setup EnemySpawner | 🔲 | Create spawner zone with spawn points |
| 3.5 | Create HUD Canvas | 🔲 | Health bar slider + ammo text + interact prompt + crosshair |
| 3.6 | Wire HUD to HUDController.cs | 🔲 | Drag UI elements to serialized fields |
| 3.7 | Create Game Over screen | 🔲 | Simple panel with "You Died" + Retry button |
| 3.8 | **TEST**: Aim + shoot ✅ | 🔲 | Right-click aim, left-click shoot, ammo decreases |
| 3.9 | **TEST**: Zombies chase + attack ✅ | 🔲 | Zombie detects, chases, attacks player |
| 3.10 | **TEST**: Kill 3 zombies ✅ | 🔲 | Zombies take damage, die, fall |

---

## STAGE 4: PHASE 3 CORE SYSTEMS 🔲

> **MILESTONE**: Complete gameplay loop in greybox Level 1

| # | Task | Status | Details |
|---|------|--------|---------|
| 4.1 | Build Inventory UI grid (4×6) | 🔲 | Grid layout group with 24 slots |
| 4.2 | Wire InventoryUI to InventoryManager | 🔲 | Show/hide on Tab, display items |
| 4.3 | Item use/drop/examine from UI | 🔲 | Buttons in inventory for actions |
| 4.4 | Implement Save/Load UI | 🔲 | 3 save slot buttons in pause menu |
| 4.5 | Test Save/Load works | 🔲 | Save state, reload, position restored |
| 4.6 | Pause menu fully functional | 🔲 | Resume, Settings, Save, Load, Quit |
| 4.7 | Audio setup (basic SFX) | 🔲 | Footsteps, gunshot, zombie groan, door |
| 4.8 | Build greybox Level 1 (House) | 🔲 | Multiple rooms, locked door, key, enemies |
| 4.9 | **TEST**: Full Level 1 loop ✅ | 🔲 | Start → explore → key → fight → door → exit |

---

## STAGE 5: PHASE 4 CONTENT 🔲

> **MILESTONE**: Play from Level 1 to Level 5

| # | Task | Status | Details |
|---|------|--------|---------|
| 5.1 | Build Level 2: Streets | 🔲 | Open area, Hospital Keycard, Pipe Wrench |
| 5.2 | Build Level 3: Hospital | 🔲 | Puzzle-heavy, Shotgun, Sewer Map |
| 5.3 | Build Level 4: Sewers | 🔲 | Heavy zombie density, survival gauntlet |
| 5.4 | Build Level 5: Finale | 🔲 | Boss fight (Zombie Brute) + escape |
| 5.5 | Implement Zombie Crawler | 🔲 | Model + prefab + test |
| 5.6 | Implement Zombie Brute (boss) | 🔲 | Model + charge/smash attacks + test |
| 5.7 | All puzzles placed per level | 🔲 | Per master prompt Section 8 |
| 5.8 | Scene transitions + loading | 🔲 | SceneLoader between levels |
| 5.9 | Main Menu scene | 🔲 | New Game, Continue, Settings, Quit |
| 5.10 | **TEST**: Play through all 5 levels ✅ | 🔲 | Start to credits without blocking bugs |

---

## STAGE 6: PHASE 5 POLISH 🔲

> **MILESTONE**: Looks and sounds like a real game

| # | Task | Status | Details |
|---|------|--------|---------|
| 6.1 | Replace greybox with real 3D art | 🔲 | Models, textures per Section 9 |
| 6.2 | Lighting passes on all levels | 🔲 | Baked light + point lights + flashlight |
| 6.3 | Post-processing setup (URP Volume) | 🔲 | Bloom, vignette, film grain, color grading |
| 6.4 | All player animations integrated | 🔲 | Locomotion, combat, interaction |
| 6.5 | All enemy animations integrated | 🔲 | Walk, attack, death, stagger |
| 6.6 | Full SFX implementation | 🔲 | Surface footsteps, spatial audio |
| 6.7 | Music tracks per area | 🔲 | Ambient, combat, boss |
| 6.8 | UI polish (fonts, animations) | 🔲 | TextMeshPro, DOTween transitions |
| 6.9 | Cutscenes (intro + ending) | 🔲 | Timeline sequences |
| 6.10 | **TEST**: Full visual/audio experience ✅ | 🔲 | Feels like a complete game |

---

## STAGE 7: PHASE 6 TESTING & SHIP 🔲

> **MILESTONE**: v1.0 release build

| # | Task | Status | Details |
|---|------|--------|---------|
| 7.1 | Playtesting (3-5 testers) | 🔲 | Collect feedback |
| 7.2 | Bug fixing pass | 🔲 | Fix all reported issues |
| 7.3 | Performance optimization | 🔲 | Profile with Unity Profiler, hit 60 FPS |
| 7.4 | Build for Windows PC | 🔲 | File → Build Settings → Build |
| 7.5 | Create trailer / screenshots | 🔲 | Marketing materials |
| 7.6 | **SHIP**: v1.0 release ✅ | 🔲 | 🎉 |

---

## 🆓 FREE RESOURCES

### Where to Get Assets

| Resource | URL | What You Get |
|----------|-----|-------------|
| **Mixamo** | [mixamo.com](https://www.mixamo.com) | Free 3D characters + animations (FBX) |
| **Unity Asset Store** | [assetstore.unity.com](https://assetstore.unity.com) | Search "free" for free assets |
| **Freesound** | [freesound.org](https://freesound.org) | Free SFX (CC licensed) |
| **ZapSplat** | [zapsplat.com](https://www.zapsplat.com) | Free horror sounds |
| **Kenney** | [kenney.nl](https://kenney.nl) | Free game art (CC0) |
| **OpenGameArt** | [opengameart.org](https://opengameart.org) | Free sprites & 3D models |

---

## 📝 SESSION LOG

| Date | Session | What Was Done | Next Step |
|------|---------|---------------|-----------|
| 2026-03-28 | Session 1 | Project setup, folder structure, 15 foundation scripts | Stage 0 complete |
| 2026-03-29 | Session 2 | 25 new gameplay scripts, input actions, checklist created | Start Stage 1 |
| 2026-03-29 | Session 3 | AI automated Stage 1 steps 1.1-1.5: packages, tags, layers, physics matrix, 8 scenes, build settings | Start Step 1.6 (Create ScriptableObjects) |
| | | | |

---

> **TO RESUME**: Tell the AI: _"@DEVELOPMENT_CHECKLIST.md — Continue from Stage [X], Step [X.X]"_
