using BitAuto.CarDataUpdate.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Xml;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 车系销售排行
    /// </summary>
    public class SerialSaleRank
    {
        private string SerialSaleRankUrl = string.Empty;
        private string FileName = string.Empty;

        public SerialSaleRank()
        {
            DateTime saleMonth = DateTime.Now.AddMonths(-2);
            SerialSaleRankUrl = CommonData.CommonSettings.SerialSaleRankUrl.Replace("{year}", saleMonth.Year.ToString()).Replace("{month}", saleMonth.Month.ToString());

            FileName = string.Concat(CommonData.CommonSettings.SavePath, "\\SerialSet\\SerialSaleRank.xml");
        }

        /// <summary>
        /// 销量排行
        /// </summary>
        public void GetSaleRank()
        {
            if (string.IsNullOrEmpty(SerialSaleRankUrl))
            {
                Log.WriteErrorLog("车系销量排行接口为空；");
                return;
            }
            string jsonArray = CommonFunction.GetContentByUrl(SerialSaleRankUrl, "utf-8");
            if (string.IsNullOrEmpty(jsonArray))
            {
                Log.WriteErrorLog("车系销量数据为空；");
                return;
            }
            List<SerialSaleCount> serialSaleList = JsonConvert.DeserializeObject<List<SerialSaleCount>>(jsonArray);
            Dictionary<int, Common.Model.SerialInfo> SerialDic = CommonData.SerialDic;//车系基本信息
            Dictionary<int, string> csPriceRange = CommonData.CsPriceRangeDic;//车系报价区间
            Dictionary<int, string> serialLevelDic = CommonData.SerialLevelDic;//车系级别
            Dictionary<int, string> dicSerialNewPhoto = CommonData.dicSerialNewPhoto;//车系白底图
            if (serialSaleList.Count > 0)
            {
                serialSaleList.Sort((x,y)=>y.SellNum - x.SellNum);
                foreach (SerialSaleCount ssc in serialSaleList)
                {
                    if (!SerialDic.ContainsKey(ssc.CsId))
                    {
                        continue;
                    }
                    Common.Model.SerialInfo serialInfo = SerialDic[ssc.CsId];
                    ssc.CsShowName = serialInfo.ShowName;

                    string serialPrice = "暂无报价";
                    if (serialInfo.CsSaleState.Trim() == "停销")
                    { serialPrice = "停售"; }
                    else if (serialInfo.CsSaleState.Trim() == "待销")
                    { serialPrice = "未上市"; }
                    else
                    { serialPrice = csPriceRange.ContainsKey(ssc.CsId) ? csPriceRange[ssc.CsId] : "暂无报价"; }
                    if (serialPrice.IndexOf("万-") > -0)
                    {
                        serialPrice = serialPrice.Replace("万-", "-");
                    }

                    ssc.PriceRange = serialPrice;

                    ssc.Level = serialLevelDic.ContainsKey(ssc.CsId) ? serialLevelDic[ssc.CsId] : string.Empty;

                    ssc.ImgUrl = dicSerialNewPhoto.ContainsKey(ssc.CsId) ? dicSerialNewPhoto[ssc.CsId] : string.Empty;
                }
            }
            SaveDocument(serialSaleList);
        }

        /// <summary>
        /// 保存文档
        /// </summary>
        /// <param name="list"></param>
        private void SaveDocument(List<SerialSaleCount> list)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(root);
            XmlDeclaration declarEle = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            xmlDoc.InsertBefore(declarEle, root);

            foreach (SerialSaleCount serial in list)
            {
                XmlElement xmlEle = xmlDoc.CreateElement("Item");
                xmlEle.SetAttribute("CsId", serial.CsId.ToString());
                xmlEle.SetAttribute("ShowName", serial.CsShowName);
                xmlEle.SetAttribute("SellNum", serial.SellNum.ToString());
                xmlEle.SetAttribute("ProduceNum", serial.ProduceNum.ToString());
                xmlEle.SetAttribute("PriceRange", serial.PriceRange);
                xmlEle.SetAttribute("Level", serial.Level);
                xmlEle.SetAttribute("ImgUrl", serial.ImgUrl);
                root.AppendChild(xmlEle);
            }
            CommonFunction.SaveXMLDocument(xmlDoc,FileName);
        }
    }

    /// <summary>
    /// 车系销量类
    /// </summary>
    public class SerialSaleCount
    {
        /// <summary>
        /// 车系id
        /// </summary>
        public int CsId { get; set; }
        /// <summary>
        /// 产量
        /// </summary>
        public int ProduceNum { get; set; }
        /// <summary>
        /// 销量
        /// </summary>
        public int SellNum { get; set; }
        /// <summary>
        /// 车系显示名称
        /// </summary>
        public string CsShowName { get; set; }
        /// <summary>
        /// 车系报价
        /// </summary>
        public string PriceRange { get; set; }

        /// <summary>
        /// 车系级别
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 白底图
        /// </summary>
        public string ImgUrl { get; set; }
    }
}
