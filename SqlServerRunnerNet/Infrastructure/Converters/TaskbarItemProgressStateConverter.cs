using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shell;

namespace SqlServerRunnerNet.Infrastructure.Converters
{
	public class TaskbarItemProgressStateConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var isRunning = (bool) values[0];
			var hasCountedTotal = (bool) values[1];

			return isRunning
				? (hasCountedTotal ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Indeterminate)
				: TaskbarItemProgressState.None;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
