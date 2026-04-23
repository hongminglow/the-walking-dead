// ============================================================
// File:        PlayerAnimator.cs
// Namespace:   TWD.Player
// Description: Bridges player state to Unity Animator parameters.
//              Driven by PlayerController, PlayerCombat, etc.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Utilities;

namespace TWD.Player
{
    /// <summary>
    /// Handles all Animator parameter updates for the player.
    /// Other player scripts call methods on this to trigger animations.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        #region Private Fields

        private Animator _animator;
        private int _speedHash;
        private int _isSprintingHash;
        private int _isCrouchingHash;
        private int _isAimingHash;
        private int _shootHash;
        private int _reloadHash;
        private int _meleeHash;
        private int _takeDamageHash;
        private int _isDeadHash;
        private int _interactHash;
        private int _jumpHash;
        private int _isGroundedHash;

        private float _smoothSpeed;
        private const float SPEED_DAMP_TIME = 0.1f;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator != null && _animator.runtimeAnimatorController == null)
            {
                _animator = null; // No controller assigned — disable animation calls
            }
            if (_animator != null)
                CacheParameterHashes();
        }

        #endregion

        #region Initialization

        private void CacheParameterHashes()
        {
            _speedHash = Animator.StringToHash(Constants.AnimParams.SPEED);
            _isSprintingHash = Animator.StringToHash(Constants.AnimParams.IS_SPRINTING);
            _isCrouchingHash = Animator.StringToHash(Constants.AnimParams.IS_CROUCHING);
            _isAimingHash = Animator.StringToHash(Constants.AnimParams.IS_AIMING);
            _shootHash = Animator.StringToHash(Constants.AnimParams.SHOOT);
            _reloadHash = Animator.StringToHash(Constants.AnimParams.RELOAD);
            _meleeHash = Animator.StringToHash(Constants.AnimParams.MELEE);
            _takeDamageHash = Animator.StringToHash(Constants.AnimParams.TAKE_DAMAGE);
            _isDeadHash = Animator.StringToHash(Constants.AnimParams.IS_DEAD);
            _interactHash = Animator.StringToHash(Constants.AnimParams.INTERACT);
            _jumpHash = Animator.StringToHash(Constants.AnimParams.JUMP);
            _isGroundedHash = Animator.StringToHash(Constants.AnimParams.IS_GROUNDED);
        }

        #endregion

        #region Movement

        /// <summary>
        /// Sets the movement speed parameter (0 = idle, 1 = walk, 1.5+ = sprint).
        /// </summary>
        public void SetMovement(float speedFactor)
        {
            if (_animator == null) return;
            _smoothSpeed = Mathf.Lerp(_smoothSpeed, speedFactor, Time.deltaTime / SPEED_DAMP_TIME);
            _animator.SetFloat(_speedHash, _smoothSpeed);
        }

        /// <summary>Sets the sprinting state.</summary>
        public void SetSprinting(bool isSprinting)
        {
            if (_animator == null) return;
            _animator.SetBool(_isSprintingHash, isSprinting);
        }

        /// <summary>Sets the crouching state.</summary>
        public void SetCrouching(bool isCrouching)
        {
            if (_animator == null) return;
            _animator.SetBool(_isCrouchingHash, isCrouching);
        }

        /// <summary>Sets the grounded state for jump/fall transitions.</summary>
        public void SetGrounded(bool isGrounded)
        {
            if (_animator == null) return;
            _animator.SetBool(_isGroundedHash, isGrounded);
        }

        /// <summary>Triggers the jump animation.</summary>
        public void TriggerJump()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_jumpHash);
        }

        #endregion

        #region Combat

        /// <summary>Sets the aiming state.</summary>
        public void SetAiming(bool isAiming)
        {
            if (_animator == null) return;
            _animator.SetBool(_isAimingHash, isAiming);
        }

        /// <summary>Triggers the shoot animation.</summary>
        public void TriggerShoot()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_shootHash);
        }

        /// <summary>Triggers the reload animation.</summary>
        public void TriggerReload()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_reloadHash);
        }

        /// <summary>Triggers the melee attack animation.</summary>
        public void TriggerMelee()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_meleeHash);
        }

        #endregion

        #region Damage & Death

        /// <summary>Triggers the take damage animation.</summary>
        public void TriggerDamage()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_takeDamageHash);
        }

        /// <summary>Sets the death state (stays in death pose).</summary>
        public void SetDead(bool isDead)
        {
            if (_animator == null) return;
            _animator.SetBool(_isDeadHash, isDead);
        }

        #endregion

        #region Interaction

        /// <summary>Triggers the interaction animation (pick up, open door).</summary>
        public void TriggerInteract()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_interactHash);
        }

        #endregion
    }
}
