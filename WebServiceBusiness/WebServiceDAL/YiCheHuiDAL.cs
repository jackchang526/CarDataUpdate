using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.WebServiceModel;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
	/// <summary>
	/// 惠买车 for 黄超强 张涛
	/// </summary>
	public class YiCheHuiDAL
	{
		public static bool Update(XElement bodyElement, string opType)
		{
			try
			{
				string guid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "EntityId" });
				string csId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "CsID" });
				string carId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "CarID" });
				string activityID = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "ActivityID" });
				string cityId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "CityID" });
				string price = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "Price" });
				string shortRemarks = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "slogan" });
				//add by sk 2016-06-03
				string url = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "Url" });
				string mUrl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "YiCheHui", "MUrl" });
				if (opType != "delete")
				{
					if (ConvertHelper.GetDecimal(price) <= 0)
					{
						Log.WriteErrorLog("惠买车 (惠买车 for 黄超强 张涛) 车型price <=0,guid=" + guid);
						return false;
					}
				}

				SqlParameter[] _params = { 
									 new SqlParameter("@Guid",SqlDbType.UniqueIdentifier),
									 new SqlParameter("@CsId",SqlDbType.Int),
									 new SqlParameter("@CarId",SqlDbType.Int),
									 new SqlParameter("@ActivityID",SqlDbType.Int),
									 new SqlParameter("@CityId",SqlDbType.Int),
									 new SqlParameter("@Price",SqlDbType.Decimal),
									 new SqlParameter("@Url",SqlDbType.VarChar),
									 new SqlParameter("@MUrl",SqlDbType.VarChar),
									new SqlParameter("@OperateType",SqlDbType.VarChar)
									 };
				_params[0].Value = Guid.Parse(guid);
				_params[1].Value = ConvertHelper.GetInteger(csId);
				_params[2].Value = ConvertHelper.GetInteger(carId);
				_params[3].Value = ConvertHelper.GetInteger(activityID);
				_params[4].Value = ConvertHelper.GetInteger(cityId);
				_params[5].Value = ConvertHelper.GetDecimal(price);
				_params[6].Value = url;
				_params[7].Value = mUrl;
				_params[8].Value = opType;
				bool isSuccess = (SqlHelper.ExecuteNonQuery(
					Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.StoredProcedure, @"[SP_Car_YiCheHui_UpdateV2]", _params) > 0);

				Guid g = Guid.Empty;
				Guid.TryParse(guid, out g);
				var entity = new BuyCarServiceEntity()
				{
					Guid = g,
					CarId = ConvertHelper.GetInteger(carId),
					CityId = ConvertHelper.GetInteger(cityId),
					CsId = ConvertHelper.GetInteger(csId),
					ShortRemarks = shortRemarks,
					Price = ConvertHelper.GetDecimal(price),	//Math.Round((ConvertHelper.GetDecimal(price) / 10000), 2),
					Url = !string.IsNullOrEmpty(url) ? url : string.Format("http://mai.bitauto.com/detail-{0}-{1}.html?cityid={2}", activityID, carId, cityId),
					MUrl = !string.IsNullOrEmpty(mUrl) ? mUrl : string.Format("http://mai.m.yiche.com/detail-{0}-{1}.html?cityid={2}", activityID, carId, cityId),
				};
				BuyCarServiceDAL.Update(entity, opType, Define.ProductType.YiCheHui);

				return isSuccess;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog((bodyElement != null ? bodyElement.ToString() : string.Empty) + ex.ToString());
				return false;
			}
		}
	}
}
