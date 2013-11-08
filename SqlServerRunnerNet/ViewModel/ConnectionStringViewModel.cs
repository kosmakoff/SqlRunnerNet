using System.ComponentModel;
using System.Runtime.CompilerServices;
using SqlServerRunnerNet.Annotations;
using SqlServerRunnerNet.Infrastructure;

namespace SqlServerRunnerNet.ViewModel
{
	public class ConnectionStringViewModel : INotifyPropertyChanged
	{
		private AuthenticationType _authenticationType;
		private string _serverName;
		private string _userName;
		private string _password;
		private string _databaseName;

		#region INotifyPropertyChanged interface implementation
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

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
				OnPropertyChanged("SqlServerUsername");
			}
		}

		public string LocalUsername
		{
			get
			{
				return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			}
		}

		public string Password
		{
			get { return _password; }
			set
			{
				if (value == _password) return;
				_password = value;
				OnPropertyChanged("Password");
			}
		}

		public string DatabaseName
		{
			get { return _databaseName; }
			set
			{
				if (value == _databaseName) return;
				_databaseName = value;
				OnPropertyChanged("DatabaseName");
			}
		}

		public bool IsSqlServerUser
		{
			get
			{
				return AuthenticationType == AuthenticationType.SqlServerAuthentication;
			}
		}
	}
}
