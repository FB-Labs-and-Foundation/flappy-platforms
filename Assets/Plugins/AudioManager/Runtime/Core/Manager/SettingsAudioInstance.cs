using UnityEngine;
using UnityEngine.Audio;

namespace AudioManager.Runtime.Core.Manager
{
	public class SettingsAudioInstance : ScriptableObject
	{
		[SerializeField]
		public AudioClip audioClip;

		[SerializeField]
		public AudioMixerGroup audioMixer;

		[Space(5f)]
		[SerializeField] [Range(0f, 1.0f)]
		public float volume;

		[Space(5f)]
		[SerializeField]
		public bool loop;

		[Space(5f)]
		[SerializeField] [Range(-3.0f, 3.0f)]
		public float pitchMin;

		[SerializeField] [Range(-3.0f, 3.0f)]
		public float pitchMax;

		[Header("3D")]
		[SerializeField]
		public bool is3D;
		[SerializeField]
		public float maxDist3D;

		[Header("Others")]
		[Space(10f)]
		[SerializeField] [Tooltip("Плавное вступление.")]
		public float fadeIn;

		[Space(5f)]
		[SerializeField] [Tooltip("Начать трек не сначала.")]
		public float startTimeMin;

		[SerializeField] [Tooltip("Начать трек не сначала.")]
		public float startTimeMax;

		[Space(5f)]
		[SerializeField] [Tooltip("Задержка перед проигрыванием.")]
		public float delayTime;
	}
}