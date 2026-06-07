using System.Collections.Generic;
using UnityEngine;
using Game.Audio;

namespace Game.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Library")]
        [SerializeField] private SoundLibrary library;

        [Header("Pool")]
        [SerializeField] private int poolSize = 10;

        [Header("Volume")]
        [SerializeField] private float sfxVolume = 1f;

        private readonly Dictionary<SoundId, SoundData> soundMap = new();
        private readonly List<AudioSource> sourcePool = new();

        public float CurrentVolume => sfxVolume;

        #region Unity Lifecycle

        private void Awake()
        {
            if (library == null)
            {
                Debug.LogError("[SoundManager] Missing SoundLibrary.");
                enabled = false;
                return;
            }

            Debug.Log($"SoundManager Awake | Instance Exists: {Instance != null}");

            if (Instance != null && Instance != this)
            {
                Debug.Log("Duplicate SoundManager destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            BuildLibrary();
            CreatePool();
            LoadVolume();
        }


        #endregion

        #region Initialization

        private void BuildLibrary()
        {
            soundMap.Clear();

            foreach (var entry in library.sounds)
            {
                if (entry.clip == null)
                    continue;

                soundMap[entry.id] = entry;
            }
        }

        private void CreatePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = new($"AudioSource_{i}");

                obj.transform.SetParent(transform);

                AudioSource source = obj.AddComponent<AudioSource>();

                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;

                sourcePool.Add(source);
            }
        }

        #endregion

        #region Public API

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);

            foreach (AudioSource source in sourcePool)
            {
                if (source.isPlaying)
                {
                    source.volume = sfxVolume;
                }
            }

            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        private void LoadVolume()
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        public static void Play(SoundId soundId)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[SoundManager] Instance missing.");
                return;
            }

            Instance.PlayLocalInternal(soundId);
        }

        #endregion

        #region Internal Playback



        private void PlayLocalInternal(SoundId soundId)
        {
            if (!soundMap.TryGetValue(soundId, out SoundData data))
                return;

            AudioSource source = GetAvailableSource();

            ResetSource(source);

            source.transform.localPosition = Vector3.zero;

            source.spatialBlend = 0f;

            source.clip = data.clip;

            source.volume = data.volume * sfxVolume;

            // Pitch variation for repetitive sounds
            switch (soundId)
            {
                case SoundId.FootstepLeft:
                case SoundId.FootstepRight:
                    source.pitch = Random.Range(0.95f, 1.05f);
                    break;

                case SoundId.Jump:
                    source.pitch = Random.Range(0.98f, 1.02f);
                    break;

                case SoundId.Landing:
                    source.pitch = Random.Range(0.97f, 1.03f);
                    break;

                default:
                    source.pitch = 1f;
                    break;
            }

            source.Play();

            Debug.Log($"Playing sound: {soundId}");
        }

        public static void PlayLocal(SoundId soundId)
        {
            if (Instance == null)
                return;

            Instance.PlayLocalInternal(soundId);
        }

        public static void PlayWorld(SoundId soundId, Vector3 position)
        {
            if (Instance == null)
                return;

            Instance.PlayWorldInternal(soundId, position);
        }

        private void PlayWorldInternal(SoundId soundId, Vector3 position)
        {
            if (!soundMap.TryGetValue(soundId, out SoundData data))
                return;

            AudioSource source = GetAvailableSource();

            ResetSource(source);


            source.transform.position = position;

            source.clip = data.clip;

            source.volume = data.volume * sfxVolume;

            source.spatialBlend = data.spatialBlend;

            source.minDistance = data.minDistance;
            source.maxDistance = data.maxDistance;

            source.rolloffMode = AudioRolloffMode.Linear;

            source.Play();

            Debug.Log($"Playing sound: {soundId}");
        }

        public static AudioSource PlayAttachedLoop(
    SoundId soundId,
    Transform target)
        {
            if (Instance == null || target == null)
                return null;

            return Instance.PlayAttachedLoopInternal(soundId, target);
        }

        private AudioSource PlayAttachedLoopInternal(
            SoundId soundId,
            Transform target)
        {
            if (!soundMap.TryGetValue(soundId, out SoundData data))
                return null;

            AudioSource source = GetAvailableSource();

            ResetSource(source);

            source.transform.position = target.position;

            source.clip = data.clip;

            source.volume = data.volume * sfxVolume;

            source.spatialBlend = data.spatialBlend;

            source.minDistance = data.minDistance;

            source.maxDistance = data.maxDistance;

            source.rolloffMode = AudioRolloffMode.Logarithmic;

            source.loop = true;

            FollowTarget follow = source.GetComponent<FollowTarget>();

            if (follow == null)
                follow = source.gameObject.AddComponent<FollowTarget>();

            follow.SetTarget(target);

            source.Play();

            return source;
        }

        public static void StopLoop(AudioSource source)
        {
            if (source == null)
                return;

            source.Stop();
            source.clip = null;

            FollowTarget follow = source.GetComponent<FollowTarget>();

            if (follow != null)
                follow.ClearTarget();

            source.transform.localPosition = Vector3.zero;
        }

        private AudioSource GetAvailableSource()
        {
            foreach (AudioSource source in sourcePool)
            {
                if (!source.isPlaying)
                    return source;
            }

            GameObject obj = new($"AudioSource_Extra");

            obj.transform.SetParent(transform);

            AudioSource newSource = obj.AddComponent<AudioSource>();

            newSource.playOnAwake = false;
            newSource.loop = false;

            sourcePool.Add(newSource);

            return newSource;
        }

        #endregion

        private void ResetSource(AudioSource source)
        {
            source.Stop();

            source.clip = null;

            source.volume = 1f;

            source.pitch = 1f;

            source.loop = false;

            source.spatialBlend = 0f;

            source.minDistance = 1f;

            source.maxDistance = 500f;

            source.rolloffMode = AudioRolloffMode.Logarithmic;

            source.panStereo = 0f;

            source.dopplerLevel = 0f;

            source.spread = 0f;
        }
    }
}