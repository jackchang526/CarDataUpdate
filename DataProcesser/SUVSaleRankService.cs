using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using BitAuto.CarDataUpdate.Common;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class SUVSaleRankService
	{
		public void Generate()
		{
			string fileName = Path.Combine(CommonData.CommonSettings.SavePath, @"EP\SUVMonthSaleRankTop10.xml");
			try
			{
				var serialInfo = CommonData.SerialDic;

				StringBuilder sb = new StringBuilder();

				WebClient wc = new WebClient();
				var result = wc.DownloadString(CommonData.CommonSettings.SUVSaleRankUrl);
				if (!string.IsNullOrEmpty(result))
				{
					var entity = JsonConvert.DeserializeObject<SUVSaleRankEntity>(result);
					sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
					sb.AppendFormat("<Root Date=\"{0}\">", Convert.ToDateTime(entity.Date).ToString("yyyy.MM"));
					foreach (var serial in entity.List)
					{
						sb.AppendFormat("<Item Id=\"{0}\" Name=\"{1}\" AllSpell=\"{4}\" Count=\"{2}\" Rank=\"{3}\"/>", serial.ID,
							serialInfo.ContainsKey(serial.ID) ? serialInfo[serial.ID].ShowName : "",
							serial.Count,
							serial.Rank,
							serialInfo.ContainsKey(serial.ID) ? serialInfo[serial.ID].AllSpell : "");
					}
					sb.Append("</Root>");
					CommonFunction.SaveFileContent(sb.ToString(), fileName, Encoding.UTF8);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
	}


	internal class RankList
	{

		[JsonProperty("ID")]
		public int ID { get; set; }

		[JsonProperty("Count")]
		public int Count { get; set; }

		[JsonProperty("Rank")]
		public int Rank { get; set; }
	}

	internal class SUVSaleRankEntity
	{

		[JsonProperty("date")]
		public string Date { get; set; }

		[JsonProperty("list")]
		public RankList[] List { get; set; }
	}



}
