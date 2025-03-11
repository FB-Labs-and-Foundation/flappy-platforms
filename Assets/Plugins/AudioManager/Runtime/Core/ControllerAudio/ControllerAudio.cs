using System.Collections.Generic;
using System.Linq;
using AudioManager.Runtime.Core.Manager;
using AudioManager.Runtime.Core.ManagerAudioConfig;
using AudioManager.Runtime.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

#if AUDIO_MANAGER_SPINE
using Spine.Unity;
#endif

#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEditor;
#endif

namespace AudioManager.Runtime.Core.ControllerAudio
{
	public class ControllerAudio : MonoBehaviour, IPointerClickHandler
	{
		#pragma warning disable 0649
		[SerializeField]
		private ManagerAudioConfig.ManagerAudioConfig _audioConfig;

		[SerializeField]
		private EventsHolder[] events;

		[SerializeField]
		private bool isCameraZoomDependence;
		#pragma warning restore 0649

		private ManagerAudio _managerAudio;
		private AudioSource hashAudioSource;

		private bool isAnimationEvents;

		private bool isInitialized;
		private string prevAnimationName;
		private string prevAnimationTAudio;
		private void Awake()
		{
			_managerAudio = ManagerAudio.SharedInstance;
		}

		private void Start()
		{
			var isComponentActive = false;
			if (this.isCameraZoomDependence) {
				isComponentActive = true;

				_managerAudio.OmCameraZoomChanged += this.ChangeVolumeByZoom;
			}
			else
			{
				for (var i = 0; i < events.Length; i++)
				{
					var _event = events[i];
					if (_event.type == EventTypes.Animation)
					{
						isComponentActive = true;
						isAnimationEvents = true;
					}
					else if (_event.type == EventTypes.Click)
					{
						isComponentActive = true;
					}
					else if (_event.type == EventTypes.Start)
					{
						isComponentActive = true;
					}
				}
			}

			enabled = isComponentActive;

			isInitialized = true;

			OnEnable();
		}

		private void FixedUpdate()
		{
			if (isAnimationEvents)
			{
				var isFound = false;
				for (var i = 0; i < events.Length; i++)
					if (events[i].type == EventTypes.Animation)
					{
						var _event = events[i];

#if AUDIO_MANAGER_SPINE
						if (_event.animatorValue != null || _event.SkeletonAnimation != null)
#else
						if (_event.animatorValue != null)
#endif
						{
							if (_event.animatorValue != null)
								if (!_event.animatorValue.GetCurrentAnimatorStateInfo(_event.intValue)
									    .IsName(_event.stringValue))
									continue;

#if AUDIO_MANAGER_SPINE
							if (_event.SkeletonAnimation != null)
								if (!_event.SkeletonAnimation.AnimationName
									    .Equals(_event.stringValue))
									continue;
#endif

							isFound = true;

							// Если только включилась.
							if (prevAnimationName != _event.stringValue)
							{
								if (prevAnimationName != null)
									_managerAudio.StopAudioIfLoop(prevAnimationTAudio, gameObject, 0.1f, 0f, true);

								prevAnimationName = _event.stringValue;
								prevAnimationTAudio = _event.tAudio;
								_managerAudio.PlayAudioClip(gameObject, _event.tAudio, null, 0f, true);
								break;
							}
						}
					}

				if (!isFound && prevAnimationName != null)
				{
					_managerAudio.StopAudioIfLoop(prevAnimationTAudio, gameObject, 0.1f, 0f, true);

					prevAnimationName = null;
				}
			}
		}

		private void OnEnable()
		{
			if (!isInitialized) return;

			for (var i = 0; i < events.Length; i++)
				if (events[i].type == EventTypes.Start)
					_managerAudio.PlayAudioClip(gameObject, events[i].tAudio, null, 0f, true);
		}

		private void OnDisable()
		{
			if (!isInitialized) return;

			for (var i = 0; i < events.Length; i++)
				if (events[i].type == EventTypes.Start)
					_managerAudio.StopAudio(gameObject, events[i].tAudio);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			for (var i = 0; i < events.Length; i++)
				if (events[i].type == EventTypes.Click)
					_managerAudio.PlayAudioClip(gameObject, events[i].tAudio, null, 0f, true);
		}

		public bool IfCameraZoomDependence() {
			return this.isCameraZoomDependence;
		}

		private AudioSource GetAudioSource()
		{
			if (hashAudioSource == null)
				hashAudioSource = GetComponent<AudioSource>();

			return hashAudioSource;
		}

		public void StopLastAnimationAudio()
		{
			_managerAudio.StopAudio(prevAnimationTAudio);
		}

		private void ChangeVolumeByZoom(float cameraOrhographicSize)
		{
			var volume = (cameraOrhographicSize - 4f) / (11f - 4f);
			var audioSource = GetAudioSource();
			if (audioSource != null) audioSource.volume = 0.5f + (1 - volume) * 0.5f;
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(ControllerAudio))]
		public class ControllerAudioEditor : UnityEditor.Editor
		{
			private string[] audioDropdownArray;
			private TextAsset audioFilesJson;
			private string[] tAudioNames;

			public override void OnInspectorGUI()
			{
				var _target = (ControllerAudio) target;

				serializedObject.Update();

				EditorGUILayout.Space();
				_target._audioConfig = EditorGUILayout.ObjectField("", _target._audioConfig, typeof(ManagerAudioConfig.ManagerAudioConfig), true) as ManagerAudioConfig.ManagerAudioConfig;
				EditorGUILayout.Space();
				_target.isCameraZoomDependence = EditorGUILayout.Toggle("Volume Camera Zoom Dependence", _target.isCameraZoomDependence);
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				ShowEventsArray(serializedObject.FindProperty("events"), _target);

				if (GUI.changed)
				{
					EditorUtility.SetDirty(target);
					serializedObject.ApplyModifiedProperties();
				}
			}

