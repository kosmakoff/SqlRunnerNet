namespace SqlServerRunnerNet.Utils.Logging
{
	interface ILog
	{
		void WriteError(string message);

		void WriteInfo(string message);
	}
}
