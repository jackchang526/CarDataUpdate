using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Net;
using BitAuto.Utils;
using System.IO;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.imgsvr;
using System.Threading;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class Exhibition
    {
        public event LogHandler Log;
        //车展参数配置类
        private List<ExhibitionObject> _objectlist = new List<ExhibitionObject>();
        private string _RootPath = "CarShow\\{0}\\";
        private string _MasterBrand = "MasterBrand";
        private string _MasterBrandVideo = "MasterBrandVideos";
        private string _Serial = "Serial";
        private string _SerialDianPing = "SerialDianping";
        private string _SerialVideo = "SerialVideo";
        private string _DefaultImage = "DefaultImages.xml";
        private string _SerialImage = "SerialImages.xml";
        private string _TuJieImage = "ImageTuJie";

        private string _ImageTotalUrl = "http://imgsvr.bitauto.com/zhuanti/exhibitinterface.aspx";
        private string _ImageDefaultUrl = "http://imgsvr.bitauto.com/zhuanti/guangzhou2009.aspx?code=gz2009mastermodel&classid={0}&rownum={1}";
        /// <summary>
        /// 参数1:请求页面地址
        /// 参数2:请求品牌ID
        /// 参数3:请求新闻数量
        /// 参数4:请求标签名称
        /// 参数5:新闻发布的开始时间
        /// </summary>
        private string _MasterBrandNewsUrl = "{0}?bigbrand={1}&include=1&getcount={2}&tags={3}&begintime={4}";
        /// <summary>
        /// 参数1:请求页面地址
        /// 参数2:请求品牌ID
        /// 参数3:请求新闻数量
        /// 参数4:请求标签名称
        /// 参数5:新闻发布的开始时间
        /// </summary>
        private string _SerialBrandNewsUrl = "{0}?brandid={1}&getcount={2}&tags={3}&begintime={4}&articaltype=0";
        /// <summary>
        /// 点评地址
        /// </summary>
        private string _DianPing = "http://koubei.bitauto.com/api/handler.ashx?apikey=a1b7f24d6c1e473d9bb87336b92885f3&cat=topic&op=get&carid={0}&level={1}&maxresults=10";
        /// <summary>
        /// 主品牌视频请求地址
        /// </summary>
        private string _MasterBrandShiPin = "{0}?articaltype=3&getcount={1}&include=1&bigbrand={2}";
        /// <summary>
        /// 子品牌视频请求地址
        /// </summary>
        private string _SerialShiPin = "{0}?articaltype=3&getcount={1}&include=1&brandid={2}&tags={3}";
        /// <summary>
        /// 构造函数
        /// </summary>
        public Exhibition()
        {
            ExhibitionObject eo = new ExhibitionObject();
            eo.ExhibitionId = 19;
            eo.ExhibitionTagId = 24;
            eo.FileSavePath = "2010Beijing";
            eo.ImageSiteId = 25347;
            eo.ImageCarId = 25353;
            eo.ImageModelId = 25352;
            eo.BrandNewsCount = 9;
            eo.SerialNewsCount = 9;
            eo.VideoNewsCount = 12;
            eo.ImageCount = 14;
            eo.ImageCarCount = 10;
            eo.ImageSerailParam = "key=bj2010imagelist&classid={0}&rownum={1}";
            eo.ImageParam = "key=bj2010classlist";
            eo.PublishTimeSpan = (new DateTime(2010, 1, 1)).Ticks;
            //北京车展2010
            _objectlist.Add(eo);

            ExhibitionObject eo1 = new ExhibitionObject();
            eo1.ExhibitionId = 48;
            eo1.ExhibitionTagId = 71;
            eo1.FileSavePath = "2010guangzhou";
            eo1.ImageSiteId = 39001;
            eo1.ImageCarId = 39002;
            eo1.ImageModelId = 39003;
            eo1.SerialNewsCount = 9;
            eo1.BrandNewsCount = 9;
            eo1.VideoNewsCount = 12;
            eo1.ImageCount = 14;
            eo1.ImageCarCount = 10;
            eo1.ImageSerailParam = "key=bj2010imagelist&classid={0}&rownum={1}";
            eo1.ImageParam = "key=bj2010classlist&classid=39001&newcarid=39002&modelid=39003";
            eo1.PublishTimeSpan = (new DateTime(2010, 1, 1)).Ticks;
            //广州车展2010
            _objectlist.Add(eo1);

            ExhibitionObject eo2 = new ExhibitionObject();
            eo2.ExhibitionId = 59;
            eo2.ExhibitionTagId = 1;
            eo2.FileSavePath = "2011shanghai";
            eo2.ImageSiteId = 40691;
            eo2.ImageCarId = 40694;
            eo2.ImageModelId = 40693;
            eo2.SerialNewsCount = 9;
            eo2.BrandNewsCount = 9;
            eo2.VideoNewsCount = 6;
            eo2.ImageCount = 14;
            eo2.ImageCarCount = 10;
            eo2.ImageSerailParam = "key=bj2010imagelist&classid={0}&rownum={1}";
            eo2.ImageParam = "key=bj2010classlist&classid=40691&newcarid=40694&modelid=40693&modellevel=2";
            eo2.ImageTuJiUrl = "http://imgsvr.bitauto.com/Photo/ImageService.aspx?dataname=imageingroup&showimages=true"
                              + "&showfullurl=true&showaccount=true&serialid={0}&rownum=4&groupid=12&orderby=&imagename=chezhan-shanghai2011";
            eo2.PublishTimeSpan = (new DateTime(2011, 1, 1)).Ticks;
            //上海车展2011
            _objectlist.Add(eo2);

            // modified by chengl Nov.17.2011
            // 2011 广州
            ExhibitionObject eo3 = new ExhibitionObject();
            eo3.ExhibitionId = 71;
            eo3.ExhibitionTagId = 71;
            eo3.FileSavePath = "2011guangzhou";
            eo3.ImageSiteId = 44163;
            eo3.ImageCarId = 44165;
            eo3.ImageModelId = 44164;
            eo3.SerialNewsCount = 9;
            eo3.BrandNewsCount = 9;
            eo3.VideoNewsCount = 6;
            eo3.ImageCount = 14;
            eo3.ImageCarCount = 10;
            eo3.ImageSerailParam = "key=bj2010imagelist&classid={0}&rownum={1}";
            eo3.ImageParam = "key=bj2010classlist&classid=" + eo3.ImageSiteId + "&newcarid=" + eo3.ImageCarId + "&modelid=" + eo3.ImageModelId + "&modellevel=2";
            eo3.ImageTuJiUrl = "http://imgsvr.bitauto.com/Photo/ImageService.aspx?dataname=imageingroup&showimages=true"
                              + "&showfullurl=true&showaccount=true&serialid={0}&rownum=4&groupid=12&orderby=&imagename=chezhan-shanghai2011";
            eo3.PublishTimeSpan = (new DateTime(2011, 1, 1)).Ticks;
            // 2011 广州
            _objectlist.Add(eo3);

			// 2012 北京车展
			ExhibitionObject eo4 = new ExhibitionObject();
			eo4.ExhibitionId = 84;
			eo4.ExhibitionTagId = 24;
			eo4.FileSavePath = "2012beijing";
			eo4.ImageSiteId = 46248;
			eo4.ImageCarId = 46250;
			eo4.ImageModelId = 46249;
			eo4.SerialNewsCount = 9;
			eo4.BrandNewsCount = 9;
			eo4.VideoNewsCount = 6;
			eo4.ImageCount = 14;
			eo4.ImageCarCount = 10;
			eo4.ImageSerailParam = "key=bj2010imagelist&classid={0}&rownum={1}";
			eo4.ImageParam = "key=bj2010classlist&classid=" + eo4.ImageSiteId + "&newcarid=" + eo4.ImageCarId + "&modelid=" + eo4.ImageModelId + "&modellevel=2";
			eo4.ImageTuJiUrl = "http://imgsvr.bitauto.com/Photo/ImageService.aspx?dataname=imageingroup&showimages=true"
							  + "&showfullurl=true&showaccount=true&serialid={0}&rownum=4&groupid=12&orderby=&imagename=chezhan-beijing2012";
			eo4.PublishTimeSpan = (new DateTime(2011, 1, 1)).Ticks;
			_objectlist.Add(eo4);
        }
        /// <summary>
        /// 得到车展相关内容
        /// </summary>
        public void GetContent()
        {
            try
            {
                //得到焦点图新闻
                GetCarShowTopNews(0);
                //得到主品牌\子品牌封面图
                GetCarshowBrandImage(0);
                //得到车展子品牌图片
                GetCarshowSerialImage(0);
                //得到所有子品牌的XML
                GetAllSerialXmlForCarShow();
                //得到子品牌的点评数据
                GetCarshowSerialDianping(0);
            }
            catch(Exception ex)
            {
				OnLog(ex.Message,true);
			}
        }
        /// <summary>
        /// 得到焦点区新闻和视频新闻
        /// </summary>
        /// <param name="id"></param>
        public void GetCarShowTopNews(int id)
        {
            OnLog("		Get the exhibition TopNews.", true);
            ExhibitionObject eo = new ExhibitionObject();
            //判断是否跑指定的车展新闻
            if (id != 0)
            {
                foreach (ExhibitionObject entity in _objectlist)
                {
                    if (entity.ExhibitionId != id) continue;
                    eo = entity;
                    break;
                }
            }
            else
            {
                eo = _objectlist[_objectlist.Count - 1];
            }
            //如果车展ID小于零，则不是有效的车展
            if (eo.ExhibitionId < 1) return;

            XmlDocument treeDoc = GetCarshowBrandTree(eo.ExhibitionId);
            Dictionary<int, List<int>> masterDic = new Dictionary<int, List<int>>();
            List<int> serialList = new List<int>();
            XmlNodeList serialNodes = treeDoc.SelectNodes("/Exhibition/MasterBrand/Brand/Serial");
            foreach (XmlElement tempNode in serialNodes)
            {
                int serialId = ConvertHelper.GetInteger(tempNode.GetAttribute("ID"));
                XmlElement brandNode = (XmlElement)tempNode.ParentNode;
                int brandId = ConvertHelper.GetInteger(brandNode.GetAttribute("ID"));
                XmlElement masterNode = (XmlElement)brandNode.ParentNode;
                int masterId = ConvertHelper.GetInteger(masterNode.GetAttribute("ID"));
                //判断主品牌和子品牌是否存在
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
            // modified by chengl Nov.17.2011
            ////2011上海车展
            //if (eo.ExhibitionId == 59) 
            //{
            //    GetCarshowSerialNews(serialList, eo);
            //    GetCarshowSerialVideo(serialList, eo);
            //    //得到图解的图片
            //    //GetTuJiImage(serialList, eo);
            //    return;
            //}
            ////2011广州车展
			//if (eo.ExhibitionId == 71)
			//{
			//    GetCarshowSerialNews(serialList, eo);
			//    GetCarshowSerialVideo(serialList, eo);
			//    //得到图解的图片
			//    //GetTuJiImage(serialList, eo);
			//    return;
			//}
			//2012北京车展
			if (eo.ExhibitionId == 84)
			{
				GetCarshowSerialNews(serialList, eo);
				GetCarshowSerialVideo(serialList, eo);
				//得到图解的图片
				//GetTuJiImage(serialList, eo);
				return;
			}
            //生成新闻
            GetCarshowMasterbrandNews(masterDic, eo);
            GetCarshowSerialNews(serialList, eo);
            GetCarShowMasterBrandVideos(masterDic, eo);
        }
        /// <summary>
        /// 获取主品牌的车展新闻
        /// </summary>
        /// <param name="masterDic"></param>
        /// <param name="carshowTagId"></param>
        private void GetCarshowMasterbrandNews(Dictionary<int, List<int>> masterDic, ExhibitionObject eo)
        {
            string filePath = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _MasterBrand);
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

                string xmlUrl = string.Format(_MasterBrandNewsUrl, CommonData.CommonSettings.NewsUrl, brandIdStr, eo.BrandNewsCount, eo.ExhibitionTagId, eo.PublishTimeSpan);
                XmlDocument newsDoc = new XmlDocument();
                try
                {
                    newsDoc.Load(xmlUrl);
                    CommonFunction.SaveAndBackupNews(newsDoc, xmlFile, backupFile);
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
        private void GetCarshowSerialNews(List<int> serialList, ExhibitionObject eo)
        {
            string filePath = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _Serial);
            string backupPath = Path.Combine(filePath, "Backup");

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("Get carshow Serial:" + serialId + " news,(" + counter + "/" + serialList.Count + ")", false);

                string xmlFile = "Carshow_Serial_" + serialId + ".xml";
                string backupFile = Path.Combine(backupPath, xmlFile);
                xmlFile = Path.Combine(filePath, xmlFile);

                string xmlUrl = string.Format(_SerialBrandNewsUrl, CommonData.CommonSettings.NewsUrl, serialId, eo.BrandNewsCount, eo.ExhibitionTagId, eo.PublishTimeSpan);
                XmlDocument newsDoc = new XmlDocument();
                try
                {
                    newsDoc.Load(xmlUrl);
                    CommonFunction.SaveAndBackupNews(newsDoc, xmlFile, backupFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }
        /// <summary>
        /// 获取车展子品牌新闻
        /// </summary>
        /// <param name="serialList"></param>
        /// <param name="eo"></param>
        private void GetCarshowSerialVideo(List<int> serialList, ExhibitionObject eo)
        {
            string newsPath = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _SerialVideo);
            string newsBackupPath = Path.Combine(newsPath, "Backup");
            if (!Directory.Exists(newsBackupPath))
                Directory.CreateDirectory(newsBackupPath);

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("Get master brand:" + serialId + " videos(" + counter + "/" + serialList.Count + ")...", false);
                try
                {
                    string xmlFile = serialId + ".xml";
                    string backupFile = Path.Combine(newsBackupPath, xmlFile);
                    xmlFile = Path.Combine(newsPath, xmlFile);

                    string xmlUrl = string.Format(_SerialShiPin, CommonData.CommonSettings.NewsUrl, eo.VideoNewsCount, serialId, eo.ExhibitionTagId);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveAndBackupNews(xmlDoc, xmlFile, backupFile);
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
        private void GetTuJiImage(List<int> serialList, ExhibitionObject eo)
        {
            string filePath = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _TuJieImage);
            string backupPath = Path.Combine(filePath, "Backup");

            int counter = 0;
            foreach (int serialId in serialList)
            {
                counter++;
                OnLog("Get carshow Serial:" + serialId + " news,(" + counter + "/" + serialList.Count + ")", false);

                string xmlFile = serialId + ".xml";
                string backupFile = Path.Combine(backupPath, xmlFile);
                xmlFile = Path.Combine(filePath, xmlFile);

                string xmlUrl = string.Format(eo.ImageTuJiUrl, serialId);
                XmlDocument newsDoc = new XmlDocument();
                try
                {
                    newsDoc.Load(xmlUrl);
                    CommonFunction.SaveAndBackupNews(newsDoc, xmlFile, backupFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }
        /// <summary>
        /// 获取主品牌视频
        /// </summary>
        public void GetCarShowMasterBrandVideos(Dictionary<int, List<int>> masterDic, ExhibitionObject eo)
        {
            string newsPath = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _MasterBrandVideo);
            string newsBackupPath = Path.Combine(newsPath, "Backup");
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

                    string xmlUrl = string.Format(_MasterBrandShiPin, CommonData.CommonSettings.NewsUrl, eo.VideoNewsCount, brandIdList);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);

                    CommonFunction.SaveAndBackupNews(xmlDoc, xmlFile, backupFile);
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
        }
        /// <summary>
        /// 获取车展中的主品牌，子品牌的封面图
        /// </summary>
        public void GetCarshowBrandImage(int id)
        {
            ExhibitionObject eo = new ExhibitionObject();
            //判断是否跑指定的车展新闻
            if (id != 0)
            {
                foreach (ExhibitionObject entity in _objectlist)
                {
                    if (entity.ExhibitionId != id) continue;
                    eo = entity;
                    break;
                }
            }
            else
            {
                eo = _objectlist[_objectlist.Count - 1];
            }
            //如果车展ID小于零，则不是有效的车展
            if (eo.ExhibitionId < 1) return;
            string xmlFile = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _DefaultImage);
            string backupFile = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)) + "backup\\", _DefaultImage);

            XmlDocument imgDoc = new XmlDocument();
            XmlElement root = imgDoc.CreateElement("Images");
            imgDoc.AppendChild(root);
            XmlDeclaration xmlDeclar = imgDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            imgDoc.InsertBefore(xmlDeclar, root);

            try
            {
                OnLog("		Get car show default images...", true);
                int carshowId = eo.ImageSiteId;
                CommonService commonSrv = new CommonService();
                DataSet imgDs = commonSrv.GetChildrenByParentId(carshowId, true);

                int parentId = eo.ImageCarId;		//新车图片的分类ID，测试用22308
                int modelId = eo.ImageModelId;		//车模图片的分类ID，测试用22309
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
                            OnLog("		Get car show default images master:" + masterId + "(" + counter + "/" + masterRows.Length + ")...", false);

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
                                string modelUrl = String.Format(_ImageDefaultUrl, albumId, eo.ImageCount);
                                try
                                {
                                    XmlDocument mbDoc = new XmlDocument();
                                    mbDoc.Load(modelUrl);
                                    Thread.Sleep(1000);
                                    masterNode.InnerXml = mbDoc.DocumentElement.InnerXml;
                                }
                                catch { }
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
                            OnLog("		Get car show default images Serial:" + serialId + "(" + counter + "/" + serialRows.Length + ")...", false);

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
								// modified by chengl May.10.2012
								// PagedImageList imageList = commonSrv.GetImageListByAlbumId(albumId, 10, 1);
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

            CommonFunction.SaveAndBackupNews(imgDoc, xmlFile, backupFile);
        }
        /// <summary>
        /// 获取车展子品牌图片
        /// </summary>
        public void GetCarshowSerialImage(int id)
        {
            ExhibitionObject eo = new ExhibitionObject();
            //判断是否跑指定的车展新闻
            if (id != 0)
            {
                foreach (ExhibitionObject entity in _objectlist)
                {
                    if (entity.ExhibitionId != id) continue;
                    eo = entity;
                    break;
                }
            }
            else
            {
                eo = _objectlist[_objectlist.Count - 1];
            }
            //如果车展ID小于零，则不是有效的车展
            if (eo.ExhibitionId < 1) return;
            string ImageSrvUrl = _ImageTotalUrl + "?" + eo.ImageParam;
            string serialImgSrvUrl = _ImageTotalUrl + "?" + eo.ImageSerailParam;
            string xmlFile = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _SerialImage);
            string backupFile = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)) + "backup\\", _SerialImage);

            XmlDocument imgDoc = new XmlDocument();
            XmlElement root = imgDoc.CreateElement("Images");
            imgDoc.AppendChild(root);
            XmlDeclaration xmlDeclar = imgDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            imgDoc.InsertBefore(xmlDeclar, root);

            XmlDocument srvDoc = new XmlDocument();
            srvDoc.Load(ImageSrvUrl);
            //所有车展中的子品牌
            XmlNodeList serialNodeList = srvDoc.SelectNodes("/Data/NewCar/Master/Serial");
            foreach (XmlElement serialNode in serialNodeList)
            {
                int serialId = Convert.ToInt32(serialNode.GetAttribute("Id"));
                int classId = Convert.ToInt32(serialNode.GetAttribute("ClassId"));
                XmlElement serialImageRoot = imgDoc.CreateElement("SerialImages");
                root.AppendChild(serialImageRoot);
                serialImageRoot.SetAttribute("Id", serialId.ToString());
                serialImageRoot.SetAttribute("ClassId", classId.ToString());

                //取子品牌的图片
                string url = String.Format(serialImgSrvUrl, classId, eo.ImageCarCount);
                XmlDocument tmpDoc = new XmlDocument();
                tmpDoc.Load(url);
                string imgDomain = tmpDoc.DocumentElement.GetAttribute("ImageDomain");
                string baseImgUrl = tmpDoc.DocumentElement.GetAttribute("TargetUrlBase");
                XmlNodeList imgNodeList = tmpDoc.SelectNodes("/Data/Image");
                foreach (XmlElement imgNode in imgNodeList)
                {
                    string albumUrl = Path.Combine(baseImgUrl, imgNode.GetAttribute("TargetUrl"));
                    string imgUrl = Path.Combine(CommonFunction.GetPublishHashImageDomain(ConvertHelper.GetInteger(imgNode.GetAttribute("ImageId")))
                        , imgNode.GetAttribute("ImageUrl"));
                    imgNode.SetAttribute("TargetUrl", albumUrl);
                    imgNode.SetAttribute("ImageUrl", imgUrl);
                    serialImageRoot.AppendChild(imgDoc.ImportNode(imgNode, true));
                }
            }

            CommonFunction.SaveAndBackupNews(imgDoc, xmlFile, backupFile);
        }
        /// <summary>
        /// 为车展获取所有子品牌，品牌，主品牌信息
        /// </summary>
        public void GetAllSerialXmlForCarShow()
        {
            string xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, "MasterToBrandToSerialAllSaleAndLevel.xml");
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(CommonData.CommonSettings.AllSaleAndLevelUrl);
                xmlDoc.Save(xmlFile);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }
        /// <summary>
        /// 获取车展中子品牌的点评
        /// </summary>
        public void GetCarshowSerialDianping(int id)
        {
            OnLog("		Get the exhibition SerialDianping.", true);
            ExhibitionObject eo = new ExhibitionObject();
            //判断是否跑指定的车展新闻
            if (id != 0)
            {
                foreach (ExhibitionObject entity in _objectlist)
                {
                    if (entity.ExhibitionId != id) continue;
                    eo = entity;
                    break;
                }
            }
            else
            {
                eo = _objectlist[_objectlist.Count - 1];
            }
            //如果车展ID小于零，则不是有效的车展
            if (eo.ExhibitionId < 1) return;
            string filePath = Path.Combine(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(_RootPath, eo.FileSavePath)), _SerialDianPing);
            string backupPath = Path.Combine(filePath, "Backup");

            string baseUrl = _DianPing;

            int carShowId = eo.ExhibitionId;
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
                    catch { }
                }

                root.SetAttribute("count", dianpingCount.ToString());

                CommonFunction.SaveAndBackupNews(xmlDoc, xmlFile, backupFile);
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
    }
    /// <summary>
    /// 展会对象,保存一次展会要用到的一些对象
    /// </summary>
    public struct ExhibitionObject
    {
        //车展的展会Id
        public int ExhibitionId;
        //车展焦点图新闻标签Id
        public int ExhibitionTagId;
        //文件保存路径
        public string FileSavePath;
        //子品牌新闻数量
        public int SerialNewsCount;
        //品牌新闻数量
        public int BrandNewsCount;
        //视频新闻数量
        public int VideoNewsCount;
        //图片大分类ID
        public int ImageSiteId;
        //图片的车型ID
        public int ImageCarId;
        //图片的车模ID
        public int ImageModelId;
        //查询用到的变量
        public string ImageParam;
        //得到指定张数的子品牌车型图片用到的参数
        public string ImageSerailParam;
        //图片图解地址
        public string ImageTuJiUrl;
        //图片数量
        public int ImageCount;
        //车型图片张数
        public int ImageCarCount;
        //发布时间戳
        public long PublishTimeSpan;

    }
}
