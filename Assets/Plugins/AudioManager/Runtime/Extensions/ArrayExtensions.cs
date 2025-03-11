namespace AudioManager.Runtime.Extensions
{
	public static class ArrayExtensions
	{
		public static T[] Add<T>(this T[] array, params T[] values)
		{
			if (array == null) array = new T[] { };

			if (values == null) values = new T[] { };

			var result = new T[array.Length + values.Length];
			array.CopyTo(result, 0);
			values.CopyTo(result, array.Length);

			return result;
		}

		public static T[] RemoveAt<T>(this T[] array, int index)
		{
			if (array == null) return new T[] { };
			if (index >= array.Length) return array;

			var result = new T[array.Length - 1];
			var offset = 0;
			for (var i = 0; i < result.Length; i++)
			{
				if (i == index) offset++;
				result[i] = array[i + offset];
			}

			return result;
		}
	}
}