using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BitAuto.Utils.Data;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Enum;
using System.Xml;

namespace BitAuto.CarDataUpdate.Common.Services
{
    public class FocusNewsService
    {
        /// <summary>
        /// 获取编辑排序新闻
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        public static Dictionary<int, NewsEntity> GetOrderNewsList(int serialId)
        {
            if (serialId < 1)
                return null;

            try
            {
                DataSet ds = FocusNewsRespository.GetOrderNewsData(serialId);
                if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
                    return null;
                DataRowCollection rows = ds.Tables[0].Rows;
                //最终新闻列表
                Dictionary<int, NewsEntity> newsList = new Dictionary<int, NewsEntity>(rows.Count);

                foreach (DataRow row in rows)
                {
                    int order = Convert.ToInt32(row["OrderNumber"].ToString());
                    if (!newsList.ContainsKey(order))
                    {
                        newsList.Add(order, GetNewsObjectByXmlNode(row));
                    }
                }
                return newsList;
            }
            catch (Exception exp)
            {
                Log.WriteErrorLog(exp);
                return null;
            }
        }
        /// <summary>
        /// 获取焦点新闻
        /// </summary>
        /// <param name="orderNewsList">排序新闻列表</param>
        /// <param name="serialId"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static List<NewsEntity> GetFocusNewsListNew(Dictionary<int, NewsEntity> orderNewsList, int serialId, int top = 8)
        {
            if (serialId < 1)
                return null;
            List<int> exitsNewsList = new List<int>();

            List<NewsEntity> endNewsList = new List<NewsEntity>();
            List<NewsEntity> newsList = new List<NewsEntity>();
            List<NewsEntity> videoNewsList = new List<NewsEntity>();
            DataSet ds = FocusNewsRespository.GetFocusNewsData(serialId);
            //if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            //    return null;
            // modified by chengl Mar.3.2014
            // 当没有符合的焦点新闻时 继续补视频和指定行为
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dtNews = ds.Tables[0];
                DataRow[] arrNewsRows = dtNews.Select("subnewstype=1");
                if (arrNewsRows.Length > 0)
                {
                    //循环取新闻
                    for (int i = 1; i < (top + 1); i++)
                    {
                        foreach (DataRow row in arrNewsRows)
                        {
                            int newsId = ConvertHelper.GetInteger(row["CmsNewsId"]);
                            if (newsId <= 0 || exitsNewsList.Contains(newsId))
                                continue;
                            int cateId = ConvertHelper.GetInteger(row["CategoryId"]);
                            if (!CommonData.CategoryPathDic.ContainsKey(cateId))
                                continue;

                            //如果当前顺序为1，而且当前分类又不包括在指定分类内
                            if (i == 1)
                            {
                                //如果没有原创标识或不是原创的不做为头条
                                if (string.IsNullOrEmpty(row["CreativeType"].ToString()))
                                    continue;
                                int newsType = ConvertHelper.GetInteger(row["CreativeType"]);
                                if (newsType != 0)
                                    continue;
                                //不在头条分类范围内的，不做为头条
                                if (IsNoContainsNews(CommonData.CategoryPathDic[cateId]))
                                    continue;
                            }
                            newsList.Add(GetNewsObjectByXmlNode(row));
                            exitsNewsList.Add(newsId);
                            break;
                        }
                    }
                }
            }
            //add by sk 2013-09-13 新版视频
            List<VideoEntityV2> videoList = VideoService.GetVideoList(serialId, VideoEnum.VideoSource.All, top);
            for (int i = 0; i < videoList.Count; i++)
            {
                if (newsList.Count >= (top - 2) && i >= 2) break;
                if (newsList.Count < (top - 2) && i >= (top - newsList.Count)) break;
                NewsEntity newsObject = new NewsEntity();
                newsObject.NewsId = (int)videoList[i].VideoId;
                newsObject.Title = videoList[i].Title;
                newsObject.FaceTitle = videoList[i].ShortTitle;
                newsObject.PageUrl = videoList[i].ShowPlayUrl;
                newsObject.PublishTime = videoList[i].Publishtime;
                newsObject.CategoryId = newsObject.CategoryId;
                newsObject.NewsCategoryShowName = new NewsCategoryShowName() { CategoryShowName = "视频", CategoryUrl = "shipin" };
                newsObject.Author = videoList[i].EditorName;
                newsObject.ImageLink = videoList[i].ImageLink.Replace("/Video/", "/newsimg-180-w0/Video/");
                videoNewsList.Add(newsObject);
            }

