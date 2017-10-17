using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;
using BitAuto.Utils.Data;
using System.Threading;
using System.Configuration;
using System.Data.SqlClient;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class CarProduceAndSellData
	{
		private CommonSettings m_config;							//配置
		private ConnStringSettings m_connStrSetting;				//配置
		private XmlDocument m_newsCateDoc;							//新闻分类XML
		private Dictionary<int, NewsCategory> m_newCategorys;		//新闻分类字典
		public event LogHandler Log;
		private string _FilePath;
		private Dictionary<string, List<int>> _LevelList;

		public CarProduceAndSellData()
		{
			m_config = BitAuto.CarDataUpdate.Common.CommonData.CommonSettings;
			m_connStrSetting = BitAuto.CarDataUpdate.Common.CommonData.ConnectionStringSettings;
			m_newsCateDoc = new XmlDocument();
			m_newCategorys = new Dictionary<int, NewsCategory>();

			_FilePath = Path.Combine(m_config.SavePath, "ProduceAndSell");

			_LevelList = new Dictionary<string, List<int>>(){
                {"suv",new List<int>(){424}},
                {"mpv",new List<int>(){425}},
                {"weixingche",new List<int>(){321}},
                {"xiaoxingche",new List<int>(){338}},
                {"jincouxingche",new List<int>(){339}},
                {"zhongxingche",new List<int>(){340}},
                {"zhongdaxingche",new List<int>(){341}},
                {"haohuache",new List<int>(){342}},
                {"car",new List<int>(){321,338,339,340,341,342}}
            };
		}

		#region 从ContentGetter.cs挪过来的部分
		/// <summary>
		/// 获取所产销数据包括的厂商，品牌，子品牌信息
		/// </summary>
		public void UpdateSellBrandTree()
		{
			string backupPath = Path.Combine(_FilePath, "Backup");

			if (!Directory.Exists(backupPath))
				Directory.CreateDirectory(backupPath);

			OnLog("     Begin update SellBrandTree......", true);
			string fileName = Path.Combine(_FilePath, "BrandTree.xml");
			string backFileName = Path.Combine(backupPath, "BrandTree.xml");

			DataSet ds = GetProduceSellBrandTree();

			XmlDocument xmlDoc = new XmlDocument();
			XmlElement root = xmlDoc.CreateElement("root");
			xmlDoc.AppendChild(root);
			XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			xmlDoc.InsertBefore(xmlDeclar, root);

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				int pId = Convert.ToInt32(row["cp_Id"]);
				//厂商
				XmlElement pNode = (XmlElement)root.SelectSingleNode("Producer[@id=\"" + pId + "\"]");
				if (pNode == null)
				{
					pNode = xmlDoc.CreateElement("Producer");
					string pName = Convert.ToString(row["Cp_Name"]);
					pNode.SetAttribute("id", pId.ToString());
					pNode.SetAttribute("name", pName);
					root.AppendChild(pNode);
				}

				//品牌
				int bId = Convert.ToInt32(row["cb_Id"]);
				XmlElement bNode = (XmlElement)pNode.SelectSingleNode("Brand[@id=\"" + bId + "\"]");
				if (bNode == null)
				{
					string bName = Convert.ToString(row["cb_Name"]);
					bNode = xmlDoc.CreateElement("Brand");
					bNode.SetAttribute("id", bId.ToString());
					bNode.SetAttribute("name", bName);
					pNode.AppendChild(bNode);
				}

				int sId = Convert.ToInt32(row["CsId"]);
				string sName = Convert.ToString(row["csName"]);
				XmlElement sNode = xmlDoc.CreateElement("Serial");
				sNode.SetAttribute("id", sId.ToString());
				sNode.SetAttribute("name", sName);
				bNode.AppendChild(sNode);
			}

			SaveXmlDocument(xmlDoc, fileName, backFileName);
		}

		/// <summary>
		/// 获取产销数据的厂商列表
		/// </summary>
		/// <returns></returns>
		private List<int> GetPsBrandList(string type)
		{
			string xmlPath = "";
			type = type.ToLower();
			if (type == "producer")
				xmlPath = "/root/Producer";
			else if (type == "brand")
				xmlPath = "/root/Producer/Brand";
			else if (type == "serial")
				xmlPath = "/root/Producer/Brand/Serial";

			List<int> idList = new List<int>();
			string xmlFile = Path.Combine(_FilePath, "BrandTree.xml");
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(xmlFile);

			XmlNodeList nodeList = xmlDoc.SelectNodes(xmlPath);
			foreach (XmlElement node in nodeList)
			{
				int id = Convert.ToInt32(node.GetAttribute("id"));
				idList.Add(id);
			}
			return idList;
		}

		/// <summary>
		/// 获取厂商、品牌、子品牌的层级关系
		/// </summary>
		/// <returns></returns>
		private DataSet GetProduceSellBrandTree()
		{
			string sqlStr = @"SELECT  a.CsId, b.csName, b.cb_Id, c.cb_Name, c.cp_Id,
        Cp_Name = SUBSTRING(ISNULL(d.Spell, ''), 1, 1) + ' ' + d.CpShortName
FROM    ( SELECT DISTINCT
                    CsId
          FROM      dbo.CarProduceAndSellData
        ) a
        INNER JOIN dbo.Car_Serial b ON a.CsId = b.cs_Id
        INNER JOIN dbo.Car_Brand c ON c.cb_Id = b.cb_Id
        INNER JOIN dbo.Car_producer d ON c.cp_Id = d.Cp_Id";
			return SqlHelper.ExecuteDataset(m_connStrSetting.AutoStroageConnString, CommandType.Text, sqlStr);
		}

		/// <summary>
		/// 获取产销数据的内容
		/// </summary>
		public void GetPsData()
		{
			try
			{
				OnLog("     Get produce and sell all news...", true);
				UpdateSellNews();
				OnLog("     Get produce and sell Producer news...", true);
				UpdateSellProducerNews(0);
				OnLog("     Get produce and sell Brand news...", true);
				UpdateSellBrandNews(0);
				OnLog("     Get produce and sell Serial news...", true);
				UpdateSellSerialNews(0);
			}
			catch (System.Exception ex)
			{
				OnLog(ex.ToString(), true);
			}
		}

		/// <summary>
		/// 获取销售数据的新闻
		/// </summary>
		public void UpdateSellNews()
		{
			string backupPath = Path.Combine(_FilePath, "Backup");

			if (!Directory.Exists(backupPath))
				Directory.CreateDirectory(backupPath);

			OnLog("     Start Update SellNews......", true);
			string newsUrl = m_config.NewsUrl + "?nonewstype=2&getcount=100";
			int[] cateIdList = new int[] { 2, 3, 4, 5, 34, 98, 145, 148, 149, 150, 151, 179 };
			//166:看车，168:用车，169:新闻
			//int[] cateIdList = new int[] { 166, 168, 169 };
			string xmlPath = GetCategoryXmlPath(cateIdList);
			int newsNum = 10;

			//获取所有的厂商的新闻
			//List<XmlElement> newsAllList = QueryNewsByCategoryId(newsUrl, cateIdList, 10);
			XmlDocument allDoc = new XmlDocument();
			allDoc.Load(newsUrl);
			AppendNewsInfoToDocument(allDoc);
			XmlNodeList newsAllList = allDoc.SelectNodes("/NewDataSet/listNews[" + xmlPath + "]");

			XmlDocument xmlAllDoc = new XmlDocument();
			XmlDeclaration xmlAllDeclar = xmlAllDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			xmlAllDoc.AppendChild(xmlAllDeclar);
			XmlElement rootAll = xmlAllDoc.CreateElement("NewDataSet");
			xmlAllDoc.AppendChild(rootAll);
			foreach (XmlElement newsNode in newsAllList)
			{
				rootAll.AppendChild(xmlAllDoc.ImportNode(newsNode, true));
				if (rootAll.ChildNodes.Count == newsNum)
					break;
			}

			string backupAllFileName = Path.Combine(backupPath, "All.xml");
			string xmlAllFileName = Path.Combine(_FilePath, "All.xml");

			SaveXmlDocument(xmlAllDoc, xmlAllFileName, backupAllFileName);

		}

		/// <summary>
		/// 获取产销数据中厂商的新闻
		/// </summary>
		/// <param name="producerId"></param>
		public void UpdateSellProducerNews(int producerId)
		{
			List<int> producerList = null;
			if (producerId == 0)
				producerList = GetPsBrandList("producer");
			else
			{
				producerList = new List<int>();
				producerList.Add(producerId);
			}

			string FilePath = Path.Combine(_FilePath, "Producer");
			string backupPath = Path.Combine(FilePath, "Backup");

			if (!Directory.Exists(backupPath))
				Directory.CreateDirectory(backupPath);

			string newsUrl = m_config.NewsUrl + "?nonewstype=2&getcount=100";
			int[] cateIdList = new int[] { 2, 3, 4, 5, 34, 98, 145, 148, 149, 150, 151, 179 };
			//166:看车，168:用车，169:新闻
			//int[] cateIdList = new int[] { 166, 168, 169 };
			string xmlPath = GetCategoryXmlPath(cateIdList);
			int newsNum = 10;
			//获取厂商新闻
			int pidCounter = 0;
			foreach (int pId in producerList)
			{
				pidCounter++;
				OnLog("     Get producer:" + pId + " news(" + pidCounter + "/" + producerList.Count + ")...", false);
				string pUrl = newsUrl + "&producerid=" + pId + "&include=1";
				//List<XmlElement> newsList = QueryNewsByCategoryId(pUrl, cateIdList, 10);
				XmlDocument prdDoc = new XmlDocument();
				prdDoc.Load(pUrl);
				AppendNewsInfoToDocument(prdDoc);

				XmlNodeList newsList = prdDoc.SelectNodes("/NewDataSet/listNews[" + xmlPath + "]");

				XmlDocument xmlDoc = new XmlDocument();
				XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
				xmlDoc.AppendChild(xmlDeclar);
				XmlElement root = xmlDoc.CreateElement("NewDataSet");
				xmlDoc.AppendChild(root);
				foreach (XmlElement newsNode in newsList)
				{
					root.AppendChild(xmlDoc.ImportNode(newsNode, true));
					if (root.ChildNodes.Count == newsNum)
						break;
				}

				string xmlFileName = "Producer_" + pId + ".xml";
				string backupFileName = Path.Combine(backupPath, xmlFileName);
				xmlFileName = Path.Combine(FilePath, xmlFileName);

				SaveXmlDocument(xmlDoc, xmlFileName, backupFileName);
			}
		}

		/// <summary>
		/// 获取产销数据中品牌的新闻
		/// </summary>
		/// <param name="brandId"></param>
		public void UpdateSellBrandNews(int brandId)
		{
			List<int> brandList = null;
			if (brandId == 0)
				brandList = GetPsBrandList("brand");
			else
			{
				brandList = new List<int>();
				brandList.Add(brandId);
			}

			string FilePath = Path.Combine(_FilePath, "Brand");
			string backupPath = Path.Combine(FilePath, "Backup");

			if (!Directory.Exists(backupPath))
				Directory.CreateDirectory(backupPath);

			string newsUrl = m_config.NewsUrl + "?nonewstype=2&getcount=100";
			int[] cateIdList = new int[] { 2, 3, 4, 5, 34, 98, 145, 148, 149, 150, 151, 179 };
			//166:看车，168:用车，169:新闻
			//int[] cateIdList = new int[] { 166, 168, 169 };
			string xmlPath = GetCategoryXmlPath(cateIdList);
			int newsNum = 10;

			//获取品牌新闻
			int brandCounter = 0;
			foreach (int bId in brandList)
			{
				brandCounter++;
				OnLog("     Get brand:" + bId + " news(" + brandCounter + "/" + brandList.Count + ")...", false);
				string brandUrl = newsUrl + "&bigbrand=" + bId + "&include=1";
				//List<XmlElement> newsList = QueryNewsByCategoryId(brandUrl, cateIdList, 10);
				XmlDocument brandDoc = new XmlDocument();
				brandDoc.Load(brandUrl);
				AppendNewsInfoToDocument(brandDoc);
				XmlNodeList newsList = brandDoc.SelectNodes("/NewDataSet/listNews[" + xmlPath + "]");

				XmlDocument xmlDoc = new XmlDocument();
				XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
				xmlDoc.AppendChild(xmlDeclar);
				XmlElement root = xmlDoc.CreateElement("NewDataSet");
				xmlDoc.AppendChild(root);
				foreach (XmlElement newsNode in newsList)
				{
					root.AppendChild(xmlDoc.ImportNode(newsNode, true));
					if (root.ChildNodes.Count == newsNum)
						break;
				}

				string xmlFileName = "Brand_" + bId + ".xml";
				string backupFileName = Path.Combine(backupPath, xmlFileName);
				xmlFileName = Path.Combine(FilePath, xmlFileName);

				//备份文件
				SaveXmlDocument(xmlDoc, xmlFileName, backupFileName);
			}
		}

		/// <summary>
		/// 获取产销数据中所有子品牌的新闻
		/// </summary>
		/// <param name="sId"></param>
		public void UpdateSellSerialNews(int serialId)
		{
			List<int> serialList = null;
			if (serialId == 0)
				serialList = GetPsBrandList("serial");
			else
			{
				serialList = new List<int>();
				serialList.Add(serialId);
			}

			string FilePath = Path.Combine(_FilePath, "Serial");
			string backupPath = Path.Combine(FilePath, "Backup");

			if (!Directory.Exists(backupPath))
				Directory.CreateDirectory(backupPath);

			string newsUrl = m_config.NewsUrl + "?nonewstype=2&getcount=100";
			int[] cateIdList = new int[] { 2, 3, 4, 5, 34, 98, 145, 148, 149, 150, 151, 179 };
			//166:看车，168:用车，169:新闻
			//int[] cateIdList = new int[] { 166, 168, 169 };
			string xmlPath = GetCategoryXmlPath(cateIdList);
			int newsNum = 10;

			//获取子品牌新闻
			int serialCounter = 0;
			foreach (int sId in serialList)
			{
				serialCounter++;
				OnLog("     Get serial:" + sId + " news(" + serialCounter + "/" + serialList.Count + ")...", false);
				string serialUrl = newsUrl + "&brandid=" + sId;
				//List<XmlElement> newsList = QueryNewsByCategoryId(serialUrl, cateIdList, 10);
				XmlDocument serialDoc = new XmlDocument();
				serialDoc.Load(serialUrl);
				AppendNewsInfoToDocument(serialDoc);
				XmlNodeList newsList = serialDoc.SelectNodes("/NewDataSet/listNews[" + xmlPath + "]");

				XmlDocument xmlDoc = new XmlDocument();
				XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
				xmlDoc.AppendChild(xmlDeclar);
				XmlElement root = xmlDoc.CreateElement("NewDataSet");
				xmlDoc.AppendChild(root);
				foreach (XmlElement newsNode in newsList)
				{
					root.AppendChild(xmlDoc.ImportNode(newsNode, true));
					if (root.ChildNodes.Count == newsNum)
						break;
				}

				string xmlFileName = "Serial_" + sId + ".xml";
				string backupFileName = Path.Combine(backupPath, xmlFileName);
				xmlFileName = Path.Combine(FilePath, xmlFileName);

				SaveXmlDocument(xmlDoc, xmlFileName, backupFileName);
			}

		}
		/// <summary>
		/// 为XmlDocument加上新闻的分类ID详细信息
		/// </summary>
		/// <param name="xmlDoc"></param>
		private void AppendNewsInfoToDocument(XmlDocument xmlDoc)
		{
			XmlNodeList newsList = xmlDoc.SelectNodes("/NewDataSet/listNews");
			foreach (XmlElement newsNode in newsList)
				AppendNewsInfo(newsNode);
		}
		/// <summary>
		/// 给新闻内容加入根分类ID及分类路径信息
		/// </summary>
		/// <param name="newsNode"></param>
		private void AppendNewsInfo(XmlElement newsNode)
		{
			int cateId = 0;
			try
			{
				if (m_newCategorys == null || m_newCategorys.Count == 0)
					GetNewsCategorys();
				cateId = Convert.ToInt32(newsNode.SelectSingleNode("categoryId").InnerText);
				if (m_newCategorys.ContainsKey(cateId))
				{
					//加入根分类ID
					XmlElement rootIdEle = newsNode.OwnerDocument.CreateElement("RootCategoryId");
					newsNode.AppendChild(rootIdEle);
					rootIdEle.InnerText = m_newCategorys[cateId].RootCategoryId.ToString();

					//加入分类路径
					XmlElement pathEle = newsNode.OwnerDocument.CreateElement("CategoryPath");
					newsNode.AppendChild(pathEle);
					pathEle.InnerText = m_newCategorys[cateId].CategoryPath;
				}
			}
			catch (System.Exception ex)
			{
				OnLog("AppendNewsInfo issued:CategoryId=" + cateId, true);
				OnLog(ex.ToString(), true);
			}

		}
		#endregion

		#region UpdateSellDataMap
		/// <summary>
		/// 生成产销量地图数据
		/// </summary>
		public void UpdateSellDataMap()
		{
			OnLog("     Start CarProduceAndSellData.UpdateSellDataMap ......", true);
			string xmlFile = Path.Combine(_FilePath, "SellDataMap.xml");
			if (ExistsDirectory(_FilePath))
			{
				XmlDocument mapDoc = new XmlDocument(), serialDoc = new XmlDocument();
				try
				{
					mapDoc.Load(m_config.SellDataMapUrl);
					string autoFile = Path.Combine(m_config.SavePath, "AllAutoData.xml");
					serialDoc.Load(autoFile);
					mapDoc.DocumentElement.SetAttribute("updateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					XmlNodeList monthList = mapDoc.SelectNodes("/Data/Month");
					//归类所有子品牌的级别
					foreach (XmlElement monthNode in monthList)
					{
						Dictionary<string, XmlElement> levelNodeDic = new Dictionary<string, XmlElement>();
						for (int i = monthNode.ChildNodes.Count - 1; i >= 0; i--)
						{
							XmlElement serialNode = (XmlElement)monthNode.ChildNodes[i];
							monthNode.RemoveChild(serialNode);
							int serialId = Convert.ToInt32(serialNode.GetAttribute("SerialId"));
							//取级别及全拼
							XmlElement templNode = (XmlElement)serialDoc.SelectSingleNode("/Params/MasterBrand/Brand/Serial[@ID=\"" + serialId + "\"]");
							if (templNode == null)
								continue;
							string level = templNode.GetAttribute("CsLevel");
							string spell = templNode.GetAttribute("AllSpell").ToLower();
							string showName = templNode.GetAttribute("ShowName");
							serialNode.SetAttribute("allSpell", spell);
							serialNode.SetAttribute("SerialName", showName);
							serialNode.RemoveAttribute("ClassId");
							if (!levelNodeDic.ContainsKey(level))
							{
								XmlElement levelRoot = mapDoc.CreateElement("Level");
								levelRoot.SetAttribute("level", level);
								monthNode.AppendChild(levelRoot);
								levelNodeDic[level] = levelRoot;
							}
							levelNodeDic[level].AppendChild(serialNode);
						}

						foreach (XmlElement levelNode in levelNodeDic.Values)
						{
							monthNode.AppendChild(levelNode);
						}
					}

					CommonFunction.SaveXMLDocument(mapDoc, xmlFile);
				}
				catch (Exception exp)
				{
					OnLog("Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...", true);
				}
			}
			OnLog("     End CarProduceAndSellData.UpdateSellDataMap ......", true);
		}
		#endregion

		#region UpdateCarData
		/// <summary>
		/// 生成产销量车辆数据，按月生成
		/// </summary>
		/// <param name="dateTimeStr"></param>
		public void UpdateCarData(string dateTimeStr)
		{
			OnLog("     Start CarProduceAndSellData.UpdateCarData ......", true);

			DateTime dateTime;
			if (DateTime.TryParse(dateTimeStr, out dateTime))
			{
				int current = 0, count = _LevelList.Count;

				foreach (KeyValuePair<string, List<int>> level in _LevelList)
				{
					current++;
					OnLog(string.Format("Generating DateTime:{0}, Level:{1}, Number:({2}/{3}) ", dateTime.ToString("yyyy-MM"), level.Key, current.ToString(), count.ToString()), false);

					GetSellDataXml(dateTime, level.Key, level.Value, true);
				}
			}
			else
			{
				OnLog("Error DateTime Format Error (param:" + dateTimeStr + ")......", true);
			}
			OnLog("     End CarProduceAndSellData.UpdateCarData!", true);
		}
		//生成产销量车辆数据
		public void UpdateAllCarData()
		{
			OnLog("     Start CarProduceAndSellData.UpdateAllCarData ......", true);
			List<DateTime> lastMonths = GetLastDataMonths();

			int current = 0, count = lastMonths.Count * _LevelList.Count;

			foreach (DateTime dateTime in lastMonths)
			{
				foreach (KeyValuePair<string, List<int>> level in _LevelList)
				{
					current++;
					OnLog(string.Format("Generating DateTime:{0}, Level:{1}, Number:({2}/{3}) ", dateTime.ToString("yyyy-MM"), level.Key, current.ToString(), count.ToString()), false);

					GetSellDataXml(dateTime, level.Key, level.Value, false);
				}
			}
			OnLog("     End CarProduceAndSellData.UpdateAllCarData!", true);
		}

		/// <summary>
		/// 获取某月的销售数据前10
		/// </summary>
		/// <param name="dataDate">时间</param>
		/// <param name="dataType">数据类型</param>
		/// <param name="needNewData">是否获取最新数据</param>
		/// <returns></returns>
		private void GetSellDataXml(DateTime dataDate, string dataType, List<int> levelList, bool isRefresh)
		{
			dataDate = new DateTime(dataDate.Year, dataDate.Month, 1);

			string cacheKey = "SellStatisticData_" + dataType + "_" + dataDate.ToString("yyyyMMdd");

			string fileName = Path.Combine(_FilePath, "CarData\\" + cacheKey + ".xml");
			if (isRefresh || !File.Exists(fileName))
			{
				RefreshSellDataXml(dataDate, levelList, fileName);
			}
		}
		/// <summary>
		/// 刷新产销数据的Xml缓存
		/// </summary>
		/// <param name="dataDate"></param>
		/// <param name="levelList"></param>
		/// <param name="cacheKey"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private void RefreshSellDataXml(DateTime dataDate, List<int> levelList, string fileName)
		{
			XmlDocument xmlDoc = null;

			#region 获取统计数据
			Dictionary<string, DataTable> sellData = GetSellData(dataDate, levelList);
			string curDate = dataDate.ToString("yyyy-MM-01");
			//string curDate = GetLastMonth().ToString("yyyy-MM-01");
			string preMonthDate = dataDate.AddMonths(-1).ToString("yyyy-MM-01");
			string preYearDate = dataDate.AddYears(-1).ToString("yyyy-MM-01");

			//进行合并与计算
			xmlDoc = new XmlDocument();
			//加根结点
			XmlElement root = xmlDoc.CreateElement("SellDataList");
			root.SetAttribute("curDate", curDate);
			Dictionary<int, XmlElement> nodeDic = new Dictionary<int, XmlElement>();
			xmlDoc.AppendChild(root);

			//时间标记
			XmlElement timeEle = xmlDoc.CreateElement("Time");
			XmlText textNode = xmlDoc.CreateTextNode(DateTime.Now.ToString("yyyy-MM-dd"));
			timeEle.AppendChild(textNode);
			root.AppendChild(timeEle);

			foreach (DataRow row in sellData[curDate].Rows)
			{
				int csId = Convert.ToInt32(row["CsId"]);
				string csName = Convert.ToString(row["csShowName"]);
				string csSpell = Convert.ToString(row["allSpell"]);
				int curSell = Convert.ToInt32(row["SellNum"]);
				XmlElement sellNode = xmlDoc.CreateElement("SellData");
				sellNode.SetAttribute("CsId", csId.ToString());
				sellNode.SetAttribute("CsName", csName);
				sellNode.SetAttribute("AllSpell", csSpell);
				sellNode.SetAttribute("CurrentSellData", curSell.ToString());
				sellNode.SetAttribute("preMonthSellData", "0");
				sellNode.SetAttribute("preYearSellData", "0");
				sellNode.SetAttribute("currentCount", "0");
				sellNode.SetAttribute("preYearCount", "0");
				sellNode.SetAttribute("preMonthIncrease", "--");
				sellNode.SetAttribute("preYearIncrease", "--");
				sellNode.SetAttribute("countIncrease", "--");

				root.AppendChild(sellNode);
				nodeDic[csId] = sellNode;
			}

			//加入前一月的数据
			foreach (DataRow row in sellData[preMonthDate].Rows)
			{
				int csId = Convert.ToInt32(row["CsId"]);
				int preMonthSell = Convert.ToInt32(row["SellNum"]);
				if (nodeDic.ContainsKey(csId))
				{
					nodeDic[csId].SetAttribute("preMonthSellData", preMonthSell.ToString());
				}
			}

			//加入前一年数据
			foreach (DataRow row in sellData[preYearDate].Rows)
			{
				int csId = Convert.ToInt32(row["CsId"]);
				int preYearSell = Convert.ToInt32(row["SellNum"]);
				if (nodeDic.ContainsKey(csId))
				{
					nodeDic[csId].SetAttribute("preYearSellData", preYearSell.ToString());
				}
			}

			//加入累计数据
			foreach (DataRow row in sellData["CurrentCount"].Rows)
			{
				int csId = Convert.ToInt32(row["CsId"]);
				int sellCount = Convert.ToInt32(row["SellCount"]);
				if (nodeDic.ContainsKey(csId))
				{
					nodeDic[csId].SetAttribute("currentCount", sellCount.ToString());
				}
			}

			//加入上一年的累计数据
			foreach (DataRow row in sellData["PreYearCount"].Rows)
			{
				int csId = Convert.ToInt32(row["CsId"]);
				int sellCount = Convert.ToInt32(row["SellCount"]);
				if (nodeDic.ContainsKey(csId))
				{
					nodeDic[csId].SetAttribute("preYearCount", sellCount.ToString());
				}
			}

			//计算增长比例
			foreach (XmlElement sellNode in nodeDic.Values)
			{
				int curSell = GetAttributeValue(sellNode, "CurrentSellData");
				int preMonthSell = GetAttributeValue(sellNode, "preMonthSellData");
				int preYearSell = GetAttributeValue(sellNode, "preYearSellData");
				int curCount = GetAttributeValue(sellNode, "currentCount");
				int preYearCount = GetAttributeValue(sellNode, "preYearCount");
				//环比
				string upStr = "";
				double up = 0;
				if (preMonthSell == 0)
					upStr = "--";
				else
				{
					up = (curSell - preMonthSell) / (double)preMonthSell * 10000;
					up = Math.Round(up, MidpointRounding.AwayFromZero);
					upStr = (up / 100) + "%";
				}
				sellNode.SetAttribute("preMonthIncrease", upStr);

				//同比
				if (preYearSell == 0)
					upStr = "--";
				else
				{
					up = (curSell - preYearSell) / (double)preYearSell * 10000;
					up = Math.Round(up, MidpointRounding.AwayFromZero);
					upStr = (up / 100) + "%";
				}
				sellNode.SetAttribute("preYearIncrease", upStr);

				//累计增长
				if (preYearCount == 0)
				{
					upStr = "--";
				}
				else
				{
					up = (curCount - preYearCount) / (double)preYearCount * 10000;
					up = Math.Round(up, MidpointRounding.AwayFromZero);
					upStr = (up / 100) + "%";
				}
				sellNode.SetAttribute("countIncrease", upStr);
			}

			CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			#endregion
		}
		/// <summary>
		/// 获取有数据的最后一个月
		/// </summary>
		/// <returns></returns>
		private List<DateTime> GetLastDataMonths()
		{
			List<DateTime> lastMonths = new List<DateTime>();
			string sqlStr = "SELECT TOP 12 DataDate FROM CarProduceAndSellData GROUP BY DataDate ORDER BY datadate DESC";
			DataSet monthsDs = SqlHelper.ExecuteDataset(m_connStrSetting.AutoStroageConnString, CommandType.Text, sqlStr);
			if (monthsDs != null && monthsDs.Tables.Count > 0)
			{
				foreach (DataRow row in monthsDs.Tables[0].Rows)
				{
					if (row["DataDate"] != null && row["DataDate"] != DBNull.Value)
						lastMonths.Add(Convert.ToDateTime(row["DataDate"]));
				}
			}
			return lastMonths;
		}
		private Dictionary<string, DataTable> GetSellData(DateTime dataDate, List<int> dataTypeList)
		{
			Dictionary<string, DataTable> dataRes = new Dictionary<string, DataTable>();
			string curDate = dataDate.ToString("yyyy-MM-01");
			string preMonthDate = dataDate.AddMonths(-1).ToString("yyyy-MM-01");
			string preYearDate = dataDate.AddYears(-1).ToString("yyyy-MM-01");
			//先查询当月的数据
			DataSet ds = GetCurMonthSellData(curDate, dataTypeList);
			dataRes.Add(curDate, ds.Tables[0]);

			//取子品牌ID
			string serialIdList = "";
			int idCounter = 0;
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				idCounter++;
				string serialId = Convert.ToString(row["CsId"]);
				serialIdList += serialId;
				if (idCounter < ds.Tables[0].Rows.Count)
					serialIdList += ",";
			}

			//取前一月的数据
			ds = GetOtherMonthSellData(preMonthDate, serialIdList);
			dataRes[preMonthDate] = ds.Tables[0];
			ds = GetOtherMonthSellData(preYearDate, serialIdList);
			dataRes[preYearDate] = ds.Tables[0];

			//当前累计
			ds = GetSellDataCount(dataDate, serialIdList);
			dataRes["CurrentCount"] = ds.Tables[0];
			//同期累计
			ds = GetSellDataCount(dataDate.AddYears(-1), serialIdList);
			dataRes["PreYearCount"] = ds.Tables[0];

			return dataRes;
		}
		/// <summary>
		/// 取其他月份的销售数据
		/// </summary>
		/// <param name="dataDate"></param>
		/// <param name="serialIdList"></param>
		/// <returns></returns>
		private DataSet GetOtherMonthSellData(string dataDate, string serialIdList)
		{
			StringBuilder sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine("SELECT CsId,SellNum FROM dbo.CarProduceAndSellData");
			sqlBuilder.AppendLine("WHERE DataDate='" + dataDate + "'");
			if (serialIdList.Length > 0)
				sqlBuilder.AppendLine("AND CsId IN (" + serialIdList + ")");
			return SqlHelper.ExecuteDataset(m_connStrSetting.AutoStroageConnString, CommandType.Text, sqlBuilder.ToString());
		}
		/// <summary>
		/// 统计数据
		/// </summary>
		/// <param name="curDate"></param>
		/// <param name="serialIdList"></param>
		/// <returns></returns>
		private DataSet GetSellDataCount(DateTime curDate, string serialIdList)
		{
			string startDate = new DateTime(curDate.Year, 1, 1).ToString("yyyy-MM-dd");
			string endDate = curDate.ToString("yyyy-MM-01");
			StringBuilder sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine("SELECT CsId,SUM(SellNum) AS SellCount FROM dbo.CarProduceAndSellData");
			sqlBuilder.AppendLine("WHERE DataDate BETWEEN '" + startDate + "' AND '" + endDate + "'");
			if (serialIdList.Length > 0)
				sqlBuilder.AppendLine("AND CsId IN (" + serialIdList + ")");
			sqlBuilder.AppendLine("GROUP BY CsId");

			return SqlHelper.ExecuteDataset(m_connStrSetting.AutoStroageConnString, CommandType.Text, sqlBuilder.ToString());
		}
		/// <summary>
		/// 查询当月的数据，取前10
		/// </summary>
		/// <param name="dataDate"></param>
		/// <param name="dataType"></param>
		/// <returns></returns>
		private DataSet GetCurMonthSellData(string dataDate, List<int> dataTypeList)
		{
			StringBuilder sqlBuilder = new StringBuilder();

			//查询级别范围
			string levelCondition = "";
			if (dataTypeList.Count == 1)
			{
				levelCondition = " b.carlevel=" + dataTypeList[0];
			}
			else
			{
				levelCondition = " b.carlevel IN (";
				int levelCounter = 0;
				foreach (int level in dataTypeList)
				{
					levelCounter++;
					levelCondition += level.ToString();
					if (levelCounter < dataTypeList.Count)
						levelCondition += ",";
				}
				levelCondition += ")";
			}

			sqlBuilder.AppendLine("SELECT TOP 10 a.CsId,b.csShowName,b.allSpell,a.SellNum FROM dbo.CarProduceAndSellData a");
			sqlBuilder.AppendLine("INNER JOIN dbo.Car_Serial b ON a.CsId = b.cs_Id AND b.IsState=0");
			if (levelCondition.Length > 0)
				sqlBuilder.AppendLine("AND " + levelCondition);
			sqlBuilder.AppendLine("WHERE DataDate='" + dataDate + "' ORDER BY a.SellNum DESC");


			return SqlHelper.ExecuteDataset(m_connStrSetting.AutoStroageConnString, CommandType.Text, sqlBuilder.ToString());
		}
		#endregion

		#region 公用方法
		/// <summary>
		/// 获取结点值
		/// </summary>
		/// <param name="sellNode"></param>
		/// <param name="attrName"></param>
		private int GetAttributeValue(XmlElement sellNode, string attrName)
		{
			string attrValue = sellNode.GetAttribute(attrName);
			if (String.IsNullOrEmpty(attrValue))
				attrValue = "0";
			return Convert.ToInt32(attrValue);
		}
		/// <summary>
		/// 生成根据CategoryID查询新闻的表达式
		/// </summary>
		/// <param name="cateIds"></param>
		/// <returns></returns>
		private string GetCategoryXmlPath(int[] cateIds)
		{
			string xmlPath = "";
			foreach (int cateId in cateIds)
			{
				if (xmlPath.Length == 0)
					xmlPath = "contains(CategoryPath,\"," + cateId + ",\")";
				else
					xmlPath += "or contains(CategoryPath,\"," + cateId + ",\")";
			}
			return xmlPath;
		}
		/// <summary>
		/// 获取新闻分类级别列表
		/// </summary>
		private void GetNewsCategorys()
		{
			string xmlUrl = m_config.NewsUrl + "?showcategory=1";
			try
			{
				m_newsCateDoc.Load(xmlUrl);
				XmlNodeList cateList = m_newsCateDoc.SelectNodes("/NewDataSet/NewsCategory");
				foreach (XmlElement cateNode in cateList)
				{
					//分析分类ID，路径及根ID，并加入分类字典
					int cateId = Convert.ToInt32(cateNode.SelectSingleNode("newscategoryid").InnerText);
					string catePath = cateNode.SelectSingleNode("newscategoryidpath").InnerText;
					NewsCategory newsCate = new NewsCategory(cateId);
					newsCate.CategoryPath = catePath;
					m_newCategorys[cateId] = newsCate;
				}
			}
			catch (System.Exception ex)
			{
				Common.Log.WriteErrorLog("获取新闻分类级别列表 异常:" + xmlUrl + "\r\n" + ex.ToString());
			}

		}
		/// <summary>
		/// 保存XmlDocumnet到文件中
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <param name="xmlFile"></param>
		/// <param name="backupFile"></param>
		private void SaveXmlDocument(XmlDocument xmlDoc, string xmlFile, string backupFile)
		{
			try
			{
				string backPath = Path.GetDirectoryName(backupFile);
				if (!Directory.Exists(backPath))
					Directory.CreateDirectory(backPath);
				if (File.Exists(xmlFile))
					File.Copy(xmlFile, backupFile, true);
			}
			catch { }

			int counter = 0;

			Exception err = null;
			while (counter < 5)
			{
				counter++;
				try
				{
					xmlDoc.Save(xmlFile);
					err = null;
					break;
				}
				catch (System.Exception ex)
				{
					err = ex;
					Thread.Sleep(500);
				}
			}
			if (err != null)
				OnLog(err.ToString(), true);
		}
		/// <summary>
		/// 检测目录是否存在，如果不存在将创建
		/// </summary>
		/// <returns></returns>
		private bool ExistsDirectory(string dirPath)
		{
			if (!Directory.Exists(dirPath))
			{
				try
				{
					Directory.CreateDirectory(dirPath);
				}
				catch (Exception exp)
				{
					OnLog("Create Directory Error (Path:" + dirPath + ";Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...", true);
					return false;
				}
			}
			return true;
		}
		/// <summary>
		/// 检测XML文件是否存在
		/// </summary>
		/// <returns></returns>
		bool ExistsXmlDocument(string xmlPath, out XmlDocument xmlDoc)
		{
			bool result = false;
			xmlDoc = null;
			if (!string.IsNullOrEmpty(xmlPath) && File.Exists(xmlPath))
			{
				XmlReader reader = null;
				try
				{
					reader = XmlReader.Create(xmlPath);
					xmlDoc = new XmlDocument();
					xmlDoc.Load(reader);
					result = true;
				}
				catch (Exception exp)
				{
					OnLog("Error Read XML (Path:" + xmlPath + ";message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")", true);
				}
				finally
				{
					if (reader != null && reader.ReadState != ReadState.Closed)
						reader.Close();
				}
			}
			else
			{
				OnLog("Error XML File Not Found (Path:" + xmlPath + ")", true);
			}
			return result;
		}
		/// <summary>
		/// 写Log
		/// </summary>
		/// <param name="logText"></param>
		public void OnLog(string logText, bool nextLine)
		{
			if (Log != null)
				Log(this, new LogArgs(logText, nextLine));
		}
		#endregion

		#region QueryData /***/
		/*
        public void UpdateQueryData()
        {
            OnLog("Start CarProduceAndSellData.UpdateQueryData ......", true);
            XmlDocument brandTreeDoc;
            if (ExistsXmlDocument(Path.Combine(_FilePath, "BrandTree.xml"), out brandTreeDoc))
            {
                string queryPath = Path.Combine(_FilePath, "QueryData");
                #region All
                DataSet ds = GetQueryDataAll();
                CreateQueryData(ds, Path.Combine(queryPath, "QuerySellData_Brand_all.xml"), 0, 0, 0); 
                #endregion

                int objId,i=0;
                #region Producer
                OnLog("Generating QueryData Producer", true);
                XmlNodeList nodeList = brandTreeDoc.SelectNodes("root/Producer/@id");
                foreach (XmlNode node in nodeList)
                {
                    i++;
                    OnLog(string.Format("Current ({0}/{1})", i.ToString(), nodeList.Count), false);
                    if (int.TryParse(node.InnerText, out objId))
                    {
                        ds = GetQueryDataByProducer(objId);
                        CreateQueryData(ds, Path.Combine(queryPath, string.Format("QuerySellData_Producer_{0}.xml", objId.ToString())), objId, 0, 0);
                    }
                } 
                #endregion

                #region Brand
                i = 0;
                OnLog("Generating QueryData Brand", true);
                nodeList = brandTreeDoc.SelectNodes("root/Producer/Brand/@id");
                foreach (XmlNode node in nodeList)
                {
                    i++;
                    OnLog(string.Format("Current ({0}/{1})", i.ToString(), nodeList.Count), false);
                    if (int.TryParse(node.InnerText, out objId))
                    {
                        ds = GetQueryDataByBrand(objId);
                        CreateQueryData(ds,  Path.Combine(queryPath, string.Format("QuerySellData_Brand_{0}.xml" , objId.ToString())), 0, objId, 0);
                    }
                } 
                #endregion

                #region Serial
                i = 0;
                OnLog("Generating QueryData Serial", true);
                nodeList = brandTreeDoc.SelectNodes("root/Producer/Brand/Serial/@id");
                foreach (XmlNode node in nodeList)
                {
                    i++;
                    OnLog(string.Format("Current ({0}/{1})", i.ToString(), nodeList.Count), false);
                    if (int.TryParse(node.InnerText, out objId))
                    {
                        ds = GetQueryDataBySerial(objId);
                        CreateQueryData(ds,  Path.Combine(queryPath, string.Format("QuerySellData_Serial_{0}.xml" , objId.ToString())), 0, 0, objId);
                    }
                } 
                #endregion
            }
            OnLog("End CarProduceAndSellData.UpdateQueryData ......", true);
        }
        void CreateQueryData(DataSet ds, string filePath, int pId, int bId, int sId)
        {
            if (ds == null || ds.Tables.Count <= 0 || string.IsNullOrEmpty(filePath))
                return;

            //生成Xml
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
            XmlElement sellDataRoot = xmlDoc.CreateElement("SellData");
            root.AppendChild(sellDataRoot);
            XmlElement timeEle = xmlDoc.CreateElement("Time");
            timeEle.InnerText = DateTime.Now.ToString("yyyy-MM-dd");
            root.AppendChild(timeEle);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string monthDate = Convert.ToDateTime(row["DataDate"]).ToString("yyyy-MM");
                string sellCount = Convert.ToString(row["SellCount"]);
                XmlElement rowEle = xmlDoc.CreateElement("r");
                XmlElement dateEle = xmlDoc.CreateElement("d");
                XmlElement sellEle = xmlDoc.CreateElement("s");
                dateEle.InnerText = monthDate;
                sellEle.InnerText = sellCount;
                rowEle.AppendChild(sellEle);
                rowEle.AppendChild(dateEle);
                sellDataRoot.InsertBefore(rowEle, sellDataRoot.FirstChild);
            }

            //读取相关新闻
            string newsXml = GetNewsForProduceAndSellData(pId, bId, sId);
            XmlElement newsEle = xmlDoc.CreateElement("NewsData");
            newsEle.InnerXml = newsXml;
            root.AppendChild(newsEle);

            //处理标题
            XmlNodeList titleNodes = newsEle.SelectNodes("NewDataSet/listNews/facetitle");
            foreach (XmlElement titleNode in titleNodes)
            {
                string newsTitle = titleNode.InnerText;
                newsTitle = StringHelper.RemoveHtmlTag(newsTitle);
                titleNode.InnerText = newsTitle;
            }

            SaveXml(xmlDoc, filePath);
        }
        /// <summary>
        /// 按厂商查询销量
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet GetQueryDataByProducer(int pId)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("SELECT TOP 6 a.DataDate,SUM(a.SellNum) AS SellCount");
            sqlBuilder.AppendLine("FROM dbo.CarProduceAndSellData a");
            sqlBuilder.AppendLine("INNER JOIN dbo.Car_Serial b ON a.CsId=b.cs_Id");
            sqlBuilder.AppendLine("INNER JOIN dbo.Car_Brand c ON c.cb_Id=b.cb_Id");
            sqlBuilder.AppendLine("WHERE c.cp_Id = @pId");
            sqlBuilder.AppendLine("GROUP BY a.DataDate");
            sqlBuilder.AppendLine("ORDER BY a.DataDate DESC");

            return SqlHelper.ExecuteDataset(m_config.AutoStroageConnString, CommandType.Text, sqlBuilder.ToString(), new SqlParameter("@pId", pId));
        }
        /// <summary>
        /// 按品牌查询销量
        /// </summary>
        /// <param name="bId"></param>
        /// <returns></returns>
        private DataSet GetQueryDataByBrand(int bId)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("SELECT TOP 6 a.DataDate,SUM(a.SellNum) AS SellCount");
            sqlBuilder.AppendLine("FROM dbo.CarProduceAndSellData a");
            sqlBuilder.AppendLine("INNER JOIN dbo.Car_Serial b ON a.CsId=b.cs_Id");
            sqlBuilder.AppendLine("WHERE b.cb_Id = @bId");
            sqlBuilder.AppendLine("GROUP BY a.DataDate");
            sqlBuilder.AppendLine("ORDER BY a.DataDate DESC");

            return SqlHelper.ExecuteDataset(m_config.AutoStroageConnString, CommandType.Text, sqlBuilder.ToString(), new SqlParameter("@bId", bId));
        }
        /// <summary>
        /// 按子品牌查询销量
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        private DataSet GetQueryDataBySerial(int sId)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("SELECT TOP 6 DataDate,SUM(SellNum) AS SellCount");
            sqlBuilder.AppendLine("FROM dbo.CarProduceAndSellData");
            sqlBuilder.AppendLine("WHERE CsId = @sId");
            sqlBuilder.AppendLine("GROUP BY DataDate");
            sqlBuilder.AppendLine("ORDER BY DataDate DESC");

            return SqlHelper.ExecuteDataset(m_config.AutoStroageConnString, CommandType.Text, sqlBuilder.ToString(), new SqlParameter("@sId", sId));
        }
        /// <summary>
        /// 获取前六个月所有销量
        /// </summary>
        /// <returns></returns>
        private DataSet GetQueryDataAll()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("SELECT TOP 6 DataDate,SUM(SellNum) AS SellCount");
            sqlBuilder.AppendLine("FROM dbo.CarProduceAndSellData");
            sqlBuilder.AppendLine("GROUP BY DataDate");
            sqlBuilder.AppendLine("ORDER BY DataDate DESC");

            return SqlHelper.ExecuteDataset(m_config.AutoStroageConnString, CommandType.Text, sqlBuilder.ToString());
        }
        /// <summary>
        /// 为产销数据查询新闻
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="bId"></param>
        /// <param name="sId"></param>
        /// <returns></returns>
        public string GetNewsForProduceAndSellData(int pId, int bId, int sId)
        {
            string xmlFile = "";
            if (sId != 0)
                xmlFile = "Serial\\Serial_" + sId;
            else if (bId != 0)
                xmlFile = "Brand\\Brand_" + bId;
            else if (pId != 0)
                xmlFile = "Producer\\Producer_" + pId;
            else
                xmlFile = "All";
            xmlFile += ".xml";
            xmlFile = Path.Combine(_FilePath , xmlFile);
            XmlDocument xmlDoc;
            if (ExistsXmlDocument(xmlFile, out xmlDoc))
            {
                return xmlDoc.DocumentElement.OuterXml;
            }
            else
            { return ""; }
        }
*/
		#endregion
	}
}
