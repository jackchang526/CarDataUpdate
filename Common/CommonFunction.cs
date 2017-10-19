using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Utils;
using System.Configuration;
using System.Messaging;
using BitAuto.Services.Cache;
using System.Xml.Linq;
using Newtonsoft.Json;
using ServiceStack.Text;

namespace BitAuto.CarDataUpdate.Common
{
    public static class CommonFunction
    {

        #region xml

        /// <summary>
        /// 获取指定位置xml对象，不会判断文件是否存在
        /// </summary>
        public static XmlDocument GetXmlDocument(string filepath, bool ignoreLog = false)
        {
            XmlDocument result = null;
            XmlReader reader = null;
            XmlDocument newsDoc = null;
            try
            {
                reader = XmlReader.Create(filepath);
                newsDoc = new XmlDocument();
                newsDoc.Load(reader);
                result = newsDoc;
            }
            catch (WebException wex)
            {
                HttpWebResponse response = (HttpWebResponse) wex.Response;
                if (response!=null)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound && ignoreLog)
                    {
                        //忽略
                    }
                    else
                    {
                        Log.WriteErrorLog("获取Url异常:" + filepath + "\r\n" + wex.ToString());
                    }
                }
            }
            catch (System.Exception ex)
            {
                //此处记录日志
                Log.WriteErrorLog("获取Url异常:" + filepath + "\r\n" + ex.ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return result;
        }
        /// <summary>
        /// 获取指定位置xml对象，会判断文件是否存在
        /// </summary>
        public static XmlDocument GetLocalXmlDocument(string filepath)
        {
            XmlDocument result = null;
            if (File.Exists(filepath))
                result = GetXmlDocument(filepath);
            else
                Log.WriteErrorLog("not find file:" + filepath);
            return result;
        }
        /// <summary>
        /// 根据xpath获取xmldoc中的值
        /// </summary>
        public static string GetXmlElementInnerText(XmlDocument doc, string xPath, string defaultValue)
        {
            if (doc == null || string.IsNullOrEmpty(xPath))
                return defaultValue;

            XmlNode node = doc.SelectSingleNode(xPath);
            if (node == null || string.IsNullOrEmpty(node.InnerText.Trim()))
                return defaultValue;

            return node.InnerText.Trim();
        }
        /// <summary>
        /// 根据xpath获取xmldoc中的值
        /// </summary>
        public static string GetXmlElementInnerText(XmlElement xmlNode, string xPath, string defaultValue)
        {
            if (xmlNode == null || string.IsNullOrEmpty(xPath))
                return defaultValue;

            XmlNode node = xmlNode.SelectSingleNode(xPath);
            if (node == null || string.IsNullOrEmpty(node.InnerText.Trim()))
                return defaultValue;

            return node.InnerText.Trim();
        }

        /// <summary>
        /// 保存XML文件
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="filePath"></param>
        public static void SaveXMLDocument(XmlDocument xmlDoc, string filePath)
        {
            if (xmlDoc == null || string.IsNullOrEmpty(filePath)) return;

            string path = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            int counter = 0;

            Exception err = null;
            while (counter < 5)
            {
                counter++;
                try
                {
                    xmlDoc.Save(filePath);
                    err = null;
                    break;
                }
                catch (System.Exception ex)
                {
                    err = ex;
                    Thread.Sleep(500);
                }
            }
            if (err != null)
                Log.WriteErrorLog(err);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        public static void SaveXMLDocument(string url, string filePath)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filePath)) return;
            XmlDocument xmlDoc = GetXmlDocument(url);
            if (xmlDoc == null) return;
            SaveXMLDocument(xmlDoc, filePath);

        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        public static void SaveXMLDocument(string url, string filePath, int RoundNumber, out int NewsNumber)
        {
            NewsNumber = 0;
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filePath)) return;
            XmlDocument xmlDoc = GetXmlDocument(url);
            if (xmlDoc == null) return;

            XmlNode xNode = xmlDoc.SelectSingleNode("NewDataSet/newsAllCount/allcount");
            if (xNode == null || string.IsNullOrEmpty(xNode.InnerText)) NewsNumber = 0;
            else
            {
                NewsNumber = ConvertHelper.GetInteger(xNode.InnerText);
                if (NewsNumber > RoundNumber) NewsNumber = RoundNumber;
            }

            SaveXMLDocument(xmlDoc, filePath);

        }

        /// <summary>
        /// 根据节点的层级取节点内容 add by chengl Mar.10.2014
        /// </summary>
        /// <param name="rootElement">xml块根节点，可以为任何层级</param>
        /// <param name="elementPath">需求节点路径，最后元素为需求节点名</param>
        /// <returns></returns>
        public static string GetXElementByNamePath(XElement rootElement, string[] elementPath)
        {
            if (rootElement != null)
            {
                var ele = rootElement;
                if (elementPath != null && elementPath.Length > 0)
                {
                    foreach (string elementName in elementPath)
                    {
                        if (!string.IsNullOrEmpty(elementName) && ele != null)
                        {
                            ele = ele.Element(elementName.Trim());
                        }
                    }
                }

                if (ele != null)
                {
                    return ele.Value;
                }
            }
            return "";
        }

        #endregion

        /// <summary>
        /// 记录新闻数量
        /// </summary>
        /// <param name="masterCount">主品牌数量数据</param>
        /// <param name="filePath">文件保存地址</param>
        /// <returns></returns>
        public static void RecordNewsNumber(Dictionary<int, int> masterCount, string filePath, string type, string ElementName)
        {
            if (masterCount == null || masterCount.Count < 1) return;
            XmlDocument xmlDoc = new XmlDocument();
            bool IsExits = true;
            if (File.Exists(filePath)) xmlDoc.Load(filePath);
            if (xmlDoc == null || xmlDoc.SelectSingleNode("root") == null) IsExits = false;
            XmlElement xElem;
            if (IsExits) { xElem = (XmlElement)xmlDoc.SelectSingleNode("root"); }
            else { xElem = xmlDoc.CreateElement("root"); }

            if (!IsExits)
            {
                foreach (KeyValuePair<int, int> entity in masterCount)
                {
                    XmlElement xMasterElem;
                    xMasterElem = xmlDoc.CreateElement(ElementName);
                    xMasterElem.SetAttribute("ID", entity.Key.ToString());
                    xMasterElem.SetAttribute(type, entity.Value.ToString());
                    xElem.AppendChild(xMasterElem);
                }
            }
            else
            {
                foreach (KeyValuePair<int, int> entity in masterCount)
                {
                    XmlNode xMasterNode = xElem.SelectSingleNode(ElementName + "[@ID=" + entity.Key + "]");
                    XmlElement xMasterElem;
                    if (xMasterNode == null)
                    {
                        xMasterElem = xmlDoc.CreateElement(ElementName);
                        xMasterElem.SetAttribute("ID", entity.Key.ToString());
                        xMasterElem.SetAttribute(type, entity.Value.ToString());
                        xElem.AppendChild(xMasterElem);
                    }
                    else
                    {
                        xMasterElem = (XmlElement)xMasterNode;
                        xMasterElem.SetAttribute("ID", entity.Key.ToString());
                        xMasterElem.SetAttribute(type, entity.Value.ToString());
                    }
                }
            }

            if (!IsExits && !Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (!IsExits)
            {
                xmlDoc.AppendChild(xElem);
            }

            SaveXMLDocument(xmlDoc, filePath);
        }

        /// <summary>
        /// 创建指定的目录
        /// 如果存在此目录跳过
        /// </summary>
        /// <param name="fullPath">目录全路径</param>
        /// <returns>目录信息</returns>
        public static DirectoryInfo CreateDirecotry(string fullPath)
        {
            return !Directory.Exists(fullPath) ?
                Directory.CreateDirectory(fullPath) : new DirectoryInfo(fullPath);
        }

        /// <summary>
        /// 保存XmlDocumnet到文件中
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="xmlFile"></param>
        /// <param name="backupFile"></param>
        public static void SaveAndBackupNews(XmlDocument xmlDoc, string xmlFile, string backupFile)
        {
            try
            {
                string backPath = Path.GetDirectoryName(backupFile);
                if (!Directory.Exists(backPath))
                    Directory.CreateDirectory(backPath);
                if (File.Exists(xmlFile))
                    File.Copy(xmlFile, backupFile, true);
            }
            catch { }

            int counter = 0;

            Exception err = null;
            while (counter < 5)
            {
                counter++;
                try
                {
                    xmlDoc.Save(xmlFile);
                    err = null;
                    break;
                }
                catch (System.Exception ex)
                {
                    err = ex;
                    Thread.Sleep(500);
                }
            }
            if (err != null)
                Log.WriteErrorLog(err.Message);
        }

        #region 原来的common文件
        private static Dictionary<int, NewsCategory> m_newCategorys = new Dictionary<int, NewsCategory>();
        private static XmlDocument m_newsCateDoc = new XmlDocument();
        /// <summary>
        /// 添加字符串数组
        /// </summary>
        /// <param name="isJoinString"></param>
        /// <returns></returns>
        public static string joinStringArray(int[] isJoinString)
        {
            if (isJoinString == null || isJoinString.Length < 1)
            {
                return "";
            }

            StringBuilder joinString = new StringBuilder();
            foreach (int entity in isJoinString)
            {
                joinString.Append(",");
                joinString.Append(entity.ToString());
            }
            joinString.Remove(0, 1);
            return joinString.ToString();
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
        /// <summary>
        /// 获取每个主品牌包括品牌的列表字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, List<int>> GetMasterBrandDic()
        {
            Dictionary<int, List<int>> masterDic = new Dictionary<int, List<int>>();
            string autoDataFile = Path.Combine(Common.CommonData.CommonSettings.SavePath, "autodata.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(autoDataFile);

            //生成主品牌，品牌，子品牌列表
            XmlNodeList masterNodeList = xmlDoc.SelectNodes("/Params/MasterBrand");
            foreach (XmlElement masterNode in masterNodeList)
            {
                int masterId = Convert.ToInt32(masterNode.GetAttribute("ID"));
                masterDic[masterId] = new List<int>();
                XmlNodeList brandNodeList = masterNode.SelectNodes("Brand");
                foreach (XmlElement brandNode in brandNodeList)
                {
                    int brandId = Convert.ToInt32(brandNode.GetAttribute("ID"));
                    masterDic[masterId].Add(brandId);
                }
            }
            return masterDic;
        }

        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
        public static List<int> GetBrandList()
        {
            List<int> brandList = new List<int>();
            string autoDataFile = Path.Combine(Common.CommonData.CommonSettings.SavePath, "autodata.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(autoDataFile);

            XmlNodeList brandNodeList = xmlDoc.SelectNodes("/Params/MasterBrand/Brand");
            foreach (XmlElement brandNode in brandNodeList)
            {
                int brandId = Convert.ToInt32(brandNode.GetAttribute("ID"));
                brandList.Add(brandId);
            }
            return brandList;
        }
        /// <summary>
        /// 得到所有的车型数据XML
        /// </summary>
        /// <returns></returns>
        public static XmlDocument GetAllCarTypeData()
        {
            string autoDataFile = Path.Combine(Common.CommonData.CommonSettings.SavePath, "autodata.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(autoDataFile);
            return xmlDoc;
        }
        /// <summary>
        /// 获取子品牌ID列表
        /// </summary>
        /// <returns></returns>
        public static List<int> GetSerialList()
        {
            return CommonData.SerialDic.Keys.ToList();
        }

        /// <summary>
        /// 获取车款ID列表
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, CarEntity> GetCarList()
        {
            return CommonData.GetAllCarData();
        }

        /// <summary>
        /// 获取车款ID列表
        /// </summary>
        /// <returns></returns>
        public static CarEntity GetCarEntity(int carId)
        {
            return CommonData.GetCarDataById(carId);
        }

        /// <summary>
        /// 生成根据CategoryID查询新闻的表达式
        /// </summary>
        /// <param name="cateIds"></param>
        /// <returns></returns>
        public static string GetCategoryXmlPath(int[] cateIds)
        {
            string xmlPath = "";
            foreach (int cateId in cateIds)
            {
                if (xmlPath.Length == 0)
                    xmlPath = "contains(CategoryPath,\"," + cateId + ",\")";
                else
                    xmlPath += "or contains(CategoryPath,\"," + cateId + ",\")";
            }
            return xmlPath;
        }
        /// <summary>
        /// 为XmlDocument加上新闻的分类ID详细信息
        /// </summary>
        /// <param name="xmlDoc"></param>
        public static void AppendNewsInfoToDocument(XmlDocument xmlDoc)
        {
            XmlNodeList newsList = xmlDoc.SelectNodes("/NewDataSet/listNews");
            foreach (XmlElement newsNode in newsList)
                AppendNewsInfo(newsNode);
        }
        /// <summary>
        /// 给新闻内容加入根分类ID及分类路径信息
        /// </summary>
        /// <param name="newsNode"></param>
        public static void AppendNewsInfo(XmlElement newsNode)
        {
            int cateId = 0;
            try
            {
                if (m_newCategorys == null || m_newCategorys.Count == 0)
                    GetNewsCategorys();
                cateId = Convert.ToInt32(newsNode.SelectSingleNode("categoryId").InnerText);
                if (m_newCategorys.ContainsKey(cateId))
                {
                    //加入根分类ID
                    XmlElement rootIdEle = newsNode.OwnerDocument.CreateElement("RootCategoryId");
                    newsNode.AppendChild(rootIdEle);
                    rootIdEle.InnerText = m_newCategorys[cateId].RootCategoryId.ToString();

                    //加入分类路径
                    XmlElement pathEle = newsNode.OwnerDocument.CreateElement("CategoryPath");
                    newsNode.AppendChild(pathEle);
                    pathEle.InnerText = m_newCategorys[cateId].CategoryPath;
                }
            }
            catch (System.Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
        /// <summary>
        /// 获取新闻分类级别列表
        /// </summary>
        public static void GetNewsCategorys()
        {
            CommonSettings m_config = Common.CommonData.CommonSettings;
            string xmlUrl = m_config.NewsUrl + "?showcategory=1";
            try
            {
                m_newsCateDoc.Load(xmlUrl);
                XmlNodeList cateList = m_newsCateDoc.SelectNodes("/NewDataSet/NewsCategory");
                foreach (XmlElement cateNode in cateList)
                {
                    //分析分类ID，路径及根ID，并加入分类字典
                    int cateId = Convert.ToInt32(cateNode.SelectSingleNode("newscategoryid").InnerText);
                    string catePath = cateNode.SelectSingleNode("newscategoryidpath").InnerText;
                    NewsCategory newsCate = new NewsCategory(cateId);
                    newsCate.CategoryPath = catePath;
                    m_newCategorys[cateId] = newsCate;
                }
            }
            catch (System.Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }

        }
        /// <summary>
        /// 得到Hash后的图片域名
        /// </summary>
        /// <param name="imgId"></param>
        /// <returns></returns>
        public static string GetPublishHashImageDomain(int imgId)
        {
            if (imgId < 1) return "";
            string domainName = "http://img" + (imgId % 4 + 1).ToString() + ".bitautoimg.com/autoalbum/";
            return domainName;
        }
        /// <summary>
        /// 得到子品牌年款信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, List<int>> GetSerialRelationYear()
        {
            string sql = "select cs_id,carYear from car_serialyear where isstate = 0";

            try
            {
                using (DataSet ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(Common.CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql))
                {
                    if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
                    {
                        return null;
                    }

                    Dictionary<int, List<int>> sryList = new Dictionary<int, List<int>>();

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int serialId = ConvertHelper.GetInteger(dr["cs_id"].ToString());
                        int year = ConvertHelper.GetInteger(dr["carYear"].ToString());
                        if (sryList.ContainsKey(serialId) && !sryList[serialId].Contains(year))
                        {
                            sryList[serialId].Add(year);
                        }
                        else if (!sryList.ContainsKey(serialId))
                        {
                            sryList.Add(serialId, new List<int>());
                            sryList[serialId].Add(year);
                        }
                    }
                    return sryList;
                }
            }
            catch (System.Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 通过Url地址得到内容信息
        /// </summary>
        /// <param name="url">地址</param>
        /// <returns>内容字符串:"":表示出现异常或未取得数据</returns>
        public static string GetContentByUrl(string url, string encodetype)
        {
            string content = string.Empty;
            try
            {
                WebClient wc = new WebClient();
                content = wc.DownloadString(url);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("error GetContentByURL:" + url + "异常\r\n" + ex.ToString());
            }
            return content;
        }

        /// <summary>
        /// 保存文件内容
        /// </summary>
        /// <param name="content"></param>
        public static void SaveFileContent(string content, string filepath, string encoding)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(filepath)) return;

            string path = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            int counter = 0;

            Exception err = null;
            while (counter < 5)
            {
                counter++;
                try
                {
                    File.WriteAllText(filepath, content, Encoding.GetEncoding(encoding));
                    err = null;
                    break;
                }
                catch (System.Exception ex)
                {
                    err = ex;
                    Thread.Sleep(500);
                }
            }
            if (err != null)
                Log.WriteErrorLog(err.Message);

        }

        /// <summary>
        /// 保存文件内容
        /// add by chengl Feb.4.2013
        /// </summary>
        /// <param name="content"></param>
        public static void SaveFileContent(string content, List<string> filepathArray, string fileName, Encoding encoding)
        {
            if (filepathArray != null && filepathArray.Count > 0)
            {
                foreach (string filepath in filepathArray)
                {
                    SaveFileContent(content, filepath + fileName, encoding);
                }
            }
        }
        /// <summary>
        /// 保存文件内容
        /// </summary>
        /// <param name="content"></param>
        public static void SaveFileContent(string content, string filepath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(filepath)) return;

            string path = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            int counter = 0;

            Exception err = null;
            while (counter < 5)
            {
                counter++;
                try
                {
                    File.WriteAllText(filepath, content, encoding);
                    err = null;
                    break;
                }
                catch (System.Exception ex)
                {
                    err = ex;
                    Thread.Sleep(500);
                }
            }
            if (err != null)
                Log.WriteErrorLog(err.Message);
        }
        /// <summary>
        /// 得到城市字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetCityIdDic()
        {
            XmlDocument cityDoc = new XmlDocument();
            string cityFile = Path.Combine(CommonData.CommonSettings.SavePath, "city.xml");
            cityDoc.Load(cityFile);

            XmlNodeList cityList = cityDoc.SelectNodes("/CityValueSet/CityItem");
            Dictionary<int, int> cityDic = new Dictionary<int, int>();
            //城市列表
            foreach (XmlElement cityNode in cityList)
            {
                int cityId = Convert.ToInt32(cityNode.SelectSingleNode("CityId").InnerText);
                if (cityDic.ContainsKey(cityId)) continue;
                cityDic.Add(cityId, 0);
            }
            return cityDic;
        }
        #endregion

        #region 原News类方法
        /// <summary>
        /// 得到总部编辑名称列表
        /// </summary>
        /// <param name="Type">0:代表外地编辑;1:代表总部编辑</param>
        public static string[] GetEditerByType()
        {
            string Url = CommonData.CommonSettings.EidtorUserUrl;

            XmlDocument xmlDoc = GetXmlDocument(Url);
            if (xmlDoc == null) return null;
            XmlNodeList xNodelist = xmlDoc.SelectNodes("Root/User/UserName");
            if (xNodelist == null || xNodelist.Count < 1) return null;

            List<string> editerlist = new List<string>();
            for (int i = 0; i < xNodelist.Count; i++)
            {
                editerlist.Add(xNodelist[i].InnerText.Trim());
            }
            return editerlist.ToArray();
        }
        /// <summary>
        /// 获取新闻分类级别列表
        /// </summary>
        public static Dictionary<int, NewsCategory> GetNewsCategorysFromNews()
        {
            string xmlUrl = CommonData.CommonSettings.NewsUrl + "?showcategory=1";
            XmlDocument m_newsCateDoc = new XmlDocument();
            Dictionary<int, NewsCategory> m_newCategorys = new Dictionary<int, NewsCategory>();		//新闻分类字典
            try
            {
                m_newsCateDoc.Load(xmlUrl);
                XmlNodeList cateList = m_newsCateDoc.SelectNodes("/NewDataSet/NewsCategory");
                foreach (XmlElement cateNode in cateList)
                {
                    //分析分类ID，路径及根ID，并加入分类字典
                    int cateId = Convert.ToInt32(cateNode.SelectSingleNode("newscategoryid").InnerText);
                    string catePath = cateNode.SelectSingleNode("newscategoryidpath").InnerText;
                    NewsCategory newsCate = new NewsCategory(cateId);
                    newsCate.CategoryPath = catePath;
                    if (!m_newCategorys.ContainsKey(cateId))
                        m_newCategorys[cateId] = newsCate;
                }
            }
            catch (System.Exception ex)
            {
                Log.WriteErrorLog("Error issued:xmlUrl=" + xmlUrl);
                Log.WriteErrorLog(ex.ToString());
            }
            return m_newCategorys;
        }
        #endregion

        /// <summary>
        /// 解码html，并删除html标记
        /// </summary>
        public static string HtmlDecode(string title)
        {
            if (string.IsNullOrEmpty(title))
                return title;
            return BitAuto.Utils.StringHelper.RemoveHtmlTag(System.Web.HttpUtility.HtmlDecode(title));
        }

        /// <summary>
        /// 删除分类列表中的子分类
        /// </summary>
        public static List<int> RemoveCarNewsTypeSubCategory(List<int> cateList)
        {
            if (cateList == null || cateList.Count < 2)//cateList.Count < 2 只有一个元素，不需要删除
                return cateList;

            List<int> newList = new List<int>(cateList);
            foreach (int cateId in cateList)
            {
                if (!CommonData.CategoryTreeDic.ContainsKey(cateId))
                {
                    newList.Remove(cateId);
                    continue;
                }
                foreach (int subCateId in CommonData.CategoryTreeDic[cateId])
                {
                    if (newList.Contains(subCateId))
                        newList.Remove(subCateId);
                }
            }
            return newList;
        }

        /// <summary>
        /// 向消息队列发送消息
        /// </summary>
        public static void SendQueueMessage(int contentId, DateTime dateTime, string from, string contentType)
        {
            MessageQueue queue = null;
            try
            {
                //MessageQueue组件初始化
                queue = new MessageQueue(CommonData.CommonSettings.QueueName);
                queue.DefaultPropertiesToSend.Recoverable = true;

                queue.Send(GetQueueXmlDocument(contentId, dateTime, from, contentType));
            }
            finally
            {
                if (queue != null)
                    queue.Dispose();
            }
        }

        /// <summary>
        /// 创建消息xmldocument
        /// </summary>
        public static XmlDocument GetQueueXmlDocument(int contentId, DateTime dateTime, string from, string contentType)
        {
            //创建一个XMLDocument对象，用来在消息中传输
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root = xmldoc.CreateElement("MessageBody");
            xmldoc.AppendChild(root);
            //来源
            XmlElement fromEle = xmldoc.CreateElement("From");
            root.AppendChild(fromEle);
            fromEle.InnerText = from;

            //消息类型
            XmlElement typeEle = xmldoc.CreateElement("ContentType");
            root.AppendChild(typeEle);
            typeEle.InnerText = contentType;

            //内容ID
            XmlElement idEle = xmldoc.CreateElement("ContentId");
            root.AppendChild(idEle);
            idEle.InnerText = contentId.ToString();

            //更新时间
            XmlElement timeEle = xmldoc.CreateElement("UpdateTime");
            root.AppendChild(timeEle);

            timeEle.InnerText = dateTime.ToString("yyyy-MM-dd");
            return xmldoc;
        }

        /// <summary>
        /// 字符串转换为List<int>
        /// </summary>
        public static List<int> StringToIntList(string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString)) return null;

            int id;
            string[] strs = sourceString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> result = new List<int>(strs.Length);
            foreach (string str in strs)
            {
                if (int.TryParse(str, out id) && !result.Contains(id))
                {
                    result.Add(id);
                }
            }
            return result;
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            return ReadFile(path, Encoding.Default);
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadFile(string path, Encoding encoding)
        {
            string result = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(path, encoding))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return result;
        }

        #region memcache

        /// <summary>
        /// 加入memcache
        /// </summary>
        /// <param name="key"> 缓存的Key</param>
        /// <param name="cacheItem">缓存对象</param>
        /// <param name="numberOfMilliSeconds">缓存的时间长度(毫秒)</param>
        public static void MemcacheInsert(string key, object cacheItem, long numberOfMilliSeconds)
        {
            // Log.WriteLog("key:" + key + " cacheItem.length:" + (cacheItem != null ? cacheItem.ToString().Length.ToString() : "0") + " numberOfMilliSeconds:" + numberOfMilliSeconds.ToString());
            if (!string.IsNullOrEmpty(key) && cacheItem != null && numberOfMilliSeconds >= 0)
            {
                DistCacheWrapper.Insert(key.Trim(), cacheItem, numberOfMilliSeconds);
            }
        }

        /// <summary>
        /// 取memcache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object MemcacheGet(string key)
        {
            object obj = null;
            if (!string.IsNullOrEmpty(key))
            {
                obj = DistCacheWrapper.Get(key);
            }
            return obj;
        }



        #endregion

        #region 导航头入库

        /// <summary>
        /// 导航头入库
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tagName"></param>
        /// <param name="headContent"></param>
        public static void InsertCommonHead(int id, string tagName, string headContent)
        {
            string CarCommonHeadProcedureName = "dbo.Car_CommonHead_Insert";
            SqlParameter[] parameters = { 
											new SqlParameter("@ID", SqlDbType.Int) ,
											new SqlParameter("@TagName", SqlDbType.VarChar) ,
											new SqlParameter("@CommonHeadContent", SqlDbType.NVarChar) 
										};
            parameters[0].Value = id;
            parameters[1].Value = tagName;
            parameters[2].Value = headContent;
            int res = BitAuto.Utils.Data.SqlHelper.ExecuteNonQuery(
               Common.CommonData.ConnectionStringSettings.CarChannelConnString
               , CommandType.StoredProcedure, CarCommonHeadProcedureName, parameters);
        }

        /// <summary>
        /// 导航头入库
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tagName"></param>
        /// <param name="headContent"></param>
        public static void InsertCommonHeadV2(int id, string tagName, string headContent)
        {
            string CarCommonHeadProcedureName = "dbo.Car_CommonHeadV2_Insert";
            SqlParameter[] parameters = { 
											new SqlParameter("@ID", SqlDbType.Int) ,
											new SqlParameter("@TagName", SqlDbType.VarChar) ,
											new SqlParameter("@CommonHeadContent", SqlDbType.NVarChar) 
										};
            parameters[0].Value = id;
            parameters[1].Value = tagName;
            parameters[2].Value = headContent;
            int res = BitAuto.Utils.Data.SqlHelper.ExecuteNonQuery(
               Common.CommonData.ConnectionStringSettings.CarChannelConnString
               , CommandType.StoredProcedure, CarCommonHeadProcedureName, parameters);
        }

        #endregion


        /// <summary>
        /// 初始化评测的各个标签 名及匹配规则
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, PingCeTag> IntiPingCeTagInfo()
        {
            Dictionary<int, PingCeTag> dic = new Dictionary<int, PingCeTag>();
            // 导语
            PingCeTag pct1 = new PingCeTag();
            pct1.tagName = "导语";
            pct1.tagRegularExpressions = "(导语：|导语:)";
            dic.Add(1, pct1);
            // 外观
            PingCeTag pct2 = new PingCeTag();
            pct2.tagName = "外观";
            pct2.tagRegularExpressions = "(外观：|外观:)";
            dic.Add(2, pct2);
            // 内饰
            PingCeTag pct3 = new PingCeTag();
            pct3.tagName = "内饰";
            pct3.tagRegularExpressions = "(内饰：|内饰:)";
            dic.Add(3, pct3);
            // 空间
            PingCeTag pct4 = new PingCeTag();
            pct4.tagName = "空间";
            pct4.tagRegularExpressions = "(空间：|空间:)";
            dic.Add(4, pct4);
            // 视野
            PingCeTag pct5 = new PingCeTag();
            pct5.tagName = "视野";
            pct5.tagRegularExpressions = "(视野：|视野:)";
            dic.Add(5, pct5);
            // 灯光
            PingCeTag pct6 = new PingCeTag();
            pct6.tagName = "灯光";
            pct6.tagRegularExpressions = "(灯光：|灯光:)";
            dic.Add(6, pct6);
            // 动力
            PingCeTag pct7 = new PingCeTag();
            pct7.tagName = "动力";
            pct7.tagRegularExpressions = "(动力：|动力:)";
            dic.Add(7, pct7);
            // 操控
            PingCeTag pct8 = new PingCeTag();
            pct8.tagName = "操控";
            pct8.tagRegularExpressions = "(操控：|操控:)";
            dic.Add(8, pct8);
            // 舒适性
            PingCeTag pct9 = new PingCeTag();
            pct9.tagName = "舒适性";
            pct9.tagRegularExpressions = "(舒适性：|舒适：|舒适性:|舒适:)";
            dic.Add(9, pct9);
            // 油耗
            PingCeTag pct10 = new PingCeTag();
            pct10.tagName = "油耗";
            pct10.tagRegularExpressions = "(油耗：|油耗:)";
            dic.Add(10, pct10);
            // 配置
            PingCeTag pct11 = new PingCeTag();
            pct11.tagName = "配置";
            pct11.tagRegularExpressions = "(配置与安全：|配置：|配置与安全:|配置:)";
            dic.Add(11, pct11);
            // 总结
            PingCeTag pct12 = new PingCeTag();
            pct12.tagName = "总结";
            pct12.tagRegularExpressions = "(总结：|总结:)";
            dic.Add(12, pct12);
            return dic;
        }

        /// <summary>
        /// 根据评测新闻url，获取新闻id
        /// </summary>
        public static int GetPingCeNewsId(string url)
        {
            int newsId = 0;
            string[] arrTempURL = url.Split('/');
            string pageName = arrTempURL[arrTempURL.Length - 1];
            if (pageName.Length >= 10)
            {
                if (int.TryParse(pageName.Substring(3, 7), out newsId))
                { }
            }
            return newsId;
        }
        /// <summary>
        /// 获取评测新闻dataset
        /// </summary>
        //public static DataSet GetPingCeNewsDataSet(int newsId)
        //{
        //    object pingCeNewByNewID = null;
        //    DataSet ds = new DataSet();
        //    if (pingCeNewByNewID == null)
        //    {
        //        try
        //        {
        //            ds.ReadXml(string.Concat(CommonData.CommonSettings.NewsUrl, "?id=", newsId));
        //        }
        //        catch (Exception exp)
        //        {
        //            Log.WriteErrorLog(exp);
        //        }
        //    }
        //    return ds;
        //}
        /// <summary>
        /// desc: 根据新闻id获取接口返回的json数据并转换成实体,此方法为通用方法，多处调用  ; 注释掉了上面GetPingCeNewsDataSet方法，用以下方法替换，此处新闻数据从周新锋提供的接口出；
        /// date:2017-1-10
        /// author:zf
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        public static NewsEntityV2 GetNewsEntityFromApi(int newsId)
        {
            NewsEntityV2 curNewJson = null;
            try
            {
                string url = CommonData.CommonSettings.NewsUrl + "?id=" + newsId;
                string singleNewObject = GetResponseFromUrl(url);
                if (!string.IsNullOrEmpty(singleNewObject))
                {
                    //json转换
                    curNewJson = JsonConvert.DeserializeObject<NewsEntityV2>(singleNewObject);
                }
            }
            catch(Exception exp)
            {
                Log.WriteErrorLog(exp);
            }
            return curNewJson;
        }

        public static DataSet GetPingCeNewsDataSetV2(int newsId)
        {
            object pingCeNewByNewID = null;
            DataSet ds = new DataSet();
            if (pingCeNewByNewID == null)
            {
                try
                {
                    ds.ReadXml(string.Format(CommonData.CommonSettings.NewsDetailUrl, newsId));
                }
                catch (Exception exp)
                {
                    Log.WriteErrorLog(exp);
                }
            }
            return ds;
        }

        public static void InsertMessageDbLog(XElement bodyElement, string from, string contentType, bool isDelete)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(bodyElement.Parent.ToString());
                InsertContentMsgBodyLogDB(new Common.Model.ContentMessage()
                {
                    From = from,
                    ContentType = contentType,
                    IsDelete = isDelete,
                    UpdateTime = DateTime.Now,
                    ContentId = 0,
                    ContentBody = xDoc
                }
                , new Guid(xDoc.SelectSingleNode("/Messages/Body/EntityId").InnerText));
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("写消息数据库日志出现异常！：msgxml：[" +
                    ((bodyElement != null) ? bodyElement.ToString() : string.Empty) +
                    "];\r\nerrormsg:[" + ex.ToString() + "]");
            }
        }

        /// <summary>
        /// 将消息写入消息日志数据库 0.86\baadb1 AutoCarChannelManage 库 MessageLog 表
        /// </summary>
        /// <param name="cm"></param>
        public static void InsertContentMsgBodyLogDB(ContentMessage cm, Guid guid)
        {
            string sp = "SP_MessageLog_InsertNew";
            SqlParameter[] param = new SqlParameter[8] { 
				new SqlParameter("@MessageFrom",SqlDbType.VarChar,50),
				new SqlParameter("@ContentType",SqlDbType.VarChar,50),
				new SqlParameter("@ContentId",SqlDbType.Int),
				new SqlParameter("@UpdateTime",SqlDbType.DateTime),
				new SqlParameter("@IsDelete",SqlDbType.Bit),
				new SqlParameter("@MessageBody",SqlDbType.VarChar,1000),
				new SqlParameter("@AutoID",SqlDbType.BigInt),
                new SqlParameter("@Id",SqlDbType.UniqueIdentifier),
			};
            param[0].Value = cm.From;
            param[1].Value = cm.ContentType;
            param[2].Value = cm.ContentId;
            param[3].Value = cm.UpdateTime;
            param[4].Value = cm.IsDelete;
            param[5].Value = cm.ContentBody.InnerXml.Length > 1000 ?
                cm.ContentBody.InnerXml.Substring(0, 1000) : cm.ContentBody.InnerXml;
            param[6].Direction = ParameterDirection.Output;

            param[7].Value = guid;

            int res = BitAuto.Utils.Data.SqlHelper.ExecuteNonQuery(
                CommonData.ConnectionStringSettings.CarChannelManageConnString
                , CommandType.StoredProcedure, sp, param);
            // 设置日志ID
            int autoID = 0;
            if (int.TryParse(param[6].Value.ToString(), out autoID))
            {
                if (autoID > 0)
                { cm.LogID = autoID; }
            }
        }

        public static int DateDiff(string dateInterval, DateTime dateTime1, DateTime dateTime2)
        {
            int dateDiff = 0;
            try
            {
                TimeSpan timeSpan = new TimeSpan(dateTime2.Ticks - dateTime1.Ticks);

                switch (dateInterval.ToLower())
                {
                    case "year":
                    case "y":
                        dateDiff = dateTime2.Year - dateTime1.Year;
                        break;
                    case "month":
                    case "m":
                        dateDiff = (dateTime2.Year * 12 + dateTime2.Month) - (dateTime1.Year * 12 + dateTime1.Month);
                        break;
                    case "day":
                    case "d":
                        dateDiff = (int)timeSpan.TotalDays;
                        break;
                    case "hour":
                    case "h":
                        dateDiff = (int)timeSpan.TotalHours;
                        break;
                    case "minute":
                    case "n":
                        dateDiff = (int)timeSpan.TotalMinutes;
                        break;
                    case "second":
                    case "s":
                        dateDiff = (int)timeSpan.TotalSeconds;
                        break;
                    case "milliseconds":
                    case "ms":
                        dateDiff = (int)timeSpan.TotalMilliseconds;
                        break;
                    default:
                        dateDiff = (int)timeSpan.TotalMinutes;
                        break;
                }
            }
            catch
            {

            }
            return dateDiff;
        }

        public static string GetResponseFromUrl(string url, int interval = 60000)
        {
            string result = string.Empty;
            HttpWebRequest req = null;
            HttpWebResponse response = null;
            Stream responseStream = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = interval;
                using (response = req.GetResponse() as HttpWebResponse)
                using (responseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException webException)
            {
                var responseText = string.Empty;
                var resEx = webException.Response as HttpWebResponse;
                if (resEx == null)
                {
                    Log.WriteErrorLog(string.Format("请求新闻Url:{0},webException异常信息:null", url));
                    throw webException;
                }

                try
                {
                    using (StreamReader sr = new StreamReader(resEx.GetResponseStream(), Encoding.UTF8))
                    {
                        responseText = sr.ReadToEnd();
                    }
                    resEx.Close();
                    resEx = null;
                    Log.WriteErrorLog(string.Format("status={0} {1},请求新闻Url:{2},webException异常信息:{3}", webException.Status, webException.Message, url, responseText));
                }
                catch (Exception subException)
                {
                    Log.WriteErrorLog(string.Format("webException异常信息:{0}", subException.ToString()));
                    throw subException;
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(string.Format("请求新闻Url:{1},GetResponseStream异常信息:{0}", ex.ToString(), url));
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (req != null)
                {
                    req.Abort();
                }
            }
            return result;
        }
    }

}
