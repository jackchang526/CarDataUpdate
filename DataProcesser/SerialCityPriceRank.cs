using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.DataProcesser.Repository;
using System.Data;
using System.IO;
using BitAuto.CarDataUpdate.Common;
using System.Xml;
using BitAuto.Utils;
using System.Xml.Serialization;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class SerialCityPriceRank
	{
		private static Dictionary<int, XmlNode> dictPriceRange;

		static SerialCityPriceRank()
		{
			InitData();
		}

		private static void InitData()
		{
			dictPriceRange = GetSerialPriceRange();
		}

		public static Dictionary<int, XmlNode> GetSerialPriceRange()
		{
			Dictionary<int, XmlNode> dict = new Dictionary<int, XmlNode>();
			string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "AllAutoData.xml");
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(filePath);
			if (xmlDoc != null)
			{
				XmlNodeList nodeList = xmlDoc.SelectNodes("//Serial");
				foreach (XmlNode node in nodeList)
				{
					int serialId = ConvertHelper.GetInteger(node.Attributes["ID"].Value);
					if (!dict.ContainsKey(serialId))
						dict.Add(serialId, node);
				}
			}
			return dict;
		}

		public static void GenerateSerialCityPriceRank()
		{
			//根据级别排行 只生成特定城市 0代表全国
			int[] cityIdArray = { 0, 201, 2401, 501, 502, 301, 1501, 1502, 3001, 3002, 101, 1001, 1601, 1201, 1301, 2501, 3101, 2901, 2301, 401, 2201, 901, 2101, 2102, 2601, 1401, 1701, 1708, 1101, 1801 };
			foreach (var cityId in cityIdArray)
			{
				Log.WriteLog("开始生成报价区间子品牌城市排行，城市：" + cityId);
				Dictionary<int, List<SerialEntity>> dictPriceRangeSerial = new Dictionary<int, List<SerialEntity>>();
				DataSet ds = SerialCityPVRepository.GetSerialCityPVRank(cityId);
				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
 					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						int serialId = ConvertHelper.GetInteger(dr["csid"]);
						if (dictPriceRange.ContainsKey(serialId))
						{
							var priceRange = dictPriceRange[serialId].Attributes["MultiPriceRange"].Value;
							var priceRangeArray = priceRange.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
								.Select(p => ConvertHelper.GetInteger(p));
							var serialName = dictPriceRange[serialId].Attributes["Name"].Value;
							var serialShowName = dictPriceRange[serialId].Attributes["ShowName"].Value;
							var allSpell = dictPriceRange[serialId].Attributes["AllSpell"].Value;
							foreach (var p in priceRangeArray)
							{
								//if (p <= 0) continue;
								if (!dictPriceRangeSerial.ContainsKey(p))
								{
									var list = new List<SerialEntity>();
									list.Add(new SerialEntity
									{
										ID = serialId,
										Name = serialName,
										ShowName = serialShowName,
										AllSpell = allSpell
									});
									dictPriceRangeSerial[p] = list;
								}
								else
								{
									dictPriceRangeSerial[p].Add(new SerialEntity()
									{
										ID = serialId,
										Name = serialName,
										ShowName = serialShowName,
										AllSpell = allSpell
									});
								}
							}
						}
					}
					//生成 城市 排行文件
					RenderContent(dictPriceRangeSerial, cityId);
 				}
			}
		}

		public static void RenderContent(Dictionary<int, List<SerialEntity>> dictPriceRangeSerial, int cityId)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
			sb.Append("<CityPriceSort>");
			foreach (var key in dictPriceRangeSerial)
			{
				sb.Append("<Price Name=\"" + key.Key + "\" >");
				foreach (var cs in key.Value)
				{
					sb.AppendFormat("<Serial ID=\"{0}\" Name=\"{1}\" ShowName=\"{2}\" AllSpell=\"{3}\"/>",
						cs.ID, cs.Name, cs.ShowName, cs.AllSpell);
				}
				sb.Append("</Price>");
			}
			sb.Append("</CityPriceSort>");
			string filePath = Path.Combine(CommonData.CommonSettings.SavePath, string.Format("SerialCityPricePV/{0}.xml", cityId));

			CommonFunction.SaveFileContent(sb.ToString(), filePath, Encoding.UTF8);
		}
		public struct SerialEntity
		{
			public int ID;
			public string Name;
			public string ShowName;
			public string AllSpell;
		}
	}
}
