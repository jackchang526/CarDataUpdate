using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Config
{
	public class VideoCategoryConfigHandler : IConfigurationSectionHandler
	{

		public object Create(object parent, object configContext, System.Xml.XmlNode section)
		{
			VideoCategoryConfig config = new VideoCategoryConfig();
			XmlNodeList nodeList = section.SelectNodes("/VideoCategoryConfig/add");
			foreach (XmlNode node in nodeList)
			{
				string key = node.Attributes["key"].Value;
				VideoCategoryConfigSetting setting = new VideoCategoryConfigSetting();
				setting.Key = key;
				setting.Name = node.Attributes["name"].Value;
				setting.CategoryIds = node.Attributes["categoryIds"].Value;
				setting.CategoryType = ConvertHelper.GetInteger(node.Attributes["categoryType"].Value);
				if (!config.ConfigList.ContainsKey(setting.Key))
				{
					config.ConfigList.Add(key, setting);
				}
			}
			return config;
		}
	}
}
