// ============================================================
// File:        PlayerController.cs
// Namespace:   TWD.Player
// Description: Main player controller using Unity's New Input System.
//              Handles movement, sprinting, crouching, and rotation
//              relative to the camera. Uses CharacterController.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Player
{
    /// <summary>
    /// Core player controller. Reads input, moves via CharacterController,
    /// manages stamina, and drives the player animator.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = Constants.Player.WALK_SPEED;
        [SerializeField] private float _sprintSpeed = Constants.Player.SPRINT_SPEED;
        [SerializeField] private float _crouchSpeed = Constants.Player.CROUCH_SPEED;
        [SerializeField] private float _rotationSpeed = Constants.Player.ROTATION_SPEED;
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -15f;
        [SerializeField] private float _groundCheckDistance = 0.3f;
        [SerializeField] private LayerMask _groundMask;

        [Header("Stamina")]
        [SerializeField] private float _maxStamina = Constants.Player.MAX_STAMINA;
        [SerializeField] private float _staminaDrainRate = Constants.Player.STAMINA_DRAIN_RATE;
        [SerializeField] private float _staminaRegenRate = Constants.Player.STAMINA_REGEN_RATE;
        [SerializeField] private float _staminaRegenDelay = 1f;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        #endregion

        #region Private Fields

        private CharacterController _characterController;
        private PlayerAnimator _playerAnimator;
        private PlayerHealth _playerHealth;
        private Vector2 _moveInput;
        private Vector3 _verticalVelocity;
        private float _currentStamina;
        private float _staminaRegenTimer;
        private bool _isSprinting;
        private bool _isCrouching;
        private bool _isAiming;
        private bool _isGrounded;
        private bool _inputEnabled = true;
        private bool _jumpQueued;

        #endregion

        #region Properties

        /// <summary>Whether the player is currently sprinting.</summary>
        public bool IsSprinting => _isSprinting;

        /// <summary>Whether the player is currently crouching.</summary>
        public bool IsCrouching => _isCrouching;

        /// <summary>Whether the player is currently aiming.</summary>
        public bool IsAiming => _isAiming;

        /// <summary>Whether the player is on the ground.</summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>Current stamina value (0 to MaxStamina).</summary>
        public float CurrentStamina => _currentStamina;

        /// <summary>Current move speed based on state.</summary>
        public float CurrentSpeed
        {
            get
            {
                if (_isAiming) return 0f; // Cannot move while aiming (RE-style)
                float baseSpeed;
                if (_isCrouching) baseSpeed = _crouchSpeed;
                else if (_isSprinting && _currentStamina > 0f) baseSpeed = _sprintSpeed;
                else baseSpeed = _walkSpeed;

                // Slow down when critically injured
                float healthMult = _playerHealth != null ? _playerHealth.SpeedMultiplier : 1f;
                return baseSpeed * healthMult;
            }
        }

        /// <summary>Normalized movement magnitude for animations.</summary>
        public float MoveAmount { get; private set; }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerAnimator = GetComponent<PlayerAnimator>();
            _playerHealth = GetComponent<PlayerHealth>();
            _currentStamina = _maxStamina;
        }

        private void Start()
        {
            if (_cameraTransform == null)
            {
                _cameraTransform = UnityEngine.Camera.main?.transform;
                if (_cameraTransform == null)
                {
                    Debug.LogError("[PlayerController] No camera found! Assign CameraTransform.");
                }
            }

            if (_groundMask.value == 0)
                _groundMask = RuntimeSceneResolver.MaskFromLayers(Constants.Layers.GROUND);
        }

        private void Update()
        {
            if (!_inputEnabled || !GameManager.Instance.IsPlaying) return;

            ReadInput();
            GroundCheck();
            HandleJump();
            HandleMovement();
            HandleStamina();
            ApplyGravity();
            SyncAnimatorState();
        }

        #endregion

        #region Direct Input Polling

        private void ReadInput()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                float x = 0f, y = 0f;
                if (kb.wKey.isPressed) y += 1f;
                if (kb.sKey.isPressed) y -= 1f;
                if (kb.aKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed) x += 1f;
                _moveInput = new Vector2(x, y).normalized;

                _isSprinting = kb.leftShiftKey.isPressed;

                if (kb.cKey.wasPressedThisFrame)
                {
                    _isCrouching = !_isCrouching;
                    if (_isCrouching) _isSprinting = false;
                    _characterController.height = _isCrouching ? 1.2f : 1.8f;
                    _characterController.center = new Vector3(0f, _characterController.height / 2f, 0f);
                }

                if (kb.spaceKey.wasPressedThisFrame)
                    _jumpQueued = true;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                _isAiming = mouse.rightButton.isPressed;
            }
        }

        /// <summary>Called by PlayerInput component for Move action.</summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        public void OnMove(InputValue value)
        {
            if (value != null)
                _moveInput = value.Get<Vector2>();
        }

        /// <summary>Called by PlayerInput component for Sprint action.</summary>
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
                _isSprinting = true;
            else if (context.canceled)
                _isSprinting = false;
        }

        public void OnSprint(InputValue value)
        {
            _isSprinting = value != null && value.isPressed;
        }

        /// <summary>Called by PlayerInput component for Crouch action (toggle).</summary>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ToggleCrouchState();
            }
        }

        public void OnCrouch(InputValue value)
        {
            if (value != null && value.isPressed)
                ToggleCrouchState();
        }

        /// <summary>Called by PlayerInput component for Aim action.</summary>
        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.performed)
                _isAiming = true;
            else if (context.canceled)
                _isAiming = false;
        }

        public void OnAim(InputValue value)
        {
            _isAiming = value != null && value.isPressed;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                _jumpQueued = true;
        }

        public void OnJump(InputValue value)
        {
            if (value != null && value.isPressed)
                _jumpQueued = true;
        }

        #endregion

        #region Movement

        private void GroundCheck()
        {
            _isGrounded = Physics.CheckSphere(
                transform.position + Vector3.up * 0.1f,
                _groundCheckDistance,
                _groundMask
            );
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;

            if (_moveInput.sqrMagnitude > 0.01f && !_isAiming)
            {
                // Calculate direction relative to camera
                Vector3 camForward = _cameraTransform.forward.Flat().normalized;
                Vector3 camRight = _cameraTransform.right.Flat().normalized;

                moveDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

                // Rotate player toward movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
            else if (_isAiming && _cameraTransform != null)
            {
                // While aiming, face camera forward direction
                Vector3 aimDirection = _cameraTransform.forward.Flat().normalized;
                if (aimDirection.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(aimDirection),
                        _rotationSpeed * 2f * Time.deltaTime
                    );
                }
            }

            // Apply movement
            float speed = CurrentSpeed;
            MoveAmount = _isAiming ? 0f : Mathf.Clamp01(_moveInput.magnitude);
            _characterController.Move(moveDirection * speed * Time.deltaTime);

            // Update animator
            _playerAnimator?.SetMovement(MoveAmount * (speed / _walkSpeed));
        }

        private void HandleJump()
        {
            if (!_jumpQueued)
                return;

            _jumpQueued = false;

            if (!_isGrounded || _isCrouching)
                return;

            _verticalVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _isGrounded = false;
            _isSprinting = false;
            _playerAnimator?.SetGrounded(false);
            _playerAnimator?.TriggerJump();
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _verticalVelocity.y < 0f)
            {
                _verticalVelocity.y = -2f; // Small downward force to keep grounded
            }
            else
            {
                _verticalVelocity.y += _gravity * Time.deltaTime;
            }

            _characterController.Move(_verticalVelocity * Time.deltaTime);
        }

        #endregion

        #region Stamina

        private void HandleStamina()
        {
            if (_isSprinting && _moveInput.sqrMagnitude > 0.01f)
            {
                _currentStamina -= _staminaDrainRate * Time.deltaTime;
                _staminaRegenTimer = _staminaRegenDelay;

                if (_currentStamina <= 0f)
                {
                    _currentStamina = 0f;
                    _isSprinting = false;
                }
            }
            else
            {
                _staminaRegenTimer -= Time.deltaTime;
                if (_staminaRegenTimer <= 0f)
                {
                    _currentStamina = Mathf.Min(_currentStamina + _staminaRegenRate * Time.deltaTime, _maxStamina);
                }
            }

            EventBus.PlayerStaminaChanged(_currentStamina);
        }

        private void ToggleCrouchState()
        {
            _isCrouching = !_isCrouching;
            if (_isCrouching) _isSprinting = false;
            _characterController.height = _isCrouching ? 1.2f : 1.8f;
            _characterController.center = new Vector3(0f, _characterController.height / 2f, 0f);
        }

        private void SyncAnimatorState()
        {
            if (_playerAnimator == null) return;

            bool isActivelySprinting = _isSprinting &&
                                       !_isAiming &&
                                       !_isCrouching &&
                                       _moveInput.sqrMagnitude > 0.01f &&
                                       _currentStamina > 0f;

            _playerAnimator.SetSprinting(isActivelySprinting);
            _playerAnimator.SetCrouching(_isCrouching);
            _playerAnimator.SetAiming(_isAiming);
            _playerAnimator.SetGrounded(_isGrounded);
        }

        #endregion

        #region Public Methods

        /// <summary>Enables or disables player input processing.</summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled)
            {
                _moveInput = Vector2.zero;
                _isSprinting = false;
                _isAiming = false;
                MoveAmount = 0f;
            }

            SyncAnimatorState();
        }

        /// <summary>Forces the player to stop all movement.</summary>
        public void StopMovement()
        {
            _moveInput = Vector2.zero;
            _isSprinting = false;
            MoveAmount = 0f;
            _playerAnimator?.SetMovement(0f);
            SyncAnimatorState();
        }

        /// <summary>Applies a stamina value from save/load or scripted events.</summary>
        public void SetStamina(float stamina)
        {
            _currentStamina = Mathf.Clamp(stamina, 0f, _maxStamina);
            _staminaRegenTimer = _staminaRegenDelay;
            EventBus.PlayerStaminaChanged(_currentStamina);
            SyncAnimatorState();
        }

        #endregion
    }
}
