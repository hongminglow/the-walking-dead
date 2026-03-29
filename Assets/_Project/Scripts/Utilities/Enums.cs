// ============================================================
// File:        Enums.cs
// Namespace:   TWD.Utilities
// Description: All game enumerations in one place.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

namespace TWD.Utilities
{
    /// <summary>
    /// Current state of the overall game.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        Inventory,
        Cutscene,
        GameOver
    }

    /// <summary>
    /// Player health status tiers with associated visual/audio feedback.
    /// </summary>
    public enum HealthStatus
    {
        Fine,       // 71-100 HP
        Hurt,       // 31-70 HP
        Critical,   // 1-30 HP
        Dead        // 0 HP
    }

    /// <summary>
    /// Player movement/action states for the state machine.
    /// </summary>
    public enum PlayerState
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Aiming,
        Shooting,
        Reloading,
        Melee,
        Interacting,
        TakingDamage,
        Dead
    }

    /// <summary>
    /// Enemy AI behaviour states.
    /// </summary>
    public enum EnemyState
    {
        Idle,
        Wandering,
        Chasing,
        Attacking,
        Searching,
        Staggered,
        Dead
    }

    /// <summary>
    /// Types of enemies in the game.
    /// </summary>
    public enum EnemyType
    {
        ZombieBasic,
        ZombieCrawler,
        ZombieBrute
    }

    /// <summary>
    /// Weapon categories.
    /// </summary>
    public enum WeaponType
    {
        Melee,
        Pistol,
        Shotgun
    }

    /// <summary>
    /// Inventory item categories.
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Ammo,
        Healing,
        Key,
        Document,
        Puzzle
    }

    /// <summary>
    /// Types of ammo in the game.
    /// </summary>
    public enum AmmoType
    {
        None,           // Melee weapons
        PistolAmmo,
        ShotgunShells
    }

    /// <summary>
    /// Types of interactable objects in the world.
    /// </summary>
    public enum InteractableType
    {
        ItemPickup,
        Door,
        Puzzle,
        Document,
        Switch,
        SavePoint
    }

    /// <summary>
    /// Physical surface types for footstep audio.
    /// </summary>
    public enum SurfaceType
    {
        Concrete,
        Wood,
        Metal,
        Gravel,
        Water,
        Grass,
        Tile
    }

    /// <summary>
    /// Door states.
    /// </summary>
    public enum DoorState
    {
        Locked,
        Unlocked,
        Open,
        Jammed     // Cannot be opened (narrative blocker)
    }
}