            var topNews = newsList.Take(1).ToList();
            endNewsList.AddRange(newsList.Skip(1));
            endNewsList.AddRange(videoNewsList);
            //最后对结果进行排序
            endNewsList.Sort((p1, p2) => { return DateTime.Compare(p2.PublishTime, p1.PublishTime); });
            topNews.AddRange(endNewsList.Take(top - 1));
            if (orderNewsList != null)
            {
                foreach (KeyValuePair<int, NewsEntity> kv in orderNewsList)
                {
                    var existNewsByOrder = topNews.Find(p => p.NewsId == kv.Value.NewsId);
                    if (existNewsByOrder != null)
                        topNews.Remove(existNewsByOrder);

                    if (kv.Key > topNews.Count)
                        topNews.Add(kv.Value);
                    else
                        topNews.Insert(kv.Key - 1, kv.Value);
                }
            }
            return topNews.Take(top).ToList();
        }

        public static List<NewsEntity> GetFocusNewsListNewForWireless(Dictionary<int, NewsEntity> orderNewsList, int serialId, out bool existPingce, int top = 4)
        {
            existPingce = false;
            if (serialId < 1)
                return null;
            List<int> exitsNewsList = new List<int>();

            List<NewsEntity> endNewsList = new List<NewsEntity>();
            List<NewsEntity> newsList = new List<NewsEntity>();
            List<NewsEntity> videoNewsList = new List<NewsEntity>();
            ////插入车型详解新闻 第一条
            //var pingceNewsEntity = GetPingceNews(serialId);
            //if (pingceNewsEntity != null)
            //{
            //	existPingce = true;
            //	newsList.Add(pingceNewsEntity);
            //}

            DataSet ds = FocusNewsRespository.GetFocusNewsData(serialId);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dtNews = ds.Tables[0];
                DataRow[] arrNewsRows = dtNews.Select("subnewstype=1");
                if (arrNewsRows.Length > 0)
                {
                    //循环取新闻
                    for (int i = 1; i <= (top + 1); i++)
                    {
                        foreach (DataRow row in arrNewsRows)
                        {
                            int newsId = ConvertHelper.GetInteger(row["CmsNewsId"]);
                            if (newsId <= 0 || exitsNewsList.Contains(newsId))
                                continue;
                            int cateId = ConvertHelper.GetInteger(row["CategoryId"]);
                            if (!CommonData.CategoryPathDic.ContainsKey(cateId))
                                continue;
                            ////如果有车型详解文章 排除
                            //if (pingceNewsEntity != null && pingceNewsEntity.NewsId == newsId)
                            //	continue;

                            //如果当前顺序为1，而且当前分类又不包括在指定分类内
                            if (i == 1)
                            {
                                //如果没有原创标识或不是原创的不做为头条
                                if (string.IsNullOrEmpty(row["CreativeType"].ToString()))
                                    continue;
                                int newsType = ConvertHelper.GetInteger(row["CreativeType"]);
                                if (newsType != 0)
                                    continue;
                                //不在头条分类范围内的，不做为头条
                                if (IsNoContainsNews(CommonData.CategoryPathDic[cateId]))
                                    continue;
                            }
                            newsList.Add(GetNewsObjectByXmlNode(row));
                            exitsNewsList.Add(newsId);
                            break;
                        }
                    }
                }
            }
            var topNews = newsList;

