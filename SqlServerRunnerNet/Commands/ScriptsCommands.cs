using System.Windows.Input;

namespace SqlServerRunnerNet.Commands
{
	public static class ScriptsCommands
	{
		public static readonly ICommand AddCommand = new RoutedUICommand(
			"Add", "Add", typeof(ScriptsCommands));

		public static readonly ICommand RemoveCommand = new RoutedUICommand(
			"Remove", "Remove", typeof(ScriptsCommands));

		public static readonly ICommand ClearCommand = new RoutedUICommand(
			"Clear", "Clear", typeof (ScriptsCommands));

		public static readonly ICommand RunSelectedCommand = new RoutedUICommand(
			"Run Selected", "Run Selected", typeof (ScriptsCommands));

		public static readonly ICommand ClearOutputCommand = new RoutedUICommand(
			"Clear Output", "Clear Output", typeof (ScriptsCommands));
	}
}
