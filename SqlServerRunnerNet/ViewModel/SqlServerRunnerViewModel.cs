using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SqlServerRunnerNet.Annotations;
using SqlServerRunnerNet.Business;
using SqlServerRunnerNet.Infrastructure;
using SqlServerRunnerNet.Infrastructure.Commands;

namespace SqlServerRunnerNet.ViewModel
{
	public class SqlServerRunnerViewModel : INotifyPropertyChanged
	{
		private TrulyObservableCollection<ScriptsFolderViewModel> _scripts;
		private string _connectionString;
		private bool? _allScriptsChecked;
		private ObservableCollection<FolderViewModel> _executedScripts;
		private readonly Window _parent;
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private SqlServerRunnerViewModel()
		{
			Scripts = new TrulyObservableCollection<ScriptsFolderViewModel>();
			Scripts.ItemChanged += (sender, args) => OnPropertyChanged("AllScriptsChecked");
			Scripts.CollectionChanged += (sender, args) => OnPropertyChanged("AllScriptsChecked");

			ExecutedScripts = new ObservableCollection<FolderViewModel>();

			AddScriptCommand = new DelegateCommand(AddScriptCommandExecute);
			RemoveScriptCommand = new DelegateCommand(RemoveScriptCommandExecute, RemoveScriptCommandCanExecute);
			ClearScriptsCommand = new DelegateCommand(ClearScriptsCommandExecute, ClearScriptsCommandCanExecute);
			MoveScriptUpCommand = new DelegateCommand(MoveScriptUpCommandExecute, MoveScriptUpCommandCanExecute);
			MoveScriptDownCommand = new DelegateCommand(MoveScriptDownCommandExecute, MoveScriptDownCommandCanExecute);
			RunSelectedScriptsCommand = new AwaitableDelegateCommand(RunSelectedScriptsCommandExecute, RunSelectedScriptsCommandCanExecute);
		}

		public SqlServerRunnerViewModel(Window parent)
			:this()
		{
			_parent = parent;
		}

		public string ConnectionString
		{
			get { return _connectionString; }
			set
			{
				if (value == _connectionString) return;
				_connectionString = value;
				OnPropertyChanged();
			}
		}

		public string TopLevelScriptsFolder { get; set; }

		public bool? AllScriptsChecked
		{
			get
			{
				var groups = Scripts.GroupBy(script => script.IsChecked).ToList();

				if (groups.Count == 0)
					_allScriptsChecked = false;
				else if (groups.Count > 1)
					_allScriptsChecked = null;
				else
				_allScriptsChecked = groups[0].Key;

				return _allScriptsChecked;
			}
			set
			{
				if (value.Equals(_allScriptsChecked)) return;

				_allScriptsChecked = value;

				if (value.HasValue)
				{
					Scripts.InvokeWithoutNotify(script=>script.IsChecked = value.Value);
				}

				OnPropertyChanged();
			}
		}

		public TrulyObservableCollection<ScriptsFolderViewModel> Scripts
		{
			get { return _scripts; }
			private set
			{
				if (Equals(value, _scripts)) return;
				_scripts = value;
				OnPropertyChanged();
				OnPropertyChanged("AllScriptsChecked");
			}
		}

		public ObservableCollection<FolderViewModel> ExecutedScripts
		{
			get { return _executedScripts; }
			set
			{
				if (Equals(value, _executedScripts)) return;
				_executedScripts = value;
				OnPropertyChanged();
			}
		}

		#region Commands

		public ICommand AddScriptCommand { get; private set; }
		
		public ICommand RemoveScriptCommand { get; private set; }

		public ICommand ClearScriptsCommand { get; private set; }

		public ICommand MoveScriptUpCommand { get; private set; }
		
		public ICommand MoveScriptDownCommand { get; private set; }

		public ICommand RunSelectedScriptsCommand { get; private set; }

		#endregion

		#region Commands Implementations

		private void AddScriptCommandExecute()
		{
			var existingScriptsInfos = Scripts.Select(script => new DirectoryInfo(script.FilePath)).ToList();

			var browseWindow = new BrowseScriptsWindow(_parent);

			browseWindow.SetTopLevelFolder(TopLevelScriptsFolder);
			if (browseWindow.ShowDialog() == true)
			{
				var newDirectories = browseWindow
					.SelectedDirectories
					.Where(info => !existingScriptsInfos.Contains(info, new DirectoryInfoComparer()))
					.ToList();

				if (!newDirectories.Any())
					return;

				foreach (var directoryInfo in newDirectories)
				{
					Scripts.Add(new ScriptsFolderViewModel {FilePath = directoryInfo.FullName.TrimEnd('\\'), IsChecked = true});
				}

				var last = newDirectories.Last();
				TopLevelScriptsFolder = last.Parent != null ? last.Parent.FullName : last.FullName;
			}
		}

