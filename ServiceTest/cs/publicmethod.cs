using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Messaging;
using System.Windows.Forms;

namespace ServiceTest
{
	public static class publicmethod
	{
		public static void sendMq(XmlDocument msg)
		{
			//队列名称
			string queuePath = System.Configuration.ConfigurationManager.AppSettings["QueueString"];
			//MessageQueue组件初始化
			MessageQueue queue = new MessageQueue(queuePath);

			queue.Send(msg);
		}
        /// <summary>
        /// 添加字符串数组
        /// </summary>
        /// <param name="isJoinString"></param>
        /// <returns></returns>
        public static string joinList(List<int> isJoinString)
        {
            if (isJoinString == null || isJoinString.Count < 1) return "";

            StringBuilder joinString = new StringBuilder();
            foreach (int entity in isJoinString)
            {
                joinString.Append(",");
                joinString.Append(entity.ToString());
            }
            joinString.Remove(0, 1);
            return joinString.ToString();
        }
	}
}
