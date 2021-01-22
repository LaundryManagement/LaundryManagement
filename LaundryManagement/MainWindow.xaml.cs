using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace LaundryManagement
{
	internal class Record
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
			StoreNo = Convert.ToInt32(split[0]);
			StoreName = split[1];
			DeviceNo = Convert.ToInt32(split[5]);
			ServiceType = split[6];
			Paid = split[8] == "-" ? 0 : Convert.ToDouble(split[8][1..]);
			CardDiscount = split[9] == "-" ? 0 : Convert.ToDouble(split[9][1..]);
		}
	}

	public class Data
	{
		public string Type { get; set; }
		public string Income { get; set; }
		public string Cash { get; set; }
		public string Card { get; set; }

		public Data(string type, double cash, double card)
		{
			Type = type;
			Income = "￥" + (cash + card).ToString("F2");
			Cash = "￥" + cash.ToString("F2");
			Card = "￥" + card.ToString("F2");
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
		private ObservableCollection<Data> datas;

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

				string s0 = (string)r0.Result;

				string[] data = s0.Split('\n');

				for (int i = 0; i < data.Length; i++)
				{
					string s = data[i];
					SourceData.Add(s);
				}
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
					Thread.Sleep(300);
				}
			}
			JavascriptResponse prev = await browser.EvaluateScriptAsync
					("document.getElementsByClassName(\" ant-pagination-prev\")[0].attributes[\"aria-disabled\"].value");
			while ((string)prev.Result == "false")
			{
				browser.ExecuteScriptAsync
					("document.getElementsByClassName(\" ant-pagination-prev\")[0].click()");
				Thread.Sleep(300);
				prev = await browser.EvaluateScriptAsync
					("document.getElementsByClassName(\" ant-pagination-prev\")[0].attributes[\"aria-disabled\"].value");
			}
			records = new List<Record>();
			foreach (string source in SourceData)
			{
				records.Add(new Record(source));
			}
			// TODO: 汇总数据
			IEnumerable<double> queryCash = from r in records
											where r.ServiceType.EndsWith('洗')
											select r.Paid,
								queryCard = from r in records
											where r.ServiceType.EndsWith('洗')
											select r.CardDiscount;
			datas = new ObservableCollection<Data>();
			datas.Add(new Data("洗衣", queryCash.Sum(), queryCard.Sum()));
			queryCash = from r in records
						where r.ServiceType.EndsWith('烘')
						select r.Paid;
			queryCard = from r in records
						where r.ServiceType.EndsWith('烘')
						select r.CardDiscount;
			datas.Add(new Data("烘衣", queryCash.Sum(), queryCard.Sum()));
			queryCash = from r in records
						select r.Paid;
			queryCard = from r in records
						select r.CardDiscount;
			datas.Add(new Data("合计", queryCash.Sum(), queryCard.Sum()));
			dataGrid.DataContext = datas;
		}

		private void BtnExport_Click(object sender, RoutedEventArgs e)
		{
			if (dataGrid.DataContext != datas)
			{
				MessageBox.Show("暂无数据，请确认是否已获取");
				return;
			}
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				DefaultExt = "csv",
				Filter = "逗号分隔值文件 (*.csv)|*.csv"
			};
			if ((bool)saveFileDialog.ShowDialog())
			{
				dataGrid.SelectAllCells();
				object t = Clipboard.GetDataObject();
				Clipboard.Clear();
				ApplicationCommands.Copy.Execute(null, dataGrid);
				dataGrid.UnselectAllCells();
				string dataText = "";
				for (int i = 0; i < 10; i++)
				{
					try
					{
						dataText = Clipboard.GetText(TextDataFormat.CommaSeparatedValue);
						break;
					}
					catch { }
					if (i == 9)
					{
						MessageBox.Show("导出失败");
						return;
					}
					Thread.Sleep(100);
				}
				Clipboard.SetDataObject(t);
				Clipboard.Flush();
				try
				{
					File.WriteAllText(saveFileDialog.FileName, dataText, Encoding.UTF8);
					MessageBox.Show("导出成功", "提示");
				}
				catch (Exception exception)
				{
					MessageBox.Show("导出文件时发生错误：\n" + exception.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			loginWindow.Close();
		}
	}
}