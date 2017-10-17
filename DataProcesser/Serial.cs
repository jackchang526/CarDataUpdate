using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Linq;

using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.price;
using BitAuto.CarDataUpdate.HtmlBuilder;
using BitAuto.CarDataUpdate.DataProcesser.Services;


namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class Serial
	{
		public event LogHandler Log;
		private string _RootPath = string.Empty;
		private string _MaintancePath = string.Empty;
		private string _MaintancePricePath = string.Empty;
		private string _MaintanceNewsPath = string.Empty;
		private string _NewsNumberPath = string.Empty;
		private string _News = string.Empty;
		private string _FocusNews = string.Empty;
		private string _Daogou = string.Empty;
		private string _ShiJia = string.Empty;
		private string _YongChe = string.Empty;
		private string _HangQing = string.Empty;
		private string _PingCe = string.Empty;
		private string _GaiZhuang = string.Empty;
		private string _TreePingCe = string.Empty;
		private string _Security = string.Empty;
		private string _Technology = string.Empty;
		private string _NoNewsContent = string.Empty;
		private string _XiaoliangXml = string.Empty;
		private string _BaiduAlding = string.Empty;
		private string _CityPriceFile = string.Empty;
		private string _PingCeAndShiJia = string.Empty;
		private string _PingCeImageSpan = string.Empty;
		private string _PingCeVideoSpan = string.Empty;
		private string _VideoPath = string.Empty;
		private string _BaiduAldingUrl = "http://car.bitauto.com/interface/forbaidu/forbaiduToaliding.aspx";
		private string _CityPriceUrl = "http://price.bitauto.com/Interface/Common/Handler.ashx?user={0}&interfaceid=1&op={1}&csid={2}";
		// private string _SerialPingCeRightShiPin = "http://api.admin.bitauto.com/api/list/newstocar.aspx?articaltype=3&getcount=7&brandid={0}";
		private string _SerialPingCeRightShiPin = "http://v.bitauto.com/RestfulApi/Video/GetVideoByCsId/{0}/3/0?apiKey=3ace31a2-1482-4062-91fc-a3d7df4059aa";
		private string _SerialPingCeRightImageSpan = "http://imgsvr.bitauto.com/autochannel/getserialfocus.aspx?dataname=cms&sid={0}";
		private string _FocusNewsHTML = string.Empty;
		private string _MustWatchNewsHTML = string.Empty;
		private Dictionary<string, string> _NewsSavePathDic = new Dictionary<string, string>();
		private Dictionary<int, NewsCategory> m_newCategorys;		//新闻分类字典
		private Dictionary<int, List<int>> m_serialYearList;//子品牌年款列表
		private AskAndKouBei aakb = new AskAndKouBei();
		private MasterBrand master = new MasterBrand();
		private CityProcesser _CityEntity;

		private string _pinceBlockHTML;
		private string _pinceNewsXml;
		private string[] _editorList;

		private List<int> _videoList;

		/// <summary>
		/// 子品牌构造函数 
		/// </summary>
		public Serial()
		{
			_RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews");
			_MaintancePath = Path.Combine(_RootPath, "Maintance\\Message");
			_MaintancePricePath = Path.Combine(_RootPath, "Maintance\\Price");
			_MaintanceNewsPath = Path.Combine(_RootPath, "Maintance\\News");
			_NewsNumberPath = Path.Combine(_RootPath, "newsNum.xml");
			_News = Path.Combine(_RootPath, "xinwen");
			_FocusNews = Path.Combine(_RootPath, "FocusNews\\Xml");
			_FocusNewsHTML = Path.Combine(_RootPath, "FocusNews\\Html");
			_pinceNewsXml = Path.Combine(_RootPath, "pingce\\Xml");
			_pinceBlockHTML = Path.Combine(_RootPath, "pingce\\Html");
			_PingCeImageSpan = Path.Combine(_RootPath, "pingce\\Image");
			_PingCeVideoSpan = Path.Combine(_RootPath, "pingce\\ShiPinNew");
			_Daogou = Path.Combine(_RootPath, "daogou");
			_ShiJia = Path.Combine(_RootPath, "shijia");
			_YongChe = Path.Combine(_RootPath, "yongche");
			_HangQing = Path.Combine(_RootPath, "hangqing");
			_PingCe = Path.Combine(_RootPath, "pingce");
			_TreePingCe = Path.Combine(_RootPath, "treepingce");
			_Security = Path.Combine(_RootPath, "anquan");
			_Technology = Path.Combine(_RootPath, "keji");
			_GaiZhuang = Path.Combine(_RootPath, "gaizhuang");
			_VideoPath = Path.Combine(_RootPath, "SerialVideo");
			_PingCeAndShiJia = Path.Combine(_RootPath, "pingjia\\Xml");
			_MustWatchNewsHTML = Path.Combine(_RootPath, "pingjia\\Html");
			_NoNewsContent = Path.Combine(CommonData.CommonSettings.SavePath, "nonewscontent");
			_BaiduAlding = Path.Combine(CommonData.CommonSettings.SavePath, "forbaidualding.xml");
			_XiaoliangXml = Path.Combine(CommonData.CommonSettings.SavePath, "tree_xiaoliang.xml");
			_CityPriceFile = Path.Combine(CommonData.CommonSettings.SavePath, "SerialCityPrice");

			_editorList = CommonFunction.GetEditerByType();//new string[] { "毕远浩", "郑思卿", "何晨怡", "贺重钢", "王天一", "张天旭", "朱祖光", "孙文凯", "芦家庆", "俞士强", "易柏燚", "朱玉翔", "张效铭", "仝楠楠", "霍弘伟", "石宇", "侯智洋", "柳丰", "阎宇南", "马霆", "初晓敏" };
			//_editorList = new string[] { "毕远浩", "郑思卿", "何晨怡", "贺重钢", "王天一", "张天旭", "朱祖光", "孙文凯", "芦家庆", "俞士强", "易柏燚", "朱玉翔", "张效铭", "仝楠楠", "霍弘伟", "石宇", "侯智洋", "柳丰", "阎宇南", "王勇", "孙立刚","刘经纬" };

			m_serialYearList = CommonFunction.GetSerialRelationYear();
			_CityEntity = new CityProcesser(0);
			//得到保存地址的列表
			GetSavePathDictionary();

			_videoList = new List<int>() { 237, 74, 70, 348 };//应该是74,70,348，但是因为视频分类有多关联，所以加入237
		}
		/// <summary>
		/// 得到保存地址的列表
		/// </summary>
		private void GetSavePathDictionary()
		{
			_NewsSavePathDic.Add("focus", Path.Combine(_FocusNews, "Serial_FocusNews_{0}.xml"));
			_NewsSavePathDic.Add("xinwen", Path.Combine(_News, "Serial_All_News_{0}.xml"));
			_NewsSavePathDic.Add("daogou", Path.Combine(_Daogou, "Serial_All_News_{0}.xml"));
			_NewsSavePathDic.Add("shijia", Path.Combine(_ShiJia, "Serial_All_News_{0}.xml"));
			_NewsSavePathDic.Add("pingce", Path.Combine(_pinceNewsXml, "Serial_All_News_{0}.xml"));
			_NewsSavePathDic.Add("hangqing", Path.Combine(_HangQing, "Serial_All_News_{0}.xml"));
			_NewsSavePathDic.Add("pingjia", Path.Combine(_PingCeAndShiJia, "Serial_All_News_{0}.xml"));
			_NewsSavePathDic.Add("cityJs", Path.Combine(CommonData.CommonSettings.SavePath, "SerialHangQingCity.html"));
		}
        
		/// <summary>
		/// 得到保养信息
		/// </summary>
		/// <param name="id"></param>
		public void GetMaintanceMessage(int id)
		{
			List<int> serialIdList = new List<int>();
			if (id > 0)
			{
				serialIdList.Add(id);
			}
			else
			{
				serialIdList = CommonFunction.GetSerialList();
			}

			if (serialIdList == null)
			{
				OnLog("     Get seriallist is null)...", false);
				return;
			}
			int counter = 0;
			//得到子品牌信息块
			foreach (int serialId in serialIdList)
			{
				counter++;
				OnLog("     Get serial brand:" + serialId + " maintancemessage (" + counter + "/" + serialIdList.Count + ")...", false);

				string filepath = Path.Combine(_MaintancePath, serialId + ".html");
				if (!Directory.Exists(_MaintancePath)) Directory.CreateDirectory(_MaintancePath);
				MaintainService ms = new MaintainService();
				string maintanceMessage = ms.GetMaintainCycByCsId(serialId);

				try
				{
					if (string.IsNullOrEmpty(maintanceMessage))
					{
						if (File.Exists(filepath)) File.Delete(filepath);
						continue;
					}
					CommonFunction.SaveFileContent(maintanceMessage, filepath, "utf-8");
					//using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					//{
					//    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
					//    {
					//        sw.Write(maintanceMessage);
					//    }
					//}
				}
				catch
				{
					OnLog("Write serial brand:" + serialId + " fault;", false);
				}
			}
		}
        /*  // del by lsf 2016-01-06
		/// <summary>
		/// 得到保养报价信息
		/// </summary>
		/// <param name="id"></param>
		public void GetMaintancePriceMessage(int id)
		{
			List<int> serialIdList = new List<int>();
			if (id > 0)
			{
				serialIdList.Add(id);
			}
			else
			{
				serialIdList = CommonFunction.GetSerialList();
			}

			if (serialIdList == null)
			{
				OnLog("     Get seriallist is null)...", false);
				return;
			}
			int counter = 0;
			//得到子品牌信息块
			foreach (int serialId in serialIdList)
			{
				counter++;
				OnLog("     Get serial brand:" + serialId + " maintancepricemessage (" + counter + "/" + serialIdList.Count + ")...", false);

				string filepath = Path.Combine(_MaintancePricePath, serialId + ".html");
				if (!Directory.Exists(_MaintancePricePath)) Directory.CreateDirectory(_MaintancePricePath);
				MaintainService ms = new MaintainService();
				string maintanceMessage = ms.GetMaintainPriceScopeByCsId(serialId);

				try
				{
					if (string.IsNullOrEmpty(maintanceMessage))
					{
						if (File.Exists(filepath)) File.Delete(filepath);
						continue;
					}
					CommonFunction.SaveFileContent(maintanceMessage, filepath, "utf-8");
					//using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					//{
					//    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
					//    {
					//        sw.Write(maintanceMessage);
					//    }
					//}
				}
				catch
				{
					OnLog("Write serial brand:" + serialId + " fault;", false);
				}
			}
		}*/
		/// <summary>
		/// 得到保养新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetMaintanceNews(int id)
		{
			SaveNewsContent(id, CarNewsTypes.baoyang);
		}
		/// <summary>
		/// 得到子品牌焦点新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetFocusNews(int id)
		{
			List<int> serialIdList = new List<int>();
			if (id == 0)
				serialIdList = CommonFunction.GetSerialList();
			else
			{
				serialIdList.Add(id);
			}
			string cateIdString = CommonData.CarNewsTypeSettings.CarNewsTypeList[(int)CarNewsTypes.serialfocus].CategoryIds;
			string xmlUrlFormat = CommonData.CommonSettings.NewsUrl + "?brandid={0}&NatureType=0,1,5&getcount=100&ismain=1&ishq=1&categoryId=" + cateIdString;
			foreach (int serialId in serialIdList)
			{
				OnLog("Get serial " + serialId + " focus news...", true);
				string xmlFile = "Serial_FocusNews_" + serialId + ".xml";
				xmlFile = Path.Combine(_FocusNews, xmlFile);

				XmlDocument xmlDoc = null;
				try
				{
					//初始化新闻链接
					xmlDoc = CommonFunction.GetXmlDocument(string.Format(xmlUrlFormat, serialId.ToString()));
					if (xmlDoc == null) continue;

					XmlNodeList newsList = xmlDoc.SelectNodes("/NewDataSet/listNews");
					foreach (XmlElement newsNode in newsList)
					{
						if (!IsVideo(newsNode))
						{
							if (newsNode.SelectSingleNode("relatedcityid[text()='0' or contains(concat(',',text(),','),',0,')]") == null) continue;
							//if (!IsNoEditer(newsNode)) continue;	//总部编辑
							//if (!IsOriginal(newsNode)) continue;	//原创文章
						}
						CommonFunction.SendQueueMessage(Convert.ToInt32(newsNode.SelectSingleNode("newsid").InnerText), DateTime.Now, "CMS", "News");
					}
				}
				catch (System.Exception ex)
				{
					OnLog("Get serial " + serialId + " focus news...", true);
					OnLog(ex.ToString(), true);
				}
			}
			OnLog("完成", true);
		}
		/// <summary>
		/// 得到子品牌新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetNews(int id)
		{
			SaveNewsContent(id, CarNewsTypes.xinwen);
		}
		/// <summary>
		/// 得到导购新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetDaoGouNews(int id)
		{
			SaveNewsContent(id, CarNewsTypes.daogou);
		}
		/// <summary>
		/// 得到试驾的新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetShiJiaNews(int id)
		{
			SaveNewsContent(id, CarNewsTypes.shijia);
		}
		/// <summary>
		/// 得到用车新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetYongCheNews(int id)
		{
			SaveNewsContent(id, CarNewsTypes.yongche);
		}
		/// <summary>
		/// 得到行情新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetHangQingNews(int id)
		{
			SaveNewsContent(id, CarNewsTypes.hangqing);
		}
		/// <summary>
		/// 得到评测新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetPingCeNews(int id)
		{
			SaveNewsContent(id, CarNewsTypes.pingce);
		}
		/// <summary>
		/// 得到树形评测新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetTreePingCe(int id)
		{
			SaveNewsContent(id, CarNewsTypes.treepingce);
		}
		/// <summary>
		/// 得到汽车安全新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetSecurity(int id)
		{
			SaveNewsContent(id, CarNewsTypes.anquan);
		}
		/// <summary>
		/// 得到汽车科技新闻
		/// </summary>
		/// <param name="id"></param>
		public void GetTechnology(int id)
		{
			SaveNewsContent(id, CarNewsTypes.keji);
		}
		/// <summary>
		/// 得到改装新闻文章
		/// </summary>
		/// <param name="id"></param>
		public void GetGaiZhuang(int id)
		{
			SaveNewsContent(id, CarNewsTypes.gaizhuang);
		}
		/// <summary>
		/// 跑百度阿拉丁的接口
		/// </summary>
		public void ForBaiduAlding()
		{
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(_BaiduAldingUrl);
				CommonFunction.SaveXMLDocument(xmlDoc, _BaiduAlding);
			}
			catch (Exception ex)
			{
				OnLog(ex.Message, true);
			}
		}
		/// <summary>
		/// 获取有评测文章，但没有在彩虹条中设置的子品牌
		/// </summary>
		public void GetPingceNewsNotInRainbow()
		{
			//取彩虹彩条中设置的评测文章数量
			string sqlStr = "SELECT CSID FROM RainbowEdit where RainbowItemID=43";
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sqlStr);
			Dictionary<int, int> hasDic = new Dictionary<int, int>();
			if (ds != null && ds.Tables.Count > 0)
			{
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					int csId = ConvertHelper.GetInteger(row[0]);
					hasDic[csId] = 1;
				}
			}

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(_NewsNumberPath);
			XmlNodeList nList = xmlDoc.SelectNodes("SerilaList/Serial");
			if (nList == null || nList.Count < 1) return;
			string ids = "0";
			foreach (XmlElement xElem in nList)
			{
				string typeNum = xElem.GetAttribute("pingce");
				int id = ConvertHelper.GetInteger(xElem.GetAttribute("id"));
				if (ConvertHelper.GetInteger(typeNum) > 0 && !hasDic.ContainsKey(id))
				{
					ids += "," + id;
				}
			}
			sqlStr = "SELECT cs_Id,cs_ShowName FROM Car_Serial cs  where cs_Id in (" + ids + ")";
			ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sqlStr);
			string fileText = "";
			if (ds != null && ds.Tables.Count > 0)
			{
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					int csId = ConvertHelper.GetInteger(row[0]);
					string csName = row[1].ToString().Trim();
					fileText += csId + "," + csName + "\r\n";
				}
			}
			if (!Directory.Exists(_NoNewsContent))
			{
				Directory.CreateDirectory(_NoNewsContent);
			}

			File.AppendAllText(Path.Combine(_NoNewsContent, "NotInRainbow.txt"), fileText);

		}
		/// <summary>
		/// 得到没有指定类型的子品牌新闻
		/// </summary>
		/// <param name="typeName"></param>
		public void GetNoNewsSerialByType(string typeName)
		{
			if (string.IsNullOrEmpty(typeName)) return;
			XmlDocument xmlDoc = new XmlDocument();
			OnLog("Get NoNewsSerialByType......", true);
			try
			{
				List<int> noidList = new List<int>();
				Dictionary<int, string> nonewsidList = new Dictionary<int, string>();
				if (typeName != "maintancenews")
				{
					xmlDoc.Load(_NewsNumberPath);
					if (xmlDoc == null) return;
					XmlNodeList nList = xmlDoc.SelectNodes("SerilaList/Serial");
					if (nList == null || nList.Count < 1) return;
					foreach (XmlElement xElem in nList)
					{
						string typeNum = xElem.GetAttribute(typeName);
						int id = ConvertHelper.GetInteger(xElem.GetAttribute("id"));
						if (!string.IsNullOrEmpty(typeName) && ConvertHelper.GetInteger(typeNum) > 0) continue;
						noidList.Add(id);
					}
				}
				else
				{
					noidList = GetNoNewsSerialInMaintanceByType("maintancenews");
				}

				if (noidList == null || noidList.Count < 1) return;

				//以下完成表里面元素名称的添加
				using (SqlConnection sqlConn = new SqlConnection(CommonData.ConnectionStringSettings.AutoStroageConnString))
				{
					try
					{
						sqlConn.Open();
						//给ID赋名称值
						foreach (int entity in noidList)
						{
							SqlCommand sqlComm = new SqlCommand();
							sqlComm.CommandText = string.Format("select csShowName as ename from Car_Serial where cs_id ={0} and CsSaleState='在销'", entity);
							sqlComm.Connection = sqlConn;
							SqlDataAdapter sda = new SqlDataAdapter();
							sda.SelectCommand = sqlComm;
							DataSet ElementDs = new DataSet();
							sda.Fill(ElementDs);

							if (ElementDs == null
								|| ElementDs.Tables.Count < 1
								|| ElementDs.Tables[0].Rows.Count < 1) continue;

							if (!nonewsidList.ContainsKey(entity))
							{
								nonewsidList.Add(entity, ElementDs.Tables[0].Rows[0]["ename"].ToString());
							}

						}
					}
					catch (Exception ex)
					{
						OnLog(ex.Message, true);
					}
					finally
					{
						sqlConn.Close();
					}
				}

				if (nonewsidList == null || nonewsidList.Count < 1) return;
				StringBuilder serialList = new StringBuilder();
				foreach (KeyValuePair<int, string> entity in nonewsidList)
				{
					serialList.AppendFormat("<serial id=\"{0}\" name=\"{1}\"></serial>", entity.Key, entity.Value);
				}

				XmlDocument resultDoc = new XmlDocument();
				resultDoc.LoadXml("<root>" + serialList.ToString() + "</root>");

				CommonFunction.SaveXMLDocument(resultDoc, Path.Combine(_NoNewsContent, "Serial_" + typeName + ".xml"));
			}
			catch
			{ }
		}
		/// <summary>
		/// 得到销量的XML文件
		/// </summary>
		public void GetXiaoLiaoTreeXml()
		{
			OnLog("     Start Tree XiaoLiang XmlDocument:...", false);
			//得到销量有关的所有品牌数据
			DataSet treeDs = GetXiaoLiangTreeXml();
			if (treeDs == null) return;
			//得到加入PV列以后的数据表
			DataTable tempDt = BuildMasterBrandPv(treeDs.Tables[0]);
			if (tempDt == null || tempDt.Rows.Count < 1) return;
			//创建要保存的文档
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode xNode = xmlDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
			xmlDoc.AppendChild(xNode);
			XmlElement xElement = xmlDoc.CreateElement("Params");
			DataRow[] drList = tempDt.Select("", "singleSpell asc ,pvcount desc");
			//循环创建xml文档
			foreach (DataRow masterDr in drList)
			{
				if (masterDr.IsNull("bs_id")) continue;
				//创建主品牌结点
				XmlElement masterElement = xmlDoc.CreateElement("MasterBrand");
				int masterId = ConvertHelper.GetInteger(masterDr["bs_id"].ToString());
				masterElement.SetAttribute("ID", masterDr["bs_id"].ToString());
				masterElement.SetAttribute("Name", masterDr["bs_Name"].ToString());
				masterElement.SetAttribute("Spell", masterDr["Spell"].ToString());
				masterElement.SetAttribute("AllSpell", masterDr["bsallspell"].ToString().ToLower());
				//得到主品牌包含的品牌
				DataView brandDv = treeDs.Tables[0].DefaultView;
				brandDv.RowFilter = "bs_id=" + masterId.ToString();
				DataTable brandDt = brandDv.ToTable(true, "cb_id", "cb_Name", "cballspell", "country");
				brandDv.RowFilter = "";
				if (brandDt == null || brandDt.Rows.Count < 1) continue;
				DataRow[] brandList = brandDt.Select("", "country desc,cb_Name asc");
				//循环创建品牌结点
				foreach (DataRow brandBr in brandList)
				{
					if (brandBr.IsNull("cb_id")) continue;
					int brandId = ConvertHelper.GetInteger(brandBr["cb_id"].ToString());
					int country = ConvertHelper.GetInteger(brandBr["country"].ToString());
					string brandName = country == 0 ? "进口" + brandBr["cb_Name"].ToString() : brandBr["cb_Name"].ToString();
					//创建品牌结点
					XmlElement brandElement = xmlDoc.CreateElement("Brand");
					brandElement.SetAttribute("ID", brandId.ToString());
					brandElement.SetAttribute("Name", brandName);
					brandElement.SetAttribute("Country", country.ToString());
					brandElement.SetAttribute("AllSpell", brandBr["cballspell"].ToString().ToLower());
					//创建子品牌结点
					DataView serialDv = treeDs.Tables[0].DefaultView;
					brandDv.RowFilter = "cb_id=" + brandId.ToString();
					DataTable serialDt = brandDv.ToTable(true, "cs_id", "cs_Name", "csallspell", "CsSaleState");
					brandDv.RowFilter = "";

					if (serialDt == null || serialDt.Rows.Count < 1) continue;

					DataRow[] serialList = serialDt.Select("", "CsSaleState asc,cs_Name asc");
					StringBuilder serialString = new StringBuilder();
					foreach (DataRow serialdr in serialList)
					{
						if (serialdr.IsNull("cs_id")) continue;
						int csId = ConvertHelper.GetInteger(serialdr["cs_id"].ToString());
						string csName = serialdr["cs_Name"].ToString();
						int csSaleState = ConvertHelper.GetInteger(serialdr["CsSaleState"].ToString());
						string csallspell = serialdr["csallspell"].ToString();

						//创建子品牌结点
						XmlElement serialElement = xmlDoc.CreateElement("Serial");
						serialElement.SetAttribute("ID", csId.ToString());
						serialElement.SetAttribute("Name", csName);
						serialElement.SetAttribute("SaleState", csSaleState.ToString());
						serialElement.SetAttribute("AllSpell", csallspell.ToLower());

						brandElement.AppendChild(serialElement);
					}
					//添加品牌结点到主品牌结点里
					masterElement.AppendChild(brandElement);
				}

				//添加主品牌结点
				xElement.AppendChild(masterElement);
			}
			//添加XML根结点
			xmlDoc.AppendChild(xElement);
			//保存销量的xml文件
			CommonFunction.SaveXMLDocument(xmlDoc, _XiaoliangXml);
			OnLog("     End Tree XiaoLiang XmlDocument:...", false);
		}
		/// <summary>
		/// 获取有维修保养信息的子品牌
		/// </summary>
		public void GetHasMaintanceSerial()
		{
			string[] mFiles = Directory.GetFiles(_MaintancePath, "*.html", SearchOption.TopDirectoryOnly);
			List<int> idList = new List<int>();
			foreach (string mFile in mFiles)
			{
				string csId = Path.GetFileNameWithoutExtension(mFile);
				idList.Add(ConvertHelper.GetInteger(csId));
			}

			//取子品牌名称
			string reXml = "<root>";
			using (SqlConnection sqlConn = new SqlConnection(CommonData.ConnectionStringSettings.AutoStroageConnString))
			{
				SqlCommand cmd = new SqlCommand();
				cmd.CommandText = "select csShowName as ename from Car_Serial where cs_id =@id";
				cmd.Parameters.Add("@id", SqlDbType.Int);
				cmd.Connection = sqlConn;
				try
				{
					sqlConn.Open();
					//给ID赋名称值
					foreach (int id in idList)
					{
						cmd.Parameters["@id"].Value = id;
						string csName = cmd.ExecuteScalar().ToString();
						if (csName.Length == 0)
							continue;

						reXml += "<serial id=\"" + id + "\" name=\"" + csName + "\"/>";
					}
					reXml += "</root>";
				}
				catch (Exception ex)
				{
					reXml += ex.ToString();
				}
				finally
				{
					sqlConn.Close();
				}
			}

			File.AppendAllText(Path.Combine(_NoNewsContent, "HasMaintanceSerial.xml"), reXml);
		}
		/// <summary>
		/// 取子品牌中所有车型的城市商家报价
		/// </summary>
		public void GetCarCityPriceBySerial(int sId)
		{
			OnLog("     Get CarCityPriceBySerial...", true);
			string dataPath = _CityPriceFile;
			string backupPath = Path.Combine(dataPath, "Backup");
			if (!Directory.Exists(backupPath))
				Directory.CreateDirectory(backupPath);

			List<int> serialList = null;
			if (sId == 0)
				serialList = CommonFunction.GetSerialList();
			else
			{
				serialList = new List<int>();
				serialList.Add(sId);
			}

			int counter = 0;
			foreach (int serialId in serialList)
			{
				counter++;
				OnLog("     Get serial:" + serialId + "(" + counter + "/" + serialList.Count + ")...", false);
				//每个子品牌存储一个文件
				string xmlFile = "cityPrice_" + serialId + ".xml";
				string backupFile = Path.Combine(backupPath, xmlFile);
				xmlFile = Path.Combine(dataPath, xmlFile);
				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(string.Format(_CityPriceUrl, "car", "GetCarPriceScopeByCsIdForCity", serialId));

					//保存文件
					//SaveXmlDocument(xmlDoc, xmlFile, backupFile);
					CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
				}
				catch
				{
					continue;
				}
			}
		}
		/// <summary>
		/// 得到视频评测XML文件
		/// </summary>
		/// <param name="sId"></param>
		public void GetPingCeShiPinXml(int sId)
		{
			List<int> serialIdList = new List<int>();
			if (sId == 0)
				serialIdList = CommonFunction.GetSerialList();
			else
			{
				serialIdList.Add(sId);
			}

			int counter = 0;
			int sCounter = serialIdList.Count;
			foreach (int entity in serialIdList)
			{
				counter++;
				OnLog("     Get serial PingCePage ShiPing Span:" + entity + " (" + counter + "/" + sCounter + ")...", false);
				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					string fileContent = string.Format(_SerialPingCeRightShiPin, entity);
					string filePath = Path.Combine(_PingCeVideoSpan, string.Format("{0}.xml", entity));

					CommonFunction.SaveXMLDocument(CommonFunction.GetXmlDocument(fileContent), filePath);
				}
				catch (Exception ex)
				{
					OnLog(ex.Message, false);
				}
			}
		}
		/// <summary>
		/// 得到图片评测XML文件
		/// </summary>
		/// <param name="sId"></param>
		public void GetPingCeImageXml(int sId)
		{
			List<int> serialIdList = new List<int>();
			if (sId == 0)
				serialIdList = CommonFunction.GetSerialList();
			else
			{
				serialIdList.Add(sId);
			}

			int counter = 0;
			int sCounter = serialIdList.Count;
			foreach (int entity in serialIdList)
			{
				counter++;
				OnLog("     Get serial PingCePage Image Span:" + entity + " (" + counter + "/" + sCounter + ")...", false);
				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					string fileContent = string.Format(_SerialPingCeRightImageSpan, entity);
					string filePath = Path.Combine(_PingCeImageSpan, string.Format("{0}.xml", entity));

					CommonFunction.SaveXMLDocument(CommonFunction.GetXmlDocument(fileContent), filePath);
				}
				catch (Exception ex)
				{
					OnLog(ex.Message, false);
				}
			}
		}
		/// <summary>
		/// 得到没有维修保养新闻的子品牌新闻
		/// </summary>
		/// <param name="typeName"></param>
		private List<int> GetNoNewsSerialInMaintanceByType(string typeName)
		{
			List<int> serialList = CommonFunction.GetSerialList();
			if (serialList == null || serialList.Count < 0) return null;
			//维修保养新闻链接地址

			//获取子品牌新闻
			int serialCounter = 0;
			List<int> noNewsId = new List<int>();
			foreach (int sId in serialList)
			{
				serialCounter++;
				OnLog("Get serial:" + sId + " news(" + serialCounter + "/" + serialList.Count + ")...", false);

				string xmlFileName = sId + ".xml";
				xmlFileName = Path.Combine(_MaintanceNewsPath, xmlFileName);

				if (!File.Exists(xmlFileName))
				{
					noNewsId.Add(sId);
					continue;
				}
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(xmlFileName);
				if (xmlDoc != null && xmlDoc.SelectSingleNode("NewDataSet/listNews") != null) continue;
				noNewsId.Add(sId);
			}
			return noNewsId;
		}
		/// <summary>
		/// 保存新闻内容
		/// </summary>
		/// <param name="id"></param>
		private void SaveNewsContent(int id, CarNewsTypes carNewsType)
		{
			List<int> serialIdList = new List<int>();
			if (id == 0)
				serialIdList = CommonFunction.GetSerialList();
			else
			{
				serialIdList.Add(id);
			}

			int sCounter = 0;
			foreach (int serialId in serialIdList)
			{
				sCounter++;
				//"Get serial:{0} {1}({2}/{3})..."
				//OnLog("Get serial:" + serialId + " " + carNewsType.ToString() + "(" + sCounter + "/" + serialIdList.Count + ")...", false);
				OnLog(string.Format("Get serial:{0} {1}({2}/{3})...", serialId, carNewsType.ToString(), sCounter, serialIdList.Count), false);

				GetSerialKindAllNews(carNewsType, serialId);

				if (m_serialYearList == null || !m_serialYearList.ContainsKey(serialId)) continue;

				foreach (int year in m_serialYearList[serialId])
				{
					GetSerialYearKindAllNews(carNewsType, serialId, year);
				}
			}
		}
		/// <summary>
		/// 获取市场或导购类新闻
		/// </summary>
		private void GetSerialKindAllNews(CarNewsTypes carNewsType, int serialId)
		{
			int carNewsTypeId = (int)carNewsType;
			if (!CommonData.CarNewsTypeSettings.CarNewsTypeList.ContainsKey(carNewsTypeId))
			{
				OnLog("车型新闻类型不存在!", true);
				return;
			}
			string cateIdString = CommonData.CarNewsTypeSettings.CarNewsTypeList[carNewsTypeId].CategoryIds;
			string xmlUrl = CommonData.CommonSettings.NewsUrl + "?brandid=" + serialId + "&NatureType=0,1,5&getcount=1000&ismain=1&categoryId=" + cateIdString;
			XmlReader reader = null;
			try
			{
				reader = XmlReader.Create(xmlUrl);
				while (reader.ReadToFollowing("newsid"))
				{
					CommonFunction.SendQueueMessage(Convert.ToInt32(reader.ReadString()), DateTime.Now, "CMS", "News");
				}
			}
			catch (Exception ex)
			{
				OnLog(ex.ToString(), true);
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}
		/// <summary>
		/// 获取市场或导购新闻年款新闻数量
		/// </summary>
		/// <param name="kind"></param>
		/// <param name="serialId"></param>
		/// <returns></returns>
		private void GetSerialYearKindAllNews(CarNewsTypes carNewsType, int serialId, int year)
		{
			int carNewsTypeId = (int)carNewsType;
			if (!CommonData.CarNewsTypeSettings.CarNewsTypeList.ContainsKey(carNewsTypeId))
			{
				OnLog("车型新闻类型不存在!", true);
			}
			string cateIdString = CommonData.CarNewsTypeSettings.CarNewsTypeList[carNewsTypeId].CategoryIds;
			string xmlUrl = string.Format("{0}?brandid={1}&NatureType=0,1,5&getcount=1000&ismain=1&caryear={2}&categoryId={3}"
											, CommonData.CommonSettings.NewsUrl, serialId.ToString(), year.ToString(), cateIdString);

			XmlReader reader = null;
			try
			{
				reader = XmlReader.Create(xmlUrl);
				while (reader.ReadToFollowing("newsid"))
				{
					CommonFunction.SendQueueMessage(Convert.ToInt32(reader.ReadString()), DateTime.Now, "CMS", "News");
				}
			}
			catch (Exception ex)
			{
				OnLog(ex.ToString(), true);
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}
        /*  // del by lsf 2016-01-06
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
					m_newCategorys = CommonFunction.GetNewsCategorysFromNews();
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
        
		/// <summary>
		/// 得到子品牌城市新闻数量的XML
		/// </summary>
		/// <param name="newsXmlDoc"></param>
		/// <returns></returns>
		private void SaveSerialCityNumberXml(XmlNodeList xNodeList, string xmlCityNumberFile)
		{
			if (xNodeList == null || xNodeList.Count < 1) return;
			Dictionary<int, int> initCityList = CommonFunction.GetCityIdDic();
			int[] cityIdArray = new int[initCityList.Count];
			initCityList.Keys.CopyTo(cityIdArray, 0);
			XmlDocument cityXmlDocument = new XmlDocument();
			XmlElement root = cityXmlDocument.CreateElement("root");
			cityXmlDocument.AppendChild(root);
			XmlDeclaration xmlDeclar = cityXmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			cityXmlDocument.InsertBefore(xmlDeclar, root);

			try
			{
				//初始化城市数量列表
				foreach (XmlElement entity in xNodeList)
				{
					XmlNode xNode = entity.SelectSingleNode("relatedcityid");
					if (xNode == null || string.IsNullOrEmpty(xNode.InnerText)) continue;
					string[] cityIdString = xNode.InnerText.Split(',');
					if (cityIdString == null || cityIdString.Length < 1) continue;
					List<int> cityIdList = new List<int>();

					foreach (string idString in cityIdString)
					{
						if (cityIdList.Contains(ConvertHelper.GetInteger(idString))) continue;
						cityIdList.Add(ConvertHelper.GetInteger(idString));
					}
					if (cityIdList == null || cityIdList.Count < 1) continue;

					foreach (int cityId in cityIdList)
					{
						if (cityId == 0)//如果城市为“全国”,则所有城市新闻数量加1
						{
							foreach (int cId in cityIdArray)
							{
								int count = initCityList[cId];
								initCityList[cId] = count + 1;
							}

						}
						if (!initCityList.ContainsKey(cityId)) continue;
						initCityList[cityId] += 1;
					}
				}
				//初始化城市结点元素
				foreach (KeyValuePair<int, int> entity in initCityList)
				{
					XmlElement xelem = cityXmlDocument.CreateElement("City");
					xelem.SetAttribute("id", entity.Key.ToString());
					xelem.SetAttribute("newscount", entity.Value.ToString());
					root.AppendChild(xelem);
				}
				//保存城市数量
				CommonFunction.SaveXMLDocument(cityXmlDocument, xmlCityNumberFile);
			}
			catch (Exception ex)
			{
				OnLog(ex.ToString(), true);
			}

		}
		/// <summary>
		/// 得到子品牌城市新闻数量的XML
		/// </summary>
		/// <param name="xNodeList"></param>
		/// <param name="xmlCityNumberFile"></param>
		private void NewsSaveSerialCityNumberXml(XmlNodeList xNodeList, string xmlCityNumberFile, string type)
		{
			try
			{
				XmlDocument xmlDoc = _CityEntity.GetProvinceContainsCityIdXmlStruct();
				if (xmlDoc == null || xmlDoc.ChildNodes.Count < 1) return;
				XmlElement nodeElement = (XmlElement)xmlDoc.SelectSingleNode("root/Province[@ID=0]");
				if (nodeElement != null) nodeElement.SetAttribute(type, xNodeList.Count.ToString());

				//省新闻的列表
				Dictionary<int, List<int>> provinceNumberList = new Dictionary<int, List<int>>();
				Dictionary<int, List<int>> cityNumberList = new Dictionary<int, List<int>>();

				foreach (XmlElement entity in xNodeList)
				{
					//得到新闻关联的城市
					XmlNode xNode = entity.SelectSingleNode("relatedcityid");
					if (xNode == null || string.IsNullOrEmpty(xNode.InnerText)) continue;
					//得到新闻的ID
					int newsId = ConvertHelper.GetInteger(entity.SelectSingleNode("newsid").InnerText);
					//得到城市列表
					string[] cityIdString = xNode.InnerText.Split(',');
					if (cityIdString == null || cityIdString.Length < 1) continue;
					List<int> cityIdList = new List<int>();

					foreach (string idString in cityIdString)
					{
						if (cityIdList.Contains(ConvertHelper.GetInteger(idString))) continue;
						cityIdList.Add(ConvertHelper.GetInteger(idString));
					}
					if (cityIdList == null || cityIdList.Count < 1) continue;
					//循环城市列表
					foreach (int cityId in cityIdList)
					{
						if (cityId < 0) continue;
						//如果当前城市ID不等于全国
						if (cityId != 0)
						{
							//添加城市列表
							XmlElement entityElement = (XmlElement)xmlDoc.SelectSingleNode("root/Province/City[@ID=" + cityId + "]");
							if (entityElement == null) continue;
							if (!cityNumberList.ContainsKey(cityId))
							{
								List<int> newsList = new List<int>();
								newsList.Add(newsId);
								cityNumberList.Add(cityId, newsList);
							}
							else if (!cityNumberList[cityId].Contains(newsId))
							{
								List<int> newsList = cityNumberList[cityId];
								newsList.Add(newsId);
								cityNumberList[cityId] = newsList;
							}
							//添加省份列表
							int parentId = ConvertHelper.GetInteger(((XmlElement)entityElement.ParentNode).GetAttribute("ID"));
							if (!provinceNumberList.ContainsKey(parentId))
							{
								List<int> newsList = new List<int>();
								newsList.Add(newsId);
								provinceNumberList.Add(parentId, newsList);
							}
							else if (!provinceNumberList[parentId].Contains(newsId))
							{
								List<int> newsList = provinceNumberList[parentId];
								newsList.Add(newsId);
								provinceNumberList[parentId] = newsList;
							}
							continue;
						}
						//如果当前指定城市ID为全国
						if (xmlDoc.ChildNodes.Count < 1) continue;
						foreach (XmlNode rootNode in xmlDoc.ChildNodes)
						{
							if (rootNode.ChildNodes.Count < 1) continue;
							foreach (XmlElement provElem in rootNode.ChildNodes)
							{
								//将与省相当的新闻，添加列表中
								int provinceId = ConvertHelper.GetInteger(provElem.GetAttribute("ID"));
								if (provinceId == 0) continue;
								if (!provinceNumberList.ContainsKey(provinceId))
								{
									List<int> newsList = new List<int>();
									newsList.Add(newsId);
									provinceNumberList.Add(provinceId, newsList);
								}
								else if (!provinceNumberList[provinceId].Contains(newsId))
								{
									List<int> newsList = provinceNumberList[provinceId];
									newsList.Add(newsId);
									provinceNumberList[provinceId] = newsList;
								}
								//城市新闻
								foreach (XmlElement cityElem in provElem.ChildNodes)
								{
									int cityElementId = ConvertHelper.GetInteger(cityElem.GetAttribute("ID"));
									if (!cityNumberList.ContainsKey(cityElementId))
									{
										List<int> newsList = new List<int>();
										newsList.Add(newsId);
										cityNumberList.Add(cityElementId, newsList);
									}
									else if (!cityNumberList[cityElementId].Contains(newsId))
									{
										List<int> newsList = cityNumberList[cityElementId];
										newsList.Add(newsId);
										cityNumberList[cityElementId] = newsList;
									}
								}
							}
						}
					}//城市数量统计 END
				}//新闻结点循环 END

				//计算省份的新闻数量
				foreach (XmlNode rootNode in xmlDoc.ChildNodes)
				{
					if (rootNode.ChildNodes.Count < 1) continue;
					foreach (XmlElement provElem in rootNode.ChildNodes)
					{
						//将与省相当的新闻，添加列表中
						int provinceId = ConvertHelper.GetInteger(provElem.GetAttribute("ID"));
						if (!provinceNumberList.ContainsKey(provinceId)) continue;
						//设置省份数量
						provElem.SetAttribute(type, provinceNumberList[provinceId].Count.ToString());
						if (provElem.ChildNodes.Count < 1) continue;

						foreach (XmlElement cityElem in provElem.ChildNodes)
						{
							int cityId = ConvertHelper.GetInteger(cityElem.GetAttribute("ID"));
							if (!cityNumberList.ContainsKey(cityId)) continue;
							cityElem.SetAttribute(type, cityNumberList[cityId].Count.ToString());
						}
					}
				}

				//保存数量文件
				CommonFunction.SaveXMLDocument(xmlDoc, xmlCityNumberFile);
			}
			catch (Exception ex)
			{
				OnLog(ex.ToString(), true);
			}
		}
         * */
        /// <summary>
		/// 是否视频文章
		/// </summary>
		private bool IsVideo(XmlElement newsNode)
		{
			string cateId = CommonFunction.GetXmlElementInnerText(newsNode, "categoryId", string.Empty);
			if (string.IsNullOrEmpty(cateId))
				return false;
			return _videoList.Contains(Convert.ToInt32(cateId));
		}
        /*  // del by lsf 2016-01-06
		/// <summary>
		/// 如果不是总部编辑
		/// </summary>
		/// <param name="xNode">新闻结点</param>
		/// <returns>如果是总部编辑返回:true;不是则返回:false</returns>
		private bool IsNoEditer(XmlNode xNode)
		{
			XmlNode editeNode = xNode.SelectSingleNode("editorName");
			if (editeNode == null || string.IsNullOrEmpty(editeNode.InnerText))
				return false;

			string editerMessage = editeNode.InnerText;
			Regex reg = new Regex(@"(?<=UserName\:).*");
			Match m = reg.Match(editerMessage);
			string editerName = string.Empty;
			if (m.Groups.Count > 0)
			{
				editerName = m.Groups[0].Value.Trim();
			}

			if (string.IsNullOrEmpty(editerName) || string.IsNullOrEmpty(editerName.Trim()))
				return false;

			bool isEditorInbj = false;
			foreach (string entity in _editorList)
			{
				if (entity == editerName)
				{
					isEditorInbj = true;
					break;
				}
			}
			return isEditorInbj;
		}
        
		/// <summary>
		/// 判断一篇文章是否属于原创
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		private bool IsOriginal(XmlNode xmlNode)
		{
			bool isOriginal = false;
			XmlNode rNode = xmlNode.SelectSingleNode("CreativeType");
			if (rNode != null && rNode.InnerText == "0")
				isOriginal = true;
			return isOriginal;

		}*/
        /// <summary>
		/// 得到销量树形XML结构
		/// </summary>
		/// <returns></returns>
		private DataSet GetXiaoLiangTreeXml()
		{
			string sqlString = @"SELECT  cs.cs_id, cs.csName AS cs_Name, CASE cs.CsSaleState
                                          WHEN '在销' THEN 0
                                          WHEN '待销' THEN 1
                                          WHEN '停销' THEN 2
                                        END CsSaleState,
                                        cs.allspell AS csallspell, cs.csShowName AS csShowName, cb.cb_id,
                                        cb.cb_Name, cb.allspell AS cballspell, cmb.bs_id, cmb.bs_Name,
                                        cmb.Spell, cmb.urlspell AS bsallspell,
                                        LEFT(cmb.spell, 1) AS singleSpell, CASE cb.cb_country
                                                                             WHEN 90 THEN 1
                                                                             ELSE 0
                                                                           END AS country
                                FROM    CarProduceAndSellData AS cpsd
                                        LEFT JOIN Car_Serial AS cs ON cpsd.csid = cs.cs_id
                                        LEFT JOIN Car_Brand AS cb ON cs.cb_id = cb.cb_id
                                        LEFT JOIN Car_MasterBrand_Rel AS cmr ON cmr.cb_id = cb.cb_id
                                                                                AND cmr.IsState = 0
                                        LEFT JOIN Car_MasterBrand AS cmb ON cmb.bs_id = cmr.bs_id
                                ORDER BY LEFT(cmb.spell, 1)";
			try
			{
				using (DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlString))
				{
					if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
						return null;

					return ds;
				}
			}
			catch
			{
				return null;
			}
		}
		/// <summary>
		/// 生成包含PV的数据表
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		private DataTable BuildMasterBrandPv(DataTable dt)
		{
			DataSet ds = master.GetMasterBrandPvCount();
			if (ds == null) return null;
			if (dt == null || dt.Rows.Count < 1) return null;
			if (!dt.Columns.Contains("pvcount"))
				dt.Columns.Add("pvcount", typeof(System.Int32));
			DataTable tempDt = dt.Clone();
			foreach (DataRow dr in dt.DefaultView.ToTable(true, "bs_id", "bs_Name", "Spell", "bsallspell", "singleSpell", "pvcount").Rows)
			{
				if (dr.IsNull("bs_id")) continue;
				int bsId = ConvertHelper.GetInteger(dr["bs_id"].ToString());
				string bsName = dr["bs_Name"].ToString();
				string bsSpell = dr["Spell"].ToString();
				string bsAllSpell = dr["bsallspell"].ToString();
				string singSpell = dr["singleSpell"].ToString();
				int pvcount = 0;
				DataRow[] uvDr = ds.Tables[0].Select("bs_Id=" + bsId, "");
				if (uvDr != null && uvDr.Length > 0)
				{
					pvcount = ConvertHelper.GetInteger(uvDr[0]["UVCount"]);
				}
				DataRow newdr = tempDt.NewRow();
				newdr["bs_id"] = bsId;
				newdr["bs_Name"] = bsName;
				newdr["Spell"] = bsSpell;
				newdr["bsallspell"] = bsAllSpell;
				newdr["pvcount"] = pvcount;
				newdr["singleSpell"] = singSpell;
				tempDt.Rows.Add(newdr);
			}
			return tempDt;
		}
        /*  // del by lsf 2016-01-06
		/// <summary>
		/// 买车必看
		/// add by chengl Jun.14.2012
		/// </summary>
		/// <param name="sId"></param>
		public void GetWatchMustsHTML(int sId)
		{
			List<int> serialIdList = new List<int>();
			if (sId == 0)
				serialIdList = CommonFunction.GetSerialList();
			else
			{
				serialIdList.Add(sId);
			}
			int sCounter = serialIdList.Count;

			WatchMustHtmlBuilder wmhb = new WatchMustHtmlBuilder(); ;
			int counter = 0;
			foreach (int entity in serialIdList)
			{
				counter++;
				OnLog("     Get serialPage WatchMusts:" + entity + " (" + counter + "/" + sCounter + ")...", false);
				try
				{
					wmhb.BuilderDataOrHtml(entity);
				}
				catch (Exception ex)
				{
					OnLog(ex.ToString(), false);
				}
			}
		}
        */
		/// <summary>
		/// 得到焦点处新闻的分类
		/// </summary>
		/// <param name="sId"></param>
		public void GetFocusNewsHTML(int sId)
		{
			List<int> serialIdList = new List<int>();
			if (sId == 0)
				serialIdList = CommonFunction.GetSerialList();
			else
			{
				serialIdList.Add(sId);
			}
			int sCounter = serialIdList.Count;

			FocusNewsHtmlBuilder fnhb = new FocusNewsHtmlBuilder(); ;
			int counter = 0;
			foreach (int entity in serialIdList)
			{
				counter++;
				OnLog("     Get serialPage FocusNews:" + entity + " (" + counter + "/" + sCounter + ")...", false);
				try
				{
					fnhb.BuilderDataOrHtml(entity);
				}
				catch (Exception ex)
				{
					OnLog(ex.Message, false);
				}
			}
		}

		/// <summary>
		/// 得到焦点处新闻的分类
		/// </summary>
		/// <param name="sId"></param>
		public void GetPinceBlockHTML(int sId)
		{
			List<int> serialIdList = new List<int>();
			if (sId == 0)
				serialIdList = CommonFunction.GetSerialList();
			else
			{
				serialIdList.Add(sId);
			}
			int sCounter = serialIdList.Count;

			PingceBlockHtmlBuilder fnhb = new PingceBlockHtmlBuilder();
			int counter = 0;
			foreach (int entity in serialIdList)
			{
				counter++;
				OnLog("     Get serialPage PingceBlock for csSummary:" + entity + " (" + counter + "/" + sCounter + ")...", false);
				try
				{
					fnhb.BuilderDataOrHtml(entity);
				}
				catch (Exception ex)
				{
					OnLog(ex.Message, false);
				}
			}
		}

		/// <summary>
		/// 得到内容
		/// </summary>
		public void GetContent()
		{
			try
			{
				//GetMaintanceMessage(0);
                //GetMaintancePriceMessage(0); // del by lsf 2016-01-06
				//保养新闻已存入数据库
				//GetMaintanceNews(0);
				ForBaiduAlding();
				/* // 开放 Jul.16.2011 by chengl
				GetFocusNews(0);
				GetNews(0);
				GetDaoGouNews(0);
				GetShiJiaNews(0);
				GetYongCheNews(0);
				GetHangQingNews(0);
				GetPingCeNews(0);
				GetTreePingCe(0);*/

				//GetFocusNewsHTML(0);
				//GetPinceBlockHTML(0);

				GetPingCeImageXml(0);
				GetPingCeShiPinXml(0);
			}
			catch (System.Exception ex)
			{
				OnLog(ex.ToString(), true);
			}
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

		/// <summary>
		/// 子品牌全部新闻
		/// </summary>
		public void GetAllNews(int serialId)
		{
			if (serialId < 1)
			{
				OnLog("子品牌id不能小于零！", true);
				return;
			}
			//先是焦点新闻
			OnLog(CarNewsTypes.serialfocus.ToString() + "...", true);
			GetFocusNews(serialId);

			List<CarNewsTypes> types = new List<CarNewsTypes>()
			{
				CarNewsTypes.xinwen,
				CarNewsTypes.daogou,
				CarNewsTypes.shijia,
				CarNewsTypes.yongche,
				CarNewsTypes.pingce,
				CarNewsTypes.treepingce,
				CarNewsTypes.gaizhuang,
				CarNewsTypes.keji,
				CarNewsTypes.anquan,
				CarNewsTypes.baoyang,
				CarNewsTypes.hangqing
			};
			foreach (CarNewsTypes carNewsType in types)
			{
				OnLog(carNewsType.ToString() + "...", true);
				SaveNewsContent(serialId, carNewsType);
			}
			OnLog("完成！", true);
		}

		/// <summary>
		/// 清除过期的子品牌焦点新闻顺序设置
		/// </summary>
		public void ClearTimeoutFocusNews(int serialId)
		{
			List<int> serialIdList = new List<int>();
			SqlParameter blockType = new SqlParameter("@BlockType", (int)NewsBlockOrderTypes.serialfocus);
			SqlParameter nowTime = new SqlParameter("@NowTime", DateTime.Now.Date);
			SqlParameter objId = new SqlParameter("@ObjId", SqlDbType.Int);
			if (serialId == 0)
			{
				DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text
					, "SELECT [ObjId] FROM NewsBlockOrder WHERE BlockType=@BlockType AND EndTime<@NowTime AND IsTimeout=0"
					, blockType
					, nowTime);
				if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
				{
					OnLog("    ClearTimeoutFocusNews 没有可执行的数据.", true);
					return;
				}
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					int id = Convert.ToInt32(row["objid"]);
					if (!serialIdList.Contains(id))
						serialIdList.Add(id);
				}
			}
			else
			{
				serialIdList.Add(serialId);
			}
			int sCounter = serialIdList.Count;

			FocusNewsHtmlBuilder fnhb = new FocusNewsHtmlBuilder();
			int counter = 0;
			foreach (int entity in serialIdList)
			{
				counter++;
				OnLog("    ClearTimeoutFocusNews:" + entity + " (" + counter + "/" + sCounter + ")...", false);
				try
				{
					fnhb.BuilderDataOrHtml(entity);

					objId.Value = entity;

					SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text
						, "UPDATE NewsBlockOrder SET IsTimeout = 1 WHERE BlockType=@BlockType AND EndTime<@NowTime AND IsTimeout=0 AND [ObjId]=@ObjId"
						, blockType
						, nowTime
						, objId);
				}
				catch (Exception ex)
				{
					OnLog(ex.Message, false);
				}
			}
        }

        #region 子品牌 综述页 车款列表  del by lsf 2016-01-06
        /*
		/// <summary>
		/// 匿名类型转化
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T Cast<T>(object o, T type)
		{
			return (T)o;
		}
        
		/// <summary>
		/// 子品牌 车款列表 html
		/// </summary>
		/// <param name="serialId">子品牌ID</param>
		public static void MakeSerialCarListHtml(int serialId)
		{
			string fileName = Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"SerialSummary\{0}.html", serialId));
			StringBuilder sb = new StringBuilder();
			List<string> carSaleListHtml = new List<string>();
			List<string> carNoSaleListHtml = new List<string>();
			List<string> carWaitSaleListHtml = new List<string>();
			Dictionary<int, SerialInfo> dictSerial = CommonData.SerialDic;
			SerialInfo serialInfo = dictSerial.ContainsKey(serialId) ? dictSerial[serialId] : null;
			string serialAllspell = dictSerial.ContainsKey(serialId) ? dictSerial[serialId].AllSpell : "";

			List<CarInfoForSerialSummaryEntity> carinfoList = SerialService.GetCarInfoForSerialSummaryBySerialId(serialId);
			int maxPv = 0;
			List<string> saleYearList = new List<string>();
			List<string> noSaleYearList = new List<string>();
			foreach (CarInfoForSerialSummaryEntity carInfo in carinfoList)
			{
				if (carInfo.CarPV > maxPv)
					maxPv = carInfo.CarPV;
				if (carInfo.CarYear.Length > 0)
				{
					string yearType = carInfo.CarYear + "款";

					if (carInfo.SaleState == "停销")
					{
						if (!noSaleYearList.Contains(yearType))
							noSaleYearList.Add(yearType);
					}
					else
					{
						if (!saleYearList.Contains(yearType))
							saleYearList.Add(yearType);
					}
				}
			}
			//排除包含在售年款
			foreach (string year in saleYearList)
			{
				if (noSaleYearList.Contains(year))
				{
					noSaleYearList.Remove(year);
				}
			}
			List<CarInfoForSerialSummaryEntity> carinfoSaleList = carinfoList.FindAll(p => p.SaleState == "在销");
			//List<CarInfoForSerialSummaryEntity> carinfoNoSaleList = carinfoList.FindAll(p => p.SaleState == "停销");
			List<CarInfoForSerialSummaryEntity> carinfoWaitSaleList = carinfoList.FindAll(p => p.SaleState == "待销");

			carinfoSaleList.Sort(StaticCompare.CompareCarByExhaustAndPowerAndInhaleType);
			//carinfoNoSaleList.Sort(StaticCompare.CompareCarByExhaustAndPowerAndInhaleType);
			carinfoWaitSaleList.Sort(StaticCompare.CompareCarByExhaustAndPowerAndInhaleType);

			noSaleYearList.Sort(StaticCompare.CompareStringDesc);

			//sb.Append("<div class=\"line_box\" id=\"car_list\">");
			sb.Append("    <h3>");
			sb.AppendFormat("        <span>{0}</span>", serialInfo.ShowName);
			sb.Append("        <div class=\"h3_tab car-comparetable-tab\">");
			sb.Append("<ul id=\"data_tab_jq5\">");
			bool isWaitSale = false;
			if (carinfoWaitSaleList.Count > 0)
			{
				isWaitSale = true;
				sb.Append("<li class=\"\">预售车款</li>");
			}
			sb.Append("<li class=\"current\">在售车款</li>");
			sb.Append("</ul>");
			if (noSaleYearList.Count > 0)
			{
				sb.Append("<ul id=\"car_nosaleyearlist\">");
				sb.Append("                <li class=\"last\">停售车款<em></em>");
				sb.Append("                    <dl style=\"display: none;\">");
				for (int i = 0; i < noSaleYearList.Count; i++)
				{
					string url = string.Format("/{0}/{1}/#car_list", serialAllspell, noSaleYearList[i].Replace("款", ""));
					if (i == noSaleYearList.Count - 1)
						sb.AppendFormat("<dd class=\"last\"><a href=\"{0}\">{1}</a></dd>", url, noSaleYearList[i]);
					else
						sb.AppendFormat("<dd><a href=\"{0}\">{1}</a></dd>", url, noSaleYearList[i]);
				}
				sb.Append("                    </dl>");
				sb.Append("                </li>");
				sb.Append("</ul>");
			}
			sb.Append("            </ul>");
			sb.Append("        </div>");
			sb.Append("    </h3>");
			if (isWaitSale)
			{
				sb.Append("    <div class=\"car-comparetable\" style=\"display: none;\" id=\"data_tab_jq5_0\">");
				sb.Append("        <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" id=\"compare_wait\">");
				sb.Append("            <tbody>");
				sb.Append(GetCarListHtml(carinfoWaitSaleList, serialInfo, maxPv));
				sb.Append("            </tbody>");
				sb.Append("        </table>");
				sb.Append("    </div>");
			}
			sb.AppendFormat("    <div class=\"car-comparetable\" id=\"data_tab_jq5_{0}\" style=\"display: block;\">", isWaitSale ? 1 : 0);
			sb.Append("        <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" id=\"compare_sale\">");
			sb.Append("            <tbody>");
			sb.Append(GetCarListHtml(carinfoSaleList, serialInfo, maxPv));
			sb.Append("            </tbody>");
			sb.Append("        </table>");
			sb.Append("    </div>");
			//sb.Append("    <div class=\"car-comparetable\" style=\"display: none;\" id=\"data_box5_2\">");
			//sb.Append("        <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">");
			//sb.Append("            <tbody>");
			//sb.Append(GetCarListHtml(carinfoNoSaleList, serialInfo, maxPv));
			//sb.Append("            </tbody>");
			//sb.Append("        </table>");
			//sb.Append("    </div>");
			sb.Append("    <div class=\"clear\"></div>");
			sb.Append("    <div class=\"more\">");
			sb.AppendFormat("<a href=\"http://dealer.bitauto.com/shijia/nb{0}/\" target=\"_blank\">我要预约试驾&gt;&gt;</a>", serialInfo.Id);
			sb.Append("    </div>");
			//sb.Append("</div>");
			CommonFunction.SaveFileContent(sb.ToString(), fileName, "utf-8");
		}
        
		/// <summary>
		/// 车型列表html
		/// </summary>
		/// <param name="carList">车款列表 list</param>
		/// <param name="serialInfo">子品牌信息</param>
		/// <param name="maxPv">最大pv</param>
		/// <returns></returns>
		private static string GetCarListHtml(List<CarInfoForSerialSummaryEntity> carList, SerialInfo serialInfo, int maxPv)
		{
			List<string> carListHtml = new List<string>();
			string serialAllspell = serialInfo == null ? "" : serialInfo.AllSpell;
			//if (carList.Count == 0)
			//    carListHtml.Add("<tr>暂无车款！</tr>");
			var querySale = carList.GroupBy(p => new { p.Engine_Exhaust, p.Engine_InhaleType, p.Engine_MaxPower }, p => p);
			foreach (IGrouping<object, CarInfoForSerialSummaryEntity> info in querySale)
			{
				
				carListHtml.Add("<tr class=\"\">");
				carListHtml.Add("    <th width=\"47%\" class=\"first-item\">");
				var key = Cast(info.Key, new { Engine_Exhaust = "", Engine_InhaleType = "", Engine_MaxPower = 0 });
				carListHtml.Add(string.Format("        <div class=\"pdL10\"><strong>{0}</strong> <b>/</b> {1}马力 {2}</div>", key.Engine_Exhaust, key.Engine_MaxPower, key.Engine_InhaleType));
				carListHtml.Add("    </th>");
				carListHtml.Add("    <th width=\"8%\" class=\"pd-left-one\">关注度</th>");
				carListHtml.Add("    <th width=\"10%\" class=\"pd-left-one\">变速箱</th>");
				carListHtml.Add("    <th width=\"12%\" class=\"pd-left-two\">指导价</th>");
				carListHtml.Add("    <th width=\"11%\" class=\"pd-left-three\">参考成交价</th>");
				carListHtml.Add("    <th width=\"12%\">&nbsp;</th>");
				carListHtml.Add("</tr>");
				List<CarInfoForSerialSummaryEntity> carGroupList = info.ToList<CarInfoForSerialSummaryEntity>();//分组后的集合

				foreach (CarInfoForSerialSummaryEntity entity in carGroupList)
				{
					string yearType = entity.CarYear.Trim();
					if (yearType.Length > 0)
						yearType += "款";
					else
						yearType = "未知年款";
					string stopPrd = "";
					if (entity.ProduceState == "停产")
						stopPrd = "<a href=\"javascript:void(0);\" class=\"ico-tingchan\">停产</a>";
					Dictionary<int, string> dictCarParams = CarService.GetCarAllParamByCarID(entity.CarID);
					// 节能补贴 Sep.2.2010
					string hasEnergySubsidy = "";
					if (dictCarParams.ContainsKey(853) && dictCarParams[853] == "3000元")
					{
						hasEnergySubsidy = "<a href=\"http://news.bitauto.com/consumerpolicy/20120704/1805753482.html\" class=\"ico-butie\" title=\"可获得3000元节能补贴\">补贴</a>";
					}
					//============2012-04-09 减税============================
					string strTravelTax = "";
					if (dictCarParams.ContainsKey(895))
					{
						strTravelTax = "<a target=\"_blank\" title=\"{0}\" href=\"http://news.bitauto.com/others/20120308/0805618954.html\" class=\"ico-jianshui\">减税</a>";
						if (dictCarParams[895] == "减半")
							strTravelTax = string.Format(strTravelTax, "减征50%车船使用税");
						else if (dictCarParams[895] == "免征")
							strTravelTax = string.Format(strTravelTax, "免征车船使用税");
						else
							strTravelTax = "";
					}
					//string strBest = "<a href=\"#\" class=\"ico-tuijian\">推荐</a>";
					carListHtml.Add("<tr class=\"\">");
					carListHtml.Add("<td>");
					carListHtml.Add(string.Format("    <div class=\"pdL10\"><a href=\"/{0}/m{1}/\">{2} {3}</a> {4}</div>",
						serialAllspell, entity.CarID, yearType, entity.CarName, stopPrd + strTravelTax + hasEnergySubsidy));
					carListHtml.Add("</td>");
					carListHtml.Add("<td>");
					carListHtml.Add("    <div class=\"w\">");
					//计算百分比
					int percent = (int)Math.Round((double)entity.CarPV / maxPv * 100.0, MidpointRounding.AwayFromZero);

					carListHtml.Add(string.Format("        <div class=\"p\" style=\"width: {0}%\"></div>", percent));
					carListHtml.Add("    </div>");
					carListHtml.Add("</td>");
					// 档位个数
					string forwardGearNum = (dictCarParams.ContainsKey(724) && dictCarParams[724] != "无级") ? dictCarParams[724] + "档" : "";

					carListHtml.Add(string.Format("<td>{0}</td>", forwardGearNum + entity.TransmissionType));
					carListHtml.Add(string.Format("<td style=\"text-align: right\"><span>{0}</span><a title=\"购车费用计算\" class=\"car-comparetable-ico-cal\" href=\"/gouchejisuanqi/?carid={1}\"></a></td>", entity.ReferPrice, entity.CarID));
					if (entity.CarPriceRange.Trim().Length == 0)
						carListHtml.Add(string.Format("    <td style=\"text-align: right\"><span>{0}</span></td>", "暂无报价"));
					else
					{
						//取最低报价
						string minPrice = entity.CarPriceRange;
						if (entity.CarPriceRange.IndexOf("-") != -1)
							minPrice = entity.CarPriceRange.Substring(0, entity.CarPriceRange.IndexOf('-'));

						carListHtml.Add(string.Format("<td style=\"text-align: right\"><span><a href=\"/{0}/m{1}/baojia/\">{2}</a></span></td>", serialAllspell, entity.CarID, minPrice));
					}
					carListHtml.Add("<td>");
					carListHtml.Add(string.Format("<div class=\"car-summary-btn\"><a href=\"http://dealer.bitauto.com/zuidijia/nb{0}/nc{1}/\"><s>询价</s></a></div>", serialInfo.Id, entity.CarID));
					carListHtml.Add(string.Format("<div class=\"car-summary-btn\" id=\"carcompare_btn_{0}\"><a target=\"_self\" href=\"javascript:addCarToCompare('{0}','{1}');\"><s>对比</s></a></div>", entity.CarID, entity.CarName));
					carListHtml.Add("    </td>");
					carListHtml.Add("</tr>");
				}
			}
			return string.Concat(carListHtml.ToArray());
		}
         * */
        #endregion
    }
}
