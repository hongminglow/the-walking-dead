// ============================================================
// File:        KeyLockPuzzle.cs
// Namespace:   TWD.Puzzle
// Description: Simple key-and-lock puzzle. Player needs a
//              specific key item to unlock/complete.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Core;
using TWD.Inventory;
using TWD.Utilities;

namespace TWD.Puzzle
{
    /// <summary>
    /// Key-lock puzzle. Requires a specific key item from the inventory
    /// to unlock. Displays feedback messages. Most common puzzle type.
    /// </summary>
    public class KeyLockPuzzle : PuzzleBase
    {
        #region Serialized Fields

        [Header("Key Settings")]
        [SerializeField] private string _requiredKeyId;
        [SerializeField] private bool _consumeKey = true;

        [Header("Messages")]
        [SerializeField] private string _lockedMessage = "It's locked. You need a key.";
        [SerializeField] private string _unlockedMessage = "You unlocked it!";

        #endregion

        public override string InteractPrompt =>
            _isCompleted ? "[Completed]" : "[E] Unlock";

        protected override void Awake()
        {
            base.Awake();

            if (string.IsNullOrWhiteSpace(_requiredKeyId))
                _requiredKeyId = RuntimeSceneResolver.InferRequiredItemIdFromObjectName(gameObject.name);
        }

        protected override void OnPuzzleInteract()
        {
            if (string.IsNullOrEmpty(_requiredKeyId))
            {
                CompletePuzzle();
                return;
            }

            if (InventoryManager.Instance.HasItem(_requiredKeyId))
            {
                if (_consumeKey)
                {
                    InventoryManager.Instance.ConsumeKeyItem(_requiredKeyId);
                }

                Debug.Log($"[KeyLockPuzzle] {_unlockedMessage}");
                CompletePuzzle();
            }
            else
            {
                Debug.Log($"[KeyLockPuzzle] {_lockedMessage}");
                OnPuzzleFailed();
            }
        }
    }
}
