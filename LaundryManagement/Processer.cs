using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LaundryManagement
{
	public static class Processer
	{
		public static ObservableCollection<DigestData> Process(List<Record> records)
		{
			List<Record> query = records.Where(r => r.ServiceType.EndsWith('洗')).ToList();
			IEnumerable<double> queryCash = query.Select(r => r.Paid);
			IEnumerable<double> queryCard = query.Select(r => r.CardDiscount);
			double cashWash = queryCash.Sum(),
				cardWash = queryCard.Sum();
			ObservableCollection<DigestData> datas = new ObservableCollection<DigestData>
			{
				new DigestData("洗衣", cashWash, cardWash)
			};

			query = records.Where(r => r.ServiceType.EndsWith('烘')).ToList();
			queryCash = query.Select(r => r.Paid);
			queryCard = query.Select(r => r.CardDiscount);
			double cashDry = queryCash.Sum(), cardDry = queryCard.Sum();
			datas.Add(new DigestData("烘衣", cashDry, cardDry));

			datas.Add(new DigestData("合计", cashWash + cashDry, cardWash + cardDry));
			return datas;
		}
	}
}