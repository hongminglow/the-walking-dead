// ============================================================
// File:        Trap.cs
// Namespace:   TWD.Environment
// Description: Environmental trap that damages the player on
//              trigger entry. Supports one-time and repeating traps.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Combat;
using TWD.Utilities;

namespace TWD.Environment
{
    /// <summary>
    /// Environmental trap. Damages entities entering its trigger zone.
    /// Can be configured as one-shot or repeating with cooldown.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Trap : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Damage")]
        [SerializeField] private float _damage = 20f;
        [SerializeField] private bool _oneTimeUse = true;
        [SerializeField] private float _cooldown = 2f;

        [Header("Visual / Audio")]
        [SerializeField] private AudioClip _triggerSound;
        [SerializeField] private GameObject _triggerVFX;
        [SerializeField] private Animator _animator;

        #endregion

        #region Private Fields

        private float _cooldownTimer;
        private bool _isUsed;
        private AudioSource _audioSource;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            GetComponent<Collider>().isTrigger = true;
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        #endregion

        #region Trigger

        private void OnTriggerEnter(Collider other)
        {
            if (_isUsed || _cooldownTimer > 0f) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                ActivateTrap(damageable, other.transform.position);
            }
        }

        private void ActivateTrap(IDamageable target, Vector3 position)
        {
            target.TakeDamage(_damage, position);

            // Effects
            if (_audioSource != null && _triggerSound != null)
                _audioSource.PlayOneShot(_triggerSound);

            if (_triggerVFX != null)
                Instantiate(_triggerVFX, transform.position, Quaternion.identity);

            if (_animator != null)
                _animator.SetTrigger("Activate");

            if (_oneTimeUse)
            {
                _isUsed = true;
                // Optionally disable the trap visually
            }
            else
            {
                _cooldownTimer = _cooldown;
            }

            Debug.Log($"[Trap] Activated! Dealt {_damage} damage.");
        }

        #endregion
    }
}
