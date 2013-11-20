using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SqlServerRunnerNet.Annotations;
using SqlServerRunnerNet.Infrastructure;
using SqlServerRunnerNet.Infrastructure.Commands;
using SqlServerRunnerNet.Utils;

namespace SqlServerRunnerNet.ViewModel
{
	public class ConnectionStringViewModel : INotifyPropertyChanged
	{
		private AuthenticationType _authenticationType;
		private string _serverName;
		private string _userName;
		private string _password;
		private string _databaseName;
		private ObservableCollection<string> _recentServerNames;
		private ObservableCollection<string> _databaseNamesList;

		private readonly Window _parentWindow;

		public ICommand UpdateDatabaseNamesCommand { get; private set; }

		#region INotifyPropertyChanged interface implementation
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		public ConnectionStringViewModel(Window parentWindow)
		{
			_parentWindow = parentWindow;

			RecentServerNames = new ObservableCollection<string>();
			DatabaseNamesList = new ObservableCollection<string>();

			UpdateDatabaseNamesCommand = new AwaitableDelegateCommand(UpdateDatabaseNamesCommandExecute);
		}

		public string ServerName
		{
			get { return _serverName; }
			set
			{
				if (value == _serverName) return;
				_serverName = value;
				OnPropertyChanged();
			}
		}

		public string NewServerName
		{
			set
			{
				var newServerNamePrepared = value.Trim().ToUpper();

				if (!string.IsNullOrWhiteSpace(newServerNamePrepared))
				{
					var oldIndex = RecentServerNames.IndexOf(newServerNamePrepared);

					if (oldIndex >= 0)
						RecentServerNames.RemoveAt(oldIndex);

					RecentServerNames.Insert(0, newServerNamePrepared);

					ServerName = newServerNamePrepared;
				}
			}
		}

		public ObservableCollection<string> RecentServerNames
		{
			get { return _recentServerNames; }
			private set
			{
				if (Equals(value, _recentServerNames)) return;
				_recentServerNames = value;
				OnPropertyChanged();
			}
		}

		public AuthenticationType AuthenticationType
		{
			get { return _authenticationType; }
			set
			{
				_authenticationType = value;

				OnPropertyChanged();
				OnPropertyChanged("IsSqlServerUser");
			}
		}

		public string SqlServerUsername
		{
			get { return _userName; }
			set
			{
				if (value == _userName) return;
				_userName = value;
				OnPropertyChanged();
			}
		}

		public string LocalUsername
		{
			get
			{
				var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
				if (windowsIdentity != null)
					return windowsIdentity.Name;

				return string.Empty;
			}
		}

		public string Password
		{
			get { return _password; }
			set
			{
				if (value == _password) return;
				_password = value;
				OnPropertyChanged();
			}
		}

		public string DatabaseName
		{
			get { return _databaseName; }
			set
			{
				if (value == _databaseName) return;
				_databaseName = value;
				OnPropertyChanged();
			}
		}

		public string NewDatabaseName
		{
			set
			{
				var newDatabaseName = value.Trim();

				if (!string.IsNullOrWhiteSpace(newDatabaseName))
				{
					if (DatabaseNamesList.IndexOfIgnoreCase(newDatabaseName) < 0)
						DatabaseNamesList.Add(newDatabaseName);
					
					DatabaseName = newDatabaseName;
				}
			}
		}

		public ObservableCollection<string> DatabaseNamesList
		{
			get { return _databaseNamesList; }
			private set
			{
				if (Equals(value, _databaseNamesList)) return;
				_databaseNamesList = value;
				OnPropertyChanged();
			}
		}

		public bool IsSqlServerUser
		{
			get
			{
				return AuthenticationType == AuthenticationType.SqlServerAuthentication;
			}
		}

		private async Task UpdateDatabaseNamesCommandExecute()
		{
			DoUpdateDatabasesList();
		}

		public void DoUpdateDatabasesList()
		{
			try
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder {DataSource = ServerName};

				if (AuthenticationType == AuthenticationType.WindowsAuthentication)
				{
					connectionStringBuilder.IntegratedSecurity = true;
				}
				else
				{
					connectionStringBuilder.IntegratedSecurity = false;
					connectionStringBuilder.UserID = SqlServerUsername;
					connectionStringBuilder.Password = Password;
				}

				var connectionString = connectionStringBuilder.ConnectionString;

				var list = SqlServerEnumeration.GetDatabaseNames(connectionString);

				_parentWindow.Dispatcher.Invoke(() =>
				{
					DatabaseNamesList.Clear();

					if (!list.Any())
						return;

					foreach (var name in list)
					{
						DatabaseNamesList.Add(name);
					}

					if (DatabaseNamesList.IndexOfIgnoreCase(DatabaseName) < 0)
						DatabaseName = list.First();
				});
			}
			catch
			{
				
			}
		}
	}
}
