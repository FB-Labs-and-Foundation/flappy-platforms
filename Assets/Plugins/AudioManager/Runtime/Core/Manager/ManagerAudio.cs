using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioManager.Runtime.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;


namespace AudioManager.Runtime.Core.Manager
{
    public class ManagerAudio : MonoBehaviour
    {
        public class PlayMusicData
        {
            public string Name;
            public Settings Settings;
        }

        public Action<float> OmCameraZoomChanged; //orho size
        public static ManagerAudio SharedInstance;

        [SerializeField]
        private ManagerAudioConfig.ManagerAudioConfig managerAudioConfig;

        private bool _isMetaMusicPlaying;
        private string _lastPlayedAmbient = string.Empty;
        private string _previousMusic = string.Empty;
        private IDisposable _timer;
        private IEnumerator _switchMusic;

        private List<AudioSource> audioSources;
        private List<VolumeChange> audioSourcesVolumeChangeList;

        /// <summary>
        ///     Только в режиме редактора. Для удобства изменения настроек в реальном времени.
        /// </summary>
        private List<DynamicSettingsListener> dynamicSettingsListenerList;

        private List<GameObjectAudioSources> gameObjectAudioSources;

        private bool isDestroyed;

        private GameObject managerMusicGameObject2DSounds;

        private Action preloadAudioDispatcherOnfinish;

        private bool _gameInFocus = true;

        private PlayMusicData _interruptedByAppFocus;


        private void Awake()
        {
            SharedInstance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInherit();
        }

        private void Start()
        {
            managerAudioConfig.SetSoundEnabledState(managerAudioConfig.IfSoundEnabled());
            managerAudioConfig.SetMusicEnabledState(managerAudioConfig.IfMusicEnabled());
        }

        private void OnApplicationFocus(bool focus)
        {
            _gameInFocus = focus;

            if (!_gameInFocus || _interruptedByAppFocus == null)
                return;

            PlayMusic(_interruptedByAppFocus.Name, _interruptedByAppFocus.Settings);
            _interruptedByAppFocus = null;
        }

