using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.WebServiceModel;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
    public class NewsDAL
    {
        public static string addXmlRootElement(string data)
        {
            string root = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root> ";
            string result = root + data + "</root>";
            return result;
        }
        public bool Update(NewsEntity newsEntity)
        {
            try
            {
                //获取所有总部编辑
                GetAllZBEditorIdInfo();

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("SerialId", Type.GetType("System.Int32")));
                dt.Columns.Add(new DataColumn("CarId", Type.GetType("System.Int32")));
                dt.Columns.Add(new DataColumn("Title", typeof(string)));
                dt.Columns.Add(new DataColumn("Content", typeof(string)));
                dt.Columns.Add(new DataColumn("PageUrl", typeof(string)));
                dt.Columns.Add(new DataColumn("PageIndex", Type.GetType("System.Int32")));
                dt.Columns.Add(new DataColumn("YearType", Type.GetType("System.Int32")));
                string author = ConvertHelper.GetString(newsEntity.Author);
                int categoryId = ConvertHelper.GetInteger(newsEntity.CategoryId);
                var copyright = newsEntity.CopyRight;
                string imageCoverUrl = ConvertHelper.GetString(newsEntity.ImageCoverUrl);
                int newsId = ConvertHelper.GetInteger(newsEntity.NewsId);
                var uniqueid = newsEntity.UniqueId;
                DateTime publishTime = ConvertHelper.GetDateTime(newsEntity.PublishTime);
                string relatedBrand = ConvertHelper.GetString(newsEntity.RelatedBrand);
                string shortTitle = ConvertHelper.GetString(newsEntity.ShortTitle);
                string turl = ConvertHelper.GetString(newsEntity.Url);
                string title = ConvertHelper.GetString(newsEntity.Title);
                string Summary = ConvertHelper.GetString(newsEntity.Summary);
                var SourceName = newsEntity.Source == null ? "" : newsEntity.Source.Name;
                var Sourceurl = newsEntity.Source == null ? "" : newsEntity.Source.Url;
                var EditorId = newsEntity.Editor == null ? 0 : newsEntity.Editor.Id;
                int findEditorIdIndex = lt.FindIndex(x => x == EditorId);
                int ZbEditorFlag = findEditorIdIndex > -1 ? 1 : 0;

                var EditorName = newsEntity.Editor == null ? "" : newsEntity.Editor.Name;
                var EditorUrl = newsEntity.Editor == null ? "" : newsEntity.Editor.Url;
                int commentCnt = ConvertHelper.GetInteger(newsEntity.CommentCount);
                string moreImagesStr = string.Empty;
                if (newsEntity.MoreImages.Count > 0)
                {
                    foreach (string moreImg in newsEntity.MoreImages)
                    {
                        moreImagesStr += "<moreImages>" + moreImg + "</moreImages>";
                    }
                    moreImagesStr = addXmlRootElement(moreImagesStr);
                }
                SqlXml xmlMoreImages = new SqlXml();
                if (!string.IsNullOrEmpty(moreImagesStr))
                {
                    using (XmlTextReader rdr = new XmlTextReader(moreImagesStr, XmlNodeType.Document, null))
                    {
                        xmlMoreImages = new SqlXml(rdr);
                    }
                }

                List<int> serialIdList = new List<int>();
                int pageIndex = 1;
                int pageCount = 0;
                foreach (var page in newsEntity.Pages)
                {
                    int serialid = ConvertHelper.GetInteger(page.SerialId);
                    if (serialid <= 0 || serialIdList.Contains(serialid))
                        continue;

                    var carid = ConvertHelper.GetInteger(page.Carid);
                    var content = ConvertHelper.GetString(page.Content);
                    var pageUrl = page.PageUrl;
                    if (string.IsNullOrEmpty(pageUrl))
                    {
                        pageUrl = turl;
                    }
                    var pagetitle = ConvertHelper.GetString(page.Title);
                    if (string.IsNullOrEmpty(pagetitle))
                    {
                        pagetitle = title;
                    }
                    //获取车款 年款
                    BitAuto.CarDataUpdate.Common.Model.CarBaseInfoEntity carEntity = null;
                    if (carid > 0)
                    {
                        carEntity = Common.Services.CarService.GetCarInfoById(carid);
                    };

                    serialIdList.Add(serialid);
                    DataRow dr = dt.NewRow();
                    dr["SerialId"] = serialid;
                    dr["CarID"] = carid;
                    dr["Title"] = pagetitle;
                    dr["Content"] = content;
                    dr["PageUrl"] = pageUrl;
                    dr["PageIndex"] = pageIndex;
                    dr["YearType"] = carEntity == null ? 0 : carEntity.YearType;
                    dt.Rows.Add(dr);
                    pageIndex++;
                    pageCount++;
                }
                var arrSerialId = relatedBrand.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => ConvertHelper.GetInteger(p));
                if (!arrSerialId.Any())
                {
                    Log.WriteLog("新闻操作success,主新闻未关联车型，NewsId=" + newsEntity.NewsId);
                    //未关联车系 删除操作
                    Delete(uniqueid);
                    return true;
                }
                var exceptSerialId = arrSerialId.Except(serialIdList.ToArray());
                foreach (int serialId in exceptSerialId)
                {
                    DataRow dr = dt.NewRow();
                    dr["SerialId"] = serialId;
                    dr["CarID"] = 0;
                    dr["Title"] = title;
                    dr["Content"] = "";
                    dr["PageUrl"] = turl;
                    dr["PageIndex"] = 0;
                    dr["YearType"] = 0;
                    dt.Rows.Add(dr);
                }
                //if (dt.Rows.Count <= 0)
                //{
                //    Log.WriteLog("分页新闻无关联车型，NewsId=" + newsEntity.NewsId);
                //}
                SqlParameter[] _params = {
                                          new SqlParameter("@DTCarPageNews",SqlDbType.Structured),
                                          new SqlParameter("@Guid",SqlDbType.UniqueIdentifier),
                                          new SqlParameter("@NewsId",SqlDbType.Int),
                                          new SqlParameter("@CategoryId",SqlDbType.Int),
                                          new SqlParameter("@Author",SqlDbType.NVarChar),
                                          new SqlParameter("@Title",SqlDbType.NVarChar),
                                          new SqlParameter("@ShortTitle",SqlDbType.NVarChar),
                                          new SqlParameter("@PageIndex",SqlDbType.Int),
                                          new SqlParameter("@Summary",SqlDbType.NVarChar),
                                          new SqlParameter("@PublishTime",SqlDbType.DateTime),
                                          new SqlParameter("@ImageConverUrl",SqlDbType.VarChar),
                                          new SqlParameter("@CopyRight",SqlDbType.SmallInt),
                                          new SqlParameter("@SourceName",SqlDbType.NVarChar),
                                          new SqlParameter("@SourceUrl",SqlDbType.VarChar),
                                          new SqlParameter("@EditorId",SqlDbType.Int),
                                           new SqlParameter("@ZbEditor",SqlDbType.TinyInt),
                                          new SqlParameter("@EditorName",SqlDbType.NVarChar),
                                          new SqlParameter("@EditorUrl",SqlDbType.VarChar),
                                          new SqlParameter("@PageCount",SqlDbType.Int) ,
                                          new SqlParameter("@CommentCount",SqlDbType.Int) ,
                                          new SqlParameter("@MoreImages", SqlDbType.Xml),//xmlMoreImages.Value.Length),
                                          new SqlParameter("@errMsg",SqlDbType.VarChar,255),
                                          new SqlParameter("@errCode",SqlDbType.Int,4)
                                                 };

                _params[0].Value = dt;
                _params[1].Value = uniqueid;
                _params[2].Value = newsId;
                _params[3].Value = categoryId;
                _params[4].Value = author;
                _params[5].Value = title;
                _params[6].Value = shortTitle;
                _params[7].Value = pageIndex;
                _params[8].Value = Summary;
                _params[9].Value = publishTime;
                _params[10].Value = imageCoverUrl;
                _params[11].Value = copyright;
                _params[12].Value = SourceName;
                _params[13].Value = Sourceurl;
                _params[14].Value = EditorId;
                _params[15].Value = ZbEditorFlag;
                _params[16].Value = EditorName;
                _params[17].Value = EditorUrl;
                _params[18].Value = pageIndex - 1;
                _params[19].Value = commentCnt;
                _params[20].Value = xmlMoreImages;
                _params[21].Direction = ParameterDirection.Output;
                _params[22].Direction = ParameterDirection.Output;

                SqlHelper.ExecuteNonQuery(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.StoredProcedure, @"SP_CarNewsV2_UpdateNews", _params);

                string errMsg = _params[21].Value.ToString();
                int errCode = _params[22].Value == DBNull.Value ? 0 : Convert.ToInt32(_params[22].Value);
                dt.Clear();

                if (errCode == 101)
                {
                    Log.WriteLog("新闻操作Success，guid=" + uniqueid + "\r\n");
                    return true;
                }
                else
                {
                    Log.WriteLog("新闻操作失败，guid=" + uniqueid + "\r\n" + errMsg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("新闻操作异常:" + ex.ToString());
                return false;
            }
        }
        public bool Delete(Guid guid)
        {
            SqlParameter[] _params = {
                                          new SqlParameter("@NewsGuid",SqlDbType.UniqueIdentifier),
                                            new SqlParameter("@errCode",SqlDbType.Int,4)
                                                 };

            _params[0].Value = guid;
            _params[1].Direction = ParameterDirection.Output;
            SqlHelper.ExecuteNonQuery(Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.StoredProcedure, "SP_CarNewsV2_DeleteNews", _params);

            int errCode = _params[1].Value == DBNull.Value ? 0 : Convert.ToInt32(_params[1].Value);
            if (errCode == 1)
            {
                Log.WriteLog("删除新闻操作Success，guid=" + guid);
                return true;
            }
            else
            {
                Log.WriteLog("删除新闻操作失败，guid=" + guid);
                return false;
            }
        }
        private static List<int> lt = new List<int>();
        public static void GetAllZBEditorIdInfo()
        {
            string url = Common.CommonData.CommonSettings.EidtorUserUrl;     //    http://api.admin.bitauto.com/api/list/EidtorUser.aspx?zb=1
            string responseData = string.Empty;
            responseData = GetHttpRequestData(url);
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(responseData);
            XmlNodeList xnl = xd.SelectNodes("/Root/User/UserId");
            foreach (XmlNode xn in xnl)
            {
                int editorId = Convert.ToInt32(xn.InnerText.Trim());
                lt.Add(editorId);
            }
        }
        public static string GetHttpRequestData(string url, int interval = 60000)
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
            catch (WebException ex)
            {
                Log.WriteLog("判断当前新闻是否为总部编辑：请求Url异常，Url=" + url + "\r\n" + ex.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteLog("判断当前新闻是否为总部编辑：请求Url异常，Url=" + url + "\r\n" + ex.ToString());
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
