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

namespace LaundryManagement
{
	/// <summary>
	/// Window1.xaml 的交互逻辑
	/// </summary>
	public partial class LoginWindow : Window
	{
		public LoginWindow()
		{
			InitializeComponent();
		}

		private void BtnTest_Click(object sender, RoutedEventArgs e)
		{
			Browser.Address = "http://data.landeli.com/view/basic";
		}

		private void BtnFinish_Click(object sender, RoutedEventArgs e)
		{
			Hide();
		}
	}
}