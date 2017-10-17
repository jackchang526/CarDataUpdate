using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Enum
{
	public class Define
	{
		/// <summary>
		/// 车型频道头部标签
		/// 表情定义
		/// </summary>
		public enum EnumForHeadTag
		{
			cssummary = 0,     // 子品牌综述
			cscompare = 1,      // 子品牌参数配置
			csphoto = 2,          // 子品牌图片
			csvideo = 3,          // 子品牌视频
			csshichang = 4,     // 子品牌市场
			cskoubei = 5,        // 子品牌口碑
			csdayi = 6,           // 子品牌答疑
			csbbs = 7,            // 子品牌论坛
			csdaogou = 8,       // 子品牌导购
			csshiche = 9,        // 子品牌试车
			csanquan = 10,     // 子品牌安全
			csyouhao = 11,      // 子品牌油耗
			csprice = 12,         // 子品牌经销商报价
			cszone = 13,          // 子品牌区域购车
			carsummary = 14,  // 车型综述
			carcompare = 15,   // 车型参数配置
			carprice = 16,        // 车型经销商报价
			csnew = 17,            // 子品牌新闻
			csselldata = 18,      // 子品牌销售数据
			csxinwen = 19,      // 子品牌新闻
			cshangqing = 20,      // 子品牌行情
			csyongche = 21,      // 子品牌用车
			cspingce = 22,           // 子品牌评测
			csnewsnocrumb = 23,  // CMS不带面包削
			guangzhou2009 = 24, // 2009广州车展
			csselldatamap = 25,      // 销售地图
			csucar = 26,                   // Ucar 子品牌
			cspaihang = 27,              // 子品牌排行榜
			csshijia = 28,                  // 子品牌试驾
			// beijing2010 = 29                  // 北京车展
			carphoto = 30,                  // 车型图片页
			carucar = 31,                    // Ucar 车型
			csmaintenance = 32,           // 保养信息
			guangzhou2010 = 33,          // 2010广州车展
			csgaizhuang = 34,                // 子品牌改装
			csyangche = 35,					// 子品牌养车费用
			cszhihuan = 36,						// 子品牌置换
			carjiangjia = 37,					// 车型降价
			csjiangjia = 38,						// 子品牌降价
			carchedai = 39,						// 车型车贷
			cschedai = 40,						// 子品牌车贷
			cswenzhang = 41					// 子品牌文章
		}

		/// <summary>
		/// 年度十佳车 广告
		/// </summary>
		public struct BestTopCar
		{
			public string Title;	// a标签title
			public string Link; //  a标签link
			public List<int> ListCsList; // 年度子品牌列表
		}

		public enum ProductType
		{
			Hui = 1,
			Mall = 2,
			Mai = 3,
			/// <summary>
			/// 易车惠 modified by chengl 2015-7-7
			/// </summary>
			YiCheHui=4
		}
	}
}
