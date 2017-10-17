using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
	public class CarBLL
	{
		/// <summary>
		/// 车型更新消息队列 (车型新闻消息队列 )
		/// </summary>
		private static string CarUpdateMessageAdd = CommonData.CommonSettings.QueueName;
		private static readonly string CarMessageBody = "<?xml version=\"1.0\" ?><MessageBody><From>Car</From><ContentType>{0}</ContentType><ContentId>{1}</ContentId><UpdateTime>{2}</UpdateTime><ActionType>{3}</ActionType></MessageBody>";
		private static readonly string CarMessageLabel = "Car Update Message:{0} ID:{1} {2} {3}";

		/// <summary>
		/// 主品牌添加
		/// </summary>
		/// <param name="bodyElement"></param>
		public void MasterBrandAdd(XElement bodyElement)
		{
			SendMessage(bodyElement, "MasterBrand", "Add");
		}

		/// <summary>
		/// 主品牌更新
		/// </summary>
		/// <param name="bodyElement"></param>
		public void MasterBrandUpdate(XElement bodyElement)
		{
			SendMessage(bodyElement, "MasterBrand", "Update");
		}

		/// <summary>
		/// 主品牌删除
		/// </summary>
		/// <param name="bodyElement"></param>
		public void MasterBrandDelete(XElement bodyElement)
		{
			SendMessage(bodyElement, "MasterBrand", "Delete");
		}

		/// <summary>
		/// 添加品牌
		/// </summary>
		/// <param name="bodyElement"></param>
		public void BrandAdd(XElement bodyElement)
		{
			SendMessage(bodyElement, "Brand", "Add");
		}

		/// <summary>
		/// 更新品牌
		/// </summary>
		/// <param name="bodyElement"></param>
		public void BrandUpdate(XElement bodyElement)
		{
			SendMessage(bodyElement, "Brand", "Update");
		}

		/// <summary>
		/// 品牌删除
		/// </summary>
		/// <param name="bodyElement"></param>
		public void BrandDelete(XElement bodyElement)
		{
			SendMessage(bodyElement, "Brand", "Delete");
		}

		/// <summary>
		/// 添加车系
		/// </summary>
		/// <param name="bodyElement"></param>
		public void SerialAdd(XElement bodyElement)
		{
			SendMessage(bodyElement, "Serial", "Add");
		}

		/// <summary>
		/// 更新车系
		/// </summary>
		/// <param name="bodyElement"></param>
		public void SerialUpdate(XElement bodyElement)
		{
			SendMessage(bodyElement, "Serial", "Update");
		}

		/// <summary>
		/// 删除车系
		/// </summary>
		/// <param name="bodyElement"></param>
		public void SerialDelete(XElement bodyElement)
		{
			SendMessage(bodyElement, "Serial", "Delete");
		}

		/// <summary>
		/// 添加车款
		/// </summary>
		/// <param name="bodyElement"></param>
		public void CarAdd(XElement bodyElement)
		{
			SendMessage(bodyElement, "Car", "Add");
		}

		/// <summary>
		/// 更新车款
		/// </summary>
		/// <param name="bodyElement"></param>
		public void CarUpdate(XElement bodyElement)
		{
			SendMessage(bodyElement, "Car", "Update");
		}

		/// <summary>
		/// 删除车款
		/// </summary>
		/// <param name="bodyElement"></param>
		public void CarDelete(XElement bodyElement)
		{
			SendMessage(bodyElement, "Car", "Delete");
		}

		/// <summary>
		/// 添加车款参配
		/// </summary>
		/// <param name="bodyElement"></param>
		public void CarParamAdd(XElement bodyElement)
		{
			XElement ele = ChangeBodyElement(bodyElement);
			if (ele != null)
			{
				CarAdd(ele);
			}
		}

		/// <summary>
		/// 更新车款参配
		/// </summary>
		/// <param name="bodyElement"></param>
		public void CarParamUpdate(XElement bodyElement)
		{
			XElement ele = ChangeBodyElement(bodyElement);
			if (ele != null)
			{
				CarUpdate(ele);
			}
		}

		/// <summary>
		/// 删除车款
		/// </summary>
		/// <param name="bodyElement"></param>
		public void CarParamDelete(XElement bodyElement)
		{
			XElement ele = ChangeBodyElement(bodyElement);
			if (ele != null)
			{
				CarDelete(ele);
			}
		}

		private XElement ChangeBodyElement(XElement bodyElement)
		{
			int carid = 0;
			XmlDocument xmldoc = new XmlDocument();
			try
			{
				
				xmldoc.LoadXml(bodyElement.ToString());
				XmlNode styleNode = xmldoc.SelectSingleNode("Body/Style");
				carid = styleNode == null ? 0 : Convert.ToInt32(styleNode.Attributes["Id"].Value);
				if (carid > 0)
				{
					XmlNode entityIdNode = xmldoc.SelectSingleNode("Body/EntityId");
					if (entityIdNode != null)
					{
						entityIdNode.InnerText = carid.ToString();
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLog("<!--把车款参配消息转换成车款消息异常：消息体：" + bodyElement.ToString() + "\n\r" + ex.Message + "-->");
				return null;
			}
			if (carid == 0)
			{
				Log.WriteLog("<!--车款参配消息体结构错误：消息体：" + bodyElement.ToString() + "-->");
				return null;
			}
			return XElement.Parse(xmldoc.OuterXml);

		}

		/// <summary>
		/// 添加厂商
		/// </summary>
		/// <param name="bodyElement"></param>
		public void ProducerAdd(XElement bodyElement)
		{
			SendMessage(bodyElement, "Producer", "Add");
		}

		/// <summary>
		/// 编辑厂商
		/// </summary>
		/// <param name="bodyElement"></param>
		public void ProducerUpdate(XElement bodyElement)
		{
			SendMessage(bodyElement, "Producer", "Update");
		}

		/// <summary>
		/// 删除厂商
		/// </summary>
		/// <param name="bodyElement"></param>
		public void ProducerDelete(XElement bodyElement)
		{
			SendMessage(bodyElement, "Producer", "Delete");
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="entityType"></param>
		/// <param name="operateType"></param>
		private void SendMessage(XElement bodyElement, string entityType, string operateType)
		{
			string entityId = bodyElement.Element("EntityId").Value;
			string label = string.Format(CarMessageLabel, entityType, entityId, operateType, DateTime.Now.ToString());
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(string.Format(CarMessageBody, entityType, entityId, DateTime.Now.ToString("yyyy-MM-dd"), operateType));
			int result = Common.MessageService.SendMessage(CarUpdateMessageAdd, doc, label);
			Log.WriteLog("<!-- 发送消息至车型频道新闻消息服务 是否成功: " + result.ToString() + "  TargetObject:" + entityType + " ObjectIdentity:" + entityId + " OperateType:" + entityId + "-->");
		}
	}
}
