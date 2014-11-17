using System;
using System.Windows.Data;

namespace SqlServerRunnerNet.Infrastructure.Converters
{
	public class FractionConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var value1 = (int)values[0];
			var value2 = (int)values[1];

			if (Math.Abs(value2) < 1e-6)
				return 0;

			return value1 / (double)value2;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
