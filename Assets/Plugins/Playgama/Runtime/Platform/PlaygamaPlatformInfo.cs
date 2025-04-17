using System;
using Playgama;
using Playgama.Modules.Platform;

public class PlaygamaPlatformInfo
{
	private string _platformId;
	private string _language;
	private string _payload;
	private string _tld;

	public string PlatformId
	{
		get
		{
			if (string.IsNullOrEmpty(_platformId))
			{
				_platformId = PlaygamaSDKProxy.PlaygamaBridgeGetPlatformId();
			}

			return _platformId;
		}
	}

	public string Language
	{
		get
		{
			if (string.IsNullOrEmpty(_language))
			{
				_language = PlaygamaSDKProxy.PlaygamaBridgeGetPlatformLanguage();
			}

			return _language;
		}
	}

	public string Payload
	{
		get
		{
			if (string.IsNullOrEmpty(_payload))
			{
				_payload = PlaygamaSDKProxy.PlaygamaBridgeGetPlatformPayload();
			}

			return _payload;
		}
	}

	public string Tld
	{
		get
		{
			if (string.IsNullOrEmpty(_tld))
			{
				_tld = PlaygamaSDKProxy.PlaygamaBridgeGetPlatformTld();
			}

			return _tld;
		}
	}

	public void SendMessage(PlatformMessage message)
	{
		var messageString = "";

		switch (message)
		{
			case PlatformMessage.GameReady:
				messageString = "game_ready";
				break;

			case PlatformMessage.InGameLoadingStarted:
				messageString = "in_game_loading_started";
				break;

			case PlatformMessage.InGameLoadingStopped:
				messageString = "in_game_loading_stopped";
				break;

			case PlatformMessage.GameplayStarted:
				messageString = "gameplay_started";
				break;

			case PlatformMessage.GameplayStopped:
				messageString = "gameplay_stopped";
				break;

			case PlatformMessage.PlayerGotAchievement:
				messageString = "player_got_achievement";
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(message), message, null);
		}

#if !UNITY_EDITOR
		PlaygamaSDKProxy.PlaygamaBridgeSendMessageToPlatform(messageString);
#endif
	}
}