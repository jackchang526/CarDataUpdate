using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Config
{
    public class CarNewsTypeSettingsHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            if (section == null)
                return null;

            int typeId;
            CarNewsTypeItem typeItem;
            CarNewsTypeSettings settings = new CarNewsTypeSettings();

            XmlNodeList carNewsTypeList = section.SelectNodes("/CarNewsTypeSettings/CarNewsType");
            XmlNodeList builderList;
            XmlNodeList keyValueList;
            settings.CarNewsTypeList = new Dictionary<int, CarNewsTypeItem>(carNewsTypeList.Count);

            foreach (XmlNode typeNode in carNewsTypeList)
            {
                typeId = ConvertHelper.GetInteger(typeNode.Attributes["Id"].Value);
                if (typeId > 0 && !settings.CarNewsTypeList.ContainsKey(typeId))
                {
                    typeItem = new CarNewsTypeItem()
                    {
                        Id = typeId,
                        Name = typeNode.Attributes["Name"].Value,
                        TypeStr = typeNode.Attributes["TypeStr"].Value,
                        Description = typeNode.Attributes["Description"].Value,
                        CategoryIds = typeNode.Attributes["CategoryIds"].Value,
                        CarTypes = (CarTypes)ConvertHelper.GetInteger(typeNode.Attributes["CarTypes"].Value)
                    };

                    builderList = typeNode.SelectNodes("BuilderCollection/Builder");
                    typeItem.Builders = new CarNewsTypeBuilder[builderList.Count];
                    int i=0;
                    foreach (XmlNode builder in builderList)
                    {
                        typeItem.Builders[i] = new CarNewsTypeBuilder() { Type = builder.Attributes["Type"].Value };
                        i++;
                    }
                    keyValueList = typeNode.SelectNodes("KeyValueCollection/Item");
                    typeItem.KeyValueDic = new Dictionary<string, string>(keyValueList.Count);
                    foreach (XmlNode keyValue in keyValueList)
                    {
                        typeItem.KeyValueDic.Add(keyValue.Attributes["Key"].Value, keyValue.Attributes["Value"].Value);
                    }
                    settings.CarNewsTypeList.Add(typeId, typeItem);
                }
            }
            return settings;
        }
    }
}
