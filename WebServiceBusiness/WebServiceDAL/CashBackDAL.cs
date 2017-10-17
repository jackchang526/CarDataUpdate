using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.WebServiceModel;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using System.Data;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
	public class CashBackDAL
	{
		public static bool UpdateCashBack(XElement bodyElement, string opType)
		{

			string guid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "EntityId" });
			string csid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CashBackInfo", "Cs_Id" });
			string carid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CashBackInfo", "Car_Id" });
			string cityid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CashBackInfo", "City_Id" });
			string BackPrice = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CashBackInfo", "BackPrice" });
			string url = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "CashBackInfo", "Url" });

			SqlParameter[] sqlParams = new SqlParameter[]
            {
				new SqlParameter("Guid", SqlDbType.UniqueIdentifier)
					{Value=Guid.Parse(guid)},
				new SqlParameter("SerialId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(csid)?0:int.Parse(csid)},
				new SqlParameter("CarId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(carid)?0:int.Parse(carid)},
				new SqlParameter("CityId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(cityid)?0:int.Parse(cityid)},
				new SqlParameter("BackPrice", SqlDbType.Decimal)
					{Value=string.IsNullOrEmpty(BackPrice)?0:decimal.Parse(BackPrice)},
				new SqlParameter("Url", SqlDbType.VarChar)
					{Value=string.IsNullOrEmpty(url)?"":url},
				new SqlParameter("OperateType", SqlDbType.VarChar,100)
					{Value=opType},
			};

			bool isSuccess = (SqlHelper.ExecuteNonQuery(
				Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, @"SP_CashBack_Car_Update", sqlParams) > 0);
			return isSuccess;
		}

	}
}