        private void Update()
        {
            if (audioSourcesVolumeChangeList.Count > 0)
            {
                // Если подобный источник обрабатывается более новым запросом, удалим старый.
                List<AudioSource> audioSourceList = null;

                for (var i = audioSourcesVolumeChangeList.Count - 1; i > -1; i--)
                {
                    var audio = audioSourcesVolumeChangeList[i];
                    if (audio.delayTimer > 0 && !audio.isCompleteTimer)
                    {
                        audio.delayTimer -= Time.deltaTime;
                        if (audio.delayTimer <= 0)
                            audio.isCompleteTimer = true;
                        continue;
                    }

                    if (!audio.IfTimerOverProcessed())
                    {
                        audio.UpdateChangeSpeed();
                        audio.TimerOverProcessed();
                    }

                    // Если подобный источник обрабатывается более новым запросом, удалим старый.
                    if (audioSourceList == null)
                    {
                        audioSourceList = new List<AudioSource>();
                    }
                    else if (audioSourceList.Contains(audio.audioSource))
                    {
                        audioSourcesVolumeChangeList.RemoveAt(i);

                        continue;
                    }

                    audioSourceList.Add(audio.audioSource);

                    // Проверяем не изменился ли клип в проигрывателе. Или сам контроллер мог быть удалён на объекте.
                    if (!audio.audioSource
                        || audio.audioClip != audio.audioSource.clip)
                    {
                        audioSourcesVolumeChangeList.RemoveAt(i);
                        continue;
                    }

                    var changeSide = audio.audioSource.volume > audio.targetValue ? -1 : 1;
                    var newVolume = audio.changeSpeed == 0
                        ? audio.targetValue
                        : audio.audioSource.volume + Time.deltaTime * audio.changeSpeed * changeSide;

                    if (changeSide < 0)
                    {
                        if (newVolume < audio.targetValue) newVolume = audio.targetValue;
                    }
                    else
                    {
                        if (newVolume > audio.targetValue) newVolume = audio.targetValue;
                    }

                    audio.audioSource.volume = newVolume;
                    if (audio.audioSource.volume == audio.targetValue)
                    {
                        audioSourcesVolumeChangeList.RemoveAt(i);

                        if (audio.audioSource.volume <= 0) audio.audioSource.Stop();
                    }
                }
            }

            // Динамический слушатель настроек.
#if UNITY_EDITOR
            if (!managerAudioConfig.IfDynamicListenerEnabled()) return;
            for (int i = this.dynamicSettingsListenerList.Count - 1; i > -1; i--)
            {
                if (this.dynamicSettingsListenerList[i].audioSource == null || !this.dynamicSettingsListenerList[i].audioSource.isPlaying)
                {
                    this.dynamicSettingsListenerList.RemoveAt(i);
                }
                else
                {

                    if (this.audioSourcesVolumeChangeList.All(a => a.audioSource != this.dynamicSettingsListenerList[i].audioSource))
                    {
                        var dynamicSettings = this.dynamicSettingsListenerList[i];
                        var audioSource = dynamicSettings.audioSource;
                        var settingsAudioInstance = dynamicSettings.settingsAudioInstance;
                        var controllerAudio = dynamicSettings.GetControllerAudio();

                        audioSource.loop = settingsAudioInstance.loop;
                        audioSource.spatialBlend = settingsAudioInstance.is3D ? 1 : 0;
                        audioSource.maxDistance = settingsAudioInstance.maxDist3D;

                        if (controllerAudio == null || !controllerAudio.IfCameraZoomDependence())
                        {
                            audioSource.volume = settingsAudioInstance.volume;
                        }

                        audioSource.outputAudioMixerGroup = settingsAudioInstance.audioMixer;

                        // Каждый кадр менять pitch - плохая идея т.к. он берётся по диапазону.
                        if (dynamicSettings.GetHashPitchMin() != settingsAudioInstance.pitchMin
                            || dynamicSettings.GetHashPitchMax() != settingsAudioInstance.pitchMax)
                        {
                            audioSource.pitch = Random.Range(settingsAudioInstance.pitchMin, settingsAudioInstance.pitchMax);
                            dynamicSettings.UpdatePitch();
                        }
                    }
                }
            }
#endif
        }

        private void OnDestroy()
        {
            Clear();
            isDestroyed = true;
            _timer?.Dispose();
        }

        private void InitializeInherit()
        {
            audioSources = new List<AudioSource>();
            audioSourcesVolumeChangeList = new List<VolumeChange>();
            gameObjectAudioSources = new List<GameObjectAudioSources>();
            managerMusicGameObject2DSounds = gameObject;

            dynamicSettingsListenerList = new List<DynamicSettingsListener>();
            managerAudioConfig.InitializeInherit();
            isDestroyed = false;
        }

        public ManagerAudioConfig.ManagerAudioConfig GetConfig()
        {
            return managerAudioConfig;
        }

        private void Clear()
        {
            for (var i = 0; i < audioSources.Count; i++)
            {
                var audioSource = audioSources[i];
                if (audioSource != null)
                {
                    if (audioSource.isPlaying) audioSource.Stop();

                    audioSource.clip = null;
                }

                Destroy(audioSource);
            }

            audioSources = new List<AudioSource>();


            for (var j = 0; j < gameObjectAudioSources.Count; j++)
            {
                var gameObjectAudioSource = gameObjectAudioSources[j];
                for (var i = 0; i < gameObjectAudioSource.audioSources.Count; i++)
                {
                    var audioSource = gameObjectAudioSource.audioSources[i];
                    if (audioSource != null)
                    {
                        if (audioSource.isPlaying) audioSource.Stop();

                        audioSource.clip = null;
                    }

                    Destroy(audioSource);
                }
            }

            gameObjectAudioSources = new List<GameObjectAudioSources>();

            audioSourcesVolumeChangeList = new List<VolumeChange>();
        }

        private GameObjectAudioSources FindGameObjectAudioSources(GameObject gameObject)
        {
            var sourcesCount = gameObjectAudioSources.Count;
            for (var i = 0; i < sourcesCount; i++)
                if (gameObjectAudioSources[i].gameObject == gameObject)
                    return gameObjectAudioSources[i];

            return null;
        }

