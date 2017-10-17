using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.DataProcesser
{
   
    public class UpdateCarDataForSelectToolV2
    {
        /// <summary>
        /// 更新高级选车工具车型数据
        /// </summary>
        private static void UpdateNewCarDataForSelect()
        {
            SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.StoredProcedure, "SP_UpdateSelectCarDataByCsIdV2");
        }

        /// <summary>
        /// 更新选车工具的车型报价
        /// </summary>
        /// <returns></returns>
        public static void UpdateCarPriceForSelect()
        {
           // UpdateNewCarDataForSelect();
            Dictionary<int, Dictionary<string, decimal>> dicPrice = CommonData.dictCarPriceData;
            DataSet ds = GetCarDataForSelect();
            // modified by chengl Apr.12.2011
            // 清除没有报价的车型数据
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                StringBuilder sbClear = new StringBuilder();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int carid = int.Parse(dr["carid"].ToString());
                    Log.WriteLog(string.Format("生成高级选车工具车款: msg:[carid:{0}]", carid));
                    if (!dicPrice.ContainsKey(carid))
                    {
                        // 没有报价 并且目前报价最小最大值不为零
                        decimal maxHas = 0;
                        decimal minHas = 0;
                        if (decimal.TryParse(dr["minPrice"].ToString(), out minHas))
                        { }
                        if (decimal.TryParse(dr["maxprice"].ToString(), out maxHas))
                        { }
                        if (maxHas > 0 || minHas > 0)
                        {
                            // 目前报价不为0的话清零
                            sbClear.AppendLine(" update CarInfoForSelectingV2 set minPrice=0,maxprice=0 where carid=" + carid.ToString());
                        }
                    }
                    else
                    {
                        SqlParameter[] paramPrice = { 
											new SqlParameter("@MinPrice", SqlDbType.Decimal),
											new SqlParameter("@MaxPrice", SqlDbType.Decimal),
											new SqlParameter("@carid", SqlDbType.Int)
										};
                        paramPrice[0].Value = dicPrice[carid]["MinPrice"];
                        paramPrice[1].Value = dicPrice[carid]["MaxPrice"];
                        paramPrice[2].Value = carid;
                        string sql = "UPDATE CarInfoForSelectingV2 SET MinPrice=@MinPrice,MaxPrice=@MaxPrice WHERE carid=@carid";
                        SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, paramPrice);
                    }
                }
                if (sbClear.Length > 0)
                {
                    SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sbClear.ToString());
                }
            }
        }

        /// <summary>
        /// 取目前选车工具表内车型ID
        /// </summary>
        /// <returns></returns>
        private static DataSet GetCarDataForSelect()
        {
            DataSet ds = new DataSet();
            string sql = "select carid,minPrice,maxprice from CarInfoForSelectingV2 order by carid";
            ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
            return ds;
        }
    }
}
