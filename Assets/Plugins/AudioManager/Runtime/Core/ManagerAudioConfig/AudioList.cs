using System;

namespace AudioManager.Runtime.Core.ManagerAudioConfig
{
	[Serializable]
	public class AudioList
	{
		public AudioInfo[] audios;
	}

	[Serializable]
	public class AudioInfo
	{
		public string path;
		public string name;
		public string extension;
		public int index;
	}
}