using BitAuto.Utils.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Repository
{
	public class SerialRespository
	{
		public static DataSet GetAllSerialColorRGB(int type)
		{
			DataSet ds = new DataSet();
			string sqlStr = " select autoID,cs_id,colorName,colorRGB from dbo.Car_SerialColor {0} order by cs_id,colorRGB";
			if (type >= 0)
			{
				// 有颜色类型条件 0:子品牌车身颜色 1:内饰颜色
				sqlStr = string.Format(sqlStr, " where type=" + type.ToString());
			}
			else
			{
				// 没有条件取全部颜色(子品牌车身颜色,内饰颜色)
				sqlStr = string.Format(sqlStr, "");
			}
			ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr);
			return ds;
		}
	}
}
