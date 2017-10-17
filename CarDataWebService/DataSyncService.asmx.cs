using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.WebService.DataSync;

namespace BitAuto.CarDataUpdate.WebService
{
	/// <summary>
	/// 数据同步服务
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	public class DataSyncService : System.Web.Services.WebService
	{
		[WebMethod(Description = "接收消息")]
		public void ReceiveMessage(string message)
		{

			Log.WriteLog("\r\n[" + DateTime.Now.ToString() + "]接收消息\r\n" + message);

			if (string.IsNullOrEmpty(message))
				return;

			try
			{
				XDocument doc = XDocument.Parse(message);
				DataSyncProvider.Execut(doc.Element("Messages"));

				//modified by chengl Mar.17.2015
				XmlDocument xDoc = new XmlDocument();
				xDoc.LoadXml(message);
				if (xDoc != null && xDoc.HasChildNodes)
				{
					Guid EntityId = Guid.Empty;
					if (xDoc.SelectSingleNode("/Messages/Body/EntityId") != null
						&& xDoc.SelectSingleNode("/Messages/Body/EntityId").InnerText != ""
						&& Guid.TryParse(xDoc.SelectSingleNode("/Messages/Body/EntityId").InnerText, out EntityId))
					{ }
					string EntityType = xDoc.SelectSingleNode("/Messages/Header/EntityType") != null ? xDoc.SelectSingleNode("/Messages/Header/EntityType").InnerText : "";
					string From = "UGC_" + (xDoc.SelectSingleNode("/Messages/Header/AppId") != null ? xDoc.SelectSingleNode("/Messages/Header/AppId").InnerText : "");
					CommonFunction.InsertContentMsgBodyLogDB(new Common.Model.ContentMessage()
					{
						From = From,
						ContentType = EntityType,
						IsDelete = false,
						UpdateTime = DateTime.Now,
						ContentId = 0,
						ContentBody = xDoc
					}, EntityId);
				}
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(string.Format("WebService接收消息时出现异常!ReceiveMessage:{0};errormsg:{1}", message, exp.ToString()));
			}
		}
	}
}
