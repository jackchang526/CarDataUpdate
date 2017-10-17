using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.DataProcesser.Repository
{
	public class CarRepository
	{
		/// <summary>
		/// 获取车款所有参数数据
		/// </summary>
		/// <param name="carID"></param>
		/// <returns></returns>
		public static Dictionary<int, string> GetCarAllParamByCarID(int carID)
		{
			Dictionary<int, string> dic = new Dictionary<int, string>();
			string sql = "select carid,paramid,pvalue from dbo.CarDataBase where carid=@carID";
			SqlParameter[] _param ={
                                      new SqlParameter("@carID",SqlDbType.Int)
                                  };
			_param[0].Value = carID;
			DataSet ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, _param);
			if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dr in ds.Tables[0].Rows)
				{
					int paramid = 0;
					if (int.TryParse(dr["paramid"].ToString(), out paramid))
					{
						if (paramid > 0 && dr["pvalue"].ToString().Trim() != "" && !dic.ContainsKey(paramid))
						{
							dic.Add(paramid, dr["pvalue"].ToString().Trim());
						}
					}
				}
			}
			return dic;
		}

		/// <summary>
		/// 获取在产车型所有车身颜色
		/// </summary>
		/// <param name="serialId">子品牌Id</param>
		/// <returns></returns>
		public static DataSet GetProduceCarsColorBySerialId(int serialId)
		{
			string sql = @"SELECT a.Car_Id,a.Car_YearType,b.Pvalue AS CarColor FROM Car_relation a  
                 INNER JOIN CarDataBase b ON a.Car_Id=b.CarId AND b.paramid=598 
                WHERE a.Cs_Id=@serialId AND a.IsState=0 AND a.car_ProduceState=92
                order by car_yeartype desc";
			SqlParameter[] _params = { new SqlParameter("@serialId", SqlDbType.Int) };
			_params[0].Value = serialId;
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, _params);
		}
	}
}
