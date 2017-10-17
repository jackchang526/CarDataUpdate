using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.HtmlBuilder;

namespace BitAuto.CarDataUpdate.CarMessageProcesser.CMS
{
	public class Video : BaseProcesser
	{
		List<int> masterList = new List<int>();
		List<int> brandList = new List<int>();
		List<int> serialList = new List<int>();

		public override void Processer(ContentMessage msg)
		{
			#region 废除旧视频消息处理 modified by sk 2013-10-16
			/*
			 try
			{
				int videoId = ConvertHelper.GetInteger(msg.ContentId);
				if (videoId > 0)
				{
					string deleteOp = CommonFunction.GetXmlElementInnerText(msg.ContentBody, "MessageBody/DeleteOp", string.Empty);

					if (!string.IsNullOrEmpty(deleteOp) && deleteOp == "true")
					{
						serialList = VideoService.GetRelationCarByData(videoId);

						Log.WriteLog("删除视频消息 start。videoId=" + videoId);
						VideoService.DeleteVideo(videoId);
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
					else
					{
						serialList = VideoService.GetRelationCarByData(videoId);

						Log.WriteLog("更新视频消息 start。videoId=" + videoId);
						VideoService.UpdateVideo(videoId);
						Log.WriteLog("更新视频消息 end");

						//获取视频关联车型
						List<VideoToSerialEntity> relationList = VideoService.GetRelationCar(videoId);
						if (relationList.Count > 0)
						{
							var tempSerialList = relationList.Select(p => p.SerialId).ToList();
							serialList = serialList.Union(tempSerialList).ToList();
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
					}
				}
				else { Log.WriteLog("更新视频消息. video=0"); }
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
			 */
			#endregion
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
	}
}