        private AudioSource FindAudioSources(string tAudio)
        {
            var audioClip = managerAudioConfig.GetAudioClip(tAudio);
            if (!audioClip)
                return null;

            if (AudioListener.pause) //paused by Yandex ads when going background
                return audioSources.FirstOrDefault(s => s.clip == audioClip.audioClip);

            var count = audioSources.Count;
            for (var i = 0; i < count; i++)
            {
                if (!audioSources[i].isPlaying)
                    continue;

                if (audioSources[i].clip != audioClip.audioClip)
                    continue;

                if (audioSourcesVolumeChangeList.Any(x => x.audioSource == audioSources[i]))
                    continue;

                return audioSources[i];
            }

            return null;
        }

        private AudioSource FindAudioSources(GameObject gameObject, string tAudio)
        {
            var gameObjectAudioSources = FindGameObjectAudioSources(gameObject);
            if (gameObjectAudioSources == null) return null;

            var audioClip = managerAudioConfig.GetAudioClip(tAudio);
            if (audioClip == null) return null;

            var count = gameObjectAudioSources.audioSources.Count;
            for (var i = 0; i < count; i++)
                if (gameObjectAudioSources.audioSources[i].clip == audioClip.audioClip)
                    return gameObjectAudioSources.audioSources[i];

            return null;
        }

        //TODO При выставлении задержек для проигрывания звуков - возвращяемый источник имеет isPlaying = false, и time=0
        //TODO в итоге в этот источник прокидывается другой клип, с другими параметрами, ломающий логику (loop или микшер)
        private AudioSource GetAudioSource()
        {
            var count = audioSources.Count;
            for (var i = 0; i < count; i++)
            {
                var source = audioSources[i];
                if (!source.isPlaying && (source.time == 0 || source.time >= source.clip.length))
                    return source;
            }

            return CreateAudioSource();
        }

        private AudioSource CreateAudioSource()
        {
            var newAudioSource = managerMusicGameObject2DSounds.AddComponent<AudioSource>();
            newAudioSource.volume = 1;
            newAudioSource.playOnAwake = false;
            newAudioSource.maxDistance = 1;
            audioSources.Add(newAudioSource);

            return newAudioSource;
        }

        private AudioSource GetAudioSource(GameObject gameObject)
        {
            var source = FindGameObjectAudioSources(gameObject);

            if (source == null)
            {
                source = new GameObjectAudioSources { gameObject = gameObject };
                gameObjectAudioSources.Add(source);
            }

            var count = source.audioSources.Count;
            for (var i = 0; i < count; i++)
                if (!source.audioSources[i].isPlaying)
                    return source.audioSources[i];

            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            newAudioSource.volume = 1;
            newAudioSource.playOnAwake = false;
            newAudioSource.maxDistance = 40;
            newAudioSource.rolloffMode = AudioRolloffMode.Linear;
            newAudioSource.spread = 0;
            newAudioSource.spatialBlend = 1f;
            source.audioSources.Add(newAudioSource);

            return newAudioSource;
        }

        public float GetCurrentTime(GameObject gameObject, string tAudio)
        {
            var source = FindAudioSources(gameObject, tAudio);
            return source != null ? source.time : 0;
        }

        public float GetCurrentTime(string tAudio, bool isSearchInObjects = true)
        {
            var source = FindAudioSources(tAudio);

            if (source != null) return source.time;

            if (isSearchInObjects)
            {
                var sourcesCount = gameObjectAudioSources.Count;
                for (var i = 0; i < sourcesCount; i++)
                {
                    source = FindAudioSources(gameObjectAudioSources[i].gameObject, tAudio);

                    if (source != null) return source.time;
                }
            }

            return 0;
        }

