using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Messaging;

namespace BitAuto.CarDataUpdate.Common
{
	public class MessageService
	{
		public MessageService() { }
		/// <summary>
		/// 接受消息队列
		/// </summary>
		/// <param name="queueName">消息队列路径</param>
		/// <returns>返回消息队列内容 xml</returns>
		public static XmlDocument ReceiverMessage(string queueName)
		{
			if (String.IsNullOrEmpty(queueName))
				Log.WriteErrorLog("未指定消息队列");
			MessageQueue queue = new MessageQueue(queueName);
			queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) });

			//等待时间 3s
			TimeSpan ts = new TimeSpan(3 * 1000 * 1000 * 10);
			Message msg = null;
			try
			{
				msg = queue.Receive(ts);
				Log.WriteLog("Receive successed!");
			}
			catch (MessageQueueException mqex)
			{
				msg = null;
				if (mqex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
				{
					//Console.WriteLine("No message arrived in queue.");
				}
				else
					Log.WriteErrorLog(mqex.ToString());
			}
			catch (Exception ex)
			{
				msg = null;
				Log.WriteErrorLog(ex);
			}
			if (msg == null)
				return null;
			XmlDocument xmlDoc = (XmlDocument)msg.Body;
			return xmlDoc;
		}
		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="queueName">消息队列路径</param>
		/// <param name="msgBody">消息队列内容</param>
		public static void SendMessage(string queueName, string msgBody)
		{
			if (String.IsNullOrEmpty(queueName))
				Log.WriteErrorLog("未指定消息队列");
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(msgBody);
				MessageQueue queue = new MessageQueue(queueName);
				queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) });
				Message message = new Message();
				message.Body = xmlDoc;
				queue.Send(message);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="queueName">消息队列路径</param>
		/// <param name="msgBody">消息队列内容</param>
		public static void SendMessage(string queueName, XmlDocument msgBody)
		{
			if (String.IsNullOrEmpty(queueName))
				Log.WriteErrorLog("未指定消息队列");
			try
			{
				MessageQueue queue = new MessageQueue(queueName);
				queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) });
				Message message = new Message();
				message.Body = msgBody;
				queue.Send(message);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

		public static int SendMessage(string queueName, XmlDocument msgBody, string label)
		{
			int result = 0;
			if (String.IsNullOrEmpty(queueName))
			{
				Log.WriteErrorLog("未指定消息队列:BitAuto.CarDataUpdate.Common.MessageService.SendMessage");
				return -1;
			}

			try
			{
				MessageQueue queue = new MessageQueue(queueName);
				queue.Send(msgBody, label);
				result = 1;
				//queue.Close();待验证，有没有用
			}
			catch (Exception ex)
			{
				result = -1;
				Log.WriteErrorLog(ex.ToString());
			}
			return result;
		}
	}
}
