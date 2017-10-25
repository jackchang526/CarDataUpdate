using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using System.IO;
using System.Net;
using BitAuto.CarDataUpdate.DataProcesser.Services;
using BitAuto.CarDataUpdate.Common.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 图库接口数据
    /// </summary>
    public class PhotoImageService
    {
        PhotoImageConfig photoConfig;
        public event LogHandler Log;
        public PhotoImageService()
        {
            photoConfig = (PhotoImageConfig)ConfigurationManager.GetSection("PhotoImageConfig");

        }
        #region 获取接口生成数据库
        /// <summary>
        /// 车型图片列表
        /// http://imgsvr.bitauto.com/carimage/GetImagesForCarModelUseMultiPositionGroupIds.aspx?serialbrandid=2370&carmodelid=104633&positionGroupIDs=6,7,8&Count=9
        /// </summary>
        /// <param name="serialbrandid"></param>
        /// <param name="carmodelid"></param>
        public void CarImagesListInfo(int serialbrandid, int carmodelid)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"CarImagesListInfo\{0}.xml", carmodelid));
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.CarImagesListInfoPath, serialbrandid, carmodelid));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }

        /// <summary>
        /// 车型图片子品牌颜色
        /// http://imgsvr.bitauto.com/photo/imageservice.aspx?dataname=serialcolorimage&serialid={子品牌ID}&showfullurl=true&subfix=5
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialColor(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialColor\SerialColor_{0}.xml", serialId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialColorPath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 车型图片子品牌颜色
        /// http://imgsvr.bitauto.com/photo/imageservice.aspx?dataname=serialcolorimage&serialid={子品牌ID}&showfullurl=true&subfix=5&showall=true
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialColorAll(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialColorAll\SerialColorAll_{0}.xml", serialId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialColorAllPath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 子品牌图片列表
        /// http://imgsvr.bitauto.com/autochannel/autoservice.aspx?sid={子品牌ID}
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialPhotoList(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialPhotoList\SerialPhotoList_{0}.xml", serialId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialPhotoListPath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 子品牌年款
        /// http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx?dataname=serialyearimages&serialid={子品牌ID}&year={年款}
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="year"></param>
        public void SerialYear(int serialId, int year)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialYear\SerialYear_{0}_{1}.xml", serialId, year));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialYearPath, serialId, year));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 对比子品牌
        /// http://imgsvr.bitauto.com/autochannel/imagecompareaccount.aspx?min=20&chk=true
        /// </summary>
        public void SerialCompare()
        {
            string fileName = Path.Combine(photoConfig.SavePath, "SerialCompare.xml");
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(photoConfig.SerialComparePath);
                XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.InsertBefore(xmlDeclar, xmlDoc.DocumentElement);
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 图片对比
        /// http://imgsvr.bitauto.com/autochannel/imagecompare.aspx?sid={子品牌ID}
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialPhotoCompare(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialPhotoCompare\SerialPhotoCompare_{0}.xml", serialId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialPhotoComparePath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }

        /// <summary>
        /// 车款图片对比
        /// </summary>
        /// <param name="serialId"></param>
        public void CarPhotoCompare(int carId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"CarPhotoCompare\{0}.xml", carId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.CarPhotoComparePath, carId));
                if (xmlDoc.DocumentElement != null && xmlDoc.DocumentElement.HasChildNodes)
                    CommonFunction.SaveXMLDocument(xmlDoc, fileName);
                else
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 子品牌分类(html)
        /// http://imgsvr.bitauto.com/Photo/HtmlOutput.aspx?dataname=groupandcarlist&id={子品牌ID}
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialClass(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialClass\SerialClass_{0}.html", serialId));
            string url = string.Format(photoConfig.SerialClassPath, serialId);
            try
            {
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                string content = wc.DownloadString(url);
                CommonFunction.SaveFileContent(content, fileName, "utf-8");
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 子品牌封面
        /// http://imgsvr.bitauto.com/baa/getserialcover.aspx
        /// </summary>
        public void SerialCover()
        {
            string fileName = Path.Combine(photoConfig.SavePath, "SerialCover.xml");
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(photoConfig.SerialCoverPath);
                XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
                xmlDoc.InsertBefore(xmlDeclar, xmlDoc.DocumentElement);
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /* 由SerialCoverImageAndCount方法代替 2017-04-24 lisf
 		/// <summary>
		/// 非白底封面
		/// http://imgsvr.bitauto.com/photo/getseriallist.aspx
		/// </summary>
		public void SerialCoverWithout()
		{
			string fileName = Path.Combine(photoConfig.SavePath, "SerialCoverWithout.xml");
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(photoConfig.SerialCoverWithoutPath);
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				OnLog(ex.Message + ex.StackTrace, true);
			}
		}
		 * */

        /// <summary>
        /// 车系封面图和车系图片数量
        /// </summary>
        public void SerialCoverImageAndCount()
        {
            string fileName = Path.Combine(photoConfig.SavePath, "SerialCoverImageAndCount.xml");
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(photoConfig.SerialCoverImageAndCountPath);
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }

        /*  // del by lsf 2016-01-06
		/// <summary>
		/// 子品牌12张标准图
		/// http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx?dataname=serialstandardimage&serialid={子品牌ID}&rownum=12
		/// </summary>
		public void SerialStandardImage(int serialId)
		{
			string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialStandardImage\SerialStandardImage_{0}.xml", serialId));
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(string.Format(photoConfig.SerialStandardImagePath, serialId));
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				OnLog(ex.Message + ex.StackTrace, true);
			}
		}
		/// <summary>
		/// 车型12张标准图
		/// http://imgsvr.bitauto.com/autochannel/carImageService.aspx?dataname=carstandardimagewithaccount&serialid={子品牌ID}&carid={车型ID}
		/// </summary>
		/// <param name="carId"></param>
		public void CarStandardImage(int serialId, int carId)
		{
			string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"CarStandardImage\CarStandardImage_{0}_{1}.xml", serialId, carId));
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(string.Format(photoConfig.CarStandardImagePath, serialId, carId));
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				OnLog(ex.Message + ex.StackTrace, true);
			}
		}*/
        /// <summary>
		/// 车型封面
		/// http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx?dataname=carcoverimage&showall=false&showfullurl=true&subfix=2
		/// </summary>
		public void CarCoverImage()
        {
            string fileName = Path.Combine(photoConfig.SavePath, "CarCoverImage.xml");
            //XmlDocument xmlDoc = new XmlDocument();
            //try
            //{
            //    xmlDoc.Load(photoConfig.CarCoverImagePath);
            //    CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            //}
            //catch (Exception ex)
            //{
            //    OnLog(ex.Message + ex.StackTrace, true);
            //}

            try
            {
                string json = CommonFunction.GetResponseFromUrl(string.Format(photoConfig.CarCoverImagePath, ""));

                JObject jobject = (JObject)JsonConvert.DeserializeObject(json);

                List<XElement> elementList = new List<XElement>();
                foreach (var item in (JArray)jobject["Data"])
                {
                    elementList.Add(
                            new XElement("ImageInfo",
                             new XAttribute("CarId", item["StyleId"].ToString()),
                             new XAttribute("ImageId", item["CoverPhotoId"].ToString()),
                             new XAttribute("ImageName", item["PositionName"].ToString()),
                             new XAttribute("ImageUrl", string.Format(item["PhotoUrl"].ToString(), 2)),
                             new XAttribute("ClassId", "0")
                         ));
                }

                XDocument doc = new XDocument(
                 new XElement("ImageData",
                  new XAttribute("ImageUrlBase", "http://image.bitautoimg.com/autoalbum/"),
                   new XElement("ImageList", elementList)
                     )
                 );
                doc.Save(fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 车型3张焦点图
        /// http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx?dataname=serialfocusimage&serialid={子品牌ID}&cId={车型ID}&year={年款}&showall=false&showfullurl=true
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="carId"></param>
        /// <param name="year"></param>
        public void CarFocusImage(int serialId, int carId, int year)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"CarFocusImage\CarFocusImage_{0}.xml", carId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.CarFocusImagePath, serialId, carId, year));
                if (xmlDoc != null)
                {
                    XmlNodeList nodeList = xmlDoc.SelectNodes("/ImageData/ImageList/ImageInfo");
                    if (nodeList.Count > 0)
                    {
                        CommonFunction.SaveXMLDocument(xmlDoc, fileName);
                    }
                    else
                    {
                        Common.Log.WriteLog("删除车款焦点图：" + fileName);
                        File.Delete(fileName);
                    }
                }
                else
                {
                    File.Delete(fileName);
                }

            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 年款焦点图
        /// http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx?dataname=serialfocusimage&serialid={子品牌ID}&year={年款}&showall=false&showfullurl=true&subfix=4
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="year"></param>
        public void SerialYearFocusImage(int serialId, int year)
        {
            //string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialYearFocusImage\SerialYearFocusImage_{0}_{1}.xml", serialId, year));
            //XmlDocument xmlDoc = new XmlDocument();
            //try
            //{
            //    xmlDoc.Load(string.Format(photoConfig.SerialYearFocusImagePath, serialId, year));
            //    CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            //}
            //catch (Exception ex)
            //{
            //    OnLog(ex.Message + ex.StackTrace, true);
            //}

            try
            {
                string json = CommonFunction.GetResponseFromUrl(string.Format(photoConfig.SerialYearFocusImagePath, serialId));
                JObject jobject = (JObject)JsonConvert.DeserializeObject(json);
                foreach (var item in (JArray)jobject["Data"]["YearCoverList"])
                {
                    year = ConvertHelper.GetInteger(item["CarYear"].ToString());
                    string photoUrl = item["PhotoUrl"].ToString();
                    string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialYearFocusImage\SerialYearFocusImage_{0}_{1}.xml", serialId, year));
                    if (!string.IsNullOrEmpty(photoUrl))
                    {

                        List<XElement> elementList = new List<XElement>();
                        elementList.Add(
                                new XElement("ImageInfo",
                                 new XAttribute("ImageId", item["CoverPhotoId"].ToString()),
                                 new XAttribute("ImageName", ""),
                                 new XAttribute("ImageUrl", string.Format(item["PhotoUrl"].ToString(), 4)),
                                 new XAttribute("GroupName", ""),
                                 new XAttribute("ClassId", "0"),
                                 new XAttribute("SerialId", serialId),
                                 new XAttribute("CarId", "0"),
                                 new XAttribute("CarYear", item["CarYear"].ToString()),
                                 new XAttribute("CarModelName", ""),
                                 new XAttribute("Link", string.Format(item["Url"].ToString(), 2))
                             ));
                        XDocument doc = new XDocument(
                                             new XElement("ImageData",
                                              new XAttribute("ImageUrlBase", "http://image.bitautoimg.com/autoalbum/"),
                                              new XAttribute("CarYear", year),
                                               new XElement("ImageList", elementList)
                                                 ));
                        doc.Save(fileName);
                    }
                    else
                    {
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 取子品牌默认车型
        /// http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx?dataname=serialyearcar&showall=false&showfullurl=true&subfix=2
        /// </summary>
        public void SerialDefaultCar()
        {
            string fileName = Path.Combine(photoConfig.SavePath, "SerialDefaultCar.xml");
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(photoConfig.SerialDefaultCarPath);
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 子品牌默认车型图片
        /// http://imgsvr.bitauto.com/autochannel/carImageService.aspx?dataname=caraccountbygroup&serialid={子品牌ID}&carid={车型ID}&showfullurl=true&subfix=1
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="carId"></param>
        public void SerialDefaultCarImage(int serialId, int carId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialDefaultCarImage\SerialDefaultCarImage_{0}_{1}.xml", serialId, carId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialDefaultCarImagePath, serialId, carId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /// <summary>
        /// 子品牌焦点图片
        /// http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx?dataname=serialfocusimage&amp;serialid={0}&amp;showall=false
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="carId"></param>
        public void SerialFocusImage(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialFocusImage\SerialFocusImage_{0}.xml", serialId));
            //XmlDocument xmlDoc = new XmlDocument();
            //try
            //{
            //	xmlDoc.Load(string.Format(photoConfig.SerialFocusImagePath, serialId));
            //	CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            //}
            //catch (Exception ex)
            //{
            //	OnLog(ex.Message + ex.StackTrace, true);
            //}

            try
            {
                string json = CommonFunction.GetResponseFromUrl(string.Format(photoConfig.SerialFocusImagePath, serialId));

                JObject jobject = (JObject)JsonConvert.DeserializeObject(json);

                List<XElement> elementList = new List<XElement>();
				if (string.IsNullOrEmpty(jobject["Data"].ToString()))
				{

					XDocument docEmpty = new XDocument(
					 new XElement("ImageData",
					  new XAttribute("ImageUrlBase", "http://image.bitautoimg.com/autoalbum/"),
					   new XElement("ImageList")
						 )
					 );
					docEmpty.Save(fileName);
					return;
				}
                foreach (var item in (JArray)jobject["Data"])
                {
                    elementList.Add(
                            new XElement("ImageInfo",
                                 new XAttribute("ImageId", item["PhotoId"].ToString()),
                                 new XAttribute("ImageName", item["PositionName"].ToString()),
                                 new XAttribute("ImageUrl", string.Format(item["PhotoUrl"].ToString(), 4)),
                                 new XAttribute("GroupName", item["GroupName"].ToString()),
                                 new XAttribute("ClassId", "0"),
                                 new XAttribute("SerialId", item["ModelId"].ToString()),
                                 new XAttribute("CarId", item["StyleId"].ToString()),
                                 new XAttribute("CarYear", "0"),
                                 new XAttribute("CarModelName", item["StyleName"].ToString()),
                                 new XAttribute("Link", item["Url"].ToString())
                         ));
                }

                XDocument doc = new XDocument(
                 new XElement("ImageData",
                  new XAttribute("ImageUrlBase", "http://image.bitautoimg.com/autoalbum/"),
                   new XElement("ImageList", elementList)
                     )
                 );
                doc.Save(fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.ToString(), true);
            }

        }
        /// <summary>
        /// 子品牌综述页分类位置图片
        /// http://imgsvr.bitauto.com/CarImage/GetAllCategoriesAndSomePositionImagesForSerialBrand.aspx?serialBrandID=2370&amp;positionIDs=47,43,46,94,97,215,216
        /// </summary>
        /// <param name="serialId">子品牌id</param>
        public void SerialPositionImage(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialPositionImage\SerialPositionImage_{0}.xml", serialId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialPositionImagePath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + ex.StackTrace, true);
            }
        }
        /*   //del by lsf 2016-01-06
		/// <summary>
		/// 子品牌颜色数量
		/// http://imgsvr.bitauto.com/CarImage/GetImageCountingOfSerialBrandForColor.aspx?serialBrandID=2370
		/// </summary>
		/// <param name="serialId"></param>
		public void SerialColorCount(int serialId)
		{
			string fileName = Path.Combine(photoConfig.SavePath,
				string.Format(@"SerialColorCount\SerialColorCount_{0}.xml", serialId)
				);
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(string.Format(photoConfig.SerialColorCountPath, serialId));
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex.Message + ex.StackTrace);
			}
		}*/
        /*
		/// <summary>
		/// 子品牌图片接口
		/// http://imgsvr.bitauto.com/carimagelist/serialbrandindex.ashx?serialbrandid={0}
		/// </summary>
		/// <param name="serialId"></param>
		public void SerialPhotoHtml(int serialId)
		{
			string fileName = Path.Combine(photoConfig.SavePath,
				string.Format(@"SerialPhotoHtml\{0}.html", serialId)
				);
			string url = string.Format(photoConfig.SerialPhotoHtmlPath, serialId);
			try
			{
				WebClient wc = new WebClient();
				wc.Encoding = Encoding.UTF8;
				string content = wc.DownloadString(url);
				if (!string.IsNullOrEmpty(content))
					CommonFunction.SaveFileContent(content, fileName, "utf-8");
				else
					File.Delete(fileName);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex.ToString());
			}
		}
		 * */
        /// <summary>
        /// 子品牌图片接口 1200改版 lisf 2016-8-18
        /// http://imgsvr24.bitauto.com/CarImageListHtml/SerialBrandIndex.ashx?SerialBrandID={0}&noCache=True
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialPhotoHtmlNew(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath,
                string.Format(@"SerialPhotoHtmlNew\{0}.html", serialId)
                );
            string url = string.Format(photoConfig.SerialPhotoHtmlPathNew, serialId);
            try
            {
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                string content = wc.DownloadString(url);
                if (!string.IsNullOrEmpty(content))
                    CommonFunction.SaveFileContent(content, fileName, "utf-8");
                else if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /* // del by lsf 2016-01-06
		public void TestForDeleteFile()
		{
			string fileName = Path.Combine(photoConfig.SavePath, "SerialYearPhotoHtml");
			DirectoryInfo dir = new DirectoryInfo(fileName);
			FileInfo[] files = dir.GetFiles();
			if (files != null && files.Length > 0)
			{
				foreach (FileInfo filepath in files)
				{
					if (filepath.LastWriteTime < DateTime.Now)
					{ }
				}
			}
		}
        */
        /*注释老的接口
		/// <summary>
		/// 子品牌年款图片接口
		/// http://imgsvr.bitauto.com/carimagelist/SerialBrandCarYearIndex.ashx?SerialBrandId=xxxx&caryear=xxxx
		/// </summary>
		/// <param name="serialId"></param>
		public void SerialYearPhotoHtml(int serialId, int year)
		{
			string fileName = Path.Combine(photoConfig.SavePath,
				string.Format(@"SerialYearPhotoHtml\{0}_{1}.html", serialId, year)
				);
			string url = string.Format(photoConfig.SerialYearPhotoHtmlPath, serialId, year);
			try
			{
				WebClient wc = new WebClient();
				wc.Encoding = Encoding.UTF8;
				string content = wc.DownloadString(url);
				if (!string.IsNullOrEmpty(content))
					CommonFunction.SaveFileContent(content, fileName, "utf-8");
				else
					File.Delete(fileName);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex.ToString());
			}
		}
		 * */
        /// <summary>
        /// 子品牌年款图片接口 1200改版 lisf 2016-8-18
        /// http://imgsvr.bitauto.com/carimagelist/SerialBrandCarYearIndex.ashx?SerialBrandId=xxxx&caryear=xxxx
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialYearPhotoHtmlNew(int serialId, int year)
        {
            string fileName = Path.Combine(photoConfig.SavePath,
                string.Format(@"SerialYearPhotoHtmlNew\{0}_{1}.html", serialId, year)
                );
            string url = string.Format(photoConfig.SerialYearPhotoHtmlPathNew, serialId, year);
            try
            {
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                string content = wc.DownloadString(url);
                if (!string.IsNullOrEmpty(content))
                    CommonFunction.SaveFileContent(content, fileName, "utf-8");
                else if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /*
		/// <summary>
		/// 车款图片接口
		/// http://imgsvr.bitauto.com/carimagelist/CarModelIndex.ashx?CarModelID=xxxxx
		/// </summary>
		/// <param name="carId"></param>
		public void CarPhotoHtml(int carId)
		{
			string fileName = Path.Combine(photoConfig.SavePath,
				string.Format(@"CarPhotoHtml\{0}.html", carId)
				);
			string url = string.Format(photoConfig.CarPhotoHtmlPath, carId);
			try
			{
				WebClient wc = new WebClient();
				wc.Encoding = Encoding.UTF8;
				string content = wc.DownloadString(url);
				if (!string.IsNullOrEmpty(content))
					CommonFunction.SaveFileContent(content, fileName, "utf-8");
				else
					File.Delete(fileName);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex.ToString());
			}
		}
		 * */
        /// <summary>
        /// 车款图片接口 1200改版 lisf 2016-8-18
        /// </summary>
        /// <param name="carId"></param>
        public void CarPhotoHtmlNew(int carId)
        {
            string fileName = Path.Combine(photoConfig.SavePath,
                string.Format(@"CarPhotoHtmlNew\{0}.html", carId)
                );
            string url = string.Format(photoConfig.CarPhotoHtmlPathNew, carId);
            try
            {
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                string content = wc.DownloadString(url);
                if (!string.IsNullOrEmpty(content))
                    CommonFunction.SaveFileContent(content, fileName, "utf-8");
                else if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /// <summary>
        /// 子品牌颜色实拍图
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialColorImage(int serialId)
        {
            try
            {
                List<SerialColorEntity> serialColorList = SerialService.GetProduceSerialColors(serialId);
                serialColorList.Sort((p1, p2) => p1.ColorYear - p1.ColorYear);
                List<string> list = new List<string>();
                foreach (SerialColorEntity entity in serialColorList)
                {
                    list.Add(string.Format("{0}-{1}", entity.ColorYear, entity.ColorId));
                }
                string fileName = Path.Combine(photoConfig.SavePath,
                string.Format(@"SerialColorImage\{0}.xml", serialId)
                );
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(string.Format(photoConfig.SerialColorImagePath, serialId, string.Join(",", list.ToArray())));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /// <summary>
        /// 新综述页 子品牌11张图片
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialElevenImage(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath,
            string.Format(@"SerialElevenImage\{0}.xml", serialId)
            );
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialElevenImagePath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取子品牌的官方颜色图
        /// </summary>
        /// <param name="serialBrandId"></param>
        public void SerialOfficalImage(int serialBrandId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialOfficalImage\{0}.xml", serialBrandId));
            var root = new XElement("ImageList");
            try
            {
                //添加官方图的外观图片
                var exteriorEle = GetPositionGroupImagElement(serialBrandId, 11, 6, 9);
                if (exteriorEle != null)
                    root.Add(exteriorEle);
                //添加官方图的内饰图片
                var innerEle = GetPositionGroupImagElement(serialBrandId, 11, 7, 9);
                if (innerEle != null)
                    root.Add(innerEle);
                var dir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var doc = new XDocument(root);
                doc.Save(fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }

        private XElement GetPositionGroupImagElement(int serialBrandId, int sourceId, int positionGroupId, int count)
        {
            try
            {
                var groupEle = new XElement("Group", new XAttribute("ID", positionGroupId));
                var loadDoc = XDocument.Load(string.Format(photoConfig.SerialOfficalImagePath, serialBrandId, sourceId, positionGroupId, count));
                var itemList = loadDoc.Descendants("Image");
                foreach (var element in itemList)
                {
                    var ele = new XElement("Image", new XAttribute("ImageID", element.Attribute("ImageID").Value),
                        new XAttribute("ImageName", element.Attribute("ImageName").Value),
                        new XAttribute("ImageUrl", element.Attribute("ImageUrl").Value)
                        );
                    groupEle.Add(ele);
                }
                return groupEle;
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 新综述页 子品牌 11张图 补图接口
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialDefaultCarFillImage(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath,
            string.Format(@"SerialDefaultCarFillImage\{0}.xml", serialId)
            );
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialDefaultCarFillImagePath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /// <summary>
        /// 子品牌颜色 实拍图
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialReallyColorImage(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath,
            string.Format(@"SerialReallyColorImage\{0}.xml", serialId)
            );
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(string.Format(photoConfig.SerialReallyColorImagePath, serialId));
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        #region 第四极图片数据
        public void SerialBrandFourthStageImage(int serialId)
        {
            string filePath = Path.Combine(photoConfig.SavePath, string.Format(@"SerialBrandFourthStageImage\{0}.xml", serialId));
            int[] positionIds = { 6, 7, 8 };
            XElement rootEle = new XElement("Root");
            foreach (int positionId in positionIds)
            {
                var ele = SerialFourthStagePositionImageEles(serialId, positionId);
                if (ele != null)
                    rootEle.Add(ele);
            }
            var sourceEle = SerialFourthStageSourceImageEles(serialId, 12);
            if (sourceEle != null)
                rootEle.Add(sourceEle);
            var dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            rootEle.Save(filePath);
        }

        private XElement BuildXElement(XDocument doc, int groupId)
        {
            var eles = doc.Descendants("Image");
            XElement imgEles = new XElement("Images", new XAttribute("GroupId", groupId));
            foreach (var ele in eles)
            {
                int imageId = Convert.ToInt32(ele.Attribute("ImageID").Value);
                string imageName = ele.Attribute("ImageName").Value;
                string imageUrl = ele.Attribute("ImageUrl").Value;
                imgEles.Add(new XElement("Image", new XAttribute("ImageID", imageId)
                                                   , new XAttribute("ImageName", imageName)
                                                   , new XAttribute("ImageUrl", imageUrl)));

            }
            return imgEles;
        }

        private XElement SerialFourthStagePositionImageEles(int serialId, int positionId)
        {
            string path = string.Format(photoConfig.SerialFourthStagePositionImagePath, serialId, positionId, 1, 3);
            try
            {
                XDocument doc = XDocument.Load(path);
                return BuildXElement(doc, positionId);
            }
            catch
            {
                return null;
            }
        }

        private XElement SerialFourthStageSourceImageEles(int serialId, int sourceId)
        {
            string path = string.Format(photoConfig.SerialFourthStageSourceImagePath, serialId, sourceId, 1, 3);
            try
            {
                XDocument doc = XDocument.Load(path);
                return BuildXElement(doc, sourceId);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 车款图片数量
        /// </summary>
        public void CarImageCount()
        {
            string fileName = Path.Combine(photoConfig.SavePath, "CarImageCount.xml");
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(photoConfig.CarImagesCountPath);
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }

        #endregion

        /// <summary>
        /// 保存车款实拍图
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="carId"></param>
        public void SerialCarReallyImage(int serialId, int carId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialCarReallyPic\{0}.xml", carId));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.Encoding = System.Text.Encoding.UTF8;                
                string url = string.Format(photoConfig.SerialCarReallyImagePath, serialId, carId);
                string content = wc.DownloadString(url);
                xmlDoc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(content, "Root");
                CommonFunction.SaveXMLDocument(xmlDoc, fileName);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /// <summary>
        /// 保存车系幻灯页图片
        /// </summary>
        /// <param name="serialId"></param>
        public void SerialSlidePageImage(int serialId)
        {
            string fileName = Path.Combine(photoConfig.SavePath, string.Format(@"SerialSlidePageImage\{0}.xml", serialId));
            try
            {
                string path = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string json = CommonFunction.GetResponseFromUrl(string.Format(photoConfig.SerialSlidePageImagePath, serialId));
                JObject jobject = (JObject)JsonConvert.DeserializeObject(json);

                List<XElement> elementList = new List<XElement>();
                if (string.IsNullOrEmpty(jobject["Data"].ToString()))
                {

                    XDocument docEmpty = new XDocument(
                     new XElement("ImageData",
                      new XAttribute("ImageUrlBase", "http://image.bitautoimg.com/autoalbum/"),
                       new XElement("ImageList")
                         )
                     );
                    docEmpty.Save(fileName);
                    return;
                }
                foreach (var item in (JArray)jobject["Data"]["DataList"])
                {
                    elementList.Add(
                            new XElement("ImageInfo",
                                 new XAttribute("ImageId", item["PhotoId"].ToString()),
                                 new XAttribute("ImageName", item["PositionName"].ToString()),
                                 new XAttribute("ImageUrl", string.Format(item["PhotoUrl"].ToString(), 4)),
                                 new XAttribute("GroupName", item["GroupName"].ToString()),
                                 new XAttribute("ClassId", "0"),
                                 new XAttribute("SerialId", item["ModelId"].ToString()),
                                 new XAttribute("CarId", item["StyleId"].ToString()),
                                 new XAttribute("CarYear", "0"),
                                 new XAttribute("CarModelName", item["StyleName"].ToString()),
                                 new XAttribute("Link", item["Url"].ToString())
                         ));
                    if (elementList.Count >= 8)
                    {
                        break;
                    }
                }

                XDocument doc = new XDocument(
                 new XElement("ImageData",
                  new XAttribute("ImageUrlBase", "http://image.bitautoimg.com/autoalbum/"),
                   new XElement("ImageList", elementList)
                     )
                 );
                doc.Save(fileName);
            }
            catch (Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }
        /*
		/// <summary>
		/// 子品牌三个外观标准图
		/// http://imgsvr.bitauto.com/autochannel/SerialThreeStandardImages.ashx?SerialBrandId={0}
		/// </summary>
		/// <param name="serialId"></param>
		public void SerialThreeStandardImage(int serialId)
		{
			string fileName = Path.Combine(photoConfig.SavePath,
			string.Format(@"SerialThreeStandardImage\{0}.xml", serialId)
			);
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(string.Format(photoConfig.SerialThreeStandardImagePath, serialId));
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex.ToString());
			}
		}
		/// <summary>
		/// 子品牌年款的的颜色
		/// http://imgsvr.bitauto.com/autochannel/SerialCarYearColorUrl.ashx?SerialBrandId={0}
		/// </summary>
		/// <param name="serialId"></param>
		public void SerialYearColorUrl(int serialId)
		{
			string fileName = Path.Combine(photoConfig.SavePath,
			string.Format(@"SerialYearColorUrl\{0}.xml", serialId)
			);
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(string.Format(photoConfig.SerialYearColorUrlPath, serialId));
				CommonFunction.SaveXMLDocument(xmlDoc, fileName);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex.ToString());
			}
		}
		 */
        #endregion
        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="logText"></param>
        public void OnLog(string logText, bool nextLine)
        {
            if (Log != null)
                Log(this, new LogArgs(logText, nextLine));
        }
    }
}
