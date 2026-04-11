// ============================================================
// File:        Level01ReadabilityLight.cs
// Namespace:   TWD.Core
// Description: Adds a lightweight ceiling fill for Level 1 so
//              early playtests stay readable without scene-only setup.
// Author:      The Walking Dead Team
// Created:     2026-04-11
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Runtime readability helper for the first house scene.
    /// Keeps the onboarding slice visible while final authored props and
    /// lighting are still being tuned from playtest feedback.
    /// </summary>
    public static class Level01ReadabilityLight
    {
        private const string LightRootName = "[Level01ReadabilityLight]";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureForScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureForScene(scene);
        }

        private static void EnsureForScene(Scene scene)
        {
            if (!string.Equals(scene.name, Constants.Scenes.LEVEL_01_HOUSE, System.StringComparison.Ordinal))
            {
                return;
            }

            GameObject existing = GameObject.Find(LightRootName);
            if (existing != null)
            {
                return;
            }

            GameObject lightRoot = new GameObject(LightRootName);
            SceneManager.MoveGameObjectToScene(lightRoot, scene);

            Light fillLight = lightRoot.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.color = new Color(1f, 0.93f, 0.82f, 1f);
            fillLight.intensity = 2.8f;
            fillLight.range = 20f;
            fillLight.shadows = LightShadows.None;

            lightRoot.transform.position = new Vector3(10f, 2.85f, 8f);
        }
    }
}
