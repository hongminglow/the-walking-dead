// ============================================================
// File:        DamageSystem.cs
// Namespace:   TWD.Combat
// Description: Utility class for processing damage calculations
//              including headshots, critical hits, and falloff.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Utilities;

namespace TWD.Combat
{
    /// <summary>
    /// Static utility for damage calculations.
    /// </summary>
    public static class DamageSystem
    {
        /// <summary>
        /// Calculates final damage taking headshot multiplier into account.
        /// </summary>
        /// <param name="baseDamage">Base weapon damage.</param>
        /// <param name="isHeadshot">Whether the hit was a headshot.</param>
        /// <param name="headshotMultiplier">Headshot damage multiplier.</param>
        /// <returns>Final damage value.</returns>
        public static float CalculateDamage(float baseDamage, bool isHeadshot, float headshotMultiplier = 2.5f)
        {
            float damage = baseDamage;

            if (isHeadshot)
            {
                damage *= headshotMultiplier;
            }

            return damage;
        }

        /// <summary>
        /// Calculates damage with distance falloff (for shotguns).
        /// </summary>
        /// <param name="baseDamage">Base weapon damage.</param>
        /// <param name="distance">Distance to target.</param>
        /// <param name="maxRange">Maximum effective range.</param>
        /// <param name="falloffStart">Distance at which falloff begins (fraction of max range).</param>
        /// <returns>Damage after distance falloff.</returns>
        public static float CalculateDamageWithFalloff(float baseDamage, float distance, float maxRange, float falloffStart = 0.5f)
        {
            if (distance > maxRange) return 0f;

            float falloffDistance = maxRange * falloffStart;
            if (distance <= falloffDistance) return baseDamage;

            float falloffFactor = 1f - ((distance - falloffDistance) / (maxRange - falloffDistance));
            return baseDamage * Mathf.Max(falloffFactor, 0.1f); // Min 10% damage
        }

        /// <summary>
        /// Determines if a hit should cause the target to stagger.
        /// </summary>
        /// <param name="damage">Damage dealt.</param>
        /// <param name="staggerThreshold">Minimum damage to cause stagger.</param>
        /// <returns>True if stagger should occur.</returns>
        public static bool ShouldStagger(float damage, float staggerThreshold = 30f)
        {
            return damage >= staggerThreshold;
        }
    }
}
