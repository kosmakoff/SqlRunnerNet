using System.Linq;
using System.Windows;
using System.Windows.Input;
using SqlServerRunnerNet.Infrastructure.Commands;
using SqlServerRunnerNet.Properties;
using SqlServerRunnerNet.ViewModel;

namespace SqlServerRunnerNet
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public SqlServerRunnerViewModel Model { get; private set; }

		public MainWindow()
		{
			InitializeComponent();

			Model = new SqlServerRunnerViewModel(this);
			DataContext = Model;

			((AwaitableDelegateCommand) Model.RunSelectedScriptsCommand).Executing += (sender, args) =>
			{
				ExecutionProgressBar.Visibility = Visibility.Visible;
			};

			((AwaitableDelegateCommand) Model.RunSelectedScriptsCommand).Executed += (sender) =>
			{
				ExecutionProgressBar.Visibility = Visibility.Collapsed;
			};
		}

		private void BrowseConnectionButton_OnClick(object sender, RoutedEventArgs e)
		{
			var browseWindow = new BrowseConnectionStringWindow(this) { ConnectionString = Model.ConnectionString };
			if (browseWindow.ShowDialog() == true)
				Model.ConnectionString = browseWindow.ConnectionString;
		}

		#region Commands

		private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Application.Current.Shutdown(0);
		}

		#region Clear Output

		private void ClearOutputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (Model == null)
				return;

			Model.ExecutedScripts.Clear();
		}

		private void ClearOutputCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (Model == null)
				return;

			e.CanExecute = Model.ExecutedScripts.Any();
		}

		#endregion

		#endregion

		private void Window_OnLoaded(object sender, RoutedEventArgs e)
		{
			LoadSettings();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SaveSettings();
		}

		private void LoadSettings()
		{
			Model.ConnectionString = Settings.Default.LastConnectionString;
			Model.TopLevelScriptsFolder = Settings.Default.LastCommonFolder;
		}

		private void SaveSettings()
		{
			Settings.Default.LastConnectionString = Model.ConnectionString;
			Settings.Default.LastCommonFolder = Model.TopLevelScriptsFolder;

			Settings.Default.Save();
		}
	}
}
