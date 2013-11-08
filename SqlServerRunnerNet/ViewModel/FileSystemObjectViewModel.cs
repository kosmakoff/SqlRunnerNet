using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SqlServerRunnerNet.Annotations;
using SqlServerRunnerNet.Infrastructure.Commands;

namespace SqlServerRunnerNet.ViewModel
{
	public abstract class FileSystemObjectViewModel : INotifyPropertyChanged
	{
		private string _path;
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

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
