using System.Windows.Input;

namespace SqlServerRunnerNet.Commands
{
	public static class ApplicationCommands
	{
		public static readonly ICommand ExitCommand = new RoutedUICommand(
			"Exit", "Exit", typeof (ApplicationCommands),
			new InputGestureCollection {new KeyGesture(Key.F4, ModifierKeys.Alt)});
	}
}
