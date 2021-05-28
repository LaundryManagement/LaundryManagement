using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LaundryManagement
{
	public class Record
	{
		public readonly int StoreNo;
		public readonly string StoreName;
		public readonly int DeviceNo;
		public readonly string ServiceType;
		public readonly double Paid;
		public readonly double CardDiscount;

		public Record(string SourceDataItem)
		{
			string[] split = SourceDataItem.Split('\t');
			StoreNo = Convert.ToInt32(split[0], CultureInfo.CurrentCulture);
			StoreName = split[1];
			DeviceNo = Convert.ToInt32(split[5], CultureInfo.CurrentCulture);
			ServiceType = split[6];
			Paid = split[8] == "-" ? 0 : Convert.ToDouble(split[8][1..], CultureInfo.CurrentCulture);
			CardDiscount = split[9] == "-" ? 0 : Convert.ToDouble(split[9][1..], CultureInfo.CurrentCulture);
		}
	}

	public class DigestData
	{
		public string Type { get; set; }
		public string Income { get; set; }
		public string Cash { get; set; }
		public string Card { get; set; }

		public DigestData(string type, double cash, double card)
		{
			Type = type;
			Income = "￥" + (cash + card).ToString("F2", CultureInfo.CurrentCulture);
			Cash = "￥" + cash.ToString("F2", CultureInfo.CurrentCulture);
			Card = "￥" + card.ToString("F2", CultureInfo.CurrentCulture);
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<Record> records;

		public MainWindow() => InitializeComponent();

		private LoginWindow loginWindow;
		private ObservableCollection<DigestData> digestDatas;

		private void BtnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (loginWindow == null)
			{
				loginWindow = new LoginWindow();
			}
			loginWindow.Show();
		}

		private async void BtnFetch_Click(object sender, RoutedEventArgs e)
		{
			if (loginWindow == null || loginWindow.Browser == null || loginWindow.Browser.Address != "http://data.landeli.com/view/basic")
			{
				MessageBox.Show("您还未登录", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			List<string> SourceData = new();
			using (ChromiumWebBrowser browser = loginWindow.Browser)
			{
				while (true)
				{
					JavascriptResponse r0 = await browser.EvaluateScriptAsync
						("document.getElementsByTagName(\"tbody\")[0].innerText;");

					string s0 = (string)r0.Result;
					string[] data = s0.Split('\n');

					for (int i = 0; i < data.Length; i++)
					{
						string s = data[i];
						SourceData.Add(s);
					}
					JavascriptResponse last = await browser.EvaluateScriptAsync
						("document.getElementsByClassName(\"ant-pagination-next\")[0].attributes[\"aria-disabled\"].value");
					if ((string)last.Result == "true")
					{
						MessageBox.Show($"共读取了{SourceData.Count}条数据");
						break;
					}
					else
					{
						const string GetCurrentPage = "parseInt(document.getElementsByClassName(\"ant-pagination-item-active\")[0].title);";
						int pageNum = (int)(await browser.EvaluateScriptAsync(GetCurrentPage)).Result;
						const string SwitchToNextPage = "document.getElementsByClassName(\"ant-pagination-next\")[0].click();";
						browser.ExecuteScriptAsync(SwitchToNextPage);
						Task<JavascriptResponse> nextPage = browser.EvaluateScriptAsync(GetCurrentPage);
						while ((await nextPage).Result != null && (int)(await nextPage).Result != pageNum + 1)
						{
							Thread.Sleep(300);
							browser.ExecuteScriptAsync(SwitchToNextPage);
							nextPage = browser.EvaluateScriptAsync(GetCurrentPage);
						}
					}
				}
				browser.ExecuteScriptAsync("document.getElementsByClassName(\"ant-pagination-item-1\")[0].click();");
			}
			records = new List<Record>();
			foreach (string source in SourceData)
			{
				records.Add(new Record(source));
			}
			digestDatas = Processer.Process(records);
			dataGrid.DataContext = digestDatas;
		}

		private void BtnExport_Click(object sender, RoutedEventArgs e)
		{
			if (digestDatas == null)
			{
				MessageBox.Show("暂无数据，请确认是否已获取");
				return;
			}
			SaveFileDialog saveFileDialog = new()
			{
				FileName = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture) + ".csv",
				DefaultExt = "csv",
				Filter = "逗号分隔值文件 (*.csv)|*.csv"
			};
			if (saveFileDialog.ShowDialog().GetValueOrDefault(false))
			{
				try
				{
					using StreamWriter csvFile = new(saveFileDialog.FileName, false);
					csvFile.WriteLine("业务种类,总收入,现金支付,刷卡支付");
					foreach (DigestData d in digestDatas)
					{
						csvFile.WriteLine("{0},{1},{2},{3}", d.Type, d.Income, d.Cash, d.Card);
					}
				}
				catch (UnauthorizedAccessException uaException)
				{
					MessageBox.Show("访问被拒绝，请选择其他目录\n" + uaException.Message);
				}
				catch (IOException ioException)
				{
					MessageBox.Show("I/O 错误\n" + ioException.Message);
				}
				catch (Exception exception)
				{
					MessageBox.Show("未知错误\n" + exception.StackTrace);
				}
			}
			else
			{
				MessageBox.Show("未选择文件", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (loginWindow != null)
				loginWindow.Close();
		}
	}
}