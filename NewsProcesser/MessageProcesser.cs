using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using System.Configuration;
using BitAuto.CarDataUpdate.Config;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.HtmlBuilder;
using System.Data;
using System.IO;
using BitAuto.Utils.Data;
using System.Text.RegularExpressions;
using System.Net;
namespace BitAuto.CarDataUpdate.NewsProcesser
{
	/// <summary>
	/// 新闻消息处理类
	/// 该对象将被缓存，定义变量时
	/// </summary>
	public class MessageProcesser : BaseProcesser
	{
		public MessageProcesser()
		{
			UpdateNewsNumber.NewsNumberThread();
		}

        public override void Processer(ContentMessage newsMessage)
        {
            if (newsMessage == null)
            {
                Log.WriteLog("error, ContentMessage is null from NewsProcesser!");
                return;
            }
            Log.WriteLog("start processer news [" + newsMessage.ContentId + "] !");
            int contentId = newsMessage.ContentId;
            //根据carnewsid获取categoryid与serialid,剔除掉categoryid不在carnewstypedef表中的
            List<int> cmsCtgyIdList=new List<int>();
            List<int> serialIdList = new List<int>();
            try
            {
                DataSet dsCarNewsTypeDef = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "SELECT DISTINCT CmsCategoryId FROM [CarNewsTypeDef] WHERE CarNewsTypeId=1 ORDER BY CmsCategoryId ASC");
                if (dsCarNewsTypeDef != null && dsCarNewsTypeDef.Tables.Count > 0 && dsCarNewsTypeDef.Tables[0].Rows.Count > 0)
                {
                    cmsCtgyIdList = dsCarNewsTypeDef.Tables[0].AsEnumerable().Select(row => ConvertHelper.GetInteger(row["CmsCategoryId"])).ToList();
                }
                SqlParameter curNewsId = new SqlParameter("@NewsId", SqlDbType.Int);
                curNewsId.Value = contentId;
                DataSet dsTables = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
                       "  SELECT * FROM Car_SerialNewsV2 Where NewsId=@NewsId ", curNewsId);
                if (dsTables.Tables.Count > 0)
                {
                    DataTable dt = dsTables.Tables[0];
                    foreach (DataRow dr in dt.Rows)
                    {
                        int categoryId = ConvertHelper.GetInteger(dr["CategoryId"]);
                        int serialId = ConvertHelper.GetInteger(dr["SerialId"]);
                        if (cmsCtgyIdList.Contains((categoryId)) && !serialIdList.Contains(serialId))
                        {
                            serialIdList.Add(serialId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("Excute sql exception: "+ex.Message);
                return;
            }

            FocusNewsHtmlBuilder fnhb = new FocusNewsHtmlBuilder(); ;
            int counter = 0;
            foreach (int entity in serialIdList)
            {
                counter++;
                Log.WriteLog("Get serialPage FocusNews:" + entity + " (" + counter + "/" + ")...");
                try
                {
                    fnhb.BuilderDataOrHtml(entity);
                }
                catch (Exception ex)
                {
                    Log.WriteErrorLog(ex.Message);
                }
            }
            Log.WriteLog("Processe End!");
        }

	    //public override void Processer(ContentMessage newsMessage)
        //{
        //    if (newsMessage == null)
        //    {
        //        Log.WriteLog("error, ContentMessage is null from NewsProcesser!");
        //        return;
        //    }

        //    Log.WriteLog("start processer news [" + newsMessage.ContentId + "] !");

        //    NewsRelatedData newsRelatedData = GetNewsRelatedCarNewsTypeAndSerialId(newsMessage.ContentId);   

        //    // 延期新闻在此删除 add by chengl Sep.3.2013
        //    if (IsDeleteNews(newsMessage) || newsMessage.UpdateTime > DateTime.Now)
        //    {
        //        //删除操作
        //        DeleteNewsDataByCmsNewsId(newsMessage.ContentId, false);
        //    }
        //    else
        //    {
        //        NewsContent news = GetNewsContent(newsMessage);
        //        if (news == null)
        //        {
        //            DeleteNewsDataByCmsNewsId(newsMessage.ContentId, false);
        //        }
        //        else
        //        {
        //            SetCommentNum(news);

        //            if (SaveNewsData(news))
        //            {
        //                if (newsRelatedData.CarNewsTypeIds == null)
        //                    newsRelatedData.CarNewsTypeIds = news.CarNewsTypes.Keys.ToList();
        //                else//合并去重
        //                    newsRelatedData.CarNewsTypeIds = newsRelatedData.CarNewsTypeIds.Union(news.CarNewsTypes.Keys.ToList()).Distinct().ToList();

        //                if (newsRelatedData.Serials == null)
        //                    newsRelatedData.Serials = news.NewsPages.Keys.ToList();
        //                else//合并去重
        //                    newsRelatedData.Serials = newsRelatedData.Serials.Union(news.NewsPages.Keys.ToList()).Distinct().ToList();

        //                if (newsRelatedData.CityIds == null)
        //                    newsRelatedData.CityIds = news.NewsCitys;
        //                else//合并去重
        //                    newsRelatedData.CityIds = newsRelatedData.CityIds.Union(news.NewsCitys).Distinct().ToList();

        //                if (newsRelatedData.ProvinceIds == null)
        //                    newsRelatedData.ProvinceIds = news.NewsProvinces;
        //                else//合并去重
        //                    newsRelatedData.ProvinceIds = newsRelatedData.ProvinceIds.Union(news.NewsProvinces).Distinct().ToList();
        //                //车型移动站也需要子品牌焦点新闻分类新闻 anh 20120912
        //                //if (news.CarNewsTypes.ContainsKey((int)CarNewsTypes.serialfocus))
        //                //{
        //                //    RemoveMoreSerialNews(newsRelatedData.Serials, (int)CarNewsTypes.serialfocus);
        //                //}
        //                if (news.CarNewsTypes.ContainsKey((int)CarNewsTypes.leveltopnews))
        //                {
        //                    RemoveMoreLevelNews(newsRelatedData.Serials, (int)CarNewsTypes.leveltopnews);
        //                }
        //            }
        //        }
        //    }

        //    if (newsRelatedData.CarNewsTypeIds != null && newsRelatedData.CarNewsTypeIds.Count > 0
        //        && newsRelatedData.Serials != null && newsRelatedData.Serials.Count > 0)
        //    {
        //        foreach (int carNewsType in newsRelatedData.CarNewsTypeIds)
        //        {
        //            BaseBuilder[] builders = HtmlBuilderFactory.GetHtmlBuilders(carNewsType);
        //            if (builders == null || builders.Length <= 0)
        //                continue;

        //            foreach (BaseBuilder builder in builders)
        //            {
        //                foreach (int serialId in newsRelatedData.Serials)
        //                {
        //                    try
        //                    {
        //                        builder.BuilderDataOrHtml(serialId);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Log.WriteErrorLog(ex);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //更新统计行情 
        //    //每次都要统计全国
        //    if (newsRelatedData.ProvinceIds != null && !newsRelatedData.ProvinceIds.Contains(0))
        //        newsRelatedData.ProvinceIds.Add(0);
        //    //更新行情数统计表
        //    UpdateHangqingNewsNumber(newsRelatedData);
        //    //更新行情数统计表
        //    UpdateZhiHuanNewsNumber(newsRelatedData);
        //    //更新统计行情 end

        //    //add by sk 2013.05.07 更新综述页视频块 xml 数据 （2017-04-25 13：48 注释掉）
        //    //if (newsRelatedData.CarNewsTypeIds != null && newsRelatedData.CarNewsTypeIds.Contains((int)CarNewsTypes.video))
        //    //{
        //    //	UpdateVideoNews(newsRelatedData.Serials);
        //    //}

        //    Log.WriteLog("end processer news [" + newsMessage.ContentId + "] !");
        //}
        /// <summary>
        /// 更新视频新闻 子品牌（2017-04-25 13：48 注释掉）
        /// </summary>
        /// <param name="serialList">子品牌ID list</param>
        //private void UpdateVideoNews(List<int> serialList)
        //{
        //	if (serialList == null) return;
        //	string newsPath = System.IO.Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\SerialVideo");
        //	try
        //	{
        //		foreach (int serialId in serialList)
        //		{
        //			string xmlUrl = CommonData.CommonSettings.NewsUrl + "?articaltype=3&getcount=4&brandid=" + serialId;
        //			string xmlFile = "Serial_Video_" + serialId + ".xml";
        //			xmlFile = System.IO.Path.Combine(newsPath, xmlFile);
        //			XmlDocument xmlDoc = new XmlDocument();
        //			xmlDoc.Load(xmlUrl);
        //			CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		Log.WriteErrorLog(ex);
        //	}
        //}
        /// <summary>
        /// 更新行情数统计表
        /// </summary>
        private void UpdateHangqingNewsNumber(NewsRelatedData newsRelatedData)
		{
			int hangqingTypeId = (int)CarNewsTypes.hangqing;
			if (newsRelatedData.CarNewsTypeIds == null || !newsRelatedData.CarNewsTypeIds.Contains(hangqingTypeId)) return;
			Log.WriteLog("start news hangqiunumber!");

			SqlParameter paramTypeId = new SqlParameter("@CarNewsTypeId", SqlDbType.Int);
			paramTypeId.Value = hangqingTypeId;
			SqlParameter paramCityId = new SqlParameter("@CityId", SqlDbType.Int);
			SqlParameter paramRowId = new SqlParameter("@RowId", SqlDbType.Int);
			#region //更新省行情数
			//更新省行情数
			if (newsRelatedData.ProvinceIds != null && newsRelatedData.ProvinceIds.Count > 0)
			{
				Log.WriteLog("start news hangqiunumber province!");
				foreach (int pId in newsRelatedData.ProvinceIds)
				{
					paramCityId.Value = pId;
					int rowId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "select Id from ProvinceCityNewsNumber WHERE CityId=@CityId", paramCityId));
					if (pId == 0)
					{
						//--全国
						//SELECT COUNT(1) FROM SerialNews WHERE CarNewsTypeId=12 AND IsFirst=1
						if (rowId > 0)
						{
							paramRowId.Value = rowId;
							SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
	"UPDATE ProvinceCityNewsNumber SET Num = (SELECT COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND IsFirst=1) WHERE Id=@RowId", paramTypeId, paramRowId);
						}
						else
						{
							SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
	"INSERT INTO ProvinceCityNewsNumber(CityId,Num) SELECT @CityId, COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND IsFirst=1", paramTypeId, paramCityId);
						}
					}
					else
					{
						//--省
						//SELECT COUNT(1) FROM ProvinceNews WHERE IsHQ=1 AND RelateProvinceId=2
						if (rowId > 0)
						{
							paramRowId.Value = rowId;
							SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
	"UPDATE ProvinceCityNewsNumber SET Num = (SELECT COUNT(1) FROM ProvinceNews WHERE IsHQ=1 AND RelateProvinceId=@CityId) WHERE Id=@RowId", paramCityId, paramRowId);
						}
						else
						{
							SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
	"INSERT INTO ProvinceCityNewsNumber(CityId,Num) SELECT @CityId, COUNT(1)  FROM ProvinceNews WHERE IsHQ=1 AND RelateProvinceId=@CityId", paramCityId);
						}
					}
				}
				Log.WriteLog("end news hangqiunumber province!");
			}
			#endregion

			#region //更新城市行情数
			//更新城市行情数
			if (newsRelatedData.CityIds != null && newsRelatedData.CityIds.Count > 0)
			{
				Log.WriteLog("start news hangqiunumber city!");
				foreach (int cityId in newsRelatedData.CityIds)
				{
					if (cityId == 0)
						continue;
					//--城市
					//SELECT COUNT(1) FROM CityNews WHERE IsHQ=1 AND RelateCityId=@CityId
					paramCityId.Value = cityId;
					int rowId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "select Id from ProvinceCityNewsNumber WHERE CityId=@CityId", paramCityId));
					if (rowId > 0)
					{
						paramRowId.Value = rowId;
						SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
"UPDATE ProvinceCityNewsNumber SET Num = (SELECT COUNT(1) FROM CityNews WHERE IsHQ=1 AND RelateCityId=@CityId) WHERE Id=@RowId", paramCityId, paramRowId);
					}
					else
					{
						SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
"INSERT INTO ProvinceCityNewsNumber(CityId,Num) SELECT @CityId, COUNT(1) FROM CityNews WHERE IsHQ=1 AND RelateCityId=@CityId", paramCityId);
					}
				}
				Log.WriteLog("end news hangqiunumber city!");
			}
			#endregion

			#region //更新子品牌省市行情数
			//更新子品牌省市行情数
			if (newsRelatedData.Serials != null && newsRelatedData.Serials.Count > 0)
			{
				SqlParameter paramSerialId = new SqlParameter("@SerialId", SqlDbType.Int);

				foreach (int csid in newsRelatedData.Serials)
				{
					paramSerialId.Value = csid;
					if (newsRelatedData.ProvinceIds != null && newsRelatedData.ProvinceIds.Count > 0)
					{
						foreach (int pId in newsRelatedData.ProvinceIds)
						{
							paramCityId.Value = pId;
							int rowId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "select Id from SerialCityNewsNumber WHERE SerialId=@SerialId AND CityId=@CityId", paramSerialId, paramCityId));
							if (pId == 0)
							{
								//--子品牌 全国
								//SELECT COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND sn.SerialId=@SerialId
								if (rowId > 0)
								{
									paramRowId.Value = rowId;
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			"UPDATE SerialCityNewsNumber SET Num = (SELECT COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND SerialId=@SerialId) WHERE Id=@RowId", paramTypeId, paramRowId, paramSerialId);
								}
								else
								{
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			"INSERT INTO SerialCityNewsNumber(SerialId,CityId,Num) SELECT @SerialId, @CityId, COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND SerialId=@SerialId", paramSerialId, paramTypeId, paramCityId);
								}
							}
							else
							{
								//--子品牌 省
								//SELECT COUNT(1) FROM ProvinceNews pn 
								//INNER JOIN SerialNews sn ON pn.CmsNewsId=sn.CmsNewsId
								//WHERE sn.SerialId=@SerialId AND pn.RelateProvinceId=@CityId and sn.CarNewsTypeId=@CarNewsTypeId AND pn.IsHq=1
								if (rowId > 0)
								{
									paramRowId.Value = rowId;
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			@"UPDATE SerialCityNewsNumber SET Num = (SELECT COUNT(1) FROM ProvinceNews pn 
INNER JOIN SerialNews sn ON pn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId AND pn.RelateProvinceId=@CityId and sn.CarNewsTypeId=@CarNewsTypeId AND pn.IsHq=1) WHERE Id=@RowId", paramSerialId, paramCityId, paramTypeId, paramRowId);
								}
								else
								{
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			@"INSERT INTO SerialCityNewsNumber(SerialId,CityId,Num) SELECT @SerialId, @CityId, COUNT(1) FROM ProvinceNews pn 
INNER JOIN SerialNews sn ON pn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId AND pn.RelateProvinceId=@CityId and sn.CarNewsTypeId=@CarNewsTypeId AND pn.IsHq=1", paramSerialId, paramCityId, paramTypeId);
								}
							}
						}
					}
					if (newsRelatedData.CityIds != null && newsRelatedData.CityIds.Count > 0)
					{
						foreach (int cityId in newsRelatedData.CityIds)
						{
							if (cityId == 0) continue;
							//--子品牌 城市
							//SELECT COUNT(1) FROM CityNews cn 
							//INNER JOIN SerialNews sn ON cn.CmsNewsId=sn.CmsNewsId
							//WHERE sn.SerialId=@SerialId and sn.CarNewsTypeId=@CarNewsTypeId AND cn.RelateCityId=@CityId AND cn.IsHq=1
							paramCityId.Value = cityId;
							int rowId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "select Id from SerialCityNewsNumber WHERE SerialId=@SerialId AND CityId=@CityId", paramSerialId, paramCityId));
							if (rowId > 0)
							{
								paramRowId.Value = rowId;
								SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
		@"UPDATE SerialCityNewsNumber SET Num = (SELECT COUNT(1) FROM CityNews cn 
INNER JOIN SerialNews sn ON cn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId and sn.CarNewsTypeId=@CarNewsTypeId AND cn.RelateCityId=@CityId AND cn.IsHq=1) WHERE Id=@RowId", paramSerialId, paramCityId, paramTypeId, paramRowId);
							}
							else
							{
								SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
		@"INSERT INTO SerialCityNewsNumber(SerialId,CityId,Num) SELECT @SerialId, @CityId, COUNT(1) FROM CityNews cn 
INNER JOIN SerialNews sn ON cn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId and sn.CarNewsTypeId=@CarNewsTypeId AND cn.RelateCityId=@CityId AND cn.IsHq=1", paramSerialId, paramCityId, paramTypeId);
							}
						}
					}
				}
			}
			#endregion

			Log.WriteLog("end news hangqiunumber!");
		}
		/// <summary>
		/// 更新行情数统计表
		/// </summary>
		private void UpdateZhiHuanNewsNumber(NewsRelatedData newsRelatedData)
		{
			int typeId = (int)CarNewsTypes.zhihuan;
			if (newsRelatedData.CarNewsTypeIds == null || !newsRelatedData.CarNewsTypeIds.Contains(typeId)) return;
			Log.WriteLog("start news zhihuanunumber!");

			SqlParameter paramTypeId = new SqlParameter("@CarNewsTypeId", SqlDbType.Int);
			paramTypeId.Value = typeId;
			SqlParameter paramCityId = new SqlParameter("@CityId", SqlDbType.Int);
			SqlParameter paramRowId = new SqlParameter("@RowId", SqlDbType.Int);
			#region //更新子品牌省市行情数
			//更新子品牌省市行情数
			if (newsRelatedData.Serials != null && newsRelatedData.Serials.Count > 0)
			{
				SqlParameter paramSerialId = new SqlParameter("@SerialId", SqlDbType.Int);

				foreach (int csid in newsRelatedData.Serials)
				{
					paramSerialId.Value = csid;
					if (newsRelatedData.ProvinceIds != null && newsRelatedData.ProvinceIds.Count > 0)
					{
						foreach (int pId in newsRelatedData.ProvinceIds)
						{
							paramCityId.Value = pId;
							int rowId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "select Id from SerialZhiHuanNewsNumber WHERE SerialId=@SerialId AND CityId=@CityId", paramSerialId, paramCityId));
							if (pId == 0)
							{
								//--子品牌 全国
								//SELECT COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND sn.SerialId=@SerialId
								if (rowId > 0)
								{
									paramRowId.Value = rowId;
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			"UPDATE SerialZhiHuanNewsNumber SET Num = (SELECT COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND SerialId=@SerialId) WHERE Id=@RowId", paramTypeId, paramRowId, paramSerialId);
								}
								else
								{
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			"INSERT INTO SerialZhiHuanNewsNumber(SerialId,CityId,Num) SELECT @SerialId, @CityId, COUNT(1) FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND SerialId=@SerialId", paramSerialId, paramTypeId, paramCityId);
								}
							}
							else
							{
								//--子品牌 省
								//SELECT COUNT(1) FROM ProvinceNews pn 
								//INNER JOIN SerialNews sn ON pn.CmsNewsId=sn.CmsNewsId
								//WHERE sn.SerialId=@SerialId AND pn.RelateProvinceId=@CityId and sn.CarNewsTypeId=@CarNewsTypeId AND pn.IsZH=1
								if (rowId > 0)
								{
									paramRowId.Value = rowId;
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			@"UPDATE SerialZhiHuanNewsNumber SET Num = (SELECT COUNT(1) FROM ProvinceNews pn 
INNER JOIN SerialNews sn ON pn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId AND pn.RelateProvinceId=@CityId and sn.CarNewsTypeId=@CarNewsTypeId AND pn.IsZH=1) WHERE Id=@RowId", paramSerialId, paramCityId, paramTypeId, paramRowId);
								}
								else
								{
									SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
			@"INSERT INTO SerialZhiHuanNewsNumber(SerialId,CityId,Num) SELECT @SerialId, @CityId, COUNT(1) FROM ProvinceNews pn 
INNER JOIN SerialNews sn ON pn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId AND pn.RelateProvinceId=@CityId and sn.CarNewsTypeId=@CarNewsTypeId AND pn.IsZH=1", paramSerialId, paramCityId, paramTypeId);
								}
							}
						}
					}
					if (newsRelatedData.CityIds != null && newsRelatedData.CityIds.Count > 0)
					{
						foreach (int cityId in newsRelatedData.CityIds)
						{
							if (cityId == 0) continue;
							//--子品牌 城市
							//SELECT COUNT(1) FROM CityNews cn 
							//INNER JOIN SerialNews sn ON cn.CmsNewsId=sn.CmsNewsId
							//WHERE sn.SerialId=@SerialId and sn.CarNewsTypeId=@CarNewsTypeId AND cn.RelateCityId=@CityId AND cn.IsZH=1
							paramCityId.Value = cityId;
							int rowId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "select Id from SerialZhiHuanNewsNumber WHERE SerialId=@SerialId AND CityId=@CityId", paramSerialId, paramCityId));
							if (rowId > 0)
							{
								paramRowId.Value = rowId;
								SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
		@"UPDATE SerialZhiHuanNewsNumber SET Num = (SELECT COUNT(1) FROM CityNews cn 
INNER JOIN SerialNews sn ON cn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId and sn.CarNewsTypeId=@CarNewsTypeId AND cn.RelateCityId=@CityId AND cn.IsZH=1) WHERE Id=@RowId", paramSerialId, paramCityId, paramTypeId, paramRowId);
							}
							else
							{
								SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
		@"INSERT INTO SerialZhiHuanNewsNumber(SerialId,CityId,Num) SELECT @SerialId, @CityId, COUNT(1) FROM CityNews cn 
INNER JOIN SerialNews sn ON cn.CmsNewsId=sn.CmsNewsId
WHERE sn.SerialId=@SerialId and sn.CarNewsTypeId=@CarNewsTypeId AND cn.RelateCityId=@CityId AND cn.IsZH=1", paramSerialId, paramCityId, paramTypeId);
							}
						}
					}
				}
			}
			#endregion

			Log.WriteLog("end news zhihuanunumber!");
		}
		/// <summary>
		/// 删除过多的级别表新闻
		/// </summary>
		private void RemoveMoreLevelNews(List<int> serials, int carNewsTypeId)
		{
			Log.WriteLog("start RemoveMoreLevelNews");
			if (serials == null || serials.Count <= 0 || carNewsTypeId <= 0)
			{
				Log.WriteLog("ages error");
				return;
			}
			SqlParameter[] param = new SqlParameter[]{
                new SqlParameter("@LevelId",int.MinValue),
                new SqlParameter("@CarNewsTypeId",carNewsTypeId)
            };
			foreach (int serialId in serials)
			{
				if (CommonData.SerialLevelIdDic.ContainsKey(serialId))
				{
					param[0].Value = CommonData.SerialLevelIdDic[serialId];
					SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.StoredProcedure, "dbo.Proc_RemoveMoreLevelNews", param);
				}
			}
			Log.WriteLog("end RemoveMoreLevelNews");
		}
		/// <summary>
		/// 删除过多的子品牌表新闻
		/// </summary>
		private void RemoveMoreSerialNews(List<int> serials, int carNewsTypeId)
		{
			Log.WriteLog("start RemoveMoreSerialNews!");
			if (serials == null || serials.Count <= 0 || carNewsTypeId <= 0)
			{
				Log.WriteLog("ages error");
				return;
			}
			SqlParameter[] param = new SqlParameter[]{
                new SqlParameter("@SerialId",int.MinValue),
                new SqlParameter("@CarNewsTypeId",carNewsTypeId)
            };
			foreach (int serialId in serials)
			{
				param[0].Value = serialId;
				SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.StoredProcedure, "dbo.Proc_RemoveMoreSerialNews", param);
			}
			Log.WriteLog("end RemoveMoreSerialNews!");
		}

		/// <summary>
		/// 获取评论数
		/// </summary>
		private void SetCommentNum(NewsContent news)
		{
			Log.WriteLog("start SetCommentNum!");
			if (news == null)
			{
				Log.WriteLog("NewsContent is null!");
				return;
			}

			news.CommentNum = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "select [Num] from [NewsCommentNum] where [CmsNewsId]=@CmsNewsId", new SqlParameter("@CmsNewsId", news.CmsNewsId)));

			Log.WriteLog("end SetCommentNum!");
		}
		/// <summary>
		/// 保存新闻数据
		/// </summary>
		private bool SaveNewsData(NewsContent news)
		{
			if (news == null)
				return false;
			Log.WriteLog("start save news [" + news.CmsNewsId + "] !");
			if (news.NewsPages == null || news.NewsPages.Count <= 0)
			{
				Log.WriteLog("error pages is null! newsid:" + news.CmsNewsId);
				return false;
			}

			Dictionary<int, CarNewsTypeItem> carNewsTypeDic = CommonData.CarNewsTypeSettings.CarNewsTypeList;
			SqlConnection conn = null;
			try
			{
				//删除已有新闻
				#region 删除已有新闻
				if (!DeleteNewsDataByCmsNewsId(news.CmsNewsId, true))
					return false;
				#endregion

				conn = new SqlConnection(CommonData.ConnectionStringSettings.CarDataUpdateConnString);

				#region 插入news的Command
				SqlCommand insertNewsCom = new SqlCommand("INSERT INTO [News]([CmsNewsId],[PageNum],[Author],[Title],[FaceTitle],[Summary],[Picture],[Content],[FirstPicUrl],[FilePath],[PublishTime],[SourceName],[SourceUrl],[EditorId],[EditorName],[PageCount],[CreativeType],[CarId],[IsSate],[YearType],[CommentNum],[Duration], [IsFirst], [BeginDate], [EndDate]) VALUES(@CmsNewsId,@PageNum,@Author,@Title,@FaceTitle,@Summary,@Picture,@Content,@FirstPicUrl,@FilePath,@PublishTime,@SourceName,@SourceUrl,@EditorId,@EditorName,@PageCount,@CreativeType,@CarId,@IsSate,@YearType,@CommentNum,@Duration, @IsFirst, @BeginDate, @EndDate) SELECT SCOPE_IDENTITY()", conn);
				insertNewsCom.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@PageNum", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@Author", System.Data.SqlDbType.NVarChar);
				insertNewsCom.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar);
				insertNewsCom.Parameters.Add("@FaceTitle", System.Data.SqlDbType.NVarChar);
				insertNewsCom.Parameters.Add("@Summary", System.Data.SqlDbType.NVarChar);
				insertNewsCom.Parameters.Add("@Picture", System.Data.SqlDbType.VarChar);
				insertNewsCom.Parameters.Add("@Content", System.Data.SqlDbType.NVarChar);
				insertNewsCom.Parameters.Add("@FirstPicUrl", System.Data.SqlDbType.VarChar);
				insertNewsCom.Parameters.Add("@FilePath", System.Data.SqlDbType.VarChar);
				insertNewsCom.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime);
				insertNewsCom.Parameters.Add("@SourceName", System.Data.SqlDbType.NVarChar);
				insertNewsCom.Parameters.Add("@SourceUrl", System.Data.SqlDbType.VarChar);
				insertNewsCom.Parameters.Add("@EditorId", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@EditorName", System.Data.SqlDbType.NVarChar);
				insertNewsCom.Parameters.Add("@PageCount", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@CreativeType", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@CarId", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@IsSate", System.Data.SqlDbType.Bit);
				insertNewsCom.Parameters.Add("@YearType", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@CommentNum", System.Data.SqlDbType.Int);
				insertNewsCom.Parameters.Add("@Duration", System.Data.SqlDbType.VarChar);
				insertNewsCom.Parameters.Add("@IsFirst", System.Data.SqlDbType.Bit);
				insertNewsCom.Parameters.Add("@BeginDate", System.Data.SqlDbType.Date);
				insertNewsCom.Parameters.Add("@EndDate", System.Data.SqlDbType.Date);
				#endregion

				#region 插入Serial的command
				SqlCommand insertNewsSerial = new SqlCommand("INSERT INTO [SerialNews]([CarNewsId],[CarNewsTypeId],[CmsNewsId],[CategoryId],[Title],[FilePath],[SerialId],[YearType],[PublishTime],[CarId],[CreativeType], [IsFirst]) VALUES(@CarNewsId,@CarNewsTypeId,@CmsNewsId,@CategoryId,@Title,@FilePath,@SerialId,@YearType,@PublishTime,@CarId,@CreativeType, @IsFirst)", conn);
				insertNewsSerial.Parameters.Add("@CarNewsId", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@SerialId", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@CarId", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@CarNewsTypeId", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@YearType", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@CategoryId", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar);
				insertNewsSerial.Parameters.Add("@FilePath", System.Data.SqlDbType.VarChar);
				insertNewsSerial.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime);
				insertNewsSerial.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@CreativeType", System.Data.SqlDbType.Int);
				insertNewsSerial.Parameters.Add("@IsFirst", System.Data.SqlDbType.Bit);
				#endregion

				#region 插入Brand的command
				SqlCommand insertNewsBrand = new SqlCommand("INSERT INTO [BrandNews]([CarNewsId],[BrandId],[CarNewsTypeId],[CmsNewsId],[CategoryId],[Title],[FilePath],[PublishTime],[CreativeType], [SerialId], [IsFirst]) VALUES(@CarNewsId,@BrandId,@CarNewsTypeId,@CmsNewsId,@CategoryId,@Title,@FilePath,@PublishTime,@CreativeType, @SerialId, @IsFirst)", conn);
				insertNewsBrand.Parameters.Add("@CarNewsId", System.Data.SqlDbType.Int);
				insertNewsBrand.Parameters.Add("@BrandId", System.Data.SqlDbType.Int);
				insertNewsBrand.Parameters.Add("@CarNewsTypeId", System.Data.SqlDbType.Int);
				insertNewsBrand.Parameters.Add("@CategoryId", System.Data.SqlDbType.Int);
				insertNewsBrand.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar);
				insertNewsBrand.Parameters.Add("@FilePath", System.Data.SqlDbType.VarChar);
				insertNewsBrand.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime);
				insertNewsBrand.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int);
				insertNewsBrand.Parameters.Add("@CreativeType", System.Data.SqlDbType.Int);
				insertNewsBrand.Parameters.Add("@SerialId", System.Data.SqlDbType.Int);
				insertNewsBrand.Parameters.Add("@IsFirst", System.Data.SqlDbType.Bit);
				#endregion

				#region 插入MasterBrand的command
				SqlCommand insertNewsMasterBrand = new SqlCommand("INSERT INTO [MasterBrandNews]([CarNewsId],[MasterBrandId],[CarNewsTypeId],[CmsNewsId],[CategoryId],[Title],[FilePath],[PublishTime],[CreativeType], [SerialId], [IsFirst]) VALUES(@CarNewsId,@MasterBrandId,@CarNewsTypeId,@CmsNewsId,@CategoryId,@Title,@FilePath,@PublishTime,@CreativeType, @SerialId, @IsFirst)", conn);
				insertNewsMasterBrand.Parameters.Add("@CarNewsId", System.Data.SqlDbType.Int);
				insertNewsMasterBrand.Parameters.Add("@MasterBrandId", System.Data.SqlDbType.Int);
				insertNewsMasterBrand.Parameters.Add("@CarNewsTypeId", System.Data.SqlDbType.Int);
				insertNewsMasterBrand.Parameters.Add("@CategoryId", System.Data.SqlDbType.Int);
				insertNewsMasterBrand.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar);
				insertNewsMasterBrand.Parameters.Add("@FilePath", System.Data.SqlDbType.VarChar);
				insertNewsMasterBrand.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime);
				insertNewsMasterBrand.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int);
				insertNewsMasterBrand.Parameters.Add("@CreativeType", System.Data.SqlDbType.Int);
				insertNewsMasterBrand.Parameters.Add("@SerialId", System.Data.SqlDbType.Int);
				insertNewsMasterBrand.Parameters.Add("@IsFirst", System.Data.SqlDbType.Bit);
				#endregion

				#region 插入ProducerNews的command
				SqlCommand insertNewsProducer = new SqlCommand("INSERT INTO [ProducerNews]([CarNewsId],[ProducerId],[CarNewsTypeId],[CmsNewsId],[CategoryId],[Title],[FilePath],[PublishTime],[CreativeType], [SerialId], [IsFirst]) VALUES(@CarNewsId,@ProducerId,@CarNewsTypeId,@CmsNewsId,@CategoryId,@Title,@FilePath,@PublishTime,@CreativeType, @SerialId, @IsFirst)", conn);
				insertNewsProducer.Parameters.Add("@CarNewsId", System.Data.SqlDbType.Int);
				insertNewsProducer.Parameters.Add("@ProducerId", System.Data.SqlDbType.Int);
				insertNewsProducer.Parameters.Add("@CarNewsTypeId", System.Data.SqlDbType.Int);
				insertNewsProducer.Parameters.Add("@CategoryId", System.Data.SqlDbType.Int);
				insertNewsProducer.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar);
				insertNewsProducer.Parameters.Add("@FilePath", System.Data.SqlDbType.VarChar);
				insertNewsProducer.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime);
				insertNewsProducer.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int);
				insertNewsProducer.Parameters.Add("@CreativeType", System.Data.SqlDbType.Int);
				insertNewsProducer.Parameters.Add("@SerialId", System.Data.SqlDbType.Int);
				insertNewsProducer.Parameters.Add("@IsFirst", System.Data.SqlDbType.Bit);
				#endregion

				#region 插入Level的command
				SqlCommand insertNewsLevel = new SqlCommand("INSERT INTO [LevelNews]([CarNewsId],[LevelId],[CarNewsTypeId],[CmsNewsId],[CategoryId],[Title],[FilePath],[PublishTime],[CreativeType], [SerialId], [IsFirst]) VALUES(@CarNewsId,@LevelId,@CarNewsTypeId,@CmsNewsId,@CategoryId,@Title,@FilePath,@PublishTime,@CreativeType, @SerialId, @IsFirst)", conn);
				insertNewsLevel.Parameters.Add("@CarNewsId", System.Data.SqlDbType.Int);
				insertNewsLevel.Parameters.Add("@LevelId", System.Data.SqlDbType.Int);
				insertNewsLevel.Parameters.Add("@CarNewsTypeId", System.Data.SqlDbType.Int);
				insertNewsLevel.Parameters.Add("@CategoryId", System.Data.SqlDbType.Int);
				insertNewsLevel.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar);
				insertNewsLevel.Parameters.Add("@FilePath", System.Data.SqlDbType.VarChar);
				insertNewsLevel.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime);
				insertNewsLevel.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int);
				insertNewsLevel.Parameters.Add("@CreativeType", System.Data.SqlDbType.Int);
				insertNewsLevel.Parameters.Add("@SerialId", System.Data.SqlDbType.Int);
				insertNewsLevel.Parameters.Add("@IsFirst", System.Data.SqlDbType.Bit);
				#endregion

				#region news赋值
				insertNewsCom.Parameters["@CmsNewsId"].Value = news.CmsNewsId;
				insertNewsCom.Parameters["@Author"].Value = news.Author;
				insertNewsCom.Parameters["@FaceTitle"].Value = news.FaceTitle;
				insertNewsCom.Parameters["@Summary"].Value = news.Summary;
				insertNewsCom.Parameters["@Picture"].Value = news.Picture;
				insertNewsCom.Parameters["@PublishTime"].Value = news.PublishTime;

				insertNewsCom.Parameters["@SourceName"].Value = news.SourceName;
				insertNewsCom.Parameters["@SourceUrl"].Value = news.SourceUrl;
				insertNewsCom.Parameters["@EditorId"].Value = news.EditorId;
				insertNewsCom.Parameters["@EditorName"].Value = news.EditorName;
				insertNewsCom.Parameters["@PageCount"].Value = news.PageCount;
				insertNewsCom.Parameters["@CreativeType"].Value = news.CreativeType;
				insertNewsCom.Parameters["@CarId"].Value = news.CarId;
				insertNewsCom.Parameters["@YearType"].Value = news.YearType;
				insertNewsCom.Parameters["@CommentNum"].Value = news.CommentNum;
				insertNewsCom.Parameters["@Duration"].Value = news.Duration;

				insertNewsCom.Parameters["@BeginDate"].Value = news.BeginDate.HasValue ? news.BeginDate.Value : (object)DBNull.Value;
				insertNewsCom.Parameters["@EndDate"].Value = news.EndDate.HasValue ? news.EndDate.Value : (object)DBNull.Value;

				insertNewsCom.Parameters["@IsSate"].Value = 1;
				#endregion

				#region serialNews赋值
				insertNewsSerial.Parameters["@CmsNewsId"].Value = news.CmsNewsId;
				insertNewsSerial.Parameters["@PublishTime"].Value = news.PublishTime;
				insertNewsSerial.Parameters["@CreativeType"].Value = news.CreativeType;
				#endregion

				#region BrandNews赋值
				insertNewsBrand.Parameters["@CmsNewsId"].Value = news.CmsNewsId;
				insertNewsBrand.Parameters["@PublishTime"].Value = news.PublishTime;
				insertNewsBrand.Parameters["@CreativeType"].Value = news.CreativeType;
				#endregion

				#region MasterBrandNews赋值
				insertNewsMasterBrand.Parameters["@CmsNewsId"].Value = news.CmsNewsId;
				insertNewsMasterBrand.Parameters["@PublishTime"].Value = news.PublishTime;
				insertNewsMasterBrand.Parameters["@CreativeType"].Value = news.CreativeType;
				#endregion

				#region ProducerNews赋值
				insertNewsProducer.Parameters["@CmsNewsId"].Value = news.CmsNewsId;
				insertNewsProducer.Parameters["@PublishTime"].Value = news.PublishTime;
				insertNewsProducer.Parameters["@CreativeType"].Value = news.CreativeType;
				#endregion

				#region LevelNews赋值
				insertNewsLevel.Parameters["@CmsNewsId"].Value = news.CmsNewsId;
				insertNewsLevel.Parameters["@PublishTime"].Value = news.PublishTime;
				insertNewsLevel.Parameters["@CreativeType"].Value = news.CreativeType;
				#endregion

				conn.Open();
				NewsPageInfo curPageInfo = null;
				Dictionary<int, List<int>> masterBrandUpList = new Dictionary<int, List<int>>(news.NewsPages.Count),
					brandUpList = new Dictionary<int, List<int>>(news.NewsPages.Count),
					levelUpList = new Dictionary<int, List<int>>(news.NewsPages.Count),
					producerUpList = new Dictionary<int, List<int>>(news.NewsPages.Count);
				foreach (KeyValuePair<int, NewsPageInfo> pageInfo in news.NewsPages)
				{
					curPageInfo = pageInfo.Value;
					#region 插入news数据
					insertNewsCom.Parameters["@PageNum"].Value = curPageInfo.PageIndex;

					insertNewsCom.Parameters["@Title"].Value = curPageInfo.PageTitle;

					insertNewsCom.Parameters["@FirstPicUrl"].Value = curPageInfo.FirstPicUrl;

					insertNewsCom.Parameters["@FilePath"].Value = curPageInfo.PageLink;

					insertNewsCom.Parameters["@Content"].Value = curPageInfo.PageContent;

					insertNewsCom.Parameters["@IsFirst"].Value = curPageInfo.IsFirst;

					curPageInfo.Id = ConvertHelper.GetInteger(insertNewsCom.ExecuteScalar());
					Log.WriteLog(string.Format("save news [{0}] successed. carnewsid [{1}]!", news.CmsNewsId.ToString(), curPageInfo.Id.ToString()));
					#endregion

					#region serialNews赋值
					insertNewsSerial.Parameters["@Title"].Value = curPageInfo.PageTitle;
					insertNewsSerial.Parameters["@FilePath"].Value = curPageInfo.PageLink;
					insertNewsSerial.Parameters["@CarNewsId"].Value = curPageInfo.Id;
					insertNewsSerial.Parameters["@SerialId"].Value = curPageInfo.SerialId;
					insertNewsSerial.Parameters["@YearType"].Value = curPageInfo.SerialYear;
					insertNewsSerial.Parameters["@CarId"].Value = curPageInfo.CarId;
					insertNewsSerial.Parameters["@IsFirst"].Value = curPageInfo.IsFirst;
					#endregion

					#region brandNews赋值
					int brandId = 0;
					if (CommonData.SerialBrandDic.ContainsKey(curPageInfo.SerialId))
					{
						brandId = CommonData.SerialBrandDic[curPageInfo.SerialId];
						insertNewsBrand.Parameters["@CarNewsId"].Value = curPageInfo.Id;
						insertNewsBrand.Parameters["@BrandId"].Value = brandId;
						insertNewsBrand.Parameters["@Title"].Value = curPageInfo.PageTitle;
						insertNewsBrand.Parameters["@FilePath"].Value = curPageInfo.PageLink;
						insertNewsBrand.Parameters["@SerialId"].Value = curPageInfo.SerialId;
						insertNewsBrand.Parameters["@IsFirst"].Value = curPageInfo.IsFirst;
					}
					#endregion

					#region masterbrandNews赋值
					int masterBrandId = 0;
					if (CommonData.SerialMasterBrandDic.ContainsKey(curPageInfo.SerialId))
					{
						masterBrandId = CommonData.SerialMasterBrandDic[curPageInfo.SerialId];
						insertNewsMasterBrand.Parameters["@CarNewsId"].Value = curPageInfo.Id;
						insertNewsMasterBrand.Parameters["@MasterBrandId"].Value = masterBrandId;
						insertNewsMasterBrand.Parameters["@Title"].Value = curPageInfo.PageTitle;
						insertNewsMasterBrand.Parameters["@FilePath"].Value = curPageInfo.PageLink;
						insertNewsMasterBrand.Parameters["@SerialId"].Value = curPageInfo.SerialId;
						insertNewsMasterBrand.Parameters["@IsFirst"].Value = curPageInfo.IsFirst;
					}
					#endregion

					#region ProducerNews赋值
					int cpId = 0;
					if (CommonData.SerialProducerDic.ContainsKey(curPageInfo.SerialId))
					{
						cpId = CommonData.SerialProducerDic[curPageInfo.SerialId];
						insertNewsProducer.Parameters["@CarNewsId"].Value = curPageInfo.Id;
						insertNewsProducer.Parameters["@ProducerId"].Value = cpId;
						insertNewsProducer.Parameters["@Title"].Value = curPageInfo.PageTitle;
						insertNewsProducer.Parameters["@FilePath"].Value = curPageInfo.PageLink;
						insertNewsProducer.Parameters["@SerialId"].Value = curPageInfo.SerialId;
						insertNewsProducer.Parameters["@IsFirst"].Value = curPageInfo.IsFirst;
					}
					#endregion

					#region LevelNews赋值
					int levelId = 0;
					if (CommonData.SerialLevelIdDic.ContainsKey(curPageInfo.SerialId))
					{
						levelId = CommonData.SerialLevelIdDic[curPageInfo.SerialId];
						insertNewsLevel.Parameters["@CarNewsId"].Value = curPageInfo.Id;
						insertNewsLevel.Parameters["@LevelId"].Value = levelId;
						insertNewsLevel.Parameters["@Title"].Value = curPageInfo.PageTitle;
						insertNewsLevel.Parameters["@FilePath"].Value = curPageInfo.PageLink;
						insertNewsLevel.Parameters["@SerialId"].Value = curPageInfo.SerialId;
						insertNewsLevel.Parameters["@IsFirst"].Value = curPageInfo.IsFirst;
					}
					#endregion

					foreach (KeyValuePair<int, int> carNewsType in news.CarNewsTypes)
					{
						int carNewsTypeId = carNewsType.Key;
						int categoryId = carNewsType.Value;
						CarNewsTypeItem typeItem = carNewsTypeDic[carNewsTypeId];
						if ((typeItem.CarTypes & CarTypes.Serial) == CarTypes.Serial)
						{
							Log.WriteLog(string.Format("start save carnewstype [{0}] serial [{1}]!", carNewsTypeId.ToString(), curPageInfo.SerialId.ToString()));
							insertNewsSerial.Parameters["@CarNewsTypeId"].Value = carNewsTypeId;
							insertNewsSerial.Parameters["@CategoryId"].Value = categoryId;
							insertNewsSerial.ExecuteNonQuery();
							Log.WriteLog(string.Format("end save carnewstype [{0}] serial [{1}] successed!", carNewsTypeId.ToString(), curPageInfo.SerialId.ToString()));
						}
						if ((typeItem.CarTypes & CarTypes.Brand) == CarTypes.Brand && brandId > 0)
						{
							if (IsNotInsert(brandUpList, carNewsTypeId, brandId))
							{
								Log.WriteLog(string.Format("start save carnewstype [{0}] brand [{1}]!", carNewsTypeId.ToString(), brandId.ToString()));
								insertNewsBrand.Parameters["@CarNewsTypeId"].Value = carNewsTypeId;
								insertNewsBrand.Parameters["@CategoryId"].Value = categoryId;
								insertNewsBrand.ExecuteNonQuery();
								Log.WriteLog(string.Format("end save carnewstype [{0}] brand [{1}] successed!", carNewsTypeId.ToString(), brandId.ToString()));
							}
						}
						if ((typeItem.CarTypes & CarTypes.MasterBrand) == CarTypes.MasterBrand && masterBrandId > 0)
						{
							if (IsNotInsert(masterBrandUpList, carNewsTypeId, masterBrandId))
							{
								Log.WriteLog(string.Format("start save carnewstype [{0}] masterbrand [{1}]!", carNewsTypeId.ToString(), masterBrandId.ToString()));
								insertNewsMasterBrand.Parameters["@CarNewsTypeId"].Value = carNewsTypeId;
								insertNewsMasterBrand.Parameters["@CategoryId"].Value = categoryId;
								insertNewsMasterBrand.ExecuteNonQuery();
								Log.WriteLog(string.Format("end save carnewstype [{0}] masterbrand [{1}] successed!", carNewsTypeId.ToString(), masterBrandId.ToString()));
							}
						}
						if ((typeItem.CarTypes & CarTypes.Producer) == CarTypes.Producer && cpId > 0)
						{
							if (IsNotInsert(producerUpList, carNewsTypeId, cpId))
							{
								Log.WriteLog(string.Format("start save carnewstype [{0}] producer [{1}]!", carNewsTypeId.ToString(), cpId.ToString()));
								insertNewsProducer.Parameters["@CarNewsTypeId"].Value = carNewsTypeId;
								insertNewsProducer.Parameters["@CategoryId"].Value = categoryId;
								insertNewsProducer.ExecuteNonQuery();
								Log.WriteLog(string.Format("end save carnewstype [{0}] producer [{1}] successed!", carNewsTypeId.ToString(), cpId.ToString()));
							}
						}
						if ((typeItem.CarTypes & CarTypes.Level) == CarTypes.Level && levelId > 0)
						{
							if (IsNotInsert(levelUpList, carNewsTypeId, levelId))
							{
								Log.WriteLog(string.Format("start save carnewstype [{0}] level [{1}]!", carNewsTypeId.ToString(), levelId.ToString()));
								insertNewsLevel.Parameters["@CarNewsTypeId"].Value = carNewsTypeId;
								insertNewsLevel.Parameters["@CategoryId"].Value = categoryId;
								insertNewsLevel.ExecuteNonQuery();
								Log.WriteLog(string.Format("end save carnewstype [{0}] level [{1}] successed!", carNewsTypeId.ToString(), levelId.ToString()));
							}
						}
					}
				}

				#region 关联省和市
				bool isHQ = false, isZH = false;
				if (news.CarNewsTypes != null && news.CarNewsTypes.Count > 0)
				{
					isHQ = news.CarNewsTypes.ContainsKey((int)CarNewsTypes.hangqing);
					isZH = news.CarNewsTypes.ContainsKey((int)CarNewsTypes.zhihuan);
				}
				//省
				if (news.NewsProvinces != null && news.NewsProvinces.Count > 0)
				{
					SqlCommand insertNewsProvince = new SqlCommand("INSERT INTO [ProvinceNews]([CmsNewsId],[RelateProvinceId],[IsHQ],[IsZH],[PublishTime]) VALUES(@CmsNewsId, @RelateProvinceId,@IsHQ,@IsZH,@PublishTime)", conn);
					insertNewsProvince.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int).Value = news.CmsNewsId;
					insertNewsProvince.Parameters.Add("@IsHQ", System.Data.SqlDbType.Bit).Value = isHQ;
					insertNewsProvince.Parameters.Add("@IsZH", System.Data.SqlDbType.Bit).Value = isZH;
					insertNewsProvince.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime).Value = news.PublishTime;
					insertNewsProvince.Parameters.Add("@RelateProvinceId", System.Data.SqlDbType.Int);
					foreach (int provinceId in news.NewsProvinces)
					{
						insertNewsProvince.Parameters["@RelateProvinceId"].Value = provinceId;
						insertNewsProvince.ExecuteNonQuery();
						Log.WriteLog(string.Format("save ProvinceNews [{0}] successed!", provinceId.ToString()));
					}
				}
				//市
				if (news.NewsCitys != null && news.NewsCitys.Count > 0)
				{
					SqlCommand insertNewsCity = new SqlCommand("INSERT INTO [CityNews]([CmsNewsId],[RelateCityId],[RelateProvinceId],[IsHQ],[IsZH],[PublishTime]) VALUES(@CmsNewsId,@RelateCityId, @RelateProvinceId,@IsHQ,@IsZH,@PublishTime)", conn);
					insertNewsCity.Parameters.Add("@CmsNewsId", System.Data.SqlDbType.Int).Value = news.CmsNewsId;

					insertNewsCity.Parameters.Add("@IsHQ", System.Data.SqlDbType.Bit).Value = isHQ;
					insertNewsCity.Parameters.Add("@IsZH", System.Data.SqlDbType.Bit).Value = isZH;
					insertNewsCity.Parameters.Add("@PublishTime", System.Data.SqlDbType.DateTime).Value = news.PublishTime;

					insertNewsCity.Parameters.Add("@RelateCityId", System.Data.SqlDbType.Int);
					insertNewsCity.Parameters.Add("@RelateProvinceId", System.Data.SqlDbType.Int);

					foreach (int cityId in news.NewsCitys)
					{
						insertNewsCity.Parameters["@RelateCityId"].Value = cityId;
						insertNewsCity.Parameters["@RelateProvinceId"].Value = CommonData.CityAndProvinceDic.ContainsKey(cityId) ? CommonData.CityAndProvinceDic[cityId] : cityId;
						insertNewsCity.ExecuteNonQuery();
						Log.WriteLog(string.Format("save citynews [{0}] successed!", cityId.ToString()));
					}
				}
				#endregion

				Log.WriteLog(string.Format("end save news [{0}] successed!", news.CmsNewsId.ToString()));
				return true;
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(exp);
				return false;
			}
			finally
			{
				if (conn != null && conn.State != System.Data.ConnectionState.Closed)
					conn.Close();
			}
		}

		private bool IsNotInsert(Dictionary<int, List<int>> upList, int carTypeId, int objId)
		{
			if (upList == null)
				upList = new Dictionary<int, List<int>>();
			List<int> list = null;
			if (!upList.ContainsKey(carTypeId))
			{
				list = upList[carTypeId] = new List<int>();
			}
			else
			{
				list = upList[carTypeId];
			}
			if (list.Contains(objId))
				return false;
			else
			{
				list.Add(objId);
				return true;
			}
		}
		/// <summary>
		/// 根据新闻id，删除新闻相关数据
		/// </summary>
		private bool DeleteNewsDataByCmsNewsId(int cmsNewsId, bool isDelete)
		{
			Log.WriteLog(string.Format("start delete news [{0}]. isdelete [{1}]!", cmsNewsId.ToString(), isDelete.ToString()));
			SqlConnection conn = null;
			try
			{
				conn = new SqlConnection(CommonData.ConnectionStringSettings.CarDataUpdateConnString);
				conn.Open();
				SqlCommand deleteCom = new SqlCommand("Proc_DeleteNewsByCmsNewsId", conn);
				deleteCom.CommandType = System.Data.CommandType.StoredProcedure;
				deleteCom.Parameters.Add(new SqlParameter("@CmsNewsId", cmsNewsId));
				deleteCom.Parameters.Add(new SqlParameter("@IsDelete", isDelete));
				deleteCom.ExecuteNonQuery();
				Log.WriteLog(string.Format("delete news [{0}] successed. isdelete [{1}]!", cmsNewsId.ToString(), isDelete.ToString()));
				return true;
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(exp);
				return false;
			}
			finally
			{
				if (conn != null && conn.State != System.Data.ConnectionState.Closed)
					conn.Close();
			}
		}
		/// <summary>
		/// 获取新闻实体
		/// </summary>
		private NewsContent GetNewsContent(ContentMessage newsMessage)
		{
			int newsId = newsMessage.ContentId;
			XmlDocument newsDoc = CommonFunction.GetXmlDocument(string.Format(Common.CommonData.CommonSettings.NewsDetailUrl, newsId.ToString()));
			if (newsDoc == null)
			{
				Log.WriteLog("error get news content [" + newsId + "] failed!");
				return null;
			}
			XmlElement newsNode = newsDoc.SelectSingleNode("/NewDataSet/Table") as XmlElement;
			if (newsNode == null)
			{
				Log.WriteLog("error news content is empty. retest!msg:" + newsId.ToString());

				base.OnDelayEvent(this, newsMessage);

				return null;
			}
			int creativeType = ConvertHelper.GetInteger(CommonFunction.GetXmlElementInnerText(newsNode, "CreativeType", "-1"));
			if (!CommonData.NewsCategoryConfig.CMSCreativeTypes.Contains(creativeType))
			{
				Log.WriteLog("leave news! CreativeType:" + creativeType.ToString());
				return null;
			}

			Dictionary<int, int> carNewsTypes = GetCarNewsTypes(newsNode);
			if (carNewsTypes == null || carNewsTypes.Count < 1)
			{
				Log.WriteLog("error not found CarNewsType!newsid:" + newsId);
				return null;
			}

			NewsContent news = new NewsContent();
			news.CmsNewsId = newsId;
			news.CarNewsTypes = carNewsTypes;
			news.CreativeType = creativeType;
			news.IsSate = true;

			news.EditorId = ConvertHelper.GetInteger(CommonFunction.GetXmlElementInnerText(newsNode, "editor/editorId", string.Empty));
			news.EditorName = CommonFunction.GetXmlElementInnerText(newsNode, "editor/editorName", string.Empty);

			GetRelatedProvinceAndCityList(newsNode, news);

			news.CarId = ConvertHelper.GetInteger(CommonFunction.GetXmlElementInnerText(newsNode, "carId", string.Empty));
			news.YearType = ConvertHelper.GetInteger(CommonFunction.GetXmlElementInnerText(newsNode, "YearType", string.Empty));

			news.PublishTime = ConvertHelper.GetDateTime(CommonFunction.GetXmlElementInnerText(newsNode, "publishtime", string.Empty));

			news.Title = CommonFunction.HtmlDecode(CommonFunction.GetXmlElementInnerText(newsNode, "title", string.Empty));
			news.Author = CommonFunction.GetXmlElementInnerText(newsNode, "author", string.Empty);
			news.Content = CommonFunction.GetXmlElementInnerText(newsNode, "content", string.Empty);
			news.Summary = CommonFunction.GetXmlElementInnerText(newsNode, "summary", string.Empty);
			news.Picture = CommonFunction.GetXmlElementInnerText(newsNode, "picture", string.Empty);
			news.SourceUrl = CommonFunction.GetXmlElementInnerText(newsNode, "sourceUrl", string.Empty);
			news.SourceName = CommonFunction.GetXmlElementInnerText(newsNode, "sourceName", string.Empty);
			news.FirstPicUrl = CommonFunction.GetXmlElementInnerText(newsNode, "firstPicUrl", string.Empty);
			news.FilePath = CommonFunction.GetXmlElementInnerText(newsNode, "filepath", string.Empty);
			news.FaceTitle = CommonFunction.HtmlDecode(CommonFunction.GetXmlElementInnerText(newsNode, "facetitle", string.Empty));

			news.Duration = CommonFunction.GetXmlElementInnerText(newsNode, "duration", string.Empty);

			//开始时间
			DateTime beginDate;
			string beginDateStr = CommonFunction.GetXmlElementInnerText(newsNode, "Market/ValidStartTime", string.Empty);
			if (!string.IsNullOrEmpty(beginDateStr) && DateTime.TryParse(beginDateStr, out beginDate))
				news.BeginDate = beginDate.Date;
			DateTime endDate;
			//结束时间
			string endDateStr = CommonFunction.GetXmlElementInnerText(newsNode, "Market/ValidEndTime", string.Empty);
			if (!string.IsNullOrEmpty(endDateStr) && DateTime.TryParse(endDateStr, out endDate))
				news.EndDate = endDate.Date;

			int pageCount;
			news.NewsPages = GetNewsPageInfoList(news, newsNode, out pageCount);
			// add by chengl Oct.24.2012
			if (news.NewsPages == null || news.NewsPages.Count < 1)
			{
				Log.WriteLog("error news.NewsPages.Count < 1 newsid:" + newsId);
				return null;
			}
			news.PageCount = pageCount;

			//modified by sk 2014.12.11 从评测文章限制移动到所有文章 要求 是否为北京编辑
			if (string.IsNullOrEmpty(news.EditorName) || !CommonData.CommonSettings.EditorInBeijing.Contains(news.EditorName))
			{
				Log.WriteLog("非总部编辑 newsid:" + newsId);
				return null;
			}

			//news.BrandIdList = GetNewsRelatedBrandList(newsDoc);

			// modified by chengl Oct.9.2013
			// 评测新闻分类有31 增加 29、32 
			// 同事调整逻辑为检查不符合的去掉评测分类对于
			CheckPingCeNews(news);

			// del by chengl Oct.9.2013
			//if (NeedShijiaToPingce(news))
			//{
			//    news.CarNewsTypes.Add((int)CarNewsTypes.pingce, news.CarNewsTypes[(int)CarNewsTypes.shijia]);
			//}

			Log.WriteLog("get news [" + news.CmsNewsId + "] content object successed!");

			return news;
		}
		private Dictionary<int, int> GetCarNewsTypes(XmlElement newsNode)
		{
			List<int> cateIds = CommonFunction.StringToIntList(CommonFunction.GetXmlElementInnerText(newsNode, "relatedNewsCategory", string.Empty));
			if (cateIds == null || cateIds.Count < 1)
			{
				int cateId = ConvertHelper.GetInteger(CommonFunction.GetXmlElementInnerText(newsNode, "newscategoryid", "-1"));
				if (CommonData.CategoryCarNewsTypeDic.ContainsKey(cateId))
				{
					return CommonData.CategoryCarNewsTypeDic[cateId].ToDictionary(item => item, item => cateId);
				}
				else
				{
					return null;
				}
			}

			Dictionary<int, int> result = new Dictionary<int, int>(cateIds.Count * 3);
			foreach (int cateId in cateIds)
			{
				if (CommonData.CategoryCarNewsTypeDic.ContainsKey(cateId))
				{
					foreach (int carNewsTypeId in CommonData.CategoryCarNewsTypeDic[cateId])
					{
						if (result.ContainsKey(carNewsTypeId)) continue;
						result.Add(carNewsTypeId, cateId);
					}
				}
			}

			// add by chengl Oct.17.2013 检查当前分类的父类 符合条件的 把当前分类和父分类对应的车型频道分类也入库
			foreach (int cateId in cateIds)
			{
				if (CommonData.CategoryPathDic.ContainsKey(cateId))
				{
					foreach (int fatherCateID in CommonData.CategoryPathDic[cateId])
					{
						// 父分类不是当前分类 并且 车型频道分类有父分类的对应关系
						if (fatherCateID != cateId
							&& CommonData.CategoryCarNewsTypeDic.ContainsKey(fatherCateID))
						{
							foreach (int carNewsTypeId in CommonData.CategoryCarNewsTypeDic[fatherCateID])
							{
								if (result.ContainsKey(carNewsTypeId)) continue;
								result.Add(carNewsTypeId, cateId);
							}
						}
					}
				}
			}

			//是否删除子品牌焦点新闻分类
			int serialfocusId = (int)CarNewsTypes.serialfocus;
			if (result.ContainsKey(serialfocusId))
			{
				if (!CommonData.NewsCategoryConfig.SerialFocusVideoCategoryIds.Contains(result[serialfocusId]))//是否视频,如果是视频则直接通过
				{
					//是否全国文章
					if (newsNode.SelectSingleNode("relatedcityid[text()='0' or contains(concat(',',text(),','),',0,')]") == null)
					{
						result.Remove(serialfocusId);
					}
					////if (!IsNoEditer(newsNode))//总部编辑
					////    return true;
				}
			}
			return result;
		}
		/// <summary>
		/// 如果不是总部编辑
		/// </summary>
		/// <param name="xNode">新闻结点</param>
		/// <returns>如果是总部编辑返回:true;不是则返回:false</returns>
		private bool IsNoEditer(XmlNode xNode)
		{
			XmlNode editeNode = xNode.SelectSingleNode("editor/editorName");
			if (editeNode == null || string.IsNullOrEmpty(editeNode.InnerText))
				return false;

			return CommonData.CommonSettings.EditorInBeijing.Contains(editeNode.InnerText);
		}

		/// <summary>
		/// 判断一篇文章是否属于原创
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		private bool IsOriginal(XmlNode xmlNode)
		{
			bool isOriginal = false;
			XmlNode rNode = xmlNode.SelectSingleNode("CreativeType");
			if (rNode != null && rNode.InnerText == "0")
				isOriginal = true;
			return isOriginal;

		}
		/// <summary>
		/// 获取新闻关联的品牌ID列表
		/// </summary>
		/// <param name="newsDoc"></param>
		/// <returns></returns>
		private List<int> GetNewsRelatedBrandList(XmlDocument newsDoc)
		{
			XmlNode brandsNode = newsDoc.SelectSingleNode("/NewDataSet/Table/RelatedBigBrand");
			List<int> brandIdList = new List<int>();
			if (brandsNode != null)
			{
				string[] idStrArray = brandsNode.InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string idStr in idStrArray)
				{
					int brandId = 0;
					bool isId = Int32.TryParse(idStr, out brandId);
					if (isId)
						brandIdList.Add(brandId);
				}
			}
			return brandIdList;
		}
		/// <summary>
		/// 获取新闻的分页信息
		/// </summary>
		private Dictionary<int, NewsPageInfo> GetNewsPageInfoList(NewsContent news, XmlElement newsNode, out int pageCount)
		{
			Dictionary<int, NewsPageInfo> pageInfoList = new Dictionary<int, NewsPageInfo>();
			XmlNodeList pageNodeList = newsNode.SelectNodes("PageList/ContentInfo");
			pageCount = pageNodeList.Count;
			int pageIndex = 0;
			bool isFirst = true;
			foreach (XmlElement pageNode in pageNodeList)
			{
				int serialId = 0;
				bool isId = Int32.TryParse(CommonFunction.GetXmlElementInnerText(pageNode, "BrandId", null), out serialId);
				if (isId && !pageInfoList.ContainsKey(serialId))
				{
					NewsPageInfo pageInfo = new NewsPageInfo();
					pageInfo.SerialId = serialId;

					pageInfo.PageContent = CommonFunction.HtmlDecode(CommonFunction.GetXmlElementInnerText(pageNode, "PageContent", string.Empty));
					if (string.IsNullOrEmpty(pageInfo.PageContent))
					{
						pageInfo.PageContent = news.Content;
					}
					else if (pageInfo.PageContent.Length > 100)
						pageInfo.PageContent = pageInfo.PageContent.Substring(0, 100);

					pageInfo.PageTitle = CommonFunction.HtmlDecode(CommonFunction.GetXmlElementInnerText(pageNode, "PageTitle", string.Empty));
					if (string.IsNullOrEmpty(pageInfo.PageTitle))
						pageInfo.PageTitle = news.Title;

					pageInfo.PageLink = CommonFunction.GetXmlElementInnerText(pageNode, "PageLink", string.Empty);
					if (string.IsNullOrEmpty(pageInfo.PageLink))
						pageInfo.PageLink = news.FilePath;

					string yearType = CommonFunction.GetXmlElementInnerText(pageNode, "YearType", string.Empty);
					int year = 0;
					Int32.TryParse(yearType, out year);
					pageInfo.SerialYear = year;

					//关联的车型ID
					int carId = 0;
					Int32.TryParse(CommonFunction.GetXmlElementInnerText(pageNode, "carId", "0"), out carId);
					pageInfo.CarId = carId;

					pageInfo.PageIndex = pageIndex;

					//分页第一张图片链接
					pageInfo.FirstPicUrl = CommonFunction.GetXmlElementInnerText(pageNode, "pageFirstPicUrl", string.Empty);
					if (string.IsNullOrEmpty(pageInfo.FirstPicUrl))
						pageInfo.FirstPicUrl = news.FirstPicUrl;

					pageInfo.IsFirst = isFirst;
					if (isFirst)
						isFirst = false;

					pageInfoList.Add(pageInfo.SerialId, pageInfo);
				}
				pageIndex++;
			}

			return pageInfoList;
		}
		/// <summary>
		/// 获取关联的城市列表
		/// </summary>
		private void GetRelatedProvinceAndCityList(XmlElement newsNode, NewsContent news)
		{
			if (news == null) return;
			List<int> citys = null, provinces = null;
			string tempStr = CommonFunction.GetXmlElementInnerText(newsNode, "relatedcityid", string.Empty);
			string[] cityList = tempStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (cityList.Contains("0"))
			{
				citys = new List<int>(1);
				citys.Add(0);
				provinces = new List<int>(1);
				provinces.Add(0);
			}
			else
			{
				int city, province;
				citys = new List<int>(cityList.Length);
				provinces = new List<int>(cityList.Length);
				foreach (string cityStr in cityList)
				{
					city = ConvertHelper.GetInteger(cityStr);
					if (citys.Contains(city) || !CommonData.CityAndProvinceDic.ContainsKey(city))
						continue;
					citys.Add(city);

					if (!CommonData.CityAndProvinceDic.ContainsKey(city)) continue;
					province = CommonData.CityAndProvinceDic[city];
					if (provinces.Contains(province)) continue;
					provinces.Add(province);
				}
			}
			news.NewsCitys = citys;
			news.NewsProvinces = provinces;
		}
		/// <summary>
		/// 判断是否删除
		/// </summary>
		private bool IsDeleteNews(ContentMessage newsMessage)
		{
			XmlNode delEle = newsMessage.ContentBody.SelectSingleNode("/MessageBody/DeleteOp");

			return (delEle == null) ? false : Convert.ToBoolean(delEle.InnerText);
		}

		/// <summary>
		/// 检查评测新闻的条件
		/// </summary>
		/// <param name="newsMsg"></param>
		/// <returns></returns>
		private void CheckPingCeNews(NewsContent newsMsg)
		{
			if (newsMsg.CarNewsTypes.ContainsKey((int)CarNewsTypes.pingce))
			{
				bool isValid = true;
				//是否为原创
				if (newsMsg.CreativeType != 0)
				{ isValid = false; }

				// modified by chengl Sep.25.2013
				//是否有2个分页
				if (newsMsg.PageCount < 2)
				{ isValid = false; }

				//先检查日期：要晚于2009-1-1
				if (newsMsg.PublishTime < new DateTime(2000, 1, 1))
				{ isValid = false; }

				////是否为北京编辑
				//if (string.IsNullOrEmpty(newsMsg.EditorName))
				//{ isValid = false; }
				//if (!CommonData.CommonSettings.EditorInBeijing.Contains(newsMsg.EditorName))
				//{ isValid = false; }

				// 如果评测新闻不符合条件
				if (!isValid)
				{
					newsMsg.CarNewsTypes.Remove((int)CarNewsTypes.pingce);
				}
			}
		}

		///// <summary>
		///// 检查是否要把试驾的文章加入到评测中
		///// </summary>
		///// <param name="newsMsg"></param>
		///// <returns></returns>
		//private bool NeedShijiaToPingce(NewsContent newsMsg)
		//{
		//    if (!newsMsg.CarNewsTypes.ContainsKey((int)CarNewsTypes.shijia))
		//        return false;
		//    if (newsMsg.CarNewsTypes.ContainsKey((int)CarNewsTypes.pingce))
		//        return false;

		//    //news.CategoryId
		//    //单车试驾分类ID：29
		//    if (!CommonData.CategoryPathDic[newsMsg.CarNewsTypes[(int)CarNewsTypes.shijia]].Contains(29))
		//        return false;

		//    //是否为原创
		//    if (newsMsg.CreativeType != 0)
		//        return false;

		//    // modified by chengl Sep.25.2013
		//    //是否有2个分页
		//    if (newsMsg.PageCount < 2)
		//        return false;

		//    //先检查日期：要晚于2009-6-1
		//    if (newsMsg.PublishTime < new DateTime(2009, 6, 1))
		//        return false;

		//    //是否为北京编辑
		//    if (string.IsNullOrEmpty(newsMsg.EditorName))
		//        return false;
		//    if (CommonData.CommonSettings.EditorInBeijing.Contains(newsMsg.EditorName))
		//        return true;
		//    else
		//        return false;
		//}
		/// <summary>
		/// 获取已存在新闻关联的CarNewsTypeId和子品牌id
		/// </summary>
		private NewsRelatedData GetNewsRelatedCarNewsTypeAndSerialId(int cmsNewsId)
		{
			NewsRelatedData oldData = new NewsRelatedData();
			if (cmsNewsId < 1)
				return oldData;

			Log.WriteLog("search Related id:" + cmsNewsId.ToString());

			/* 1, 关联的 CarNewsTypeId列表 int
			* 2，关联的 SerialId列表 int
			* 3，关联的 RelateCityId列表 int
			* 4，关联的 RelateProvinceId列表 int
			 */
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, "dbo.GetCmsNewsCarNewsTypeIdAndSerialId", new SqlParameter("@cmsNewsId", cmsNewsId));

			if (ds != null && ds.Tables.Count > 1)
			{
				DataRowCollection typesRows = ds.Tables[0].Rows,
					serialsRows = ds.Tables[1].Rows,
					cityRows = ds.Tables[2].Rows,
					provinceRows = ds.Tables[3].Rows;
				if (typesRows.Count > 0)
				{
					oldData.CarNewsTypeIds = new List<int>(typesRows.Count);
					foreach (DataRow row in typesRows)
					{
						oldData.CarNewsTypeIds.Add(ConvertHelper.GetInteger(row["CarNewsTypeId"]));
					}
				}
				if (serialsRows.Count > 0)
				{
					oldData.Serials = new List<int>(serialsRows.Count);
					foreach (DataRow row in serialsRows)
					{
						oldData.Serials.Add(ConvertHelper.GetInteger(row["SerialId"]));
					}
				}
				if (cityRows.Count > 0)
				{
					oldData.CityIds = new List<int>(cityRows.Count);
					foreach (DataRow row in cityRows)
					{
						oldData.CityIds.Add(ConvertHelper.GetInteger(row["RelateCityId"]));
					}
				}
				if (provinceRows.Count > 0)
				{
					oldData.ProvinceIds = new List<int>(provinceRows.Count);
					foreach (DataRow row in provinceRows)
					{
						oldData.ProvinceIds.Add(ConvertHelper.GetInteger(row["RelateProvinceId"]));
					}
				}
				Log.WriteLog(string.Format("Related count types:{0},serials:{1},citys:{2},Provinces:{3}", typesRows.Count.ToString(), serialsRows.Count.ToString(), cityRows.Count.ToString(), provinceRows.Count.ToString()));
			}
			return oldData;
		}

		/// <summary>
		/// 关联数据
		/// </summary>
		private struct NewsRelatedData
		{
			public List<int> Serials;
			public List<int> CarNewsTypeIds;
			public List<int> CityIds;
			public List<int> ProvinceIds;
		}
	}
}
