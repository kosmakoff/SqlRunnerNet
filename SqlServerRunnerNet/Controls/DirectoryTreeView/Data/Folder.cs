namespace SqlServerRunnerNet.Controls.DirectoryTreeView.Data
{
	public class Folder
	{
		public string FolderPath { get; private set; }

		public Folder(string folderPath)
		{
			FolderPath = folderPath;
		}
	}
}
