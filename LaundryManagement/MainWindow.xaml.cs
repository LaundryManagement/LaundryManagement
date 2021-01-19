using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace LaundryManagement
{
	internal class Record
	{
		public readonly string OrderNo;
		public readonly int StoreNo;
		public readonly string StoreName;
		public readonly int DeviceNo;
		public readonly string ServiceType;
		public readonly double Paid;
		public readonly double CardDiscount;

		public Record(string SourceDataItem)
		{
			string[] split = SourceDataItem.Split('\t');
			OrderNo = split[0];
			StoreNo = Convert.ToInt32(split[1]);
			StoreName = split[2];
			DeviceNo = Convert.ToInt32(split[6]);
			ServiceType = split[7];
			Paid = split[9] == "-" ? 0 : Convert.ToDouble(split[9][1..]);
			CardDiscount = split[10] == "-" ? 0 : Convert.ToDouble(split[10][1..]);
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<Record> records;

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
			if (loginWindow == null || loginWindow.Browser == null || loginWindow.Browser.Address != "http://data.landeli.com/view/basic")
			{
				MessageBox.Show("您还未登录", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			ChromiumWebBrowser browser = loginWindow.Browser;
			List<string> SourceOrderNo = new List<string>(), SourceData = new List<string>();
			while (true)
			{
				JavascriptResponse r0 = await browser.EvaluateScriptAsync
					("document.getElementsByTagName(\"tbody\")[0].innerText;");
				JavascriptResponse r1 = await browser.EvaluateScriptAsync
					("document.getElementsByTagName(\"tbody\")[1].innerText;");
				string s0 = (string)r0.Result;
				string s1 = (string)r1.Result;
				string[] data = s0.Split('\n');
				string[] orderNo = s1.Split('\n');
				for (int i = 0; i < data.Length; i++)
				{
					string s = orderNo[i] + '\t' + data[i];
					SourceData.Add(s);
				}
				Thread.Sleep(200);
				JavascriptResponse next = await browser.EvaluateScriptAsync
					("document.getElementsByClassName(\" ant-pagination-next\")[0].attributes[\"aria-disabled\"].value");
				if ((string)next.Result == "true")
				{
					MessageBox.Show($"共读取了{SourceData.Count}条数据");
					break;
				}
				else
				{
					browser.ExecuteScriptAsync("document.getElementsByClassName(\" ant-pagination-next\")[0].click()");
					Thread.Sleep(200);
				}
			}
			JavascriptResponse prev = await browser.EvaluateScriptAsync
					("document.getElementsByClassName(\" ant-pagination-prev\")[0].attributes[\"aria-disabled\"].value");
			while ((string)prev.Result == "false")
			{
				browser.ExecuteScriptAsync
					("document.getElementsByClassName(\" ant-pagination-prev\")[0].click()");
				Thread.Sleep(200);
				prev = await browser.EvaluateScriptAsync
					("document.getElementsByClassName(\" ant-pagination-prev\")[0].attributes[\"aria-disabled\"].value");
			}
			// TODO: 解析为类并放入数据库
			records = new List<Record>();
			foreach (string source in SourceData)
			{
				records.Add(new Record(source));
			}
		}
	}
}