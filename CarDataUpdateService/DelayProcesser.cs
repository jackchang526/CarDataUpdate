using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.Utils.Data;
using System.Data;
using BitAuto.Utils;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.Service
{
	public class DelayProcesser
	{
		private static List<DelayMessage> messageList;

		/// <summary>
		/// 初始化延期消息列表
		/// </summary>
		public static void InitDelayMessageList()
		{
			messageList = new List<DelayMessage>();

			DataSet ds = SqlHelper.ExecuteDataset(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, "SELECT Id, ContentId, UpdateTime, ContentType, ContentBody FROM DelayNews WHERE [State]=1");
			if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				DelayMessage newObj = null;
				DataRowCollection rows = ds.Tables[0].Rows;
				foreach (DataRow row in rows)
				{
					newObj = new DelayMessage()
					{
						Id = ConvertHelper.GetInteger(row["Id"]),
						ContentId = ConvertHelper.GetInteger(row["ContentId"]),
						UpdateDate = ConvertHelper.GetDateTime(row["UpdateTime"]),
						ContentType = row["ContentType"].ToString()
					};
					newObj.ContentBody = new XmlDocument();
					newObj.ContentBody.LoadXml(row["ContentBody"].ToString());

					messageList.Add(newObj);
				}
			}
		}
		/// <summary>
		/// 获取已经到期的消息
		/// </summary>
		/// <returns></returns>
		public static XmlDocument GetDelayMessage()
		{
			DelayMessage tmpDelayMsg = null;
			XmlDocument msgDoc = null;
			if (messageList == null) return msgDoc;
			foreach (DelayMessage delayMsg in messageList)
			{
				//延时5分钟，以免取不到新闻内容
				if (delayMsg.UpdateDate <= DateTime.Now.AddMinutes(-5))
				{
					tmpDelayMsg = delayMsg;
					msgDoc = tmpDelayMsg.ContentBody;
					//删除文件中的备份
					DelDelyMessage(delayMsg.Id);
					break;
				}
			}
			if (tmpDelayMsg != null)
				messageList.Remove(tmpDelayMsg);
			return msgDoc;
		}
		/// <summary>
		/// 删除消息
		/// </summary>
		private static void DelDelyMessage(int rowId)
		{
			if (rowId <= 0)
				return;

			SqlHelper.ExecuteNonQuery(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "UPDATE DelayNews SET	[State] = 0 WHERE Id=@Id", new SqlParameter("@Id", rowId));
		}
		/// <summary>
		/// 添加定时消息到
		/// </summary>
		internal static void AddDelayMessage(ContentMessage contentMsg)
		{
			if (messageList == null)
				messageList = new List<DelayMessage>();

			DelayMessage delayMsg = messageList.Find(delay => delay.ContentId == contentMsg.ContentId);
			if (delayMsg != null)
			{
				messageList.Remove(delayMsg);
				DelDelyMessage(delayMsg.Id);
			}

			try
			{
				int rowId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString
					, CommandType.Text
					// , "INSERT INTO DelayNews(ContentId,UpdateTime,ContentType,[State],ContentBody) VALUES(@ContentId,@UpdateTime,@ContentType,@State,@ContentBody) SELECT SCOPE_IDENTITY()"
					, "INSERT INTO DelayNews(ContentId,UpdateTime,ContentType,[State],ContentBody) VALUES(@ContentId,@UpdateTime,@ContentType,1,@ContentBody) SELECT SCOPE_IDENTITY()"
					, new SqlParameter[]{
                    new SqlParameter("@ContentId", contentMsg.ContentId)
                    ,new SqlParameter("@UpdateTime", contentMsg.UpdateTime)
                    ,new SqlParameter("@ContentType", contentMsg.ContentType)
                    // ,new SqlParameter("@State",1)
                    ,new SqlParameter("@ContentBody", contentMsg.ContentBody.OuterXml) }));

				XmlNode rowIdNode = contentMsg.ContentBody.SelectSingleNode("/MessageBody/DelayRowId");
				if (rowIdNode == null)
				{
					rowIdNode = contentMsg.ContentBody.CreateElement("DelayRowId");
					rowIdNode.InnerText = rowId.ToString();
					contentMsg.ContentBody.DocumentElement.AppendChild(rowIdNode);
				}
				else
				{
					rowIdNode.InnerText = rowId.ToString();
				}

				messageList.Add(new DelayMessage()
				{
					Id = rowId
					,
					UpdateDate = contentMsg.UpdateTime
					,
					ContentType = contentMsg.ContentType
					,
					ContentId = contentMsg.ContentId
					,
					ContentBody = contentMsg.ContentBody
				});
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(string.Format("@ContentId:[{0}],@UpdateTime:[{1}],@ContentType:[{2}],@ContentBody:[{3}]", contentMsg.ContentId, contentMsg.UpdateTime, contentMsg.ContentType, contentMsg.ContentBody.OuterXml));
				throw exp;
			}
		}

		public static void DelayEventProcesser(BaseProcesser sender, ContentMessage contentMsg)
		{
			if (contentMsg.ContentBody == null) return;

			int retestNum = 1;
			XmlNode reTestNode = contentMsg.ContentBody.SelectSingleNode("/MessageBody/ReTestNum");
			if (reTestNode == null)
			{
				reTestNode = contentMsg.ContentBody.CreateElement("ReTestNum");
				reTestNode.InnerText = "1";
				contentMsg.ContentBody.DocumentElement.AppendChild(reTestNode);
			}
			else
			{
				retestNum = ConvertHelper.GetInteger(reTestNode.InnerText);
				if (retestNum > 4)
				{
					XmlNode rowIdNode = contentMsg.ContentBody.SelectSingleNode("/MessageBody/DelayRowId");
					if (rowIdNode != null)
					{
						int rowId = ConvertHelper.GetInteger(rowIdNode.InnerText);
						if (rowId > 0)
							DelDelyMessage(rowId);
					}
					Log.WriteLog("error 延时处理重试超过5次，取消操作!contentid:" + contentMsg.ContentId.ToString());
					return;
				}
				if (retestNum < 1)
					reTestNode.InnerText = "1";
			}

			contentMsg.UpdateTime = DateTime.Now.AddMinutes(15);
			reTestNode.InnerText = (retestNum + 1).ToString();

			AddDelayMessage(contentMsg);
		}
	}
}
