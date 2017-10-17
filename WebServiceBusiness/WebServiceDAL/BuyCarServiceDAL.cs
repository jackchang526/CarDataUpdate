using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.WebServiceModel;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
	public class BuyCarServiceDAL
	{
		public static bool Update(BuyCarServiceEntity entity, string opType, Define.ProductType productType)
		{
			CarBaseInfoEntity carEntity = GetCarData(entity.CarId);

			if (opType != "delete" && !(carEntity != null && carEntity.ReferPrice > 0))
			{
				Log.WriteErrorLog("车款指导价<=0,CarId=" + entity.CarId);
			}

			SqlParameter[] _params = { 
									 new SqlParameter("@Guid",SqlDbType.UniqueIdentifier),
									 new SqlParameter("@CarId",SqlDbType.Int),
									 new SqlParameter("@CityId",SqlDbType.Int),
									 new SqlParameter("@CsId",SqlDbType.Int),
									 new SqlParameter("@Price",SqlDbType.Decimal),
 									 new SqlParameter("@ShortRemarks",SqlDbType.NVarChar),
									 new SqlParameter("@Remarks",SqlDbType.NVarChar),
									 new SqlParameter("@Url",SqlDbType.VarChar),
									 new SqlParameter("@MUrl",SqlDbType.VarChar),
									 new SqlParameter("@ImageUrl",SqlDbType.VarChar),
									 new SqlParameter("@DisplayName",SqlDbType.NVarChar),
									new SqlParameter("@Status",SqlDbType.SmallInt),
									new SqlParameter("@OperateType",SqlDbType.VarChar),
									new SqlParameter("@ProductType",SqlDbType.Int),
									new SqlParameter("@ReferPrice",SqlDbType.Decimal)
									 };
			_params[0].Value = entity.Guid;
			_params[1].Value = entity.CarId;
			_params[2].Value = entity.CityId;
			_params[3].Value = entity.CsId;
			_params[4].Value = entity.Price;
			_params[5].Value = entity.ShortRemarks;
			_params[6].Value = entity.Remarks;
			_params[7].Value = entity.Url;
			_params[8].Value = entity.MUrl;
			_params[9].Value = entity.ImageUrl;
			_params[10].Value = entity.DisplayName;
			_params[11].Value = 1;
			_params[12].Value = opType;
			_params[13].Value = (int)productType;
			_params[14].Value = (carEntity != null && carEntity.ReferPrice > 0) ? carEntity.ReferPrice : 0;

			bool isSuccess = (SqlHelper.ExecuteNonQuery(
				Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
				CommandType.StoredProcedure, @"SP_Buy_SerialResult_Update", _params) > 0);
			return isSuccess;
		}

		protected static CarBaseInfoEntity GetCarData(int carId)
		{
			CarBaseInfoEntity carEntity = null;
			try
			{
				string cacheKey = string.Format("BuyCarServiceDAL_{0}", carId);
				var cacheObj = HttpContext.Current.Cache.Get(cacheKey);
				if (cacheObj != null)
					return cacheObj as CarBaseInfoEntity;

				carEntity = CarService.GetCarInfoById(carId);
				if (carEntity == null)
					return carEntity;

				HttpContext.Current.Cache.Insert(cacheKey, carEntity, null, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);
				return carEntity;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
				return carEntity;
			}
		}
	}
}
