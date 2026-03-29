// ============================================================
// File:        SaveData.cs
// Namespace:   TWD.Core
// Description: Serializable data structures for the save system.
//              Represents the complete game state snapshot.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System;
using System.Collections.Generic;

namespace TWD.Core
{
    /// <summary>
    /// Complete game state snapshot for save/load.
    /// Serialized to JSON at Application.persistentDataPath.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // ============================================================
        // Player State
        // ============================================================
        public float playerHealth;
        public float playerStamina;
        public SerializableVector3 playerPosition;
        public SerializableQuaternion playerRotation;
        public string currentScene;

        // ============================================================
        // Inventory
        // ============================================================
        public List<SavedItem> inventoryItems = new List<SavedItem>();
        public string equippedWeaponId;
        public SerializableDict ammoCounts = new SerializableDict();

        // ============================================================
        // World State (what has been done — don't repeat on load)
        // ============================================================
        public List<string> unlockedDoors = new List<string>();
        public List<string> collectedItems = new List<string>();
        public List<string> killedEnemies = new List<string>();
        public List<string> completedPuzzles = new List<string>();
        public List<string> triggeredEvents = new List<string>();

        // ============================================================
        // Meta
        // ============================================================
        public string saveTimestamp;
        public float totalPlayTime;
        public int saveSlot;
        public string gameVersion;
    }

    /// <summary>
    /// Represents a single item in the inventory for serialization.
    /// </summary>
    [Serializable]
    public class SavedItem
    {
        public string itemId;
        public int quantity;
        public int slotIndex;
    }

    /// <summary>
    /// Serializable Vector3 (Unity's Vector3 is not serializable to JSON).
    /// </summary>
    [Serializable]
    public class SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3() { }

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializableVector3(UnityEngine.Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public UnityEngine.Vector3 ToVector3() => new UnityEngine.Vector3(x, y, z);
    }

    /// <summary>
    /// Serializable Quaternion (Unity's Quaternion is not serializable to JSON).
    /// </summary>
    [Serializable]
    public class SerializableQuaternion
    {
        public float x, y, z, w;

        public SerializableQuaternion() { }

        public SerializableQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public SerializableQuaternion(UnityEngine.Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public UnityEngine.Quaternion ToQuaternion() => new UnityEngine.Quaternion(x, y, z, w);
    }

    /// <summary>
    /// Serializable string-to-int dictionary (Unity's Dictionary is not JSON-friendly).
    /// Used for ammo counts: { "PistolAmmo": 12, "ShotgunShells": 4 }
    /// </summary>
    [Serializable]
    public class SerializableDict
    {
        public List<string> keys = new List<string>();
        public List<int> values = new List<int>();

        public void Set(string key, int value)
        {
            int index = keys.IndexOf(key);
            if (index >= 0)
            {
                values[index] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public int Get(string key, int defaultValue = 0)
        {
            int index = keys.IndexOf(key);
            return index >= 0 ? values[index] : defaultValue;
        }

        public bool ContainsKey(string key) => keys.Contains(key);
    }
}
