// ============================================================
// File:        Constants.cs
// Namespace:   TWD.Utilities
// Description: Global constants, tags, layers, and scene names.
//              Central reference to avoid magic strings.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

namespace TWD.Utilities
{
    /// <summary>
    /// Central location for all game constants.
    /// Avoid magic strings/numbers — reference these instead.
    /// </summary>
    public static class Constants
    {
        #region Scene Names
        public static class Scenes
        {
            public const string MAIN_MENU = "MainMenu";
            public const string LOADING = "Loading";
            public const string LEVEL_01_HOUSE = "Level_01_House";
            public const string LEVEL_02_STREETS = "Level_02_Streets";
            public const string LEVEL_03_HOSPITAL = "Level_03_Hospital";
            public const string LEVEL_04_UNDERGROUND = "Level_04_Underground";
            public const string LEVEL_05_FINALE = "Level_05_Finale";
            public const string TEST_SCENE = "_TestScene";
        }
        #endregion

        #region Tags
        public static class Tags
        {
            public const string PLAYER = "Player";
            public const string ENEMY = "Enemy";
            public const string INTERACTABLE = "Interactable";
            public const string ITEM_PICKUP = "ItemPickup";
            public const string DOOR = "Door";
            public const string PUZZLE = "Puzzle";
            public const string SAVE_POINT = "SavePoint";
            public const string DAMAGE_ZONE = "DamageZone";
        }
        #endregion

        #region Layers
        public static class Layers
        {
            public const string DEFAULT = "Default";
            public const string PLAYER = "Player";
            public const string ENEMY = "Enemy";
            public const string INTERACTABLE = "Interactable";
            public const string GROUND = "Ground";
            public const string OBSTACLE = "Obstacle";
            public const string PROJECTILE = "Projectile";

            // Layer indices (set these in Unity Tag Manager to match)
            public const int PLAYER_INDEX = 6;
            public const int ENEMY_INDEX = 7;
            public const int INTERACTABLE_INDEX = 8;
            public const int GROUND_INDEX = 9;
            public const int OBSTACLE_INDEX = 10;
            public const int PROJECTILE_INDEX = 11;
        }
        #endregion

        #region Player Stats
        public static class Player
        {
            public const float MAX_HEALTH = 100f;
            public const float WALK_SPEED = 3.5f;
            public const float SPRINT_SPEED = 5.5f;
            public const float CROUCH_SPEED = 1.8f;
            public const float ROTATION_SPEED = 10f;

            public const float MAX_STAMINA = 100f;
            public const float STAMINA_DRAIN_RATE = 15f;   // per second while sprinting
            public const float STAMINA_REGEN_RATE = 8f;    // per second while not sprinting

            public const float INTERACT_DISTANCE = 3f;
            public const float HEADSHOT_MULTIPLIER = 2.5f;

            // Health thresholds
            public const float HEALTH_FINE_THRESHOLD = 71f;
            public const float HEALTH_HURT_THRESHOLD = 31f;
            public const float HEALTH_CRITICAL_THRESHOLD = 30f;
        }
        #endregion

        #region Enemy Stats
        public static class Enemies
        {
            public const int MAX_ACTIVE_ENEMIES = 8;

            // Detection
            public const float SIGHT_ANGLE = 60f;
            public const float SIGHT_RANGE = 15f;
            public const float NOISE_DETECTION_RANGE = 8f;
            public const float ATTACK_COOLDOWN = 1.5f;
            public const float SEARCH_DURATION = 5f;
            public const float CHASE_LOSE_DURATION = 8f;
            public const float STAGGER_DAMAGE_THRESHOLD = 30f;

            // Corpse
            public const float CORPSE_LIFETIME = 30f;
            public const float CORPSE_FADE_DURATION = 2f;
        }
        #endregion

        #region Combat
        public static class Combat
        {
            // Pistol
            public const float PISTOL_DAMAGE = 25f;
            public const float PISTOL_FIRE_RATE = 0.4f;
            public const float PISTOL_RANGE = 30f;

            // Shotgun
            public const float SHOTGUN_DAMAGE = 60f;
            public const float SHOTGUN_FIRE_RATE = 1.2f;
            public const float SHOTGUN_RANGE = 10f;

            // Kitchen Knife
            public const float KNIFE_DAMAGE = 15f;
            public const float KNIFE_SWING_RATE = 0.8f;
            public const float KNIFE_RANGE = 1.5f;

            // Pipe Wrench
            public const float WRENCH_DAMAGE = 35f;
            public const float WRENCH_SWING_RATE = 1.0f;
            public const float WRENCH_RANGE = 2.0f;
        }
        #endregion

        #region Inventory
        public static class Inventory
        {
            public const int GRID_COLUMNS = 4;
            public const int GRID_ROWS = 6;
            public const int MAX_SLOTS = GRID_COLUMNS * GRID_ROWS; // 24
        }
        #endregion

        #region Save System
        public static class Save
        {
            public const string SAVE_FOLDER = "saves";
            public const string SAVE_FILE_PREFIX = "slot_";
            public const string SAVE_FILE_EXTENSION = ".json";
            public const int MAX_SAVE_SLOTS = 3;
        }
        #endregion

        #region Animation Parameters
        public static class AnimParams
        {
            // Player
            public const string SPEED = "Speed";
            public const string IS_SPRINTING = "IsSprinting";
            public const string IS_CROUCHING = "IsCrouching";
            public const string IS_AIMING = "IsAiming";
            public const string SHOOT = "Shoot";
            public const string RELOAD = "Reload";
            public const string MELEE = "Melee";
            public const string TAKE_DAMAGE = "TakeDamage";
            public const string IS_DEAD = "IsDead";
            public const string INTERACT = "Interact";

            // Enemy
            public const string IS_CHASING = "IsChasing";
            public const string ATTACK = "Attack";
            public const string STAGGER = "Stagger";
        }
        #endregion
    }
}
