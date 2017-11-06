using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Web;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.HtmlBuilder;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data;
using BitAuto.CarDataUpdate.DataProcesser.cn.com.baa.api;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.news;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.dealer;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.imgsvr;
using System.Threading;
using System.Xml.Linq;
using BitAuto.Services.Cache;


namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class ContentGetter
    {
        private XmlDocument m_newsCateDoc;							//新闻分类XML
        private Dictionary<int, NewsCategory> m_newCategorys;		//新闻分类字典


        private string m_levelDataPath;
        private string m_levelBackupPath;
        private string m_serialDataPath;
        private string m_serialBackupPath;
        public event LogHandler Log;

        private Dictionary<string, int[]> m_kindCateForLevel;
        private static Dictionary<int, int> carCostLevelDic;		//油耗与养车费用的级别

        private string m_serialCuxiaoNewsCountPath;

        private Dictionary<string, string> levelNameDic;

        public ContentGetter()
        {
            m_newsCateDoc = new XmlDocument();
            m_newCategorys = new Dictionary<int, NewsCategory>();


            m_levelDataPath = Path.Combine(CommonData.CommonSettings.SavePath, "LevelNews");
            m_levelBackupPath = Path.Combine(m_levelDataPath, "Backup");
            m_serialDataPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews");
            m_serialBackupPath = Path.Combine(m_serialDataPath, "Backup");

            m_serialCuxiaoNewsCountPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialCityNews\\CityNewsCount.xml");

            carCostLevelDic = new Dictionary<int, int>();
            carCostLevelDic[1] = 0;
            carCostLevelDic[2] = 1;
            carCostLevelDic[3] = 2;
            carCostLevelDic[4] = 4;
            carCostLevelDic[5] = 3;
            carCostLevelDic[6] = 5;
            carCostLevelDic[7] = 8;
            carCostLevelDic[8] = 7;
            carCostLevelDic[9] = 6;
            carCostLevelDic[10] = 9;
            carCostLevelDic[11] = 10;
            carCostLevelDic[12] = 11;

            m_kindCateForLevel = new Dictionary<string, int[]>();
            m_kindCateForLevel["xinwen"] = new int[] { 150, 152, 34, 148, 146, 144, 147, 83, 198, 149, 123, 127, 2, 13, 210, 98 };
            m_kindCateForLevel["hangqing"] = new int[] { 3 };
            m_kindCateForLevel["daogou"] = new int[] { 4, 179, 102, 115, 120 };
            m_kindCateForLevel["pingce"] = new int[] { 33, 31, 32, 29, 30 };
            m_kindCateForLevel["yongche"] = new int[] { 87, 88, 143, 142, 86, 85, 173, 56 };

            levelNameDic = new Dictionary<string, string>();
            levelNameDic["微型车"] = "微型车";
            levelNameDic["小型车"] = "小型车";
            levelNameDic["紧凑型"] = "紧凑型车";
            levelNameDic["中型车"] = "中型车";
            levelNameDic["中大型"] = "中大型车";
            levelNameDic["豪华型"] = "豪华车";
            levelNameDic["MPV"] = "MPV";
            levelNameDic["SUV"] = "SUV";
            levelNameDic["跑车"] = "跑车";
            levelNameDic["其它"] = "其它";
            levelNameDic["面包车"] = "面包车";
            levelNameDic["皮卡"] = "皮卡";
        }

        public void GetContent()
        {
            try
            {
                //级别新闻
                CreateLevelDataPath();
                GetNewsCategorys();
                //GetLevelVideos(); lisf 2017-11-02
                //GetLevelCarCost();
                GetImgUrl();

                //品牌列表
                UpdateBrandTree();
                //城市列表
                UpdateCityList();

                //子品牌综述
                CreateSerialDataPath();
                GetSerialFocusImage(0);
                GetSerialForumSubject(0);		//获取论坛数据
                //GetSerialHotNews(0);

                //http://go.bitauto.com/api/HandlerIntensionMember.ashx?modelid={0}&intensiontype=1 无效
                //GetSerialsIntensionPersion();

                //获取所有子品牌的视频数量
                // GetAllSerialVideoCount();

                // modified by chengl May.5.2015 暂不执行，车型频道不使用
                // GetBrandImages(0);

                // modified by chengl May.5.2015 暂不执行，车型频道不使用
                // GetBrandForumInfo(0);
                // GetBrandUsecar(0);

                // modified by chengl May.5.2015 暂不执行，车型频道不使用
                // GetMasterbrandForumInfo(0);
                // GetMasterbrandUsecar(0);

                /*
                 * modified by sk 2014.06.05 废除旧问答接口生成
                 * 
                //GetSerialAskEntries(0);
				 
                */

                GetSerialVideo(0);
                GetAllSerialForumUrl();
                new Serial().GetCarCityPriceBySerial(0);

                //获取所有车型油耗
                GetAllCarFuel();

                //二手车
                // GetUcarInfo(0);

                //所有子品牌的点评数
                //GetAllSerialDianpingCount();
                //GetSerialDianping(0);
                //子品牌综述页的口碑印象与优缺点
                GetSerialKoubeiImpression(0);
                //生成所有子品牌口碑块
                GenerateSerialKoubeiHtml(0);
            }
            catch (Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }
        /* //del by lsf 2016-01-06
        /// <summary>
        /// 得到城市的行情,单独服务内容
        /// </summary>
        /// <param name="serialId"></param>
        public void GetSerialCityHangqingNews(int serialId, int cityId)
        {
            List<int> serialList = null;
            if (serialId == 0)
                serialList = GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(serialId);
            }

            // 			int[] cateIdList = new int[] { 16, 215, 3 };  //行情与新车到店


            Dictionary<int, City> cityDic = GetCityDic();
            int counter = 0;
            foreach (int sId in serialList)
            {
                counter++;
                OnLog("     Get Serial :" + sId + " Hangqing news (" + counter + "/" + serialList.Count + ")...", false);
                foreach (City city in cityDic.Values)
                {
                    if (city.CityLevel > 1)
                        continue;
                    if (cityId > 0 && city.CityId != cityId)
                        continue;
                    string xmlUrl = CommonData.CommonSettings.NewsUrl + "?nonewstype=2&brandid=" + sId + "&cityid=" + city.CityId + "&getcount=500&categoryId=3,16,215";
                    string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialCityNews\\Hangqing\\" + city.CityName);
                    string backupPath = Path.Combine(filePath, "Backup");
                    string xmlFile = "SerialHangqing_" + sId + ".xml";
                    string backupFile = Path.Combine(backupPath, xmlFile);
                    xmlFile = Path.Combine(filePath, xmlFile);

                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(xmlUrl);
                        int tempCounter = 0;
                        XmlNodeList newsList = xmlDoc.SelectNodes("/NewDataSet/listNews");
                        if (newsList.Count > 0)
                        {
                            List<XmlElement> tempList = new List<XmlElement>();
                            foreach (XmlElement newsNode in newsList)
                            {
                                tempCounter++;
                                AppendNewsInfo(newsNode);
                                tempList.Add(newsNode);
                                if (tempList.Count > 10 || tempCounter == newsList.Count)
                                {
                                    AppendNewsCommentNum(tempList);
                                    tempList.Clear();
                                }
                            }
                        }
                        CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                    }
                    catch (System.Exception ex)
                    {
                        OnLog(ex.ToString(), true);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取城市字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, City> GetCityDic()
        {
            XmlDocument cityDoc = new XmlDocument();
            string cityFile = Path.Combine(CommonData.CommonSettings.SavePath, "city.xml");
            cityDoc.Load(cityFile);

            XmlNodeList cityList = cityDoc.SelectNodes("/CityValueSet/CityItem");
            Dictionary<int, City> cityDic = new Dictionary<int, City>();
            //城市列表
            foreach (XmlElement cityNode in cityList)
            {
                int cityId = Convert.ToInt32(cityNode.SelectSingleNode("CityId").InnerText);
                string cityName = cityNode.SelectSingleNode("CityName").InnerText;
                int cityLevel = Convert.ToInt32(cityNode.SelectSingleNode("Level").InnerText);
                cityDic[cityId] = new City(cityId, cityName);
                cityDic[cityId].CityLevel = cityLevel;
            }
            return cityDic;
        }
        
        /// <summary>
        /// 获取主品牌的答疑
        /// </summary>
        /// <param name="masterId"></param>
        public void GetMasterBrandAskEntries(int masterId)
        {
            string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "AskEntries\\MasterBrand");
            string newsBackupPath = Path.Combine(newsPath, "Backup");
            if (!Directory.Exists(newsBackupPath))
                Directory.CreateDirectory(newsBackupPath);

            Dictionary<int, List<int>> masterbrandList = null;
            if (masterId == 0)
                masterbrandList = GetMasterBrandDic();
            else
            {
                masterbrandList = new Dictionary<int, List<int>>();
                masterbrandList.Add(masterId, new List<int>());
            }

            int counter = 0;
            foreach (int mId in masterbrandList.Keys)
            {
                counter++;
                OnLog("     Get MasterBrand:" + mId + " ask entries(" + counter + "/" + masterbrandList.Count + ")...", false);
                try
                {
                    string xmlFile = "Masterbrand_Ask_" + mId + ".xml";
                    string backupFile = Path.Combine(newsBackupPath, xmlFile);
                    xmlFile = Path.Combine(newsPath, xmlFile);

                    string xmlUrl = String.Format(CommonData.CommonSettings.AskEntriesUrl, "bsid", mId, 12);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }
       
        /// <summary>
        /// 统计所有子品牌的各类新闻的数量
        /// </summary>
        public void StatisSerialsNewsCount()
        {
            string[] stateArray = new string[] { "在销", "待销", "停销" };
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(CommonData.CommonSettings.SavePath, "AllAutoData.xml"));
            OnLog("     Statis SerialsNews Count......", true);
            string savFile = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNewsCount.csv");
            if (File.Exists(savFile))
                File.Delete(savFile);
            string lineStr = "ID,子品牌显示名,新闻,导购,试驾,行情,用车,视频,评测\r\n";
            File.AppendAllText(savFile, lineStr, Encoding.UTF8);
            foreach (string stateStr in stateArray)
            {
                File.AppendAllText(savFile, stateStr + ",,,,,,,,\r\n\r\n\r\n", Encoding.UTF8);
                string xmlPath = "/Params/MasterBrand/Brand/Serial[@CsSaleState=\"" + stateStr + "\"]";
                XmlNodeList serialNodeList = xmlDoc.SelectNodes(xmlPath);
                Console.WriteLine(stateStr + ":" + serialNodeList.Count);
                int lineTop = Console.CursorTop;
                Console.WriteLine("0");
                int counter = 0;
                foreach (XmlElement serialNode in serialNodeList)
                {
                    counter++;

                    Console.SetCursorPosition(0, lineTop);
                    Console.WriteLine(counter);
                    lineStr = StatisSerialNewsCount(serialNode);
                    File.AppendAllText(savFile, lineStr, Encoding.UTF8);
                }
            }
        }
        
        /// <summary>
        /// 统计一个子品牌的新闻数量
        /// </summary>
        /// <param name="serialNode"></param>
        /// <returns></returns>
        private string StatisSerialNewsCount(XmlElement serialNode)
        {
            int serialId = Convert.ToInt32(serialNode.GetAttribute("ID"));
            string serialShowName = serialNode.GetAttribute("ShowName");
            string[] newsTypeArray = new string[] { "xinwen", "daogou", "shijia", "hangqing", "yongche", "SerialVideo", "pingce" };
            string lineStr = serialId + "," + serialShowName;
            foreach (string newsType in newsTypeArray)
            {
                string xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\" + newsType + "\\Serial_All_News_" + serialId + ".xml");
                string xmlPath = "/root/listNews";
                if (newsType == "SerialVideo")
                {
                    xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\" + newsType + "\\Serial_Video_" + serialId + ".xml");
                    xmlPath = "/NewDataSet/listNews";
                }
                if (File.Exists(xmlFile))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlFile);
                    XmlNodeList nodeList = xmlDoc.SelectNodes(xmlPath);
                    lineStr += "," + nodeList.Count;
                }
                else
                    lineStr += ",0";
            }

            return lineStr + "\r\n";

        }
         */
        public void GetCarShowTopNews()
        {
            OnLog("     Get the exhibition TopNews......", true);

            //北京车展标签id为：24

            string carShowName = "2010北京车展";
            int carshowTagId = 24;			//CMS的09广州车展的标签ID
            int carShowId = 19;				//展会ID
            string allBrandFile = Path.Combine(CommonData.CommonSettings.SavePath, "AllAutoData.xml");
            XmlDocument allBrandDoc = new XmlDocument();
            allBrandDoc.Load(allBrandFile);
            XmlDocument treeDoc = GetCarshowBrandTree(carShowId);
            Dictionary<int, List<int>> masterDic = new Dictionary<int, List<int>>();
            List<int> serialList = new List<int>();
            XmlNodeList serialNodes = treeDoc.SelectNodes("/Exhibition/MasterBrand/Brand/Serial");
            foreach (XmlElement tempNode in serialNodes)
            {
                int serialId = ConvertHelper.GetInteger(tempNode.GetAttribute("ID"));
                //查询所属品牌与主品牌
                XmlNode seialNode = allBrandDoc.SelectSingleNode("/Params/MasterBrand/Brand/Serial[@ID=\"" + serialId + "\"]");
                if (seialNode != null)
                {
                    XmlElement brandNode = (XmlElement)seialNode.ParentNode;
                    int brandId = ConvertHelper.GetInteger(brandNode.GetAttribute("ID"));
                    XmlElement masterNode = (XmlElement)brandNode.ParentNode;
                    int masterId = ConvertHelper.GetInteger(masterNode.GetAttribute("ID"));
                    if (brandId > 0 && masterId > 0)
                    {
                        serialList.Add(serialId);
                        if (masterDic.ContainsKey(masterId))
                        {
                            if (!masterDic[masterId].Contains(brandId))
                                masterDic[masterId].Add(brandId);
                        }
                        else
                        {
                            masterDic[masterId] = new List<int>();
                            masterDic[masterId].Add(brandId);
                        }
                    }
                }
            }

            //生成新闻
            GetCarshowMasterbrandNews(masterDic, carshowTagId);
            GetCarshowSerialNews(serialList, carshowTagId);
            GetCarShowMasterBrandVideos(masterDic);
            GetCarshowSerialVideos(serialList);
        }

        /// <summary>
        /// 获取车展中子品牌的点评
        /// </summary>
        public void GetCarshowSerialDianping()
        {
            OnLog("     Get the exhibition SerialDianping.", true);

            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "CarShow\\2010Beijing\\SerialDianping");
            string backupPath = Path.Combine(filePath, "Backup");

            string baseUrl = "http://koubei.bitauto.com/api/handler.ashx?apikey=a1b7f24d6c1e473d9bb87336b92885f3&cat=topic&op=get&carid={0}&level={1}&maxresults=10";

            int carShowId = 19;
            XmlDocument treeDoc = GetCarshowBrandTree(carShowId);
            XmlNodeList serialNodes = treeDoc.SelectNodes("/Exhibition/MasterBrand/Brand/Serial");
            foreach (XmlElement tempNode in serialNodes)
            {

                int serialId = ConvertHelper.GetInteger(tempNode.GetAttribute("ID"));
                string xmlFile = "Dianping_Serial_" + serialId + ".xml";
                string backupFile = Path.Combine(backupPath, xmlFile);
                xmlFile = Path.Combine(filePath, xmlFile);
                // OnLog(xmlFile, true);
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("SerialDianping");
                xmlDoc.AppendChild(root);
                XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.InsertBefore(xmlDeclar, root);
                //1\2\3差中好
                int dianpingCount = 0;
                for (int i = 1; i <= 3; i++)
                {
                    XmlElement dpNode = xmlDoc.CreateElement("Dianping");
                    dpNode.SetAttribute("type", i.ToString());
                    root.AppendChild(dpNode);
                    string xmlUrl = String.Format(baseUrl, serialId, i);
                    try
                    {
                        XmlDocument dpDoc = new XmlDocument();
                        dpDoc.Load(xmlUrl);
                        dpNode.InnerXml = dpDoc.DocumentElement.InnerXml;
                    }
                    catch { }

                    //获取点评数
                    string getcountUrl = xmlUrl + "&getcount=1";
                    try
                    {
                        WebClient wc = new WebClient();
                        string countStr = wc.DownloadString(getcountUrl);
                        int count = ConvertHelper.GetInteger(countStr);
                        dianpingCount += count;
                        dpNode.SetAttribute("count", count.ToString());
                    }
                    catch (Exception ex)
                    {
                        OnLog(ex.Message, true);
                    }
                }

                root.SetAttribute("count", dianpingCount.ToString());

                CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            }
        }

        /// <summary>
        /// 获取车展中的主品牌，子品牌的封面图
        /// </summary>
        public void GetGuangzhouCarshowBrandImage()
        {
            string modelImageUrl = "http://imgsvr.bitauto.com/zhuanti/guangzhou2009.aspx?code=gz2009mastermodel&classid={0}&rownum=14";
            string xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, "CarShow\\DefaultImages.xml");
            string backupFile = Path.Combine(CommonData.CommonSettings.SavePath, "CarShow\\Backup\\DefaultImages.xml");

            XmlDocument imgDoc = new XmlDocument();
            XmlElement root = imgDoc.CreateElement("Images");
            imgDoc.AppendChild(root);
            XmlDeclaration xmlDeclar = imgDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            imgDoc.InsertBefore(xmlDeclar, root);

            try
            {
                OnLog("     Get car show default images...", true);
                int carshowId = 25347;//25347，测试用：22307
                CommonService commonSrv = new CommonService();
                DataSet imgDs = commonSrv.GetChildrenByParentId(carshowId, true);

                int parentId = 25353;		//新车图片的分类ID，测试用22308
                int modelId = 25352;		//车模图片的分类ID，测试用22309
                string masterFilter = "CommonClassParentId=" + parentId;
                string serialFilter = "CommonClassCode LIKE '%," + parentId + ",%' AND CommonClassIsEnd=true";


                if (imgDs != null)
                {
                    //取主品牌封面图
                    DataRow[] masterRows = imgDs.Tables[0].Select(masterFilter);
                    int counter = 0;
                    foreach (DataRow row in masterRows)
                    {
                        counter++;
                        int masterId = ConvertHelper.GetInteger(row["ClassDependId"]);
                        if (masterId > 0)
                        {
                            OnLog("     Get car show default images master:" + masterId + "(" + counter + "/" + masterRows.Length + ")...", false);

                            int imgId = ConvertHelper.GetInteger(row["SiteImageId"]);
                            int classId = ConvertHelper.GetInteger(row["CommonClassId"]);
                            string imgUrl = ConvertHelper.GetString(row["SiteImageUrl"]);
                            int masterImageCount = ConvertHelper.GetInteger(row["ImageCount"]);

                            XmlElement masterNode = imgDoc.CreateElement("Master");
                            masterNode.SetAttribute("ID", masterId.ToString());
                            masterNode.SetAttribute("MasterClassID", classId.ToString());
                            masterNode.SetAttribute("ImgId", imgId.ToString());
                            masterNode.SetAttribute("ImgUrl", imgUrl);
                            masterNode.SetAttribute("ImageCount", masterImageCount.ToString());
                            root.AppendChild(masterNode);

                            //车模主品牌分类ID
                            string modelFilter = "CommonClassParentId=" + modelId + " AND ClassDependId=" + masterId;
                            DataRow[] masterModelRows = imgDs.Tables[0].Select(modelFilter);
                            if (masterModelRows.Length > 0)
                            {
                                int albumId = ConvertHelper.GetInteger(masterModelRows[0]["CommonClassId"]);
                                int modelImageCount = ConvertHelper.GetInteger(masterModelRows[0]["ImageCount"]);
                                masterNode.SetAttribute("ClassID", albumId.ToString());
                                masterNode.SetAttribute("modelImageCount", modelImageCount.ToString());

                                //取主品牌车模图
                                string modelUrl = String.Format(modelImageUrl, albumId);
                                try
                                {
                                    XmlDocument mbDoc = new XmlDocument();
                                    mbDoc.Load(modelUrl);
                                    Thread.Sleep(1000);
                                    masterNode.InnerXml = mbDoc.DocumentElement.InnerXml;
                                }
                                catch (Exception ex)
                                {
                                    OnLog(ex.Message, true);
                                }
                            }
                        }
                    }

                    //取子品牌封面图
                    DataRow[] serialRows = imgDs.Tables[0].Select(serialFilter);
                    counter = 0;
                    foreach (DataRow row in serialRows)
                    {
                        counter++;
                        int serialId = ConvertHelper.GetInteger(row["ClassDependId"]);
                        if (serialId > 0)
                        {
                            OnLog("Get car show default images Serial:" + serialId + "(" + counter + "/" + serialRows.Length + ")...", false);

                            int imgId = ConvertHelper.GetInteger(row["SiteImageId"]);
                            string imgUrl = ConvertHelper.GetString(row["SiteImageUrl"]);
                            int imgCount = ConvertHelper.GetInteger(row["ImageCount"]);

                            XmlElement serialNode = imgDoc.CreateElement("Serial");
                            serialNode.SetAttribute("ID", serialId.ToString());
                            serialNode.SetAttribute("ImgId", imgId.ToString());
                            serialNode.SetAttribute("ImageCount", imgCount.ToString());
                            serialNode.SetAttribute("ImgUrl", imgUrl);
                            root.AppendChild(serialNode);

                            //每子品牌取前10张图
                            //图集ID
                            int albumId = ConvertHelper.GetInteger(row["CommonClassId"]);
                            serialNode.SetAttribute("ClassID", albumId.ToString());
                            if (albumId > 0)
                            {
                                PagedImageList imageList = commonSrv.GetImageListByAlbumIdPaged(albumId, 10, 1);
                                if (imageList.Data != null)
                                {
                                    DataTable imgTable = imageList.Data.Tables[0];
                                    foreach (DataRow imgRow in imgTable.Rows)
                                    {
                                        int tImgId = ConvertHelper.GetInteger(imgRow["SiteImageId"]);
                                        string tImgUrl = ConvertHelper.GetString(imgRow["SiteImageUrl"]);
                                        string tImgName = ConvertHelper.GetString(imgRow["SiteImageName"]);
                                        XmlElement imgNode = imgDoc.CreateElement("Image");
                                        imgNode.SetAttribute("imgId", tImgId.ToString());
                                        imgNode.SetAttribute("imgName", tImgName);
                                        imgNode.SetAttribute("imgUrl", tImgUrl);
                                        serialNode.AppendChild(imgNode);
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }

            CommonFunction.SaveXMLDocument(imgDoc, xmlFile);
        }

        /// <summary>
        /// 获取车展的品牌树
        /// </summary>
        /// <returns></returns>
        private XmlDocument GetCarshowBrandTree(int exhibitionId)
        {
            string sqlStr = "SELECT RelationCar FROM Exhibition_Relation_Car WHERE ExhibitionID=" + exhibitionId;
            string xmlBrandtree = ConvertHelper.GetString(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr)).Trim();
            xmlBrandtree = "<Exhibition>" + xmlBrandtree + "</Exhibition>";
            XmlDocument treeDoc = new XmlDocument();
            try
            {
                OnLog("getting the exhibition brand tree.", true);
                treeDoc.LoadXml(xmlBrandtree);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
            return treeDoc;
        }

        /// <summary>
        /// 获取主品牌视频
        /// </summary>
        public void GetCarShowMasterBrandVideos(Dictionary<int, List<int>> masterDic)
        {
            string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "CarShow\\2010Beijing\\MasterBrandVideos");
            string newsBackupPath = Path.Combine(newsPath, "CarShow\\2010Beijing\\Backup");
            if (!Directory.Exists(newsBackupPath))
                Directory.CreateDirectory(newsBackupPath);

            int counter = 0;
            foreach (int masterId in masterDic.Keys)
            {
                counter++;
                OnLog("Get master brand:" + masterId + " videos(" + counter + "/" + masterDic.Count + ")...", false);
                try
                {
                    string brandIdList = "";
                    foreach (int brandId in masterDic[masterId])
                    {
                        brandIdList += brandId + ",";
                    }
                    brandIdList = brandIdList.TrimEnd(new char[] { ',' });

                    string xmlFile = "MasterBrand_Videos_" + masterId + ".xml";
                    string backupFile = Path.Combine(newsBackupPath, xmlFile);
                    xmlFile = Path.Combine(newsPath, xmlFile);

                    string xmlUrl = CommonData.CommonSettings.NewsUrl + "?articaltype=3&getcount=12&include=1&bigbrand=" + brandIdList;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        /// <summary>
        /// 获取所有子品牌的视频
        /// </summary>
        public void GetCarshowSerialVideos(List<int> serialList)
        {
            OnLog("Get serial video...", true);

            string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "CarShow\\2010Beijing\\SerialVideo");
            string backupPath = Path.Combine(newsPath, "Backup");
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("Get serial " + serialId + " video(" + counter + "/" + serialList.Count + ")...", false);
                string xmlUrl = CommonData.CommonSettings.NewsUrl + "?articaltype=3&getcount=4&brandid=" + serialId;
                string xmlFile = "Serial_Video_" + serialId + ".xml";
                string backupFile = Path.Combine(backupPath, xmlFile);
                xmlFile = Path.Combine(newsPath, xmlFile);

                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        /// <summary>
        /// 获取主品牌的车展新闻
        /// </summary>
        /// <param name="masterDic"></param>
        /// <param name="carshowTagId"></param>
        private void GetCarshowMasterbrandNews(Dictionary<int, List<int>> masterDic, int carshowTagId)
        {
            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "CarShow\\2010Beijing\\Masterbrand");
            string backupPath = Path.Combine(filePath, "Backup");

            int counter = 0;
            foreach (int masterId in masterDic.Keys)
            {
                counter++;
                OnLog("Get carshow masterbrand:" + masterId + " news,(" + counter + "/" + masterDic.Count + ")", false);

                string brandIdStr = "";
                foreach (int brandId in masterDic[masterId])
                {
                    brandIdStr += brandId + ",";
                }
                brandIdStr = brandIdStr.TrimEnd(',');

                string xmlFile = "Carshow_Masterbrand_" + masterId + ".xml";
                string backupFile = Path.Combine(backupPath, xmlFile);
                xmlFile = Path.Combine(filePath, xmlFile);

                string xmlUrl = CommonData.CommonSettings.NewsUrl + "?bigbrand=" + brandIdStr + "&include=1&getcount=9&tag=" + carshowTagId;
                XmlDocument newsDoc = new XmlDocument();
                try
                {
                    newsDoc.Load(xmlUrl);
                    CommonFunction.SaveXMLDocument(newsDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        /// <summary>
        /// /获取车展子品牌的新闻
        /// </summary>
        /// <param name="serialList"></param>
        /// <param name="exhibitionName"></param>
        private void GetCarshowSerialNews(List<int> serialList, int carshowTagId)
        {
            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "CarShow\\2010Beijing\\Serial");
            string backupPath = Path.Combine(filePath, "Backup");

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("Get carshow Serial:" + serialId + " news,(" + counter + "/" + serialList.Count + ")", false);

                string xmlFile = "Carshow_Serial_" + serialId + ".xml";
                string backupFile = Path.Combine(backupPath, xmlFile);
                xmlFile = Path.Combine(filePath, xmlFile);

                string xmlUrl = CommonData.CommonSettings.NewsUrl + "?brandid=" + serialId + "&getcount=9&tag=" + carshowTagId;
                XmlDocument newsDoc = new XmlDocument();
                try
                {
                    newsDoc.Load(xmlUrl);
                    CommonFunction.SaveXMLDocument(newsDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        /// <summary>
        /// 获取子品牌的城市新闻
        /// </summary>
        public void UpdateSerialCityNews()
        {
            UpdateSerialCityNews(0);
        }

        /// <summary>
        /// 获取经销商促销信息
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="cityId"></param>
        /// <param name="newsType">cuxiao|diannei</param>
        /// <returns></returns>
        private XmlDocument GetVendorNews(int serialId, int cityId, string newsType, out int totalCount)
        {
            totalCount = 0;

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                VendorNews vendor = new VendorNews();

                DataSet vendorNews = null;
                if (newsType == "cuxiao")
                    //vendorNews = vendor.GetNewsPageList(serialId, -1, cityId, -1, 2, -1, 1, 100,1);
                    vendorNews = vendor.GetNewsPageListAndCount(serialId, -1, cityId, -1, -1, -1, 1, 100, 1, out totalCount);		//取全部 ddl modi 2010-3-2
                else if (newsType == "diannei")
                    vendorNews = vendor.GetNewsPageListByNCIdList(serialId, -1, cityId, -1, -1, -1, 1, "1,3,4,5", 100, 1);

                if (vendorNews != null && vendorNews.Tables.Count > 0)
                {
                    DataColumn col = new DataColumn("url");
                    vendorNews.Tables[0].Columns.Add(col);
                    foreach (DataRow row in vendorNews.Tables[0].Rows)
                    {
                        // 						int newsId = Convert.ToInt32(row["news_Id"]);
                        // 						int vendorId = Convert.ToInt32(row["VendorID"]);
                        // 						DateTime publish = Convert.ToDateTime(row["news_PubTime"]);
                        string newsUrl = Convert.ToString(row["NewsUrl"]);
                        if (!newsUrl.StartsWith("/"))
                            newsUrl = "/" + newsUrl;
                        row["url"] = "http://dealer.bitauto.com" + newsUrl;
                    }
                    MemoryStream stream = new MemoryStream();
                    vendorNews.WriteXml(stream);
                    stream.Position = 0;
                    xmlDoc.Load(stream);
                    stream.Close();
                    XmlNodeList newsList = xmlDoc.SelectNodes("/NewDataSet/Table");
                }

            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
            return xmlDoc;
        }

        /// <summary>
        /// 获取某子品牌所有城市的经销商新闻列表
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="cityDic"></param>
        private void GetSerilaCityNews(int serialId, Dictionary<int, City> cityDic, out Dictionary<int, int> newsNumber)
        {
            newsNumber = new Dictionary<int, int>();
            Dictionary<int, City> to30CityDic = CityInitData.GetCityTo30City();
            string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialCityNews");
            string backupPath = Path.Combine(newsPath, "Backup");
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            string xmlFile = "CityNews_" + serialId + ".xml";
            string backupFile = Path.Combine(backupPath, xmlFile);
            xmlFile = Path.Combine(newsPath, xmlFile);

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
            XmlDeclaration declar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.InsertBefore(declar, root);


            foreach (int cityId in cityDic.Keys)
            {
                if (cityDic[cityId].CityLevel != 1)
                    continue;

                string cityName = cityDic[cityId].CityName;
                XmlElement cityEle = xmlDoc.CreateElement("City");
                cityEle.SetAttribute("id", cityId.ToString());
                cityEle.SetAttribute("name", cityName);
                root.AppendChild(cityEle);

                int totalCount;
                //获取促销新闻
                XmlDocument vendorDoc = GetVendorNews(serialId, cityId, "cuxiao", out totalCount);
                if (totalCount > 0)
                {
                    newsNumber[cityId] = totalCount;
                }
                //促销新闻
                XmlElement sellRoot = xmlDoc.CreateElement("Kind");
                sellRoot.SetAttribute("kind", "cuxiao");
                cityEle.AppendChild(sellRoot);
                XmlNodeList sellNodeList = vendorDoc.SelectNodes("/NewDataSet/Table");
                foreach (XmlElement newsNode in sellNodeList)
                {
                    sellRoot.AppendChild(xmlDoc.ImportNode(newsNode, true));
                }
            }

            CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
        }

        /// <summary>
        /// 获取子品牌的经销商城市新闻，如果SerialId＝0，则取全部
        /// </summary>
        /// <param name="serialId"></param>
        public void UpdateSerialCityNews(int serialId)
        {
            Dictionary<int, City> cityDic = CityInitData.Get91CityDic();
            if (serialId > 0)
            {
                XmlDocument doc = GetCuxiaoNewsCountXml();
                Dictionary<int, int> newsCount;
                OnLog("     Get serial " + serialId + " city news(1/1)...", true);
                try
                {
                    GetSerilaCityNews(serialId, cityDic, out newsCount);
                    SetCuxiaoSerialNewsCount(doc, serialId, newsCount);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
                CommonFunction.SaveXMLDocument(doc, m_serialCuxiaoNewsCountPath);
            }
            else
            {
                XmlDocument doc = GetCuxiaoNewsCountXml();
                Dictionary<int, int> newsCount;
                //取全部子品牌的城市新闻
                List<int> serialList = GetSerialList();
                int serialCounter = 0;
                foreach (int sId in serialList)
                {
                    serialCounter++;
                    OnLog("Get city news for serial:" + sId + " (" + serialCounter + "/" + serialList.Count + ")...", false);
                    try
                    {
                        GetSerilaCityNews(sId, cityDic, out newsCount);
                        SetCuxiaoSerialNewsCount(doc, sId, newsCount);
                    }
                    catch (System.Exception ex)
                    {
                        OnLog(ex.ToString(), true);
                    }
                }
                CommonFunction.SaveXMLDocument(doc, m_serialCuxiaoNewsCountPath);
            }
        }
        /// <summary>
        /// 获取促销统计xml文件
        /// </summary>
        private XmlDocument GetCuxiaoNewsCountXml()
        {
            if (File.Exists(m_serialCuxiaoNewsCountPath))
            {
                return CommonFunction.GetXmlDocument(m_serialCuxiaoNewsCountPath);
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("root");
                xmlDoc.AppendChild(root);
                XmlDeclaration declar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.InsertBefore(declar, root);
                return xmlDoc;
            }
        }
        /// <summary>
        /// 设置子品牌促销统计
        /// </summary>
        private void SetCuxiaoSerialNewsCount(XmlDocument doc, int serialId, Dictionary<int, int> newsCount)
        {
            XmlNode serialNode = doc.SelectSingleNode(string.Format("root/cs[@id='{0}']", serialId.ToString()));
            if (serialNode == null)
            {
                serialNode = doc.CreateNode(XmlNodeType.Element, "cs", string.Empty);
                serialNode.Attributes.Append(doc.CreateAttribute("id")).Value = serialId.ToString();
                doc.DocumentElement.AppendChild(serialNode);
            }
            else
            {
                for (int i = serialNode.ChildNodes.Count - 1; i >= 0; i--)
                {
                    serialNode.RemoveChild(serialNode.ChildNodes[i]);
                }
            }

            if (newsCount != null && newsCount.Count > 0)
            {
                XmlElement city;
                int count = 0;
                foreach (KeyValuePair<int, int> cityNewsCount in newsCount)
                {
                    city = doc.CreateElement("city");
                    city.SetAttribute("id", cityNewsCount.Key.ToString());
                    city.SetAttribute("num", cityNewsCount.Value.ToString());
                    serialNode.AppendChild(city);
                    count += cityNewsCount.Value;
                }
                city = doc.CreateElement("city");
                city.SetAttribute("id", "0");
                city.SetAttribute("num", count.ToString());
                serialNode.AppendChild(city);
            }
        }
        /// <summary>
        /// 获取子品牌ID列表
        /// </summary>
        /// <returns></returns>
        public List<int> GetSerialList()
        {
            List<int> serialList = new List<int>();
            string sql = "select cs_id from dbo.Car_Serial where isstate = 0";
            try
            {
                using (DataSet ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql))
                {
                    if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
                    {
                        OnLog("子品牌列表没有取到数据", true);
                        return null;
                    }

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (serialList.Contains(ConvertHelper.GetInteger(dr["cs_id"].ToString()))) continue;
                        serialList.Add(ConvertHelper.GetInteger(dr["cs_id"].ToString()));
                    }
                    return serialList;
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.ToString(), true);
                return null;
            }
        }

        /// <summary>
        /// 获取所有级别的油耗与养车费用
        /// </summary>
        public void GetAllLeveCost()
        {
            try
            {
                CreateLevelDataPath();
                GetLevelCarCost();
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }

        /// <summary>
        /// 获取所有级别的视频
        /// </summary>
        public void GetAllLeveVideos()
        {
            try
            {
                CreateLevelDataPath();
                GetLevelVideos();
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }

        /// <summary>
        /// 获取级别的焦点，顶部，及新闻列表
        /// </summary>
        public void GetAllLevelNews()
        {
            try
            {
                CreateLevelDataPath();
                GetNewsCategorys();
                GetLeveNews();
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }

        /// <summary>
        /// 获取级别新闻
        /// </summary>
        private void GetLeveNews()
        {
            string[] levels = System.Enum.GetNames(typeof(SerialLevelEnum));
            foreach (string levelName in levels)
            {
                GetLevelAllNews(levelName);
            }
        }

        /// <summary>
        /// 获取每级别下的所有新闻
        /// </summary>
        private void GetLevelAllNews(string levelName)
        {
            OnLog("Get " + levelName + " all news...", false);
            try
            {
                //要获取的分类
                foreach (string category in m_kindCateForLevel.Keys)
                {

                    string catIdList = "";
                    foreach (int cateId in m_kindCateForLevel[category])
                    {
                        catIdList += cateId + ",";
                    }
                    catIdList = catIdList.TrimEnd(',');

                    string xmlUrl = CommonData.CommonSettings.NewsUrl + "?level=" + HttpUtility.UrlEncode(levelNameDic[levelName]) + "&nonewstype=2&getcount=500&ismain=1&categoryId=" + catIdList;

                    XmlDocument tempDoc = new XmlDocument();
                    tempDoc.Load(xmlUrl);

                    XmlNodeList newsList = tempDoc.SelectNodes("/NewDataSet/listNews");

                    //加入根分类及分类路径
                    int counter = 0;
                    List<XmlElement> tempList = new List<XmlElement>();
                    foreach (XmlElement newsNode in newsList)
                    {
                        counter++;
                        AppendNewsInfo(newsNode);
                        tempList.Add(newsNode);
                        if (tempList.Count > 10 || counter == newsList.Count)
                        {
                            AppendNewsCommentNum(tempList);
                            tempList.Clear();
                        }
                    }


                    //按分类获取新闻列表
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlElement root = xmlDoc.CreateElement("AllNews");
                    xmlDoc.AppendChild(root);
                    XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    xmlDoc.InsertBefore(xmlDeclar, xmlDoc.DocumentElement);
                    //文件名
                    string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "LevelNews\\" + category);
                    string backupPath = Path.Combine(filePath, "Backup");
                    string xmlFile = category + "_" + levelName + ".xml";
                    string backupFile = Path.Combine(backupPath, xmlFile);
                    xmlFile = Path.Combine(filePath, xmlFile);


                    //加入总数
                    XmlElement countEle = xmlDoc.CreateElement("newsAllCount");
                    XmlElement countCell = xmlDoc.CreateElement("allcount");
                    root.AppendChild(countEle);
                    countEle.AppendChild(countCell);
                    countCell.InnerText = newsList.Count.ToString();
                    //加入新闻列表
                    foreach (XmlElement newsNode in newsList)
                    {
                        root.AppendChild(xmlDoc.ImportNode(newsNode, true));
                    }

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }

        }

        /// <summary>
        /// 给新闻内容加入根分类ID及分类路径信息
        /// </summary>
        /// <param name="newsNode"></param>
        private void AppendNewsInfo(XmlElement newsNode)
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
                OnLog("AppendNewsInfo issued:CategoryId=" + cateId, true);
                OnLog(ex.ToString(), true);
            }

        }

        /// <summary>
        /// 添加新闻评论数
        /// </summary>
        /// <param name="xmlDoc"></param>
        private void AppendNewsCommentNum(List<XmlElement> newsList)
        {
            Dictionary<int, int> numDic = new Dictionary<int, int>();		//评论数字典
            List<int> newsIdList = new List<int>();
            int counter = 0;

            try
            {
                //获取评论数
                foreach (XmlElement newsNode in newsList)
                {
                    counter++;
                    int newsId = Convert.ToInt32(newsNode.SelectSingleNode("newsid").InnerText);
                    newsIdList.Add(newsId);
                    if (newsIdList.Count > 9 || counter == newsList.Count)
                    {
                        Dictionary<int, int> tDic = GetNewsCommentNum(newsIdList.ToArray());
                        foreach (int nId in tDic.Keys)
                            numDic[nId] = tDic[nId];
                        newsIdList.Clear();
                    }
                }

                //加入新闻信息
                foreach (XmlElement newsNode in newsList)
                {
                    int newsId = Convert.ToInt32(newsNode.SelectSingleNode("newsid").InnerText);
                    if (numDic.ContainsKey(newsId))
                    {
                        XmlElement commentNumNode = newsNode.OwnerDocument.CreateElement("CommentNum");
                        newsNode.AppendChild(commentNumNode);
                        commentNumNode.InnerText = numDic[newsId].ToString();
                    }
                }
            }
            catch (System.Exception ex)
            {
                OnLog("AppendNewsCommentNum issued!", true);
                OnLog(ex.ToString(), true);
            }
        }

        /// <summary>
        /// 获取新闻评论数
        /// </summary>
        /// <param name="newsIdList"></param>
        /// <returns></returns>
        private Dictionary<int, int> GetNewsCommentNum(int[] newsIdList)
        {
            Dictionary<int, int> commentNumDic = new Dictionary<int, int>();
            NewsService ns = new NewsService();
            ns.Timeout = 2000;
            try
            {
                DataTable refs = ns.SortNewsByComments(newsIdList);
                foreach (DataRow row in refs.Rows)
                {
                    int newsID = Convert.ToInt32(row["ID"]);
                    int cCount = Convert.ToInt32(row["CommentCount"]);
                    commentNumDic[newsID] = cCount;
                }
            }
            catch (System.Exception ex)
            {
                OnLog("Get news comment num error!!!", true);
            }
            return commentNumDic;
        }

        /// <summary>
        /// 获取子品牌的网友印象和优缺点
        /// modified by chengl 2015-6-2 口碑报告接口迁移
        /// </summary>
        /// <param name="serialId"></param>
        public void GetSerialKoubeiImpression(int serialId)
        {
            SerialKoubeiHtmlBuilder koubei = new SerialKoubeiHtmlBuilder();
            List<int> serialList = null;
            if (serialId == 0)
            { serialList = CommonFunction.GetSerialList(); }
            else
            {
                serialList = new List<int>();
                serialList.Add(serialId);
            }
            int counter = 0;
            foreach (int csID in serialList)
            {
                counter++;
                // string koubeiUrl = "http://koubei.bitauto.com/api/handler.ashx?apikey=d6f7bea642f24ac68a7a1206a3efb6f5&cat=reportimpression&op=get&carid={0}";
                string xmlPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialDianping\\ImpressionNew\\Xml");
                // string htmPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialDianping\\Impression\\Html");
                string xmlFile = Path.Combine(xmlPath, "Impression_{0}.xml");
                // string htmFile = Path.Combine(htmPath, "Impression_{0}.html");
                // string koubeiReportFile = Path.Combine(CommonData.CommonSettings.SavePath, "AllSerialKouBeiNumber.xml");
                if (!Directory.Exists(xmlPath))
                    Directory.CreateDirectory(xmlPath);
                //if (!Directory.Exists(htmPath))
                //	Directory.CreateDirectory(htmPath);
                try
                {
                    XmlDocument csReport = koubei.GetCsReport(csID);
                    if (csReport != null && csReport.HasChildNodes)
                    {
                        csReport.Save(String.Format(xmlFile, csID));
                    }
                    else
                    {
                        if (File.Exists(xmlFile))
                        {
                            File.Delete(xmlFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnLog("koubeiimpression:" + xmlFile + ex.ToString(), true);
                }
            }
            //List<int> serialIdList = new List<int>();
            //Dictionary<int, int> reportDic = new Dictionary<int, int>();

            ////取有口碑报告的子品牌ID
            //XmlDocument doc = new XmlDocument();
            //doc.Load(koubeiReportFile);
            //XmlNodeList serialNodeList = doc.SelectNodes("/data/serial");
            //foreach (XmlElement serialNode in serialNodeList)
            //{
            //	int csId = ConvertHelper.GetInteger(serialNode.GetAttribute("id"));
            //	if (serialId > 0 && serialId != csId)
            //		continue;
            //	reportDic[csId] = 1;
            //	serialIdList.Add(csId);
            //}



            //int counter = 0;
            //foreach (int csId in serialIdList)
            //{
            //	counter++;
            //	OnLog("     Get koubei impression " + counter + "/" + serialIdList.Count, false);
            //	try
            //	{
            //		XmlDocument imDoc = new XmlDocument();
            //		imDoc.Load(String.Format(koubeiUrl, csId));
            //		imDoc.Save(String.Format(xmlFile, csId));

            //		//生成Html
            //		KoubeiImpressionHtmlBuilder builder = new KoubeiImpressionHtmlBuilder();
            //		builder.DataXmlDocument = imDoc;
            //		builder.BuilderDataOrHtml(csId);
            //	}
            //	catch (Exception ex)
            //	{
            //		OnLog("koubeiimpression:" + xmlFile + ex.ToString(), true);
            //	}
            //}
        }
        /* //del by lsf 2016-01-06
        /// <summary>
        /// 获取所有子品牌的点评数量
        /// </summary>
        public void GetAllSerialDianpingCount()
        {
            OnLog("     Get all serial dianping number...", true);
            try
            {
                string xmlFile = "AllSerialDianpingCount.xml";
                string backupPath = Path.Combine(CommonData.CommonSettings.SavePath, "Backup");
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);
                string backupFile = Path.Combine(backupPath, xmlFile);
                xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, xmlFile);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(CommonData.CommonSettings.SerialDianpingUrl);

                CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }*/

        ///// <summary>
        ///// 获取二手车信息
        ///// </summary>
        ///// <param name="sId"></param>
        //public void GetUcarInfo(int sId)
        //{
        //    string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "UsedCarInfo\\Serial");
        //    string newsBackupPath = Path.Combine(newsPath, "Backup");
        //    if (!Directory.Exists(newsBackupPath))
        //        Directory.CreateDirectory(newsBackupPath);

        //    List<int> serialList = null;
        //    if (sId == 0)
        //        serialList = CommonFunction.GetSerialList();
        //    else
        //    {
        //        serialList = new List<int>();
        //        serialList.Add(sId);
        //    }

        //    int counter = 0;
        //    foreach (int serialId in serialList)
        //    {
        //        counter++;
        //        OnLog("     Get serial:" + serialId + " userd car information(" + counter + "/" + serialList.Count + ")...", false);
        //        try
        //        {
        //            string xmlFile = "Ucar_" + serialId + ".xml";
        //            string backupFile = Path.Combine(newsBackupPath, xmlFile);
        //            xmlFile = Path.Combine(newsPath, xmlFile);

        //            string xmlUrl = String.Format(CommonData.CommonSettings.UCarUrl, "bid", serialId, 10);
        //            XmlDocument xmlDoc = new XmlDocument();
        //            xmlDoc.Load(xmlUrl);
        //            CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
        //        }
        //        catch (System.Exception ex)
        //        {
        //            OnLog(ex.ToString(), true);
        //        }
        //    }
        //}

        /// <summary>
        /// 获取所有子品牌的论坛地址
        /// </summary>
        public void GetAllSerialForumUrl()
        {
            OnLog("     Get all serial ForumUrl...", true);
            try
            {
                string xmlFile = "CarBrandToForumUrl.xml";
                xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, xmlFile);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(CommonData.CommonSettings.SerialForumUrl);

                CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }

        /// <summary>
        /// 获取新闻分类级别列表
        /// </summary>
        private void GetNewsCategorys()
        {
            string xmlUrl = CommonData.CommonSettings.NewsUrl + "?showcategory=1";
            try
            {
                OnLog("     Get NewsCategorys......", true);
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
                OnLog("     Error issued:xmlUrl=" + xmlUrl, true);
                OnLog(ex.ToString(), true);
            }

        }

        /// <summary>
        /// 获取图片URl文件
        /// </summary>
        public void GetImgUrl()
        {
            OnLog("     Get image url...", true);

            string xmlFile = "ImageUrl.xml";

            xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, xmlFile);
            XmlDocument imgDoc = new XmlDocument();
            try
            {
                imgDoc.Load(CommonData.CommonSettings.ImageUrl);
                XmlDeclaration xmlDeclar = imgDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                imgDoc.InsertBefore(xmlDeclar, imgDoc.DocumentElement);

                CommonFunction.SaveXMLDocument(imgDoc, xmlFile);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }

        /// <summary>
        /// 获取子品牌的热点新闻
        /// </summary>
        public void GetSerialHotNews(int sId)
        {
            string newsPath = Path.Combine(m_serialDataPath, "SerialHotNews");
            string newsBackupPath = Path.Combine(newsPath, "Backup");
            if (!Directory.Exists(newsBackupPath))
                Directory.CreateDirectory(newsBackupPath);

            List<int> serialList = null;
            if (sId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(sId);
            }

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("     Get serial:" + serialId + " hot news(" + counter + "/" + serialList.Count + ")...", false);
                try
                {
                    string xmlFile = "Serial_Hot_News_" + serialId + ".xml";
                    xmlFile = Path.Combine(newsPath, xmlFile);

                    string xmlUrl = CommonData.CommonSettings.NewsUrl + "?rank=1&nonewstype=2&getcount=10&ismain=1&brandid=" + serialId;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        /// <summary>
        /// 获取所有子品牌的视频数量
        /// </summary>
        public void GetAllSerialVideoCount()
        {
            // del by chengl 接口已迁移至树形使用接口：
            // http://v.bitauto.com/restfulapi/cartype/?apikey=460ad6f3-8216-469f-9b1c-52cffa5d812c
            // data/cartree/treedata/shipin.xml

            //OnLog("     Get all serial video number...", true);
            //try
            //{
            //    string xmlFile = "SerialVideoCount.xml";
            //    xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, xmlFile);

            //    XmlDocument xmlDoc = new XmlDocument();
            //    xmlDoc.Load("http://v.bitauto.com/car/SerialCount.ashx?source=car");

            //    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            //}
            //catch (System.Exception ex)
            //{
            //    OnLog(ex.ToString(), true);
            //}
        }

        /// <summary>
        /// 获取品牌的论坛信息
        /// </summary>
        /// <param name="brandId"></param>
        public void GetBrandForumInfo(int brandId)
        {
            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "ForumInfo\\Brand");

            List<int> brandList = null;
            if (brandId == 0)
                brandList = GetBrandList();
            else
            {
                brandList = new List<int>();
                brandList.Add(brandId);
            }

            int counter = 0;
            ForumService fs = new ForumService();

            foreach (int bId in brandList)
            {
                counter++;
                OnLog("     Get Brand:" + bId + " ForumInformation(" + counter + "/" + brandList.Count + ")...", false);

                string xmlFile = "ForumInfo_Brand_" + bId + ".xml";
                xmlFile = Path.Combine(filePath, xmlFile);

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("ForumInfo");
                xmlDoc.AppendChild(root);
                XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.InsertBefore(xmlDec, root);

                //大本营
                string campUrl = "";
                try
                {
                    DataTable dt = fs.GetCampLinkBy_cb_Id("bitauto", 1, bId);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        campUrl = ConvertHelper.GetString(dt.Rows[0]["url"]);
                    }
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }

                //论坛贴子与相关论坛
                string forumsXml = "";
                string subjectsXml = "";
                try
                {
                    DataTable forumUrlTable = new DataTable("cbTable");
                    DataTable forumSubjects = fs.GetDegestTopicListBy_CbId("bitauto", bId, 5, 1, ref forumUrlTable);
                    MemoryStream xmlStream = new MemoryStream();
                    forumUrlTable.WriteXml(xmlStream, XmlWriteMode.WriteSchema);
                    xmlStream.Position = 0;
                    StreamReader sr = new StreamReader(xmlStream);
                    forumsXml = sr.ReadToEnd();
                    sr.Close();

                    xmlStream = new MemoryStream();
                    forumSubjects.WriteXml(xmlStream, XmlWriteMode.WriteSchema);
                    xmlStream.Position = 0;
                    sr = new StreamReader(xmlStream);
                    subjectsXml = sr.ReadToEnd();
                    sr.Close();
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }

                //生成Xml
                XmlElement campEle = xmlDoc.CreateElement("CampUrl");
                root.AppendChild(campEle);
                campEle.InnerText = campUrl;

                XmlElement forumsEle = xmlDoc.CreateElement("Forums");
                root.AppendChild(forumsEle);
                forumsEle.InnerXml = forumsXml;

                XmlElement subjectsEle = xmlDoc.CreateElement("Subjects");
                root.AppendChild(subjectsEle);
                subjectsEle.InnerXml = subjectsXml;

                CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            }
        }

        ///// <summary>
        ///// 获取品牌的二手车信息
        ///// </summary>
        ///// <param name="bId"></param>
        //public void GetBrandUsecar(int bId)
        //{
        //    string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "UsedCarInfo\\Brand");
        //    string newsBackupPath = Path.Combine(newsPath, "Backup");
        //    if (!Directory.Exists(newsBackupPath))
        //        Directory.CreateDirectory(newsBackupPath);
        //    List<int> brandList = null;
        //    if (bId == 0)
        //        brandList = GetBrandList();
        //    else
        //    {
        //        brandList = new List<int>();
        //        brandList.Add(bId);
        //    }
        //    int counter = 0;
        //    foreach (int brandId in brandList)
        //    {
        //        counter++;
        //        OnLog("     Get brand " + brandId + " usecar(" + counter + "/" + brandList.Count + ")...", false);
        //        try
        //        {
        //            string xmlFile = "Usecar_Brand_" + brandId + ".xml";
        //            xmlFile = Path.Combine(newsPath, xmlFile);

        //            string xmlUrl = String.Format(CommonData.CommonSettings.UCarUrl, "pids", brandId, 10);
        //            XmlDocument xmlDoc = new XmlDocument();
        //            xmlDoc.Load(xmlUrl);

        //            CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
        //        }
        //        catch (System.Exception ex)
        //        {
        //            OnLog(ex.ToString(), true);
        //        }
        //    }
        //}

        /// <summary>
        /// 获取品牌的论坛信息
        /// </summary>
        /// <param name="brandId"></param>
        public void GetMasterbrandForumInfo(int masterId)
        {
            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "ForumInfo\\MasterBrand");

            Dictionary<int, List<int>> masterDic = GetMasterBrandDic();
            if (masterId != 0 && masterDic.ContainsKey(masterId))
            {
                List<int> brandList = masterDic[masterId];
                masterDic.Clear();
                masterDic[masterId] = brandList;
            }
            else if (masterId != 0 && !masterDic.ContainsKey(masterId))
                return;

            int counter = 0;
            ForumService fs = new ForumService();

            foreach (int mId in masterDic.Keys)
            {
                counter++;
                OnLog("     Get MasterBrand:" + mId + " ForumInformation(" + counter + "/" + masterDic.Count + ")...", false);

                string xmlFile = "ForumInfo_Masterbrand_" + mId + ".xml";
                xmlFile = Path.Combine(filePath, xmlFile);

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("ForumInfo");
                xmlDoc.AppendChild(root);
                XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.InsertBefore(xmlDec, root);

                //大本营
                string campUrl = "";
                try
                {
                    DataTable dt = fs.GetCampLinkBy_bs_Id("bitauto", 1, mId);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        campUrl = ConvertHelper.GetString(dt.Rows[0]["url"]);
                    }
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }

                //论坛贴子与相关论坛
                string forumsXml = "";
                string subjectsXml = "";
                try
                {
                    DataTable forumUrlTable = new DataTable("bsTable");
                    DataTable forumSubjects = fs.GetDegestTopicListBy_BsId("bitauto", mId, 5, 1, ref forumUrlTable);
                    MemoryStream xmlStream = new MemoryStream();
                    forumUrlTable.WriteXml(xmlStream, XmlWriteMode.WriteSchema);
                    xmlStream.Position = 0;
                    StreamReader sr = new StreamReader(xmlStream);
                    forumsXml = sr.ReadToEnd();
                    sr.Close();

                    xmlStream = new MemoryStream();
                    forumSubjects.WriteXml(xmlStream, XmlWriteMode.WriteSchema);
                    xmlStream.Position = 0;
                    sr = new StreamReader(xmlStream);
                    subjectsXml = sr.ReadToEnd();
                    sr.Close();
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }

                //生成Xml
                XmlElement campEle = xmlDoc.CreateElement("CampUrl");
                root.AppendChild(campEle);
                campEle.InnerText = campUrl;

                XmlElement forumsEle = xmlDoc.CreateElement("Forums");
                root.AppendChild(forumsEle);
                forumsEle.InnerXml = forumsXml;

                XmlElement subjectsEle = xmlDoc.CreateElement("Subjects");
                root.AppendChild(subjectsEle);
                subjectsEle.InnerXml = subjectsXml;

                CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            }
        }

        ///// <summary>
        ///// 获取主品牌二手车信息
        ///// </summary>
        ///// <param name="masterId"></param>
        //public void GetMasterbrandUsecar(int mId)
        //{
        //    string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "UsedCarInfo\\Masterbrand");
        //    string newsBackupPath = Path.Combine(newsPath, "Backup");
        //    if (!Directory.Exists(newsBackupPath))
        //        Directory.CreateDirectory(newsBackupPath);

        //    Dictionary<int, List<int>> masterDic = GetMasterBrandDic();
        //    if (mId != 0 && masterDic.ContainsKey(mId))
        //    {
        //        List<int> brandList = masterDic[mId];
        //        masterDic.Clear();
        //        masterDic[mId] = brandList;
        //    }
        //    else if (mId != 0 && !masterDic.ContainsKey(mId))
        //        return;

        //    int counter = 0;
        //    foreach (int masterId in masterDic.Keys)
        //    {
        //        counter++;
        //        OnLog("     Get master brand:" + masterId + " usecar(" + counter + "/" + masterDic.Count + ")...", false);
        //        try
        //        {
        //            string brandIdList = "";
        //            foreach (int brandId in masterDic[masterId])
        //            {
        //                brandIdList += brandId + ",";
        //            }
        //            brandIdList = brandIdList.TrimEnd(new char[] { ',' });

        //            string xmlFile = "Usecar_Masterbrand_" + masterId + ".xml";
        //            xmlFile = Path.Combine(newsPath, xmlFile);

        //            string xmlUrl = String.Format(CommonData.CommonSettings.UCarUrl, "pids", brandIdList, 10);
        //            XmlDocument xmlDoc = new XmlDocument();
        //            xmlDoc.Load(xmlUrl);

        //            CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
        //        }
        //        catch (System.Exception ex)
        //        {
        //            OnLog(ex.ToString(), true);
        //        }
        //    }
        //}

        /// <summary>
        /// 获取子品牌答疑
        /// </summary>
        public void GetSerialAskEntries(int sId)
        {
            string newsPath = Path.Combine(m_serialDataPath, "AskEntries");
            string newsBackupPath = Path.Combine(newsPath, "Backup");
            if (!Directory.Exists(newsBackupPath))
                Directory.CreateDirectory(newsBackupPath);

            List<int> serialList = null;
            if (sId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(sId);
            }

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("     Get serial:" + serialId + " ask entries(" + counter + "/" + serialList.Count + ")...", false);
                try
                {
                    string xmlFile = "Serial_Ask_" + serialId + ".xml";
                    xmlFile = Path.Combine(newsPath, xmlFile);

                    string xmlUrl = String.Format(CommonData.CommonSettings.AskEntriesUrl, "brandid", serialId, 3);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        public void GetAllCarFuel()
        {
            string xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, @"Koubei/AllCarFuelV2.xml");
            //XDocument xDoc = new XDocument(
            //	new XElement("Root")
            //	);
            string result = string.Empty;
            Common.Log.WriteLog("生成网友口碑数据开始");
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                //fuelDoc.Load(CommonData.CommonSettings.AllCarFuel);
                var declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
                xmlDoc.AppendChild(declaration);
                XmlNode rootNode = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootNode);
                foreach (int serialId in CommonData.SerialDic.Keys)
                {
                    var serialXml = CommonFunction.GetXmlDocument(string.Format(CommonData.CommonSettings.UserFuelUrl, DateTime.Now.ToString("yyyyMM"), serialId), true);
                    if (serialXml != null)
                    {
                        xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(serialXml.DocumentElement, true));
                    }
                }
                CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
            Common.Log.WriteLog("生成网友口碑数据结束");
        }

        /// <summary>
        /// 生成根据CategoryID查询新闻的表达式
        /// </summary>
        /// <param name="cateIds"></param>
        /// <returns></returns>
        private string GetCategoryXmlPath(int[] cateIds)
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
        /// 按子品牌ID及城市ID获取所有新闻
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="cityId"></param>
        /// <returns></returns>
        private XmlDocument GetSerialNewsForCity(int serialId, int cityId)
        {
            string xmlUrl = CommonData.CommonSettings.NewsUrl + "?nonewstype=2&getcount=500&brandid=" + serialId + "&cityid=" + cityId;// +"&categoryid=" + catId;
            XmlDocument newsDoc = new XmlDocument();
            newsDoc.Load(xmlUrl);
            XmlNodeList newsList = newsDoc.SelectNodes("/NewDataSet/listNews");
            foreach (XmlElement newsNode in newsList)
            {
                AppendNewsInfo(newsNode);
            }
            return newsDoc;
        }

        public void StatisticsCityNews(int sId)
        {
            string strStatistics = "子品牌ID,子品牌名称,";
            XmlDocument cityDoc = new XmlDocument();
            string cityFile = Path.Combine(CommonData.CommonSettings.SavePath, "city.xml");
            cityDoc.Load(cityFile);

            XmlNodeList cityList = cityDoc.SelectNodes("/CityValueSet/CityItem");
            List<int> cityIdList = new List<int>();
            //城市列表
            foreach (XmlElement cityNode in cityList)
            {
                int cityId = Convert.ToInt32(cityNode.SelectSingleNode("CityId").InnerText);
                string cityName = cityNode.SelectSingleNode("CityName").InnerText;
                int cityLevel = Convert.ToInt32(cityNode.SelectSingleNode("Level").InnerText);
                if (cityLevel == 1)
                {
                    cityIdList.Add(cityId);
                    strStatistics += cityName + ",";
                }
            }
            strStatistics += "\r\n";
            File.AppendAllText(@"d:\SerialCityNewsNum.csv", strStatistics, Encoding.Default);
            //按子品牌取数据
            string xml = CommonData.CommonSettings.AutoDataUrl;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml);
            XmlNodeList serialList = xmlDoc.SelectNodes("/Params/MasterBrand/Brand/Serial");
            string xmlPath = GetCategoryXmlPath(new int[] { 16, 215, 3 });
            int counter = 0;
            foreach (XmlElement serialNode in serialList)
            {
                int serialId = Convert.ToInt32(serialNode.GetAttribute("ID"));
                if (sId != 0 && sId != serialId)
                    continue;
                counter++;
                OnLog("     Get num (" + counter + "/" + serialList.Count + ")...", false);
                string showName = serialNode.GetAttribute("ShowName");
                strStatistics = serialId + "," + showName + ",";
                //XmlDocument newsDoc = GetSerialNewsForAllCity(serialId);
                foreach (int cityId in cityIdList)
                {
                    XmlDocument newsDoc = GetSerialNewsForCity(serialId, cityId);
                    string cityXmlPath = xmlPath;
                    //string cityXmlPath = "(" + xmlPath + ") and contains(relatedcityid,\"," + cityId + ",\")";
                    try
                    {
                        XmlNodeList cityNewsList = newsDoc.SelectNodes("/NewDataSet/listNews[" + cityXmlPath + "]");
                        strStatistics += cityNewsList.Count + ",";
                    }
                    catch
                    {
                        strStatistics += "err,";
                    }
                }

                strStatistics += "\r\n";
                File.AppendAllText(@"d:\SerialCityNewsNum.csv", strStatistics, Encoding.Default);
                if (sId != 0)
                    break;
            }

        }

        /// <summary>
        /// 得到用于Tree的子品牌服务
        /// wzht
        /// </summary>
        public void GetSerialsNewsCountInTree()
        {
            List<int> serialList = GetSerialList();
            Dictionary<int, List<int>> brandToSerial = GetBrandSerialDic();
            Dictionary<int, List<int>> masterToBrand = GetMasterBrandDic();
            string[] IsNeedNumCategory = new string[] { "daogou", "treepingce", "hangqing", "anquan", "keji" };
            Dictionary<int, int> serialNumList = new Dictionary<int, int>();
            Dictionary<string, Dictionary<int, NewsNum>> newCategory = new Dictionary<string, Dictionary<int, NewsNum>>();

            #region <!--GetSeriaNumber-->
            foreach (string newNumEntity in IsNeedNumCategory)
            {
                if (newCategory.ContainsKey(newNumEntity)) continue;
                serialNumList = new Dictionary<int, int>();
                string categoryId = joinStringArray(CommonData.KindCatesForSerial[newNumEntity]);
                //得到子品牌数量
                int counter = 0;
                foreach (int serialentity in serialList)
                {
                    counter++;
                    OnLog("     Get Serial " + serialentity + " news count" + counter + "/" + serialList.Count + ")...", false);
                    string xmlUrl = string.Format("{0}?brandid={1}&nonewstype=2&getcount=1&ismain=1&categoryId={2}"
                                                  , CommonData.CommonSettings.NewsUrl
                                                  , serialentity
                                                  , categoryId);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);
                    XmlNode xNode = xmlDoc.SelectSingleNode("NewDataSet/newsAllCount/allcount");
                    if (xNode == null || serialNumList.ContainsKey(serialentity))
                    {
                        continue;
                    }
                    serialNumList.Add(serialentity, ConvertHelper.GetInteger(xNode.InnerText.ToString().Trim()));
                }
                //得到品牌数量
                Dictionary<int, NewsNum> brandNewsNumList = new Dictionary<int, NewsNum>();
                foreach (KeyValuePair<int, List<int>> entity in brandToSerial)
                {
                    if (brandNewsNumList.ContainsKey(entity.Key)) continue;
                    NewsNum nn = new NewsNum();
                    Dictionary<int, NewsNum> newsNumList = new Dictionary<int, NewsNum>();
                    foreach (int serialid in entity.Value)
                    {
                        if (!serialNumList.ContainsKey(serialid)) continue;
                        if (newsNumList.ContainsKey(serialid)) continue;
                        NewsNum serialNum = new NewsNum();
                        serialNum.Number = serialNumList[serialid];
                        nn.Number += serialNum.Number;
                        newsNumList.Add(serialid, serialNum);
                    }
                    nn.newsNumList = newsNumList;

                    brandNewsNumList.Add(entity.Key, nn);
                }
                //得到主品牌数量
                Dictionary<int, NewsNum> masterBrandNewsNumList = new Dictionary<int, NewsNum>();
                foreach (KeyValuePair<int, List<int>> entity in masterToBrand)
                {
                    if (masterBrandNewsNumList.ContainsKey(entity.Key)) continue;
                    NewsNum nn = new NewsNum();
                    Dictionary<int, NewsNum> newsNumList = new Dictionary<int, NewsNum>();
                    foreach (int brandId in entity.Value)
                    {
                        if (!brandNewsNumList.ContainsKey(brandId) || newsNumList.ContainsKey(brandId)) continue;
                        nn.Number += brandNewsNumList[brandId].Number;
                        newsNumList.Add(brandId, brandNewsNumList[brandId]);
                    }
                    nn.newsNumList = newsNumList;
                    masterBrandNewsNumList.Add(entity.Key, nn);
                }
                newCategory.Add(newNumEntity, masterBrandNewsNumList);
            }
            #endregion

            #region <!--BuilderSerialNumber-->
            XmlDocument newsXmlDoc = new XmlDocument();
            XmlElement rootElement = newsXmlDoc.CreateElement("root");

            foreach (KeyValuePair<int, List<int>> bsEntity in masterToBrand)
            {
                //生成主品牌结点
                XmlElement masterBrandNode = newsXmlDoc.CreateElement("MasterBrand");
                masterBrandNode.SetAttribute("ID", bsEntity.Key.ToString());
                foreach (string catetory in IsNeedNumCategory)
                {
                    int newsNums = 0;
                    if (newCategory.ContainsKey(catetory)
                        && newCategory[catetory] != null
                        && newCategory[catetory].ContainsKey(bsEntity.Key)
                        && newCategory[catetory][bsEntity.Key] != null)
                    {
                        newsNums = newCategory[catetory][bsEntity.Key].Number;
                    }
                    masterBrandNode.SetAttribute(catetory, newsNums.ToString());
                }
                //生成主品牌结点
                foreach (int brandId in bsEntity.Value)
                {
                    XmlElement brandNode = newsXmlDoc.CreateElement("Brand");
                    brandNode.SetAttribute("ID", brandId.ToString());
                    foreach (string catetory in IsNeedNumCategory)
                    {
                        int newsNums = 0;
                        if (newCategory.ContainsKey(catetory)
                           && newCategory[catetory] != null
                           && newCategory[catetory].ContainsKey(bsEntity.Key)
                           && newCategory[catetory][bsEntity.Key] != null
                           && newCategory[catetory][bsEntity.Key].newsNumList.ContainsKey(brandId)
                           && newCategory[catetory][bsEntity.Key].newsNumList[brandId] != null)
                        {
                            newsNums = newCategory[catetory][bsEntity.Key].newsNumList[brandId].Number;
                        }
                        brandNode.SetAttribute(catetory, newsNums.ToString());
                    }

                    if (!brandToSerial.ContainsKey(brandId))
                    {
                        masterBrandNode.AppendChild(brandNode);
                        continue;
                    }

                    //生成子品牌结点
                    foreach (int serialId in brandToSerial[brandId])
                    {
                        XmlElement serialNode = newsXmlDoc.CreateElement("Serial");
                        serialNode.SetAttribute("ID", serialId.ToString());
                        foreach (string catetory in IsNeedNumCategory)
                        {
                            int newsNums = 0;
                            if (newCategory.ContainsKey(catetory)
                               && newCategory[catetory] != null
                               && newCategory[catetory].ContainsKey(bsEntity.Key)
                               && newCategory[catetory][bsEntity.Key] != null
                               && newCategory[catetory][bsEntity.Key].newsNumList.ContainsKey(brandId)
                               && newCategory[catetory][bsEntity.Key].newsNumList[brandId] != null
                               && newCategory[catetory][bsEntity.Key].newsNumList[brandId].newsNumList.ContainsKey(serialId)
                               && newCategory[catetory][bsEntity.Key].newsNumList[brandId].newsNumList[serialId] != null)
                            {
                                newsNums = newCategory[catetory][bsEntity.Key].newsNumList[brandId].newsNumList[serialId].Number;
                            }
                            serialNode.SetAttribute(catetory, newsNums.ToString());
                        }


                        brandNode.AppendChild(serialNode);
                    }

                    masterBrandNode.AppendChild(brandNode);
                }


                rootElement.AppendChild(masterBrandNode);
            }
            newsXmlDoc.AppendChild(rootElement);
            newsXmlDoc.Save(Path.Combine(m_serialDataPath, "treenewscount.xml"));
            #endregion
        }

        /// <summary>
        /// 添加字符串数组
        /// </summary>
        /// <param name="isJoinString"></param>
        /// <returns></returns>
        private string joinStringArray(int[] isJoinString)
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

        #region 品牌树
        /// <summary>
        /// 更新车型品牌树
        /// </summary>
        public void UpdateBrandTree()
        {
            OnLog("Get brand tree start...", true);

            com.bitauto.carser.Service service = new com.bitauto.carser.Service();

            string outSourceXml = string.Empty;
            string updateTime = service.GetMasterToBrandToSerialUpdateTime(out outSourceXml); //autodata.xml

            UpdateBrandTree(Path.Combine(CommonData.CommonSettings.SavePath, "autodata.xml")
                , outSourceXml, updateTime);

            updateTime = service.GetMasterToBrandToSerialAllSaleUpdateTime(out outSourceXml);//AllAutoData.xml
            UpdateBrandTree(Path.Combine(CommonData.CommonSettings.SavePath, "AllAutoData.xml")
                , outSourceXml, updateTime);

            updateTime = service.GetMasterToBrandToSerialAllSaleAndLevelUpdateTime(out outSourceXml);//MasterToBrandToSerialAllSaleAndLevel.xml
            UpdateBrandTree(Path.Combine(CommonData.CommonSettings.SavePath, "MasterToBrandToSerialAllSaleAndLevel.xml")
                , outSourceXml, updateTime);

            // add by chengl Apr.21.2014
            // allSpell重写文件更新
            updateTime = service.GetAllSpellForMasterToCar(out outSourceXml);// AllSpellMasterToCar.xml
            UpdateBrandTree(Path.Combine(CommonData.CommonSettings.SavePath, "AllSpellMasterToCar.xml")
                , outSourceXml, updateTime);
            // 同时更新特殊跳转子品牌规则 SpecialNecessaryMatch.xml
            outSourceXml = outSourceXml.ToLower().Replace("allspellmastertocar.xml", "SpecialNecessaryMatch.xml");
            UpdateBrandTree(Path.Combine(CommonData.CommonSettings.SavePath, "SpecialNecessaryMatch.xml")
                , outSourceXml, updateTime);

            OnLog("Get brand tree end...", true);
        }
        /// <summary>
        /// 更新品牌树xml
        /// </summary>
        private void UpdateBrandTree(string targetPath, string sourcePath, string updateTime)
        {
            OnLog(string.Format("Get brand tree start...msg[target:{0}, source:{1}, updateTime:{2}]", targetPath, sourcePath, updateTime), true);

            bool isUpdate = true;
            DateTime sUpdateTime = ConvertHelper.GetDateTime(updateTime);
            try
            {
                if (File.Exists(targetPath))
                {
                    try
                    {
                        using (XmlReader reader = XmlReader.Create(targetPath))
                        {
                            if (reader.ReadToFollowing("Params")
                                && reader.HasAttributes
                                && reader["Time"] != null)
                            {
                                if (ConvertHelper.GetDateTime(reader["Time"]) >= sUpdateTime)
                                {
                                    isUpdate = false;
                                    OnLog(string.Format("updateTime Equals!msg[isUpdate:false,target:{0},source:{1}]", reader["Time"], updateTime), true);
                                }
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception exp)
                    {
                        OnLog("read targetXml error! errorMsg:" + exp.ToString(), true);
                        Common.Log.WriteErrorLog(exp);
                    }
                }

                if (isUpdate)
                {
                    OnLog("source save to target start...", true);

                    CommonFunction.SaveXMLDocument(sourcePath, targetPath);

                    OnLog("source save to target end...", true);
                }
            }
            catch (Exception exp)
            {
                OnLog(exp.ToString(), true);
                Common.Log.WriteErrorLog(exp);
            }

            OnLog(string.Format("Get brand tree end...msg[target:{0}, source:{1}, updateTime:{2}]", targetPath, sourcePath, updateTime), true);
        }

        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
        private List<int> GetBrandList()
        {
            List<int> brandList = new List<int>();
            string autoDataFile = Path.Combine(CommonData.CommonSettings.SavePath, "autodata.xml");
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
        /// 获取品牌中的子品牌的字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, List<int>> GetBrandSerialDic()
        {
            List<int> brandList = GetBrandList();
            Dictionary<int, List<int>> brandDic = new Dictionary<int, List<int>>();

            string autoDataFile = Path.Combine(CommonData.CommonSettings.SavePath, "AllAutoData.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(autoDataFile);

            foreach (int brandId in brandList)
            {
                XmlNodeList serialNodeList = xmlDoc.SelectNodes("/Params/MasterBrand/Brand[@ID=\"" + brandId + "\"]/Serial");
                List<int> serialList = new List<int>();
                foreach (XmlElement serialNode in serialNodeList)
                {
                    int serialId = Convert.ToInt32(serialNode.GetAttribute("ID"));
                    serialList.Add(serialId);
                }
                brandDic[brandId] = serialList;
            }

            return brandDic;
        }

        /// <summary>
        /// 获取每个主品牌包括品牌的列表字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, List<int>> GetMasterBrandDic()
        {
            Dictionary<int, List<int>> masterDic = new Dictionary<int, List<int>>();
            string autoDataFile = Path.Combine(CommonData.CommonSettings.SavePath, "autodata.xml");
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

        #endregion

        #region 获取子品牌城市新闻
        /// <summary>
        /// 获取城市列表
        /// </summary>
        /// <returns></returns>
        public void UpdateCityList()
        {
            OnLog("     Update citylist", true);
            try
            {
                XmlDocument cityDoc = new XmlDocument();
                cityDoc.Load(CommonData.CommonSettings.CityListUrl);
                //存储城市
                string cityFile = Path.Combine(CommonData.CommonSettings.SavePath, "city.xml");
                CommonFunction.SaveXMLDocument(cityDoc, cityFile);

                //存储城市到30个中心城的关联
                cityDoc.Load("http://api.admin.bitauto.com/api/common/cityvalueset.aspx?type=citymapping");
                cityFile = Path.Combine(CommonData.CommonSettings.SavePath, "citymapping.xml");
                CommonFunction.SaveXMLDocument(cityDoc, cityFile);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }
        #endregion

        #region 级别页新闻

        /// <summary>
        /// 创建级别数据目录
        /// </summary>
        private void CreateLevelDataPath()
        {
            if (!Directory.Exists(m_levelDataPath))
                Directory.CreateDirectory(m_levelDataPath);
            if (!Directory.Exists(m_levelBackupPath))
                Directory.CreateDirectory(m_levelBackupPath);
        }

        /// <summary>
        /// 获取每个级别的视频，存储为一个文件
        /// </summary>
        private void GetLevelVideos()
        {
            string xmlFile = "LevelVideo.xml";
            xmlFile = Path.Combine(m_levelDataPath, xmlFile);

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(root);
            XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.InsertBefore(xmlDeclar, root);

            string[] levels = System.Enum.GetNames(typeof(SerialLevelEnum));
            foreach (string levelName in levels)
            {
                OnLog("     Get level:" + levelName + " video...", false);
                XmlElement levelRoot = xmlDoc.CreateElement("Level");
                levelRoot.SetAttribute("name", levelName);
                root.AppendChild(levelRoot);

                try
                {
                    string xmlUrl = CommonData.CommonSettings.NewsUrl + "?level=" + HttpUtility.UrlEncode(levelNameDic[levelName]) + "&getcount=10&articaltype=3";
                    XmlDocument videoDoc = new XmlDocument();
                    videoDoc.Load(xmlUrl);

                    XmlNodeList videoList = videoDoc.SelectNodes("/NewDataSet/listNews");

                    foreach (XmlElement videoNode in videoList)
                    {
                        levelRoot.AppendChild(xmlDoc.ImportNode(videoNode, true));
                    }
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }

            CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
        }

        /// <summary>
        /// 获取级别新闻的油耗与养车费用
        /// </summary>
        private void GetLevelCarCost()
        {
            int[] levelIds = (int[])Enum.GetValues(typeof(SerialLevelEnum));
            string[] dataTypes = new string[] { "fuel", "fee" };
            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "LevelNews\\CarCost");
            foreach (int levelId in levelIds)
            {
                string levelName = ((SerialLevelEnum)levelId).ToString();
                OnLog("     Get level " + levelName + " Carcost...", false);
                try
                {
                    int fId = carCostLevelDic[levelId];
                    foreach (string dataType in dataTypes)
                    {
                        //文件名
                        string xmlFile = "Level_" + dataType + "_CarCost_" + levelName + ".xml";
                        xmlFile = Path.Combine(filePath, xmlFile);

                        string fuelUrl = CommonData.CommonSettings.LevelCarCostUrl + "?type=" + dataType + "&category=" + fId;
                        try
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.Load(fuelUrl);
                            CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                        }
                        catch { }
                    }
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }
        #endregion

        #region 子品牌综述新闻与图片

        /// <summary>
        /// 创建子品牌文件目录
        /// </summary>
        private void CreateSerialDataPath()
        {
            if (!Directory.Exists(m_serialBackupPath))
                Directory.CreateDirectory(m_serialBackupPath);
        }

        /// <summary>
        /// 获取子品牌的焦点图片
        /// </summary>
        public void GetSerialFocusImage(int sId)
        {
            //按子品牌取焦点图片
            string focusImgPath = Path.Combine(m_serialDataPath, "FocusImageNew");
            string focusImgBackupPath = Path.Combine(focusImgPath, "Backup");
            if (!Directory.Exists(focusImgBackupPath))
                Directory.CreateDirectory(focusImgBackupPath);

            List<int> serialList = null;
            if (sId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(sId);
            }

            //按子品牌取焦点图片
            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("     Get serial:" + serialId + " focus images(" + counter + "/" + serialList.Count + ")...", false);
                try
                {
                    string xmlFile = "Serial_FocusImage_" + serialId + ".xml";
                    xmlFile = Path.Combine(focusImgPath, xmlFile);

                    string xmlUrl = String.Format(CommonData.CommonSettings.SerialFocusImageUrl, serialId);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);

                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        public void GetSerialForumSubject(int sId)
        {
            List<int> serialList = null;
            if (sId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(sId);
            }

            int sCounter = 0;
            foreach (int serialId in serialList)
            {
                sCounter++;
                OnLog("     Get serial:" + serialId + " forumSubject(" + sCounter + "/" + serialList.Count + ")...", false);
                try
                {
                    // modified by chengl Aug.16.2013
                    // 论坛数据单独存储
                    string focusNewsPath = Path.Combine(m_serialDataPath, "Forum");
                    string focusNewsBackupPath = Path.Combine(focusNewsPath, "Backup");
                    if (!Directory.Exists(focusNewsBackupPath))
                        Directory.CreateDirectory(focusNewsBackupPath);
                    string xmlFile = serialId + ".xml";
                    xmlFile = Path.Combine(focusNewsPath, xmlFile);

                    XmlDocument xmlFoucsDoc = new XmlDocument();
                    if (File.Exists(xmlFile))
                        xmlFoucsDoc.Load(xmlFile);

                    XmlElement root = xmlFoucsDoc.DocumentElement;
                    if (root == null)
                    {
                        root = xmlFoucsDoc.CreateElement("root");
                        xmlFoucsDoc.AppendChild(root);
                        XmlDeclaration xmlDeclar = xmlFoucsDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                        xmlFoucsDoc.InsertBefore(xmlDeclar, root);
                    }


                    XmlElement forumRoot = (XmlElement)root.SelectSingleNode("Forum");
                    if (forumRoot == null)
                    {
                        forumRoot = xmlFoucsDoc.CreateElement("Forum");
                        root.AppendChild(forumRoot);
                    }
                    else
                    {
                        forumRoot.RemoveAll();
                    }

                    Dictionary<int, TopicImgsInfo> dicTopicImg = new Dictionary<int, TopicImgsInfo>();
                    DataTable forumDatatable = GetForumSubjectBySerial(serialId, ref dicTopicImg);
                    foreach (DataRow row in forumDatatable.Rows)
                    {
                        int tid = Convert.ToInt32(row["tid"]);
                        string title = Convert.ToString(row["title"]);
                        string poster = Convert.ToString(row["poster"]);
                        string url = Convert.ToString(row["url"]);
                        int digest = Convert.ToInt32(row["digest"]);
                        // add by zhangll Aug.18.2014
                        int replies = Convert.ToInt32(row["replies"]);
                        // add by chengl Jun.15.2012
                        string postdatetime = Convert.ToString(row["postdatetime"]);
                        XmlElement forumNode = xmlFoucsDoc.CreateElement("ForumSubject");
                        forumRoot.AppendChild(forumNode);
                        XmlElement idNode = xmlFoucsDoc.CreateElement("tid");
                        forumNode.AppendChild(idNode);
                        idNode.InnerText = tid.ToString();
                        XmlElement titleNode = xmlFoucsDoc.CreateElement("title");
                        forumNode.AppendChild(titleNode);
                        titleNode.InnerText = title;
                        XmlElement urlNode = xmlFoucsDoc.CreateElement("url");
                        forumNode.AppendChild(urlNode);
                        urlNode.InnerText = url;
                        XmlElement digestNode = xmlFoucsDoc.CreateElement("digest");
                        forumNode.AppendChild(digestNode);
                        digestNode.InnerText = digest.ToString();
                        // add by zhangll Aug.18.2014
                        XmlElement repliesNode = xmlFoucsDoc.CreateElement("replies");
                        forumNode.AppendChild(repliesNode);
                        repliesNode.InnerText = replies.ToString();
                        XmlElement posterNode = xmlFoucsDoc.CreateElement("poster");
                        forumNode.AppendChild(posterNode);
                        posterNode.InnerText = poster;

                        // add by chengl Jun.15.2012
                        XmlElement postdatetimeNode = xmlFoucsDoc.CreateElement("postdatetime");
                        forumNode.AppendChild(postdatetimeNode);
                        postdatetimeNode.InnerText = postdatetime;

                        // add by chengl Jun.23.2015 MD 3年了 
                        XmlElement imgListNode = xmlFoucsDoc.CreateElement("imgList");
                        forumNode.AppendChild(imgListNode);
                        List<string> listTempImg = new List<string>();
                        if (dicTopicImg.Count > 0 && dicTopicImg.ContainsKey(tid))
                        {
                            if (dicTopicImg[tid].ImgCount > 0 && dicTopicImg[tid].ImgList.Length > 0)
                            {
                                int loopImg = 0;
                                foreach (cn.com.baa.api.AttachmentSimple asImg in dicTopicImg[tid].ImgList)
                                {
                                    loopImg++;
                                    if (loopImg > 5)
                                    { break; }
                                    listTempImg.Add(string.Format("<img>{0}</img>", System.Security.SecurityElement.Escape(asImg.SmallUrl)));
                                }
                            }
                        }
                        imgListNode.InnerXml = string.Join("", listTempImg.ToArray());
                    }
                    CommonFunction.SaveXMLDocument(xmlFoucsDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog("Get serial " + serialId + " focus news...", true);
                    OnLog(ex.ToString(), true);
                }
            }

        }

        /// <summary>
        /// 获取品牌的图片
        /// </summary>
        /// <param name="brandId"></param>
        public void GetBrandImages(int brandId)
        {
            string xmlPath = Path.Combine(CommonData.CommonSettings.SavePath, "BrandImage");
            string backPath = Path.Combine(xmlPath, "Backup");
            Dictionary<int, List<int>> brandDic = GetBrandSerialDic();

            Dictionary<int, List<int>> tempDic = null;
            if (brandId == 0)
                tempDic = brandDic;
            else
            {
                tempDic = new Dictionary<int, List<int>>();
                if (brandDic.ContainsKey(brandId))
                    tempDic[brandId] = brandDic[brandId];
            }

            int counter = 0;
            foreach (int bId in tempDic.Keys)
            {
                counter++;
                OnLog("     Get Brand:" + bId + " images(" + counter + "/" + tempDic.Count + ")...", false);
                string xmlFile = Path.Combine(xmlPath, "BrandImage_" + bId + ".xml");
                List<int> serialList = tempDic[bId];
                string idList = "";
                foreach (int serialId in serialList)
                {
                    idList += serialId + ",";
                }
                idList = idList.TrimEnd(',');

                string xmlUrl = String.Format(CommonData.CommonSettings.BrandImageUrl, idList);

                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        #endregion

        #region 统计子品牌新闻数量
        /// <summary>
        /// 获取所有子品牌的计划购买的人
        /// </summary>
        public void GetSerialsIntensionPersion()
        {

            string xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, "SerialIntensionUsers.xml");

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("Serials");
            xmlDoc.AppendChild(root);
            XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.InsertBefore(xmlDeclar, root);

            string baseUrl = "http://go.bitauto.com/api/HandlerIntensionMember.ashx?modelid={0}&intensiontype=1";

            List<int> serialList = CommonFunction.GetSerialList();

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;

                string logText = "getting the Serial Intension Users (" + counter + "/" + serialList.Count + ")";
                OnLog(logText, false);
                XmlElement serialNode = xmlDoc.CreateElement("Serial");
                serialNode.SetAttribute("ID", serialId.ToString());
                root.AppendChild(serialNode);
                try
                {
                    XmlDocument tmpDoc = new XmlDocument();
                    tmpDoc.Load(String.Format(baseUrl, serialId));
                    XmlNodeList personNodeList = tmpDoc.SelectNodes("/result/user");
                    foreach (XmlElement userNode in personNodeList)
                    {
                        serialNode.AppendChild(xmlDoc.ImportNode(userNode, true));
                    }
                }
                catch { }
            }

            CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
        }
        #endregion

        public void GenerateSerialKoubeiHtml(int sId)
        {
            List<int> serialList = null;
            if (sId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(sId);
            }
            SerialKoubeiHtmlBuilder koubei = new SerialKoubeiHtmlBuilder();
            foreach (int serialId in serialList)
            {
                Common.Log.WriteLog("更新子品牌口碑块 start serialId=" + serialId);
                koubei.BuilderDataOrHtml(serialId);
                Common.Log.WriteLog("更新子品牌口碑块 start serialId=" + serialId);
            }
        }
		/*
        /// <summary>
        /// 生成综述页竞品口碑排名
        /// </summary>
        /// <param name="sId"></param>
        public void GenerateSerialCompetitiveKoubeiHtml(int sId)
        {
            List<int> serialList = null;
            if (sId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(sId);
            }
            SerialCompetitiveKoubeiHtmlBuilder koubei = new SerialCompetitiveKoubeiHtmlBuilder(null);
            foreach (int serialId in serialList)
            {
                Common.Log.WriteLog("更新子品牌竞品口碑排名块 start serialId=" + serialId);
                koubei.BuilderDataOrHtml(serialId);
                //koubei.BuilderDataOrHtmlV2(serialId);//20160829
                Common.Log.WriteLog("更新子品牌竞品口碑排名块 start serialId=" + serialId);
            }
        }*/
        public void GenerateCarKoubeiHtml(int carId)
        {
            Dictionary<int, CarEntity> carEntities;
            if (carId == 0)
                carEntities = CommonFunction.GetCarList();
            else
            {
                var carEntity = CommonFunction.GetCarEntity(carId);
                carEntities = new Dictionary<int, CarEntity>();
                carEntities.Add(carId, carEntity);
            }
            var koubei = new CarSummaryKouBei();
            Common.Log.WriteLog("更新车款口碑块 开始");
            foreach (var carEntityModel in carEntities)
            {
				if (!CommonData.SerialDic.ContainsKey(carEntityModel.Value.CsId))
				{
					continue;
				}
                Common.Log.WriteLog("更新车款口碑块 start carId=" + carEntityModel.Key);
                koubei.Generate(carEntityModel.Value.CsId, carEntityModel.Value.CarId);
                Common.Log.WriteLog("更新车款口碑块 end carId=" + carEntityModel.Key);
            }
            Common.Log.WriteLog("更新车款口碑块 结束");
        }

        #region 子品牌视频
        /// <summary>
        /// 获取所有子品牌的视频
        /// </summary>
        public void GetSerialVideo(int sId)
        {
            OnLog("     Get serial video...", true);
            List<int> serialList = null;
            if (sId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(sId);
            }
            #region modified by sk 2013-09-12 废弃旧版视频
            /*
 			string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\SerialVideo");
			SerialVideoHtmlBuilder serialVideoHtml = new SerialVideoHtmlBuilder();

			Dictionary<int, int> videoNumDic = new Dictionary<int, int>();
			int counter = 0;
			foreach (int serialId in serialList)
			{
				counter++;
				OnLog("Get serial " + serialId + " video(" + counter + "/" + serialList.Count + ")...", false);
				string xmlUrl = CommonData.CommonSettings.NewsUrl + "?articaltype=3&getcount=4&brandid=" + serialId;
				string xmlFile = "Serial_Video_" + serialId + ".xml";
				xmlFile = Path.Combine(newsPath, xmlFile);

				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(xmlUrl);

					//取视频总数
					XmlNode numNode = xmlDoc.SelectSingleNode("/NewDataSet/newsAllCount/allcount");
					if (numNode != null)
					{
						int num = ConvertHelper.GetInteger(numNode.InnerText);
						if (num > 0)
							videoNumDic[serialId] = num;
					}
 					CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
					//生成子品牌综述页视频块
					serialVideoHtml.BuilderDataOrHtml(serialId);
				}
				catch (System.Exception ex)
				{
					OnLog(ex.ToString(), true);
				}
			}
			*/
            #endregion
            //add by sk 2013-09-12 新视频生成
            SerialVideoHtmlNewBuilder serialVideoHtml = new SerialVideoHtmlNewBuilder();
            Dictionary<int, int> videoNumDic = new Dictionary<int, int>();
            foreach (int serialId in serialList)
            {
                int count = Common.Services.VideoService.GetVideoCountBySerialId(serialId);
                if (count > 0)
                    videoNumDic[serialId] = count;
                Common.Log.WriteLog("更新子品牌视频块 start serialId=" + serialId);
                serialVideoHtml.BuilderDataOrHtml(serialId);
                Common.Log.WriteLog("更新子品牌视频块 end serialId=" + serialId);
            }
            //记录视频数量
            string numXmlFile = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\newsNum.xml");
            XmlDocument numDoc = new XmlDocument();
            numDoc.Load(numXmlFile);
            foreach (int serialId in videoNumDic.Keys)
            {
                string xmlPath = "/SerilaList/Serial[@id=" + serialId + "]";
                XmlElement serialNode = (XmlElement)numDoc.SelectSingleNode(xmlPath);
                if (serialNode == null)
                {
                    serialNode = numDoc.CreateElement("Serial");
                    serialNode.SetAttribute("id", serialId.ToString());
                    numDoc.DocumentElement.AppendChild(serialNode);
                }
                serialNode.SetAttribute("video", videoNumDic[serialId].ToString());
            }
            numDoc.Save(numXmlFile);

            //add by sk 2013.04.27 数据同时放到memcache里（文件读取不到或者被占用情况）
            string memCacheKey = "Car_XML_Data_Serial_NewsNum";
            if (numDoc != null && !string.IsNullOrEmpty(numDoc.OuterXml))
                DistCacheWrapper.Insert(memCacheKey, numDoc.OuterXml, 1000 * 60 * 60 * 24 * 3);
            else
                Common.Log.WriteErrorLog("FunctionName:GetSerialVideo memcache Key:Car_XML_Data_Serial_NewsNum 插入内容为空。");
        }

        #endregion

        /// <summary>
        /// 根据子品牌ID获取论坛话题
        /// add by chengl 2015-6-23 增加输出论坛图片
        /// </summary>
        /// <param name="serialId">子品牌ID</param>
        /// <param name="dicTopicImg">帖子小图片字典 帖子ID为key</param>
        /// <returns></returns>
        private DataTable GetForumSubjectBySerial(int serialId, ref Dictionary<int, TopicImgsInfo> dicTopicImg)
        {
            dicTopicImg = new Dictionary<int, TopicImgsInfo>();
            ForumService ser = new ForumService();
            string forumUrl = "";
            // 第一个参数是固定的，第二个参数是新品牌ID,第三个参数是获取数据的数量，第五个参数是返回的品牌易车会域名。
            // DataTable dt = ser.GetLatestTopicListByBrand("groupproduct", serialId, 5, 0, ref forumUrl);

            cn.com.baa.api.TopicImgsInfo[] tiiArray = new TopicImgsInfo[] { };
            DataTable dt = ser.GetLatestTopicListByBrandAndImgListByFgid("groupproduct", serialId, 5, 0, ref forumUrl, out tiiArray); ;

            if (tiiArray != null && tiiArray.Length > 0)
            {
                foreach (TopicImgsInfo tii in tiiArray)
                {
                    if (tii.Tid > 0 && tii.ImgCount > 0 && tii.ImgList.Length > 0 && !dicTopicImg.ContainsKey(tii.Tid))
                    {
                        dicTopicImg.Add(tii.Tid, tii);
                    }
                }
            }

            dt.Columns.Add("serialid");
            //dt.Columns.Add("url");
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    //dr["url"] = string.Format("http://{0}.baa.com.cn/thread-{1}.html", forumUrl, dr["tid"]);
                    dr["serialid"] = serialId;
                }
            }
            return dt;
        }

        /// <summary>
        /// 获取品牌的答疑
        /// </summary>
        /// <param name="brandId"></param>
        public void GetBrandAskEntries(int brandId)
        {
            string newsPath = Path.Combine(CommonData.CommonSettings.SavePath, "AskEntries\\Brand");
            string newsBackupPath = Path.Combine(newsPath, "Backup");
            if (!Directory.Exists(newsBackupPath))
                Directory.CreateDirectory(newsBackupPath);

            List<int> brandList = null;
            if (brandId == 0)
                brandList = GetBrandList();
            else
            {
                brandList = new List<int>();
                brandList.Add(brandId);
            }

            int counter = 0;
            foreach (int bId in brandList)
            {
                counter++;
                OnLog("     Get Brand:" + bId + " ask entries(" + counter + "/" + brandList.Count + ")...", false);
                try
                {
                    string xmlFile = "Brand_Ask_" + bId + ".xml";
                    string backupFile = Path.Combine(newsBackupPath, xmlFile);
                    xmlFile = Path.Combine(newsPath, xmlFile);

                    string xmlUrl = String.Format(CommonData.CommonSettings.AskEntriesUrl, "seriesid", bId, 12);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }

        /// <summary>
        /// 获取所有子品牌的好中差的点评
        /// modified by sk 2013-10-15 更改口碑接口
        /// </summary>
        public void GetSerialDianping(int serId)
        {
            OnLog("     Get the Serial Dianping.", true);

            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialDianping\\Xml");

            string htmlPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialDianping\\Html");

            //string baseUrl = "http://koubei.bitauto.com/api/handler.ashx?apikey=a1b7f24d6c1e473d9bb87336b92885f3&cat=topic&op=get&carid={0}&level={1}&maxresults=10&orderby=hot";

            string baseUrl = CommonData.CommonSettings.KouBeiSerialDianpingUrl;

            List<int> serialList = null;
            if (serId == 0)
                serialList = CommonFunction.GetSerialList();
            else
            {
                serialList = new List<int>();
                serialList.Add(serId);
            }
            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                string logText = "      getting the Serial Dianping (" + counter + "/" + serialList.Count + ") serialId=" + serialId;
                OnLog(logText, false);
                string xmlFile = "Dianping_Serial_" + serialId + ".xml";
                xmlFile = Path.Combine(filePath, xmlFile);

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("SerialDianping");
                xmlDoc.AppendChild(root);
                XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.InsertBefore(xmlDeclar, root);
                //1\2\3差中好
                int dianpingCount = 0;
                for (int i = 1; i <= 3; i++)
                {
                    XmlElement dpNode = xmlDoc.CreateElement("Dianping");
                    dpNode.SetAttribute("type", i.ToString());
                    root.AppendChild(dpNode);
                    int count = 0;
                    string xmlUrl = String.Format(baseUrl, serialId, i);
                    try
                    {
                        XmlDocument dpDoc = new XmlDocument();
                        dpDoc.Load(xmlUrl);
                        dpNode.InnerXml = dpDoc.DocumentElement.InnerXml;
                        //获取点评数
                        count = ConvertHelper.GetInteger(dpDoc.SelectSingleNode("/feed").Attributes["totalcount"].Value);
                        dianpingCount += count;
                        dpNode.SetAttribute("count", count.ToString());
                    }
                    catch (Exception ex)
                    {
                        Common.Log.WriteErrorLog(ex);
                    }
                }
                root.SetAttribute("count", dianpingCount.ToString());
                CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);

                //生成Html
                logText = "MakeDianpingHtml!";
                OnLog(logText, false);
                DianpingHtmlBuilder dpBuilder = new DianpingHtmlBuilder();
                dpBuilder.DianpingXmlDocument = xmlDoc;
                dpBuilder.BuilderDataOrHtml(serialId);
            }
        }

        /// <summary>
        /// 获取厂商新闻
        /// </summary>
        public void GetProducerNews(int producerId)
        {
            List<int> producerList = null;
            if (producerId == 0)
                producerList = GetProducerList();
            else
            {
                producerList = new List<int>(1);
                producerList.Add(producerId);
            }
            string xmlUrl = CommonData.CommonSettings.NewsUrl + "?nonewstype=2&getcount=3000&ismain=1&include=1&producerid=";
            int counter = 0;
            foreach (int pId in producerList)
            {
                counter++;
                OnLog(string.Format("     Get producer {0} news({1}/{2})...", pId.ToString(), counter.ToString(), producerList.Count.ToString()), false);
                try
                {
                    XmlReader reader = null;
                    try
                    {
                        reader = XmlReader.Create((xmlUrl + pId.ToString()));
                        while (reader.ReadToFollowing("newsid"))
                        {
                            CommonFunction.SendQueueMessage(Convert.ToInt32(reader.ReadString()), DateTime.Now, "CMS", "News");
                        }
                    }
                    catch (Exception ex)
                    {
                        OnLog(ex.ToString(), true);
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
            OnLog("完成！", true);
        }

        /// <summary>
        /// 获取厂商列表
        /// </summary>
        private List<int> GetProducerList()
        {
            List<int> producerList = new List<int>();
            try
            {
                string sqlStr = "SELECT Cp_Id FROM Car_producer WHERE IsState=0";
                DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int pId = Convert.ToInt32(row[0]);
                    producerList.Add(pId);
                }
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
            return producerList;
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

        /// <summary>
        ///     生成车型首页问答的HTML块
        ///     author:songcl date:2014-11-27
        /// </summary>
        public void BuilderDefaultAskHtml()
        {
            try
            {
                var askHtmlBuilder = new AskHtmlBuilder();
                askHtmlBuilder.BuilderDefaultAskHtml();
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
                //throw new Exception(ex.ToString());
            }
            //try
            //{
            //	var askHtmlBuilder = new AskHtmlBuilder();  //lisf 1200改版 2016-8-01 经王淞确认 问答块去掉
            //	askHtmlBuilder.BuilderDefaultAskHtmlNew();
            //}
            //catch (Exception ex)
            //{
            //	Common.Log.WriteErrorLog(ex.ToString());
            //	//throw new Exception(ex.ToString());
            //}
        }

        /// <summary>
        ///     生成主品牌问答的HTML块
        ///     author:songcl date:2014-11-27
        /// </summary>
        /// <param name="id"></param>
        public void BuilderMasterAskHtml(int id)
        {
            try
            {
                var askHtmlBuilder = new AskHtmlBuilder();
                askHtmlBuilder.BuilderMasterAskHtml(id);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("生成主品牌问答块异常，masterId=" + id + "\r\n" + ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        ///     生成子牌问答的HTML块
        ///     author:songcl date:2014-11-27
        /// </summary>
        /// <param name="id"></param>
        public void BuilderSerialAskHtml(int id)
        {
            try
            {
                var askHtmlBuilder = new AskHtmlBuilder();
                askHtmlBuilder.BuilderSerialAskHtml(id);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("生成子品牌问答块异常，masterId=" + id + "\r\n" + ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        ///     生成子品牌综述页关键报告中的车内空间HTML块
        ///     author:songcl date:2014-12-10
        /// </summary>
        /// <param name="id"></param>
        public void BuilderKongJianHtml(int id)
        {
            var kongJianHtmlBuilder = new KongJianHtmlBuilder();
            kongJianHtmlBuilder.BuilderDataOrHtml(id);
        }

        public void BuildH5ArticalXml(int id)
        {
            H5HtmlBuilder h5HtmlBuilder = new H5HtmlBuilder();
            h5HtmlBuilder.BuildH5ArticalXml(id);
        }
    }

    /// <summary>
    /// 子品级别枚举
    /// </summary>
    public enum SerialLevelEnum
    {
        微型车 = 1,
        小型车 = 2,
        紧凑型 = 3,
        中大型 = 4,
        中型车 = 5,
        豪华型 = 6,
        MPV = 7,
        SUV = 8,
        跑车 = 9,
        其它 = 10,
        面包车 = 11,
        皮卡 = 12
    }

    /// <summary>
    /// 新闻数量
    /// </summary>
    public class NewsNum
    {
        public int Number = 0;
        public Dictionary<int, NewsNum> newsNumList = new Dictionary<int, NewsNum>();
    }
}
