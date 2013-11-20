using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SqlServerRunnerNet.Infrastructure
{
	public static class ExtensionMethods
	{
		public static int IndexOfIgnoreCase(this IList<string> list, string item)
		{
			for (int i = 0; i < list.Count; i++)
			{
				var str = list[i];

				if (string.Equals(str, item, StringComparison.InvariantCultureIgnoreCase))
					return i;
			}

			return -1;
		}
	}
}
