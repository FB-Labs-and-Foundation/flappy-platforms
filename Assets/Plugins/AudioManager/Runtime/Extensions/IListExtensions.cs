using System;
using System.Collections.Generic;

namespace AudioManager.Runtime.Extensions
{
	public static class IListExtensions
	{
		public static void Shuffle<T>(this IList<T> list, Random random)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var j = random.Next(i, list.Count);
				var temp = list[j];
				list[j] = list[i];
				list[i] = temp;
			}
		}

		public static IList<T> GetRandom<T>(this IList<T> collection, int count)
		{
			if (collection == null || collection.Count < count)
				throw new Exception("ICollection elements count lower then target.");

			var list = new List<T>(collection);
			var random = new Random();
			list.Shuffle(random);
			list.RemoveRange(count - 1, list.Count - count);
			return list;
		}
	}
}