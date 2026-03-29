// ============================================================
// File:        AudioManager.cs
// Namespace:   TWD.Core
// Description: Centralized audio management. Controls music,
//              SFX, ambient, and voice audio layers with
//              independent volume controls.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System.Collections;
using UnityEngine;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Manages all audio in the game. Provides separate channels
    /// for Music, SFX, Voice, Ambient, and UI with configurable volumes.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        #region Serialized Fields

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _ambientSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _voiceSource;
        [SerializeField] private AudioSource _uiSource;

        [Header("Default Volumes")]
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _voiceVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _ambientVolume = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _uiVolume = 0.5f;

        [Header("Music Settings")]
        [SerializeField] private float _musicFadeDuration = 2f;

        #endregion

        #region Properties

        public float MusicVolume => _musicVolume;
        public float SFXVolume => _sfxVolume;
        public float VoiceVolume => _voiceVolume;
        public float AmbientVolume => _ambientVolume;
        public float UIVolume => _uiVolume;

        #endregion

        #region Lifecycle

        protected override void OnSingletonAwake()
        {
            // Create audio sources if not assigned
            if (_musicSource == null) _musicSource = CreateAudioSource("Music", true);
            if (_ambientSource == null) _ambientSource = CreateAudioSource("Ambient", true);
            if (_sfxSource == null) _sfxSource = CreateAudioSource("SFX", false);
            if (_voiceSource == null) _voiceSource = CreateAudioSource("Voice", false);
            if (_uiSource == null) _uiSource = CreateAudioSource("UI", false);

            ApplyVolumes();
        }

        #endregion

        #region Music

        /// <summary>Plays a music track with crossfade.</summary>
        public void PlayMusic(AudioClip clip, bool fade = true)
        {
            if (clip == null) return;

            if (fade && _musicSource.isPlaying)
            {
                StartCoroutine(CrossfadeMusic(clip));
            }
            else
            {
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }

        /// <summary>Stops the current music track.</summary>
        public void StopMusic(bool fade = true)
        {
            if (fade)
            {
                StartCoroutine(FadeOut(_musicSource, _musicFadeDuration));
            }
            else
            {
                _musicSource.Stop();
            }
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            yield return FadeOut(_musicSource, _musicFadeDuration * 0.5f);
            _musicSource.clip = newClip;
            _musicSource.Play();
            yield return FadeIn(_musicSource, _musicVolume, _musicFadeDuration * 0.5f);
        }

        #endregion

        #region Ambient

        /// <summary>Plays an ambient loop.</summary>
        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null) return;
            _ambientSource.clip = clip;
            _ambientSource.Play();
        }

        /// <summary>Stops ambient audio.</summary>
        public void StopAmbient()
        {
            _ambientSource.Stop();
        }

        #endregion

        #region SFX

        /// <summary>Plays a one-shot SFX clip.</summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>Plays SFX at a world position (3D spatial).</summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, _sfxVolume);
        }

        #endregion

        #region Voice

        /// <summary>Plays a voice line (stops any current voice).</summary>
        public void PlayVoice(AudioClip clip)
        {
            if (clip == null) return;
            _voiceSource.Stop();
            _voiceSource.clip = clip;
            _voiceSource.Play();
        }

        #endregion

        #region UI

        /// <summary>Plays a UI sound effect.</summary>
        public void PlayUI(AudioClip clip)
        {
            if (clip == null) return;
            _uiSource.PlayOneShot(clip, _uiVolume);
        }

        #endregion

        #region Volume Control

        /// <summary>Sets music volume (0-1).</summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            _musicSource.volume = _musicVolume;
        }

        /// <summary>Sets SFX volume (0-1).</summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            _sfxSource.volume = _sfxVolume;
        }

        /// <summary>Sets voice volume (0-1).</summary>
        public void SetVoiceVolume(float volume)
        {
            _voiceVolume = Mathf.Clamp01(volume);
            _voiceSource.volume = _voiceVolume;
        }

        /// <summary>Sets ambient volume (0-1).</summary>
        public void SetAmbientVolume(float volume)
        {
            _ambientVolume = Mathf.Clamp01(volume);
            _ambientSource.volume = _ambientVolume;
        }

        /// <summary>Sets UI volume (0-1).</summary>
        public void SetUIVolume(float volume)
        {
            _uiVolume = Mathf.Clamp01(volume);
            _uiSource.volume = _uiVolume;
        }

        private void ApplyVolumes()
        {
            _musicSource.volume = _musicVolume;
            _sfxSource.volume = _sfxVolume;
            _voiceSource.volume = _voiceVolume;
            _ambientSource.volume = _ambientVolume;
            _uiSource.volume = _uiVolume;
        }

        #endregion

        #region Helpers

        private AudioSource CreateAudioSource(string name, bool loop)
        {
            var go = new GameObject($"AudioSource_{name}");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            return source;
        }

        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            source.Stop();
            source.volume = startVolume;
        }

        private IEnumerator FadeIn(AudioSource source, float targetVolume, float duration)
        {
            source.volume = 0f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }

        #endregion
    }
}
