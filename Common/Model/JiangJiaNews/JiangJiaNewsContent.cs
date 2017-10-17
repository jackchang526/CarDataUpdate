using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Common.Model.JiangJiaNews
{
	/// <summary>
	/// 降价新闻内容实体
	/// </summary>
	public class JiangJiaNewsContent
	{
		/// <summary>
		/// 标识id
		/// </summary>
		public int Id;
		/// <summary>
		/// 新闻id
		/// </summary>
		public int NewsId;
		/// <summary>
		/// 作者
		/// </summary>
		public string Author;
		/// <summary>
		/// 标题
		/// </summary>
		public string Title;
		/// <summary>
		/// 车型图片
		/// </summary>
		public string CarImage;
		/// <summary>
		/// 新闻地址
		/// </summary>
		public string NewsUrl;
		/// <summary>
		/// 发布时间
		/// </summary>
		public DateTime PublishTime;
		/// <summary>
		/// 主品牌Id
		/// </summary>
		public int MasterBrandId
		{
			get
			{
				if (this.SerialId > 0
					&& CommonData.SerialMasterBrandDic != null
					&& CommonData.SerialMasterBrandDic.Count > 0
					&& CommonData.SerialMasterBrandDic.ContainsKey(this.SerialId))
				{
					return CommonData.SerialMasterBrandDic[this.SerialId];
				}
				else
				{
					return 0;
				}
			}
		}
		/// <summary>
		/// 品牌Id
		/// </summary>
		public int BrandId
		{
			get
			{
				if (this.SerialId > 0
					&& CommonData.SerialBrandDic != null
					&& CommonData.SerialBrandDic.Count > 0
					&& CommonData.SerialBrandDic.ContainsKey(this.SerialId))
				{
					return CommonData.SerialBrandDic[this.SerialId];
				}
				else
				{
					return 0;
				}
			}
		}
		/// <summary>
		/// 子品牌Id
		/// </summary>
		public int SerialId;
		/// <summary>
		/// 状态
		/// </summary>
		public bool IsState;
		/// <summary>
		/// 关联省
		/// </summary>
		public int ProvinceId;
		/// <summary>
		/// 关联城市
		/// </summary>
		public int CityId;
		/// <summary>
		/// 经销商Id
		/// </summary>
		public int VendorId;
		/// <summary>
		/// 经销商名称
		/// </summary>
		public string VendorName;
		/// <summary>
		/// 最高优惠 decimal(8,3)
		/// </summary>
		public decimal MaxFavorablePrice
		{
			get
			{
				if (CarList != null && CarList.Count>0)
				{
					return CarList.Max(item => item.FavorablePrice);
				}
				return decimal.Zero;
			}
		}
        /// <summary>
        /// 最高降幅 decimal(10,2)
        /// </summary>
        public decimal MaxFavorableRate
        {
            get
            {
                if (CarList != null && CarList.Count > 0)
                {
                    return CarList.Max(item => item.FavorableRate);
                }
                return decimal.Zero;
            }
        }
		/// <summary>
		/// 开始时间
		/// </summary>
		public DateTime StartDateTime;
		/// <summary>
		/// 结束时间
		/// </summary>
		public DateTime EndDateTime;
		/// <summary>
		/// 其他信息
		/// </summary>
		public string OtherInfo;
		/// <summary>
		/// 关联车型列表
		/// </summary>
		public List<JiangJiaNewsCarContent> CarList;

		public static JiangJiaNewsContent GetObjectByDataRow(DataRow row)
		{
			JiangJiaNewsContent newObj = new JiangJiaNewsContent();

			newObj.NewsId = ConvertHelper.GetInteger(row["NewsID"].ToString());
			newObj.Title = row["NewsTitle"].ToString();
			newObj.VendorId = ConvertHelper.GetInteger(row["VendorID"].ToString());
			newObj.VendorName = row["vendorName"].ToString();
			newObj.Author = row["Author"].ToString();
			newObj.PublishTime = ConvertHelper.GetDateTime(row["NewsPubTime"].ToString());
			newObj.NewsUrl = row["NewsUrl"].ToString();
			newObj.StartDateTime = ConvertHelper.GetDateTime(row["StartDateTime"]);
			newObj.EndDateTime = ConvertHelper.GetDateTime(row["EndDateTime"]);
			newObj.CarImage = row["CarImage"].ToString();
			newObj.OtherInfo = row["OtherInfo"].ToString();
			newObj.SerialId = ConvertHelper.GetInteger(row["BrandID"].ToString());
			newObj.CityId = ConvertHelper.GetInteger(row["cityID"].ToString());
			newObj.ProvinceId = ConvertHelper.GetInteger(row["provinceID"].ToString());
			newObj.IsState = true;

			return newObj;
		}
	}
}
