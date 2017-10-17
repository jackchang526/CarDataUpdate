using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Messaging;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using System.IO;

namespace BitAuto.CarDataUpdate.Service
{
	/// <summary>
	/// 消息接收类
	/// </summary>
	public class MessageReceiver
	{
		public MessageReceiver()
		{

		}
		/// <summary>
		/// 获取新消息
		/// </summary>
		public ContentMessage ReceiverMessage()
		{
			string queueName = Common.CommonData.CommonSettings.QueueName;
			if (String.IsNullOrEmpty(queueName))
				throw (new Exception("未指定消息队列！"));
			MessageQueue queue = new MessageQueue(queueName);
			queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) });

			//等待时间
			TimeSpan ts = new TimeSpan(3 * 1000 * 1000 * 10);
			Message msg = null;
			try
			{
				msg = queue.Receive(ts);
				Log.WriteLog("Receive successed!");
			}
			catch (MessageQueueException)
			{
				msg = null;
			}
			catch (Exception ex)
			{
				msg = null;
				Log.WriteErrorLog(ex);
			}
			if (msg == null)
				return null;
			XmlDocument xmlDoc = (XmlDocument)msg.Body;
			// 暂时先写文件 用于比较和数据库的一致性
			WriteContentMsgBodyLog(xmlDoc);
			ContentMessage contentMsg = TranslateToContentMessage(xmlDoc);
			// 将消息写入消息日志数据库 0.86\baadb1 AutoCarChannelManage 库 MessageLog 表
			CommonFunction.InsertContentMsgBodyLogDB(contentMsg, Guid.Empty);
			//modified by sk 2015.09.17 只针对 cms 延迟新闻
			if (contentMsg.ContentType.ToLower() == "news")
			{
				if (contentMsg.UpdateTime > DateTime.Now)
				{
					Log.WriteLog("Send to DelayMessageList!");
					//发布日期晚的文章放入到延期处理列表
					DelayProcesser.AddDelayMessage(contentMsg);
					// modified by chengl Sep.3.2013
					// 延迟新闻也处理 用于删除已入库的数据(适用于CMS和降价新闻)
					// return null;
				}
			}
			return contentMsg;
		}
		/// <summary>
		/// 获取已经到期的延期消息
		/// </summary>
		/// <returns></returns>
		public ContentMessage GetDelayMessage()
		{
			XmlDocument xmlDoc = DelayProcesser.GetDelayMessage();
			if (xmlDoc != null)
			{
				Log.WriteLog("Get Message from DelayMessageList!");
				ContentMessage msg = TranslateToContentMessage(xmlDoc);
				return msg;
			}
			else
				return null;
		}

		/// <summary>
		/// 将收到消息重新封装
		/// </summary>
		/// <param name="msgDoc"></param>
		/// <returns></returns>
		public ContentMessage TranslateToContentMessage(XmlDocument msgDoc)
		{
			if (msgDoc == null)
				throw (new Exception("无消息数据！"));
			ContentMessage contentMsg = new ContentMessage();

			XmlNode typeEle = msgDoc.SelectSingleNode("/MessageBody/ContentType");
			if (typeEle == null || string.IsNullOrEmpty(typeEle.InnerText))
				throw (new Exception("消息格式错误"));
			contentMsg.ContentType = typeEle.InnerText.ToLower();

			XmlNode fromEle = msgDoc.SelectSingleNode("/MessageBody/From");
			if (fromEle == null)
				throw (new Exception("消息格式错误"));
			contentMsg.From = fromEle.InnerText;

			XmlNode idEle = msgDoc.SelectSingleNode("/MessageBody/ContentId");
			if (idEle == null)
				throw (new Exception("消息格式错误"));
			contentMsg.ContentId = ConvertHelper.GetInteger(idEle.InnerText);

			// 是否是删除消息
			XmlNode isDeleteEle = msgDoc.SelectSingleNode("/MessageBody/DeleteOp");
			if (isDeleteEle != null)
			{
				contentMsg.IsDelete = ConvertHelper.GetBoolean(isDeleteEle.InnerText);
			}

			XmlNode timeEle = msgDoc.SelectSingleNode("/MessageBody/UpdateTime");
			if (timeEle == null)
				throw (new Exception("消息格式错误"));
			DateTime upTime = DateTime.Now;
			bool isDate = DateTime.TryParse(timeEle.InnerText, out upTime);
			if (isDate)
				contentMsg.UpdateTime = upTime;
			else
				contentMsg.UpdateTime = DateTime.Now;

			contentMsg.ContentBody = msgDoc;

			Log.WriteLog("Translate a news message:id=" + contentMsg.ContentId);
			Log.WriteLog("update:" + contentMsg.UpdateTime.ToString());
			return contentMsg;
		}

		/// <summary>
		/// 将消息写入消息日志数据库 0.86\baadb1 AutoCarChannelManage 库 MessageLog 表
		/// </summary>
		/// <param name="cm"></param>
		public void InsertContentMsgBodyLogDB(ContentMessage cm)
		{
			string sp = "SP_MessageLog_Insert";
			SqlParameter[] param = new SqlParameter[7] { 
				new SqlParameter("@MessageFrom",SqlDbType.VarChar,50),
				new SqlParameter("@ContentType",SqlDbType.VarChar,50),
				new SqlParameter("@ContentId",SqlDbType.Int),
				new SqlParameter("@UpdateTime",SqlDbType.DateTime),
				new SqlParameter("@IsDelete",SqlDbType.Bit),
				new SqlParameter("@MessageBody",SqlDbType.VarChar,1000),
				new SqlParameter("@AutoID",SqlDbType.BigInt)
			};
			param[0].Value = cm.From;
			param[1].Value = cm.ContentType;
			param[2].Value = cm.ContentId;
			param[3].Value = cm.UpdateTime;
			param[4].Value = cm.IsDelete;
			param[5].Value = cm.ContentBody.InnerXml.Length > 1000 ?
				cm.ContentBody.InnerXml.Substring(0, 1000) : cm.ContentBody.InnerXml;
			param[6].Direction = ParameterDirection.Output;
			int res = BitAuto.Utils.Data.SqlHelper.ExecuteNonQuery(
				CommonData.ConnectionStringSettings.CarChannelManageConnString
				, CommandType.StoredProcedure, sp, param);
			// 设置日志ID
			int autoID = 0;
			if (int.TryParse(param[6].Value.ToString(), out autoID))
			{
				if (autoID > 0)
				{ cm.LogID = autoID; }
			}
		}

		/// <summary>
		/// 不写文件了，改用数据库 AutoCarChannelManage 表 MessageStat
		/// </summary>
		/// <param name="contentMsgBody"></param>
		public void WriteContentMsgBodyLog(XmlDocument contentMsgBody)
		{
			try
			{
				//if (ConvertHelper.GetBoolean(System.Configuration.ConfigurationManager.AppSettings["IsWriteContentBodyLog"]))
				//{
				string fileName = DateTime.Now.ToString("MM-dd") + ".log";
				string logPath = Path.Combine(CommonData.ApplicationPath, "Log\\msgbody");
				if (!Directory.Exists(logPath))
					Directory.CreateDirectory(logPath);
				fileName = Path.Combine(logPath, fileName);
				File.AppendAllText(fileName
					, string.Format("\r\n[{0}]\r\n{1}", DateTime.Now.ToString("HH:mm:ss"), contentMsgBody.OuterXml));
				//}
			}
			catch (System.Exception ex)
			{
				string errInfo = "\r\nwrite log error:\r\n";
				errInfo += "logInfo:" + contentMsgBody.OuterXml + "\r\nerror:" + ex.ToString();
				Log.WriteErrorLog(errInfo);
			}
		}
	}
}
