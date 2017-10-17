using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace BitAuto.CarDataUpdate.Config
{
	class NewsCategoryConfigHandler : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, System.Xml.XmlNode section)
		{
			if (section == null)
				return null;

			NewsCategoryConfig config = new NewsCategoryConfig();
			SetArry(config.CMSCreativeTypes, section, "CMSCreativeType/@TypeIds");
			SetArry(config.SerialFocusTopCategoryIds, section, "SerialFocusTop/@CategoryIds");
			SetArry(config.SerialFocusVideoCategoryIds, section, "SerialFocusVideo/@CategoryIds");
			SetCategoryShowName(section, config);
			return config;
		}

		private void SetCategoryShowName(XmlNode section, NewsCategoryConfig config)
		{
			XmlNodeList nodeList = section.SelectNodes("CategoryShowNames/Add");
			foreach (XmlNode node in nodeList)
			{
				if(node.Attributes["Key"]==null)
					continue;
				string key = node.Attributes["Key"].Value.Trim().ToLower();
				if (config.NewsCategoryShowNames.ContainsKey(key))
					continue;
				NewsCategoryShowName category=new NewsCategoryShowName();
				category.CategoryKey=key;
				category.CategoryShowName=node.Attributes["ShowName"].Value.Trim();
				category.CategoryUrl=node.Attributes["Url"].Value.Trim();
				config.NewsCategoryShowNames.Add(category.CategoryKey, category);
				if (category.CategoryKey == NewsCategoryConfig.QitaCategoryKey)
					continue;
				SetArry(category.CategoryIds, node, "@CategoryIds");
			}
		}
		private void SetArry(List<int> intList, XmlNode section, string xmlPath)
		{
			XmlNode node = section.SelectSingleNode(xmlPath);
			if (node != null && !string.IsNullOrEmpty(node.InnerText.Trim()))
			{
				SetArry(intList
					, node.InnerText.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			}
		}
		private void SetArry(List<int> intList, string[] stringList)
		{
			int id;
			foreach (string str in stringList)
			{
				if (int.TryParse(str, out id) && !intList.Contains(id))
				{
					intList.Add(id);
				}
			}
		}
	}
}