        public IEnumerator SwitchAudioClip(string currentClip,
            string newClip,
            bool fade = false,
            float speedInSeconds = 0,
            float delay = 0)
        {
            _interruptedByAppFocus = new PlayMusicData() { Name = newClip };

            bool enabledMusicOnStart = managerAudioConfig.GetEnabledMusicOnStart();
            if (!enabledMusicOnStart)
                yield return null;

            var source = FindAudioSources(currentClip);

            if (source == null)
            {
                PlayAudioClip(newClip);
                _switchMusic = null;
                _previousMusic = newClip;
                _interruptedByAppFocus = null;
                yield break;
            }

            if (fade)
            {
                ChangeVolume(source, 0, speedInSeconds, delay);
                yield return new WaitForSeconds(speedInSeconds + delay);
            }

            var config = managerAudioConfig.GetAudioClip(newClip);
            source.clip = config.audioClip;
            source.Play();

            ChangeVolume(source, config.volume, speedInSeconds);
            _switchMusic = null;
            _previousMusic = newClip;
            _interruptedByAppFocus = null;
        }

        public void PlayAudioClip(GameObject gameObject,
            string tAudio,
            Settings settings = null,
            float delayExtra = 0,
            bool isSmart = false)
        {
            if (!managerAudioConfig.IfManagerAudioEnabled()) return;

            managerAudioConfig.GetAudioClip(tAudio,
                delegate (SettingsAudioInstance audioClip)
                {
                    if (audioClip != null)
                    {
                        var audioSource = !isSmart || audioClip.is3D ? GetAudioSource(gameObject) : GetAudioSource();
                        audioSource.clip = audioClip.audioClip;
                        audioSource.rolloffMode = AudioRolloffMode.Linear;
                        audioSource.spread = 0;
                        PlayAudioClipProcess(audioSource, audioClip, settings, delayExtra);
                    }
                });
        }

        public void PlayAudioClip(string soundId, Settings settings = null, float delayExtra = 0)
        {
            if (!managerAudioConfig.CanPlayUnfocused && !_gameInFocus)
                return;

            if (!managerAudioConfig.IfManagerAudioEnabled())
                return;

            if (soundId.Equals(string.Empty))
                return;

            var audioClip = managerAudioConfig.GetAudioClip(soundId);
            if (audioClip is null)
                return;

            var audioSource = GetAudioSource();
            audioSource.clip = audioClip.audioClip;
            PlayAudioClipProcess(audioSource, audioClip, settings, delayExtra);
        }

        public void PlayAudioClip(string[] audios, Settings settings = null, float delayExtra = 0)
        {
            var tAudio = audios.GetRandom();
            PlayAudioClip(tAudio, settings, delayExtra);
        }

        public void ChangeAmbientMusic(string key, float fade = 0)
        {
            var newAmbient = GetConfig().GetAmbient(key);
            if (newAmbient.Equals(_lastPlayedAmbient))
                return;

            StopAudio(_lastPlayedAmbient);
            _lastPlayedAmbient = newAmbient;
            PlayAudioClip(_lastPlayedAmbient, new Settings { fadeIn = fade, loop = true });
        }

        public void PlayMusic(string music, Settings settings)
        {
            if (!managerAudioConfig.CanPlayUnfocused && !_gameInFocus)
            {
                _interruptedByAppFocus = new PlayMusicData() { Name = music, Settings = settings };
                return;
            }

            if (_switchMusic != null)
                return;

            const float fadeSpeed = 1f;
            if (!string.IsNullOrEmpty(_previousMusic))
            {
                _switchMusic = SwitchAudioClip(_previousMusic, music, true, fadeSpeed);
                StartCoroutine(_switchMusic);
            }
            else
            {
                PlayAudioClip(music, settings);
                _previousMusic = music;
            }

        }

        private void PlayRandomMetaMusic()
        {
            var tAudio = GetConfig().GetRandomMetaMusic();
            PlayMusic(tAudio, null);
        }

        public void PlayMetaMusic(float fadeIn = 0)
        {
            PlayMetaMusic("", fadeIn);
        }

        public void PlayMetaMusic(string key, float fadeIn = 0)
        {
            if (_isMetaMusicPlaying)
                return;

            if (!managerAudioConfig.IfManagerAudioEnabled())
                return;

            StopAudioAll(fadeIn);
            ChangeAmbientMusic(key);
            PlayRandomMetaMusic();
            _isMetaMusicPlaying = true;
        }

