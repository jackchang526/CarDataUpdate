using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.Utils;
using System.Data;
using BitAuto.CarDataUpdate.Common.Model;

namespace BitAuto.CarDataUpdate.Common.Services
{
	public class CarService
	{
		/// <summary>
		/// 获取车款所有参数
		/// </summary>
		/// <param name="carID">车款id</param>
		/// <returns></returns>
		public static Dictionary<int, string> GetCarAllParamByCarID(int carID)
		{
			return CarRepository.GetCarAllParamByCarID(carID);
		}

		public static CarBaseInfoEntity GetCarInfoById(int carId)
		{
			 

			var entity = new CarBaseInfoEntity();
			DataSet ds = CarRepository.GetCarInfoById(carId);
			if (!(ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0))
				return null;

			DataRow dr = ds.Tables[0].Rows[0];
			var carEntity = new CarBaseInfoEntity
			{
				CarId = carId,
				CarName = ConvertHelper.GetString(dr["Car_Name"]),
				YearType = ConvertHelper.GetInteger(dr["Car_YearType"]),
				ReferPrice = ConvertHelper.GetDecimal(dr["car_ReferPrice"])
			};
			 
			return carEntity;
		}
 	}
}
