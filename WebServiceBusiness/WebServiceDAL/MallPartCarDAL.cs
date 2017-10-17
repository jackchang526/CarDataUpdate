using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.WebServiceModel;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
	public class MallPartCarDAL
	{
		public static bool Update(XElement bodyElement, string opType)
		{
			try
			{
				string guid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "EntityId" });
				string csId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "CsId" });
				string carId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "CarId" });
				string cityId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "CityId" });
				string price = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "Price" });
				string imageUrl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "ImageUrl" });
				string displayName = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "DisplayName" });
				string url = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "Url" });
				string mUrl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "MUrl" });
				string carType = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "MallCarInfo", "Type" });
				if (opType != "delete")
				{
					if (ConvertHelper.GetDecimal(price) <= 0)
					{
						Log.WriteErrorLog("易车商城包销 平行进口车型price <=0,guid=" + guid);
						return false;
					}
				}
				url = string.IsNullOrEmpty(url) ? "http://www.yichemall.com/car/detail/c_" + carId + "" : url;
				mUrl = string.IsNullOrEmpty(mUrl) ? "http://m.yichemall.com/car/Detail/Index?carId=" + carId + "" : mUrl;

				SqlParameter[] _params = { 
									 new SqlParameter("@Guid",SqlDbType.UniqueIdentifier),
									 new SqlParameter("@CsId",SqlDbType.Int),
									 new SqlParameter("@CarId",SqlDbType.Int),
									 new SqlParameter("@CityId",SqlDbType.Int),
									 new SqlParameter("@CarType",SqlDbType.TinyInt),
									 new SqlParameter("@Price",SqlDbType.Decimal),
									 new SqlParameter("@Url",SqlDbType.VarChar),
									 new SqlParameter("@MUrl",SqlDbType.VarChar),
									 new SqlParameter("@ImageUrl",SqlDbType.VarChar),
									 new SqlParameter("@DisplayName",SqlDbType.NVarChar),
									 new SqlParameter("@OperateType",SqlDbType.VarChar)
									 };
				_params[0].Value = Guid.Parse(guid);
				_params[1].Value = ConvertHelper.GetInteger(csId);
				_params[2].Value = ConvertHelper.GetInteger(carId);
				_params[3].Value = ConvertHelper.GetInteger(cityId);
				_params[4].Value = ConvertHelper.GetInteger(carType);
				_params[5].Value = ConvertHelper.GetDecimal(price);
				_params[6].Value = url;
				_params[7].Value = mUrl;
				_params[8].Value = imageUrl;
				_params[9].Value = displayName;
				_params[10].Value = opType;
				bool isSuccess = (SqlHelper.ExecuteNonQuery(
					Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.StoredProcedure, @"SP_Car_MallPartCar_Update", _params) > 0);
				// add by sk 2015.11.24 接入 购车服务数据
				UpdateBuyCarService(opType, guid, csId, carId, cityId, price, url, mUrl, imageUrl, displayName);
				return isSuccess;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog((bodyElement != null ? bodyElement.ToString() : string.Empty) + ex.ToString());
				return false;
			}
		}

		private static void UpdateBuyCarService(string opType, string guid, string csid, string carid, string cityid, string price, string url, string mUrl, string imageUrl, string displayName)
		{
			try
			{
				var priceTen = Math.Round(ConvertHelper.GetDecimal(price), 2);
				if (opType != "delete")
				{
					if (priceTen <= 0)
					{
						Common.Log.WriteErrorLog("商城直销车款价格为<=0 ,guid=" + guid);
						return;
					}
				}
				Guid g = Guid.Empty;
				Guid.TryParse(guid, out g);
				var entity = new BuyCarServiceEntity()
				{
					Guid = g,
					CarId = ConvertHelper.GetInteger(carid),
					CityId = ConvertHelper.GetInteger(cityid),
					CsId = ConvertHelper.GetInteger(csid),
					Price = priceTen,	//Math.Round((ConvertHelper.GetDecimal(price) / 10000), 2),
					ImageUrl = imageUrl,
					DisplayName = displayName,
					Url = url,
					MUrl = mUrl,
				};
				BuyCarServiceDAL.Update(entity, opType, Define.ProductType.Mall);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(string.Format("更新商城购车服务异常：opType:{0},guid:{1},csid:{2},carid:{3},cityid:{4},price:{5}\r\n", opType, guid, csid, carid, cityid, price) + ex.ToString());
			}
		}
	}
}
