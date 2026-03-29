// ============================================================
// File:        SceneLoader.cs
// Namespace:   TWD.Core
// Description: Async scene loading with loading screen support.
//              Handles scene transitions smoothly.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Manages scene transitions with async loading and progress tracking.
    /// Shows a loading screen between levels.
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        [Header("Settings")]
        [SerializeField] private float _minimumLoadTime = 1.5f;
        [SerializeField] private string _loadingSceneName = Constants.Scenes.LOADING;

        private bool _isLoading = false;

        /// <summary>Whether a scene is currently being loaded.</summary>
        public bool IsLoading => _isLoading;

        /// <summary>Fires during loading with progress 0-1.</summary>
        public event Action<float> OnLoadProgress;

        /// <summary>Fires when loading is complete.</summary>
        public event Action OnLoadComplete;

        /// <summary>
        /// Loads a scene asynchronously with a loading screen.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load (use Constants.Scenes).</param>
        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SceneLoader] Already loading a scene!");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        /// <summary>
        /// Loads a scene directly without loading screen (for menus).
        /// </summary>
        public void LoadSceneDirect(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Reloads the current active scene (for retry on death).
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Gets the name of the currently active scene.
        /// </summary>
        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;
            float startTime = Time.unscaledTime;

            // 1. Load the loading screen first
            yield return SceneManager.LoadSceneAsync(_loadingSceneName);

            // 2. Start loading the target scene in background
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // 3. Track progress
            while (!asyncLoad.isDone)
            {
                // Unity reports progress 0 to 0.9, then pauses until allowSceneActivation
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnLoadProgress?.Invoke(progress);

                // When Unity is ready (0.9 = done loading assets)
                if (asyncLoad.progress >= 0.9f)
                {
                    // Enforce minimum load time so screen doesn't flash
                    float elapsed = Time.unscaledTime - startTime;
                    if (elapsed < _minimumLoadTime)
                    {
                        yield return new WaitForSecondsRealtime(_minimumLoadTime - elapsed);
                    }

                    OnLoadProgress?.Invoke(1f);
                    yield return new WaitForSecondsRealtime(0.3f); // Brief pause at 100%

                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            _isLoading = false;
            OnLoadComplete?.Invoke();
            Debug.Log($"[SceneLoader] Loaded scene: {sceneName}");
        }
    }
}
