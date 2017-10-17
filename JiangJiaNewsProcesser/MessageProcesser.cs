using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common;
using System.Xml;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using System.Data;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.dealer.api.jiangjia;
using BitAuto.CarDataUpdate.Common.Model.JiangJiaNews;

namespace BitAuto.CarDataUpdate.JiangJiaNewsProcesser
{
	/// <summary>
	/// 降价消息处理类
	/// 该对象将被缓存，定义变量时
	/// </summary>
	public class MessageProcesser : BaseProcesser
	{
		public MessageProcesser()
		{

		}

		public override void Processer(ContentMessage newsMessage)
		{
			if (newsMessage == null)
			{
				Log.WriteLog("error, ContentMessage is null from JiangJiaNewsProcesser!");
				return;
			}

			Log.WriteLog(string.Format("start processer JiangJianews [{0}] !", newsMessage.ContentId));

			List<JiangJiaNewsRelatedData> updateData = null;
			JiangJiaNewsRelatedData relatedData = GetNewsRelatedCarNewsTypeAndSerialId(newsMessage.ContentId);

			// 延期新闻在此删除 add by chengl Sep.3.2013
			if (IsDeleteNews(newsMessage) || newsMessage.UpdateTime>DateTime.Now)
			{
				//删除操作
				DeleteNewsDataByCmsNewsId(newsMessage.ContentId, false);
			}
			else
			{
				JiangJiaNewsContent news = GetJiangJiaNewsContent(newsMessage);
				if (news == null)
				{
					DeleteNewsDataByCmsNewsId(newsMessage.ContentId, false);
				}
				else
				{
					if (SaveNewsData(news))
					{
						updateData = UnionData(relatedData, news);
					}
				}
			}

			DataProcesser.JiangJiaNews jiangjiaNews = new DataProcesser.JiangJiaNews();
			////更新汇总数据
			if (updateData != null)
				jiangjiaNews.UpdateJiangJiaSummary(updateData);
			else
			{
				if (relatedData.SerialId > 0 && relatedData.CarIds != null && relatedData.CarIds.Count > 0)
				{
					jiangjiaNews.UpdateJiangJiaSummary(relatedData);
				}
			}

			Log.WriteLog(string.Format("end processer JiangJianews [{0}] !", newsMessage.ContentId));
		}
		/// <summary>
		/// 合并旧数据与新数据
		/// 合并原则：
		/// 1，如果新数据与旧数据的子品牌id相同，则将省id、市id、车型id合并到oldRelatedData，并返回；
		/// 2，如果新数据与旧数据的子品牌id不相同，并且旧数据不含有车型列表，那么只返回新数据，
		///		否则将返回的list为两项，第一项为oldRelatedData，第二项由newNews转为RelatedData，返回；
		/// 3，只有新数据，则返回新数据
		/// 4，只有旧数据，则返回旧数据
		/// </summary>
		private List<JiangJiaNewsRelatedData> UnionData(JiangJiaNewsRelatedData oldRelatedData, JiangJiaNewsContent newNews)
		{
			List<JiangJiaNewsRelatedData> result = null;
			if (oldRelatedData.SerialId > 0 && newNews != null && newNews.SerialId > 0)
			{
				List<int> carIds = newNews.CarList.ConvertAll<int>(item => item.CarId);
				if (oldRelatedData.SerialId == newNews.SerialId)
				{
					if (!oldRelatedData.CityIds.Contains(newNews.CityId))
					{
						oldRelatedData.CityIds.Add(newNews.CityId);
					}

					if (!oldRelatedData.ProvinceIds.Contains(newNews.ProvinceId))
					{
						oldRelatedData.ProvinceIds.Add(newNews.ProvinceId);
					}

					if (oldRelatedData.CarIds == null || oldRelatedData.CarIds.Count < 1)
						oldRelatedData.CarIds = carIds;
					else
						oldRelatedData.CarIds = oldRelatedData.CarIds.Union(carIds).Distinct().ToList();

					result = new List<JiangJiaNewsRelatedData> { oldRelatedData };
				}
				else
				{
					JiangJiaNewsRelatedData newRelatedData = new JiangJiaNewsRelatedData()
					{
						SerialId = newNews.SerialId,
						CarIds = carIds,
						CityIds = new List<int> { newNews.CityId },
						ProvinceIds = new List<int> { newNews.ProvinceId }
					};

					if (oldRelatedData.CarIds == null || oldRelatedData.CarIds.Count < 1)
					{
						result = new List<JiangJiaNewsRelatedData> { newRelatedData };
					}
					else
					{
						result = new List<JiangJiaNewsRelatedData> { oldRelatedData, newRelatedData };
					}
				}
			}
			else if (newNews != null && newNews.SerialId > 0)
			{
				JiangJiaNewsRelatedData newRelatedData = new JiangJiaNewsRelatedData()
				{
					SerialId = newNews.SerialId,
					CarIds = newNews.CarList.ConvertAll<int>(item => item.CarId),
					CityIds = new List<int> { newNews.CityId },
					ProvinceIds = new List<int> { newNews.ProvinceId }
				};
				result = new List<JiangJiaNewsRelatedData> { newRelatedData };
			}
			else if (oldRelatedData.SerialId > 0
				 && oldRelatedData.CarIds != null && oldRelatedData.CarIds.Count > 0)
			{
				result = new List<JiangJiaNewsRelatedData> { oldRelatedData };
			}
			return result;
		}
		/// <summary>
		/// 保存数据
		/// </summary>
		private bool SaveNewsData(JiangJiaNewsContent news)
		{
			if (news == null)
				return false;

			Log.WriteLog(string.Format("start save JiangJiaNews [{0}] !", news.NewsId.ToString()));

			#region 删除已有新闻
			if (!DeleteNewsDataByCmsNewsId(news.NewsId, true))
				return false;
			#endregion

			#region 主表
			SqlParameter newsIdParam = new SqlParameter("@NewsId", SqlDbType.Int, 4);
			newsIdParam.Value = news.NewsId;
			SqlParameter titleParam = new SqlParameter("@Title", SqlDbType.VarChar, 200);
			titleParam.Value = news.Title;
			SqlParameter vendorIdParam = new SqlParameter("@VendorId", SqlDbType.Int, 4);
			vendorIdParam.Value = news.VendorId;
			SqlParameter vendorNameParam = new SqlParameter("@VendorName", SqlDbType.VarChar, 200);
			vendorNameParam.Value = news.VendorName;
			SqlParameter authorParam = new SqlParameter("@Author", SqlDbType.VarChar, 50);
			authorParam.Value = news.Author;
			SqlParameter publishTimeParam = new SqlParameter("@PublishTime", SqlDbType.DateTime, 8);
			publishTimeParam.Value = news.PublishTime;
			SqlParameter startDateParam = new SqlParameter("@StartDateTime", SqlDbType.DateTime, 8);
			startDateParam.Value = news.StartDateTime;
			SqlParameter endDateParam = new SqlParameter("@EndDateTime", SqlDbType.DateTime, 8);
			endDateParam.Value = news.EndDateTime;
			SqlParameter newsUrlParam = new SqlParameter("@NewsUrl", SqlDbType.VarChar);
			newsUrlParam.Value = news.NewsUrl;
			SqlParameter carImageParam = new SqlParameter("@CarImage", SqlDbType.VarChar, 300);
			carImageParam.Value = news.CarImage;
			SqlParameter otherInfoParam = new SqlParameter("@OtherInfo", SqlDbType.NVarChar, -1);
			otherInfoParam.Value = news.OtherInfo;
			SqlParameter masterIdParam = new SqlParameter("@MasterId", SqlDbType.Int, 4);
			masterIdParam.Value = news.MasterBrandId;
			SqlParameter brandIdParam = new SqlParameter("@BrandId", SqlDbType.Int, 4);
			brandIdParam.Value = news.BrandId;
			SqlParameter serialIdParam = new SqlParameter("@SerialId", SqlDbType.Int, 4);
			serialIdParam.Value = news.SerialId;
			SqlParameter cityIdParam = new SqlParameter("@CityId", SqlDbType.Int, 4);
			cityIdParam.Value = news.CityId;
			SqlParameter provinceIdParam = new SqlParameter("@ProvinceId", SqlDbType.Int, 4);
			provinceIdParam.Value = news.ProvinceId;
			SqlParameter maxPriceParam = new SqlParameter("@MaxFavorablePrice", SqlDbType.Decimal);
			maxPriceParam.Precision = 8;
			maxPriceParam.Scale = 3;
			maxPriceParam.Value = news.MaxFavorablePrice;
            
            SqlParameter maxRateParam = new SqlParameter("@MaxFavorableRate", SqlDbType.Decimal);
            maxRateParam.Precision = 6;
            maxRateParam.Scale = 2;
            maxRateParam.Value = news.MaxFavorableRate;

			SqlParameter isStateParam = new SqlParameter("@IsState", SqlDbType.Bit, 1);
			isStateParam.Value = news.IsState;

			SqlParameter[] paramList = new SqlParameter[]
			{
newsIdParam,
titleParam,
vendorIdParam,
vendorNameParam,
authorParam,
publishTimeParam,
startDateParam,
endDateParam,
newsUrlParam,
carImageParam,
otherInfoParam,
masterIdParam,
brandIdParam,
serialIdParam,
cityIdParam,
provinceIdParam,
maxPriceParam,
maxRateParam,
isStateParam
			}; 
			#endregion

			#region 车型列表
			SqlParameter mIdParam = new SqlParameter("@MId", SqlDbType.Int, 4);
			SqlParameter carIdParam = new SqlParameter("@CarId", SqlDbType.Int, 4);
			SqlParameter priceParam = new SqlParameter("@FavorablePrice", SqlDbType.Decimal);
			priceParam.Precision = 8;
			priceParam.Scale = 3;

            SqlParameter rateParam = new SqlParameter("@FavorableRate", SqlDbType.Decimal);
            rateParam.Precision = 6;
            rateParam.Scale = 2;

			SqlParameter[] carParamList = new SqlParameter[]
			{
				mIdParam,
				newsIdParam,
				serialIdParam,
				cityIdParam,
				provinceIdParam,
				carIdParam,
				priceParam,
                rateParam
			}; 
			#endregion

			try
			{
				object result = SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text
                    , "INSERT INTO JiangJiaNews( NewsId,Title,VendorId,VendorName,Author,PublishTime,StartDateTime,EndDateTime,NewsUrl,CarImage,OtherInfo,MasterId,BrandId,SerialId,CityId,ProvinceId,MaxFavorablePrice,MaxFavorableRate,IsState) OUTPUT INSERTED.Id VALUES ( @NewsId,@Title,@VendorId,@VendorName,@Author,@PublishTime,@StartDateTime,@EndDateTime,@NewsUrl,@CarImage,@OtherInfo,@MasterId,@BrandId,@SerialId,@CityId,@ProvinceId,@MaxFavorablePrice,@MaxFavorableRate,@IsState)"
					, paramList);
				int rowId = ConvertHelper.GetInteger(result);
				if (rowId > 0)
				{
					mIdParam.Value = rowId;
					Log.WriteLog(string.Format("end save JiangJiaNews successed! msg:[id:{0},newsid:{1}] !", rowId.ToString(), news.NewsId.ToString()));

					Log.WriteLog("start save JiangJiaNewsCarList!");
					foreach (JiangJiaNewsCarContent carContent in news.CarList)
					{
						carIdParam.Value = carContent.CarId;
						priceParam.Value = carContent.FavorablePrice;
                        rateParam.Value = carContent.FavorableRate;

						SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text
                        , "INSERT INTO JiangJiaNewsCarList(NewsId, MId,SerialId, CityId, ProvinceId,CarId, FavorablePrice, FavorableRate) VALUES ( @NewsId, @MId, @SerialId, @CityId, @ProvinceId, @CarId, @FavorablePrice, @FavorableRate)"
					, carParamList);
					}
					Log.WriteLog("end save JiangJiaNewsCarList!");
					return true;
				}
				else
				{
					Log.WriteLog(string.Format("end save JiangJiaNews failed! msg:[newsid:{0}] !", news.NewsId.ToString()));
					return false;
				}
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(string.Format("end save JiangJiaNews error! msg:[newsid:{0},msg:{1}] !", news.NewsId.ToString(), exp.ToString()));
				return false;
			}
		}
		/// <summary>
		/// 获取降价新闻
		/// </summary>
		private JiangJiaNewsContent GetJiangJiaNewsContent(ContentMessage newsMessage)
		{
			int newsId = newsMessage.ContentId;
			DataSet ds = null;
			try
			{
				GetFavourableNews newsService = new DataProcesser.com.bitauto.dealer.api.jiangjia.GetFavourableNews();
				ds = newsService.GetNewsInfoByID(newsId);
			}
			catch (Exception exp)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews service msg:[id:{0},errormsg:{1}]!", newsId, exp.ToString()));
				return null;
			}
			if (ds == null || ds.Tables.Count < 2 || ds.Tables[0].Rows.Count < 1 || ds.Tables[1].Rows.Count < 1)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews content failed! newsid:{0}", newsId));
				return null;
			}

			#region 降价新闻主表
			JiangJiaNewsContent newsObj = JiangJiaNewsContent.GetObjectByDataRow(ds.Tables[0].Rows[0]);

			//子品牌
			if (newsObj.SerialId < 1)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews serialid is null! newsid:{0}", newsId));
				return null;
			}
			//主品牌
			if (newsObj.MasterBrandId < 1)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews masterbrandid is null! newsid:{0}", newsId));
				return null;
			}
			//品牌
			if (newsObj.BrandId < 1)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews brandid is null! newsid:{0}", newsId));
				return null;
			}
			//城市
			if (newsObj.CityId < 1)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews cityid is null! newsid:{0}", newsId));
				return null;
			}
			//开始日期
			if (newsObj.StartDateTime == DateTime.MinValue)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews starttime is null! newsid:{0}", newsId));
				return null;
			}
			//结束日期
			if (newsObj.EndDateTime == DateTime.MinValue)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews endtime is null! newsid:{0}", newsId));
				return null;
			}
			//当前日前大于结束日期
			if (newsObj.EndDateTime.Date < DateTime.Now.Date)
			{
				Log.WriteLog(string.Format("error get JiangJiaNews endtime 小于当前日期! newsid:{0}, endtime:{1}", newsId, newsObj.EndDateTime.Date));
				return null;
			}
			#endregion

			#region 降价新闻车型表
			newsObj.CarList = new List<JiangJiaNewsCarContent>(ds.Tables[1].Rows.Count);
			foreach (DataRow carRow in ds.Tables[1].Rows)
			{
				newsObj.CarList.Add(JiangJiaNewsCarContent.GetObjectByDataRow(carRow));
			}
			#endregion

            //20130523 anh 目前降价金额有0的情况，为避免再有小于0的情况，所以取消此判断
            ////最高优惠
            //if (newsObj.MaxFavorablePrice < decimal.Zero)
            //{
            //    Log.WriteLog(string.Format("error get JiangJiaNews MaxFavorablePrice is null! newsid:{0}", newsId));
            //    return null;
            //}

			return newsObj;
		}

		/// <summary>
		/// 根据新闻id，删除新闻相关数据
		/// </summary>
		private bool DeleteNewsDataByCmsNewsId(int newsId, bool isDelete)
		{
			Log.WriteLog(string.Format("start delete JiangJiaNews [{0}]. isdelete [{1}]!", newsId.ToString(), isDelete.ToString()));
			try
			{
				string sql = string.Empty;
				SqlParameter param = new SqlParameter("@NewsId", System.Data.SqlDbType.Int, 4);
				param.Value = newsId;
				if (isDelete)
				{
					sql = "DELETE JiangJiaNews WHERE NewsId=@NewsId;";
				}
				else
				{
					sql = "UPDATE JiangJiaNews SET IsState = 0 WHERE NewsId=@NewsId;";
				}
				sql += "DELETE FROM JiangJiaNewsCarList WHERE NewsId=@NewsId";

				SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, param);
				
				Log.WriteLog(string.Format("delete JiangJiaNews [{0}] successed. isdelete [{1}]!", newsId.ToString(), isDelete.ToString()));
				return true;
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(exp);
				return false;
			}
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
		/// 获取已存在新闻关联的CarNewsTypeId和子品牌id
		/// </summary>
		private JiangJiaNewsRelatedData GetNewsRelatedCarNewsTypeAndSerialId(int newsId)
		{
			JiangJiaNewsRelatedData oldData = new JiangJiaNewsRelatedData();
			if (newsId < 1)
				return oldData;

			Log.WriteLog("search JiangJiaRelated id:" + newsId.ToString());

			/* 
			* 1，关联的 SerialId
			* 2，关联的 RelateCityId
			* 3，关联的 RelateProvinceId
			 */
			SqlParameter param = new SqlParameter("@NewsId", SqlDbType.Int, 4);
			param.Value = newsId;
			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.Text,
				"SELECT  SerialId, CityId, ProvinceId FROM JiangJiaNews WHERE NewsId=@NewsId;SELECT CarId FROM JiangJiaNewsCarList WHERE NewsId=@NewsId",
				param);

			if (ds != null && ds.Tables.Count > 1 && ds.Tables[0].Rows.Count > 0)
			{
				DataRow row = ds.Tables[0].Rows[0];
				oldData.SerialId = ConvertHelper.GetInteger(row["SerialId"].ToString());
				oldData.CityIds = new List<int>();
				oldData.ProvinceIds = new List<int>();
				oldData.CarIds = new List<int>();
				oldData.CityIds.Add(ConvertHelper.GetInteger(row["CityId"].ToString()));
				oldData.ProvinceIds.Add(ConvertHelper.GetInteger(row["ProvinceId"].ToString()));
				if (ds.Tables[1].Rows.Count > 0)
				{
					foreach (DataRow carRow in ds.Tables[1].Rows)
					{
						oldData.CarIds.Add(ConvertHelper.GetInteger(carRow["CarId"].ToString()));
					}
				}
				Log.WriteLog(string.Format("JiangJiaRelated serial:{0},city:{1},province:{2},carids:{3}", oldData.SerialId.ToString(), oldData.CityIds[0].ToString(), oldData.ProvinceIds[0].ToString(), oldData.CarIds.Count.ToString()));
			}
			return oldData;
		}
	}
}
