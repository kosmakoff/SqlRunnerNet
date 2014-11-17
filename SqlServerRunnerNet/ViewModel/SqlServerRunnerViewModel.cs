using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SqlServerRunnerNet.Business;
using SqlServerRunnerNet.Infrastructure;
using SqlServerRunnerNet.Infrastructure.Commands;

namespace SqlServerRunnerNet.ViewModel
{
	public class SqlServerRunnerViewModel : NotifyPropertyChangedBase
	{
		private TrulyObservableCollection<ScriptsFolderViewModel> _scripts;
		private string _connectionString;
		private bool? _allScriptsChecked;
		private ObservableCollection<FolderViewModel> _executedScripts;
		private readonly Window _parent;
		private int _currentScriptsCount;
		private int _totalScriptsCount;
		private bool _hasTotalScriptsCountBeenDetermined;

		private ConcurrentQueue<ScriptViewModel> _scriptsQueue;
		private ManualResetEvent _mreNoMoreScriptsExpected;

		private CancellationTokenSource _cancellationTokenSource;

		private SqlServerRunnerViewModel()
		{
			Scripts = new TrulyObservableCollection<ScriptsFolderViewModel>();
			Scripts.ItemChanged += (sender, args) => OnPropertyChanged("AllScriptsChecked");
			Scripts.CollectionChanged += (sender, args) => OnPropertyChanged("AllScriptsChecked");

			ExecutedScripts = new ObservableCollection<FolderViewModel>();

			BrowseConnectionStringCommand = new DelegateCommand(BrowseConnectionStringExecute);
			AddScriptCommand = new DelegateCommand(AddScriptCommandExecute);
			RemoveScriptCommand = new DelegateCommand(RemoveScriptCommandExecute, RemoveScriptCommandCanExecute);
			ClearScriptsCommand = new DelegateCommand(ClearScriptsCommandExecute, ClearScriptsCommandCanExecute);
			MoveScriptUpCommand = new DelegateCommand(MoveScriptUpCommandExecute, MoveScriptUpCommandCanExecute);
			MoveScriptDownCommand = new DelegateCommand(MoveScriptDownCommandExecute, MoveScriptDownCommandCanExecute);
			RunSelectedScriptsCommand = new AwaitableDelegateCommand(RunSelectedScriptsCommandExecute, RunSelectedScriptsCommandCanExecute);

			_currentScriptsCountProgress = new Progress<int>(ProgressCurrentScriptsCount);
			_totalScriptsCountProgress = new Progress<int>(ProgressTotalScriptsCount);
			_progressScript = new Progress<ScriptViewModel>(ProgressScript);
		}

		public SqlServerRunnerViewModel(Window parent)
			: this()
		{
			_parent = parent;
		}

		#region Properties

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
					Scripts.InvokeWithoutNotify(script => script.IsChecked = value.Value);
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

		public int CurrentScriptsCount
		{
			get { return _currentScriptsCount; }
			set
			{
				if (value == _currentScriptsCount) return;
				_currentScriptsCount = value;
				OnPropertyChanged();
			}
		}

		public int TotalScriptsCount
		{
			get { return _totalScriptsCount; }
			set
			{
				if (value == _totalScriptsCount) return;
				_totalScriptsCount = value;
				OnPropertyChanged();
			}
		}

		public bool HasTotalScriptsCountBeenDetermined
		{
			get { return _hasTotalScriptsCountBeenDetermined; }
			set
			{
				if (value.Equals(_hasTotalScriptsCountBeenDetermined)) return;
				_hasTotalScriptsCountBeenDetermined = value;
				OnPropertyChanged();
			}
		}