		private void RemoveScriptCommandExecute()
		{
			Scripts.Where(script => script.IsSelected).ToList().ForEach(script => Scripts.Remove(script));
		}

		private bool RemoveScriptCommandCanExecute()
		{
			return Scripts.Any(script => script.IsSelected);
		}

		private void ClearScriptsCommandExecute()
		{
			Scripts.Clear();
		}

		private bool ClearScriptsCommandCanExecute()
		{
			return Scripts.Any();
		}

		private void MoveScriptUpCommandExecute()
		{
			var scriptToMove = Scripts.Single(script => script.IsSelected);
			var index = Scripts.IndexOf(scriptToMove);

			Scripts.RemoveAt(index);
			Scripts.Insert(index - 1, scriptToMove);
		}

		private void MoveScriptDownCommandExecute()
		{
			var scriptToMove = Scripts.Single(script => script.IsSelected);
			var index = Scripts.IndexOf(scriptToMove);

			Scripts.RemoveAt(index);
			Scripts.Insert(index + 1, scriptToMove);
		}

		private bool MoveScriptUpCommandCanExecute()
		{
			return Scripts.Count(script => script.IsSelected) == 1 &&
			       Scripts.IndexOf(Scripts.Single(script => script.IsSelected)) > 0;
		}

		private bool MoveScriptDownCommandCanExecute()
		{
			return Scripts.Count(script => script.IsSelected) == 1 &&
			       Scripts.IndexOf(Scripts.Single(script => script.IsSelected)) < Scripts.Count - 1;
		}

		private async Task RunSelectedScriptsCommandExecute()
		{
			bool isError = false;

			try
			{
				if (string.IsNullOrWhiteSpace(ConnectionString))
					throw new Exception();

				new SqlConnectionStringBuilder { ConnectionString = ConnectionString };
			}
			catch
			{
				MessageBox.Show(_parent, "Invalid connection string", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				isError = true;
			}

			// and here we try to establish the connection

			try
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
				}
			}
			catch
			{
				MessageBox.Show(_parent, "Cannot establish the connection to the server", "Error", MessageBoxButton.OK,
					MessageBoxImage.Error);
				isError = true;
			}

			if (isError)
				await Task.Yield();

			await Task.Factory.StartNew(() =>
			{

				// main part of the APP

				foreach (var scriptFolder in Scripts.Where(script=>script.IsChecked))
				{
					var path = scriptFolder.FilePath;
					var di = new DirectoryInfo(path);
					if (!di.Exists)
						continue;

					var sqlFiles = di.EnumerateFiles("*.sql").ToList();

					if (!sqlFiles.Any())
						continue;

					var executedFolderViewModel = new FolderViewModel {Path = path};

					_parent.Dispatcher.Invoke(() => ExecutedScripts.Add(executedFolderViewModel));
					
					foreach (var fileInfo in sqlFiles)
					{
						var scriptPath = fileInfo.FullName;

						var executedScriptModel = new ScriptViewModel {Path = scriptPath};

						string errorMessage;

						if (!SqlScriptRunner.RunSqlScriptOnConnection(ConnectionString, scriptPath, out errorMessage))
						{
							executedScriptModel.ErrorMessage = errorMessage;
						}

						_parent.Dispatcher.Invoke(() => executedFolderViewModel.Scripts.Add(executedScriptModel));
					}
				}
			});
		}

		private bool RunSelectedScriptsCommandCanExecute()
		{
			return Scripts.Any(script => script.IsChecked);
		}

		#endregion

		class DirectoryInfoComparer : IEqualityComparer<DirectoryInfo>
		{
			public bool Equals(DirectoryInfo x, DirectoryInfo y)
			{
				return x.FullName.TrimEnd('\\').Equals(y.FullName.TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase);
			}

			public int GetHashCode(DirectoryInfo obj)
			{
				return obj.FullName.TrimEnd('\\').ToLowerInvariant().GetHashCode();
			}
		}
	}
}
