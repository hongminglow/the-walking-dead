// ============================================================
// File:        ItemPickup.cs
// Namespace:   TWD.Inventory
// Description: World-space item pickup. When the player interacts,
//              adds the item to inventory and destroys the pickup.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Core;
using TWD.Environment;
using TWD.Utilities;

namespace TWD.Inventory
{
    /// <summary>
    /// Place this on a world object to make it a pickupable item.
    /// Implements IInteractable for the PlayerInteraction system.
    /// </summary>
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Item")]
        [SerializeField] private ItemData _itemData;
        [SerializeField] private int _quantity = 1;

        [Header("Pickup Settings")]
        [SerializeField] private string _pickupId;
        [SerializeField] private bool _destroyOnPickup = true;

        [Header("Visual")]
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.1f;
        [SerializeField] private float _rotateSpeed = 45f;
        [SerializeField] private bool _animatePickup = true;

        #endregion

        #region Private Fields

        private Vector3 _startPosition;
        private bool _hasBeenPickedUp;

        #endregion

        #region IInteractable

        public string InteractPrompt => $"[E] Pick Up {_itemData?.itemName ?? "Item"}";
        public bool CanInteract => !_hasBeenPickedUp && _itemData != null;

        public void Interact()
        {
            if (_hasBeenPickedUp) return;

            if (InventoryManager.Instance.AddItem(_itemData, _quantity))
            {
                _hasBeenPickedUp = true;

                Debug.Log($"[ItemPickup] Picked up: {_itemData.itemName} x{_quantity}");

                if (_destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.Log("[ItemPickup] Inventory full!");
                // TODO: Show "Inventory Full" message on HUD
            }
        }

        public void OnLookAt()
        {
            if (CanInteract)
                EventBus.ShowInteractPrompt(InteractPrompt);
        }

        public void OnLookAway()
        {
            EventBus.HideInteractPrompt();
        }

        #endregion

        #region Lifecycle

        private void Start()
        {
            if (_itemData == null)
            {
                _itemData = RuntimeSceneResolver.InferItemDataFromObjectName(gameObject.name);
                if (_itemData == null)
                {
                    Debug.LogWarning($"[ItemPickup] Could not infer item data for pickup '{gameObject.name}'.");
                }
            }

            _startPosition = transform.position;

            if (string.IsNullOrEmpty(_pickupId))
                _pickupId = gameObject.name.Replace(" ", "_").ToLowerInvariant();
        }

        private void Update()
        {
            if (!_animatePickup) return;

            // Gentle bob and rotate
            float y = _startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(_startPosition.x, y, _startPosition.z);
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
        }

        #endregion

        #region Properties

        /// <summary>Unique ID for save system tracking.</summary>
        public string PickupId => _pickupId;

        /// <summary>The item data for this pickup.</summary>
        public ItemData Item => _itemData;

        #endregion
    }
}
