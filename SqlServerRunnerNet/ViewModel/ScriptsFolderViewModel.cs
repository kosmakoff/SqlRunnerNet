namespace SqlServerRunnerNet.ViewModel
{
	public class ScriptsFolderViewModel : NotifyPropertyChangedBase
	{
		private bool _isChecked;
		private string _filePath;
		private bool _isSelected;

		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				if (value.Equals(_isChecked)) return;
				_isChecked = value;
				OnPropertyChanged();
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value.Equals(_isSelected)) return;
				_isSelected = value;
				OnPropertyChanged();
			}
		}

		public string FilePath
		{
			get { return _filePath; }
			set
			{
				if (value == _filePath) return;
				_filePath = value;
				OnPropertyChanged();
			}
		}
	}
}
