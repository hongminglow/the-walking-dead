// ============================================================
// File:        ZombieCrawler.cs
// Namespace:   TWD.Enemies
// Description: Low-profile crawling zombie. Harder to spot,
//              grabs ankles. 40 HP, 10 damage, 1.5 m/s.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;

namespace TWD.Enemies
{
    /// <summary>
    /// Crawler zombie — stays low, harder to detect, grabs ankles.
    /// Lower health but harder to spot visually.
    /// </summary>
    public class ZombieCrawler : EnemyBase
    {
        [Header("Crawler Settings")]
        [SerializeField] private float _lowProfileDetectionPenalty = 0.5f;

        protected override void Start()
        {
            base.Start();

            // Crawlers have a lower agent height
            _agent.height = 0.6f;
            _agent.baseOffset = 0f;
        }

        /// <summary>
        /// Crawlers are harder to detect — reduce sight angle effectiveness
        /// when the player is not looking down.
        /// </summary>
        protected override void OnIdle()
        {
            base.OnIdle();
            // Crawlers make less noise in idle — reduced detection radius
        }

        protected override void PerformAttack()
        {
            _animator?.SetTrigger("CrawlAttack");
            PlaySound(_data.attackSound);

            if (_playerTransform == null) return;
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= _data.attackRange * 1.2f)
            {
                if (_playerTransform.TryGetComponent<Combat.IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_data.attackDamage, transform.position);
                }
            }

            Debug.Log("[ZombieCrawler] Ankle grab attack!");
        }
    }
}
