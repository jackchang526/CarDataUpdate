using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.Utils;
using System.Data.SqlClient;
using System.Data;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class CarInfoForSelecting
	{
		/// <summary>
		/// 更新选车工具表数据
		/// </summary>
		/// <param name="carId">车型ID</param>
		public static void UpdateCarDataByCarId(int carId)
		{
			SqlParameter[] param = { new SqlParameter("@carId", SqlDbType.Int) };
			param[0].Value = carId;
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.StoredProcedure, "SP_UpdateSelectCarDataByCarId", param);
			//更新车型报价
			Dictionary<int, Dictionary<string, decimal>> dict = CommonData.dictCarPriceData;
			if (dict.ContainsKey(carId))
			{
				SqlParameter[] paramPrice = { 
											new SqlParameter("@MinPrice", SqlDbType.Decimal),
											new SqlParameter("@MaxPrice", SqlDbType.Decimal),
											new SqlParameter("@carid", SqlDbType.Int)
										};
				paramPrice[0].Value = dict[carId]["MinPrice"];
				paramPrice[1].Value = dict[carId]["MaxPrice"];
				paramPrice[2].Value = carId;
				string sql = "UPDATE CarInfoForSelecting SET MinPrice=@MinPrice,MaxPrice=@MaxPrice WHERE carid=@carid";
				SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, paramPrice);
			}
		}
		/// <summary>
		/// 更新选车工具表数据
		/// </summary>
		/// <param name="csId">子品牌ID</param>
		public static void UpdateCarDataByCsId(int csId)
		{
			SqlParameter[] param = { new SqlParameter("@csId", SqlDbType.Int) };
			param[0].Value = csId;
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.StoredProcedure, "SP_UpdateSelectCarDataByCsId", param);
		}
        /// <summary>
        /// 更新高级选车工具数据
        /// </summary>
        /// <param name="carId">车型ID</param>
        public static void UpdateCarDataByCarIdV2(int carId)
        {
            SqlParameter[] param = { new SqlParameter("@carId", SqlDbType.Int) };
            param[0].Value = carId;
            SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.StoredProcedure, "SP_UpdateSelectCarDataByCarIdV2", param);
            //更新车型报价
            Dictionary<int, Dictionary<string, decimal>> dict = CommonData.dictCarPriceData;
            if (dict.ContainsKey(carId))
            {
                SqlParameter[] paramPrice = { 
											new SqlParameter("@MinPrice", SqlDbType.Decimal),
											new SqlParameter("@MaxPrice", SqlDbType.Decimal),
											new SqlParameter("@carid", SqlDbType.Int)
										};
                paramPrice[0].Value = dict[carId]["MinPrice"];
                paramPrice[1].Value = dict[carId]["MaxPrice"];
                paramPrice[2].Value = carId;
                string sql = "UPDATE CarInfoForSelectingV2 SET MinPrice=@MinPrice,MaxPrice=@MaxPrice WHERE carid=@carid";
                SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, paramPrice);
            }
        }
        /// <summary>
        /// 更新高级选车工具表数据
        /// </summary>
        /// <param name="csId">子品牌ID</param>
        public static void UpdateCarDataByCsIdV2(int csId)
        {
            SqlParameter[] param = { new SqlParameter("@csId", SqlDbType.Int) };
            param[0].Value = csId;
            SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.StoredProcedure, "SP_UpdateSelectCarDataByCsIdV2", param);
        }
		/// <summary>
		/// 更新购车服务选车表数据
		/// </summary>
		/// <param name="csId"></param>
		public static void UpdateBuyCarServiceSelectCar(int csId, int carId)
		{
			SqlParameter[] param = { new SqlParameter("@CsId", SqlDbType.Int),
								   new SqlParameter("@CarId", SqlDbType.Int)};
			param[0].Value = csId;
			param[1].Value = carId;
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.BuyCarServiceConnectionString, CommandType.StoredProcedure, "SP_Buy_SelectCar_Update", param);
		}
	}
}
