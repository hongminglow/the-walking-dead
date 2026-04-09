// ============================================================
// File:        GameManager.cs
// Namespace:   TWD.Core
// Description: Top-level game manager. Controls game state,
//              scene flow, pause, and acts as the central hub.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Central game manager singleton. Persists across all scenes.
    /// Controls game state transitions, pause, and coordinates
    /// other manager systems.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;

        private GameState _previousState;
        private float _playTime;

        /// <summary>Raised whenever the game state changes. Params: previous, current.</summary>
        public event Action<GameState, GameState> OnStateChanged;

        // ================================================================
        // Properties
        // ================================================================

        /// <summary>Current game state.</summary>
        public GameState CurrentState => _currentState;

        /// <summary>Whether the game is currently paused.</summary>
        public bool IsPaused => _currentState == GameState.Paused;

        /// <summary>Whether gameplay input should be processed.</summary>
        public bool IsPlaying => _currentState == GameState.Playing;

        /// <summary>Total play time in seconds for current session.</summary>
        public float PlayTime => _playTime;

        // ================================================================
        // Lifecycle
        // ================================================================

        protected override void OnSingletonAwake()
        {
            Application.targetFrameRate = 60;
            SetState(GameState.MainMenu);
        }

        private void Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != Constants.Scenes.MAIN_MENU && _currentState == GameState.MainMenu)
            {
                SetState(GameState.Playing);
                Debug.Log($"[GameManager] Auto-started Playing for scene: {sceneName}");
            }
        }

        private void Update()
        {
            if (_currentState == GameState.Playing)
            {
                _playTime += Time.deltaTime;
            }
        }

        // ================================================================
        // State Management
        // ================================================================

        /// <summary>
        /// Transitions the game to a new state.
        /// Handles Time.timeScale and cursor visibility automatically.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            GameState oldState = _currentState;
            _previousState = _currentState;
            _currentState = newState;

            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    SetCursor(visible: true, locked: false);
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    SetCursor(visible: false, locked: true);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    SetCursor(visible: true, locked: false);
                    EventBus.PauseToggled(true);
                    break;

                case GameState.Inventory:
                    Time.timeScale = 0f;
                    SetCursor(visible: true, locked: false);
                    break;

                case GameState.Cutscene:
                    Time.timeScale = 1f;
                    SetCursor(visible: false, locked: true);
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    SetCursor(visible: true, locked: false);
                    break;

                case GameState.Loading:
                    Time.timeScale = 1f;
                    SetCursor(visible: false, locked: true);
                    break;
            }

            if (_showDebugInfo)
            {
                Debug.Log($"[GameManager] State: {oldState} → {newState}");
            }

            OnStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Toggles pause on/off. Only works during Playing state.
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
            else if (_currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
                EventBus.PauseToggled(false);
            }
        }

        /// <summary>
        /// Returns to the previous state (e.g., closing inventory).
        /// </summary>
        public void ReturnToPreviousState()
        {
            SetState(_previousState);
        }

        // ================================================================
        // Game Flow
        // ================================================================

        /// <summary>
        /// Starts a new game. Resets all state and loads Level 1.
        /// </summary>
        public void StartNewGame()
        {
            _playTime = 0f;
            EventBus.ClearAll(); // Clean slate
            SetState(GameState.Loading);
            SceneLoader.Instance.LoadScene(Constants.Scenes.LEVEL_01_HOUSE);
            Debug.Log("[GameManager] New game started. Loading Level 1.");
        }

        /// <summary>
        /// Returns to main menu. Cleans up events.
        /// </summary>
        public void ReturnToMainMenu()
        {
            EventBus.ClearAll();
            SetState(GameState.MainMenu);
            SceneLoader.Instance.LoadSceneDirect(Constants.Scenes.MAIN_MENU);
            Debug.Log("[GameManager] Returning to main menu.");
        }

        /// <summary>
        /// Called when a level is completed. Triggers transition.
        /// </summary>
        public void CompleteLevel(string nextSceneName)
        {
            EventBus.LevelCompleted(nextSceneName);
            SetState(GameState.Loading);
            SceneLoader.Instance.LoadScene(nextSceneName);
            Debug.Log($"[GameManager] Level complete. Next: {nextSceneName}");
        }

        /// <summary>
        /// Restarts the current gameplay scene through the loading flow.
        /// </summary>
        public void RestartCurrentLevel()
        {
            SetState(GameState.Loading);
            SceneLoader.Instance.ReloadCurrentScene();
            Debug.Log($"[GameManager] Restarting scene: {SceneLoader.Instance.CurrentSceneName}");
        }

        /// <summary>
        /// Attempts to continue from the latest save. Falls back to a fresh game if none exist.
        /// </summary>
        public void ContinueLatestOrStartNew()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.HasAnySave())
            {
                SaveManager.Instance.LoadLatest();
                return;
            }

            StartNewGame();
        }

        /// <summary>
        /// Triggers game over state.
        /// </summary>
        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);
            Debug.Log("[GameManager] Game Over triggered.");
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game.");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // ================================================================
        // Helpers
        // ================================================================

        private void SetCursor(bool visible, bool locked)
        {
            Cursor.visible = visible;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
