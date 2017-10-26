using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class NewCarIntoMarket
    {
        private Dictionary<int, XmlDocument> carReallyPicDic = new Dictionary<int, XmlDocument>();
        private string fileName = Path.Combine(CommonData.CommonSettings.SavePath, @"SerialSet\NewCarIntoMarket.xml");

        public void GetNewCarIntoMarket()
        {
            Dictionary<int,SerialInfo> serialDic = CommonData.SerialDic;
            if (serialDic == null || serialDic.Count == 0)
                return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendFormat("<Root Date=\"{0}\">", DateTime.Now.ToString("yyyy.MM.dd"));
            foreach (KeyValuePair<int, SerialInfo> kv in serialDic)
            {
                string showTxt = GetSerialShowText(kv.Value);
                if (!string.IsNullOrEmpty(showTxt))
                {
                    sb.AppendFormat("<Item CsId=\"{0}\" ShowTxt=\"{1}\" />", kv.Key, showTxt);
                }
            }
            sb.Append("</Root>");
            CommonFunction.SaveFileContent(sb.ToString(), fileName, Encoding.UTF8);
        }

        /// <summary>
        /// 获取导航头状态标签显示文案
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="csid"></param>
        /// <returns></returns>
        private string GetSerialShowText(SerialInfo serial)
        {
            string showText = "";
            List<TimeTagEntity> carList = CommonNavigationService.GetAllCarBySerialId(serial.Id);
            //在售
            if (serial.CsSaleState == "在销" || serial.CsSaleState == "停销")
            {
                //95在销 96停销  97待销

                //筛选待销车款
                IEnumerable<TimeTagEntity> newCarList = carList.Where(i => i.CarSaleState == 97);
                //在销车系下有待销车款
                if (newCarList.Count() > 0)
                {
                    //待销车款中筛选填写了上市时间的待销车款
                    IEnumerable<TimeTagEntity> newCarMarketDateTimeList = newCarList.Where(a => DateTime.Compare(a.MarketDateTime, DateTime.MinValue) != 0);
                    //存在填写了上市时间的待销车款
                    if (newCarMarketDateTimeList.Count() > 0)
                    {
                        TimeTagEntity car = newCarMarketDateTimeList.First();//从已经填写的时间中选择最早的时间
                        //排除未上市车填写了过去的上市时间（这种情况属于数据错误，通过程序筛选控制）
                        if (DateTime.Compare(car.MarketDateTime, DateTime.Now) >= 0)
                        {
                            showText = "将于" + car.MarketDateTime.ToString("yy年MM月dd日") + "上市";
                        }
                    }
                    //没有填写上市时间
                    else
                    {
                        //判断车款是否有实拍图
                        int count = 0;
                        foreach (var item in newCarList)
                        {
                            XmlDocument xmlDoc = null;
                            if (carReallyPicDic.ContainsKey(item.CarId))
                            {
                                xmlDoc = carReallyPicDic[item.CarId];
                            }
                            else
                            {
                                xmlDoc = CommonFunction.GetLocalXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"PhotoImage\SerialCarReallyPic\{0}.xml", item.CarId)));
                                carReallyPicDic.Add(item.CarId, xmlDoc);
                            }
                            if (xmlDoc != null && xmlDoc.HasChildNodes)
                            {
                                XmlNode node = xmlDoc.SelectSingleNode("//Data//Total");
                                var countStr = node.InnerText;
                                int.TryParse(countStr, out count);
                                if (count > 0)
                                {
                                    showText = "新款即将上市";
                                    break;
                                }
                                else
                                {
                                    //是否有指导价
                                    if (item.ReferPrice != "")
                                    {
                                        showText = "新款即将上市";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                //新车款上市初期
                else
                {
                    IEnumerable<TimeTagEntity> newCarMarketDateTimeList = carList.Where(a => DateTime.Compare(a.MarketDateTime, DateTime.MinValue) != 0);
                    if (newCarMarketDateTimeList.Count() > 0)
                    {
                        TimeTagEntity car = newCarMarketDateTimeList.First();//倒叙排列，取第一个即可
                        if (car != null)
                        {
                            int days = GetDaysAboutCurrentDateTime(car.MarketDateTime);
                            if (days >= 0 && days <= 30)
                            {
                                //只有一个年款    ***新车上市***
                                if (carList.GroupBy(i => i.CarYearType).Count() == 1)
                                {
                                    showText = "新车上市";
                                }
                                //不止一个年款    ***新款上市***
                                else
                                {
                                    showText = "新款上市";
                                }
                            }
                        }
                    }
                }
            }
            //待查 待销(未上市)
            else
            {
                //筛选填写了上市时间的待销车
                IEnumerable<TimeTagEntity> newCarList = carList.Where(a => DateTime.Compare(a.MarketDateTime, DateTime.MinValue) != 0);
                //存在填写了上市时间的待销车
                if (newCarList.Count() > 0)
                {
                    TimeTagEntity car = carList.First();//从已经填写的时间中选择最早的时间
                    //排除未上市车填写了过去的上市时间（这种情况属于数据错误，通过程序筛选控制）
                    if (DateTime.Compare(car.MarketDateTime, DateTime.Now) >= 0)
                    {
                        showText = "将于" + car.MarketDateTime.ToString("yy年MM月dd日") + "上市";
                    }
                }
                //没有上市时间，判断有没有实拍图、指导价
                else
                {
                    //查找实拍图
                    int count = 0;
                    foreach (var item in carList)
                    {
                        XmlDocument xmlDoc = GetSerialCarRellyPicCount(item.CarId);
                        if (xmlDoc != null && xmlDoc.HasChildNodes)
                        {
                            XmlNode node = xmlDoc.SelectSingleNode("//Data//Total");
                            var countStr = node.InnerText;
                            int.TryParse(countStr, out count);
                            if (count > 0)
                            {
                                showText = "即将上市";
                                break;
                            }
                            //是否有指导价
                            else
                            {
                                //是否有指导价
                                if (item.ReferPrice != "")
                                {
                                    showText = "即将上市";
                                    break;
                                }
                            }
                        }
                    }

                }
            }

            return showText;
        }

        private string GetCarMarketFlag(DateTime marketDateTime,string carSaleState,string referPrice)
        {
            string marketflag = "";

            //if (entity != null)
            //{
                if (marketDateTime != DateTime.MinValue)
                {
                    int days = GetDaysAboutCurrentDateTime(marketDateTime);
                    if (days >= 0 && days <= 30)
                    {
                        if (carSaleState.Trim() == "在销")
                        {
                            marketflag = "<em class=\"the-new\">新上市</em>";
                        }
                    }
                    else if (days >= -30 && days < 0)
                    {
                        if (carSaleState == "待销")
                        {
                            marketflag = "<em class=\"the-new\">即将上市</em>";
                        }
                    }
                }
                else
                {
                    if (carSaleState.Trim() == "待销")
                    {
                    var picCount = 0;// carBLL.GetSerialCarRellyPicCount(entity.CarID);
                        if (picCount > 0)
                        {
                            marketflag = "<em class=\"the-new\">即将上市</em>";
                        }
                        else
                        {
                            if (referPrice != "")
                            {
                                marketflag = "<em class=\"the-new\">即将上市</em>";
                            }
                        }
                    }
                }
            //}
            return marketflag;
        }

        /// <summary>
        /// 获取车型图片文档
        /// </summary>
        /// <param name="carId"></param>
        /// <returns></returns>
        public XmlDocument GetSerialCarRellyPicCount(int carId)
        {
            XmlDocument xmlDoc = null;
            if (carReallyPicDic.ContainsKey(carId))
            {
                xmlDoc = carReallyPicDic[carId];
            }
            else
            {
                xmlDoc = CommonFunction.GetLocalXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"PhotoImage\SerialCarReallyPic\{0}.xml", carId)));
                carReallyPicDic.Add(carId, xmlDoc);
            }
            return xmlDoc;
        }

        /// <summary>
        /// 获取日期和当前时间的天数差
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int GetDaysAboutCurrentDateTime(DateTime dt)
        {
            DateTime currentDateTime = DateTime.Now;
            int days = (currentDateTime - dt).Days;
            return days;
        }
    }
}
