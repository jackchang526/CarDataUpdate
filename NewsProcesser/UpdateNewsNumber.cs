using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.NewsNumXml;
using System.Timers;
using BitAuto.CarDataUpdate.Config;
using System.Xml;

namespace BitAuto.CarDataUpdate.NewsProcesser
{
	/// <summary>
	/// 更新新闻数线程
	/// </summary>
	public static class UpdateNewsNumber
	{
		private static Timer m_timer;
		/// <summary>
		/// 更新新闻数量线程
		/// </summary>
		public static void NewsNumberThread()
		{
			m_timer = new Timer();
			m_timer.Interval = (60 * 1000);
			m_timer.Elapsed += new ElapsedEventHandler(UpdateNewsNumberThread);
			m_timer.Enabled = true;
		}

		public static void UpdateNewsNumberThread(object sender, ElapsedEventArgs e)
		{
			List<NewsNumMessage> updates = NewsNumQueue.GetNewsNumMessages();
			if (updates == null || updates.Count <= 0)
				return;

			SerialNewsNumber(updates.FindAll(item => item.NewsNumMsgType == NewsNumMsgTypes.Serial || item.NewsNumMsgType == NewsNumMsgTypes.SerialYear));
			BrandNewsNumber(updates.FindAll(item => item.NewsNumMsgType == NewsNumMsgTypes.Brand));
			MasterBrandNewsNumber(updates.FindAll(item => item.NewsNumMsgType == NewsNumMsgTypes.MasterBrand));
		}
		private static void SerialNewsNumber(List<NewsNumMessage> updates)
		{
			if (updates == null || updates.Count <= 0)
				return;

			var query = from item in updates
						orderby item.ObjId ascending,
							item.CarNewsType ascending,
							item.SerialYear ascending,
							item.UpdateTime descending
						select item;

			int serialId = -1, year = -1;
			CarNewsTypes? carNewsType = null;
			XmlDocument newsNumDoc = NewsNumXmlDocument.GetNewsNumXmlDocument(CarTypes.Serial);
			XmlElement xmlEle = null, yearEle = null;
			foreach (var item in query)
			{
				if (serialId != item.ObjId)
				{
					serialId = item.ObjId;
					year = -1;
					carNewsType = null;

					xmlEle = newsNumDoc.SelectSingleNode(string.Format("/SerilaList/Serial[@id='{0}']", item.ObjId.ToString())) as XmlElement;
					if (xmlEle == null)
					{
						xmlEle = newsNumDoc.CreateElement("Serial");
						xmlEle.SetAttribute("id", item.ObjId.ToString());
						newsNumDoc.DocumentElement.AppendChild(xmlEle);
					}
				}
				if (year != item.SerialYear)
				{
					year = item.SerialYear;
					carNewsType = null;
					if (year != 0)
					{
						yearEle = xmlEle.SelectSingleNode(string.Format("Year[@year='{0}']", year.ToString())) as XmlElement;
						if (yearEle == null)
						{
							yearEle = newsNumDoc.CreateElement("Year");
							yearEle.SetAttribute("year", year.ToString());
							xmlEle.AppendChild(yearEle);
						}
					}
				}
				if (!carNewsType.HasValue || carNewsType.Value != item.CarNewsType)
				{
					carNewsType = item.CarNewsType;
					if (item.SerialYear > 0)
					{
						yearEle.SetAttribute(carNewsType.ToString().ToLower(), item.NewsCount.ToString());
					}
					else
					{
						xmlEle.SetAttribute(carNewsType.ToString().ToLower(), item.NewsCount.ToString());
						if (item.CarNewsType == CarNewsTypes.pingce)
						{
							xmlEle.SetAttribute("pingceNewsId", item.PingCeNewsId.ToString());
						}
					}
				}
			}
			if (serialId > 0)
			{
				NewsNumXmlDocument.SaveNewsNumXmlDocument(CarTypes.Serial);
			}
		}
		private static void BrandNewsNumber(List<NewsNumMessage> updates)
		{
			BrandNewsNumber(updates, CarTypes.Brand);
		}
		private static void MasterBrandNewsNumber(List<NewsNumMessage> updates)
		{
			BrandNewsNumber(updates, CarTypes.MasterBrand);
		}
		/// <summary>
		/// 仅主品牌、品牌使用
		/// </summary>
		private static void BrandNewsNumber(List<NewsNumMessage> updates, CarTypes carType)
		{
			if (updates == null || updates.Count <= 0)
				return;

			var query = from item in updates
						orderby item.ObjId ascending, item.CarNewsType ascending, item.UpdateTime descending
						select item;

			string xmlTag = carType.ToString();
			int objId = -1;
			CarNewsTypes? carNewsType = null;
			XmlDocument newsNumDoc = NewsNumXmlDocument.GetNewsNumXmlDocument(carType);
			XmlElement xmlEle = null;
			foreach (var item in query)
			{
				if (objId != item.ObjId)
				{
					objId = item.ObjId;
					carNewsType = null;

					xmlEle = newsNumDoc.SelectSingleNode(string.Format("/root/{0}[@ID='{1}']", xmlTag, objId.ToString())) as XmlElement;
					if (xmlEle == null)
					{
						xmlEle = newsNumDoc.CreateElement(xmlTag);
						xmlEle.SetAttribute("ID", objId.ToString());
						newsNumDoc.DocumentElement.AppendChild(xmlEle);
					}
				}
				if (!carNewsType.HasValue || carNewsType.Value != item.CarNewsType)
				{
					carNewsType = item.CarNewsType;
					xmlEle.SetAttribute(carNewsType.ToString().ToLower(), item.NewsCount.ToString());
				}
			}
			if (objId > 0)
			{
				NewsNumXmlDocument.SaveNewsNumXmlDocument(carType);
			}
		}
	}
}
