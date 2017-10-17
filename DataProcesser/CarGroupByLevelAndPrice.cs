using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class CarGroupByLevelAndPrice
	{
        private CommonSettings m_config;							//配置

        public CarGroupByLevelAndPrice(CommonSettings cfg)
		{
			m_config = cfg;
		}

		public void GenGroupHtml()
		{
			string xmlFile = Path.Combine(m_config.SavePath, "autodata.xml");
			if (!File.Exists(xmlFile))
			{
				Console.WriteLine("未发现文件：" + xmlFile);
				return;
			}
			Console.WriteLine("开始生成CarGroupByLevelAndPrice块...");
            XmlDocument xmlDoc = CommonFunction.GetXmlDocument(xmlFile);
           

			XmlNodeList serialNodeList = xmlDoc.SelectNodes("/Params/MasterBrand/Brand/Serial");

			//初始化数据存储
			Dictionary<string, List<SerialPVInfo>> hotCarDic = new Dictionary<string, List<SerialPVInfo>>();
			Dictionary<int, List<SerialPVInfo>> priceCarDic = new Dictionary<int, List<SerialPVInfo>>();
			hotCarDic["jincouxingche"] = new List<SerialPVInfo>();
			hotCarDic["zhongxingche"] = new List<SerialPVInfo>();
			hotCarDic["xiaoxingche"] = new List<SerialPVInfo>();
			priceCarDic[8] = new List<SerialPVInfo>();
			foreach (XmlElement serialNode in serialNodeList)
			{
				SerialPVInfo pvInfo = new SerialPVInfo();
				pvInfo.SerialId = Convert.ToInt32(serialNode.GetAttribute("ID"));
				pvInfo.SerialName = serialNode.GetAttribute("ShowName");
				if (StringHelper.GetRealLength(pvInfo.SerialName) > 12)
					pvInfo.SerialName = serialNode.GetAttribute("Name");
				pvInfo.PVNum = Convert.ToInt32(serialNode.GetAttribute("CsPV"));
				pvInfo.AllSpell = serialNode.GetAttribute("AllSpell").Trim().ToLower();

				//检查级别
				string level = serialNode.GetAttribute("CsLevel");
				switch (level)
				{
					case "紧凑型":
						hotCarDic["jincouxingche"].Add(pvInfo);
						break;
					case "中型车":
						hotCarDic["zhongxingche"].Add(pvInfo);
						break;
					case "小型车":
						hotCarDic["xiaoxingche"].Add(pvInfo);
						break;
					case "SUV":
						priceCarDic[8].Add(pvInfo);
						break;
				}

				//suv不加入报价区间的块中显示
				if (level == "SUV")
					continue;

				//检查报价
				string[] priceRange = serialNode.GetAttribute("MultiPriceRange").Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
				bool has7 = false;
				foreach (string range in priceRange)
				{
					int priceNum = Convert.ToInt32(range);
					if (priceNum != 0)
					{
						//合并成40万以上
						if (priceNum == 8)
							priceNum = 7;
						if (priceNum == 7)
						{
							if (has7)
								continue;
							else
								has7 = true;
						}
						if (!priceCarDic.ContainsKey(priceNum))
						{
							priceCarDic[priceNum] = new List<SerialPVInfo>();
						}

						priceCarDic[priceNum].Add(pvInfo);
					}
				}
			}

			//生成ＨＴＭＬ
			GenCommonCarGroupHtml(hotCarDic, priceCarDic);
			//生成汽车族合作的HTML
			GenQichezuCarGroupHtml(hotCarDic, priceCarDic);
		}


		/// <summary>
		/// 生成通用版的HTML
		/// </summary>
		private void GenCommonCarGroupHtml(Dictionary<string, List<SerialPVInfo>> hotCarDic, Dictionary<int, List<SerialPVInfo>> priceCarDic)
		{
			List<string> htmlList = new List<string>();
			htmlList.Add("<div class=\"car_model_list\">");
			htmlList.Add("<ul id=\"car_tab_ul\">");
			htmlList.Add("<li class=\"hot1 current\"><a href=\"http://kan.bitauto.com/xinche/\" target=\"_blank\">热点车型</a></li>");
			htmlList.Add("<li><a href=\"http://car.bitauto.com/tree_chexing/search/?p=0-5\" target=\"_blank\">5万以下</a></li>");
			htmlList.Add("<li><a href=\"http://car.bitauto.com/tree_chexing/search/?p=5-8\" target=\"_blank\">5万-8万</a></li>");
			htmlList.Add("<li><a href=\"http://car.bitauto.com/tree_chexing/search/?p=8-12\" target=\"_blank\">8万-12万</a></li>");
			htmlList.Add("<li><a href=\"http://car.bitauto.com/tree_chexing/search/?p=12-18\" target=\"_blank\">12万-18万</a></li>");
			htmlList.Add("<li><a href=\"http://car.bitauto.com/tree_chexing/search/?p=18-25\" target=\"_blank\">18万-25万</a></li>");
			htmlList.Add("<li><a href=\"http://car.bitauto.com/tree_chexing/search/?p=25-40\" target=\"_blank\">25万-40万</a></li>");
			htmlList.Add("<li><a href=\"http://car.bitauto.com/tree_chexing/search/?p=40-80\" target=\"_blank\">40万以上</a></li>");
			htmlList.Add("<li class=\"last\"><a href=\"http://car.bitauto.com/suv/\" target=\"_blank\">SUV越野车</a></li>");
			htmlList.Add("</ul>");
			htmlList.Add("<div id=\"data_table_0\" style=\"display: block;\">");
			//中型车
			htmlList.Add("<dl class=\"fist\" style=\"position: static;\">");
			htmlList.Add("<dt><a href=\"http://car.bitauto.com/zhongxingche/\" target=\"_blank\">中型车</a> &gt;</dt>");
			GenHotCar(hotCarDic["zhongxingche"], htmlList);
			htmlList.Add("</dl>");
			//紧凑型车
			htmlList.Add("<dl class=\"fist\" style=\"position: static;\">");
			htmlList.Add("<dt><a href=\"http://car.bitauto.com/jincouxingche/\" target=\"_blank\">紧凑型车</a> &gt;</dt>");
			GenHotCar(hotCarDic["jincouxingche"], htmlList);
			htmlList.Add("</dl>");
			//小型车
			htmlList.Add("<dl style=\"position: static;\">");
			htmlList.Add("<dt><a href=\"http://car.bitauto.com/xiaoxingche/\" target=\"_blank\">小型车</a> &gt;</dt>");
			GenHotCar(hotCarDic["xiaoxingche"], htmlList);
			htmlList.Add("</dl>");
			htmlList.Add("</div>");
			string[] priceRanges = new string[] { "0-5", "5-8", "8-12", "12-18", "18-25", "25-40", "40-80", "80-9999" };
			for (int priceNum = 1; priceNum <= 8; priceNum++)
			{
				htmlList.Add("<div style=\"display: none\" id=\"data_table_" + priceNum + "\">");
				//排序
				if (priceCarDic.ContainsKey(priceNum))
				{
					priceCarDic[priceNum].Sort(SerialPVInfo.CompareByPV);
					int counter = 0;

					foreach (SerialPVInfo pvInfo in priceCarDic[priceNum])
					{
						if (counter % 8 == 0)
							htmlList.Add("<dl class=\"fist\">");
						counter++;
						htmlList.Add("<dd><a href=\"http://car.bitauto.com/" + pvInfo.AllSpell + "/\" target=\"_blank\">" + pvInfo.SerialName + "</a></dd>");
						if (counter % 8 == 0)
							htmlList.Add("</dl>");
						if (counter >= 23)
							break;
					}
					htmlList.Add("<dd class=\"more\"><a href=\"http://car.bitauto.com/xuanchegongju/?p=" + priceRanges[priceNum - 1] + "\" target=\"_blank\">更多&gt;&gt;</a> </dd>");
				}
				htmlList.Add("</div>");
			}
			htmlList.Add("</div>");

			//保存文件
			string fileName = Path.Combine(m_config.SavePath, "SerialSet\\CarGroupByLevelAndPrice.html");
			File.WriteAllText(fileName, String.Concat(htmlList.ToArray()), Encoding.UTF8);

		}

		/// <summary>
		/// 生成通用版的HTML
		/// </summary>
		private void GenQichezuCarGroupHtml(Dictionary<string, List<SerialPVInfo>> hotCarDic, Dictionary<int, List<SerialPVInfo>> priceCarDic)
		{
			List<string> htmlList = new List<string>();
			htmlList.Add("<li class=\"aThis\"><a href=\"http://qichezu.car.bitauto.com/\" target=\"_blank\">热点车型</a></li>");
			htmlList.Add("<li><a href=\"http://qichezu.car.bitauto.com/price/1/\" target=\"_blank\">5万以下</a></li>");
			htmlList.Add("<li><a href=\"http://qichezu.car.bitauto.com/price/2/\" target=\"_blank\">5万-8万</a></li>");
			htmlList.Add("<li><a href=\"http://qichezu.car.bitauto.com/price/3/\" target=\"_blank\">8万-12万</a></li>");
			htmlList.Add("<li><a href=\"http://qichezu.car.bitauto.com/price/4/\" target=\"_blank\">12万-18万</a></li>");
			htmlList.Add("<li><a href=\"http://qichezu.car.bitauto.com/price/5/\" target=\"_blank\">18万-25万</a></li>");
			htmlList.Add("<li><a href=\"http://qichezu.car.bitauto.com/price/6/\" target=\"_blank\">25万-40万</a></li>");
			htmlList.Add("<li><a href=\"http://qichezu.car.bitauto.com/price/7/\" target=\"_blank\">40万以上</a></li>");
			htmlList.Add("<li class=\"last\"><a href=\"http://qichezu.car.bitauto.com/suv/\" target=\"_blank\">SUV越野车</a></li>");
			htmlList.Add("</ul>");
			htmlList.Add("<div id=\"showHide\">");
			htmlList.Add("<div id=\"data_table_0\" style=\"display: block;\">");
			//中型车
			htmlList.Add("<dl class=\"fist\" style=\"position: static;\">");
			htmlList.Add("<dt><a href=\"http://qichezu.car.bitauto.com/zhongxingche/\" target=\"_blank\">中型车</a> &gt;</dt>");
			GenQichezuHotCar(hotCarDic["zhongxingche"], htmlList);
			htmlList.Add("</dl>");
			//紧凑型车
			htmlList.Add("<dl class=\"fist\" style=\"position: static;\">");
			htmlList.Add("<dt><a href=\"http://qichezu.car.bitauto.com/jincouxingche/\" target=\"_blank\">紧凑型车</a> &gt;</dt>");
			GenQichezuHotCar(hotCarDic["jincouxingche"], htmlList);
			htmlList.Add("</dl>");
			//小型车
			htmlList.Add("<dl class=\"fist\" style=\"position: static;\">");
			htmlList.Add("<dt><a href=\"http://qichezu.car.bitauto.com/xiaoxingche/\" target=\"_blank\">小型车</a> &gt;</dt>");
			GenQichezuHotCar(hotCarDic["xiaoxingche"], htmlList);
			htmlList.Add("</dl>");
			htmlList.Add("</div>");
			string[] priceRanges = new string[] { "0-5", "5-8", "8-12", "12-18", "18-25", "25-40", "40-80", "80-9999" };
			for (int priceNum = 1; priceNum <= 8; priceNum++)
			{
				htmlList.Add("<div style=\"display: none\" id=\"data_table_" + priceNum + "\">");
				//排序
				if (priceCarDic.ContainsKey(priceNum))
				{
					priceCarDic[priceNum].Sort(SerialPVInfo.CompareByPV);
					int counter = 0;

					foreach (SerialPVInfo pvInfo in priceCarDic[priceNum])
					{
						if (counter % 8 == 0)
							htmlList.Add("<dl class=\"fist\">");
						counter++;
						htmlList.Add("<dd><a href=\"http://qichezu.car.bitauto.com/" + pvInfo.AllSpell + "/\" target=\"_blank\">" + pvInfo.SerialName + "</a></dd>");
						if (counter % 8 == 0)
							htmlList.Add("</dl>");
						if (counter >= 23)
							break;
					}
					htmlList.Add("<dd class=\"more\"><a href=\"http://qichezu.car.bitauto.com/xuanchegongju/?p=" + priceRanges[priceNum - 1] + "\" target=\"_blank\">更多&gt;&gt;</a> </dd>");
				}
				htmlList.Add("</div>");
			}
			htmlList.Add("</div>");

			//保存文件
			string fileName = Path.Combine(m_config.SavePath, "SerialSet\\QichezuCarGroupByLevelAndPrice.html");
			File.WriteAllText(fileName, String.Concat(htmlList.ToArray()), Encoding.UTF8);

		}


		private void GenQichezuHotCar(List<SerialPVInfo> pvinfoList, List<string> htmlList)
		{
			//中型车排序
			pvinfoList.Sort(SerialPVInfo.CompareByPV);
			int counter = 0;
			foreach (SerialPVInfo pvInfo in pvinfoList)
			{
				counter++;
				htmlList.Add("<dd><a href=\"http://qichezu.car.bitauto.com/" + pvInfo.AllSpell + "/\" target=\"_blank\">" + pvInfo.SerialName + "</a></dd>");
				if (counter >= 7)
					break;
			}
		}


		private void GenHotCar(List<SerialPVInfo> pvinfoList, List<string> htmlList)
		{
			//中型车排序
			pvinfoList.Sort(SerialPVInfo.CompareByPV);
			int counter = 0;
			foreach (SerialPVInfo pvInfo in pvinfoList)
			{
				counter++;
				htmlList.Add("<dd><a href=\"http://car.bitauto.com/" + pvInfo.AllSpell + "/\" target=\"_blank\">" + pvInfo.SerialName + "</a></dd>");
				if (counter >= 7)
					break;
			}
		}
	}

	internal class SerialPVInfo
	{
		public int SerialId;
		public string SerialName;
		public int PVNum;
		public string AllSpell;

		internal static int CompareByPV(SerialPVInfo p1,SerialPVInfo p2)
		{
			if(p1.PVNum > p2.PVNum)
				return -1;
			else if(p1.PVNum < p2.PVNum)
				return 1;
			else
				return 0;
		}
	}

}
