namespace SqlServerRunnerNet.Infrastructure.Commands
{
	public interface IRaiseCanExecuteChanged
	{
		void RaiseCanExecuteChanged();
	}

	// And an extension method to make it easy to raise changed events
}
