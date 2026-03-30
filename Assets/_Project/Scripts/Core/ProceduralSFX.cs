// ============================================================
// File:        ProceduralSFX.cs
// Namespace:   TWD.Core
// Description: Generates procedural placeholder sound effects
//              at runtime for prototyping. Creates AudioClips
//              from code (gunshot, footstep, hit, zombie groan,
//              door creak, pickup chime, etc.) so the game has
//              audio feedback before real assets are added.
// Author:      The Walking Dead Team
// Created:     2026-03-30
// ============================================================

using UnityEngine;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Generates procedural placeholder SFX as AudioClips.
    /// Attach to the [Managers] object. Subscribes to EventBus events
    /// and plays appropriate sounds through AudioManager.
    /// Replace with real audio assets later.
    /// </summary>
    public class ProceduralSFX : MonoBehaviour
    {
        #region Private Fields

        private AudioClip _gunshot;
        private AudioClip _footstep;
        private AudioClip _hit;
        private AudioClip _zombieGroan;
        private AudioClip _doorOpen;
        private AudioClip _doorLocked;
        private AudioClip _pickup;
        private AudioClip _reload;
        private AudioClip _melee;
        private AudioClip _playerHurt;
        private AudioClip _uiClick;
        private AudioClip _ambientWind;

        private float _footstepTimer;
        private Transform _playerTransform;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            GenerateAllClips();
        }

        private void Start()
        {
            var player = GameObject.FindWithTag(Constants.Tags.PLAYER);
            if (player != null) _playerTransform = player.transform;

            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            HandleFootsteps();
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            EventBus.OnWeaponFired += OnWeaponFired;
            EventBus.OnWeaponReloaded += OnReload;
            EventBus.OnPlayerDamaged += OnPlayerHurt;
            EventBus.OnItemPickedUp += OnItemPickup;
            EventBus.OnEnemyAlerted += OnZombieAlert;
            EventBus.OnEnemyKilled += OnEnemyKilled;
            EventBus.OnDoorUnlocked += OnDoorUnlocked;
        }

        private void UnsubscribeEvents()
        {
            EventBus.OnWeaponFired -= OnWeaponFired;
            EventBus.OnWeaponReloaded -= OnReload;
            EventBus.OnPlayerDamaged -= OnPlayerHurt;
            EventBus.OnItemPickedUp -= OnItemPickup;
            EventBus.OnEnemyAlerted -= OnZombieAlert;
            EventBus.OnEnemyKilled -= OnEnemyKilled;
            EventBus.OnDoorUnlocked -= OnDoorUnlocked;
        }

        #endregion

        #region Event Handlers

        private void OnWeaponFired()
        {
            AudioManager.Instance.PlaySFX(_gunshot);
        }

        private void OnReload()
        {
            AudioManager.Instance.PlaySFX(_reload);
        }

        private void OnPlayerHurt(float damage)
        {
            AudioManager.Instance.PlaySFX(_playerHurt);
        }

        private void OnItemPickup(string itemName)
        {
            AudioManager.Instance.PlaySFX(_pickup);
        }

        private void OnZombieAlert(string enemyId)
        {
            AudioManager.Instance.PlaySFX(_zombieGroan);
        }

        private void OnEnemyKilled(string enemyId)
        {
            AudioManager.Instance.PlaySFX(_hit);
        }

        private void OnDoorUnlocked(string doorId)
        {
            AudioManager.Instance.PlaySFX(_doorOpen);
        }

        #endregion

        #region Footsteps

        private void HandleFootsteps()
        {
            if (_playerTransform == null) return;

            var controller = _playerTransform.GetComponent<Player.PlayerController>();
            if (controller == null) return;

            if (controller.MoveAmount > 0.1f)
            {
                _footstepTimer -= Time.deltaTime;
                if (_footstepTimer <= 0f)
                {
                    float interval = controller.IsSprinting ? 0.3f : 0.5f;
                    _footstepTimer = interval;

                    // Vary pitch slightly for natural feel
                    var sfxSource = AudioManager.Instance.GetComponent<AudioSource>();
                    if (sfxSource == null)
                    {
                        AudioManager.Instance.PlaySFX(_footstep);
                    }
                    else
                    {
                        float origPitch = sfxSource.pitch;
                        sfxSource.pitch = Random.Range(0.85f, 1.15f);
                        AudioManager.Instance.PlaySFX(_footstep);
                        sfxSource.pitch = origPitch;
                    }
                }
            }
            else
            {
                _footstepTimer = 0f;
            }
        }

        #endregion

        #region Audio Generation

        private void GenerateAllClips()
        {
            _gunshot = GenerateGunshot();
            _footstep = GenerateFootstep();
            _hit = GenerateHit();
            _zombieGroan = GenerateZombieGroan();
            _doorOpen = GenerateDoorOpen();
            _doorLocked = GenerateDoorLocked();
            _pickup = GeneratePickup();
            _reload = GenerateReload();
            _melee = GenerateMelee();
            _playerHurt = GeneratePlayerHurt();
            _uiClick = GenerateUIClick();
            _ambientWind = GenerateAmbientWind();
        }

        /// <summary>Sharp noise burst — bang sound.</summary>
        private AudioClip GenerateGunshot()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 5; // 0.2 seconds
            var clip = AudioClip.Create("Gunshot", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Exp(-t * 20f); // Fast decay
                float noise = Random.Range(-1f, 1f);
                float lowFreq = Mathf.Sin(2f * Mathf.PI * 80f * t);
                data[i] = (noise * 0.7f + lowFreq * 0.3f) * envelope * 0.8f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Soft thud — footstep on concrete.</summary>
        private AudioClip GenerateFootstep()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 10; // 0.1 seconds
            var clip = AudioClip.Create("Footstep", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Exp(-t * 30f);
                float noise = Random.Range(-1f, 1f);
                float thud = Mathf.Sin(2f * Mathf.PI * 60f * t);
                data[i] = (noise * 0.3f + thud * 0.7f) * envelope * 0.3f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Impact sound — flesh hit.</summary>
        private AudioClip GenerateHit()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 8;
            var clip = AudioClip.Create("Hit", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Exp(-t * 15f);
                float noise = Random.Range(-1f, 1f) * 0.5f;
                float thump = Mathf.Sin(2f * Mathf.PI * 120f * t);
                data[i] = (noise + thump) * envelope * 0.5f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Low rumbling moan — zombie alert.</summary>
        private AudioClip GenerateZombieGroan()
        {
            int sampleRate = 44100;
            int samples = sampleRate; // 1 second
            var clip = AudioClip.Create("ZombieGroan", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Sin(Mathf.PI * t) * 0.6f; // Fade in/out
                float freq = 80f + Mathf.Sin(t * 3f) * 30f; // Wobbling pitch
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = Random.Range(-1f, 1f) * 0.15f;
                data[i] = (wave + noise) * envelope;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Creaking wood sound — door opening.</summary>
        private AudioClip GenerateDoorOpen()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 2; // 0.5 seconds
            var clip = AudioClip.Create("DoorOpen", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Sin(Mathf.PI * t);
                float freq = 200f + t * 400f; // Rising pitch
                float creak = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f;
                float noise = Random.Range(-1f, 1f) * 0.2f;
                data[i] = (creak + noise) * envelope * 0.4f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Metallic rattle — locked door.</summary>
        private AudioClip GenerateDoorLocked()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 4;
            var clip = AudioClip.Create("DoorLocked", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Exp(-t * 10f);
                float metal = Mathf.Sin(2f * Mathf.PI * 800f * t) * 0.3f;
                float rattle = Mathf.Sin(2f * Mathf.PI * 1200f * t) * 0.2f;
                data[i] = (metal + rattle) * envelope * 0.5f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Bright chime — item pickup.</summary>
        private AudioClip GeneratePickup()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 4; // 0.25 seconds
            var clip = AudioClip.Create("Pickup", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Exp(-t * 8f);
                float note1 = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.5f; // A5
                float note2 = Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.3f; // E6
                data[i] = (note1 + note2) * envelope * 0.5f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Metallic click-clack — reload.</summary>
        private AudioClip GenerateReload()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 2;
            var clip = AudioClip.Create("Reload", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                // Two clicks
                float click1 = (t < 0.1f) ? Mathf.Exp(-t * 50f) * Mathf.Sin(2f * Mathf.PI * 2000f * t) : 0f;
                float t2 = t - 0.3f;
                float click2 = (t2 > 0f && t2 < 0.1f) ? Mathf.Exp(-t2 * 50f) * Mathf.Sin(2f * Mathf.PI * 1500f * t2) : 0f;
                data[i] = (click1 + click2) * 0.6f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Whoosh — melee swing.</summary>
        private AudioClip GenerateMelee()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 5;
            var clip = AudioClip.Create("Melee", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Sin(Mathf.PI * t);
                float noise = Random.Range(-1f, 1f);
                // Band-pass filter effect via frequency modulation
                float freq = 200f + t * 800f;
                float swoosh = Mathf.Sin(2f * Mathf.PI * freq * t);
                data[i] = (noise * 0.3f + swoosh * 0.3f) * envelope * 0.4f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Sharp grunt — player taking damage.</summary>
        private AudioClip GeneratePlayerHurt()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 4;
            var clip = AudioClip.Create("PlayerHurt", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Exp(-t * 6f);
                float voice = Mathf.Sin(2f * Mathf.PI * 250f * t) * 0.4f;
                float noise = Random.Range(-1f, 1f) * 0.3f;
                data[i] = (voice + noise) * envelope * 0.6f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Soft click — UI interaction.</summary>
        private AudioClip GenerateUIClick()
        {
            int sampleRate = 44100;
            int samples = sampleRate / 20; // Very short
            var clip = AudioClip.Create("UIClick", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = Mathf.Exp(-t * 40f);
                data[i] = Mathf.Sin(2f * Mathf.PI * 1000f * t) * envelope * 0.3f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Low frequency wind — ambient background.</summary>
        private AudioClip GenerateAmbientWind()
        {
            int sampleRate = 44100;
            int samples = sampleRate * 5; // 5 seconds (loops)
            var clip = AudioClip.Create("AmbientWind", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float noise = Random.Range(-1f, 1f);
                // Slow modulation for wind feel
                float mod = Mathf.Sin(2f * Mathf.PI * 0.2f * t) * 0.5f + 0.5f;
                // Simple low-pass by averaging
                data[i] = noise * mod * 0.1f;
            }

            // Simple smoothing pass (poor man's low-pass)
            for (int i = 1; i < samples; i++)
            {
                data[i] = data[i] * 0.1f + data[i - 1] * 0.9f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        #endregion

        #region Public Access

        /// <summary>Gets a generated clip by name (for manual use).</summary>
        public AudioClip GetClip(string name)
        {
            return name.ToLower() switch
            {
                "gunshot" => _gunshot,
                "footstep" => _footstep,
                "hit" => _hit,
                "zombie" or "groan" => _zombieGroan,
                "door" or "dooropen" => _doorOpen,
                "locked" or "doorlocked" => _doorLocked,
                "pickup" => _pickup,
                "reload" => _reload,
                "melee" => _melee,
                "hurt" or "playerhurt" => _playerHurt,
                "click" or "ui" => _uiClick,
                "wind" or "ambient" => _ambientWind,
                _ => null
            };
        }

        /// <summary>Starts the ambient wind loop.</summary>
        public void StartAmbientWind()
        {
            AudioManager.Instance.PlayAmbient(_ambientWind);
        }

        #endregion
    }
}
