// ============================================================
// File:        PlayerInteraction.cs
// Namespace:   TWD.Player
// Description: Raycast-based interaction system. Detects IInteractable
//              objects, shows UI prompts, and triggers interaction.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TWD.Core;
using TWD.Environment;
using TWD.Utilities;

namespace TWD.Player
{
    /// <summary>
    /// Casts a ray from the camera center to detect IInteractable objects.
    /// Manages interaction prompts and triggers interactions on input.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Raycast Settings")]
        [SerializeField] private float _interactDistance = Constants.Player.INTERACT_DISTANCE;
        [SerializeField] private LayerMask _interactableLayer;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        #endregion

        #region Private Fields

        private IInteractable _currentTarget;
        private PlayerAnimator _playerAnimator;
        private bool _inputEnabled = true;

        #endregion

        #region Properties

        /// <summary>The interactable currently being looked at, if any.</summary>
        public IInteractable CurrentTarget => _currentTarget;

        /// <summary>Whether an interactable is in range.</summary>
        public bool HasTarget => _currentTarget != null;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _playerAnimator = GetComponent<PlayerAnimator>();
        }

        private void Start()
        {
            if (_cameraTransform == null)
            {
                _cameraTransform = UnityEngine.Camera.main?.transform;
            }
        }

        private void Update()
        {
            if (!_inputEnabled || !GameManager.Instance.IsPlaying) return;
            PerformRaycast();

            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
            {
                if (_currentTarget != null && _currentTarget.CanInteract)
                {
                    _currentTarget.Interact();
                    _playerAnimator?.TriggerInteract();
                }
            }
        }

        #endregion

        #region Input Callbacks

        /// <summary>Called by PlayerInput component for Interact action.</summary>
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (!_inputEnabled || !GameManager.Instance.IsPlaying) return;

            if (_currentTarget != null && _currentTarget.CanInteract)
            {
                _currentTarget.Interact();
                _playerAnimator?.TriggerInteract();
            }
        }

        #endregion

        #region Raycast

        private void PerformRaycast()
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactableLayer))
            {
                // Check if hit object implements IInteractable
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    if (interactable != _currentTarget)
                    {
                        // Lost old target
                        _currentTarget?.OnLookAway();

                        // New target acquired
                        _currentTarget = interactable;
                        _currentTarget.OnLookAt();

                        EventBus.ShowInteractPrompt(_currentTarget.InteractPrompt);
                    }
                    return;
                }
            }

            // No valid hit — clear target
            if (_currentTarget != null)
            {
                _currentTarget.OnLookAway();
                _currentTarget = null;
                EventBus.HideInteractPrompt();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Enables or disables interaction processing.</summary>
        public void SetEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled && _currentTarget != null)
            {
                _currentTarget.OnLookAway();
                _currentTarget = null;
                EventBus.HideInteractPrompt();
            }
        }

        #endregion
    }
}
