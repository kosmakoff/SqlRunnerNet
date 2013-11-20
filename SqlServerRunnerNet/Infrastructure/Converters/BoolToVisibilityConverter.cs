using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SqlServerRunnerNet.Infrastructure.Converters
{
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibilityConverter : IValueConverter
	{
		public bool IsInverted { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var boolValue = (bool) value;

			return boolValue ^ IsInverted ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
