// ============================================================
// File:        LoadingScreenOverlay.cs
// Namespace:   TWD.UI
// Description: Runtime-generated loading overlay that mirrors
//              SceneLoader progress without requiring scene setup.
// Author:      The Walking Dead Team
// Created:     2026-04-09
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TWD.Core;
using TWD.Utilities;

namespace TWD.UI
{
    /// <summary>
    /// Builds a small loading UI at runtime and binds it to SceneLoader.
    /// This keeps the loading scene readable even before authored UI exists.
    /// </summary>
    public class LoadingScreenOverlay : MonoBehaviour
    {
        private const string ROOT_NAME = "[LoadingScreenOverlay]";

        private Canvas _canvas;
        private GameObject _root;
        private Slider _progressBar;
        private Text _progressText;
        private Text _hintText;
        private bool _isSubscribed;
        private float _currentProgress;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<LoadingScreenOverlay>() != null)
            {
                return;
            }

            GameObject overlay = new GameObject(ROOT_NAME);
            DontDestroyOnLoad(overlay);
            overlay.AddComponent<LoadingScreenOverlay>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BuildOverlay();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeFromSceneLoader();
        }

        private void Update()
        {
            if (!_isSubscribed && SceneLoader.IsInitialized)
            {
                SubscribeToSceneLoader();
            }

            if (_root != null && _root.activeSelf)
            {
                _hintText.text = $"Scanning safe route{new string('.', (Time.frameCount / 20) % 4)}";
                _progressBar.value = Mathf.MoveTowards(_progressBar.value, _currentProgress, Time.unscaledDeltaTime * 2.5f);
                _progressText.text = $"{Mathf.RoundToInt(_progressBar.value * 100f)}%";
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_root == null)
            {
                return;
            }

            bool isLoadingScene = scene.name == Constants.Scenes.LOADING;
            _root.SetActive(isLoadingScene);

            if (isLoadingScene)
            {
                _currentProgress = 0f;
                _progressBar.value = 0f;
                _progressText.text = "0%";
                _hintText.text = "Scanning safe route";
            }
        }

        private void SubscribeToSceneLoader()
        {
            if (!SceneLoader.IsInitialized || SceneLoader.Instance == null)
            {
                return;
            }

            UnsubscribeFromSceneLoader();
            SceneLoader.Instance.OnLoadProgress += OnLoadProgress;
            SceneLoader.Instance.OnLoadComplete += OnLoadComplete;
            _isSubscribed = true;
        }

        private void UnsubscribeFromSceneLoader()
        {
            if (!_isSubscribed || !SceneLoader.IsInitialized || SceneLoader.Instance == null)
            {
                _isSubscribed = false;
                return;
            }

            SceneLoader.Instance.OnLoadProgress -= OnLoadProgress;
            SceneLoader.Instance.OnLoadComplete -= OnLoadComplete;
            _isSubscribed = false;
        }

        private void OnLoadProgress(float progress)
        {
            _currentProgress = Mathf.Clamp01(progress);
        }

        private void OnLoadComplete()
        {
            _currentProgress = 1f;
        }

        private void BuildOverlay()
        {
            _root = new GameObject("LoadingOverlayRoot");
            _root.transform.SetParent(transform, false);

            _canvas = _root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5000;
            _root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _root.AddComponent<GraphicRaycaster>();

            Image background = CreateImage("Background", _root.transform, new Color(0.03f, 0.03f, 0.04f, 0.95f));
            Stretch(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Text title = CreateText("Title", _root.transform, "THE WALKING DEAD", 34, TextAnchor.MiddleCenter);
            title.color = new Color(0.82f, 0.78f, 0.72f, 1f);
            SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0f, 0f), new Vector2(520f, 48f));

            _hintText = CreateText("Hint", _root.transform, "Scanning safe route", 18, TextAnchor.MiddleCenter);
            _hintText.color = new Color(0.62f, 0.67f, 0.66f, 1f);
            SetAnchoredRect(_hintText.rectTransform, new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), Vector2.zero, new Vector2(420f, 30f));

            GameObject progressBarRoot = new GameObject("ProgressBar");
            progressBarRoot.transform.SetParent(_root.transform, false);
            Image track = progressBarRoot.AddComponent<Image>();
            track.color = new Color(0.15f, 0.15f, 0.18f, 1f);
            RectTransform trackRect = track.rectTransform;
            SetAnchoredRect(trackRect, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), Vector2.zero, new Vector2(420f, 18f));

            GameObject fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(progressBarRoot.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            Stretch(fillAreaRect, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f));

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.57f, 0.09f, 0.08f, 1f);

            _progressBar = progressBarRoot.AddComponent<Slider>();
            _progressBar.fillRect = fillImage.rectTransform;
            _progressBar.targetGraphic = fillImage;
            _progressBar.direction = Slider.Direction.LeftToRight;
            _progressBar.minValue = 0f;
            _progressBar.maxValue = 1f;

            Stretch(fillImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            _progressText = CreateText("ProgressText", _root.transform, "0%", 16, TextAnchor.MiddleCenter);
            _progressText.color = new Color(0.87f, 0.85f, 0.82f, 1f);
            SetAnchoredRect(_progressText.rectTransform, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(120f, 24f));

            _root.SetActive(SceneManager.GetActiveScene().name == Constants.Scenes.LOADING);
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            return text;
        }

        private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void SetAnchoredRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}
