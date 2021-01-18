using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using CefSharp.DevTools;
using System.Threading.Tasks;

namespace LaundryManagement
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private LoginWindow loginWindow;

		private void BtnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (loginWindow != null)
			{
				loginWindow.Close();
			}
			loginWindow = new LoginWindow();
			loginWindow.Show();
		}

		private async void BtnFetch_Click(object sender, RoutedEventArgs e)
		{
			//TODO: Execute a JavaScript...
			if (loginWindow == null || loginWindow.Browser == null || loginWindow.Browser.Address != "http://data.landeli.com/view/basic")
			{
				MessageBox.Show("您还未登录", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			var browser = loginWindow.Browser;
			var r = await browser.EvaluateScriptAsync(@"document.getElementsByTagName(""tbody"")[0].innerText;");
			string s = (string)r.Result;
		}
	}
}