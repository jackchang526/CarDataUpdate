using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using System.Data;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.Common.Services
{
	public class VideoService
	{
		#region VideoV2 2017-08-02 lisf

		public static List<int> GetRelationCarByDataV2(string videoGuid)
		{
			List<int> list = new List<int>();
			try
			{
				DataSet ds = VideoRepository.GetRelationCarV2(videoGuid);
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					list.Add(ConvertHelper.GetInteger(dr["serialid"]));
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}

		/// <summary>
		/// 删除视频及相关关联子品牌数据
		/// </summary>
		/// <param name="videoId">GUID</param>
		public static void DeleteVideoV2(string videoGuid)
		{
			try
			{
				VideoRepository.DeleteByVideoIdV2(videoGuid);
			}
			catch (Exception ex)
			{
				Log.WriteLog(ex.ToString());
			}
		}
		#endregion





		/*
		/// <summary>
		/// 更新、插入 视频
		/// </summary>
		/// <param name="videoId">视频ID</param>
		public static void UpdateVideo(string videoGuid)
		{
			try
			{
				string videoUrl = string.Format(CommonData.CommonSettings.VideoNewUrl, videoGuid);
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(videoUrl);
				if (xmlDoc != null)
				{
					//加入命名空间和前缀
					var xmlnsm = new XmlNamespaceManager(xmlDoc.NameTable);
					xmlnsm.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/BitAuto.Video.RESTfulApi.ShowModel");

					int videoId = ConvertHelper.GetInteger(xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:VideoId", xmlnsm).InnerText);

					XmlNodeList nodeList = xmlDoc.SelectNodes("//ns:CarModelRelation", xmlnsm);
					List<VideoToSerialEntity> vsList = new List<VideoToSerialEntity>();
					foreach (XmlNode node in nodeList)
					{
						vsList.Add(new VideoToSerialEntity()
						{
							VideoId = videoId,
							BrandId = ConvertHelper.GetInteger(node.SelectSingleNode("./ns:BrandId", xmlnsm).InnerText),
							SerialId = ConvertHelper.GetInteger(node.SelectSingleNode("./ns:SerialId", xmlnsm).InnerText)
						});
					}

					var result = vsList.GroupBy(p => p.SerialId)
						.Select(p => p.ToList<VideoToSerialEntity>().FirstOrDefault())
						.ToList()
						.FindAll(p => p.SerialId > 0);
					if (result.Count <= 0)
					{
						Log.WriteLog("更新视频消息，没有关联子品牌。videoId=" + videoId);
						return;
					}
					int categoryId = ConvertHelper.GetInteger(xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:CategoryId", xmlnsm).InnerText);
					////视频分类
					//List<int> categoryList = GetCarVideoCategoryId(categoryId);
					//if (categoryList.Count <= 0)
					//{
					//    Log.WriteLog("更新视频消息，视频分类不在分类配置中。videoId=" + videoId);
					//    return;
					//}
					//不区分分类 所有视频默认分类 0
					List<int> categoryList = new List<int>() { 0 };

					VideoEntity videoEntity = new VideoEntity();
					videoEntity.VideoGuId = new Guid(videoGuid);
					videoEntity.VideoId = videoId;
					//videoEntity.SerialIds = vsList;
					videoEntity.CategoryId = categoryId;
					videoEntity.Title = xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:Title", xmlnsm).InnerText;
					videoEntity.ShortTitle = xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:ShortTitle", xmlnsm).InnerText;
					videoEntity.EditorName = xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:EditorName", xmlnsm).InnerText;
					videoEntity.ImageLink = xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:ImageLink", xmlnsm).InnerText;
					videoEntity.Duration = ConvertHelper.GetInteger(xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:Duration", xmlnsm).InnerText);
					videoEntity.ShowPlayUrl = xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:ShowPlayUrl", xmlnsm).InnerText;
					videoEntity.Publishtime = ConvertHelper.GetDateTime(xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:PublishDate", xmlnsm).InnerText);
					VideoRepository.UpdateVideo(videoEntity, result, categoryList);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("UGC 视频消息异常，Guid=" + videoGuid + ex.ToString());
			}
		}
		/// <summary>
		/// 删除视频及相关关联子品牌数据
		/// </summary>
		/// <param name="videoId"></param>
		public static void DeleteVideo(string videoGuid)
		{
			try
			{
				VideoRepository.DeleteByVideoId(videoGuid);
			}
			catch (Exception ex)
			{
				Log.WriteLog(ex.ToString());
			}
		}
		 * */
		/// <summary>
		/// 获取视频数据 根据子品牌
		/// </summary>
		/// <param name="serialId">子品牌ID</param>
		/// <returns></returns>
		public static List<VideoEntityV2> GetVideoList(int serialId, VideoEnum.VideoSource source, int top)
		{
			List<VideoEntityV2> list = new List<VideoEntityV2>();
			try
			{
				DataSet ds = VideoRepository.GetVideoDataBySerialId(serialId, (int)source, top);
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					list.Add(GetVideoEntityByDr(dr));
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="masterId"></param>
		/// <param name="source"></param>
		/// <param name="top"></param>
		/// <returns></returns>
		public static List<VideoEntityV2> GetVideoListByMasterId(int masterId, VideoEnum.VideoSource source, int top)
		{
			List<VideoEntityV2> list = new List<VideoEntityV2>();
			try
			{
				string serialIds = string.Join(",", CommonData.SerialMasterBrandDic
													.Where(p => p.Value == masterId)
													.Select(p => p.Key.ToString())
													.Distinct()
													.ToList()
													.ToArray());
				if (!string.IsNullOrEmpty(serialIds))
				{
					DataSet ds = VideoRepository.GetVideoDataBySerialIds(serialIds,(int)source, top);
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						list.Add(GetVideoEntityByDr(dr));
					}
				}
				else
				{
					Log.WriteLog("主品牌下没有子品牌，masterId=" + masterId);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}
		/// <summary>
		/// 根据主品牌ID获取视频数
		/// </summary>
		/// <param name="masterId"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public static int GetVideoCountByMasterId(int masterId, VideoEnum.VideoSource source)
		{
			int count = 0;
			try
			{
				string serialIds = string.Join(",", CommonData.SerialMasterBrandDic
														.Where(p => p.Value == masterId)
														.Select(p => p.Key.ToString())
														.Distinct()
														.ToList()
														.ToArray());
				if (!string.IsNullOrEmpty(serialIds))
				{
					count = VideoRepository.GetVideoCountBySerialIds(serialIds, (int)source);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return count;
		}
		/// <summary>
		/// 根据CMS视频分类获取视频数据
		/// </summary>
		/// <param name="masterId">主品牌ID</param>
		/// <param name="source">车型定义视频来源</param>
		/// <param name="categoryId">Cms视频分类</param>
		/// <param name="top"></param>
		/// <returns></returns>
		public static List<VideoEntityV2> GetVideoListByMasterIdAndCategoryId(int masterId, VideoEnum.VideoSource source, int categoryId, int top)
		{
			List<VideoEntityV2> list = new List<VideoEntityV2>();
			try
			{
				string serialIds = string.Join(",", CommonData.SerialMasterBrandDic
													.Where(p => p.Value == masterId)
													.Select(p => p.Key.ToString())
													.Distinct()
													.ToList()
													.ToArray());
				if (!string.IsNullOrEmpty(serialIds))
				{
					DataSet ds = VideoRepository.GetVideoDataBySerialIdsAndCategoryId(serialIds, (int)source, categoryId, top);
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						list.Add(GetVideoEntityByDr(dr));
					}
				}
				else
				{
					Log.WriteLog("主品牌下没有子品牌，masterId=" + masterId);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}
		/// <summary>
		/// 根据品牌ID 获取视频数据
		/// </summary>
		/// <param name="brandId">品牌ID</param>
		/// <param name="source">视频类型</param>
		/// <param name="top">前几条</param>
		/// <returns></returns>
		public static List<VideoEntityV2> GetVideoListByBrandId(int brandId, VideoEnum.VideoSource source, int top)
		{
			List<VideoEntityV2> list = new List<VideoEntityV2>();
			try
			{
				string serialIds = string.Join(",", CommonData.SerialBrandDic
													.Where(p => p.Value == brandId)
													.Select(p => p.Key.ToString())
													.Distinct()
													.ToList()
													.ToArray());
				if (!string.IsNullOrEmpty(serialIds))
				{
					DataSet ds = VideoRepository.GetVideoDataBySerialIds(serialIds, (int)source, top);
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						list.Add(GetVideoEntityByDr(dr));
					}
				}
				else
				{
					Log.WriteLog("品牌下没有子品牌，brandId=" + brandId);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}
		/// <summary>
		/// 根据品牌Id 获取视频数据
		/// </summary>
		/// <param name="brandId"></param>
		/// <param name="categoryType"></param>
		/// <returns></returns>
		public static int GetVideoCountByBrandId(int brandId, VideoEnum.CategoryTypeEnum categoryType)
		{
			int count = 0;
			try
			{
				string serialIds = string.Join(",", CommonData.SerialBrandDic
													.Where(p => p.Value == brandId)
													.Select(p => p.Key.ToString())
													.Distinct()
													.ToList()
													.ToArray());
				if (!string.IsNullOrEmpty(serialIds))
				{
					count = VideoRepository.GetVideoCountBySerialIds(serialIds, (int)categoryType);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return count;
		}
		/// <summary>
		/// 根据品牌ID 获取视频数据
		/// </summary>
		/// <param name="brandId">品牌ID</param>
		/// <param name="source">车型视频类型</param>
		/// <param name="categoryId">Cms视频分类</param>
		/// <param name="top">前几条</param>
		/// <returns></returns>
		public static List<VideoEntityV2> GetVideoListByBrandIdAndCategoryId(int brandId, VideoEnum.VideoSource source, int categoryId, int top)
		{
			List<VideoEntityV2> list = new List<VideoEntityV2>();
			try
			{
				string serialIds = string.Join(",", CommonData.SerialBrandDic
													.Where(p => p.Value == brandId)
													.Select(p => p.Key.ToString())
													.Distinct()
													.ToList()
													.ToArray());
				if (!string.IsNullOrEmpty(serialIds))
				{
					DataSet ds = VideoRepository.GetVideoDataBySerialIdsAndCategoryId(serialIds, (int)source, categoryId, top);
					foreach (DataRow dr in ds.Tables[0].Rows)
					{
						list.Add(GetVideoEntityByDr(dr));
					}
				}
				else
				{
					Log.WriteLog("品牌下没有子品牌，brandId=" + brandId);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}
		/// <summary>
		/// 获取视频数据 根据视频ID
		/// </summary>
		/// <param name="videoId">视频ID</param>
		/// <param name="source">来源</param>
		/// <returns></returns>
		public static VideoEntityV2 GetVideoByVideoId(int videoId,int source)
		{
			VideoEntityV2 entity = null;
			try
			{
				DataSet ds = VideoRepository.GetVideoDataByVideoId(videoId, source);
				if (ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					entity = GetVideoEntityByDr(dr);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return entity;
		}

		/// <summary>
		/// 根据datarow获取视频实体
		/// </summary>
		/// <param name="dr"></param>
		/// <returns></returns>
		private static VideoEntityV2 GetVideoEntityByDr(DataRow dr)
		{
			if (dr == null) return null;
			VideoEntityV2 entity = new VideoEntityV2();
			entity.Id = ConvertHelper.GetInteger(dr["Id"]);
			entity.VideoId = ConvertHelper.GetInteger(dr["VideoId"]);
			entity.CategoryId = ConvertHelper.GetInteger(dr["CategoryId"]);
			entity.Title = dr["Title"].ToString();
			entity.ShortTitle = ConvertHelper.GetString(dr["ShortTitle"]);
			entity.EditorName = ConvertHelper.GetString(dr["EditorName"]);
			entity.ImageLink = ConvertHelper.GetString(dr["ImageLink"]);
			entity.Duration = ConvertHelper.GetInteger(dr["Duration"]);
			entity.ShowPlayUrl = ConvertHelper.GetString(dr["ShowPlayUrl"]);
			entity.Publishtime = ConvertHelper.GetDateTime(dr["Publishtime"]);
			entity.UserId = ConvertHelper.GetInteger(dr["UserId"]);
			entity.MShowPlayUrl = ConvertHelper.GetString(dr["ShowPlayUrl"]);
			entity.Source = ConvertHelper.GetInteger(dr["Source"]);
			return entity;
		}

		/// <summary>
		/// 获取视频数量 根据子品牌ID
		/// </summary>
		/// <param name="serialId">子品牌ID</param>
		/// <returns></returns>
		public static int GetVideoCountBySerialId(int serialId)
		{
			int count = 0;
			try
			{
				count = VideoRepository.GetVideoCountBySerialId(serialId);
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return count;
		}
		/// <summary>
		/// 获取视频分类
		/// </summary>
		/// <returns></returns>
		public static List<VideoCategoryEntity> GetVideoCategory()
		{
			List<VideoCategoryEntity> list = new List<VideoCategoryEntity>();
			try
			{
				string categoryUrl = string.Format(CommonData.CommonSettings.VideoCategoryNewUrl);
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(categoryUrl);
				if (xmlDoc != null)
				{
					//加入命名空间和前缀
					var xmlnsm = new XmlNamespaceManager(xmlDoc.NameTable);
					xmlnsm.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/BitAuto.Video.Model");
					XmlNodeList nodeList = xmlDoc.SelectNodes("//ns:Category", xmlnsm);
					foreach (XmlNode node in nodeList)
					{
						list.Add(new VideoCategoryEntity()
						{
							CategoryId = ConvertHelper.GetInteger(node.SelectSingleNode("./ns:CategoryId", xmlnsm).InnerText),
							CategoryName = node.SelectSingleNode("./ns:CategoryName", xmlnsm).InnerText,
							ParentId = ConvertHelper.GetInteger(node.SelectSingleNode("./ns:ParentId", xmlnsm).InnerText)
						});
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}

		/// <summary>
		/// 所有有视频的子品牌ID
		/// </summary>
		/// <returns></returns>
		public static List<int> GetAllHasVideoSerialID()
		{
			List<int> list = new List<int>();
			try
			{
				string url = string.Format(CommonData.CommonSettings.VideoHasSerialIDUrl);
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(url);
				if (xmlDoc != null)
				{
					XmlNodeList xnl = xmlDoc.SelectNodes("/SerialIds/Id");
					foreach (XmlNode xn in xnl)
					{
						int csid = 0;
						if (int.TryParse(xn.InnerText, out csid))
						{
							if (csid > 0 && !list.Contains(csid))
							{ list.Add(csid); }
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}

		/// <summary>
		/// 获取视频关联车型
		/// </summary>
		/// <param name="videoId"></param>
		/// <returns></returns>
		public static List<VideoToSerialEntity> GetRelationCar(string videoGuid)
		{
			List<VideoToSerialEntity> vsList = new List<VideoToSerialEntity>();
			try
			{
				string videoUrl = string.Format(CommonData.CommonSettings.VideoNewUrl, videoGuid);
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(videoUrl);
				if (xmlDoc != null)
				{
					//加入命名空间和前缀
					var xmlnsm = new XmlNamespaceManager(xmlDoc.NameTable);
					xmlnsm.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/BitAuto.Video.RESTfulApi.ShowModel");

					int videoId = ConvertHelper.GetInteger(xmlDoc.SelectSingleNode("/ns:VideoDetail/ns:VideoId", xmlnsm).InnerText);

					XmlNodeList nodeList = xmlDoc.SelectNodes("//ns:CarModelRelation", xmlnsm);
					foreach (XmlNode node in nodeList)
					{
						vsList.Add(new VideoToSerialEntity()
						{
							VideoId = videoId,
							BrandId = ConvertHelper.GetInteger(node.SelectSingleNode("./ns:BrandId", xmlnsm).InnerText),
							SerialId = ConvertHelper.GetInteger(node.SelectSingleNode("./ns:SerialId", xmlnsm).InnerText)
						});
					}
				}
				var result = vsList.GroupBy(p => p.SerialId)
						.Select(p => p.ToList<VideoToSerialEntity>().FirstOrDefault())
						.ToList()
						.FindAll(p => p.SerialId > 0);
				return result;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return vsList;
		}
		/// <summary>
		/// 从数据库中获取关联车型
		/// </summary>
		/// <param name="videoId">视频ID</param>
		/// <returns></returns>
		public static List<int> GetRelationCarByData(string videoGuid)
		{
			List<int> list = new List<int>();
			try
			{
				DataSet ds = VideoRepository.GetRelationCar(videoGuid);
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					list.Add(ConvertHelper.GetInteger(dr["serialid"]));
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			return list;
		}
		/// <summary>
		/// 根据视频分类 获取车型对应视频分类
		/// </summary>
		/// <param name="categroyId">视频分类ID</param>
		/// <returns>车型本地对应视频分类ID</returns>
		public static List<int> GetCarVideoCategoryId(int categroyId)
		{
			List<int> resultList = new List<int>();
			List<VideoCategoryEntity> categoryList = CommonData.VideoCategoryList;
			if (categoryList.Count > 0)
			{
				List<int> parentIdList = new List<int>();
				//获取该分类所属所有父分类
				GetParentIds(categroyId, parentIdList);
				//foreach (string enumName in System.Enum.GetNames(typeof(VideoEnum.CategoryTypeEnum)))
				foreach (string enumName in CommonData.VideoCategoryConfig.ConfigList.Keys)
				{
					//int categoryType = 0;
					//if (System.Enum.IsDefined(typeof(VideoEnum.CategoryTypeEnum), enumName))
					//{
					//    VideoEnum.CategoryTypeEnum categroyTypeEnum = (VideoEnum.CategoryTypeEnum)System.Enum.Parse(typeof(VideoEnum.CategoryTypeEnum), enumName);
					//    categoryType = (int)categroyTypeEnum;
					//}
					int categoryType = CommonData.VideoCategoryConfig.ConfigList[enumName].CategoryType;
					if (categoryType <= 0) continue;
					//List<int> allSubCategoryList = new List<int>();
					string categoryIds = CommonData.VideoCategoryConfig.ConfigList[enumName].CategoryIds;
					var categoryIdsList = categoryIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.ToList()
						.Select(p => ConvertHelper.GetInteger(p));
					if (categoryIdsList.Contains(categroyId))
					{
						resultList.Add(categoryType);
					}
					else
					{
						/*
						 foreach (string focusCategoryId in categoryArray)
						{
							int cid = ConvertHelper.GetInteger(focusCategoryId);
							var focusAllList = categoryList.FindAll(p => p.CategoryId == cid)
								.Select(p => p.CategoryId);
							allSubCategoryList.AddRange(focusAllList);
							if (categoryList.Find(p => p.ParentId == cid) != null)
							{
								GetSubCategoryId(cid, ref allSubCategoryList);
							}
						}
						if (allSubCategoryList.Distinct().Contains(categroyId))
						{
							resultList.Add(categoryType);
						}
						 */
						var both = categoryIdsList.Intersect(parentIdList);
						if (both.Count() > 0)
						{
							resultList.Add(categoryType);
						}
						//foreach (string focusCategoryId in categoryArray)
						//{
						//    int cid = ConvertHelper.GetInteger(focusCategoryId);
						//    if (parentIdList.Find(p => p == cid) != null)
						//    {
						//        resultList.Add(categoryType);
						//        break;
						//    }
						//}
					}
				}
			}
			return resultList;
		}
		/// <summary>
		/// 递归获取 分类下所有子分类
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		public static void GetSubCategoryId(int parentId, ref List<int> resultList)
		{
			List<VideoCategoryEntity> categoryList = CommonData.VideoCategoryList;
			if (categoryList.Count > 0)
			{
				var subCategoryIdList = categoryList.FindAll(p => p.ParentId == parentId)
					.Select(p => p.CategoryId);
				resultList.AddRange(subCategoryIdList);
				foreach (int cid in subCategoryIdList)
				{
					if (categoryList.Find(p => p.ParentId == cid) != null)
					{
						//递归
						GetSubCategoryId(cid, ref resultList);
					}
				}
			}
		}
		/// <summary>
		/// 获取当前分类所有父分类
		/// </summary>
		/// <param name="categoryId"></param>
		/// <param name="resultList"></param>
		public static void GetParentIds(int categoryId, List<int> parentIdList)
		{
			List<VideoCategoryEntity> categoryList = CommonData.VideoCategoryList;
			if (categoryList.Count > 0)
			{
				var currentCategory = categoryList.Find(p => p.CategoryId == categoryId);
				if (currentCategory == null) return;
				parentIdList.Add(categoryId);
				var parentId = currentCategory.ParentId;
				if (parentId > 0)
				{
					GetParentIds(parentId, parentIdList);
				}
			}
		}
	}
}
