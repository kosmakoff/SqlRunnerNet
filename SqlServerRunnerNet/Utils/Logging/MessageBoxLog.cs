using System.ComponentModel.Composition;
using System.Windows;

namespace SqlServerRunnerNet.Utils.Logging
{
#if DEBUG
	[Export(typeof(ILog))]
#endif
	public class MessageBoxLog : ILog
	{
		public void WriteError(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public void WriteInfo(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
