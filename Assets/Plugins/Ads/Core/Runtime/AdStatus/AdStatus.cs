using UnityEngine.Events;

namespace Ads.Core.Runtime.AdStatus
{
	public class AdStatus
	{
		public readonly UnityEvent<AdStatusValue> OnStatusChange;
		public readonly UnityEvent OnSuccessWatched;

		public AdStatusValue Value { get; private set; }

		public AdStatus(AdStatusValue value)
		{
			Value = value;
			OnStatusChange = new UnityEvent<AdStatusValue>();
			OnSuccessWatched = new UnityEvent();
		}

		public void Set(AdStatusValue value)
		{
			Value = value;
			if (Value == AdStatusValue.SuccessWatched)
			{
				OnSuccessWatched.Invoke();
			}
			OnStatusChange.Invoke(Value);
		}
	}
}