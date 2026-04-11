// ============================================================
// File:        PauseMenu.cs
// Namespace:   TWD.UI
// Description: Pause menu UI controller. Handles resume, settings,
//              save, load, and quit menu functionality.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TWD.Core;
using TWD.Utilities;

namespace TWD.UI
{
    /// <summary>
    /// Pause menu controller. Toggles on Escape/Start input.
    /// Manages menu button actions: resume, save, load, quit.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private const string RuntimeRootName = "[PauseMenuRuntime]";

        #region Serialized Fields

        [Header("UI References")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Text _statusText;

        #endregion

        #region Lifecycle

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsurePauseMenuForScene(scene);
        }

        private static void EnsurePauseMenuForScene(Scene scene)
        {
            string sceneName = scene.name;
            if (string.Equals(sceneName, Constants.Scenes.MAIN_MENU, StringComparison.Ordinal) ||
                string.Equals(sceneName, Constants.Scenes.LOADING, StringComparison.Ordinal))
            {
                return;
            }

            PauseMenu pauseMenu = FindScenePauseMenu(scene);
            if (pauseMenu == null)
            {
                GameObject runtimeRoot = new GameObject(RuntimeRootName, typeof(RectTransform));
                pauseMenu = runtimeRoot.AddComponent<PauseMenu>();
                SceneManager.MoveGameObjectToScene(runtimeRoot, scene);
            }

            pauseMenu.PrepareForRuntime();
        }

        private static PauseMenu FindScenePauseMenu(Scene scene)
        {
            PauseMenu[] pauseMenus = Resources.FindObjectsOfTypeAll<PauseMenu>();
            for (int i = 0; i < pauseMenus.Length; i++)
            {
                PauseMenu pauseMenu = pauseMenus[i];
                if (pauseMenu == null)
                {
                    continue;
                }

                GameObject pauseObject = pauseMenu.gameObject;
                if (!pauseObject.scene.IsValid() || pauseObject.scene != scene)
                {
                    continue;
                }

                return pauseMenu;
            }

            return null;
        }

        private void Awake()
        {
            if (gameObject.scene.IsValid())
            {
                PrepareForRuntime();
            }
        }

        private void Start()
        {
            HidePanels();
        }

        private void Update()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        #endregion

        #region Input Callbacks

