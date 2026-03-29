// ============================================================
// File:        ItemData.cs
// Namespace:   TWD.Inventory
// Description: ScriptableObject definition for all inventory items.
//              Create instances via Assets > Create > TWD > Items.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Utilities;

namespace TWD.Inventory
{
    /// <summary>
    /// Data container for inventory items. Create instances in
    /// Assets/_Project/ScriptableObjects/Items/ via the asset menu.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_NewItem", menuName = "TWD/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name shown in inventory UI.")]
        public string itemName;

        [Tooltip("Description shown when examining the item.")]
        [TextArea(2, 5)]
        public string description;

        [Tooltip("Unique identifier for save/load system.")]
        public string itemId;

        [Header("Visual")]
        [Tooltip("Icon displayed in inventory grid.")]
        public Sprite icon;

        [Tooltip("3D model prefab dropped in the world.")]
        public GameObject worldPrefab;

        [Header("Classification")]
        [Tooltip("What kind of item this is.")]
        public ItemType itemType;

        [Header("Stacking")]
        [Tooltip("Can multiple of this item share a slot?")]
        public bool isStackable;

        [Tooltip("Max stack size (only if stackable).")]
        [Range(1, 99)]
        public int maxStack = 1;

        [Header("Usage")]
        [Tooltip("Can this item be used directly from inventory?")]
        public bool isUsable;

        [Tooltip("Can this item be combined with another?")]
        public bool isCombinable;

        [Tooltip("Is this a key item that cannot be dropped?")]
        public bool isKeyItem;

        [Header("Effects (if usable)")]
        [Tooltip("Health restored when used (Healing items).")]
        public float healAmount;

        [Tooltip("Ammo amount granted when used (Ammo items).")]
        public int ammoAmount;

        [Tooltip("Which ammo type this provides.")]
        public AmmoType ammoType;
    }
}
