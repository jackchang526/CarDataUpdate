using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.HtmlBuilder;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Services;
using System.Xml.Linq;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Repository;
using System.IO;
using System.Xml;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
	public class VideoSyncService
	{
		List<int> masterList = new List<int>();
		List<int> brandList = new List<int>();
		List<int> serialList = new List<int>();

		/// <summary>
		/// 添加视频
		/// </summary>
		/// <param name="bodyElement"></param>
		public void Add(XElement bodyElement)
		{
			CommonFunction.InsertMessageDbLog(bodyElement, "Video", "CMS", false);
			this.Update(bodyElement);
		}

		/// <summary>
		/// 添加更新视频数据
		/// </summary>
		/// <param name="bodyElement"></param>
		public void Update(XElement bodyElement)
		{
			UpdateDB(bodyElement, 0);
		}

		/// <summary>
		/// 更新社区视频
		/// </summary>
		public void UpdateBaa(XElement bodyElement)
		{
			List<int> userList = GetVipUserList();
			int userId = ConvertHelper.GetInteger(bodyElement.Element("UserId").Value);
			if (userList == null || !userList.Contains(userId))
			{
				Log.WriteLog("用户id不在VIP列表。UserId=" + userId);
				return;
			}
			int status = ConvertHelper.GetInteger(bodyElement.Element("Status").Value);
			if (status == 1) //新发布
			{
				UpdateDB(bodyElement, 1);
			}
			else if(status == 0)//删除
			{
				Delete(bodyElement);
			}
		}

		/// <summary>
		/// 更新数据库
		/// </summary>
		/// <param name="bodyElement">消息体</param>
		/// <param name="source">来源，0：视频库，1：社区</param>
		private void UpdateDB(XElement bodyElement,int source)
		{
			try
			{
				CommonFunction.InsertMessageDbLog(bodyElement, "Video", "CMS", false);

				CommonData.InitSerialDataDic();
				Guid guid = Guid.Empty;
				string entityId = bodyElement.Element("EntityId").Value;
				if (!string.IsNullOrEmpty(entityId) && Guid.TryParse(entityId, out guid))
				{
					Log.WriteLog("更新视频消息 start。entityId=" + entityId);

					var oldRelationList = VideoService.GetRelationCarByDataV2(entityId);//获取老的关联车系

					List<int> categoryList = new List<int>() { 0 };//不区分分类 所有视频默认分类 0
					VideoEntityV2 videoEntity = new VideoEntityV2();
					videoEntity.VideoGuId = new Guid(entityId);
					videoEntity.VideoId = ConvertHelper.GetLong(bodyElement.Element("BusinessId").Value);
					//videoEntity.SerialIds = vsList;
					videoEntity.CategoryId = ConvertHelper.GetInteger(bodyElement.Element("CategoryId").Value);
					videoEntity.Title = bodyElement.Element("Title").Value;
					videoEntity.ShortTitle = bodyElement.Element("ShortTitle").Value;
					videoEntity.EditorName = string.Empty;
					videoEntity.ImageLink = bodyElement.Element("CoverImageUrl").Value;
					videoEntity.Duration = ConvertHelper.GetInteger(bodyElement.Element("Duration").Value);
					videoEntity.ShowPlayUrl = bodyElement.Element("Url").Value;
					videoEntity.MShowPlayUrl = bodyElement.Element("MUrl").Value;
					videoEntity.Publishtime = ConvertHelper.GetDateTime(bodyElement.Element("PublishTime").Value);
					videoEntity.SerialIds = bodyElement.Element("CsIds").Value;
					videoEntity.Source = source;
					videoEntity.UserId = ConvertHelper.GetInteger(bodyElement.Element("UserId").Value);
					if (string.IsNullOrWhiteSpace(videoEntity.SerialIds))
					{
						//Log.WriteLog("更新视频消息，没有关联子品牌。entityId=" + entityId);
						Delete(bodyElement);//如果没有关联车系，删除视频
						return;
					}
					int carVideoId = VideoRepository.UpdateVideoV2(videoEntity, categoryList);
					if (carVideoId == 0)
					{
						Log.WriteLog("更新视频消息失败，entityId=" + entityId);
					}
					Log.WriteLog("更新视频消息 end");

					serialList = VideoService.GetRelationCarByDataV2(entityId);
					
					serialList = serialList.Union(oldRelationList).ToList();//新关联车系和老的关联车系求并集，并更新相关的品牌、主品牌的视频信息
					brandList = CommonData.SerialBrandDic
						.Where(p => serialList.Contains(p.Key))
						.Select(p => p.Value)
						.Distinct()
						.ToList();
					//brandList = relationList.Select(p => p.BrandId).Distinct().ToList();
					masterList = CommonData.BrandMasterBrandDic
						.Where(p => brandList.Contains(p.Key))
						.Select(p => p.Value)
						.Distinct()
						.ToList();
					//更新视频相关块数据
					UpdateVideoBlock();
				}
				else { Log.WriteLog("更新视频消息. video=" + entityId); }
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog((bodyElement != null ? bodyElement.ToString() : string.Empty) + ex.ToString());
			}
		}

		/// <summary>
		/// 删除视频数据
		/// </summary>
		/// <param name="bodyElement"></param>
		public void Delete(XElement bodyElement)
		{
			try
			{
				CommonFunction.InsertMessageDbLog(bodyElement, "Video", "CMS", true);

				CommonData.InitSerialDataDic();

				Guid guid = Guid.Empty;
				string entityId = bodyElement.Element("EntityId").Value;
				if (string.IsNullOrEmpty(entityId) && !Guid.TryParse(entityId, out guid))
				{
					Log.WriteLog("删除视频guid异常. videoGuid=" + entityId);
					return;
				}
				serialList = VideoService.GetRelationCarByDataV2(entityId);

				Log.WriteLog("删除视频消息 start。videoGuid=" + entityId);
				VideoService.DeleteVideoV2(entityId);
				Log.WriteLog("删除视频消息 end");

				if (serialList.Count > 0)
				{
					brandList = CommonData.SerialBrandDic
						.Where(p => serialList.Contains(p.Key))
						.Select(p => p.Value)
						.Distinct()
						.ToList();
					masterList = CommonData.BrandMasterBrandDic
						.Where(p => brandList.Contains(p.Key))
						.Select(p => p.Value)
						.Distinct()
						.ToList();
					//更新视频相关块数据
					UpdateVideoBlock();
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}
		/// <summary>
		/// 更新视频块
		/// </summary>
		public void UpdateVideoBlock()
		{
			try
			{
				//主品牌块
				MasterVideoHtmlBuilder masterBrand = new MasterVideoHtmlBuilder();
				foreach (int masterId in masterList)
				{
					Log.WriteLog("更新主品牌视频块 start masterId=" + masterId);
					masterBrand.BuilderDataOrHtml(masterId);
					Log.WriteLog("更新主品牌视频块 end masterId=" + masterId);
				}
				//品牌块
				BrandVideoHtmlBuilder brand = new BrandVideoHtmlBuilder();
				foreach (int brandId in brandList)
				{
					Log.WriteLog("更新品牌视频块 start brandId=" + brandId);
					brand.BuilderDataOrHtml(brandId);
					Log.WriteLog("更新品牌视频块 end brandId=" + brandId);
				}
				//初始化子品牌数据
				CommonData.GetSerialData();
				CommonData.InitNewsCategoryDic();
				//add by sk 2015.11.09 for songcl focusnew H5
				CommonData.GetRainbowData();
				//子品牌块
				SerialVideoHtmlNewBuilder serial = new SerialVideoHtmlNewBuilder();
				//焦点区视频
				FocusNewsHtmlBuilder focus = new FocusNewsHtmlBuilder();
				foreach (int serialId in serialList)
				{
					Log.WriteLog("更新子品牌品牌视频块及焦点区 start serialId=" + serialId);
					serial.BuilderDataOrHtml(serialId);
					focus.BuilderDataOrHtml(serialId);
					Log.WriteLog("更新子品牌品牌视频块及焦点区 end serialId=" + serialId);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

		/// <summary>
		/// 获取社区Vip用户id列表
		/// </summary>
		/// <returns></returns>
		private List<int> GetVipUserList()
		{
			List<int> list = null;
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\VideoVip.xml");
			try
			{
				if (!File.Exists(filePath))
				{
					return list;
				}
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(filePath);
				XmlNodeList nodeList = xmlDoc.SelectNodes("/root/user");
				list = new List<int>();
				foreach (XmlNode node in nodeList)
				{
					list.Add(ConvertHelper.GetInteger(node.Attributes["id"].Value));
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(string.Format("WebService读取社区Vip用户文件异常，异常信息:", ex.ToString()));
			}
			return list;
		}
	}
}
