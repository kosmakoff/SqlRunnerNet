using System.IO;
using SqlServerRunnerNet.Controls.DirectoryTreeView.Data;

namespace SqlServerRunnerNet.Controls.DirectoryTreeView.ViewModel
{
	public class FolderViewModel : DirectoryTreeViewItemViewModel
	{
		private readonly Folder _folder;
		private readonly string _displayPath;

		public FolderViewModel(Folder folder, DirectoryTreeViewItemViewModel parentFolderViewModel)
			: base(parentFolderViewModel, true)
		{
			_folder = folder;

			var dir = new DirectoryInfo(_folder.FolderPath);
			_displayPath = dir.Name;
		}

		public override string FullPath
		{
			get { return _folder.FolderPath; }
		}

		public override string DisplayPath
		{
			get { return _displayPath; }
		}
	}
}
