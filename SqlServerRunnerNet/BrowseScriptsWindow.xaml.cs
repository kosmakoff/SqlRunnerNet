﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SqlServerRunnerNet
{
	/// <summary>
	/// Interaction logic for BrowseScriptsWindow.xaml
	/// </summary>
	public partial class BrowseScriptsWindow : Window
	{
		public List<DirectoryInfo> SelectedDirectories
		{
			get
			{
				return FoldersTreeViewControl
					.GetCheckedFolders()
					.Select(folder => new DirectoryInfo(folder))
					.ToList();
			}
			set { FoldersTreeViewControl.SetCheckedFolders(value.Select(di => di.FullName).ToList()); }
		}

		public BrowseScriptsWindow()
		{
			InitializeComponent();
		}

		public BrowseScriptsWindow(Window parent)
			: this()
		{
			Owner = parent;
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		public void SetTopLevelFolder(string topLevelScriptsFolder)
		{
			FoldersTreeViewControl.SetSelectedPath(topLevelScriptsFolder);
		}
	}
}
