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
	public class HuimaicheDAL
	{
		public static bool Update(XElement bodyElement, string opType)
		{
			try
			{
				string guid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "EntityId" });
				string csId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "CsId" });
				string carId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "CarId" });
				string cityId = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "CityId" });
				string price = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "Price" });
				string shortRemarks = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "ShortRemarks" });
				string remarks = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "Remarks" });

				string url = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "Url" });
				string mUrl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CarInfo", "MUrl" });

				if (opType != "delete")
				{
					if (ConvertHelper.GetDecimal(price) <= 0)
					{
						Log.WriteErrorLog("惠买车车款价格 <=0,guid=" + guid);
						return false;
					}
				}


				Guid g = Guid.Empty;
				Guid.TryParse(guid, out g);
				var entity = new BuyCarServiceEntity()
				{
					Guid = g,
					CarId = ConvertHelper.GetInteger(carId),
					CityId = ConvertHelper.GetInteger(cityId),
					CsId = ConvertHelper.GetInteger(csId),
					Price = ConvertHelper.GetDecimal(price),
					ShortRemarks = shortRemarks,
					Remarks = remarks,
					Url = url,
					MUrl = mUrl,
				};

				return BuyCarServiceDAL.Update(entity, opType, Define.ProductType.Hui);



				//SqlParameter[] _params = { 
				//					 new SqlParameter("@Guid",SqlDbType.UniqueIdentifier),
				//					 new SqlParameter("@CarId",SqlDbType.Int),
				//					 new SqlParameter("@CityId",SqlDbType.Int),
				//					 new SqlParameter("@CsId",SqlDbType.Int),
				//					 new SqlParameter("@Price",SqlDbType.Decimal),
				//					 new SqlParameter("@ShortRemarks",SqlDbType.NVarChar),
				//					 new SqlParameter("@Remarks",SqlDbType.NVarChar),
				//					 new SqlParameter("@Url",SqlDbType.VarChar),
				//					 new SqlParameter("@MUrl",SqlDbType.VarChar),
				//					new SqlParameter("@Status",SqlDbType.SmallInt),
				//					new SqlParameter("@OperateType",SqlDbType.VarChar),
				//					new SqlParameter("@ProductType",SqlDbType.Int)
				//					 };
				//_params[0].Value = Guid.Parse(guid);
				//_params[1].Value = carId;
				//_params[2].Value = cityId;
				//_params[3].Value = csId;
				//_params[4].Value = ConvertHelper.GetDecimal(price);
				//_params[5].Value = shortRemarks;
				//_params[6].Value = remarks;
				//_params[7].Value = url;
				//_params[8].Value = mUrl;
				//_params[9].Value = 1;
				//_params[10].Value = opType;
				//_params[11].Value = (int)Define.ProductType.Hui;
				//bool isSuccess = (SqlHelper.ExecuteNonQuery(
				//	Common.CommonData.ConnectionStringSettings.BuyCarServiceConnectionString,
				//	CommandType.StoredProcedure, @"SP_Buy_SerialResult_Update", _params) > 0);
				//return isSuccess;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog((bodyElement != null ? bodyElement.ToString() : string.Empty) + ex.ToString());
				return false;
			}
		}
	}
}
