// ============================================================
// File:        InventoryUI.cs
// Namespace:   TWD.UI
// Description: Inventory grid UI. Shows 4×6 grid of item slots,
//              handles item selection, use, drop, and examine.
//              Toggles on Tab key press.
// Author:      The Walking Dead Team
// Created:     2026-03-30
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TWD.Core;
using TWD.Inventory;
using TWD.Utilities;

namespace TWD.UI
{
    /// <summary>
    /// Manages the inventory grid UI. Displays items in a 4×6 grid,
    /// allows selection, use, and drop actions. Toggles with Tab.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panels")]
        [SerializeField] private GameObject _inventoryPanel;

        [Header("Grid")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _slotPrefab;

        [Header("Item Info")]
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _itemDescriptionText;

        [Header("Action Buttons")]
        [SerializeField] private Button _useButton;
        [SerializeField] private Button _dropButton;

        #endregion

        #region Private Fields

        private InventorySlotUI[] _slotUIs;
        private int _selectedSlot = -1;
        private bool _isOpen;

        #endregion

        #region Properties

        /// <summary>Whether the inventory UI is currently open.</summary>
        public bool IsOpen => _isOpen;

        #endregion

        #region Lifecycle

        private void Start()
        {
            if (_inventoryPanel != null)
                _inventoryPanel.SetActive(false);

            InitializeSlots();

            if (_useButton != null)
                _useButton.onClick.AddListener(OnUseClicked);
            if (_dropButton != null)
                _dropButton.onClick.AddListener(OnDropClicked);

            // Subscribe to inventory changes
            EventBus.OnItemPickedUp += _ => RefreshUI();
            EventBus.OnItemUsed += _ => RefreshUI();
            EventBus.OnItemDropped += _ => RefreshUI();
        }

        private void OnDestroy()
        {
            EventBus.OnItemPickedUp -= _ => RefreshUI();
            EventBus.OnItemUsed -= _ => RefreshUI();
            EventBus.OnItemDropped -= _ => RefreshUI();
        }

        #endregion

        #region Input

        /// <summary>Called by PlayerInput for Inventory action (Tab key).</summary>
        public void OnInventory(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            Toggle();
        }

        #endregion

        #region Toggle

        /// <summary>Toggles inventory visibility.</summary>
        public void Toggle()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        /// <summary>Opens the inventory UI.</summary>
        public void Open()
        {
            _isOpen = true;
            _inventoryPanel?.SetActive(true);
            RefreshUI();

            // Show cursor for item clicking
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Pause-like state (optional — freeze player movement)
            Time.timeScale = 0.1f; // Slow-motion while inventory is open
        }

        /// <summary>Closes the inventory UI.</summary>
        public void Close()
        {
            _isOpen = false;
            _inventoryPanel?.SetActive(false);
            _selectedSlot = -1;
            ClearItemInfo();

            // Hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Time.timeScale = 1f;
        }

        #endregion

        #region Slots

        private void InitializeSlots()
        {
            if (_slotContainer == null || _slotPrefab == null)
            {
                Debug.LogWarning("[InventoryUI] Missing slot container or prefab.");
                return;
            }

            int slotCount = Constants.Inventory.MAX_SLOTS;
            _slotUIs = new InventorySlotUI[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = Instantiate(_slotPrefab, _slotContainer);
                slotObj.name = $"Slot_{i}";

                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI == null)
                    slotUI = slotObj.AddComponent<InventorySlotUI>();

                int index = i; // Capture for closure
                slotUI.Initialize(index, () => SelectSlot(index));
                _slotUIs[i] = slotUI;
            }
        }

        private void SelectSlot(int index)
        {
            _selectedSlot = index;

            var slots = InventoryManager.Instance.Slots;
            if (index >= 0 && index < slots.Length && !slots[index].IsEmpty)
            {
                var item = slots[index].ItemData;
                ShowItemInfo(item.itemName, item.description, item.isUsable, !item.isKeyItem);
            }
            else
            {
                ClearItemInfo();
            }

            // Highlight selected slot
            for (int i = 0; i < _slotUIs.Length; i++)
            {
                _slotUIs[i]?.SetSelected(i == _selectedSlot);
            }
        }

        #endregion

        #region Refresh

        /// <summary>Refreshes all slot visuals from InventoryManager data.</summary>
        public void RefreshUI()
        {
            if (_slotUIs == null) return;

            var slots = InventoryManager.Instance.Slots;
            for (int i = 0; i < _slotUIs.Length; i++)
            {
                if (i < slots.Length)
                {
                    _slotUIs[i]?.UpdateSlot(slots[i]);
                }
            }
        }

        #endregion

        #region Item Info

        private void ShowItemInfo(string name, string description, bool canUse, bool canDrop)
        {
            if (_itemNameText != null) _itemNameText.text = name;
            if (_itemDescriptionText != null) _itemDescriptionText.text = description;
            if (_useButton != null) _useButton.gameObject.SetActive(canUse);
            if (_dropButton != null) _dropButton.gameObject.SetActive(canDrop);
        }

        private void ClearItemInfo()
        {
            if (_itemNameText != null) _itemNameText.text = "";
            if (_itemDescriptionText != null) _itemDescriptionText.text = "";
            if (_useButton != null) _useButton.gameObject.SetActive(false);
            if (_dropButton != null) _dropButton.gameObject.SetActive(false);
        }

        #endregion

        #region Actions

        private void OnUseClicked()
        {
            if (_selectedSlot < 0) return;
            InventoryManager.Instance.UseItem(_selectedSlot);
            RefreshUI();
            SelectSlot(_selectedSlot);
        }

        private void OnDropClicked()
        {
            if (_selectedSlot < 0) return;
            InventoryManager.Instance.RemoveItem(_selectedSlot);
            RefreshUI();
            ClearItemInfo();
        }

        #endregion
    }
}
