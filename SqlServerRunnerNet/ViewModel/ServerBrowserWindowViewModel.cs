using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using SqlServerRunnerNet.Annotations;
using SqlServerRunnerNet.Utils;

namespace SqlServerRunnerNet.ViewModel
{
	public class ServerBrowserWindowViewModel : INotifyPropertyChanged
	{
		private ObservableCollection<SqlServerInstance> _localInstancesCollection;
		private ObservableCollection<SqlServerInstance> _remoteInstancesCollection;
		private bool _isLoading;
		public event PropertyChangedEventHandler PropertyChanged;

		private readonly Window _parentWindow;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public ObservableCollection<SqlServerInstance> LocalInstancesCollection
		{
			get { return _localInstancesCollection; }
			private set
			{
				if (Equals(value, _localInstancesCollection)) return;
				_localInstancesCollection = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<SqlServerInstance> RemoteInstancesCollection
		{
			get { return _remoteInstancesCollection; }
			private set
			{
				if (Equals(value, _remoteInstancesCollection)) return;
				_remoteInstancesCollection = value;
				OnPropertyChanged();
			}
		}

		public bool IsLoading
		{
			get { return _isLoading; }
			private set
			{
				if (value.Equals(_isLoading)) return;
				_isLoading = value;
				OnPropertyChanged();
			}
		}

		private ServerBrowserWindowViewModel()
		{
			LocalInstancesCollection = new ObservableCollection<SqlServerInstance>();
			RemoteInstancesCollection = new ObservableCollection<SqlServerInstance>();
			IsLoading = false;
		}

		public ServerBrowserWindowViewModel(Window parentWindow)
			:this()
		{
			_parentWindow = parentWindow;
		}

		public async Task LoadSqlServerInstances()
		{
			IsLoading = true;

			var localInstancesTask = Task.Factory.StartNew(() =>
			{
				var localInstances = SqlServerEnumeration.EnumLocalInstances();

				_parentWindow.Dispatcher.Invoke(() => localInstances.ForEach((instance => LocalInstancesCollection.Add(instance))));
			});

			var remoteInstancesTask = Task.Factory.StartNew(() =>
			{
				var remoteInstances = SqlServerEnumeration.EnumRemoteInstances();

				_parentWindow.Dispatcher.Invoke(() => remoteInstances.ForEach((instance => RemoteInstancesCollection.Add(instance))));
			});

			await Task.WhenAll(localInstancesTask, remoteInstancesTask);

			IsLoading = false;
		}
	}
}