		public bool ExecutionInProgress
		{
			get { return _executionInProgress; }
			set
			{
				if (value.Equals(_executionInProgress)) return;
				_executionInProgress = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Commands

		public ICommand BrowseConnectionStringCommand { get; private set; }

		public ICommand AddScriptCommand { get; private set; }

		public ICommand RemoveScriptCommand { get; private set; }

		public ICommand ClearScriptsCommand { get; private set; }

		public ICommand MoveScriptUpCommand { get; private set; }

		public ICommand MoveScriptDownCommand { get; private set; }

		public ICommand RunSelectedScriptsCommand { get; private set; }

		#endregion

		#region Progress Reporting

		private readonly IProgress<int> _currentScriptsCountProgress;

		private readonly IProgress<int> _totalScriptsCountProgress;

		private readonly IProgress<ScriptViewModel> _progressScript;
		private bool _executionInProgress;

		private void ProgressCurrentScriptsCount(int progress)
		{
			CurrentScriptsCount += progress;
		}

		private void ProgressTotalScriptsCount(int progress)
		{
			TotalScriptsCount += progress;
		}

		private void ProgressScript(ScriptViewModel model)
		{
			var folderViewModel = model.Parent;

			if (!ExecutedScripts.Contains(folderViewModel))
				ExecutedScripts.Add(folderViewModel);

			folderViewModel.Scripts.Add(model);
		}

		#endregion

		#region Commands Implementations

		private void BrowseConnectionStringExecute()
		{
			var browseWindow = new BrowseConnectionStringWindow(_parent) { ConnectionString = ConnectionString };
			if (browseWindow.ShowDialog() == true)
				ConnectionString = browseWindow.ConnectionString;
		}

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
					Scripts.Add(new ScriptsFolderViewModel { FilePath = directoryInfo.FullName.TrimEnd('\\'), IsChecked = true });
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

				// ReSharper disable once ObjectCreationAsStatement
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
				_scriptsQueue = new ConcurrentQueue<ScriptViewModel>();
				_mreNoMoreScriptsExpected = new ManualResetEvent(false);

				CurrentScriptsCount = 0;
				TotalScriptsCount = 0;
				HasTotalScriptsCountBeenDetermined = false;
				ExecutionInProgress = true;

				_cancellationTokenSource = new CancellationTokenSource();
				var token = _cancellationTokenSource.Token;

				var getScriptsTask = new Task(param =>
				{
					var cancellationToken = (CancellationToken)param;

					foreach (var scriptFolder in Scripts.Where(script => script.IsChecked))
					{
						cancellationToken.ThrowIfCancellationRequested();
						var path = scriptFolder.FilePath;
						var di = new DirectoryInfo(path);
						if (!di.Exists)
							continue;

						var sqlFiles = di.EnumerateFiles("*.sql").ToList();

						if (!sqlFiles.Any())
							continue;

						_totalScriptsCountProgress.Report(sqlFiles.Count);

						var folderViewModel = new FolderViewModel { Path = path };

						foreach (var fileInfo in sqlFiles)
						{
							cancellationToken.ThrowIfCancellationRequested();
							var scriptPath = fileInfo.FullName;
							var scriptViewModel = new ScriptViewModel { Parent = folderViewModel, Path = scriptPath };
							_scriptsQueue.Enqueue(scriptViewModel);
						}
					}
				}, token, token);

				var finishedProcessingTask = getScriptsTask.ContinueWith(continuation =>
				{
					HasTotalScriptsCountBeenDetermined = true;
					_mreNoMoreScriptsExpected.Set();
				}, token);

				var processScriptsTask = Task.Factory.StartNew(param =>
				{
					var cancellationToken = (CancellationToken)param;

					while (true)
					{
						cancellationToken.ThrowIfCancellationRequested();

						ScriptViewModel viewModel;
						if (_scriptsQueue.TryDequeue(out viewModel))
						{
							SqlScriptRunner.RunSqlScriptOnConnection(ConnectionString, viewModel);
							_currentScriptsCountProgress.Report(1);
							_progressScript.Report(viewModel);
						}
						else
						{
							if (_mreNoMoreScriptsExpected.WaitOne(TimeSpan.Zero))
								break;
						}
					}
				}, token, token);

				getScriptsTask.Start();

				try
				{
					Task.WaitAll(finishedProcessingTask, processScriptsTask);
				}
				catch (AggregateException aex)
				{
					aex.Handle(ex => ex is TaskCanceledException);
				}

				ExecutionInProgress = false;
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
