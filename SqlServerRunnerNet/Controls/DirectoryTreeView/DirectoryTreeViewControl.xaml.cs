using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using SqlServerRunnerNet.Controls.DirectoryTreeView.ViewModel;

namespace SqlServerRunnerNet.Controls.DirectoryTreeView
{
	/// <summary>
	/// Interaction logic for DirectoryTreeViewControl.xaml
	/// </summary>
	public partial class DirectoryTreeViewControl : UserControl
	{
		private string _selectedPath;
		public DirectoryModelCollection Model { get; set; }

		public string SelectedPath {
			get
			{
				return _selectedPath;
			}
			set
			{
				_selectedPath = value;
				Model.SetSelectedFolder(_selectedPath);
			}
		}

		public DirectoryTreeViewControl()
		{
			InitializeComponent();

			Model = new DirectoryModelCollection();
			FoldersTreeView.DataContext = Model;
		}

		public void SetCheckedFolders(List<string> paths)
		{
			Model.SetCheckedFolders(paths);

			FoldersTreeView.Focus();
		}

		public void SetSelectedPath(string path)
		{
			Model.SetSelectedFolder(path);
		}

		public List<string> GetCheckedFolders()
		{
			return Model.GetCheckedFolders().Select(model => model.FullPath).ToList();
		}
	}
}
