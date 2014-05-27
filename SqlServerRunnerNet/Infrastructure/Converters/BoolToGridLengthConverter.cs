using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SqlServerRunnerNet.Infrastructure.Converters
{
	[ValueConversion(typeof(bool), typeof(GridLength))]
	class BoolToGridLengthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var gridLengthConverter = new GridLengthConverter();

			var gridLengthRaw = gridLengthConverter.ConvertFrom(parameter);
			if (gridLengthRaw == null)
				throw new ArgumentException("Parameter must be convertable to GridLength");

			var defaultGridLength = (GridLength)gridLengthRaw;

			var boolValue = (bool) value;

			return boolValue ? defaultGridLength : new GridLength(0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
