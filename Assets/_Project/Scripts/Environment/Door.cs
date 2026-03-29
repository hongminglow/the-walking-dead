// ============================================================
// File:        Door.cs
// Namespace:   TWD.Environment
// Description: Interactive door that can be locked, unlocked
//              with a key item, and opened/closed with animation.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Core;
using TWD.Inventory;
using TWD.Utilities;

namespace TWD.Environment
{
    /// <summary>
    /// Interactive door. Can be locked/unlocked, requires key items,
    /// and plays open/close animations. Implements IInteractable.
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Door Settings")]
        [SerializeField] private string _doorId;
        [SerializeField] private DoorState _initialState = DoorState.Locked;
        [SerializeField] private string _requiredKeyId;
        [SerializeField] private bool _consumeKey = true;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _openSpeed = 2f;

        [Header("Audio")]
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _lockedSound;
        [SerializeField] private AudioClip _unlockSound;

        [Header("Messages")]
        [SerializeField] private string _lockedMessage = "The door is locked.";
        [SerializeField] private string _jammedMessage = "This door won't budge.";
        [SerializeField] private string _needKeyMessage = "You need a key to open this.";

        #endregion

        #region Private Fields

        private DoorState _currentState;
        private AudioSource _audioSource;
        private bool _isAnimating;
        private Quaternion _closedRotation;
        private Quaternion _openRotation;

        #endregion

        #region IInteractable

        public string InteractPrompt
        {
            get
            {
                return _currentState switch
                {
                    DoorState.Locked => "[E] Open (Locked)",
                    DoorState.Unlocked => "[E] Open Door",
                    DoorState.Open => "[E] Close Door",
                    DoorState.Jammed => "[E] Open (Jammed)",
                    _ => "[E] Door"
                };
            }
        }

        public bool CanInteract => !_isAnimating;

        public void Interact()
        {
            switch (_currentState)
            {
                case DoorState.Locked:
                    TryUnlock();
                    break;

                case DoorState.Unlocked:
                    Open();
                    break;

                case DoorState.Open:
                    Close();
                    break;

                case DoorState.Jammed:
                    PlaySound(_lockedSound);
                    Debug.Log($"[Door] {_doorId}: {_jammedMessage}");
                    break;
            }
        }

        public void OnLookAt()
        {
            EventBus.ShowInteractPrompt(InteractPrompt);
        }

        public void OnLookAway()
        {
            EventBus.HideInteractPrompt();
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _currentState = _initialState;
            _closedRotation = transform.rotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, _openAngle, 0f);

            if (string.IsNullOrEmpty(_doorId))
                _doorId = $"door_{gameObject.GetInstanceID()}";
        }

        #endregion

        #region Door Operations

        private void TryUnlock()
        {
            if (string.IsNullOrEmpty(_requiredKeyId))
            {
                // No key needed, just locked — show locked message
                PlaySound(_lockedSound);
                Debug.Log($"[Door] {_doorId}: {_lockedMessage}");
                return;
            }

            // Check if player has the key
            if (InventoryManager.Instance.HasItem(_requiredKeyId))
            {
                if (_consumeKey)
                {
                    InventoryManager.Instance.ConsumeKeyItem(_requiredKeyId);
                }

                Unlock();
                Open();
            }
            else
            {
                PlaySound(_lockedSound);
                Debug.Log($"[Door] {_doorId}: {_needKeyMessage}");
            }
        }

        private void Unlock()
        {
            _currentState = DoorState.Unlocked;
            PlaySound(_unlockSound);
            EventBus.DoorUnlocked(_doorId);
            Debug.Log($"[Door] {_doorId} unlocked!");
        }

        private void Open()
        {
            _currentState = DoorState.Open;
            _isAnimating = true;

            PlaySound(_openSound);

            if (_animator != null)
            {
                _animator.SetTrigger("Open");
                _isAnimating = false; // Animator handles timing
            }
            else
            {
                StartCoroutine(RotateDoor(_openRotation));
            }

            Debug.Log($"[Door] {_doorId} opened.");
        }

        private void Close()
        {
            _currentState = DoorState.Unlocked;
            _isAnimating = true;

            PlaySound(_closeSound);

            if (_animator != null)
            {
                _animator.SetTrigger("Close");
                _isAnimating = false;
            }
            else
            {
                StartCoroutine(RotateDoor(_closedRotation));
            }
        }

        private System.Collections.IEnumerator RotateDoor(Quaternion target)
        {
            while (Quaternion.Angle(transform.rotation, target) > 1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * _openSpeed);
                yield return null;
            }

            transform.rotation = target;
            _isAnimating = false;
        }

        #endregion

        #region Public Methods

        /// <summary>Forces the door to a specific state (for save/load).</summary>
        public void SetState(DoorState state)
        {
            _currentState = state;
            if (state == DoorState.Open)
            {
                transform.rotation = _openRotation;
            }
        }

        #endregion

        #region Audio

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        #endregion
    }
}
