using System;
using System.Globalization;
using System.Windows.Data;

namespace SqlServerRunnerNet.Infrastructure.Converters
{
	[ValueConversion(typeof(bool), typeof(bool))]
	public class InvertBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var boolValue = (bool) value;
			return !boolValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
