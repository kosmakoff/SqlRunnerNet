using System.Linq;
using SqlServerRunnerNet.Infrastructure;

namespace SqlServerRunnerNet.ViewModel
{
	public class FolderViewModel : FileSystemObjectViewModel
	{
		private TrulyObservableCollection<ScriptViewModel> _scripts;

		public FolderViewModel()
		{
			Scripts = new TrulyObservableCollection<ScriptViewModel>();
			
			Scripts.ItemChanged += (sender, args) =>
			{
				OnPropertyChanged("HasError");
				OnPropertyChanged("SucceededScriptsCount");
				OnPropertyChanged("FailedScriptsCount");
			};

			Scripts.CollectionChanged += (sender, args) =>
			{
				OnPropertyChanged("HasError");
				OnPropertyChanged("SucceededScriptsCount");
				OnPropertyChanged("FailedScriptsCount");
			};
		}

		public override bool HasError
		{
			get { return Scripts.Any(script => script.HasError); }
		}

		public override string DisplayName
		{
			get { return Path; }
		}

		public TrulyObservableCollection<ScriptViewModel> Scripts
		{
			get { return _scripts; }
			set
			{
				if (Equals(value, _scripts)) return;
				_scripts = value;
				OnPropertyChanged();
			}
		}

		public int SucceededScriptsCount
		{
			get
			{
				return Scripts.Count(script => !script.HasError);
			}
		}

		public int FailedScriptsCount
		{
			get
			{
				return Scripts.Count(script => script.HasError);
			}
		}

		#region Commands



		#endregion
	}
}
