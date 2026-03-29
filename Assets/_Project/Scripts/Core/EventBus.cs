// ============================================================
// File:        EventBus.cs
// Namespace:   TWD.Core
// Description: Global event system for decoupled communication
//              between game systems. Subscribe/unsubscribe to
//              events without direct references.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System;

namespace TWD.Core
{
    /// <summary>
    /// Static event bus for decoupled cross-system communication.
    /// Systems subscribe to events they care about without needing
    /// direct references to other systems.
    /// 
    /// Usage:
    ///   Subscribe:   EventBus.OnPlayerDeath += HandlePlayerDeath;
    ///   Unsubscribe: EventBus.OnPlayerDeath -= HandlePlayerDeath;
    ///   Fire:        EventBus.PlayerDied();
    /// </summary>
    public static class EventBus
    {
        #region Player Events

        /// <summary>Fired when player health changes. Param: new health value.</summary>
        public static event Action<float> OnPlayerHealthChanged;

        /// <summary>Fired when player stamina changes. Param: new stamina value.</summary>
        public static event Action<float> OnPlayerStaminaChanged;

        /// <summary>Fired when player dies.</summary>
        public static event Action OnPlayerDeath;

        /// <summary>Fired when player takes damage. Param: damage amount.</summary>
        public static event Action<float> OnPlayerDamaged;

        /// <summary>Fired when player heals. Param: heal amount.</summary>
        public static event Action<float> OnPlayerHealed;

        #endregion

        #region Combat Events

        /// <summary>Fired when ammo count changes. Params: current ammo, max ammo.</summary>
        public static event Action<int, int> OnAmmoChanged;

        /// <summary>Fired when player switches weapon. Param: weapon name.</summary>
        public static event Action<string> OnWeaponSwitched;

        /// <summary>Fired when player fires a weapon.</summary>
        public static event Action OnWeaponFired;

        /// <summary>Fired when player reloads.</summary>
        public static event Action OnWeaponReloaded;

        #endregion

        #region Item Events

        /// <summary>Fired when an item is picked up. Param: item name.</summary>
        public static event Action<string> OnItemPickedUp;

        /// <summary>Fired when an item is used. Param: item name.</summary>
        public static event Action<string> OnItemUsed;

        /// <summary>Fired when an item is dropped. Param: item name.</summary>
        public static event Action<string> OnItemDropped;

        /// <summary>Fired when a key item is obtained. Param: key ID.</summary>
        public static event Action<string> OnKeyItemObtained;

        #endregion

        #region Enemy Events

        /// <summary>Fired when an enemy is killed. Param: enemy ID.</summary>
        public static event Action<string> OnEnemyKilled;

        /// <summary>Fired when an enemy detects the player. Param: enemy ID.</summary>
        public static event Action<string> OnEnemyAlerted;

        /// <summary>Fired when an enemy loses the player. Param: enemy ID.</summary>
        public static event Action<string> OnEnemyLostPlayer;

        #endregion

        #region Environment Events

        /// <summary>Fired when a door is unlocked. Param: door ID.</summary>
        public static event Action<string> OnDoorUnlocked;

        /// <summary>Fired when a puzzle is completed. Param: puzzle ID.</summary>
        public static event Action<string> OnPuzzleCompleted;

        /// <summary>Fired when player enters a trigger zone. Param: zone ID.</summary>
        public static event Action<string> OnZoneEntered;

        #endregion

        #region Game State Events

        /// <summary>Fired when a level is completed. Param: scene name.</summary>
        public static event Action<string> OnLevelComplete;

        /// <summary>Fired when pause state toggles. Param: is paused.</summary>
        public static event Action<bool> OnPauseToggle;

        /// <summary>Fired when game is saved. Param: slot index.</summary>
        public static event Action<int> OnGameSaved;

        /// <summary>Fired when game is loaded. Param: slot index.</summary>
        public static event Action<int> OnGameLoaded;

        /// <summary>Fired when a cutscene starts. Param: cutscene ID.</summary>
        public static event Action<string> OnCutsceneStarted;

        /// <summary>Fired when a cutscene ends. Param: cutscene ID.</summary>
        public static event Action<string> OnCutsceneEnded;

        #endregion

        #region UI Events

        /// <summary>Fired when interaction prompt should show. Param: prompt text.</summary>
        public static event Action<string> OnShowInteractPrompt;

        /// <summary>Fired when interaction prompt should hide.</summary>
        public static event Action OnHideInteractPrompt;

