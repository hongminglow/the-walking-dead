// ============================================================
// File:        ZombieBrute.cs
// Namespace:   TWD.Enemies
// Description: Mini-boss zombie. Massive, slow, tanky. Has a
//              charge attack and ground smash. 300 HP, 40 damage.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Utilities;

namespace TWD.Enemies
{
    /// <summary>
    /// Brute zombie — tanky mini-boss with charge and smash attacks.
    /// 300 HP, slow but devastating. Used as final obstacle.
    /// </summary>
    public class ZombieBrute : EnemyBase
    {
        #region Serialized Fields

        [Header("Brute Settings")]
        [SerializeField] private float _chargeSpeed = 4f;
        [SerializeField] private float _chargeDamage = 50f;
        [SerializeField] private float _chargeDistance = 8f;
        [SerializeField] private float _chargeCooldown = 6f;
        [SerializeField] private float _smashRadius = 3f;
        [SerializeField] private float _smashDamage = 45f;

        #endregion

        #region Private Fields

        private float _chargeTimer;
        private bool _isCharging;

        #endregion

        #region Lifecycle

        protected override void Update()
        {
            base.Update();

            if (_chargeTimer > 0f)
                _chargeTimer -= Time.deltaTime;
        }

        #endregion

        #region State Overrides

        protected override void OnChase()
        {
            base.OnChase();

            // Try to charge if cooldown is up and player is at mid-range
            if (_chargeTimer <= 0f && _playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTransform.position);
                if (dist > _data.attackRange * 2f && dist <= _chargeDistance)
                {
                    StartCharge();
                }
            }
        }

        protected override void PerformAttack()
        {
            // Randomly choose between smash and regular attack
            if (Random.value < 0.4f)
            {
                PerformSmash();
            }
            else
            {
                base.PerformAttack();
            }
        }

        #endregion

        #region Charge

        private void StartCharge()
        {
            _isCharging = true;
            _chargeTimer = _chargeCooldown;
            _agent.speed = _chargeSpeed;

            _animator?.SetTrigger("Charge");

            Debug.Log("[ZombieBrute] CHARGE!");

            // Charge continues in OnChase via movement
            // End charge on hit or after short duration
            Invoke(nameof(EndCharge), 2f);
        }

        private void EndCharge()
        {
            _isCharging = false;
            _agent.speed = _data.chaseSpeed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isCharging) return;

            // Hit player during charge
            if (collision.gameObject.CompareTag(Constants.Tags.PLAYER))
            {
                if (collision.gameObject.TryGetComponent<Combat.IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_chargeDamage, transform.position);
                }
                EndCharge();
            }
        }

        #endregion

        #region Smash

        private void PerformSmash()
        {
            _animator?.SetTrigger("Smash");
            PlaySound(_data.attackSound);

            Debug.Log("[ZombieBrute] SMASH!");

            // AoE damage around the brute
            Collider[] hits = Physics.OverlapSphere(transform.position, _smashRadius, _playerLayer);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Combat.IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_smashDamage, transform.position);
                }
            }
        }

        #endregion

        #region Death Override

        protected override void Die()
        {
            // Brute has special death animation with roar
            _animator?.SetTrigger("Roar");
            base.Die();
        }

        #endregion
    }
}
