using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
		}

		private static readonly HttpClient client = new HttpClient();

		private async Task Login(string username, string password)
		{
			try
			{
				HttpResponseMessage response = await client.GetAsync("https://baidu.com");
			}
			catch (TimeoutException exception)
			{
				MessageBox.Show("超时" + exception.Message);
			}
			catch (Exception exception)
			{
				MessageBox.Show("用户名或密码错误" + exception.Message);
			}
		}

		private async Task BtnLogin_ClickAsync(object sender, RoutedEventArgs e)
		{
			await Login(tbUsername.Text, tbPassword.Password);
			// if (success)
			Close();
			// else
			// ErrorMsg.Visibility = Visibility.Visible;
		}
	}
}