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

namespace SqlServerRunnerNet
{
	/// <summary>
	/// Interaction logic for ServerBrowserWindow.xaml
	/// </summary>
	public partial class ServerBrowserWindow : Window
	{
		public ServerBrowserWindow()
		{
			InitializeComponent();
		}

		public ServerBrowserWindow(Window parent)
			:this()
		{
			Owner = parent;
		}
	}
}
