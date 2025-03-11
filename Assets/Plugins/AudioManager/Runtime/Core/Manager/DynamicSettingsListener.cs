using UnityEngine;

namespace AudioManager.Runtime.Core.Manager
{
	internal class DynamicSettingsListener
	{
		public readonly AudioSource audioSource;

		private readonly ControllerAudio.ControllerAudio hashControllerAudio;
		public readonly SettingsAudioInstance settingsAudioInstance;
		private float hashPitchMax;

		private float hashPitchMin;

		public DynamicSettingsListener(AudioSource audioSource, SettingsAudioInstance settingsAudioInstance)
		{
			this.audioSource = audioSource;
			this.settingsAudioInstance = settingsAudioInstance;

			UpdatePitch();

			hashControllerAudio = this.audioSource.GetComponent<ControllerAudio.ControllerAudio>();
		}

		public void UpdatePitch()
		{
			hashPitchMin = settingsAudioInstance.pitchMin;
			hashPitchMax = settingsAudioInstance.pitchMax;
		}

		public ControllerAudio.ControllerAudio GetControllerAudio()
		{
			return hashControllerAudio;
		}

		public float GetHashPitchMin()
		{
			return hashPitchMin;
		}

		public float GetHashPitchMax()
		{
			return hashPitchMax;
		}
	}
}