using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlServerRunnerNet.Controls.DirectoryTreeView.Data;

namespace SqlServerRunnerNet.Controls.DirectoryTreeView.ViewModel
{
	public class DirectoryModelCollection
	{
		public List<DirectoryTreeViewItemViewModel> Roots { get; private set; }

		public DirectoryModelCollection()
		{
			Roots = new List<DirectoryTreeViewItemViewModel>();

			Roots.AddRange(
				DriveInfo.GetDrives()
					.Where(drive => drive.DriveType == DriveType.Fixed)
					.Select(drive => new DriveViewModel(new Folder(drive.RootDirectory.FullName))));

			var firstRoot = Roots.FirstOrDefault();

			if (firstRoot != null)
				firstRoot.IsExpanded = true;
		}

		public void SetSelectedFolder(string selectedPath)
		{
			var folder = EnsureFolder(selectedPath);

			if (folder != null)
				folder.IsSelected = true;
		}

		public void SetCheckedFolders(List<string> paths)
		{
			var firstSelected = false;

			foreach (var path in paths)
			{
				var folder = EnsureFolder(path);
				if (folder == null)
					continue;

				folder.IsChecked = true;

				if (folder.Parent != null)
					folder.Parent.IsExpanded = true;

				if (!firstSelected)
				{
					firstSelected = true;
					folder.IsSelected = true;
				}
			}
		}

		public DirectoryTreeViewItemViewModel EnsureFolder(string path)
		{
			var dirInfo = new DirectoryInfo(path);
			var segments = GetPathSegments(dirInfo);

			var currentRoots = new List<DirectoryTreeViewItemViewModel>(Roots);

			DirectoryTreeViewItemViewModel currentSegment = null;

			foreach (var segment in segments)
			{
				currentSegment = currentRoots.FirstOrDefault(
					vm => vm.DisplayPath.Equals(segment, StringComparison.InvariantCultureIgnoreCase));
				if (currentSegment == null) break;

				currentSegment.EnsureItemsLoaded();

				currentRoots = new List<DirectoryTreeViewItemViewModel>(currentSegment.Children);
			}

			return currentSegment;
		}

		private static string[] GetPathSegments(DirectoryInfo dirInfo)
		{
			var pathStack = new Stack<string>();
			DirectoryInfo currentDirInfo = dirInfo;

			do
			{
				pathStack.Push(currentDirInfo.Name);
				currentDirInfo = currentDirInfo.Parent;
			} while (currentDirInfo != null);

			return pathStack.ToArray();
		}

		public List<DirectoryTreeViewItemViewModel> GetCheckedFolders()
		{
			return Roots.SelectMany(GetCheckedFolders).ToList();
		}

		private static IEnumerable<DirectoryTreeViewItemViewModel> GetCheckedFolders(DirectoryTreeViewItemViewModel node)
		{
			if (node.IsChecked == true)
				yield return node;

			if (node.HasDummyChild)
				yield break;

			foreach (var subnode in node.Children.SelectMany(GetCheckedFolders))
			{
				yield return subnode;
			}
		}
	}
}
