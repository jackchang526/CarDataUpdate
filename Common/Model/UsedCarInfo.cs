using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Utils;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class UsedCarInfo
    {
        /// <summary>
        /// 年份
        /// </summary>
        public string BuyCarDate { get; set; }

        /// <summary>
        /// 车源信息
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        ///  地区
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        ///  价格
        /// </summary>
        public string DisplayPrice { get; set; }

        /// <summary>
        /// 车源信息链接
        /// </summary>
        public string CarlistUrl { get; set; }

        /// <summary>
        /// 车源信息链接
        /// </summary>
        public string CityUrl { get; set; }

        public static UsedCarInfo ConvertToUsedCarInfo(XmlElement usedCarNode)
        {
            UsedCarInfo usedCarInfo = new UsedCarInfo();
            if (usedCarNode != null)
            {
                usedCarInfo.BuyCarDate = XmlUtils.GetChildNodeInnerText(usedCarNode,
                    "BuyCarDate");
                usedCarInfo.BrandName = XmlUtils.GetChildNodeInnerText(usedCarNode,
                    "BrandName");
                usedCarInfo.CityName = XmlUtils.GetChildNodeInnerText(usedCarNode,
                    "CityName");
                usedCarInfo.DisplayPrice = XmlUtils.GetChildNodeInnerText(usedCarNode,
                    "DisplayPrice");
                usedCarInfo.CarlistUrl = XmlUtils.GetChildNodeInnerText(usedCarNode,
                    "CarlistUrl");
                usedCarInfo.CityUrl = XmlUtils.GetChildNodeInnerText(usedCarNode,
                    "CityUrL");
            }
            return usedCarInfo;
        }

        public static UsedCarInfo[] ConvertToUsedCarInfos(XmlNode dataSetNode)
        {
            XmlNodeList itemNodes = dataSetNode.SelectNodes("./item");
            UsedCarInfo[] usedCarInfos = new UsedCarInfo[itemNodes.Count];
            for (int i = 0; i < itemNodes.Count; i++)
            {
                usedCarInfos[i] = ConvertToUsedCarInfo((XmlElement)itemNodes[i]);
            }
            return usedCarInfos;
        }
    }

}
