// ============================================================
// File:        RuntimeSceneResolver.cs
// Namespace:   TWD.Utilities
// Description: Shared runtime fallback helpers for scenes that
//              are partially wired or still using greybox setup.
// Author:      The Walking Dead Team
// Created:     2026-04-09
// ============================================================

using System;
using UnityEngine;
using TWD.Combat;
using TWD.Enemies;
using TWD.Inventory;

namespace TWD.Utilities
{
    /// <summary>
    /// Provides best-effort runtime lookups so authored scenes can recover
    /// from missing serialized references while content is still in flux.
    /// </summary>
    public static class RuntimeSceneResolver
    {
        public static LayerMask MaskFromLayers(params string[] layerNames)
        {
            int mask = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                int layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer >= 0)
                    mask |= 1 << layer;
            }

            return mask;
        }

        public static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
                return null;

            if (string.Equals(root.name, childName, StringComparison.OrdinalIgnoreCase))
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindChildRecursive(root.GetChild(i), childName);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static EnemyData FindEnemyData(EnemyType enemyType)
        {
            EnemyData[] allEnemies = Resources.FindObjectsOfTypeAll<EnemyData>();
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] != null && allEnemies[i].enemyType == enemyType)
                    return allEnemies[i];
            }

            return null;
        }

        public static WeaponData FindWeaponById(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return null;

            WeaponData[] allWeapons = Resources.FindObjectsOfTypeAll<WeaponData>();
            for (int i = 0; i < allWeapons.Length; i++)
            {
                if (allWeapons[i] != null && string.Equals(allWeapons[i].weaponId, weaponId, StringComparison.OrdinalIgnoreCase))
                    return allWeapons[i];
            }

            return null;
        }

        public static WeaponData FindWeaponByType(WeaponType weaponType, string preferredNameHint = null)
        {
            WeaponData[] allWeapons = Resources.FindObjectsOfTypeAll<WeaponData>();
            WeaponData fallback = null;

            for (int i = 0; i < allWeapons.Length; i++)
            {
                WeaponData weapon = allWeapons[i];
                if (weapon == null || weapon.weaponType != weaponType)
                    continue;

                fallback = weapon;

                if (!string.IsNullOrEmpty(preferredNameHint))
                {
                    string haystack = $"{weapon.weaponId} {weapon.weaponName}".ToLowerInvariant();
                    if (haystack.Contains(preferredNameHint.ToLowerInvariant()))
                        return weapon;
                }
            }

            return fallback;
        }

        public static ItemData InferItemDataFromObjectName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return null;

            string name = objectName.ToLowerInvariant();

            if (name.Contains("health"))
                return FindItem(item =>
                    item.itemType == ItemType.Healing ||
                    (!string.IsNullOrEmpty(item.itemId) &&
                     item.itemId.IndexOf("health", StringComparison.OrdinalIgnoreCase) >= 0));

            if (name.Contains("shell"))
                return FindItem(item => item.itemType == ItemType.Ammo && item.ammoType == AmmoType.ShotgunShells);

            if (name.Contains("ammo"))
                return FindItem(item => item.itemType == ItemType.Ammo && item.ammoType == AmmoType.PistolAmmo);

            if (name.Contains("housekey") || name.Contains("house_key") || name.Contains("house key"))
                return FindItem(item => string.Equals(item.itemId, "key_house", StringComparison.OrdinalIgnoreCase));

            if (name.Contains("hospitalkey") || name.Contains("hospital_key") || name.Contains("keycard") || name.Contains("hospital key"))
                return FindItem(item => string.Equals(item.itemId, "key_hospital", StringComparison.OrdinalIgnoreCase));

            return null;
        }

        private static ItemData FindItem(Func<ItemData, bool> predicate)
        {
            ItemData[] allItems = Resources.FindObjectsOfTypeAll<ItemData>();
            for (int i = 0; i < allItems.Length; i++)
            {
                ItemData item = allItems[i];
                if (item != null && predicate(item))
                    return item;
            }

            return null;
        }
    }
}
