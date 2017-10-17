using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.Common.Repository
{
	public class CarPingceInfoRepository
	{
		public static DataSet GetDataBySerialId(int serialId)
		{
			string sql = @"select csid,url,tagid from CarPingceInfo where csid=@SerialId ORDER BY tagid";
			SqlParameter[] _params = { new SqlParameter("@SerialId", DbType.Int32) };
			_params[0].Value = serialId;
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString
			   , CommandType.Text
			   , sql, _params);
		}
	}
}
