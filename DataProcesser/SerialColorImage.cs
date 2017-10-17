using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;
using System.Data;
using System.Xml;
using BitAuto.Utils;
using System.IO;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;
using System.Data.SqlClient;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class SerialColorImage
    {
        public event LogHandler Log;
        private string _RootPath = string.Empty;
        private struct SerialColorRGB
        {
            public string ColorName;
            public string ColorRGB;
            public SerialColorRGB(string colorName, string colorRGB)
            {
                this.ColorName = colorName;
                this.ColorRGB = colorRGB;
            }
        }
        private Dictionary<int, string> _serialList = new Dictionary<int, string>();
        private Dictionary<int, List<SerialColorRGB>> _serialColors = new Dictionary<int, List<SerialColorRGB>>();
        private Dictionary<int, XmlNode> _serialOutSetList = new Dictionary<int, XmlNode>();


        public SerialColorImage()
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialSet\\SerialColorImage");
        }

        #region 数据初始化
        private void InitData(int serialId)
        {
            InitSerialList(serialId);
            InitSerialColors(serialId);
            InitSerialOutSetList(serialId);
        }
        private void InitSerialList(int serialId)
        {
            OnLog("初始化 车型列表", false);
            DataSet ds = null;
            if (serialId <= 0)
            {
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, @"select cs_id, isnull(cs_seoname, csName) as cs_seoname from Car_Serial where isState=0");
            }
            else
            {
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, @"select cs_id, isnull(cs_seoname, csName) as cs_seoname from Car_Serial where isState=0 and cs_id=@cs_id", new SqlParameter("@cs_id", serialId));
            }
            if (ds == null || ds.Tables.Count < 1)
                return;

            DataRowCollection rows = ds.Tables[0].Rows;
            if (rows.Count < 1)
                return;
            int tempSerialId;
            foreach (DataRow row in rows)
            {
                tempSerialId = ConvertHelper.GetInteger(row["cs_id"].ToString());
                _serialList.Add(tempSerialId, row["cs_seoname"].ToString());
            }
            OnLog(string.Format("初始化 车型列表 完成。获取数量:{0}", _serialList.Count), false);
        }
        private void InitSerialColors(int serialId)
        {
            OnLog("初始化 车型颜色", false);
            DataSet ds = null;
            if (serialId <= 0)
            {
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, @"SELECT distinct a.Cs_Id, a.car_yeartype, b.Pvalue AS CarColor, cs.CsSaleState FROM Car_relation a 
inner join Car_Serial cs on cs.cs_id=a.cs_id
INNER JOIN CarDataBase b ON a.Car_Id=b.CarId AND b.paramid=598
WHERE a.IsState=0 and (cs.CsSaleState = '停销' or (cs.CsSaleState <> '停销' and a.car_ProduceState=92));
select cs_id,colorName,colorRGB from dbo.Car_SerialColor where colorname is not null and colorname <> '' and colorrgb is not null and colorrgb<>'' order by cs_id, colorRGB");
            }
            else
            {
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, @"SELECT distinct a.Cs_Id, a.car_yeartype, b.Pvalue AS CarColor, cs.CsSaleState FROM Car_relation a 
inner join Car_Serial cs on cs.cs_id=a.cs_id
INNER JOIN CarDataBase b ON a.Car_Id=b.CarId AND b.paramid=598
WHERE a.IsState=0 and cs.cs_id=@cs_id and (cs.CsSaleState = '停销' or (cs.CsSaleState <> '停销' and a.car_ProduceState=92));
select cs_id,colorName,colorRGB from dbo.Car_SerialColor where colorname is not null and colorname <> '' and colorrgb is not null and colorrgb<>'' and cs_id=@cs_id order by cs_id, colorRGB", new SqlParameter("@cs_id", serialId));
            }
            if (ds == null || ds.Tables.Count < 2 || _serialList == null || _serialList.Count < 1)
                return;
            List<SerialColorRGB> colorRGB = null;
            string expression;
            DataTable carColorTable = ds.Tables[0], serialColorTable = ds.Tables[1];
            List<string> colorList = null;
            DataRow[] carColors = null, serialColors = null;
            foreach (int tempSerialId in _serialList.Keys)
            {
                expression = string.Format("Cs_Id={0}", tempSerialId);
                carColors = carColorTable.Select(expression);
                if (carColors.Length <= 0)
                    continue;

                serialColors = serialColorTable.Select(expression);
                if (serialColors.Length <= 0)
                    continue;

                colorList = GetSerialColors(carColors);

                colorRGB = new List<SerialColorRGB>();

                foreach (DataRow serialColorRow in serialColors)
                {
                    string carColorName = serialColorRow["colorName"].ToString();
                    if (colorList.Contains(carColorName) && !colorRGB.Exists(item => item.ColorName == carColorName))
                    {
                        colorRGB.Add(new SerialColorRGB(carColorName, serialColorRow["colorRGB"].ToString()));
                    }
                }
                if (colorRGB.Count > 0)
                    _serialColors.Add(tempSerialId, colorRGB);
            }

            OnLog(string.Format("初始化 车型颜色 完成。获取数量:{0}", _serialColors.Count), true);
        }
        private void InitSerialOutSetList(int serialId)
        {
            OnLog("初始化 车型外形尺寸", false);
            XmlDocument doc;
            if (ExistsLoaclXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, "SerialSet\\SerialOutSet.xml"), out doc))
            {
                string xpath = string.Empty;
                if (serialId <= 0)
                {
                    xpath = "Root/Serial/YearType[1]";
                }
                else
                {
                    xpath = string.Format("Root/Serial[@id='{0}']/YearType[1]", serialId);
                }
                XmlNodeList nodeList = doc.SelectNodes(xpath);
                if (nodeList.Count > 0)
                {
                    int serialid;
                    foreach (XmlNode node in nodeList)
                    {
                        serialid = ConvertHelper.GetInteger(node.ParentNode.Attributes["id"].Value);
                        if (serialid != 0)
                            _serialOutSetList.Add(serialid, node);
                    }
                }
            }
            OnLog(string.Format("初始化 车型外形尺寸 完成。获取数量:{0}", _serialOutSetList.Count), false);
        }
        #endregion
        /// <summary>
        /// 删除文件
        /// </summary>
        private void DeleteFiles(int serialId)
        {
            OnLog(string.Format("删除旧文件 id:{0}...", serialId), true);
            try
            {
                int count = 0;
                if (serialId > 0)
                {
                    string filePath = Path.Combine(_RootPath, string.Format("SerialColorImage_{0}.html", serialId));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        count++;
                    }
                }
                else
                {
                    string[] files = Directory.GetFiles(_RootPath, "*.html");
                    foreach (string file in files)
                    {
                        File.Delete(file);
                        count++;
                    }
                }
                OnLog(string.Format("完成 删除旧文件 id:{0}, 删除数量：{1}。", serialId, count), true);
            }
            catch (Exception exp)
            {
                OnLog(string.Format("异常 删除旧文件 error:{0}, stackTrace:{1}。", exp.Message, exp.StackTrace), true);
            }
        }
        #region  del by lsf 2016-01-06
        /*
        public void MakeSerialImageCarsHTML(int serialId)
        {
            OnLog("开始 生成子品牌综述页图释块，子品牌id为:" + serialId, true);
            InitData(serialId);
            DeleteFiles(serialId);

            if (_serialList != null)
            {
                if (ExistsDirectory())
                {
                    foreach (KeyValuePair<int, string> keyValue in _serialList)
                    {
                        //OnLog(string.Format("生成【id:{0},name:{1}】图释块", keyValue.Key, keyValue.Value), true);
                        string html = GetSerialImageCarHTML(keyValue);
                        if (string.IsNullOrEmpty(html))
                        {
                            OnLog("未生成图释块", true);
                            continue;
                        }

                        //OnLog("保存到文件", true);
                        CommonFunction.SaveFileContent(html, Path.Combine(_RootPath, string.Format("SerialColorImage_{0}.html", keyValue.Key)), Encoding.UTF8);
                    }
                }
            }
            //OnLog("结束 生成子品牌综述页图释块", true);
        }
        public void MakeSerialImageCarsHTMLALL()
        {
            OnLog("		开始 生成子品牌综述页图释块", true);
            InitData(0);
            DeleteFiles(0);
            if (_serialList != null)
            {
                if (ExistsDirectory())
                {
                    foreach (KeyValuePair<int, string> keyValue in _serialList)
                    {
                        //OnLog(string.Format("生成【id:{0},name:{1}】图释块", keyValue.Key, keyValue.Value), true);
                        string html = GetSerialImageCarHTML(keyValue);
                        if (string.IsNullOrEmpty(html))
                        {
                            OnLog(string.Format("未生成图释块.[{0}],[{1}]", keyValue.Value.ToString(), keyValue.Key.ToString()), true);
                            continue;
                        }

                        //OnLog("保存到文件", true);
                        CommonFunction.SaveFileContent(html, Path.Combine(_RootPath, string.Format("SerialColorImage_{0}.html", keyValue.Key)), Encoding.UTF8);
                    }
                }
            }
            //OnLog("结束 生成子品牌综述页图释块", true);
        }
        private string GetSerialImageCarHTML(KeyValuePair<int, string> serialInfo)
        {
            bool isContinue = false;
            int serialId = serialInfo.Key;
            if (!_serialColors.ContainsKey(serialId))
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder(8000);
            StringBuilder colorBuilder = new StringBuilder(4000);

            stringBuilder.AppendFormat("<div class=\"line_box\"><h3><span>{0}-图释 </span></h3>", serialInfo.Value);

            #region 图释左半部分
            stringBuilder.Append("<div class=\"carExample\">");
            stringBuilder.Append("<div class=\"exampleCarPic\" id=\"exampleCarPic\">");
            //获取颜色
            Dictionary<string, XmlNode> dicColor = GetSerialColorPhotoByCsID(serialId);
            if (dicColor != null && dicColor.Count > 0)
            {
                int index = 0;
                List<SerialColorRGB> notImgList = new List<SerialColorRGB>();
                List<SerialColorRGB> serialColors = _serialColors[serialId];

                XmlDocument carRealPicDoc = new XmlDocument();
                // modified by chengl Mar.8.2013
				// ExistsLoaclXmlDocument(Path.Combine(CommonData.CommonSettings.CarRealPicPath, serialId.ToString() + ".xml"), out carRealPicDoc);
				carRealPicDoc.Load(string.Format(CommonData.CommonSettings.CarRealPicPath, serialId.ToString()));

                foreach (SerialColorRGB colorRGB in serialColors)
                {
                    if (dicColor.ContainsKey(colorRGB.ColorName))
                    {
                        int carRealPicId = 0;
						if (carRealPicDoc != null && carRealPicDoc.HasChildNodes)
                        {
                            XmlNode realpicNode = carRealPicDoc.SelectSingleNode(string.Format("//O[@Name=\"{0}\"]", colorRGB.ColorName));
                            if (realpicNode != null)
                            {
                                carRealPicId = ConvertHelper.GetInteger(realpicNode.Attributes["ID"].Value);
                            }
                        }
                        XmlNode node = dicColor[colorRGB.ColorName];
                        if (HasXmlNodeAttributeValue(node, "ImageUrl") && HasXmlNodeAttributeValue(node, "Link"))
                        {
                            string carRealPicHtml = string.Empty;
                            string link = node.Attributes["Link"].Value.Trim(), image = node.Attributes["ImageUrl"].Value.Trim();
							if (index == 0)
							{
								if (carRealPicId > 0)
								{
									carRealPicHtml = string.Format("| <a href=\"http://photo.bitauto.com/serial/{0}/c{1}/\" target=\"_blank\">看实拍</a>", serialId, carRealPicId);
								}
								stringBuilder.AppendFormat("<a href=\"{0}\" target=\"_blank\"><span><img data-original=\"{1}\" alt=\"\" /></span></a>", link, image);
								colorBuilder.AppendFormat("<li><a href=\"{0}\" target=\"_blank\"><em class=\"current\"><span style=\"background:{1}\">{2}</span></em><b>{2}</b></a><strong>{3}</strong></li>", link, colorRGB.ColorRGB, colorRGB.ColorName, carRealPicHtml);
								index++;
							}
							else
							{
								if (carRealPicId > 0)
								{
									carRealPicHtml = string.Format("| <a href=\"http://photo.bitauto.com/serial/{0}/c{1}/\" target=\"_blank\">看实拍</a>", serialId, carRealPicId);
								}
								stringBuilder.AppendFormat("<a href=\"{0}\" target=\"_blank\"><span><img data-original=\"{1}\" alt=\"\" /></span></a>", link, image);
								colorBuilder.AppendFormat("<li><a href=\"{0}\" target=\"_blank\"><em><span style=\"background:{1}\">{1}</span></em><b style=\"display:none\">{2}</b></a><strong style=\"display:none;\">{3}</strong></li>", link, colorRGB.ColorRGB, colorRGB.ColorName, carRealPicHtml);
							}
                            isContinue = true;
                        }
                        else
                        {
                            notImgList.Add(colorRGB);
                        }
                    }
                    else
                    {
                        notImgList.Add(colorRGB);
                    }
                }

                if (isContinue && notImgList.Count > 0)
                {
                    foreach (SerialColorRGB colorNotImg in notImgList)
                    {
                        string carRealPicHtml = string.Empty;
                        if (carRealPicDoc != null)
                        {
                            XmlNode realpicNode = carRealPicDoc.SelectSingleNode(string.Format("//O[@Name=\"{0}\"]", colorNotImg.ColorName));
                            if (realpicNode != null)
                            {
                                carRealPicHtml = string.Format("| <a href=\"http://photo.bitauto.com/serial/{0}/c{1}/\" target=\"_blank\">看实拍</a>", serialId, ConvertHelper.GetInteger(realpicNode.Attributes["ID"].Value));
                            }
                        }

                        stringBuilder.AppendFormat("<a><span><img data-original=\"{0}\" alt=\"\" /></span></a>", CommonData.CommonSettings.SerialColorImageDefaultUrl);
                        colorBuilder.AppendFormat("<li><a><em><span style=\"background:{0}\">{1}</span></em><b style=\"display:none\">{1}</b></a><strong style=\"display:none;\">{2}</strong></li>", colorNotImg.ColorRGB, colorNotImg.ColorName, carRealPicHtml);
                    }
                }
            }

            if (!isContinue)
                return string.Empty;

            stringBuilder.Append("</div>");
            stringBuilder.Append("<a href=\"javascript:void(0)\" class=\"arrowUp\" id=\"arrowUp\" >向上</a>");
            stringBuilder.Append("<div class=\"exampleCarColorBox\" id=\"exampleCarColor\" style=\"margin-top:28px;\">");
            stringBuilder.Append("<ul class=\"exampleCarColor\" id=\"colorBox\" style=\"top:0; left:0\">");
            stringBuilder.Append(colorBuilder.ToString());
            stringBuilder.Append("</ul>");
            stringBuilder.Append("</div>");
            stringBuilder.Append("<a href=\"javascript:void(0)\" class=\"arrowDown\" id=\"arrowDown\" >向下</a>");
            stringBuilder.Append("</div>");
            #endregion

            //分割线
            stringBuilder.Append("<div class=\"exampleVdash\"></div>");

            #region 图释右半部分

            bool hasImg = false;
            string length = string.Empty, width = string.Empty, height = string.Empty, wheelbase = string.Empty, fronttread = string.Empty, backtread = string.Empty;
            string dir = "default";
            string imgZm60 = string.Empty, imgZm150 = string.Empty, imgZc60 = string.Empty, imgZc150 = string.Empty, imgBm60 = string.Empty, imgBm150 = string.Empty;

            //hasimg="0" length="4755" string"1795" height="1440" wheelbase="2725" fronttread="1560" backtread="1560"
            if (_serialOutSetList.ContainsKey(serialId))
            {
                XmlNode yearNode = _serialOutSetList[serialId];
                if (yearNode.Attributes["hasimg"] != null && !string.IsNullOrEmpty(yearNode.Attributes["hasimg"].Value) && yearNode.Attributes["hasimg"].Value == "1")
                {
                    //"http://t.image.bitauto.com/images/carsize/{0}/{0}_{1}_{2}_{3}.jpg";
                    //http://t.image.bitauto.com/images/carsize/1560/1560_150_zc_2005.jpg
                    string serialIdStr = serialId.ToString(), yearStr = yearNode.Attributes["year"].Value;
                    imgZm60 = string.Format(CommonData.CommonSettings.SerialOutSetWebPath, serialIdStr, "60", "zm", yearStr);
                    imgZm150 = string.Format(CommonData.CommonSettings.SerialOutSetWebPath, serialIdStr, "150", "zm", yearStr);
                    imgZc60 = string.Format(CommonData.CommonSettings.SerialOutSetWebPath, serialIdStr, "60", "zc", yearStr);
                    imgZc150 = string.Format(CommonData.CommonSettings.SerialOutSetWebPath, serialIdStr, "150", "zc", yearStr);
                    imgBm60 = string.Format(CommonData.CommonSettings.SerialOutSetWebPath, serialIdStr, "60", "bm", yearStr);
                    imgBm150 = string.Format(CommonData.CommonSettings.SerialOutSetWebPath, serialIdStr, "150", "bm", yearStr);
                    hasImg = true;
                }
                else
                {
                    switch (yearNode.Attributes["carlevel"].Value)
                    {
                        case "MPV":
                            dir = "mpv";
                            break;
                        case "SUV":
                            dir = "suv";
                            break;
                        case "面包车":
                            dir = "mianbaoche";
                            break;
                        case "跑车":
                            dir = "paoche";
                            break;
                        case "皮卡":
                            dir = "pika";
                            break;
                        case "豪华车":
                        case "紧凑型车":
                        case "微型车":
                        case "小型车":
                        case "中大型车":
                        case "中型车":
                            if (string.IsNullOrEmpty(yearNode.Attributes["carbodyform"].Value) || yearNode.Attributes["carbodyform"].Value == "三厢轿车")
                            {
                                dir = "3xiang";
                            }
                            else
                            {
                                dir = "2xiang";
                            }
                            break;
                    }
                }
                if (yearNode.Attributes["length"] != null && !string.IsNullOrEmpty(yearNode.Attributes["length"].Value))
                    length = yearNode.Attributes["length"].Value + "mm";
                if (yearNode.Attributes["width"] != null && !string.IsNullOrEmpty(yearNode.Attributes["width"].Value))
                    width = yearNode.Attributes["width"].Value + "mm";
                if (yearNode.Attributes["height"] != null && !string.IsNullOrEmpty(yearNode.Attributes["height"].Value))
                    height = yearNode.Attributes["height"].Value + "mm";
                if (yearNode.Attributes["wheelbase"] != null && !string.IsNullOrEmpty(yearNode.Attributes["wheelbase"].Value))
                    wheelbase = yearNode.Attributes["wheelbase"].Value + "mm";
                if (yearNode.Attributes["fronttread"] != null && !string.IsNullOrEmpty(yearNode.Attributes["fronttread"].Value))
                    fronttread = yearNode.Attributes["fronttread"].Value + "mm";
                if (yearNode.Attributes["backtread"] != null && !string.IsNullOrEmpty(yearNode.Attributes["backtread"].Value))
                    backtread = yearNode.Attributes["backtread"].Value + "mm";
            }
            if (!hasImg)
            {
                imgZm60 = string.Format(CommonData.CommonSettings.SerialOutSetDefaultWebPath, dir, "zm", "60");
                imgZm150 = string.Format(CommonData.CommonSettings.SerialOutSetDefaultWebPath, dir, "zm", "150");
                imgZc60 = string.Format(CommonData.CommonSettings.SerialOutSetDefaultWebPath, dir, "zc", "60");
                imgZc150 = string.Format(CommonData.CommonSettings.SerialOutSetDefaultWebPath, dir, "zc", "150");
                imgBm60 = string.Format(CommonData.CommonSettings.SerialOutSetDefaultWebPath, dir, "bm", "60");
                imgBm150 = string.Format(CommonData.CommonSettings.SerialOutSetDefaultWebPath, dir, "bm", "150");
            }
			stringBuilder.Append("<div class=\"carPicSize\" id=\"carPicSize\">");
            stringBuilder.Append("<div class=\"mainPic\" id=\"sizeMainPic\">");
            stringBuilder.Append("<div><img data-original=\"" + imgZc150 + "\" alt=\"\" /><span class=\"carSizeTop\">" + length + "</span><span class=\"carSizeBottom\">" + wheelbase + "</span></div>");
            stringBuilder.Append("<div style=\"display:none\"><img data-original=\"" + imgZm150 + "\" alt=\"\" /><span class=\"carSizeTop\">" + width + "</span><span class=\"carSizeBottom\">" + fronttread + "</span></div>");
            stringBuilder.Append("<div style=\"display:none\"><img data-original=\"" + imgBm150 + "\" alt=\"\" /><span class=\"carSizeBottom\">" + backtread + "</span><span class=\"carSizeRight\">" + height + "</span></div>");
            stringBuilder.Append("</div>");

            stringBuilder.Append("<ul id=\"sizeSubPic\">");
            stringBuilder.Append("<li class=\"current\"><a><img data-original=\"" + imgZc60 + "\" alt=\"\" /></a><p>侧面</p></li>");
            stringBuilder.Append("<li><a><img data-original=\"" + imgZm60 + "\" alt=\"\" /></a><p>正面</p></li>");
            stringBuilder.Append("<li><a><img data-original=\"" + imgBm60 + "\" alt=\"\" /></a><p>背面</p></li>");
            stringBuilder.Append("</ul>");
            stringBuilder.Append("</div>");

            #endregion

            stringBuilder.Append("<div class=\"clear\"></div></div>");

            return stringBuilder.ToString();
        }
        
        private bool HasXmlNodeAttributeValue(XmlNode currentNode, string attributeName)
        {
            return (currentNode != null && currentNode.Attributes[attributeName] != null && !string.IsNullOrEmpty(currentNode.Attributes[attributeName].Value.Trim()));
        }
         * */
        #endregion
        
        /// <summary>
        /// 检测XML文件是否存在
        /// </summary>
        /// <returns></returns>
        private bool ExistsLoaclXmlDocument(string xmlPath, out XmlDocument xmlDoc)
        {
            bool result = false;
            xmlDoc = null;
            if (!string.IsNullOrEmpty(xmlPath) && File.Exists(xmlPath))
            {
                FileStream stream = null;
                XmlReader reader = null;
                try
                {
                    stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read);
                    reader = XmlReader.Create(stream);
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(reader);
                    result = true;
                }
                catch (Exception exp)
                {
                    OnLog("Error Read XML (Path:" + xmlPath + ";message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")", true);
                }
                finally
                {
                    if (reader != null && reader.ReadState != ReadState.Closed)
                        reader.Close();
                    if (stream != null)
                        stream.Dispose();
                }
            }
            else
            {
                OnLog("Error XML File Not Found (Path:" + xmlPath + ")", true);
            }
            return result;
        }
        /// <summary>
        /// 检测XML文件是否存在
        /// </summary>
        /// <returns></returns>
        private bool ExistsWebXmlDocument(string xmlPath, out XmlDocument xmlDoc)
        {
            bool result = false;
            xmlDoc = null;
            if (!string.IsNullOrEmpty(xmlPath))
            {
                XmlReader reader = null;
                try
                {
                    reader = XmlReader.Create(xmlPath);
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(reader);
                    result = true;
                }
                catch (Exception exp)
                {
                    OnLog("Error Read XML (Path:" + xmlPath + ";message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")", true);
                }
                finally
                {
                    if (reader != null && reader.ReadState != ReadState.Closed)
                        reader.Close();
                }
            }
            else
            {
                OnLog("Error XML File Not Found (Path:" + xmlPath + ")", true);
            }
            return result;
        }
        /*  // del by lsf 2016-01-06
        /// <summary>
        /// 获取子品牌颜色图片
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, XmlNode> GetSerialColorPhotoByCsID(int serialId)
        {
            XmlDocument doc;
            Dictionary<string, XmlNode> dic = null;
            if (ExistsWebXmlDocument(string.Format(CommonData.CommonSettings.SerialColorPhotoUrl, serialId, 4, "false"), out doc))
            {
                dic = new Dictionary<string, XmlNode>();

                if (doc != null && doc.HasChildNodes)
                {
                    XmlNodeList xnl = null;

                    // 子品牌
                    xnl = doc.SelectNodes("/ImageData/ImageList/ImageInfo");
                    if (xnl != null && xnl.Count > 0)
                    {
                        foreach (XmlNode xn in xnl)
                        {
                            if (xn.Attributes["ImageName"] != null && xn.Attributes["ImageName"].Value.Trim() != "")
                            {
                                if (!dic.ContainsKey(xn.Attributes["ImageName"].Value.Trim()))
                                {
                                    dic.Add(xn.Attributes["ImageName"].Value.Trim(), xn);
                                }
                            }
                        }
                    }
                }
            }
            return dic;
        }
         * */
        private List<string> GetSerialColors(DataRow[] rows)
        {
            List<string> colorList = new List<string>();
            int year = 0;
            bool isStopSale = false;
            foreach (DataRow row in rows)
            {
                if (row["CsSaleState"].ToString() == "停销")
                {
                    isStopSale = true;
                    break;
                }
                int tempYear = ConvertHelper.GetInteger(row["car_yeartype"]);
                if (tempYear > year)
                    year = tempYear;
            }

            foreach (DataRow row in rows)
            {
                if (isStopSale || year == 0 || ConvertHelper.GetInteger(row["car_yeartype"]) == year)
                {
                    string[] colors = row["CarColor"].ToString().Replace("，", ",").Split(',');
                    foreach (string colorStr in colors)
                    {
                        if (colorStr.Length > 0 && !colorList.Contains(colorStr.Trim()))
                            colorList.Add(colorStr.Trim());
                    }
                }
            }

            return colorList;
        }
        /*  // del by lsf 2016-01-06
        /// <summary>
        /// 检测目录是否存在，如果不存在将创建
        /// </summary>
        /// <returns></returns>
        private bool ExistsDirectory()
        {
            if (!Directory.Exists(_RootPath))
            {
                OnLog("Start Create SerialSet\\SerialColorImage Directory (Path:" + _RootPath + ")...", true);
                try
                {
                    Directory.CreateDirectory(_RootPath);
                }
                catch (Exception exp)
                {
                    OnLog("Create SerialSet\\SerialColorImage Directory Error (Message:" + exp.Message + ";StackTrace:" + exp.StackTrace + ")...", true);
                    return false;
                }
                OnLog("End Create SerialSet\\SerialColorImage Directory ...", true);
            }
            return true;
        }*/
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
