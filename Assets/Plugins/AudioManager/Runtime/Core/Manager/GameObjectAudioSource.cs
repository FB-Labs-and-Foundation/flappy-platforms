using System.Collections.Generic;
using UnityEngine;

namespace AudioManager.Runtime.Core.Manager
{
	internal class GameObjectAudioSources
	{
		internal readonly List<AudioSource> audioSources = new();
		internal GameObject gameObject;
	}
}