// ============================================================
// File:        SaveManager.cs
// Namespace:   TWD.Core
// Description: JSON-based save/load system with 3 slots,
//              auto-save support, and simple XOR encryption.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Manages game saving and loading. Serializes SaveData to JSON files
    /// at Application.persistentDataPath/saves/slot_N.json.
    /// Supports 3 save slots and simple XOR encryption.
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        #region Serialized Fields

        [Header("Settings")]
        [SerializeField] private bool _encryptSaves = true;
        [SerializeField] private string _encryptionKey = "TWD_SAVE_2026";

        #endregion

        #region Properties

        /// <summary>Path to the saves directory.</summary>
        public string SaveDirectory => Path.Combine(Application.persistentDataPath, Constants.Save.SAVE_FOLDER);

        #endregion

        #region Private Fields

        private SaveData _pendingLoadData;

        #endregion

        #region Lifecycle

        protected override void OnSingletonAwake()
        {
            // Ensure save directory exists
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the current game state to a slot.
        /// </summary>
        /// <param name="slot">Save slot index (0-2).</param>
        public void Save(int slot)
        {
            if (slot < 0 || slot >= Constants.Save.MAX_SAVE_SLOTS)
            {
                Debug.LogError($"[SaveManager] Invalid slot: {slot}");
                return;
            }

            SaveData data = GatherSaveData();
            data.saveSlot = slot;
            data.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.totalPlayTime = GameManager.Instance.PlayTime;
            data.gameVersion = Application.version;

            string json = JsonUtility.ToJson(data, true);

            if (_encryptSaves)
            {
                json = XorEncrypt(json, _encryptionKey);
            }

            string path = GetSavePath(slot);
            File.WriteAllText(path, json);

            EventBus.GameSaved(slot);
            Debug.Log($"[SaveManager] Game saved to slot {slot} at {path}");
        }

        /// <summary>Auto-save to slot 0 (used at level transitions).</summary>
        public void AutoSave()
        {
            Save(0);
            Debug.Log("[SaveManager] Auto-save complete.");
        }

        #endregion

        #region Load

        /// <summary>
        /// Loads a saved game from a slot.
        /// </summary>
        /// <param name="slot">Save slot index (0-2).</param>
        /// <returns>True if load was successful.</returns>
        public bool Load(int slot)
        {
            if (slot < 0 || slot >= Constants.Save.MAX_SAVE_SLOTS)
            {
                Debug.LogError($"[SaveManager] Invalid slot: {slot}");
                return false;
            }

            string path = GetSavePath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveManager] No save file at slot {slot}");
                return false;
            }

            string json = File.ReadAllText(path);

            if (_encryptSaves)
            {
                json = XorEncrypt(json, _encryptionKey);
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("[SaveManager] Failed to deserialize save data!");
                return false;
            }

            ApplySaveData(data);
            EventBus.GameLoaded(slot);

            Debug.Log($"[SaveManager] Game loaded from slot {slot}");
            return true;
        }

        /// <summary>Loads the most recent save across all slots.</summary>
        public bool LoadLatest()
        {
            SaveData latest = null;
            int latestSlot = -1;

            for (int i = 0; i < Constants.Save.MAX_SAVE_SLOTS; i++)
            {
                string path = GetSavePath(i);
                if (!File.Exists(path)) continue;

                string json = File.ReadAllText(path);
                if (_encryptSaves) json = XorEncrypt(json, _encryptionKey);

                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) continue;

                if (latest == null || string.Compare(data.saveTimestamp, latest.saveTimestamp) > 0)
                {
                    latest = data;
                    latestSlot = i;
                }
            }

            if (latest != null)
            {
                ApplySaveData(latest);
                EventBus.GameLoaded(latestSlot);
                return true;
            }

            Debug.LogWarning("[SaveManager] No saves found.");
            return false;
        }

        #endregion

        #region Query

        /// <summary>Whether a save exists at the given slot.</summary>
        public bool HasSave(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        /// <summary>Whether any save exists across all slots.</summary>
        public bool HasAnySave()
        {
            for (int i = 0; i < Constants.Save.MAX_SAVE_SLOTS; i++)
            {
                if (HasSave(i)) return true;
            }
            return false;
        }

        /// <summary>Deletes a save file at the given slot.</summary>
        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveManager] Deleted save at slot {slot}");
            }
        }

        /// <summary>Gets the save metadata (timestamp, scene) without full load.</summary>
        public SaveData GetSaveInfo(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            if (_encryptSaves) json = XorEncrypt(json, _encryptionKey);

            return JsonUtility.FromJson<SaveData>(json);
        }

        #endregion

        #region Data Gathering / Applying

        private SaveData GatherSaveData()
        {
            var data = new SaveData();

            // Player state
            var player = GameObject.FindWithTag(Constants.Tags.PLAYER);
            if (player != null)
            {
                data.playerPosition = new SerializableVector3(player.transform.position);
                data.playerRotation = new SerializableQuaternion(player.transform.rotation);

                if (player.TryGetComponent<Player.PlayerHealth>(out var health))
                {
                    data.playerHealth = health.CurrentHealth;
                }

                if (player.TryGetComponent<Player.PlayerController>(out var controller))
                {
                    data.playerStamina = controller.CurrentStamina;
                }

                if (player.TryGetComponent<Player.PlayerCombat>(out var combat))
                {
                    combat.WriteSaveData(data);
                }
            }

            // Current scene
            data.currentScene = SceneLoader.Instance.CurrentSceneName;

            // Inventory
            if (Inventory.InventoryManager.IsInitialized)
            {
                data.inventoryItems = Inventory.InventoryManager.Instance.GetSaveData();
            }

            return data;
        }

        private void ApplySaveData(SaveData data)
        {
            _pendingLoadData = data;

            if (SceneLoader.Instance.CurrentSceneName == data.currentScene)
            {
                StartCoroutine(ApplySaveDataNextFrame());
                return;
            }

            GameManager.Instance.SetState(GameState.Loading);
            SceneLoader.Instance.LoadScene(data.currentScene);

            Debug.Log($"[SaveManager] Loading scene: {data.currentScene}. " +
                      $"HP: {data.playerHealth}, PlayTime: {data.totalPlayTime:F1}s");
        }

        #endregion

        #region Encryption

        private string XorEncrypt(string text, string key)
        {
            char[] result = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                result[i] = (char)(text[i] ^ key[i % key.Length]);
            }
            return new string(result);
        }

        #endregion

        #region Helpers

        private string GetSavePath(int slot)
        {
            return Path.Combine(SaveDirectory,
                $"{Constants.Save.SAVE_FILE_PREFIX}{slot}{Constants.Save.SAVE_FILE_EXTENSION}");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_pendingLoadData == null)
            {
                return;
            }

            if (!string.Equals(scene.name, _pendingLoadData.currentScene, StringComparison.Ordinal))
            {
                return;
            }

            StartCoroutine(ApplySaveDataNextFrame());
        }

        private System.Collections.IEnumerator ApplySaveDataNextFrame()
        {
            yield return null;
            yield return null;

            if (_pendingLoadData == null)
            {
                yield break;
            }

            SaveData data = _pendingLoadData;
            _pendingLoadData = null;

            var player = GameObject.FindWithTag(Constants.Tags.PLAYER);
            if (player == null)
            {
                Debug.LogWarning("[SaveManager] Could not find player after scene load. Save application aborted.");
                yield break;
            }

            player.transform.SetPositionAndRotation(
                data.playerPosition != null ? data.playerPosition.ToVector3() : player.transform.position,
                data.playerRotation != null ? data.playerRotation.ToQuaternion() : player.transform.rotation);

            if (player.TryGetComponent<Player.PlayerHealth>(out var health))
            {
                health.SetHealth(data.playerHealth > 0f ? data.playerHealth : health.MaxHealth);
            }

            if (player.TryGetComponent<Player.PlayerController>(out var controller))
            {
                controller.SetStamina(data.playerStamina > 0f ? data.playerStamina : Constants.Player.MAX_STAMINA);
            }

            if (Inventory.InventoryManager.IsInitialized)
            {
                Inventory.InventoryManager.Instance.LoadFromSaveData(data.inventoryItems, BuildItemLookup());
            }

            if (player.TryGetComponent<Player.PlayerCombat>(out var combat))
            {
                combat.ApplySaveData(data, BuildWeaponLookup());
            }

            GameManager.Instance.SetState(GameState.Playing);
            Debug.Log("[SaveManager] Save data applied after scene load.");
        }

        private Dictionary<string, Inventory.ItemData> BuildItemLookup()
        {
            var lookup = new Dictionary<string, Inventory.ItemData>();
            Inventory.ItemData[] allItems = Resources.FindObjectsOfTypeAll<Inventory.ItemData>();

            for (int i = 0; i < allItems.Length; i++)
            {
                Inventory.ItemData item = allItems[i];
                if (item == null || string.IsNullOrEmpty(item.itemId) || lookup.ContainsKey(item.itemId))
                {
                    continue;
                }

                lookup.Add(item.itemId, item);
            }

            return lookup;
        }

        private Dictionary<string, Combat.WeaponData> BuildWeaponLookup()
        {
            var lookup = new Dictionary<string, Combat.WeaponData>();
            Combat.WeaponData[] allWeapons = Resources.FindObjectsOfTypeAll<Combat.WeaponData>();

            for (int i = 0; i < allWeapons.Length; i++)
            {
                Combat.WeaponData weapon = allWeapons[i];
                if (weapon == null || string.IsNullOrEmpty(weapon.weaponId) || lookup.ContainsKey(weapon.weaponId))
                {
                    continue;
                }

                lookup.Add(weapon.weaponId, weapon);
            }

            return lookup;
        }

        #endregion
    }
}
