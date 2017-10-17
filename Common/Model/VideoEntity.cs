using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
	public class VideoEntity
	{
		public System.Guid VideoGuId { get; set; }
		public System.Int64 VideoId { get; set; }
		//public List<VideoToSerialEntity> SerialIds { get; set; }
		public int BrandId { get; set; }
		public int SerialId { get; set; }
		public int CategoryId { get; set; }
		public string Title { get; set; }
		public string ShortTitle { get; set; }
		public string EditorName { get; set; }
		public string ImageLink { get; set; }
		public int Duration { get; set; }
		public string ShowPlayUrl { get; set; }
		public DateTime Publishtime { get; set; }

	}

	public class VideoToSerialEntity
	{
		public System.Int64 VideoId { get; set; }
		public int BrandId { get; set; }
		public int SerialId { get; set; }
		public int CategoryType { get; set; }
	}

	/// <summary>
	/// 视频消息实体
	/// </summary>
	public class VideoEntityV2
	{
		/// <summary>
		/// 自增id
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// GUID
		/// </summary>
		public System.Guid VideoGuId { get; set; }
		/// <summary>
		/// 视频id
		/// </summary>
		public System.Int64 VideoId { get; set; }
		//public List<VideoToSerialEntity> SerialIds { get; set; }
		/// <summary>
		/// 关联的车系id
		/// </summary>
		public string SerialIds { get; set; }
		/// <summary>
		/// 视频分类
		/// </summary>
		public int CategoryId { get; set; }
		/// <summary>
		/// 视频标题
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// 视频短标题
		/// </summary>
		public string ShortTitle { get; set; }
		/// <summary>
		/// 标记名称
		/// </summary>
		public string EditorName { get; set; }
		/// <summary>
		/// 封面图地址
		/// </summary>
		public string ImageLink { get; set; }
		/// <summary>
		/// 时长
		/// </summary>
		public int Duration { get; set; }
		/// <summary>
		/// PC播放地址
		/// </summary>
		public string ShowPlayUrl { get; set; }
		/// <summary>
		/// M播放地址
		/// </summary>
		public string MShowPlayUrl { get; set; }
		/// <summary>
		/// 发布时间
		/// </summary>
		public DateTime Publishtime { get; set; }
		/// <summary>
		/// 用户id
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// 视频来源
		/// </summary>
		public int Source { get; set; }
	}


	/// <summary>
	/// 视频车系关联实体
	/// </summary>
	public class VideoToSerialEntityV2
	{
		/// <summary>
		/// 车型视频自增id
		/// </summary>
		public int CarVideoId { get; set; }
		/// <summary>
		/// 关联车系
		/// </summary>
		public int SerialId { get; set; }
	}
}
