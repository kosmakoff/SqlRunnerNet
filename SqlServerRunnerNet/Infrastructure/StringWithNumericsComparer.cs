using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServerRunnerNet.Infrastructure
{
	public class StringWithNumericsComparer : IComparer<string>
	{
		private static readonly string[] Separators = { "_", ".", "-" };

		public int Compare(string s1, string s2)
		{
			var s1Parts = SplitString(s1);
			var s2Parts = SplitString(s2);

			var zip = s1Parts.Zip(s2Parts, (s1Part, s2Part) => new Tuple<string, string>(s1Part, s2Part));

			foreach (var tuple in zip)
			{
				int s1Int, s2Int;

				bool s1IsInt = int.TryParse(tuple.Item1, out s1Int);
				bool s2IsInt = int.TryParse(tuple.Item2, out s2Int);

				// if both are ints - compare as ints
				if (s1IsInt && s2IsInt)
				{
					var compare = s1Int - s2Int;

					if (compare != 0)
						return compare;
				}

				// string is always "less than" int
				if (!s1IsInt && s2IsInt)
					return -1;

				if (s1IsInt && !s2IsInt)
					return 1;

				// for two strings - compare them ignroing the case
				if (!s1IsInt/* && !s2IsInt*/)
				{
					var compare = string.Compare(tuple.Item1, tuple.Item2, StringComparison.OrdinalIgnoreCase);
					if (compare != 0)
						return compare;
				}
			}

			// since common items didn't yield any result, use sequences lengths to compare
			return s1Parts.Length - s2Parts.Length;
		}

		private static string[] SplitString(string @string)
		{
			return @string.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
		}
	}
}
