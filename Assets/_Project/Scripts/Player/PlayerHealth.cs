// ============================================================
// File:        PlayerHealth.cs
// Namespace:   TWD.Player
// Description: Player health system with visual feedback tiers.
//              Implements IDamageable for unified damage handling.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Combat;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Player
{
    /// <summary>
    /// Manages player health, damage, healing, death, and
    /// health status tiers that drive visual/audio feedback.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        #region Serialized Fields

        [Header("Health")]
        [SerializeField] private float _maxHealth = Constants.Player.MAX_HEALTH;

        [Header("Damage Feedback")]
        [SerializeField] private float _invincibilityDuration = 0.5f;
        [SerializeField] private float _damageScreenDuration = 2f;

        [Header("Audio")]
        [SerializeField] private AudioClip _hurtSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _healSound;
        [SerializeField] private AudioClip _heartbeatSound;

        #endregion

        #region Private Fields

        private float _currentHealth;
        private HealthStatus _currentStatus = HealthStatus.Fine;
        private float _invincibilityTimer;
        private bool _isDead;
        private AudioSource _audioSource;
        private PlayerController _controller;

        #endregion

        #region IDamageable Implementation

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsAlive => !_isDead;

        #endregion

        #region Properties

        /// <summary>Current health status tier.</summary>
        public HealthStatus Status => _currentStatus;

        /// <summary>Health as a 0-1 normalized value.</summary>
        public float HealthNormalized => _currentHealth / _maxHealth;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _controller = GetComponent<PlayerController>();
            _currentHealth = _maxHealth;
            UpdateHealthStatus();
        }

        private void Update()
        {
            if (_invincibilityTimer > 0f)
            {
                _invincibilityTimer -= Time.deltaTime;
            }
        }

        #endregion

        #region IDamageable Methods

        /// <summary>
        /// Applies damage to the player. Triggers visual and audio feedback.
        /// </summary>
        public void TakeDamage(float amount, Vector3 hitPoint, bool isHeadshot = false)
        {
            if (_isDead || _invincibilityTimer > 0f) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            _invincibilityTimer = _invincibilityDuration;

            EventBus.PlayerHealthChanged(_currentHealth);
            EventBus.PlayerDamaged(amount);
            UpdateHealthStatus();

            // Audio feedback
            if (_audioSource != null && _hurtSound != null)
            {
                _audioSource.PlayOneShot(_hurtSound);
            }

            if (_currentHealth <= 0f)
            {
                Die();
            }

            Debug.Log($"[PlayerHealth] Took {amount} damage. Health: {_currentHealth}/{_maxHealth}");
        }

        /// <summary>
        /// Heals the player. Cannot exceed max health.
        /// </summary>
        public void Heal(float amount)
        {
            if (_isDead) return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);

            float actualHeal = _currentHealth - previousHealth;
            if (actualHeal > 0f)
            {
                EventBus.PlayerHealthChanged(_currentHealth);
                EventBus.PlayerHealed(actualHeal);
                UpdateHealthStatus();

                if (_audioSource != null && _healSound != null)
                {
                    _audioSource.PlayOneShot(_healSound);
                }

                Debug.Log($"[PlayerHealth] Healed {actualHeal}. Health: {_currentHealth}/{_maxHealth}");
            }
        }

        #endregion

        #region Health Status

        private void UpdateHealthStatus()
        {
            HealthStatus newStatus;

            if (_currentHealth <= 0f)
                newStatus = HealthStatus.Dead;
            else if (_currentHealth <= Constants.Player.HEALTH_HURT_THRESHOLD)
                newStatus = HealthStatus.Critical;
            else if (_currentHealth <= Constants.Player.HEALTH_FINE_THRESHOLD)
                newStatus = HealthStatus.Hurt;
            else
                newStatus = HealthStatus.Fine;

            if (newStatus != _currentStatus)
            {
                _currentStatus = newStatus;
                OnHealthStatusChanged(newStatus);
            }
        }

        private void OnHealthStatusChanged(HealthStatus status)
        {
            switch (status)
            {
                case HealthStatus.Fine:
                    // Normal: no effects
                    break;

                case HealthStatus.Hurt:
                    // Red tint on screen edges, slight limp
                    Debug.Log("[PlayerHealth] Status: HURT — screen tint active.");
                    break;

                case HealthStatus.Critical:
                    // Heavy vignette, heartbeat SFX, slow movement
                    Debug.Log("[PlayerHealth] Status: CRITICAL — heartbeat SFX active.");
                    break;

                case HealthStatus.Dead:
                    // Handled by Die()
                    break;
            }
        }

        #endregion

        #region Death

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            Debug.Log("[PlayerHealth] Player died!");

            // Disable input
            _controller?.SetInputEnabled(false);
            _controller?.StopMovement();

            // Play death sound
            if (_audioSource != null && _deathSound != null)
            {
                _audioSource.PlayOneShot(_deathSound);
            }

            // Notify systems
            EventBus.PlayerDied();

            // GameManager handles the game over screen
            GameManager.Instance.TriggerGameOver();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fully restores health to max. Used on new game / load.
        /// </summary>
        public void RestoreFullHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            _invincibilityTimer = 0f;
            UpdateHealthStatus();
            EventBus.PlayerHealthChanged(_currentHealth);
        }

        /// <summary>
        /// Sets health to a specific value. Used by save/load system.
        /// </summary>
        public void SetHealth(float health)
        {
            _currentHealth = Mathf.Clamp(health, 0f, _maxHealth);
            _isDead = _currentHealth <= 0f;
            UpdateHealthStatus();
            EventBus.PlayerHealthChanged(_currentHealth);
        }

        #endregion
    }
}
