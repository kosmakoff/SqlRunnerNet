using System.IO;

namespace SqlServerRunnerNet.ViewModel
{
	public class ScriptViewModel : FileSystemObjectViewModel
	{
		private string _errorMessage;

		public override bool HasError
		{
			get { return !string.IsNullOrWhiteSpace(ErrorMessage); }
		}

		public override string DisplayName
		{
			get { return new FileInfo(Path).Name; }
		}

		public string ErrorMessage
		{
			get { return _errorMessage; }
			set
			{
				if (value == _errorMessage) return;

				_errorMessage = value;
				OnPropertyChanged();
				OnPropertyChanged("HasError");
			}
		}

		public FolderViewModel Parent { get; set; }
	}
}
