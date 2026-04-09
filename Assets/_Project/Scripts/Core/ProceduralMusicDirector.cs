// ============================================================
// File:        ProceduralMusicDirector.cs
// Namespace:   TWD.Core
// Description: Scene-driven procedural music/ambience director
//              used as a polish layer before final authored audio.
// Author:      The Walking Dead Team
// Created:     2026-04-09
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Auto-bootstrapped music director that generates simple horror
    /// loops in code and swaps them based on scene and combat intensity.
    /// </summary>
    public class ProceduralMusicDirector : MonoBehaviour
    {
        private const string BOOTSTRAP_NAME = "[ProceduralMusicDirector]";
        private const float COMBAT_HOLD_DURATION = 7f;

        private AudioClip _menuMusic;
        private AudioClip _houseMusic;
        private AudioClip _streetMusic;
        private AudioClip _hospitalMusic;
        private AudioClip _undergroundMusic;
        private AudioClip _finaleMusic;
        private AudioClip _combatMusic;
        private AudioClip _bossMusic;

        private AudioClip _windAmbient;
        private AudioClip _indoorAmbient;
        private AudioClip _dripAmbient;

        private float _combatUntilTime;
        private bool _subscribedToEvents;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<ProceduralMusicDirector>() != null)
            {
                return;
            }

            GameObject bootstrap = new GameObject(BOOTSTRAP_NAME);
            DontDestroyOnLoad(bootstrap);
            bootstrap.AddComponent<ProceduralMusicDirector>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            GenerateClips();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SubscribeToEventBus();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeFromEventBus();
        }

        private void Start()
        {
            ApplySceneAudio(SceneManager.GetActiveScene().name, false);
        }

        private void Update()
        {
            if (_combatUntilTime > 0f && Time.unscaledTime >= _combatUntilTime)
            {
                _combatUntilTime = 0f;
                ApplySceneAudio(SceneManager.GetActiveScene().name, true);
            }

            if (!_subscribedToEvents)
            {
                SubscribeToEventBus();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SubscribeToEventBus();
            _combatUntilTime = 0f;
            ApplySceneAudio(scene.name, false);
        }

        private void OnEnemyAlerted(string enemyId)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == Constants.Scenes.MAIN_MENU || sceneName == Constants.Scenes.LOADING)
            {
                return;
            }

            _combatUntilTime = Time.unscaledTime + COMBAT_HOLD_DURATION;
            PlayMusicForScene(sceneName, inCombat: true, immediate: false);
        }

        private void OnPlayerDeath()
        {
            if (!AudioManager.IsInitialized || AudioManager.Instance == null)
            {
                return;
            }

            AudioManager.Instance.StopAmbient();
            AudioManager.Instance.PlayMusic(_bossMusic, fade: true);
        }

        private void OnCutsceneStarted(string cutsceneId)
        {
            if (!AudioManager.IsInitialized || AudioManager.Instance == null)
            {
                return;
            }

            _combatUntilTime = 0f;
            AudioManager.Instance.SetMusicVolume(0.2f);
            AudioManager.Instance.SetAmbientVolume(0.18f);
        }

        private void OnCutsceneEnded(string cutsceneId)
        {
            if (!AudioManager.IsInitialized || AudioManager.Instance == null)
            {
                return;
            }

            AudioManager.Instance.SetMusicVolume(0.3f);
            AudioManager.Instance.SetAmbientVolume(0.4f);
            ApplySceneAudio(SceneManager.GetActiveScene().name, true);
        }

        private void ApplySceneAudio(string sceneName, bool immediate)
        {
            if (!AudioManager.IsInitialized || AudioManager.Instance == null)
            {
                return;
            }

            PlayAmbientForScene(sceneName);
            PlayMusicForScene(sceneName, inCombat: _combatUntilTime > Time.unscaledTime, immediate: immediate);
        }

        private void PlayMusicForScene(string sceneName, bool inCombat, bool immediate)
        {
            AudioClip musicClip = sceneName switch
            {
                Constants.Scenes.MAIN_MENU => _menuMusic,
                Constants.Scenes.LEVEL_01_HOUSE => inCombat ? _combatMusic : _houseMusic,
                Constants.Scenes.LEVEL_02_STREETS => inCombat ? _combatMusic : _streetMusic,
                Constants.Scenes.LEVEL_03_HOSPITAL => inCombat ? _combatMusic : _hospitalMusic,
                Constants.Scenes.LEVEL_04_UNDERGROUND => inCombat ? _combatMusic : _undergroundMusic,
                Constants.Scenes.LEVEL_05_FINALE => inCombat ? _bossMusic : _finaleMusic,
                _ => _houseMusic
            };

            AudioManager.Instance.PlayMusic(musicClip, fade: !immediate);
        }

        private void PlayAmbientForScene(string sceneName)
        {
            AudioClip ambientClip = sceneName switch
            {
                Constants.Scenes.MAIN_MENU => _windAmbient,
                Constants.Scenes.LEVEL_01_HOUSE => _indoorAmbient,
                Constants.Scenes.LEVEL_02_STREETS => _windAmbient,
                Constants.Scenes.LEVEL_03_HOSPITAL => _indoorAmbient,
                Constants.Scenes.LEVEL_04_UNDERGROUND => _dripAmbient,
                Constants.Scenes.LEVEL_05_FINALE => _windAmbient,
                _ => _windAmbient
            };

            AudioManager.Instance.PlayAmbient(ambientClip);
        }

        private void SubscribeToEventBus()
        {
            if (_subscribedToEvents)
            {
                UnsubscribeFromEventBus();
            }

            EventBus.OnEnemyAlerted += OnEnemyAlerted;
            EventBus.OnPlayerDeath += OnPlayerDeath;
            EventBus.OnCutsceneStarted += OnCutsceneStarted;
            EventBus.OnCutsceneEnded += OnCutsceneEnded;
            _subscribedToEvents = true;
        }

        private void UnsubscribeFromEventBus()
        {
            if (!_subscribedToEvents)
            {
                return;
            }

            EventBus.OnEnemyAlerted -= OnEnemyAlerted;
            EventBus.OnPlayerDeath -= OnPlayerDeath;
            EventBus.OnCutsceneStarted -= OnCutsceneStarted;
            EventBus.OnCutsceneEnded -= OnCutsceneEnded;
            _subscribedToEvents = false;
        }

        private void GenerateClips()
        {
            _menuMusic = GeneratePulseLoop("MenuMusic", 52f, 0.05f, 0.09f, 6f);
            _houseMusic = GeneratePulseLoop("HouseMusic", 64f, 0.08f, 0.16f, 6f);
            _streetMusic = GeneratePulseLoop("StreetMusic", 72f, 0.1f, 0.18f, 6f);
            _hospitalMusic = GeneratePulseLoop("HospitalMusic", 84f, 0.08f, 0.15f, 6f);
            _undergroundMusic = GeneratePulseLoop("UndergroundMusic", 46f, 0.16f, 0.12f, 6f);
            _finaleMusic = GeneratePulseLoop("FinaleMusic", 90f, 0.14f, 0.22f, 6f);
            _combatMusic = GeneratePulseLoop("CombatMusic", 118f, 0.22f, 0.24f, 6f);
            _bossMusic = GeneratePulseLoop("BossMusic", 132f, 0.28f, 0.3f, 6f);

            _windAmbient = GenerateNoiseLoop("WindAmbient", 0.09f, 0.15f, 6f);
            _indoorAmbient = GenerateNoiseLoop("IndoorAmbient", 0.03f, 0.08f, 6f);
            _dripAmbient = GenerateDripLoop("DripAmbient", 6f);
        }

        private AudioClip GeneratePulseLoop(string clipName, float baseFrequency, float pulseSpeed, float intensity, float lengthSeconds)
        {
            int sampleRate = 44100;
            int samples = Mathf.RoundToInt(sampleRate * lengthSeconds);
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)sampleRate;
                float beat = 0.55f + Mathf.Sin(time * Mathf.PI * pulseSpeed * 2f) * 0.45f;
                float drone = Mathf.Sin(2f * Mathf.PI * baseFrequency * time) * intensity;
                float upper = Mathf.Sin(2f * Mathf.PI * baseFrequency * 1.5f * time) * intensity * 0.3f;
                float noise = (Mathf.PerlinNoise(time * 0.7f, 0.5f) - 0.5f) * intensity * 0.15f;
                data[i] = (drone + upper + noise) * beat;
            }

            SmoothLoopEdges(data, sampleRate / 8);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateNoiseLoop(string clipName, float noiseAmount, float driftAmount, float lengthSeconds)
        {
            int sampleRate = 44100;
            int samples = Mathf.RoundToInt(sampleRate * lengthSeconds);
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            float[] data = new float[samples];

            float last = 0f;
            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)sampleRate;
                float wind = (Mathf.PerlinNoise(time * 0.35f, 0.2f) - 0.5f) * noiseAmount;
                float drone = Mathf.Sin(2f * Mathf.PI * 34f * time) * driftAmount;
                last = Mathf.Lerp(last, wind + drone, 0.08f);
                data[i] = last;
            }

            SmoothLoopEdges(data, sampleRate / 8);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateDripLoop(string clipName, float lengthSeconds)
        {
            int sampleRate = 44100;
            int samples = Mathf.RoundToInt(sampleRate * lengthSeconds);
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)sampleRate;
                float baseNoise = (Mathf.PerlinNoise(time * 0.2f, 0.8f) - 0.5f) * 0.04f;
                float dripPhase = Mathf.Repeat(time, 1.4f);
                float drip = dripPhase < 0.08f
                    ? Mathf.Sin(dripPhase * Mathf.PI * 45f) * Mathf.Exp(-dripPhase * 22f) * 0.18f
                    : 0f;

                data[i] = baseNoise + drip;
            }

            SmoothLoopEdges(data, sampleRate / 8);
            clip.SetData(data, 0);
            return clip;
        }

        private void SmoothLoopEdges(float[] data, int fadeSamples)
        {
            if (data == null || data.Length == 0 || fadeSamples <= 0)
            {
                return;
            }

            fadeSamples = Mathf.Min(fadeSamples, data.Length / 2);

            for (int i = 0; i < fadeSamples; i++)
            {
                float t = i / (float)fadeSamples;
                float blended = Mathf.Lerp(data[data.Length - fadeSamples + i], data[i], t);
                data[i] = blended;
                data[data.Length - fadeSamples + i] = blended;
            }
        }
    }
}
