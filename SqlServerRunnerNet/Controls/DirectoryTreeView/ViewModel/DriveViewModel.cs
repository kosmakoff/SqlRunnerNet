using SqlServerRunnerNet.Controls.DirectoryTreeView.Data;

namespace SqlServerRunnerNet.Controls.DirectoryTreeView.ViewModel
{
	public class DriveViewModel : DirectoryTreeViewItemViewModel
	{
		private readonly Folder _folder;

		public DriveViewModel(Folder folder)
			: base(null, true)
		{
			_folder = folder;
		}

		public override string DisplayPath
		{
			get { return _folder.FolderPath; }
		}

		public override string FullPath
		{
			get { return _folder.FolderPath; }
		}
	}
}