        private void PlayAudioClipProcess(AudioSource audioSource, SettingsAudioInstance settingsAudioInstance, Settings settings, float delayExtra)
        {
            audioSource.outputAudioMixerGroup = settingsAudioInstance.audioMixer;

            var volume = settings is { volume: >= 0 } ? settings.volume : settingsAudioInstance.volume;
            var playDelay = settingsAudioInstance.delayTime + delayExtra;

            audioSource.loop = settings?.loop ?? settingsAudioInstance.loop;
            audioSource.pitch = Random.Range(settingsAudioInstance.pitchMin, settingsAudioInstance.pitchMax);

            if (settings is { fadeIn: > 0 })
            {
                audioSource.volume = 0;
                ChangeVolume(audioSource, volume, settings.fadeIn, playDelay);
            }
            else if (settingsAudioInstance.fadeIn > 0)
            {
                audioSource.volume = 0;
                ChangeVolume(audioSource, volume, settingsAudioInstance.fadeIn, playDelay);
            }
            else
            {
                audioSource.volume = volume;
            }

            if (settings != null && settings.startTime >= 0)
                audioSource.time = settings.startTime;
            else if (settingsAudioInstance.startTimeMax > 0)
                audioSource.time = Random.Range(settingsAudioInstance.startTimeMin, settingsAudioInstance.startTimeMax);
            else
                audioSource.time = 0f;

            if (playDelay > 0)
                audioSource.PlayDelayed(playDelay);
            else
                audioSource.Play();

            // Динамический слушатель настроек.
#if UNITY_EDITOR
            if (!managerAudioConfig.IfDynamicListenerEnabled()) return;
            dynamicSettingsListenerList.Add(
                new DynamicSettingsListener(audioSource, settingsAudioInstance)
            );
#endif
        }

        private void ChangeVolume(AudioSource audioSource, float volume, float speedInSeconds = 0, float delay = 0)
        {
            if (!audioSource) return;

            audioSourcesVolumeChangeList.Add(
                new VolumeChange(audioSource, volume, speedInSeconds, delay)
            );
        }

        public void ChangeVolume(GameObject gameObject,
            string tAudio,
            float volume,
            float speedInSeconds = 0,
            float delay = 0)
        {
            var audioSource = FindAudioSources(gameObject, tAudio);
            if (audioSource == null) Debug.LogWarning("AudioSource Not Found, " + tAudio + ", volme to:" + volume);

            ChangeVolume(audioSource, volume, speedInSeconds, delay);
        }

        public void ChangeVolume(string tAudio, float volume, float speedInSeconds = 0, float delay = 0)
        {
            var audioSource = FindAudioSources(tAudio);

            if (!audioSource) return;

            ChangeVolume(audioSource, volume, speedInSeconds, delay);
        }

        public void PauseAudioAll()
        {
            var count = audioSources.Count;
            for (var i = 0; i < count; i++)
                if (audioSources[i].isPlaying)
                    audioSources[i].Pause();


            count = gameObjectAudioSources.Count;
            for (var i = 0; i < count; i++)
                foreach (var a in gameObjectAudioSources[i].audioSources)
                    if (a.isPlaying)
                        a.Pause();
        }

        public void ResumeAudioAll()
        {
            var count = audioSources.Count;
            for (var i = 0; i < count; i++)

                if (!audioSources[i].isPlaying && audioSources[i].time != 0)
                    audioSources[i].UnPause();


            count = gameObjectAudioSources.Count;
            for (var i = 0; i < count; i++)
                foreach (var a in gameObjectAudioSources[i].audioSources)
                    if (!audioSources[i].isPlaying && audioSources[i].time != 0)
                        a.UnPause();
        }

        public void StopAudio(string tAudio, float speedInSeconds = 0, float delay = 0)
        {
            ChangeVolume(tAudio, 0, speedInSeconds, delay);
        }

        public void StopAudio(string[] tAudios, GameObject gameObject, float speedInSeconds = 0, float delay = 0)
        {
            var count = tAudios.Length;
            for (var i = 0; i < count; i++) StopAudio(gameObject, tAudios[i], speedInSeconds, delay);
        }

