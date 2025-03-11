using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AudioManager.Runtime.Core.Manager;
using AudioManager.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioManager.Runtime.Core.ManagerAudioConfig
{
	[AttributeSettingsTitle("Audio System")]
	[CreateAssetMenu(fileName = "SettingsManagerAudio", menuName = "Project/SettingsManagerAudio")]
	public class ManagerAudioConfig : ScriptableObject
	{
		[Header("Audio Mixer Parameters")]
		[SerializeField]
		public string MusicVolumeKey = "MusicVolume";

		[SerializeField]
		public string SoundVolumeKey = "SoundsVolume";

		[Space]
		[SerializeField]
		private string soundsPath;

		public string SoundsPath => soundsPath;

		[SerializeField]
		private string saveDataPath;

		public string SaveDataPath => saveDataPath;

		[SerializeField]
		private string tAudioPath;

		public string TAudioPath => tAudioPath;

		[Space(15)] [SerializeField]
		private TextAsset audioFilesJson;

		[SerializeField]
		private AudioMixerGroup audioMixerDefaultGroup;

		[SerializeField]
		private AudioMixer audioMixer;

		[Space(10)] [SerializeField]
		private bool isEnableMusicOnStart = true;

		[Space(10)] [SerializeField] [Tooltip("Only Editor Mode")]
		private bool isEnabledDynamicListener;

		[Header("Music On Scene Start")]
		[SerializeField]
		private string[] metaMusic;
		
		[SerializeField]
		private string[] coreMusic;
		
		[SerializeField]
		private KeyValueMusicWrapper[] ambientSounds;

		[SerializeField]
		private SettingsAudioInstance[] soundsAssets;

		[Header("EmotionsSounds")]
		[SerializeField]
		private KeyValueMusicWrapper[] emotionsSounds;

		[field: SerializeField] public bool CanPlayUnfocused { get; private set; } = true;

		private readonly List<string> _excludedList = new();
		private bool isManagerAudioEnabled;

		private List<SoundHolder> sounds;
		private Dictionary<string, SoundHolder> soundsMap;
		private AudioList audioList;

		public bool IfManagerAudioEnabled()
		{
			return isManagerAudioEnabled;
		}

		public bool IfDynamicListenerEnabled()
		{
			return isEnabledDynamicListener;
		}

		public void InitializeInherit()
		{
			isManagerAudioEnabled = true;

			if (audioFilesJson != null)
			{
				audioList = JsonUtility.FromJson<AudioList>(audioFilesJson.ToString());
				UpdateAudioMixerMusicVolume();
				UpdateAudioMixerSoundVolume();
			}

			FillSounds();
		}

		private void FillSounds()
		{
			sounds = new List<SoundHolder>();
			soundsMap = new Dictionary<string, SoundHolder>();

			for (var i = 0; i < soundsAssets.Length; i++)
			{
				var asset = soundsAssets[i];
				if (asset == null)
				{
					Debug.LogError(string.Format(
						"RepositoryManagerAudio. There is an empty element in array. Remove it, index: {0}.\r\n\r\n",
						i));
				}
				else
				{
					var s = new SoundHolder();

					s.SetAudio(asset);
					s.tAudio = asset.name;
					sounds.Add(s);
					soundsMap.Add(asset.name, s);
				}
			}
		}

		public bool GetEnabledMusicOnStart()
		{
			return isEnableMusicOnStart;
		}

		public string GetRandomMetaMusic()
		{
			return metaMusic.GetRandom();
		}
		
		public string GetRandomCoreMusic()
		{
			return coreMusic.GetRandom();
		}

		public string GetExclusiveRandomMetaMusic()
		{
			var audio = string.Empty;
			var resultList = metaMusic.Except(_excludedList).ToArray();

			if (resultList.Length <= 0)
			{
				var lastPlayed = _excludedList[^1];
				_excludedList.Clear();
				_excludedList.Add(lastPlayed);
				resultList = metaMusic.Except(_excludedList).ToArray();
				audio = resultList.GetRandom();
			}
			else
			{
				audio = resultList.GetRandom();
			}

			if (!_excludedList.Contains(audio)) _excludedList.Add(audio);

			return audio;
		}

		public string GetAmbient(string key)
		{
			var audio = string.Empty;

			if (key.Equals(string.Empty))
				return audio;

			foreach (var ambientSound in ambientSounds)
			{
				if (!key.Equals(ambientSound.key)) continue;

				audio = ambientSound.tAudio.GetRandom();
			}

			return audio;
		}

		public string GetEmotionSound(string emotionName)
		{
			var sounds = emotionsSounds.FirstOrDefault(x => x.key == emotionName);
			return sounds is null ? string.Empty : sounds.tAudio.GetRandom();
		}

		public SettingsAudioInstance GetAudioClip(string soundId)
		{
			if (soundsMap.ContainsKey(soundId))
				return soundsMap[soundId].GetAudio();

			var setting = new SoundHolder {tAudio = soundId};
			sounds.Add(setting);
			soundsMap.Add(soundId, setting);

			return setting.GetAudio();
		}

		public void GetAudioClip(string tAudio, System.Action<SettingsAudioInstance> callback)
		{
			if (tAudio.IsNullOrEmpty())
			{
				callback(null);
				return;
			}

			SoundHolder setting = null;
			var count = this.sounds.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.sounds[i].tAudio == tAudio)
				{
					setting = this.sounds[i];
					break;
				}
			}

			if (setting == null)
			{
				setting = new SoundHolder {tAudio = tAudio};
				this.sounds.Add(setting);
			}

			SettingsAudioInstance audio = setting.GetAudio();
			if (audio == null)
			{
				Debug.LogWarning("NO AUDIO FILE: " + tAudio);
				callback(null);
			}
			else
			{
				callback(audio);
			}
		}

		public void UnLoadAudio(string tAudio)
		{
			for (var i = sounds.Count - 1; i > -1; i--)
				if (sounds[i].tAudio.Equals(tAudio))
				{
					sounds.RemoveAt(i);
					var soundId = tAudio;
					if (soundsMap.ContainsKey(soundId))
						soundsMap.Remove(soundId);
				}
		}

		public string GetPathToFile(string tAudio)
		{
			if (this.audioList != null)
			{
				var audioName = tAudio.ToString();

				var audios = this.audioList.audios;
				var count = this.audioList.audios.Length;
				for (int i = 0; i < count; i++)
				{
					if (audios[i].name == audioName)
					{
						return "Settings/Audio/" + audios[i].path + "/" + audios[i].name;
					}
				}
			}

			return "";
		}

		#region Music

		private void UpdateAudioMixerMusicVolume()
		{
			audioMixer.SetFloat(MusicVolumeKey, IfMusicEnabled() ? 0 : -80);
		}

		public bool IfMusicEnabled()
		{
			return PlayerPrefs.GetInt(MusicVolumeKey, 1) > 0;
		}

		public void SetMusicEnabledState(bool value)
		{
			PlayerPrefs.SetInt(MusicVolumeKey, value ? 1 : 0);
			UpdateAudioMixerMusicVolume();
		}

		#endregion Music

		#region Sound

		private void UpdateAudioMixerSoundVolume()
		{
			audioMixer.SetFloat(SoundVolumeKey, IfSoundEnabled() ? 0 : -80);
		}

		public bool IfSoundEnabled()
		{
			return PlayerPrefs.GetInt(SoundVolumeKey, 1) > 0;
		}

		public void SetSoundEnabledState(bool value)
		{
			PlayerPrefs.SetInt(SoundVolumeKey, value ? 1 : 0);
			UpdateAudioMixerSoundVolume();
		}

		#endregion Sound