            if (orderNewsList != null)
            {
                foreach (KeyValuePair<int, NewsEntity> kv in orderNewsList)
                {
                    var existNewsByOrder = topNews.Find(p => p.NewsId == kv.Value.NewsId);
                    if (existNewsByOrder != null)
                        topNews.Remove(existNewsByOrder);

                    if (kv.Key > topNews.Count)
                        topNews.Add(kv.Value);
                    else
                        topNews.Insert(kv.Key - 1, kv.Value);
                }
            }
            return topNews.Take(top).ToList();
        }

        public static List<NewsEntity> GetFocusNewsListNewForH5(Dictionary<int, NewsEntity> orderNewsList, int serialId, out bool existPingce, out bool existDaogou, int top = 10)
        {
            existPingce = false;
            existDaogou = false;
            if (serialId < 1)
                return null;
            List<int> exitsNewsList = new List<int>();

            List<NewsEntity> endNewsList = new List<NewsEntity>();
            List<NewsEntity> newsList = new List<NewsEntity>();
            List<NewsEntity> videoNewsList = new List<NewsEntity>();
            //插入车型详解新闻 第一条
            var pingceNewsEntity = GetPingceNews(serialId);
            if (pingceNewsEntity != null)
            {
                existPingce = true;
                newsList.Add(pingceNewsEntity);
            }
            var daogouEntity = GetDaoGouNews(serialId);
            if (daogouEntity != null)
            {
                existDaogou = true;
                newsList.Add(daogouEntity);
            }
            DataSet ds = FocusNewsRespository.GetFocusNewsData(serialId);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dtNews = ds.Tables[0];
                DataRow[] arrNewsRows = dtNews.Select("subnewstype=1");
                if (arrNewsRows.Length > 0)
                {
                    //循环取新闻
                    var loopCount = top + 1 - newsList.Count;
                    for (int i = 1; i < loopCount; i++)
                    {
                        foreach (DataRow row in arrNewsRows)
                        {
                            int newsId = ConvertHelper.GetInteger(row["CmsNewsId"]);
                            if (newsId <= 0 || exitsNewsList.Contains(newsId))
                                continue;
                            int cateId = ConvertHelper.GetInteger(row["CategoryId"]);
                            if (!CommonData.CategoryPathDic.ContainsKey(cateId))
                                continue;
                            //如果有车型详解文章 排除
                            if (pingceNewsEntity != null && pingceNewsEntity.NewsId == newsId)
                                continue;

                            //如果当前顺序为1，而且当前分类又不包括在指定分类内
                            if (i == 1)
                            {
                                //如果没有原创标识或不是原创的不做为头条
                                if (string.IsNullOrEmpty(row["CreativeType"].ToString()))
                                    continue;
                                int newsType = ConvertHelper.GetInteger(row["CreativeType"]);
                                if (newsType != 0)
                                    continue;
                                //不在头条分类范围内的，不做为头条
                                if (IsNoContainsNews(CommonData.CategoryPathDic[cateId]))
                                    continue;
                            }
                            newsList.Add(GetNewsObjectByXmlNode(row));
                            exitsNewsList.Add(newsId);
                            break;
                        }
                    }
                }
            }
            var topNews = newsList;

            if (orderNewsList != null)
            {
                foreach (KeyValuePair<int, NewsEntity> kv in orderNewsList)
                {
                    var existNewsByOrder = topNews.Find(p => p.NewsId == kv.Value.NewsId);
                    if (existNewsByOrder != null)
                        topNews.Remove(existNewsByOrder);

                    if (kv.Key > topNews.Count)
                        topNews.Add(kv.Value);
                    else
                        topNews.Insert(kv.Key - 1, kv.Value);
                }
            }
            return topNews.Take(top).ToList();
        }



        public static NewsEntity GetPingceNews(int serialId)
        {
            NewsEntity entity = null;
            string sql = @"SELECT TOP 1  [url] 
							FROM    [dbo].[CarPingceInfo]
							WHERE   csid = @SerialId";
            SqlParameter[] _params ={
			                          new SqlParameter("@SerialId",SqlDbType.Int)
			                      };
            _params[0].Value = serialId;
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, _params);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                int newsId = 0;
                var url = ConvertHelper.GetString(dr["url"]);
                string[] arrTempURL = url.Split('/');
                string pageName = arrTempURL[arrTempURL.Length - 1];
                if (pageName.Length >= 10)
                {
                    if (int.TryParse(pageName.Substring(3, 7), out newsId))
                    { }
                }
                if (newsId > 0)
                {
                    DataSet dsNews = CommonFunction.GetPingCeNewsDataSetV2(newsId);
                    if (dsNews != null && dsNews.Tables.Count > 0 && dsNews.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = dsNews.Tables[0].Rows[0];
                        entity = GetNewsObjectByXmlNodeForNewsUrl(row);
                        entity.NewsCategoryShowName = new NewsCategoryShowName();
                        entity.NewsCategoryShowName.CategoryShowName = "车型详解";
                    }
                }
            }
            return entity;
        }

        public static NewsEntity GetDaoGouNews(int serialId)
        {
            NewsEntity entity = null;

            string url = "";

            DataRow[] rows = CommonData.RainbowData.Tables[0].Select(" csId='" + serialId + "' and RainbowitemId='42' ");
            if (rows != null && rows.Length > 0)
            {
                url = rows[0]["url"].ToString().Trim().ToLower();
            }
            if (string.IsNullOrEmpty(url)) return entity;

            int newsId = 0;
            string[] arrTempURL = url.Split('/');
            string pageName = arrTempURL[arrTempURL.Length - 1];
            if (pageName.Length >= 10)
            {
                if (int.TryParse(pageName.Substring(3, 7), out newsId))
                { }
            }
            if (newsId > 0)
            {
                DataSet dsNews = CommonFunction.GetPingCeNewsDataSetV2(newsId);
                if (dsNews != null && dsNews.Tables.Count > 0 && dsNews.Tables[0].Rows.Count > 0)
                {
                    DataRow row = dsNews.Tables[0].Rows[0];
                    entity = GetNewsObjectByXmlNodeForNewsUrl(row);
                    entity.NewsCategoryShowName = new NewsCategoryShowName();
                    entity.NewsCategoryShowName.CategoryShowName = "购车手册";
                }
            }
            return entity;
        }

        /// <summary>
        /// 得到新闻对象
        /// </summary>
        /// <param name="xNode"></param>
        /// <returns></returns>
        public static NewsEntity GetNewsObjectByXmlNode(DataRow row)
        {
            var imageUrl = row.Table.Columns.Contains("FirstPicUrl") ? row["FirstPicUrl"].ToString() : "";
            if (imageUrl.Length > 7)
            {
                if (imageUrl.Contains("bitauto") || imageUrl.Contains("yiche"))
                {
                    imageUrl = imageUrl.Insert(imageUrl.IndexOf('/', 7) + 1, "newsimg-180-w0/");
                }
            }
            NewsEntity newsObject = new NewsEntity();
            newsObject.NewsId = ConvertHelper.GetInteger(row["cmsnewsid"]);
            newsObject.Title = row["title"].ToString();
            newsObject.FaceTitle = (row.Table != null && row.Table.Columns.Contains("facetitle")) ? row["facetitle"].ToString() : string.Empty;
            newsObject.PageUrl = row["filepath"].ToString();
            newsObject.PublishTime = ConvertHelper.GetDateTime(row["publishTime"]);
            newsObject.CategoryId = ConvertHelper.GetInteger(row["CategoryId"]);
            newsObject.NewsCategoryShowName = GetNewsCategory(CommonData.CategoryPathDic[newsObject.CategoryId]);
            newsObject.Author = row.Table.Columns.Contains("Author") ? row["Author"].ToString() : string.Empty;
            newsObject.CommentNum = row.Table.Columns.Contains("CommentNum") ? ConvertHelper.GetInteger(row["CommentNum"]) : 0;
            newsObject.ImageLink = imageUrl;
            return newsObject;
        }

        public static NewsEntity GetNewsObjectByXmlNodeForNewsUrl(DataRow row)
        {
            var imageUrl = row["firstPicUrl"].ToString();
            if (imageUrl.Length > 7)
            {
                if (imageUrl.Contains("bitauto") || imageUrl.Contains("yiche"))
                {
                    imageUrl = imageUrl.Insert(imageUrl.IndexOf('/', 7) + 1, "newsimg-180-w0/");
                }
            }
            NewsEntity newsObject = new NewsEntity();
            newsObject.NewsId = ConvertHelper.GetInteger(row["newsid"]);
            newsObject.Title = row["title"].ToString();
            newsObject.FaceTitle = (row.Table != null && row.Table.Columns.Contains("facetitle")) ? row["facetitle"].ToString() : string.Empty;
            newsObject.PageUrl = row["filepath"].ToString();
            newsObject.PublishTime = ConvertHelper.GetDateTime(row["publishTime"]);
            newsObject.CategoryId = ConvertHelper.GetInteger(row["newscategoryid"]);
            //newsObject.NewsCategoryShowName = GetNewsCategory(CommonData.CategoryPathDic[newsObject.CategoryId]);
            newsObject.Author = row.Table.Columns.Contains("author") ? row["author"].ToString() : string.Empty;
            //newsObject.CommentNum = ConvertHelper.GetInteger(row["CommentNum"]);
            newsObject.ImageLink = imageUrl;
            return newsObject;
        }

        public static NewsCategoryShowName GetNewsCategory(List<int> cateIds)
        {
            NewsCategoryConfig categoryConfig = CommonData.NewsCategoryConfig;
            foreach (int tempCateId in cateIds)
            {
                foreach (KeyValuePair<string, NewsCategoryShowName> kindCate in categoryConfig.NewsCategoryShowNames)
                {
                    if (kindCate.Key != NewsCategoryConfig.QitaCategoryKey
                        && kindCate.Value.CategoryIds.Contains(tempCateId))
                        return kindCate.Value;
                }
            }
            return categoryConfig.NewsCategoryShowNames.ContainsKey(NewsCategoryConfig.QitaCategoryKey)
                ? categoryConfig.NewsCategoryShowNames[NewsCategoryConfig.QitaCategoryKey] : null;
        }

        public static bool IsNoContainsNews(List<int> cateIds)
        {
            foreach (int tempCateId in cateIds)
            {
                if (CommonData.NewsCategoryConfig.SerialFocusTopCategoryIds.Contains(tempCateId))
                    return false;
            }
            return true;
        }
    }
}
