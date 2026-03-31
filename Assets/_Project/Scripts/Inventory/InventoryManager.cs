// ============================================================
// File:        InventoryManager.cs
// Namespace:   TWD.Inventory
// Description: Grid-based inventory system (4x6 = 24 slots).
//              Manages items, stacking, key items, and queries.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Inventory
{
    /// <summary>
    /// Manages the player's inventory using a grid-based slot system.
    /// Items are stored as ScriptableObject references with quantities.
    /// </summary>
    public class InventoryManager : Singleton<InventoryManager>
    {
        #region Private Fields

        private InventorySlot[] _slots;

        #endregion

        #region Properties

        /// <summary>All inventory slots.</summary>
        public InventorySlot[] Slots => _slots;

        /// <summary>Total number of slots.</summary>
        public int SlotCount => Constants.Inventory.MAX_SLOTS;

        /// <summary>Number of occupied slots.</summary>
        public int UsedSlots
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (!_slots[i].IsEmpty) count++;
                }
                return count;
            }
        }

        /// <summary>Whether there's at least one empty slot.</summary>
        public bool HasSpace => UsedSlots < SlotCount;

        #endregion

        #region Lifecycle

        protected override void OnSingletonAwake()
        {
            _slots = new InventorySlot[Constants.Inventory.MAX_SLOTS];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = new InventorySlot();
            }
        }

        #endregion

        #region Add / Remove

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        /// <returns>True if successfully added.</returns>
        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (item == null) return false;

            // Try to stack with existing items
            if (item.isStackable)
            {
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (!_slots[i].IsEmpty && _slots[i].ItemData == item)
                    {
                        int canAdd = item.maxStack - _slots[i].Quantity;
                        if (canAdd > 0)
                        {
                            int toAdd = Mathf.Min(quantity, canAdd);
                            _slots[i].AddQuantity(toAdd);
                            quantity -= toAdd;

                            if (quantity <= 0)
                            {
                                EventBus.ItemPickedUp(item.itemName);
                                return true;
                            }
                        }
                    }
                }
            }

            // Find empty slot for remaining
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _slots[i].SetItem(item, Mathf.Min(quantity, item.isStackable ? item.maxStack : 1));
                    quantity -= _slots[i].Quantity;

                    if (quantity <= 0)
                    {
                        EventBus.ItemPickedUp(item.itemName);

                        if (item.isKeyItem)
                        {
                            EventBus.KeyItemObtained(item.itemId);
                        }

                        return true;
                    }
                }
            }

            Debug.LogWarning($"[Inventory] Not enough space for {item.itemName}!");
            return false;
        }

        /// <summary>
        /// Removes an item from a specific slot.
        /// </summary>
        public bool RemoveItem(int slotIndex, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
            if (_slots[slotIndex].IsEmpty) return false;

            // Key items cannot be dropped
            if (_slots[slotIndex].ItemData.isKeyItem)
            {
                Debug.Log("[Inventory] Cannot remove key item.");
                return false;
            }

            string itemName = _slots[slotIndex].ItemData.itemName;
            _slots[slotIndex].RemoveQuantity(quantity);
            EventBus.ItemDropped(itemName);
            return true;
        }

        /// <summary>
        /// Uses an item at the given slot (healing, ammo, etc.).
        /// </summary>
        public bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
            if (_slots[slotIndex].IsEmpty) return false;

            ItemData item = _slots[slotIndex].ItemData;
            if (!item.isUsable)
            {
                Debug.Log($"[Inventory] {item.itemName} is not usable.");
                return false;
            }

            bool used = false;

            switch (item.itemType)
            {
                case ItemType.Healing:
                    var player = GameObject.FindWithTag(Constants.Tags.PLAYER);
                    if (player != null && player.TryGetComponent<Player.PlayerHealth>(out var health))
                    {
                        if (health.CurrentHealth < health.MaxHealth)
                        {
                            health.Heal(item.healAmount);
                            used = true;
                        }
                    }
                    break;

                case ItemType.Ammo:
                    var playerAmmo = GameObject.FindWithTag(Constants.Tags.PLAYER);
                    if (playerAmmo != null && playerAmmo.TryGetComponent<Player.PlayerCombat>(out var combat))
                    {
                        combat.AddReserveAmmo(item.ammoAmount);
                        used = true;
                    }
                    break;
            }

            if (used)
            {
                _slots[slotIndex].RemoveQuantity(1);
                EventBus.ItemUsed(item.itemName);
            }

            return used;
        }

        #endregion

        #region Query

        /// <summary>Checks if the inventory contains an item with the given ID.</summary>
        public bool HasItem(string itemId)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].ItemData.itemId == itemId)
                    return true;
            }
            return false;
        }

        /// <summary>Gets total count of an item by ID across all slots.</summary>
        public int GetItemCount(string itemId)
        {
            int total = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].ItemData.itemId == itemId)
                    total += _slots[i].Quantity;
            }
            return total;
        }

        /// <summary>Consumes a key item by ID. Returns true if found and consumed.</summary>
        public bool ConsumeKeyItem(string itemId)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].ItemData.itemId == itemId)
                {
                    _slots[i].Clear();
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Clear

        /// <summary>Clears the entire inventory. Used on new game.</summary>
        public void ClearAll()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].Clear();
            }
        }

        #endregion

        #region Save/Load Helpers

        /// <summary>Generates save data from current inventory state.</summary>
        public List<SavedItem> GetSaveData()
        {
            var items = new List<SavedItem>();
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty)
                {
                    items.Add(new SavedItem
                    {
                        itemId = _slots[i].ItemData.itemId,
                        quantity = _slots[i].Quantity,
                        slotIndex = i
                    });
                }
            }
            return items;
        }

        #endregion
    }
}
