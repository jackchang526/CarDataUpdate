using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Common.Services
{
	public class CarPingceInfoService
	{
		public static Dictionary<int, PingCeTagEntity> GetPingceTagsBySerialId(int serialId)
		{
			Dictionary<int, PingCeTagEntity> dicAllTagInfo = IntiPingCeTagInfoNew();
			Dictionary<int, PingCeTagEntity> dict = new Dictionary<int, PingCeTagEntity>();

			DataSet ds = CarPingceInfoRepository.GetDataBySerialId(serialId);
			if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					int tagId = ConvertHelper.GetInteger(dr["tagid"]);
					string url = dr["url"].ToString();
					PingCeTagEntity pingce = new PingCeTagEntity();
					pingce.tagId = tagId;
					pingce.tagName = dicAllTagInfo[tagId].tagName;
					pingce.url = url;
					if (!dict.ContainsKey(tagId))
					{
						dict.Add(tagId, pingce);
					}
				}
			}
			return dict;
		}

		public static Dictionary<int, PingCeTagEntity> IntiPingCeTagInfoNew()
		{
			Dictionary<int, PingCeTagEntity> dic = new Dictionary<int, PingCeTagEntity>();
			// 导语
			PingCeTagEntity pct1 = new PingCeTagEntity();
			pct1.tagName = "导语";
			pct1.tagRegularExpressions = "(导语：|导语:)";
			dic.Add(1, pct1);
			// 外观
			PingCeTagEntity pct2 = new PingCeTagEntity();
			pct2.tagName = "外观";
			pct2.tagRegularExpressions = "(外观：|外观:)";
			dic.Add(2, pct2);
			// 内饰
			PingCeTagEntity pct3 = new PingCeTagEntity();
			pct3.tagName = "内饰";
			pct3.tagRegularExpressions = "(内饰：|内饰:)";
			dic.Add(3, pct3);
			// 空间
			PingCeTagEntity pct4 = new PingCeTagEntity();
			pct4.tagName = "空间";
			pct4.tagRegularExpressions = "(空间：|空间:)";
			dic.Add(4, pct4);
			// 视野
			PingCeTagEntity pct5 = new PingCeTagEntity();
			pct5.tagName = "视野";
			pct5.tagRegularExpressions = "(视野：|视野:)";
			dic.Add(5, pct5);
			// 灯光
			PingCeTagEntity pct6 = new PingCeTagEntity();
			pct6.tagName = "灯光";
			pct6.tagRegularExpressions = "(灯光：|灯光:)";
			dic.Add(6, pct6);
			// 动力
			PingCeTagEntity pct7 = new PingCeTagEntity();
			pct7.tagName = "动力";
			pct7.tagRegularExpressions = "(动力：|动力:)";
			dic.Add(7, pct7);
			// 操控
			PingCeTagEntity pct8 = new PingCeTagEntity();
			pct8.tagName = "操控";
			pct8.tagRegularExpressions = "(操控：|操控:)";
			dic.Add(8, pct8);
			// 舒适性
			PingCeTagEntity pct9 = new PingCeTagEntity();
			pct9.tagName = "舒适性";
			pct9.tagRegularExpressions = "(舒适性：|舒适：|舒适性:|舒适:)";
			dic.Add(9, pct9);
			// 油耗
			PingCeTagEntity pct10 = new PingCeTagEntity();
			pct10.tagName = "油耗";
			pct10.tagRegularExpressions = "(油耗：|油耗:)";
			dic.Add(10, pct10);
			// 配置
			PingCeTagEntity pct11 = new PingCeTagEntity();
			pct11.tagName = "配置";
			pct11.tagRegularExpressions = "(配置与安全：|配置：|配置与安全:|配置:)";
			dic.Add(11, pct11);
			// 总结
			PingCeTagEntity pct12 = new PingCeTagEntity();
			pct12.tagName = "总结";
			pct12.tagRegularExpressions = "(总结：|总结:)";
			dic.Add(12, pct12);
			return dic;
		}
	}
}
