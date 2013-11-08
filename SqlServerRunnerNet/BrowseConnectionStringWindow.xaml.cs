using System.Data.SqlClient;
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

			DataContext = Model;
			Model = new ConnectionStringViewModel();
		}

		public BrowseConnectionStringWindow(Window parent)
			: this()
		{
			Owner = parent;
		}

		private void SetConnectionString(string connectionString)
		{
			var builder = new SqlConnectionStringBuilder(connectionString);

			Model.ServerName = builder.DataSource;

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
			browserWindow.ShowDialog();
		}
	}
}