        public void StopAudio(GameObject gameObject, string tAudio, float speedInSeconds = 0, float delay = 0)
        {
            var source = FindGameObjectAudioSources(gameObject);
            if (source != null)
            {
                var audioClip = managerAudioConfig.GetAudioClip(tAudio);
                if (!audioClip) return;

                var count = source.audioSources.Count;
                for (var i = 0; i < count; i++)
                    if (source.audioSources[i].clip == audioClip.audioClip)
                    {
                        ChangeVolume(source.audioSources[i], 0, speedInSeconds, delay);
                        gameObjectAudioSources.Remove(source);
                    }
            }
        }

        public void StopAudioAll(float speedInSeconds = 0, float delay = 0)
        {
            var count = audioSources.Count;
            for (var i = 0; i < count; i++)
                if (audioSources[i].isPlaying)
                {
                    if (speedInSeconds == 0 && delay == 0)
                        audioSources[i].Stop();
                    else
                        ChangeVolume(audioSources[i], 0, speedInSeconds, delay);
                }

            //проход по всем объектам и выключение их звуков
            for (var i = 0; i < gameObjectAudioSources.Count; i++)
                for (var j = 0; j < gameObjectAudioSources[i].audioSources.Count; j++)
                    if (gameObjectAudioSources[i].audioSources[j].isPlaying)
                    {
                        if (speedInSeconds == 0 && delay == 0)
                            gameObjectAudioSources[i].audioSources[j].Stop();
                        else
                            ChangeVolume(gameObjectAudioSources[i].audioSources[j], 0, speedInSeconds, delay);
                    }

            _isMetaMusicPlaying = false;
            _lastPlayedAmbient = string.Empty;
            _timer?.Dispose();
        }

        public void StopAudioIfLoop(string tAudio,
            GameObject gameObject,
            float speedInSeconds = 0,
            float delay = 0,
            bool isSmart = false)
        {
            var audioClip = managerAudioConfig.GetAudioClip(tAudio);
            if (!audioClip || !audioClip.loop) return;

            if (isSmart && !audioClip.is3D)
                StopAudio(tAudio, speedInSeconds, delay);
            else
                StopAudio(gameObject, tAudio, speedInSeconds, delay);
        }

        public void PreLoadAudio(List<string> tAudios)
        {
            PreLoadAudio(tAudios.ToArray());
        }

        public void PreLoadAudio(params string[] tAudios)
        {
            foreach (var mType in tAudios) managerAudioConfig.GetAudioClip(mType);
        }

        public void UnLoadAudio(List<string> tAudios)
        {
            UnLoadAudio(tAudios.ToArray());
        }

        public void UnLoadAudio(params string[] tAudios)
        {
            foreach (var mType in tAudios) managerAudioConfig.UnLoadAudio(mType);
        }

        private bool IfSoundEnabled()
        {
            return this.managerAudioConfig.IfSoundEnabled();
        }
        public bool IfPlaying(string tAudio)
        {
            var audioClip = managerAudioConfig.GetAudioClip(tAudio);
            if (!audioClip) return false;

            var audioSourcesCount = audioSources.Count;
            for (var i = 0; i < audioSourcesCount; i++)
            {
                var source = audioSources[i];
                if (source.isPlaying && source.clip == audioClip.audioClip) return true;
            }

            return false;
        }

        public bool IfPlaying(string tAudio, GameObject gameObject)
        {
            GameObjectAudioSources sources = null;
            var count = gameObjectAudioSources.Count;
            for (var i = 0; i < count; i++)
                if (gameObjectAudioSources[i].gameObject == gameObject)
                {
                    sources = gameObjectAudioSources[i];
                    break;
                }

            if (sources == null) return false;

            var audioClip = managerAudioConfig.GetAudioClip(tAudio);
            if (!audioClip) return false;

            var audioSourcesCount = sources.audioSources.Count;
            for (var i = 0; i < audioSourcesCount; i++)
            {
                var source = sources.audioSources[i];
                if (source.isPlaying && source.clip == audioClip.audioClip) return true;
            }

            return false;
        }

        public bool IfDestroyFinalized()
        {
            return isDestroyed;
        }

        public bool IfDestroyed()
        {
            return isDestroyed;
        }
    }
}