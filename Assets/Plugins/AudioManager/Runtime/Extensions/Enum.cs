using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace AudioManager.Runtime.Extensions
{
	public class Enum<T> where T : struct, IConvertible
	{
		public static int Count
		{
			get
			{
				if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type.");

				return Enum.GetNames(typeof(T)).Length;
			}
		}

		public static IEnumerable<T> GetValues()
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static IEnumerable<T> GetValues(params T[] exept)
		{
			return Enum.GetValues(typeof(T)).Cast<T>().Except(exept);
		}

		/// <summary>
		///     Parse enum value.
		/// </summary>
		public static T Parse(string value, T defaultValue)
		{
			if (Enum.TryParse(value, true, out T result)) return result;

			return defaultValue;
		}

		public static T PickRandom(params T[] except)
		{
			var count = Count;

			if (count == 0) return default;

			var values = GetValues();

			if (except != null)
			{
				var exceptIndexesCount = except.Length;
				var exceptIndexes = new int[exceptIndexesCount];
				var exceptIndexesIterator = 0;
				var valuesIterator = 0;
				foreach (var value in values)
				{
					for (var i = 0; i < exceptIndexesCount; i++)
						if (except[i].Equals(value))
						{
							exceptIndexes[exceptIndexesIterator++] = valuesIterator;
							break;
						}

					if (exceptIndexesIterator == exceptIndexesCount) break;

					valuesIterator++;
				}


				var startRandomIndex = Random.Range(0, count);
				var randomIndex = startRandomIndex;
				while (true)
				{
					var isExcept = false;
					var exceptLength = except.Length;
					for (var i = 0; i < exceptLength; i++)
						if (exceptIndexes[i] == randomIndex)
						{
							isExcept = true;
							break;
						}

					if (!isExcept) return values.ElementAt(randomIndex);

					if (++randomIndex == count) randomIndex = 0;

					if (randomIndex == startRandomIndex) break;
				}
			}

			return values.ElementAt(Random.Range(0, count));
		}
	}
}