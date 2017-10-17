using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.HtmlBuilder.com.bitauto.baa.api;
using BitAuto.Utils;
using Newtonsoft.Json;
using System.Threading;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 移动端车款综述页口碑
    /// </summary>
    public class CarSummaryKouBei
    {
        //private readonly com.bitauto.baa.api.userCarService _userCarService;
        private string KoubeiDetailUrl = CommonData.CommonSettings.KoubeiDetailUrl;
        public CarSummaryKouBei()
        {
            //_userCarService = new com.bitauto.baa.api.userCarService()
            //{
            //	ApiSoapHeaderValue = new com.bitauto.baa.api.ApiSoapHeader()
            //	{
            //		AppKey = "100049",
            //		AppPwd = "EAB934E5-6021-48DF-BA69-B063FCFEA72B"
            //	}
            //};
        }

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string GetUserImage(int userId)
        {
            string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
            try
            {
                using (var client = new HtmlBuilder.DataCenter.WCF.DataProvideClient())
                {
                    var userFieldInfo = client.GetUserData(new int[] { userId }, new string[] { "Avatar60" });
                    userImage = userFieldInfo[userId]["avatar60"].ToString();
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("获取用户头像异常userid:" + userId + "\r\n" + ex.ToString());
            }
            return userImage;
        }

        public void Generate(int csId, int carId)
        {
            string fileName = Path.Combine(CommonData.CommonSettings.SavePath, @"Koubei\CarTopicList\" + string.Format("{0}.xml", carId));
            string result = string.Empty;
            bool hasTopicList = false;
            try
            {
                //int count;
                string url = KoubeiDetailUrl.Replace("{csid}", csId.ToString()).Replace("{carid}", carId.ToString()).Replace("{cpy}", string.Empty).Replace("{wn}", "").Replace("{page}", "1").Replace("{size}", "10");
                //result = _userCarService.GetUserCarTopicList(csId, carId, "", "", 1, 10, out count);

                //if (!string.IsNullOrEmpty(url))
                //{
                var xmlDoc = new XmlDocument();
                //xmlDoc.LoadXml(result);
                xmlDoc.Load(url);
                XmlNodeList userItems = xmlDoc.SelectNodes("UserCarList/Item");
                if (userItems != null && userItems.Count > 0)
                {
                    var xmlOutPutDoc = new XDocument();//提取所要数据后保存的XML
                    var userCarList = new XElement("UserCarList");
                    xmlOutPutDoc.Add(userCarList);
                    foreach (XmlNode node in userItems)
                    {
                        var itemElement = new XElement("Item");
                        userCarList.Add(itemElement);
                        var userNameElement = new XElement("UserName", node.SelectSingleNode("./UserName").InnerText);
                        itemElement.Add(userNameElement);
                        var userId = ConvertHelper.GetInteger(node.SelectSingleNode("./UserId").InnerText);
                        string userImage = "http://pic.baa.bitautotech.com/newavatar/60.jpg";
                        string userUrl = "#";
                        if (userId > 0)
                        {
                            userImage = GetUserImage(userId);
                            userUrl = "http://i.m.yiche.com/u" + userId + "/";
                        }
                        var userImageElement = new XElement("UserImage", userImage);
                        itemElement.Add(userImageElement);
                        var userUrlElement = new XElement("UserUrl", userUrl);
                        itemElement.Add(userUrlElement);

                        var topicListItems = node.SelectNodes("./TopicList/Item");
                        if (topicListItems != null && topicListItems.Count > 0)
                        {
                            hasTopicList = true;
                            var topicListElement = new XElement("TopicList");
                            itemElement.Add(topicListElement);
                            foreach (XmlNode topicListItem in topicListItems)
                            {
                                var itemEl = new XElement("Item");
                                topicListElement.Add(itemEl);
                                //行驶公里数
                                var topicIdElement = new XElement("TopicId", topicListItem.SelectSingleNode("./TopicId").InnerText);
                                itemEl.Add(topicIdElement);
                                //行驶公里数
                                var mileageElement = new XElement("Mileage", topicListItem.SelectSingleNode("./Mileage").InnerText);
                                itemEl.Add(mileageElement);
                                //发口碑时间
                                var cTime = DateTime.MinValue;
                                DateTime.TryParse(topicListItem.SelectSingleNode("./CreateTime").InnerText, out cTime);
                                var createTimeElement = new XElement("CreateTime", cTime.ToString("yyyy-MM-dd"));
                                itemEl.Add(createTimeElement);
                                //实测油耗
                                var fuelElement = new XElement("Fuel", topicListItem.SelectSingleNode("./Fuel").InnerText);
                                itemEl.Add(fuelElement);

                                var titleElement = new XElement("Title", topicListItem.SelectSingleNode("./Title").InnerText);
                                itemEl.Add(titleElement);

                                var contentElement = new XElement("Content", topicListItem.SelectSingleNode("./Content").InnerText);
                                itemEl.Add(contentElement);
                            }
                        }
                    }
                    if (hasTopicList)
                    {
                        CommonFunction.SaveFileContent(xmlOutPutDoc.ToString(), fileName, Encoding.UTF8);
                    }
                    else
                    {
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                        Log.WriteLog(string.Format("车款口碑为空，删除操作，csid={0},carid={1},result={2}\r\n", csId, carId, result));
                    }
                }
                else
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    Log.WriteLog(string.Format("车款口碑为空，删除操作，csid={0},carid={1},result={2}\r\n", csId, carId, result));
                }
                //}
                //Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(string.Format("Msg:生成车款口碑异常，csid={0},carid={1},result={2}\r\n,StackTrace:{3}\r\n", csId, carId, result, ex.ToString()));
            }
        }
    }
}
