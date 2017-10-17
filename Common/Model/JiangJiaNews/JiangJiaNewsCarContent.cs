using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.Utils;
using System.Data;

namespace BitAuto.CarDataUpdate.Common.Model.JiangJiaNews
{
	/// <summary>
	/// 降价新闻关联车型内容实体
	/// </summary>
	public class JiangJiaNewsCarContent
	{
		public int CarId;
        /// <summary>
        /// 降价幅度
        /// </summary>
		public decimal FavorablePrice;
        /// <summary>
        /// 降价比率
        /// </summary>
        public decimal FavorableRate;

		public static JiangJiaNewsCarContent GetObjectByDataRow(DataRow row)
		{
			JiangJiaNewsCarContent carObj = new JiangJiaNewsCarContent();

			//<Car_Id>16567</Car_Id>
			//<FavorablePrice>5.000</FavorablePrice>
            //<MarginPrice>0.10</MarginPrice>

			carObj.CarId = ConvertHelper.GetInteger(row["Car_Id"].ToString());
			carObj.FavorablePrice = ConvertHelper.GetDecimal(row["FavorablePrice"].ToString());
            carObj.FavorableRate = ConvertHelper.GetDecimal(row["MarginPrice"].ToString());

			return carObj;
		}
	}
}
