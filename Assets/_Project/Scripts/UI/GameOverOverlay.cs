// ============================================================
// File:        GameOverOverlay.cs
// Namespace:   TWD.UI
// Description: Runtime-built game over overlay with retry,
//              continue, and main menu actions for incomplete scenes.
// Author:      The Walking Dead Team
// Created:     2026-04-09
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TWD.Core;

namespace TWD.UI
{
    /// <summary>
    /// Provides a consistent game over screen across all scenes even when
    /// authored UI is not yet fully wired in the editor.
    /// </summary>
    public class GameOverOverlay : MonoBehaviour
    {
        private const string RootName = "[GameOverOverlay]";

        private GameObject _root;
        private Text _subtitleText;
        private Text _hintText;
        private Button _continueButton;
        private bool _isVisible;
        private float _lastRefreshTime;
        private float _cachedMusicVolume = -1f;
        private float _cachedAmbientVolume = -1f;
        private float _cachedSfxVolume = -1f;
        private bool _isSubscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<GameOverOverlay>() != null)
                return;

            GameObject overlay = new GameObject(RootName, typeof(RectTransform));
            DontDestroyOnLoad(overlay);
            overlay.AddComponent<GameOverOverlay>();
        }

        private void Awake()
        {
            if (FindObjectsByType<GameOverOverlay>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            BuildOverlay();
            EnsureEventSystemExists();
        }

        private void OnEnable()
        {
            EnsureGameManagerSubscription();
        }

        private void Start()
        {
            EnsureGameManagerSubscription();
            RefreshVisibility(force: true);
        }

        private void OnDisable()
        {
            if (_isSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
                _isSubscribed = false;
            }
        }

        private void Update()
        {
            EnsureGameManagerSubscription();

            if (GameManager.Instance == null)
                return;

            if (_isVisible)
            {
                if (Time.unscaledTime - _lastRefreshTime >= 0.25f)
                {
                    UpdateSummary();
                    _lastRefreshTime = Time.unscaledTime;
                }

                Keyboard keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.rKey.wasPressedThisFrame)
                        RetryCurrentScene();
                    else if (keyboard.mKey.wasPressedThisFrame)
                        ReturnToMainMenu();
                    else if (keyboard.cKey.wasPressedThisFrame && _continueButton != null && _continueButton.gameObject.activeSelf)
                        ContinueLatestSave();
                }
            }
            else if (_root != null && _root.activeSelf)
            {
                RefreshVisibility(force: true);
            }
        }

        private void OnGameStateChanged(TWD.Utilities.GameState previous, TWD.Utilities.GameState current)
        {
            RefreshVisibility(force: true);
        }

        private void RefreshVisibility(bool force = false)
        {
            bool shouldShow = GameManager.Instance != null &&
                              GameManager.Instance.CurrentState == TWD.Utilities.GameState.GameOver;

            if (!force && shouldShow == _isVisible)
                return;

            _isVisible = shouldShow;

            if (_root != null)
                _root.SetActive(_isVisible);

            if (_isVisible)
            {
                UpdateSummary();
                ApplyAudioDuck();
            }
            else
            {
                RestoreAudioLevels();
            }
        }

        private void RetryCurrentScene()
        {
            RestoreAudioLevels();
            GameManager.Instance.RestartCurrentLevel();
        }

        private void ContinueLatestSave()
        {
            RestoreAudioLevels();
            GameManager.Instance.ContinueLatestOrStartNew();
        }

        private void ReturnToMainMenu()
        {
            RestoreAudioLevels();
            GameManager.Instance.ReturnToMainMenu();
        }

        private void UpdateSummary()
        {
            if (_subtitleText != null && GameManager.Instance != null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Replace('_', ' ');
                float playTime = GameManager.Instance.PlayTime;
                int minutes = Mathf.FloorToInt(playTime / 60f);
                int seconds = Mathf.FloorToInt(playTime % 60f);
                _subtitleText.text = $"You died in {sceneName}\nSession time {minutes:00}:{seconds:00}";
            }

            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();
            if (_continueButton != null)
                _continueButton.gameObject.SetActive(hasSave);

            if (_hintText != null)
                _hintText.text = hasSave ? "R Retry  C Continue Save  M Main Menu" : "R Retry  M Main Menu";
        }

        private void ApplyAudioDuck()
        {
            if (AudioManager.Instance == null || _cachedMusicVolume >= 0f)
                return;

            _cachedMusicVolume = AudioManager.Instance.MusicVolume;
            _cachedAmbientVolume = AudioManager.Instance.AmbientVolume;
            _cachedSfxVolume = AudioManager.Instance.SFXVolume;

            AudioManager.Instance.SetMusicVolume(Mathf.Min(_cachedMusicVolume, 0.12f));
            AudioManager.Instance.SetAmbientVolume(Mathf.Min(_cachedAmbientVolume, 0.08f));
            AudioManager.Instance.SetSFXVolume(Mathf.Min(_cachedSfxVolume, 0.35f));
        }

        private void RestoreAudioLevels()
        {
            if (AudioManager.Instance == null || _cachedMusicVolume < 0f)
                return;

            AudioManager.Instance.SetMusicVolume(_cachedMusicVolume);
            AudioManager.Instance.SetAmbientVolume(_cachedAmbientVolume);
            AudioManager.Instance.SetSFXVolume(_cachedSfxVolume);

            _cachedMusicVolume = -1f;
            _cachedAmbientVolume = -1f;
            _cachedSfxVolume = -1f;
        }

        private void BuildOverlay()
        {
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;

            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();

            _root = CreateUIObject("PanelRoot", gameObject.transform);
            RectTransform rootRect = _root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image scrim = _root.AddComponent<Image>();
            scrim.color = new Color(0.02f, 0.01f, 0.01f, 0.92f);

            GameObject card = CreateUIObject("Card", _root.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(520f, 320f);
            cardRect.anchoredPosition = Vector2.zero;

            Image cardImage = card.AddComponent<Image>();
            cardImage.color = new Color(0.08f, 0.05f, 0.05f, 0.96f);

            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(36, 36, 32, 28);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            ContentSizeFitter fitter = card.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateLabel("GAME OVER", 34, FontStyle.Bold, card.transform, font, new Color(0.9f, 0.84f, 0.78f, 1f));
            _subtitleText = CreateLabel("You didn't make it.", 16, FontStyle.Normal, card.transform, font, new Color(0.82f, 0.77f, 0.73f, 1f));
            _hintText = CreateLabel("R Retry  M Main Menu", 13, FontStyle.Normal, card.transform, font, new Color(0.64f, 0.57f, 0.54f, 1f));

            GameObject buttonRow = CreateUIObject("Buttons", card.transform);
            HorizontalLayoutGroup buttonLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 12f;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonLayout.childControlHeight = false;
            buttonLayout.childControlWidth = false;

            LayoutElement rowLayout = buttonRow.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 52f;

            _continueButton = CreateButton("Continue Save", buttonRow.transform, font, new Color(0.28f, 0.18f, 0.14f, 1f));
            _continueButton.onClick.AddListener(ContinueLatestSave);

            Button retryButton = CreateButton("Retry Scene", buttonRow.transform, font, new Color(0.5f, 0.15f, 0.15f, 1f));
            retryButton.onClick.AddListener(RetryCurrentScene);

            Button menuButton = CreateButton("Main Menu", buttonRow.transform, font, new Color(0.18f, 0.18f, 0.2f, 1f));
            menuButton.onClick.AddListener(ReturnToMainMenu);

            _root.SetActive(false);
        }

        private void EnsureGameManagerSubscription()
        {
            if (_isSubscribed || GameManager.Instance == null)
                return;

            GameManager.Instance.OnStateChanged += OnGameStateChanged;
            _isSubscribed = true;
        }

        private static void EnsureEventSystemExists()
        {
            if (EventSystem.current != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(eventSystem);
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Text CreateLabel(string content, int fontSize, FontStyle style, Transform parent, Font font, Color color)
        {
            GameObject labelObject = CreateUIObject(content.Replace(" ", string.Empty), parent);
            Text label = labelObject.AddComponent<Text>();
            label.text = content;
            label.font = font;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;

            LayoutElement layout = labelObject.AddComponent<LayoutElement>();
            layout.preferredHeight = fontSize + 12;

            return label;
        }

        private static Button CreateButton(string label, Transform parent, Font font, Color color)
        {
            GameObject buttonObject = CreateUIObject(label.Replace(" ", string.Empty) + "Button", parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.08f;
            colors.pressedColor = color * 0.92f;
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.45f);
            button.colors = colors;

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140f, 44f);

            LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 140f;
            layout.preferredHeight = 44f;

            Text text = CreateLabel(label, 15, FontStyle.Bold, buttonObject.transform, font, Color.white);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}
