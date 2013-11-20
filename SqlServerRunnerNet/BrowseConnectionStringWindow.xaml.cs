using System;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Windows;
using SqlServerRunnerNet.Infrastructure;
using SqlServerRunnerNet.ViewModel;

namespace SqlServerRunnerNet
{
	/// <summary>
	/// Interaction logic for BrowseConnectionStringWindow.xaml
	/// </summary>
	public partial class BrowseConnectionStringWindow : Window
	{
		public ConnectionStringViewModel Model { get; private set; }

		public string ConnectionString
		{
			get
			{
				return GetConnectionString();
			}
			set
			{
				SetConnectionString(value);
			}
		}

		public BrowseConnectionStringWindow()
		{
			InitializeComponent();

			Model = new ConnectionStringViewModel(this);
			DataContext = Model;

			LoadSettings();

			Loaded += (sender, args) => Model.UpdateDatabaseNamesCommand.Execute(new CancellationTokenSource().Token);
		}

		public BrowseConnectionStringWindow(Window parent)
			: this()
		{
			Owner = parent;
		}

		private void SetConnectionString(string connectionString)
		{
			SqlConnectionStringBuilder builder;

			try
			{
				builder = new SqlConnectionStringBuilder(connectionString);
			}
			catch
			{
				builder = new SqlConnectionStringBuilder();
			}

			Model.NewServerName = builder.DataSource;

			var isSqlUser = !string.IsNullOrWhiteSpace(connectionString) && !builder.IntegratedSecurity;

			Model.AuthenticationType = !isSqlUser
				? AuthenticationType.WindowsAuthentication
				: AuthenticationType.SqlServerAuthentication;

			if (isSqlUser)
			{
				Model.SqlServerUsername = builder.UserID;
				Model.Password = builder.Password;
			}
			else
			{
				Model.SqlServerUsername = string.Empty;
				Model.Password = string.Empty;
			}

			Model.DatabaseName = builder.InitialCatalog;
		}

		private string GetConnectionString()
		{
			var builder = new SqlConnectionStringBuilder();
			builder.Clear();

			builder.DataSource = Model.ServerName;

			if (Model.AuthenticationType == AuthenticationType.WindowsAuthentication)
			{
				builder.IntegratedSecurity = true;
			}
			else
			{
				builder.IntegratedSecurity = false;
				builder.UserID = Model.SqlServerUsername;
				builder.Password = Model.Password;
			}

			builder.InitialCatalog = Model.DatabaseName;

			return builder.ConnectionString;
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void BrowseServersButton_OnClick(object sender, RoutedEventArgs e)
		{
			var browserWindow = new ServerBrowserWindow(this);
			if (browserWindow.ShowDialog() == true)
			{
				Model.NewServerName = browserWindow.SqlServerInstanceName;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SaveSettings();
		}
		
		private void LoadSettings()
		{
			if (Properties.Settings.Default.RecentServerNames == null)
				return;

			Model.RecentServerNames.Clear();
			
			foreach (var serverName in Properties.Settings.Default.RecentServerNames)
			{
				Model.RecentServerNames.Add(serverName);
			}
		}

		private void SaveSettings()
		{
			if (Properties.Settings.Default.RecentServerNames == null)
				Properties.Settings.Default.RecentServerNames = new StringCollection();

			Properties.Settings.Default.RecentServerNames.Clear();
			Properties.Settings.Default.RecentServerNames.AddRange(Model.RecentServerNames.ToArray());
		}
	}
}
