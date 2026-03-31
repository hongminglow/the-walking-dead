// ============================================================
// File:        PlayerHealth.cs
// Namespace:   TWD.Player
// Description: Player health system with visual feedback tiers.
//              Implements IDamageable for unified damage handling.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
        [SerializeField] private float _criticalSpeedMultiplier = 0.6f;

        [Header("Audio")]
        [SerializeField] private AudioClip _hurtSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _healSound;
        [SerializeField] private AudioClip _heartbeatSound;

        [Header("Post-Processing")]
        [SerializeField] private Volume _postProcessVolume;

        #endregion

        #region Private Fields

        private float _currentHealth;
        private HealthStatus _currentStatus = HealthStatus.Fine;
        private float _invincibilityTimer;
        private bool _isDead;
        private AudioSource _audioSource;
        private AudioSource _heartbeatSource;
        private PlayerController _controller;
        private float _damageFlashTimer;
        private Vignette _vignette;
        private ColorAdjustments _colorAdjustments;
        private float _baseVignetteIntensity;

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

        /// <summary>Speed multiplier based on health status (1.0 normal, 0.6 critical).</summary>
        public float SpeedMultiplier => _currentStatus == HealthStatus.Critical ? _criticalSpeedMultiplier : 1f;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _controller = GetComponent<PlayerController>();
            _currentHealth = _maxHealth;

            // Create dedicated heartbeat audio source
            var hbGo = new GameObject("HeartbeatSource");
            hbGo.transform.SetParent(transform);
            _heartbeatSource = hbGo.AddComponent<AudioSource>();
            _heartbeatSource.loop = true;
            _heartbeatSource.playOnAwake = false;
            _heartbeatSource.spatialBlend = 0f;
            _heartbeatSource.volume = 0.6f;

            UpdateHealthStatus();
        }

        private void Start()
        {
            // Find post-process volume in scene if not assigned
            if (_postProcessVolume == null)
                _postProcessVolume = FindFirstObjectByType<Volume>();

            if (_postProcessVolume != null && _postProcessVolume.profile != null)
            {
                _postProcessVolume.profile.TryGet(out _vignette);
                _postProcessVolume.profile.TryGet(out _colorAdjustments);
                if (_vignette != null)
                    _baseVignetteIntensity = _vignette.intensity.value;
            }
        }

        private void Update()
        {
            if (_invincibilityTimer > 0f)
                _invincibilityTimer -= Time.deltaTime;

            // Damage flash fade-out (blood splatter effect)
            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer -= Time.deltaTime;
                UpdateDamageVignette();
            }

            // Continuous health-based vignette
            UpdateHealthVignette();
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

            // Blood splatter / damage flash
            _damageFlashTimer = _damageScreenDuration;

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
                    StopHeartbeat();
                    SetVignetteColor(Color.black);
                    break;

                case HealthStatus.Hurt:
                    StopHeartbeat();
                    // Red tint on screen edges
                    SetVignetteColor(new Color(0.5f, 0f, 0f));
                    break;

                case HealthStatus.Critical:
                    // Heavy vignette + heartbeat SFX + slow movement
                    StartHeartbeat();
                    SetVignetteColor(new Color(0.7f, 0f, 0f));
                    break;

                case HealthStatus.Dead:
                    StopHeartbeat();
                    break;
            }
        }

        #endregion

        #region Visual Feedback

        private void UpdateHealthVignette()
        {
            if (_vignette == null) return;

            float healthPct = _currentHealth / _maxHealth;
            // Vignette increases as health drops: 0.25 at full → 0.55 at critical
            float targetIntensity = Mathf.Lerp(0.55f, _baseVignetteIntensity, healthPct);

            // Add damage flash on top
            if (_damageFlashTimer > 0f)
            {
                float flashPct = _damageFlashTimer / _damageScreenDuration;
                targetIntensity = Mathf.Max(targetIntensity, 0.6f * flashPct);
            }

            _vignette.intensity.value = targetIntensity;
        }

        private void UpdateDamageVignette()
        {
            // Red color tint fades out after taking damage
            if (_colorAdjustments != null && _damageFlashTimer > 0f)
            {
                float flashPct = _damageFlashTimer / _damageScreenDuration;
                _colorAdjustments.colorFilter.value = Color.Lerp(Color.white, new Color(1f, 0.7f, 0.7f), flashPct * 0.3f);
            }
            else if (_colorAdjustments != null)
            {
                _colorAdjustments.colorFilter.value = Color.white;
            }
        }

        private void SetVignetteColor(Color color)
        {
            if (_vignette != null)
                _vignette.color.value = color;
        }

        private void StartHeartbeat()
        {
            if (_heartbeatSource != null && _heartbeatSound != null && !_heartbeatSource.isPlaying)
            {
                _heartbeatSource.clip = _heartbeatSound;
                _heartbeatSource.Play();
            }
        }

        private void StopHeartbeat()
        {
            if (_heartbeatSource != null && _heartbeatSource.isPlaying)
                _heartbeatSource.Stop();
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
