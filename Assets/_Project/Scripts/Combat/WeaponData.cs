// ============================================================
// File:        WeaponData.cs
// Namespace:   TWD.Combat
// Description: ScriptableObject definition for weapon stats.
//              Create instances via Assets > Create > TWD > Weapons.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Utilities;

namespace TWD.Combat
{
    /// <summary>
    /// Data container for weapon stats. Create instances in
    /// Assets/_Project/ScriptableObjects/Weapons/ via the asset menu.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_NewWeapon", menuName = "TWD/Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Weapon display name.")]
        public string weaponName;

        [Tooltip("Unique identifier for save/load.")]
        public string weaponId;

        [Tooltip("Weapon category.")]
        public WeaponType weaponType;

        [Header("Visual")]
        [Tooltip("HUD icon for this weapon.")]
        public Sprite icon;

        [Tooltip("Weapon model prefab (attached to player hand).")]
        public GameObject weaponPrefab;

        [Header("Combat Stats")]
        [Tooltip("Base damage per hit.")]
        public float damage;

        [Tooltip("Seconds between attacks/shots.")]
        public float fireRate;

        [Tooltip("Maximum effective range in meters.")]
        public float range;

        [Tooltip("Headshot damage multiplier.")]
        public float headshotMultiplier = 2.5f;

        [Header("Ammo (Ranged Only)")]
        [Tooltip("Ammo type consumed.")]
        public AmmoType ammoType = AmmoType.None;

        [Tooltip("Magazine/clip size.")]
        public int magazineSize;

        [Tooltip("Time to reload in seconds.")]
        public float reloadTime = 1.5f;

        [Header("Feel")]
        [Tooltip("Camera shake intensity when firing.")]
        [Range(0f, 1f)]
        public float cameraShakeIntensity = 0.2f;

        [Tooltip("Recoil amount (ranged weapons).")]
        [Range(0f, 5f)]
        public float recoil = 1f;

        [Header("Audio")]
        [Tooltip("Sound played on fire/swing.")]
        public AudioClip fireSound;

        [Tooltip("Sound played on reload.")]
        public AudioClip reloadSound;

        [Tooltip("Sound played on impact.")]
        public AudioClip impactSound;

        /// <summary>Whether this weapon uses ammo.</summary>
        public bool UsesAmmo => ammoType != AmmoType.None;

        /// <summary>Whether this is a melee weapon.</summary>
        public bool IsMelee => weaponType == WeaponType.Melee;
    }
}
