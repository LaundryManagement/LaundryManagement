using CefSharp;
using System.Windows;

namespace LaundryManagement
{
	/// <summary>
	/// Window1.xaml 的交互逻辑
	/// </summary>
	public partial class LoginWindow : Window
	{
		public LoginWindow() => InitializeComponent();

		private void BtnTest_Click(object sender, RoutedEventArgs e) => Browser.Load("http://data.landeli.com/view/basic");

		private void BtnFinish_Click(object sender, RoutedEventArgs e) => Hide();

		private void LoginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Browser.Delete();
	}
}