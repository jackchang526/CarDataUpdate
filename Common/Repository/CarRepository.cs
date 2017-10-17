using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.Common.Repository
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

		public static DataSet GetCarInfoById(int carId)
		{
			string sql = @"SELECT  Car_Name, car_ReferPrice,Car_YearType
							FROM    dbo.Car_Basic
							WHERE   Car_Id = @carId
									AND IsState = 1";

			SqlParameter[] _params = {
														   new System.Data.SqlClient.SqlParameter("@carId",System.Data.SqlDbType.Int)
														   };
			_params[0].Value = carId;
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, System.Data.CommandType.Text, sql, _params);
		}

        /// <summary>
        /// 获取最新2个年款的车款数据 
        /// 2016-01-28 songcl 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, CarInfoEntity> GetNewYearCarData(int serialId)
        {
            Dictionary<int, CarInfoEntity> dict = new Dictionary<int, CarInfoEntity>();
            try
            {
                string sql = @"SELECT  car.Car_Id, car.Car_YearType, cs.cs_ShowName, car.Car_Name,
									cs.cs_seoname
							FROM    dbo.Car_Basic car
									LEFT JOIN dbo.Car_Serial cs ON car.Cs_Id = cs.cs_Id
							WHERE   car.Cs_Id = @SerialId
									AND Car_YearType IN ( SELECT  DISTINCT TOP 2
																	Car_YearType
														  FROM      [AutoCarChannel].[dbo].[Car_Basic]
														  WHERE     Cs_Id = @SerialId
														  ORDER BY  Car_YearType DESC )";

                SqlParameter[] _params = { new SqlParameter("@SerialId", SqlDbType.Int) };
                _params[0].Value = serialId;
                DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, _params);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int carId = ConvertHelper.GetInteger(dr["Car_Id"]);
                        int yearType = ConvertHelper.GetInteger(dr["Car_YearType"]);
                        string carName = ConvertHelper.GetString(dr["Car_Name"]);
                        string serialShowName = ConvertHelper.GetString(dr["cs_ShowName"]);
                        string serialSEOName = ConvertHelper.GetString(dr["cs_seoname"]);

                        Dictionary<int, string> dictParams = CarRepository.GetCarAllParamByCarID(carId);

                        if (!dict.ContainsKey(carId))
                        {
                            var entiy = new CarInfoEntity();
                            entiy.CarId = carId;
                            entiy.CarName = carName;
                            entiy.SerialShowName = serialShowName;
                            entiy.SerialSEOName = serialSEOName;
                            entiy.UnderPan_ForwardGearNum = dictParams.ContainsKey(724) ? ConvertHelper.GetInteger(dictParams[724]) : 0;
                            entiy.UnderPan_TransmissionType = dictParams.ContainsKey(712) ? dictParams[712] : "";
                            entiy.YearType = yearType;
                            dict.Add(carId, entiy);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
            return dict;
        }

    }
}
