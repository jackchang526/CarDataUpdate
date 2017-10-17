using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Services.Cache;

namespace BitAuto.CarDataUpdate.Common.NewsNumXml
{
	/// <summary>
	/// 该类由新闻服务使用，计划任务不可使用！
	/// </summary>
	public static class NewsNumXmlDocument
	{
		private readonly static object _lockObj = new object();
		private static Dictionary<CarTypes, NewsNumXmlData> _cacheXml = new Dictionary<CarTypes, NewsNumXmlData>();

		/// <summary>
		/// 获取新闻数xml
		/// </summary>
		public static XmlDocument GetNewsNumXmlDocument(CarTypes carType)
		{
			lock (_lockObj)
			{
				if (!_cacheXml.ContainsKey(carType))
				{
					NewsNumXmlData data = GetNewsNumXmlData(carType);
					if (data == null)
						return null;

					_cacheXml.Add(carType, data);
				}
				return _cacheXml[carType].GetXmlDocument();
			}
		}
		public static void SaveNewsNumXmlDocument(CarTypes carType)
		{
			lock (_lockObj)
			{
				if (_cacheXml.ContainsKey(carType))
				{
					_cacheXml[carType].SaveXmlDocument();
				}
			}
		}

		/// <summary>
		/// 获取新闻数xml
		/// </summary>
		private static NewsNumXmlData GetNewsNumXmlData(CarTypes carType)
		{
			switch (carType)
			{
				case CarTypes.Serial:
					return new SerialNewsNumXml();
				case CarTypes.Brand:
					return new BrandNewsNumXml();
				case CarTypes.MasterBrand:
					return new MasterBrandNewsNumXml();
				default:
					return null;
			}
		}
		#region newsnum类
		abstract class NewsNumXmlData
		{
			protected DateTime _lastDateTime = DateTime.MinValue;
			/// <summary>
			/// 新闻数xml路径
			/// </summary>
			public string XmlFilePath { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public CarTypes NewsCarType { get; set; }
			/// <summary>
			/// 最近修改时间
			/// </summary>
			public DateTime lastDateTime { get { return _lastDateTime; } }
			protected XmlDocument _doc;
			public XmlDocument GetXmlDocument()
			{
				if (string.IsNullOrEmpty(XmlFilePath))
					return null;

				if (!File.Exists(XmlFilePath))
				{
					CreateNewsNumXml();
				}

				DateTime tmpTime = new FileInfo(XmlFilePath).LastWriteTime;
				if (_lastDateTime != tmpTime)
				{
					_lastDateTime = tmpTime;
					_doc = CommonFunction.GetXmlDocument(XmlFilePath);
				}
				return _doc;
			}
			public void SaveXmlDocument()
			{
				if (string.IsNullOrEmpty(XmlFilePath))
					return;

				CommonFunction.SaveXMLDocument(_doc, XmlFilePath);
				_lastDateTime = new FileInfo(XmlFilePath).LastWriteTime;

				//add by sk 2013.04.27 数据同时放到memcache里（文件读取不到或者被占用情况）
				if (NewsCarType == CarTypes.Serial)
				{
					string memCacheKey = "Car_XML_Data_Serial_NewsNum";
					// modified by chengl May.6.2013 缓存时间改为1天
					if (_doc != null && !string.IsNullOrEmpty(_doc.OuterXml))
						CommonFunction.MemcacheInsert(memCacheKey, _doc.OuterXml, 1000 * 60 * 60 * 24 * 3);
					else
						Log.WriteErrorLog("FunctionName:NewsNumXmlDocument.SaveXmlDocument memcache Key:Car_XML_Data_Serial_NewsNum 插入内容为空。");
					// DistCacheWrapper.Insert(memCacheKey, _doc.OuterXml, 1000 * 60 * 10);
				}
			}
			public virtual void CreateNewsNumXml()
			{
				if (File.Exists(XmlFilePath)) return;

				XmlDocument numDoc = new XmlDocument();
				XmlElement root = numDoc.CreateElement("root");
				numDoc.AppendChild(root);
				XmlDeclaration declarEle = numDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
				numDoc.InsertBefore(declarEle, root);

				CommonFunction.SaveXMLDocument(numDoc, XmlFilePath);
			}
		}
		class SerialNewsNumXml : NewsNumXmlData
		{
			public SerialNewsNumXml()
			{
				XmlFilePath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\newsNum.xml");
				NewsCarType = CarTypes.Serial;
			}
			public override void CreateNewsNumXml()
			{
				if (File.Exists(XmlFilePath)) return;

				XmlDocument numDoc = new XmlDocument();
				XmlElement root = numDoc.CreateElement("SerilaList");
				numDoc.AppendChild(root);
				XmlDeclaration declarEle = numDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
				numDoc.InsertBefore(declarEle, root);

				CommonFunction.SaveXMLDocument(numDoc, XmlFilePath);
			}
		}
		class BrandNewsNumXml : NewsNumXmlData
		{
			public BrandNewsNumXml()
			{
				XmlFilePath = Path.Combine(CommonData.CommonSettings.SavePath, "Brand\\NewsNumber.xml");
			}
		}
		class MasterBrandNewsNumXml : NewsNumXmlData
		{
			public MasterBrandNewsNumXml()
			{
				XmlFilePath = Path.Combine(CommonData.CommonSettings.SavePath, "MasterBrand\\NewsNumber.xml");
			}
		}
		#endregion
	}
}
