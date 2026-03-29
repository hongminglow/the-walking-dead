// ============================================================
// File:        Extensions.cs
// Namespace:   TWD.Utilities
// Description: Extension methods for common Unity operations.
//              Reduces boilerplate across the codebase.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;

namespace TWD.Utilities
{
    /// <summary>
    /// Handy extension methods used throughout the project.
    /// </summary>
    public static class Extensions
    {
        #region Transform Extensions

        /// <summary>
        /// Resets position, rotation, and scale to defaults.
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Sets only the Y position (useful for grounding).
        /// </summary>
        public static void SetY(this Transform transform, float y)
        {
            var pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }

        /// <summary>
        /// Checks if this transform can "see" the target within an angle and range.
        /// Used for enemy AI sight cones.
        /// </summary>
        public static bool IsInSightCone(this Transform observer, Transform target, float maxAngle, float maxRange)
        {
            Vector3 directionToTarget = target.position - observer.position;
            float distance = directionToTarget.magnitude;

            if (distance > maxRange) return false;

            float angle = Vector3.Angle(observer.forward, directionToTarget);
            return angle <= maxAngle * 0.5f;
        }

        #endregion

        #region Vector3 Extensions

        /// <summary>
        /// Returns the vector with Y set to 0. Useful for horizontal distance checks.
        /// </summary>
        public static Vector3 Flat(this Vector3 vector)
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        /// <summary>
        /// Returns horizontal distance between two points (ignoring Y).
        /// </summary>
        public static float FlatDistance(this Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from.Flat(), to.Flat());
        }

        #endregion

        #region GameObject Extensions

        /// <summary>
        /// Checks if gameObject has a specific component and returns it.
        /// Cleaner than GetComponent + null check.
        /// </summary>
        public static bool Has<T>(this GameObject go, out T component) where T : Component
        {
            return go.TryGetComponent(out component);
        }

        /// <summary>
        /// Sets the layer of this object and all children recursively.
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        #endregion

        #region Float Extensions

        /// <summary>
        /// Remaps a value from one range to another.
        /// Example: 50f.Remap(0, 100, 0, 1) returns 0.5f
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        /// <summary>
        /// Returns true if value is approximately zero.
        /// </summary>
        public static bool IsNearZero(this float value, float threshold = 0.01f)
        {
            return Mathf.Abs(value) < threshold;
        }

        #endregion

        #region Component Extensions

        /// <summary>
        /// Gets or adds a component. Never returns null.
        /// </summary>
        public static T GetOrAdd<T>(this GameObject go) where T : Component
        {
            if (!go.TryGetComponent<T>(out var component))
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        #endregion
    }
}
