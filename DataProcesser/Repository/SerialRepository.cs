using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.DataProcesser.Repository
{
	public class SerialRepository
	{
		/// <summary>
		/// 获取车型信息 根据子品牌ID
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		public static DataSet GetAllCarInfoForSerialSummary(int serialId)
		{
			string sql = @"select car.car_id,car.car_name,car.car_ReferPrice,car.Car_YearType,car.Car_ProduceState,car.Car_SaleState,cs.cs_id,cei.Engine_Exhaust,cei.UnderPan_TransmissionType,ccp.Pv_SumNum 
from dbo.Car_Basic car 
left join dbo.Car_Extend_Item cei on car.car_id = cei.car_id 
left join Car_serial cs on car.cs_id = cs.cs_id 
left join (select Pv_SumNum,car_id from Chart_Car_Pv where CreateDateStr >=@Date1 and CreateDateStr < @Date2) ccp on car.Car_Id = ccp.car_id where car.isState=1 and cs.isState=1 AND cs.cs_Id=@serialId ";
            SqlParameter[] _params = { 
                                         new SqlParameter("@serialId", SqlDbType.Int),
                                         new SqlParameter("@Date1",SqlDbType.DateTime),
										 new SqlParameter("@Date2",SqlDbType.DateTime)
                                     };
            _params[0].Value = serialId;
            _params[1].Value = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            _params[2].Value = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

			return BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, System.Data.CommandType.Text, sql, _params);
		}
		/// <summary>
		/// 取所有子品牌颜色RGB值
		/// </summary>
		/// <returns></returns>
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
