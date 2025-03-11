using System.Collections.Generic;

namespace AudioManager.Runtime.Core.ManagerAudioConfig
{
	internal class AudioInfoComparer : IComparer<AudioInfo>
	{
		public int Compare(AudioInfo x, AudioInfo y)
		{
			return x.index >= y.index ? 1 : -1;
		}
	}
}