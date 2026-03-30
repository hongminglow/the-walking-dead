// ============================================================
// File:        InventorySlotUI.cs
// Namespace:   TWD.UI
// Description: Individual inventory slot UI element. Displays
//              item icon, quantity, and handles click selection.
// Author:      The Walking Dead Team
// Created:     2026-03-30
// ============================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TWD.Inventory;

namespace TWD.UI
{
    /// <summary>
    /// UI representation of a single inventory slot.
    /// Shows item icon and quantity badge. Clickable for selection.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _quantityText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Button _button;

        #endregion

        #region Private Fields

        private int _slotIndex;
        private Action _onClickCallback;
        private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private Color _selectedColor = new Color(0.545f, 0f, 0f, 0.8f); // Dark red
        private Color _emptyColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);

        #endregion

        #region Initialization

        /// <summary>Sets up the slot with its index and click callback.</summary>
        public void Initialize(int index, Action onClick)
        {
            _slotIndex = index;
            _onClickCallback = onClick;

            // Auto-find components if not assigned
            if (_button == null) _button = GetComponent<Button>();
            if (_backgroundImage == null) _backgroundImage = GetComponent<Image>();
            if (_iconImage == null)
            {
                var icons = GetComponentsInChildren<Image>();
                if (icons.Length > 1) _iconImage = icons[1]; // First child image
            }
            if (_quantityText == null) _quantityText = GetComponentInChildren<Text>();

            if (_button != null)
            {
                _button.onClick.AddListener(() => _onClickCallback?.Invoke());
            }

            ClearSlot();
        }

        #endregion

        #region Update

        /// <summary>Updates the visual display based on slot data.</summary>
        public void UpdateSlot(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                ClearSlot();
                return;
            }

            // Show icon
            if (_iconImage != null)
            {
                _iconImage.enabled = true;
                _iconImage.sprite = slot.ItemData.icon;
                _iconImage.color = slot.ItemData.icon != null ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            }

            // Show quantity for stackable items
            if (_quantityText != null)
            {
                if (slot.ItemData.isStackable && slot.Quantity > 1)
                {
                    _quantityText.text = slot.Quantity.ToString();
                    _quantityText.enabled = true;
                }
                else
                {
                    _quantityText.enabled = false;
                }
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.color = _normalColor;
            }
        }

        /// <summary>Clears the slot visual.</summary>
        public void ClearSlot()
        {
            if (_iconImage != null)
            {
                _iconImage.enabled = false;
            }

            if (_quantityText != null)
            {
                _quantityText.enabled = false;
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.color = _emptyColor;
            }
        }

        /// <summary>Highlights or unhighlights this slot.</summary>
        public void SetSelected(bool selected)
        {
            if (_backgroundImage != null)
            {
                bool hasItem = _iconImage != null && _iconImage.enabled;
                if (selected)
                    _backgroundImage.color = _selectedColor;
                else
                    _backgroundImage.color = hasItem ? _normalColor : _emptyColor;
            }
        }

        #endregion
    }
}