        /// <summary>Called by PlayerInput for Pause action.</summary>
        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            TogglePause();
        }

        #endregion

        #region Pause Logic

        /// <summary>Toggles the pause menu on/off.</summary>
        public void TogglePause()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (GameManager.Instance.IsPaused)
            {
                Resume();
            }
            else if (GameManager.Instance.IsPlaying)
            {
                Pause();
            }
        }

        private void Pause()
        {
            PrepareForRuntime();

            if (_pausePanel == null)
            {
                Debug.LogWarning("[PauseMenu] Pause panel is not configured in this scene.");
                return;
            }

            RefreshRuntimeState();
            _pausePanel?.SetActive(true);
            GameManager.Instance.TogglePause();
        }

        /// <summary>Resumes gameplay from pause.</summary>
        public void Resume()
        {
            HidePanels();

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                GameManager.Instance.TogglePause();
            }
        }

        #endregion

        #region Menu Actions

        /// <summary>Opens the settings sub-panel.</summary>
        public void OpenSettings()
        {
            _settingsPanel?.SetActive(true);
        }

        /// <summary>Closes settings and returns to pause menu.</summary>
        public void CloseSettings()
        {
            _settingsPanel?.SetActive(false);
        }

        /// <summary>Saves the current game.</summary>
        public void SaveGame()
        {
            if (!SaveManager.IsInitialized || SaveManager.Instance == null)
            {
                Debug.LogWarning("[PauseMenu] SaveManager is not available.");
                return;
            }

            SaveManager.Instance.Save(0);
            RefreshRuntimeState("Game saved to slot 0.");
            Debug.Log("[PauseMenu] Game saved to slot 0.");
        }

        /// <summary>Loads the most recent save.</summary>
        public void LoadGame()
        {
            if (!SaveManager.IsInitialized || SaveManager.Instance == null)
            {
                Debug.LogWarning("[PauseMenu] SaveManager is not available.");
                return;
            }

            if (!SaveManager.Instance.HasAnySave())
            {
                RefreshRuntimeState("No save file found.");
                Debug.LogWarning("[PauseMenu] No save file found.");
                return;
            }

            Resume();
            SaveManager.Instance.Load(0);
            Debug.Log("[PauseMenu] Game loaded from slot 0.");
        }

        /// <summary>Returns to the main menu.</summary>
        public void QuitToMainMenu()
        {
            Resume();
            GameManager.Instance.ReturnToMainMenu();
        }

        /// <summary>Quits the application.</summary>
        public void QuitGame()
        {
            GameManager.Instance.QuitGame();
        }

        #endregion

        #region Runtime UI

        private void PrepareForRuntime()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            EnsureCanvasScaffold();
            EnsureRuntimePanels();
            EnsureEventSystemExists();
            HidePanels();
            RefreshRuntimeState();
        }

        private void EnsureCanvasScaffold()
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void EnsureRuntimePanels()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (_pausePanel == null)
            {
                _pausePanel = CreateFullscreenPanel("PausePanel", new Color(0.02f, 0.02f, 0.03f, 0.82f));
                GameObject card = CreateCard("PauseCard", _pausePanel.transform, new Vector2(520f, 420f));

                CreateLabel("PAUSED", 34, FontStyle.Bold, card.transform, font, new Color(0.93f, 0.89f, 0.82f, 1f));
                _statusText = CreateLabel("Hold yourself together and choose your next move.", 15, FontStyle.Normal, card.transform, font, new Color(0.75f, 0.74f, 0.72f, 1f));

                CreateActionButton("ResumeButton", "Resume", card.transform, font, Resume);
                CreateActionButton("SaveButton", "Save Game", card.transform, font, SaveGame);
                CreateActionButton("LoadButton", "Load Last Save", card.transform, font, LoadGame);
                CreateActionButton("SettingsButton", "Settings", card.transform, font, OpenSettings);
                CreateActionButton("MainMenuButton", "Main Menu", card.transform, font, QuitToMainMenu);
                CreateActionButton("QuitButton", "Quit Desktop", card.transform, font, QuitGame);
            }

            if (_settingsPanel == null)
            {
                _settingsPanel = CreateFullscreenPanel("SettingsPanel", new Color(0.02f, 0.02f, 0.03f, 0.82f));
                GameObject card = CreateCard("SettingsCard", _settingsPanel.transform, new Vector2(500f, 260f));

                CreateLabel("SETTINGS", 30, FontStyle.Bold, card.transform, font, new Color(0.9f, 0.85f, 0.8f, 1f));
                CreateLabel("Runtime fallback settings panel.\nAudio and sensitivity tuning can be added here later.", 15, FontStyle.Normal, card.transform, font, new Color(0.76f, 0.74f, 0.71f, 1f));
                CreateActionButton("BackButton", "Back", card.transform, font, CloseSettings);
            }
        }

        private GameObject CreateFullscreenPanel(string panelName, Color tint)
        {
            GameObject panel = CreateUIObject(panelName, transform);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = tint;

            panel.SetActive(false);
            return panel;
        }

        private GameObject CreateCard(string name, Transform parent, Vector2 size)
        {
            GameObject card = CreateUIObject(name, parent);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Image image = card.AddComponent<Image>();
            image.color = new Color(0.09f, 0.08f, 0.09f, 0.96f);

            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 28, 28);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = card.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return card;
        }

        private Text CreateLabel(string text, int fontSize, FontStyle fontStyle, Transform parent, Font font, Color color)
        {
            GameObject labelObject = CreateUIObject(text.Replace(" ", string.Empty) + "Text", parent);
            Text label = labelObject.AddComponent<Text>();
            label.text = text;
            label.font = font;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            LayoutElement layout = labelObject.AddComponent<LayoutElement>();
            layout.preferredHeight = Mathf.Max(32f, fontSize * 1.8f);

            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, layout.preferredHeight);

            return label;
        }

        private Button CreateActionButton(string name, string label, Transform parent, Font font, Action onClick)
        {
            GameObject buttonObject = CreateUIObject(name, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.18f, 0.18f, 0.96f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick?.Invoke());

            LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 44f;

            GameObject labelObject = CreateUIObject("Label", buttonObject.transform);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12f, 6f);
            labelRect.offsetMax = new Vector2(-12f, -6f);

            Text text = labelObject.AddComponent<Text>();
            text.text = label;
            text.font = font;
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.92f, 0.9f, 0.86f, 1f);

            return button;
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private void EnsureEventSystemExists()
        {
            EventSystem[] eventSystems = Resources.FindObjectsOfTypeAll<EventSystem>();
            for (int i = 0; i < eventSystems.Length; i++)
            {
                EventSystem existingEventSystem = eventSystems[i];
                if (existingEventSystem == null || existingEventSystem.gameObject == null)
                {
                    continue;
                }

                if (existingEventSystem.gameObject.scene.IsValid() || existingEventSystem.gameObject.activeInHierarchy)
                {
                    return;
                }
            }

            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject runtimeEventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            if (gameObject.scene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(runtimeEventSystem, gameObject.scene);
            }
        }

        private void HidePanels()
        {
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(false);
            }

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }
        }

        private void RefreshRuntimeState(string statusMessage = null)
        {
            if (_pausePanel == null)
            {
                return;
            }

            Button[] buttons = _pausePanel.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                if (string.Equals(button.gameObject.name, "LoadButton", StringComparison.Ordinal))
                {
                    button.interactable = SaveManager.IsInitialized &&
                                         SaveManager.Instance != null &&
                                         SaveManager.Instance.HasAnySave();
                }
            }

            if (string.IsNullOrWhiteSpace(statusMessage))
            {
                return;
            }

            if (_statusText == null)
            {
                Text[] texts = _pausePanel.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    Text text = texts[i];
                    if (text != null && !string.Equals(text.text, "PAUSED", StringComparison.Ordinal))
                    {
                        _statusText = text;
                        break;
                    }
                }
            }

            if (_statusText != null)
            {
                _statusText.text = statusMessage;
            }
        }

        #endregion
    }
}
