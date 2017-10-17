using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.Common.Repository
{
	public class VideoRepository
	{
		#region V2
		/// <summary>
		/// 更新视频数据库
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="vsList"></param>
		/// <param name="categoryList"></param>
		/// <returns></returns>
		public static int UpdateVideoV2(VideoEntityV2 entity, List<int> categoryList)
		{
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoId", SqlDbType.BigInt),
											new SqlParameter("@SerialIds", SqlDbType.VarChar),
											new SqlParameter("@CategoryType", SqlDbType.VarChar),
											new SqlParameter("@CategoryId", SqlDbType.Int),
											new SqlParameter("@Title", SqlDbType.NVarChar),
											new SqlParameter("@ShortTitle", SqlDbType.NVarChar),
											new SqlParameter("@EditorName", SqlDbType.NVarChar),
											new SqlParameter("@ImageLink", SqlDbType.VarChar),
											new SqlParameter("@Duration", SqlDbType.Int),
											new SqlParameter("@ShowPlayUrl", SqlDbType.VarChar),
											new SqlParameter("@Publishtime", SqlDbType.DateTime),
											new SqlParameter("@VideoGuid", SqlDbType.UniqueIdentifier),
											new SqlParameter("@Source", SqlDbType.Int),
											new SqlParameter("@UserId", SqlDbType.Int),
											new SqlParameter("@MShowPlayUrl", SqlDbType.VarChar)
									 };
			_params[0].Value = entity.VideoId;
			_params[1].Value = entity.SerialIds;
			_params[2].Value = string.Join(",", categoryList.Select(p => p.ToString()).ToArray());
			_params[3].Value = entity.CategoryId;
			_params[4].Value = entity.Title;
			_params[5].Value = entity.ShortTitle;
			_params[6].Value = entity.EditorName;
			_params[7].Value = entity.ImageLink;
			_params[8].Value = entity.Duration;
			_params[9].Value = entity.ShowPlayUrl;
			_params[10].Value = entity.Publishtime;
			_params[11].Value = entity.VideoGuId;
			_params[12].Value = entity.Source;
			_params[13].Value = entity.UserId;
			_params[14].Value = entity.MShowPlayUrl;
			return ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.StoredProcedure, "[UpdateCmsVideoV2]", _params));
		}

		/// <summary>
		/// 删除视频
		/// </summary>
		/// <param name="videoId"></param>
		/// <returns></returns>
		public static int DeleteByVideoIdV2(string videoGuid)
		{
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoGuid", SqlDbType.UniqueIdentifier)
									 };
			_params[0].Value = new Guid(videoGuid);
			return SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.StoredProcedure, "[DeleteCmsVideoV2]", _params);
		}

		/// <summary>
		/// 获取视频关联车型
		/// </summary>
		/// <param name="videoId">视频ID</param>
		/// <returns></returns>
		public static DataSet GetRelationCarV2(string videoGuid)
		{
			string sql = @" SELECT  SerialId
							FROM    dbo.Car_VideoToSerialV2
							WHERE   Id = (SELECT TOP 1
														Id
											   FROM     dbo.Car_VideosV2
											   WHERE    VideoGuid = @VideoGuid
											  )";
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoGuid", SqlDbType.UniqueIdentifier)
									 };
			_params[0].Value = new Guid(videoGuid);
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params);
		}
		#endregion



		/// <summary>
		/// 更新视频数据库
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="vsList"></param>
		/// <param name="categoryList"></param>
		/// <returns></returns>
		public static int UpdateVideo(VideoEntity entity, List<VideoToSerialEntity> vsList, List<int> categoryList)
		{
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoId", SqlDbType.BigInt),
											new SqlParameter("@SerialIds", SqlDbType.VarChar),
											new SqlParameter("@CategoryType", SqlDbType.VarChar),
											new SqlParameter("@CategoryId", SqlDbType.Int),
											new SqlParameter("@Title", SqlDbType.NVarChar),
											new SqlParameter("@ShortTitle", SqlDbType.NVarChar),
											new SqlParameter("@EditorName", SqlDbType.NVarChar),
											new SqlParameter("@ImageLink", SqlDbType.VarChar),
											new SqlParameter("@Duration", SqlDbType.Int),
											new SqlParameter("@ShowPlayUrl", SqlDbType.VarChar),
											new SqlParameter("@Publishtime", SqlDbType.DateTime),
											new SqlParameter("@VideoGuid", SqlDbType.UniqueIdentifier)
									 };
			_params[0].Value = entity.VideoId;
			_params[1].Value = string.Join(",", vsList.Select(p => p.SerialId.ToString()).ToArray());
			_params[2].Value = string.Join(",", categoryList.Select(p => p.ToString()).ToArray());
			_params[3].Value = entity.CategoryId;
			_params[4].Value = entity.Title;
			_params[5].Value = entity.ShortTitle;
			_params[6].Value = entity.EditorName;
			_params[7].Value = entity.ImageLink;
			_params[8].Value = entity.Duration;
			_params[9].Value = entity.ShowPlayUrl;
			_params[10].Value = entity.Publishtime;
			_params[11].Value = entity.VideoGuId;
			return SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.StoredProcedure, "[UpdateCmsVideo]", _params);
		}
		/// <summary>
		/// 删除视频
		/// </summary>
		/// <param name="videoId"></param>
		/// <returns></returns>
		public static int DeleteByVideoId(string videoGuid)
		{
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoGuid", SqlDbType.UniqueIdentifier)
									 };
			_params[0].Value = new Guid(videoGuid);
			return SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.StoredProcedure, "[DeleteCmsVideo]", _params);
		}
		/// <summary>
		/// 获取视频数据 根据 子品牌ID
		/// </summary>
		/// <param name="serialId">子品牌ID</param>
		/// <returns></returns>
		public static DataSet GetVideoDataBySerialId(int serialId, int source, int topN)
		{
			string sql = string.Format(@"SELECT TOP (@TopN) v.*,vs.SerialId 
FROM  Car_VideoToSerialV2 vs 
LEFT JOIN Car_VideosV2 v ON vs.Id=v.Id 
WHERE serialId=@serialId {0} 
ORDER BY v.PublishTime desc", source > -1 ? " AND Source=@Source" : "");
			List<SqlParameter> _params = new List<SqlParameter>() { 
										 new SqlParameter("@serialId", SqlDbType.Int),
										new SqlParameter("@TopN", SqlDbType.Int)
									 };
			_params[0].Value = serialId;
			_params[1].Value = topN;

			if (source > -1)
			{
				_params.Add(new SqlParameter("@Source", SqlDbType.Int));
				_params[2].Value = source;
			}
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params.ToArray());
		}
		/// <summary>
		/// 根据多个子品牌Id 取视频信息
		/// </summary>
		/// <param name="serialIds">子品牌ID （逗号分隔）</param>
		/// <param name="source">来源</param>
		/// <param name="topN">前几条</param>
		/// <returns></returns>
		public static DataSet GetVideoDataBySerialIdsAndCategoryId(string serialIds, int source, int categoryId, int topN)
		{
			string sql = string.Format(@"SELECT TOP (@TopN)
												v.*
										FROM    Car_VideosV2 v
										WHERE   Id IN (SELECT 
																	Id
															FROM    Car_VideoToSerialV2
															WHERE   serialId IN ({0})) AND CategoryId=@CategoryId {1}
										ORDER BY v.PublishTime DESC"
				, serialIds
				,source == (int)VideoEnum.VideoSource.All ? string.Empty : " AND Source=@Source");

			List<SqlParameter> _params = new List<SqlParameter>() { 
										new SqlParameter("@TopN", SqlDbType.Int),
										new SqlParameter("@CategoryId", SqlDbType.Int),
									 };
			_params[0].Value = topN;
			_params[1].Value = categoryId;
			if (source != (int)VideoEnum.VideoSource.All)
			{
				_params.Add(new SqlParameter("@Source", SqlDbType.Int));
				_params[2].Value = source;
			}
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params.ToArray());
		}
		/// <summary>
		/// 获取视频数量 根据子品牌id 多个  待修改
		/// </summary>
		/// <param name="serialIds"></param>
		/// <param name="source"></param>
		/// <param name="categoryId"></param>
		/// <returns></returns>
		public static int GetVideoCountBySerialIds(string serialIds, int source)
		{
			string sql = string.Format(@"SELECT  COUNT(DISTINCT Id)
										FROM    Car_VideoToSerialV2
										WHERE   serialId IN ({0})", serialIds);
			List<SqlParameter> _params = new List<SqlParameter>() ;

			if (source > -1)
			{
				sql = string.Format(@"
										SELECT COUNT(DISTINCT vts.id)
										FROM Car_VideoToSerialV2 vts
										LEFT JOIN Car_VideosV2 v ON vts.Id=v.Id 
										WHERE   serialId IN ({0}) AND v.Source=@source", serialIds);
				_params.Add(new SqlParameter("@source", source));
			}
			
			return ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params.ToArray()));
		}
		/// <summary>
		/// 根据多个子品牌Id 取视频信息
		/// </summary>
		/// <param name="serialIds">子品牌ID （逗号分隔）</param>
		/// <param name="source">分类</param>
		/// <param name="topN">前几条</param>
		/// <returns></returns>
		public static DataSet GetVideoDataBySerialIds(string serialIds,int source, int topN)
		{
			string sql = string.Format(@"SELECT TOP (@TopN)
												v.*
										FROM    Car_VideosV2 v
										WHERE   Id IN (SELECT 
																	Id
															FROM    Car_VideoToSerialV2
															WHERE   serialId IN ({0})) {1}
										ORDER BY v.PublishTime DESC"
				, serialIds
				, source > -1 ? " AND Source=@Source" : string.Empty);

			List<SqlParameter> _params = new List<SqlParameter>() { 
										new SqlParameter("@TopN", SqlDbType.Int)
									 };
			_params[0].Value = topN;
			if (source > -1)
			{ 
				_params.Add(new SqlParameter("@Source",SqlDbType.Int));
				_params[1].Value = source;
			}
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params.ToArray());
		}
		/// <summary>
		/// 获取单条视频数据
		/// </summary>
		/// <param name="videoId"></param>
		/// <returns></returns>
		public static DataSet GetVideoDataByVideoId(int videoId,int source)
		{
			string sql = @" SELECT v.* FROM Car_VideosV2 v WHERE VideoId=@VideoId AND Source=@Source";
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoId", SqlDbType.BigInt),
										 new SqlParameter("@Source",SqlDbType.Int)
									 };
			_params[0].Value = videoId;
			_params[0].Value = source;
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params);
		}
		/// <summary>
		/// 获取视频数量
		/// </summary>
		/// <returns></returns>
		public static int GetVideoCountBySerialId(int serialId)
		{
			string sql = @" SELECT COUNT(*) FROM Car_VideoToSerialV2 vs WHERE SerialId=@SerialId";
			SqlParameter[] _params = { 
										 new SqlParameter("@SerialId", SqlDbType.Int)
									 };
			_params[0].Value = serialId;
			return ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params));
		}
		/// <summary>
		/// 获取视频关联车型
		/// </summary>
		/// <param name="videoId">视频ID</param>
		/// <returns></returns>
		public static DataSet GetRelationCar(string videoGuid)
		{
			string sql = @" SELECT  SerialId
							FROM    dbo.Car_VideoToSerialV2
							WHERE   Id = (SELECT TOP 1
														Id
											   FROM     dbo.Car_VideosV2
											   WHERE    VideoGuid = @VideoGuid
											  )";
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoGuid", SqlDbType.UniqueIdentifier)
									 };
			_params[0].Value = new Guid(videoGuid);
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params);
		}
		/// <summary>
		/// 更新视频库 VideoGuid字段
		/// </summary>
		/// <param name="videoId"></param>
		/// <param name="videoGuid"></param>
		public static void UpdateVideoGuidByVideoId(int videoId, string videoGuid)
		{
			string sql = @"UPDATE dbo.Car_Videos SET VideoGuid=@VideoGuid WHERE VideoId=@VideoId";
			SqlParameter[] _params = { 
										 new SqlParameter("@VideoGuid", SqlDbType.UniqueIdentifier),
										  new SqlParameter("@VideoId", SqlDbType.BigInt)
									 };
 			_params[0].Value = new Guid(videoGuid);
			_params[1].Value = videoId;
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, sql, _params);
		}
	}
}