        /// <summary>Fired when a document/note is opened. Params: title, content.</summary>
        public static event Action<string, string> OnDocumentOpened;

        #endregion

        // ====================================================================
        // FIRE METHODS — Call these to trigger events
        // ====================================================================

        #region Player Fire Methods
        public static void PlayerHealthChanged(float health) => OnPlayerHealthChanged?.Invoke(health);
        public static void PlayerStaminaChanged(float stamina) => OnPlayerStaminaChanged?.Invoke(stamina);
        public static void PlayerDied() => OnPlayerDeath?.Invoke();
        public static void PlayerDamaged(float amount) => OnPlayerDamaged?.Invoke(amount);
        public static void PlayerHealed(float amount) => OnPlayerHealed?.Invoke(amount);
        #endregion

        #region Combat Fire Methods
        public static void AmmoChanged(int current, int max) => OnAmmoChanged?.Invoke(current, max);
        public static void WeaponSwitched(string weaponName) => OnWeaponSwitched?.Invoke(weaponName);
        public static void WeaponFired() => OnWeaponFired?.Invoke();
        public static void WeaponReloaded() => OnWeaponReloaded?.Invoke();
        #endregion

        #region Item Fire Methods
        public static void ItemPickedUp(string itemName) => OnItemPickedUp?.Invoke(itemName);
        public static void ItemUsed(string itemName) => OnItemUsed?.Invoke(itemName);
        public static void ItemDropped(string itemName) => OnItemDropped?.Invoke(itemName);
        public static void KeyItemObtained(string keyId) => OnKeyItemObtained?.Invoke(keyId);
        #endregion

        #region Enemy Fire Methods
        public static void EnemyKilled(string enemyId) => OnEnemyKilled?.Invoke(enemyId);
        public static void EnemyAlerted(string enemyId) => OnEnemyAlerted?.Invoke(enemyId);
        public static void EnemyLostPlayer(string enemyId) => OnEnemyLostPlayer?.Invoke(enemyId);
        #endregion

        #region Environment Fire Methods
        public static void DoorUnlocked(string doorId) => OnDoorUnlocked?.Invoke(doorId);
        public static void PuzzleCompleted(string puzzleId) => OnPuzzleCompleted?.Invoke(puzzleId);
        public static void ZoneEntered(string zoneId) => OnZoneEntered?.Invoke(zoneId);
        #endregion

        #region Game State Fire Methods
        public static void LevelCompleted(string sceneName) => OnLevelComplete?.Invoke(sceneName);
        public static void PauseToggled(bool isPaused) => OnPauseToggle?.Invoke(isPaused);
        public static void GameSaved(int slot) => OnGameSaved?.Invoke(slot);
        public static void GameLoaded(int slot) => OnGameLoaded?.Invoke(slot);
        public static void CutsceneStarted(string cutsceneId) => OnCutsceneStarted?.Invoke(cutsceneId);
        public static void CutsceneEnded(string cutsceneId) => OnCutsceneEnded?.Invoke(cutsceneId);
        #endregion

        #region UI Fire Methods
        public static void ShowInteractPrompt(string text) => OnShowInteractPrompt?.Invoke(text);
        public static void HideInteractPrompt() => OnHideInteractPrompt?.Invoke();
        public static void DocumentOpened(string title, string content) => OnDocumentOpened?.Invoke(title, content);
        #endregion

        /// <summary>
        /// Clears ALL event subscriptions. Call when returning to main menu
        /// to prevent memory leaks from destroyed objects.
        /// </summary>
        public static void ClearAll()
        {
            OnPlayerHealthChanged = null;
            OnPlayerStaminaChanged = null;
            OnPlayerDeath = null;
            OnPlayerDamaged = null;
            OnPlayerHealed = null;
            OnAmmoChanged = null;
            OnWeaponSwitched = null;
            OnWeaponFired = null;
            OnWeaponReloaded = null;
            OnItemPickedUp = null;
            OnItemUsed = null;
            OnItemDropped = null;
            OnKeyItemObtained = null;
            OnEnemyKilled = null;
            OnEnemyAlerted = null;
            OnEnemyLostPlayer = null;
            OnDoorUnlocked = null;
            OnPuzzleCompleted = null;
            OnZoneEntered = null;
            OnLevelComplete = null;
            OnPauseToggle = null;
            OnGameSaved = null;
            OnGameLoaded = null;
            OnCutsceneStarted = null;
            OnCutsceneEnded = null;
            OnShowInteractPrompt = null;
            OnHideInteractPrompt = null;
            OnDocumentOpened = null;
        }
    }
}
