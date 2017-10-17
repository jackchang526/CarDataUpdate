using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;
using System.IO;
using System.Xml;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class MaiCheSite
    {
        #region  del by lsf 2016-01-06
        /*
        public event LogHandler Log;
        private string _RootPath = string.Empty;

        private string carPrice = "http://price.bitauto.com/interface/common/Handler.ashx?op=GetCarPriceCount&interfaceid=1";
        private string carDealer = "http://price.bitauto.com/interface/common/Handler.ashx?op=GetDealerCount&interfaceid=1";
        private string uCarTotal = "http://api.ucar.cn/CarBasicIno/ForJson/GetUcarCarCount.ashx";

        /// <summary>
        /// 构造函数
        /// </summary>
        public MaiCheSite()
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "MaiCheDataTotal.xml");
        }

        /// <summary>
        /// 得到内容
        /// </summary>
        public void GetContent()
        {
            //得到总量的XML
            BuildTotalXml(0);
        }

        /// <summary>
        /// 得到结果
        /// </summary>
        public void BuildTotalXml(int id)
        {
            OnLog("		Run Price,Dealer,Ucar Data Start...", true);
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("root");
                xmlDoc.AppendChild(rootNode);
                XmlDeclaration xd = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.InsertBefore(xd, rootNode);

                //得到报价
                string car = CommonFunction.GetContentByUrl(carPrice, "utf-8");
                if (!string.IsNullOrEmpty(car))
                {
                    XmlElement elem = xmlDoc.CreateElement("element");
                    elem.SetAttribute("type", "car");
                    elem.SetAttribute("number", ConvertHelper.GetInteger(car.Trim().Replace(",", "")).ToString());
                    rootNode.AppendChild(elem);
                }
                //得到经销商
                car = CommonFunction.GetContentByUrl(carDealer, "utf-8");
                if (!string.IsNullOrEmpty(car))
                {
                    XmlElement elem = xmlDoc.CreateElement("element");
                    elem.SetAttribute("type", "dealer");
                    elem.SetAttribute("number", ConvertHelper.GetInteger(car.Trim().Replace(",", "")).ToString());
                    rootNode.AppendChild(elem);
                }
                //得到二手车
                car = CommonFunction.GetContentByUrl(uCarTotal, "utf-8");
                if (!string.IsNullOrEmpty(car))
                {
                    XmlElement elem = xmlDoc.CreateElement("element");
                    elem.SetAttribute("type", "ucar");
                    elem.SetAttribute("number", ConvertHelper.GetInteger(car.Trim().Replace(",", "")).ToString());
                    rootNode.AppendChild(elem);
                }
                CommonFunction.SaveXMLDocument(xmlDoc, _RootPath);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message, true);
            }
            finally
            {
                OnLog("		Run Price,Dealer,Ucar Data End...", true);
            }
        }

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="logText"></param>
        public void OnLog(string logText, bool nextLine)
        {
            if (Log != null)
                Log(this, new LogArgs(logText, nextLine));
        }
         * */
        #endregion
        
    }
}
