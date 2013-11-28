using System.Windows;

namespace SqlServerRunnerNet.Utils.Logging
{
	public static class LogWrapper
	{
		private static readonly ILog Log;

		static LogWrapper()
		{
			Log = ((App)Application.Current).Container.GetExportedValue<ILog>();
		}


		public static void WriteError(string message)
		{
			if (Log != null)
				Log.WriteError(message);
		}
	}
}