#if UNITY_EDITOR
		[CustomEditor(typeof(ManagerAudioConfig), true)]
		public class SettingsManagerAudioEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUILayout.LabelField("Only Editor Mode:", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(" ", EditorStyles.boldLabel, GUILayout.Width(70));
				var style = new GUIStyle(GUI.skin.button);
				style.normal.textColor = new Color(0, 150f / 255f, 90f / 255f);
				var managerAudioConfig = target as ManagerAudioConfig;
				if (GUILayout.Button("Generate Settings", style, GUILayout.Width(180), GUILayout.Height(20)))
				{
					DeleteEmptySettings(managerAudioConfig);
					var audioFilesJson = GenerateAudioList(managerAudioConfig);
					GenerateSettings(managerAudioConfig, audioFilesJson);
					GenerateTAudio(managerAudioConfig, audioFilesJson);
					GenerateExtensions(managerAudioConfig);
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(5);

				serializedObject.ApplyModifiedProperties();

				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
				EditorGUILayout.Space();

				var attribute = (target as ManagerAudioConfig)?.GetType()
					.GetCustomAttributes(
						typeof(AttributeSettingsTitle),
						true
					).FirstOrDefault() as AttributeSettingsTitle;
				if (attribute != null)
				{
					var s = new GUIStyle(EditorStyles.boldLabel);
					s.normal.textColor = new Color(166f / 255f, 44f / 255f, 44f / 255f);
					EditorGUILayout.LabelField(attribute.title, s);
					EditorGUILayout.Space();
				}

				base.OnInspectorGUI();
				if (GUI.changed)
				{
					EditorUtility.SetDirty(managerAudioConfig);
					serializedObject.ApplyModifiedProperties();
				}
			}

			private string GenerateAudioList(ManagerAudioConfig managerAudioConfig)
			{
				var settings = new AudioList();
				settings.audios = new AudioInfo[0];

				FindAudios(Application.dataPath + $"/{managerAudioConfig.soundsPath}/", ref settings.audios);


				// Старые index'ы расставляем.
				if (managerAudioConfig.audioFilesJson != null && managerAudioConfig.audioFilesJson)
				{
					AudioList audioList = null;
					try
					{
						audioList = JsonUtility.FromJson<AudioList>(managerAudioConfig.audioFilesJson.ToString());
					}
					catch
					{
						audioList = null;
					}


					if (audioList != null && audioList.audios != null && audioList.audios.Length > 0)
					{
						for (var i = 0; i < settings.audios.Length; i++)
						{
							var audio = settings.audios[i];
							var prevAudio = audioList.audios.FirstOrDefault(a => a.name == audio.name);
							audio.index = prevAudio != null ? prevAudio.index : 0;
						}

						// Новые должны попасть после сортировки на самый верх.
						var maxIndex = settings.audios.Max(a => a.index);
						for (var i = 0; i < settings.audios.Length; i++)
							if (settings.audios[i].index == 0)
								settings.audios[i].index = maxIndex + 1;
					}
				}


				Array.Sort(settings.audios, new AudioInfoComparer());

				for (var i = 0; i < settings.audios.Length; i++)
					if (i > 0)
					{
						if (settings.audios[i].index <= settings.audios[i - 1].index)
							settings.audios[i].index = settings.audios[i - 1].index + 1;
					}
					else if (settings.audios[i].index < 1)
					{
						settings.audios[i].index = 1;
					}


				var path = Application.dataPath + $"/{managerAudioConfig.saveDataPath}/AudioList.json";
				var audioFilesJson = JsonUtility.ToJson(settings);


				if (File.Exists(path)) File.Delete(path);

				var sw = new StreamWriter(path);
				sw.Write(audioFilesJson);
				sw.Close();
				AssetDatabase.Refresh();
				var pathThroughData = $"Assets/{managerAudioConfig.saveDataPath}/AudioList.json";
				managerAudioConfig.audioFilesJson = AssetDatabase.LoadAssetAtPath<TextAsset>(pathThroughData);


				Debug.Log("DONE");

				return audioFilesJson;
			}

			private void FindAudios(string path, ref AudioInfo[] audios)
			{
				var info = new DirectoryInfo(path);
				var fileInfo = info.GetFiles("*.*", SearchOption.AllDirectories);
				foreach (var file in fileInfo)
				{
					if (file.Name.Contains(".meta"))
						continue;

					var name = Path.GetFileNameWithoutExtension(file.Name);

					if (name.IsNullOrEmpty())
						continue;

					var extension = file.Extension;
					var directory = file.FullName;
					directory = directory.Replace(@"\", @"/");
					directory = directory.Remove(0, path.Length - 5);
					// Удаляем "Audio/"
					directory = directory.Substring(directory.IndexOf('/') + 1);

					// Удаляем название файла.
					directory = directory.LastIndexOf('/') > 0
						? directory.Substring(0, directory.LastIndexOf('/'))
						: string.Empty;

					directory = Path.ChangeExtension(directory, null);
					var audio = audios.FirstOrDefault(a => a.name == name);
					if (audio != null)
						Debug.LogWarning(
							$"Sound: {audio.name} already added. \r\n prev: {audio.path}  new {directory} \r\n\r\n");
					else
					{
						var audioInfo = new AudioInfo
						{
							path = directory,
							name = name,
							extension = extension,
							index = 0
						};

						audios = audios.Add(audioInfo);
					}
				}
			}

			private void GenerateSettings(ManagerAudioConfig managerAudioConfig, string audioFilesJson)
			{
				var audioList = JsonUtility.FromJson<AudioList>(managerAudioConfig.audioFilesJson.ToString());
				var instances = new List<SettingsAudioInstance>();
				foreach (var audio in audioList.audios)
				{
					var assetFolderPath = Application.dataPath + $"/{managerAudioConfig.saveDataPath}/AudioConfigs/" +
					                      audio.path;
					var assetFilePath = assetFolderPath + "/" + audio.name + ".asset";

					var assetFilePathThroughData =
						$"Assets/{managerAudioConfig.saveDataPath}/AudioConfigs/" + audio.path + "/" +
						audio.name + ".asset";
					var audioFilePathThroughData =
						$"Assets/{managerAudioConfig.soundsPath}/" + audio.path + "/" + audio.name + audio.extension;

					if (!Directory.Exists(assetFolderPath)) Directory.CreateDirectory(assetFolderPath);

					if (!File.Exists(assetFilePath))
					{
						var settingsAudioInstance = CreateInstance<SettingsAudioInstance>();
						var audioClip =
							(AudioClip) AssetDatabase.LoadAssetAtPath(audioFilePathThroughData, typeof(AudioClip));
						settingsAudioInstance.audioClip = audioClip;
						settingsAudioInstance.volume = 1f;
						settingsAudioInstance.pitchMin = 1f;
						settingsAudioInstance.pitchMax = 1f;
						settingsAudioInstance.audioMixer = managerAudioConfig.audioMixerDefaultGroup;

						AssetDatabase.CreateAsset(settingsAudioInstance, assetFilePathThroughData);
						instances.Add(settingsAudioInstance);
					}
					else
					{
						var config =
							(SettingsAudioInstance) AssetDatabase.LoadAssetAtPath(assetFilePathThroughData,
								typeof(SettingsAudioInstance));
						if (!(config is null)) instances.Add(config);
					}
				}

				managerAudioConfig.soundsAssets = instances.ToArray();
			}

			private void GenerateTAudio(ManagerAudioConfig managerAudioConfig, string audioFilesJson)
			{
				var audioList = JsonUtility.FromJson<AudioList>(managerAudioConfig.audioFilesJson.ToString());
				var tAudioFile = new StringBuilder();
				tAudioFile.Append("	public enum TAudio {\r\n");
				tAudioFile.Append("		None = 0,\r\n");
				foreach (var audio in audioList.audios)
					tAudioFile.Append("		" + audio.name + " = " + audio.index + ",\r\n");

				tAudioFile.Append("	}\r\n");
				var path = Application.dataPath + $"/{managerAudioConfig.tAudioPath}/TAudio.cs";
				File.WriteAllText(path, tAudioFile.ToString());
			}

			private void GenerateExtensions(ManagerAudioConfig managerAudioConfig)
			{
				var newString =
@"using AudioManager.Runtime.Core.Manager;

namespace Plugins.AudioManager.Runtime.Core
{
	public static class ManagerAudioExtensions
	{
		public static void PlayAudioClip(this ManagerAudio manager, TAudio audio, float delayExtra = 0)
		{
			manager.PlayAudioClip(audio.ToString(), delayExtra: delayExtra);
		}
	}
}
";
				var path = Application.dataPath + $"/{managerAudioConfig.tAudioPath}/ManagerAudioExtensions.cs";
				File.WriteAllText(path, newString);
			}

			private void DeleteEmptySettings(ManagerAudioConfig managerAudioConfig)
			{
				var guids2 = AssetDatabase.FindAssets("t:SettingsAudioInstance",
					new[] {$"Assets/{managerAudioConfig.saveDataPath}/Audio"});

				foreach (var guid2 in guids2)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid2);
					var settingsAudioInstance =
						(SettingsAudioInstance) AssetDatabase.LoadAssetAtPath(path, typeof(SettingsAudioInstance));

					if (settingsAudioInstance.audioClip == null) AssetDatabase.DeleteAsset(path);
				}
			}
		}
#endif
	}
}