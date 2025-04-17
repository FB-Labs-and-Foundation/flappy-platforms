using Ads.Core.Runtime.AdStatus;
using Playgama.Modules.Advertisement;

namespace Plugins.Playgama.Runtime.Ads
{
	public static class AdStatusConverter
	{
		public static AdStatusValue ConvertStatus(PlaygamaAdStatus playgamaStatus)
		{
			switch (playgamaStatus)
			{
				case PlaygamaAdStatus.Loading:
					return AdStatusValue.InLoading;
				case PlaygamaAdStatus.Opened:
					return AdStatusValue.Ready;

				case PlaygamaAdStatus.Shown:
					return AdStatusValue.Showed;
				case PlaygamaAdStatus.Hidden:
					return AdStatusValue.Hidden;
				case PlaygamaAdStatus.Rewarded:
					return AdStatusValue.SuccessWatched;
				case PlaygamaAdStatus.Closed:
				case PlaygamaAdStatus.Failed:
				default:
					return AdStatusValue.Failed;
			}
		}
	}
}