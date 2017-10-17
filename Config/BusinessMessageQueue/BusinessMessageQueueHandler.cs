using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace BitAuto.CarDataUpdate.Config
{
	public class BusinessMessageQueueHandler : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, System.Xml.XmlNode section)
		{
			MessageQueueConfig config = new MessageQueueConfig();
			XmlNodeList nodeList = section.SelectNodes("/MessageQueueConfig/add");
			foreach (XmlNode node in nodeList)
			{
				string businessName = node.Attributes["name"].Value;
				MessageQueueSetting setting = new MessageQueueSetting();
				setting.BusinessName = businessName;
				setting.QueueName = node.Attributes["queueName"].Value;
				if (!config.ConfigList.ContainsKey(setting.BusinessName))
				{
					config.ConfigList.Add(businessName, setting);
				}
			}
			return config;
		}
	}
}
