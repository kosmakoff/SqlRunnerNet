using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SqlServerRunnerNet.Infrastructure.Commands;

namespace SqlServerRunnerNet.ViewModel
{
	public abstract class FileSystemObjectViewModel : NotifyPropertyChangedBase
	{
		private string _path;

		protected FileSystemObjectViewModel()
		{
			CopyPathCommand = new DelegateCommand(CopyPathCommandExecute);
			OpenInShellCommand = new DelegateCommand(OpenInShellCommandExecute);
		}

		public abstract bool HasError { get; }

		public string Path
		{
			get { return _path; }
			set
			{
				if (value == _path) return;
				_path = value;
				OnPropertyChanged();
				OnPropertyChanged("DisplayName");
			}
		}

		public abstract string DisplayName { get; }

		#region Commands

		public ICommand CopyPathCommand { get; private set; }

		public ICommand OpenInShellCommand { get; private set; }

		private void CopyPathCommandExecute()
		{
			Clipboard.SetText(Path);
		}

		private void OpenInShellCommandExecute()
		{
			Process.Start(Path);
		}

		#endregion
	}
}