			private void ShowEventsArray(SerializedProperty list, ControllerAudio _target)
			{
				//EditorGUILayout.PropertyField(list);
				EditorGUI.indentLevel += 1;

				if (audioFilesJson == null)
				{
					audioFilesJson =
						(TextAsset) AssetDatabase.LoadAssetAtPath(
							$"Assets/{_target._audioConfig.SaveDataPath}/AudioList.json",
							typeof(TextAsset));
					if (audioFilesJson is null)
					{
						var msg = _target._audioConfig.SaveDataPath.IsNullOrEmpty()
							? "AudioList.json NOT FOUND! Save Data Path is Empty!"
							: $"Assets/{_target._audioConfig.SaveDataPath}/AudioList.json NOT FOUND";
						Debug.Log(msg);
						return;
					}
					var audioList = JsonUtility.FromJson<AudioList>(audioFilesJson.ToString());

					var titles = new List<string>();
					titles.Add("None");
					for (var i = 0; i < audioList.audios.Length; i++)
					{
						var audio = audioList.audios[i];
						titles.Add(audio.name);
					}

					audioDropdownArray = titles.ToArray();

					tAudioNames = new string[titles.Count()];
					var j = 0;
					foreach (var audio in titles)
					{
						tAudioNames[j] = audio;

						j++;
					}
				}

				if (list.isExpanded)
				{
					EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
					for (var i = 0; i < list.arraySize; i++)
					{
						var element = list.GetArrayElementAtIndex(i);
						GUILayout.Label("");
						var _event = null != _target.events && i < _target.events.Length ? _target.events[i] : null;

						EditorGUILayout.PropertyField(element,
							includeChildren: false,
							label: new GUIContent(_event != null ? _event.tAudio : "AddToBegin", ""));

						EditorGUI.indentLevel += 1;
						if (element.isExpanded && null != _event)
						{
							_event.type = (EventTypes) EditorGUILayout.EnumPopup("Type", _event.type);


							var audioDropdownArrayIndex = 0;
							var tAudioName = _event.tAudio;
							for (var j = 0; j < audioDropdownArray.Length; j++)
								if (audioDropdownArray[j].EndsWith(tAudioName))
								{
									audioDropdownArrayIndex = j;
									break;
								}

							var index = EditorGUILayout.Popup("Audio",
								audioDropdownArrayIndex,
								audioDropdownArray);
							var value = audioDropdownArray[index];

							_event.tAudio = default;
							for (var j = 0; j < tAudioNames.Length; j++)
								if (value.EndsWith(tAudioNames[j]))
								{
									_event.tAudio = tAudioNames[j];
									break;
								}


							switch (_event.type)
							{
								case EventTypes.Start:
								case EventTypes.Click:
									break;
								case EventTypes.Animation:
									_event.IsSkeletonAnimation =
										EditorGUILayout.Toggle("Is Skeleton Animation", _event.IsSkeletonAnimation);
									if (_event.IsSkeletonAnimation)
									{
#if AUDIO_MANAGER_SPINE
										_event.SkeletonAnimation =
											(SkeletonAnimation) EditorGUILayout.ObjectField("Animator",
												_event.SkeletonAnimation,
												typeof(SkeletonAnimation),
												allowSceneObjects: true);
										if (_event.SkeletonAnimation != null)
										{
											var animations = _event.SkeletonAnimation.skeleton.Data.Animations.Items;
											int selectedIndex = 0;

											for (int j = 0; j < animations.Length; j++)
											{
												if (_event.stringValue == animations[j].Name)
												{
													selectedIndex = j;
													break;
												}
											}

											string[] stringAnimationsArr = new string [animations.Length];
											for (int j = 0; j < animations.Length; j++)
											{
												stringAnimationsArr[j] = animations[j].Name;
											}

											selectedIndex = EditorGUILayout.Popup("Animation",
												selectedIndex,
												stringAnimationsArr.ToArray());
											_event.stringValue = stringAnimationsArr[selectedIndex];
											_event.intValue = selectedIndex;
											//TODO сворачиваются анимации
										}
#endif
									}
									else
									{
										_event.animatorValue = (Animator) EditorGUILayout.ObjectField("Animator",
											_event.animatorValue,
											typeof(Animator),
											allowSceneObjects: true);

										if (_event.animatorValue != null)
										{
											var animationNamesList = new List<string>();
											var animationLayersList = new List<int>();

											var animatorController =
												_event.animatorValue.runtimeAnimatorController as AnimatorController;
											for (int layerIndex = 0;
											     layerIndex < animatorController.layers.Length;
											     layerIndex++)
											{
												var layer = animatorController.layers[layerIndex];
												var states = layer.stateMachine.states;
												for (int k = 0; k < states.Length; k++)
												{
													animationLayersList.Add(layerIndex);
													animationNamesList.Add(states[k].state.name);
												}
											}


											int selectedIndex = 0;
											for (int j = 0; j < animationNamesList.Count; j++)
											{
												if (_event.intValue == animationLayersList[j]
												    && _event.stringValue == animationNamesList[j])
												{
													selectedIndex = j;
													break;
												}
											}

											selectedIndex = EditorGUILayout.Popup("Animation",
												selectedIndex,
												animationNamesList.ToArray());
											_event.stringValue = animationNamesList[selectedIndex];
											_event.intValue = animationLayersList[selectedIndex];
										}
									}

									break;
							}
						}

						EditorGUI.indentLevel -= 1;
					}
				}

				EditorGUI.indentLevel -= 1;
			}
		}
#endif
	}
}