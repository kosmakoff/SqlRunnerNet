using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SqlServerRunnerNet.Utils;
using SqlServerRunnerNet.ViewModel;

namespace SqlServerRunnerNet
{
	/// <summary>
	/// Interaction logic for ServerBrowserWindow.xaml
	/// </summary>
	public partial class ServerBrowserWindow : Window
	{
		public ServerBrowserWindowViewModel Model { get; private set; }

		public ServerBrowserWindow()
		{
			InitializeComponent();

			Model = new ServerBrowserWindowViewModel(this);
			DataContext = Model;

			Loaded += async (sender, args) =>
			{
				await Model.LoadSqlServerInstances();
			};
		}

		public ServerBrowserWindow(Window parent)
			:this()
		{
			Owner = parent;
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		public string SqlServerInstanceName
		{
			get
			{
				var instance = ListBox.SelectedItem as SqlServerInstance;
				return instance != null ? instance.ToString() : string.Empty;
			}
		}
	}
}
