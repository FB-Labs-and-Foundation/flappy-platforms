using System;
using UnityEngine;
#if AUDIO_MANAGER_SPINE
using Spine.Unity;
#endif
namespace AudioManager.Runtime.Core.ControllerAudio
{
	[Serializable]
	internal class EventsHolder
	{
		public EventTypes type;
		public string tAudio;

		public string stringValue;
		public int intValue;
		public bool IsSkeletonAnimation;
		public Animator animatorValue;
#if AUDIO_MANAGER_SPINE
		public SkeletonAnimation SkeletonAnimation;
#endif
	}
}