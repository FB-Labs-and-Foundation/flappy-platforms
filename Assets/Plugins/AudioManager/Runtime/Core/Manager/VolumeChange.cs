using UnityEngine;

namespace AudioManager.Runtime.Core.Manager
{
	internal class VolumeChange
	{
		/// <summary>
		///     Проверяем не изменился ли клип в проигрывателе.
		/// </summary>
		internal readonly AudioClip audioClip;

		internal readonly AudioSource audioSource;

		private readonly float changeSpeedInSeconds;

		internal readonly float targetValue;

		/// <summary>
		///     Если равно 0, значит мгновенно.
		/// </summary>
		internal float changeSpeed;

		internal float delayTimer;
		internal bool isCompleteTimer = true;

		/// <summary>
		///     Если delayTimer закончился.
		/// </summary>
		private bool isTimerOverProcessed;

		internal VolumeChange(AudioSource audioSource, float targetValue, float changeSpeedInSeconds, float delay)
		{
			this.audioSource = audioSource;
			audioClip = this.audioSource.clip;

			this.targetValue = targetValue;
			this.changeSpeedInSeconds = changeSpeedInSeconds;
			changeSpeed = 0;

			delayTimer = delay;
			isCompleteTimer = false;
			isTimerOverProcessed = false;

			UpdateChangeSpeed();
		}

		public void UpdateChangeSpeed()
		{
			changeSpeed = changeSpeedInSeconds == 0
				? 0
				: Mathf.Abs(targetValue - audioSource.volume) / changeSpeedInSeconds;
		}

		public bool IfTimerOverProcessed()
		{
			return isTimerOverProcessed;
		}

		public void TimerOverProcessed()
		{
			isTimerOverProcessed = true;
		}
	}
}