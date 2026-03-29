// ============================================================
// File:        InventorySlot.cs
// Namespace:   TWD.Inventory
// Description: Represents a single slot in the inventory grid.
//              Holds an ItemData reference and quantity.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

namespace TWD.Inventory
{
    /// <summary>
    /// A single inventory slot. Holds a reference to an ItemData
    /// ScriptableObject and a quantity for stackable items.
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        private ItemData _itemData;
        private int _quantity;

        /// <summary>The item in this slot (null if empty).</summary>
        public ItemData ItemData => _itemData;

        /// <summary>Quantity of the item (1 for non-stackable).</summary>
        public int Quantity => _quantity;

        /// <summary>Whether this slot is empty.</summary>
        public bool IsEmpty => _itemData == null || _quantity <= 0;

        /// <summary>Sets the item and quantity in this slot.</summary>
        public void SetItem(ItemData item, int quantity = 1)
        {
            _itemData = item;
            _quantity = quantity;
        }

        /// <summary>Adds to the quantity (for stacking).</summary>
        public void AddQuantity(int amount)
        {
            _quantity += amount;
            if (_itemData != null && _itemData.isStackable)
            {
                _quantity = UnityEngine.Mathf.Min(_quantity, _itemData.maxStack);
            }
        }

        /// <summary>Removes from the quantity. Clears slot if zero.</summary>
        public void RemoveQuantity(int amount)
        {
            _quantity -= amount;
            if (_quantity <= 0)
            {
                Clear();
            }
        }

        /// <summary>Clears this slot completely.</summary>
        public void Clear()
        {
            _itemData = null;
            _quantity = 0;
        }
    }
}
