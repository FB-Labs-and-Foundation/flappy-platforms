using UnityEngine;

namespace Plugins.Social.Core.Runtime.Leaderboard
{
	public abstract class ALeaderboardManager : MonoBehaviour
	{

		public abstract void Initialize();
		public abstract void SetPlayerScore(int score);
		public abstract void GetPlayerScore();

		public abstract bool ShowLeaderboardNativePopup();

		public abstract void GetLeaderboardEntries();


		public abstract int OnPlayerScore(string result);
	}
}