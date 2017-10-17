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
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.WebServiceDAL
{
	public class DirectSellDAL
	{
		public static bool UpdateDirectSell(XElement bodyElement, string opType)
		{

			string guid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "EntityId" });
			string csid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "CsId" });
			string carid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "CarId" });
			string cityid = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "CityId" });
			string price = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "Price" });
			string url = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "Url" });
			if (url.Length > 100)
			{ url = url.Substring(0, 100); }
			string csurl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "CsUrl" });
			if (csurl.Length > 100)
			{ csurl = csurl.Substring(0, 100); }
			string financingUrl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "FinancingUrl" });
			if (financingUrl.Length > 200)
			{ financingUrl = financingUrl.Substring(0, 200); }
			string mUrl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "MUrl" });
			if (mUrl.Length > 200)
			{ mUrl = mUrl.Substring(0, 200); }
			string mCsUrl = Common.CommonFunction.GetXElementByNamePath(bodyElement, new string[] { "DirectSellInfo", "MCsUrl" });
			if (mCsUrl.Length > 200)
			{ mCsUrl = mCsUrl.Substring(0, 200); }

			SqlParameter[] sqlParams = new SqlParameter[]
            {
				new SqlParameter("Guid", SqlDbType.UniqueIdentifier)
					{Value=Guid.Parse(guid)},
				new SqlParameter("CsId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(csid)?0:int.Parse(csid)},
				new SqlParameter("CarId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(carid)?0:int.Parse(carid)},
				new SqlParameter("CityId", SqlDbType.Int)
					{Value=string.IsNullOrEmpty(cityid)?0:int.Parse(cityid)},
				new SqlParameter("Price", SqlDbType.Decimal)
					{Value=string.IsNullOrEmpty(price)?0:decimal.Parse(price)},
				new SqlParameter("Url", SqlDbType.VarChar)
					{Value=string.IsNullOrEmpty(url)?"":url},
				new SqlParameter("CsUrl", SqlDbType.VarChar)
					{Value=string.IsNullOrEmpty(csurl)?"":csurl},
				new SqlParameter("FinancingUrl", SqlDbType.VarChar,200)
					{Value=string.IsNullOrEmpty(financingUrl)?"":financingUrl},
				new SqlParameter("MUrl", SqlDbType.VarChar,200)
					{Value=string.IsNullOrEmpty(mUrl)?"":mUrl},
				new SqlParameter("MCsUrl", SqlDbType.VarChar,200)
					{Value=string.IsNullOrEmpty(mCsUrl)?"":mCsUrl},
				new SqlParameter("OperateType", SqlDbType.VarChar,100)
					{Value=opType},
			};

			bool isSuccess = (SqlHelper.ExecuteNonQuery(
				Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString,
				CommandType.StoredProcedure, @"SP_Car_DirectSell_Update", sqlParams) > 0);
			////同步到购车服务中
			//UpdateBuyCarService(opType, guid, csid, carid, cityid, price, url, mUrl);

			return isSuccess;
		}

		private static void UpdateBuyCarService(string opType, string guid, string csid, string carid, string cityid, string price, string url, string mUrl)
		{
			try
			{
				var priceTen = Math.Round((ConvertHelper.GetDecimal(price) / 10000), 2);
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
					Url = string.IsNullOrEmpty(url) ? "" : url,
					MUrl = string.IsNullOrEmpty(mUrl) ? "" : mUrl,
				};
				BuyCarServiceDAL.Update(entity, opType, Define.ProductType.Mall);
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex.ToString());
			}
		}
	}
}
