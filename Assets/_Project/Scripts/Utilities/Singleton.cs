// ============================================================
// File:        Singleton.cs
// Namespace:   TWD.Utilities
// Description: Generic singleton base class for MonoBehaviours.
//              Survives scene loads via DontDestroyOnLoad.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;

namespace TWD.Utilities
{
    /// <summary>
    /// Generic singleton base class. Inherit from this to create
    /// a manager that persists across scenes with global access.
    /// Usage: public class GameManager : Singleton&lt;GameManager&gt; { }
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} requested after application quit.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindAnyObjectByType<T>();

                        if (_instance == null)
                        {
                            Debug.LogError($"[Singleton] No instance of {typeof(T)} found in scene!");
                        }
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Whether this singleton has been initialized.
        /// </summary>
        public static bool IsInitialized => _instance != null;

        protected virtual void Awake()
        {
            _isQuitting = false;

            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T)} destroyed on {gameObject.name}.");
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnSingletonAwake();
        }

        /// <summary>
        /// Override this instead of Awake() in derived classes.
        /// Called only on the surviving singleton instance.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
