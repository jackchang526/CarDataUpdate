using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Services.Cache;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 左侧树型数据
	/// </summary>
	public class CarTreeXmlDataGetter
	{
		public event LogHandler Log;

		public CarTreeXmlDataGetter()
		{
		}

		const string TREE_DATA_DIRECTORY = @"CarTree\TreeData\";

		const string TREE_STATISTICS_PATH = @"CarTree\TagNum.xml";

		private string[] _tagsToBeStatistical = { "tupian", "baojia", "jingxiaoshang", 
                                                    "koubei", "pingce", };

		/// <summary>
		/// 获取标签初始化信息
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, string> GetTreeDataInitializationInfo()
		{
			Dictionary<string, string> tagInitializeInfo = new Dictionary<string, string>();
			tagInitializeInfo.Add("chexing", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=chexing");
			tagInitializeInfo.Add("tupian", "http://imgsvr.bitauto.com/data/photo/treedata_v2.xml");
			//tagInitializeInfo.Add("shipin", "http://v.bitauto.com/car/videotree.ashx");
			//tagInitializeInfo.Add("shipin", "http://v.bitauto.com/restfulapi/cartype/?apikey=460ad6f3-8216-469f-9b1c-52cffa5d812c");
			//tagInitializeInfo.Add("hangqing", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=hangqing");
			tagInitializeInfo.Add("baojia", "http://price.bitauto.com/includeFile/treeList.xml");
			tagInitializeInfo.Add("jingxiaoshang", "http://dealer.bitauto.com/treedata1.xml");
			//tagInitializeInfo.Add("koubei", "http://image.bitautoimg.com/koubei/Statistics/serial/treedata_v2.xml?v="+DateTime.Now.ToString("yyyyMMddHHmmss"));
			//tagInitializeInfo.Add("daogou", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=daogou");
			//tagInitializeInfo.Add("pingce", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=pingce");
			//tagInitializeInfo.Add("yanghu", "http://baoyang.bitauto.com/treedata.xml");
			//tagInitializeInfo.Add("ershouche", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=ucar");
			// del by chengl Apr.24.2014 接口已不存在
			// tagInitializeInfo.Add("dayi", "http://ask.bitauto.com/inc/debris/index/treedata_v2.xml");
			//tagInitializeInfo.Add("tujie", "http://imgsvr.bitauto.com/autochannel/grouptreexmlservice.aspx?gid=12");
			//tagInitializeInfo.Add("zhishu", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=index");
			//tagInitializeInfo.Add("xiaoliang", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=xiaoliang");
			//tagInitializeInfo.Add("keji", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=keji");
			//tagInitializeInfo.Add("anquan", "http://car.bitauto.com/interfaceforbitauto/GetTreeXmlData.ashx?tagType=anquan");
			tagInitializeInfo.Add("jiangjia", "http://jiangjia.bitauto.com/includefile/treelist.xml");
			//tagInitializeInfo.Add("baojiachemengtong", "http://price.bitauto.com/includefile/treelist_chemengtong.xml");

			return tagInitializeInfo;
		}

		/// <summary>
		/// 按标签执行树型数据中标签的统计
		/// </summary>
		/// <param name="tagName">标签名称</param>
		/// <param name="xmlDocument"></param>
		/// <param name="treeDataStatistics">统计信息</param>
		public void DoTreeDataStatistics(string tagName, XmlDocument xmlDocument,
			Dictionary<string, TreeDataStatistics> treeDataStatistics)
		{
			XmlNodeList serialNodes = xmlDocument.SelectNodes("//serial");
			foreach (XmlElement serialNode in serialNodes)
			{
				string serialId = serialNode.GetAttribute("id");
				if (serialId != null)
				{
					serialId = serialId.Trim();
				}
				if (string.IsNullOrEmpty(serialId))
				{
					continue;
				}

				string serialName = serialNode.GetAttribute("name");
				string countNumStr = serialNode.GetAttribute("countnum");
				int countNum = 0;
				int.TryParse(countNumStr, out countNum);
				if (!treeDataStatistics.ContainsKey(serialId))
				{
					treeDataStatistics.Add(serialId,
						new TreeDataStatistics(serialId, serialName, _tagsToBeStatistical));
				}
				treeDataStatistics[serialId].SetCountAmount(tagName, countNum);
			}
		}

		/// <summary>
		/// 生成统计结果的xml文件
		/// </summary>
		/// <param name="treeDataStatistics">统计信息</param>
		public void GenerateTreeDataStatisticsFile(
			Dictionary<string, TreeDataStatistics> treeDataStatistics)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement listNode = xmlDocument.CreateElement("SerialList");
			xmlDocument.AppendChild(listNode);
			XmlElement currentSerialNode = null;
			foreach (string serialId in treeDataStatistics.Keys)
			{
				currentSerialNode = xmlDocument.CreateElement("Serial");
				listNode.AppendChild(currentSerialNode);

				TreeDataStatistics statistics = treeDataStatistics[serialId];
				currentSerialNode.SetAttribute("id", statistics.BrandId);
				currentSerialNode.SetAttribute("name", statistics.BrandName);
				foreach (string tagName in statistics.TagsToBeStatistical)
				{
					currentSerialNode.SetAttribute(tagName,
						statistics.StatisticsInfo[tagName].ToString());
				}
			}

			XmlDeclaration xmldecl = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
			xmlDocument.InsertBefore(xmldecl, listNode);
			xmlDocument.Save(Path.Combine(Common.CommonData.CommonSettings.SavePath, TREE_STATISTICS_PATH));
		}

		/// <summary>
		/// 更新车型左侧树形xml
		/// </summary>
		public void UpdateTreeData()
		{
			OnLog("Begin Update TreeData...");

			DirectoryInfo treeDataDirectory = GetTreeDataDirectory();
			Dictionary<string, string> tagDicts = GetTreeDataInitializationInfo();
			List<string> tagsToBeStatistical = new List<string>(_tagsToBeStatistical);
			Dictionary<string, TreeDataStatistics> treeDataStatistics =
				new Dictionary<string, TreeDataStatistics>();
 			XmlDocument xmlDocument = null;
			foreach (string tagName in tagDicts.Keys)
			{
				try
				{
					string getUrl = tagDicts[tagName];
					OnLog("getting " + getUrl);
 					xmlDocument = CommonFunction.GetXmlDocument(getUrl);
					if (xmlDocument == null)
					{
						OnLog("xmlDocument is null.");
						continue;
					}

					//如果是需要统计的标签，执行统计
					if (tagsToBeStatistical.Contains(tagName))
					{
						DoTreeDataStatistics(tagName, xmlDocument,
							treeDataStatistics);
					}
 					CommonFunction.SaveXMLDocument(xmlDocument, Path.Combine(treeDataDirectory.FullName, tagName + ".xml"));

					//add by sk 2013.05.08 文件内容同时添加到memcache中
					string memcacheKey = "Car_XML_TreeData_" + tagName;
					DistCacheWrapper.Insert(memcacheKey, xmlDocument.OuterXml, 1000 * 60 * 60 * 24);
					OnLog("successed!");
				}
				catch (Exception ex)
				{
					OnLog(ex.ToString());
					Common.Log.WriteErrorLog(ex);
				}
			}
			OnLog("End Update TreeData...");

			OnLog("Begin Generate Tree Data Statistics File...");
			GenerateTreeDataStatisticsFile(treeDataStatistics);
			OnLog("End Generate Tree Data Statistics File...");
		}

		/// <summary>
		/// 创建左侧数数据保存目录，并返回目录信息
		/// 如果已存在，直接返回目录信息
		/// </summary>
		/// <returns>左侧树数据保存目录</returns>
		private DirectoryInfo GetTreeDataDirectory()
		{
			string m_treeDataPath = Path.Combine(Common.CommonData.CommonSettings.SavePath, TREE_DATA_DIRECTORY);
			return Directory.Exists(m_treeDataPath) ?
				new DirectoryInfo(m_treeDataPath) : Directory.CreateDirectory(m_treeDataPath);
		}

		/// <summary>
		/// 写Log
		/// </summary>
		/// <param name="logText"></param>
		public void OnLog(string logText, bool nextLine = true)
		{
			if (Log != null)
				Log(this, new LogArgs(logText, nextLine));
		}
	}

	public class TreeDataStatistics
	{
		public TreeDataStatistics(string brandId, string brandName, string[] tags)
		{
			this.BrandId = brandId;
			this.BrandName = brandName;
			this.TagsToBeStatistical = tags;
			this.StatisticsInfo = new Dictionary<string, int>();
			foreach (string tagName in TagsToBeStatistical)
			{
				this.StatisticsInfo.Add(tagName, 0);
			}
		}

		public string[] TagsToBeStatistical { get; private set; }

		public string BrandId { get; private set; }

		public string BrandName { get; private set; }

		public Dictionary<string, int> StatisticsInfo { get; private set; }

		public void SetCountAmount(string tagName, int count)
		{
			if (this.StatisticsInfo.ContainsKey(tagName))
			{
				this.StatisticsInfo[tagName] = count;
			}
		}
	}
}
