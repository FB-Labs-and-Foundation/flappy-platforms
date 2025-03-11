using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioManager.Runtime.Extensions
{
	public static class ICollectionExtensions
	{
		public static T GetRandom<T>(this ICollection<T> collection)
		{
			if (collection == null || collection.Count == 0) return default;
			return collection.ElementAtOrDefault(Random.Range(0, collection.Count));
		}

		public static T Pop<T>(this ICollection<T> collection)
		{
			if (collection == null || collection.Count == 0)
				return default;

			var element = collection.ElementAtOrDefault(collection.Count - 1);
			collection.Remove(element);

			return element;
		}

		public static T PopRandom<T>(this ICollection<T> collection)
		{
			if (collection == null || collection.Count == 0)
				return default;

			var element = collection.ElementAtOrDefault(Random.Range(0, collection.Count));
			collection.Remove(element);
			return element;
		}
	}
}