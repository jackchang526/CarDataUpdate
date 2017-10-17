using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Utils;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 亮点配置关系数据更新 2015-10-29
    /// </summary>
    public class SparkleSerialRelUpdate
    {
        public void UpdateSparkleSerialRel()
        {
            var savePath = CommonData.CommonSettings.SavePath + @"\Sparkle\";

            var path = Path.Combine(savePath, Path.GetFileName("sparkle.xml"));

            XDocument doc = XDocument.Load(path);
            
            var query = from t in doc.Descendants("Item")
                select new
                {
                    SparkleId = t.Element("SparkleId").Value,
                    SparkleName = t.Element("SparkleName").Value,
                    ParaName = t.Element("ParaName").Value,
                    ParaId = t.Element("ParaId").Value,
                    ParaValues = t.Element("ParaValues").Value
                };

            //Dictionary<int, Dictionary<int, List<int>>> dictionary = new Dictionary<int, Dictionary<int, List<int>>>();

            foreach (var item in query)
            {
                DataRowCollection rowCollection = GetCarDataBase(ConvertHelper.GetInteger(item.ParaId));
                if (rowCollection == null)
                    continue;

                var strings = item.ParaValues.Split(',');
                Dictionary<int, List<int>> sparkleDic = new Dictionary<int, List<int>>();
                int sparkleId = ConvertHelper.GetInteger(item.SparkleId);
                int paraId = ConvertHelper.GetInteger(item.ParaId);
                
                foreach (DataRow row in rowCollection)
                {
                    if (!Array.Exists(strings, element => element == row["Pvalue"].ToString())) continue;

                    int csId = ConvertHelper.GetInteger(row["Cs_Id"]);
                    if (sparkleDic.ContainsKey(sparkleId))
                    {
                        if (!sparkleDic[sparkleId].Contains(csId))
                        {
                            sparkleDic[sparkleId].Add(csId);
                        }
                    }
                    else
                    {
                        sparkleDic.Add(sparkleId, new List<int> { csId });
                    }
                }

                //if (!dictionary.ContainsKey(paraId))
                //{
                //    dictionary.Add(paraId, sparkleDic);
                //}
                Update_Sparkle_Serial_Rel(sparkleDic);
            }
        }

        public DataRowCollection GetCarDataBase(int paramId)
        {
            SqlParameter[] param = { new SqlParameter("@ParamId", SqlDbType.Int) };
            param[0].Value = paramId;
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, "SELECT  CarId, ParamId,Pvalue,Cs_Id FROM [dbo].[CarDataBase] cdb LEFT JOIN dbo.Car_relation car ON car.Car_Id = cdb.CarId WHERE IsState = 0 and car.car_SaleState<>96 and ParamId=@ParamId", param);//car.car_SaleState<>96 在销,待销车款
            if (ds != null)
            {
                return ds.Tables[0].Rows;
            }
            return null;
        }

        public void Update_Sparkle_Serial_Rel(Dictionary<int, List<int>> sparkleDic)
        {
            SqlConnection sqlConnection = new SqlConnection(CommonData.ConnectionStringSettings.CarChannelConnString);
            sqlConnection.Open();
            SqlTransaction trans = sqlConnection.BeginTransaction();

            try
            {
                foreach (var item in sparkleDic)
                {
                    
                    int sparkleId = ConvertHelper.GetInteger(item.Key);
                    List<int> list = item.Value;

                    Console.WriteLine(string.Format("亮点ID：{0}，车系数量：{1}",sparkleId,list.Count));

                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@tb", SqlDbType.Structured),
                        new SqlParameter("@Id", SqlDbType.Int)
                    };
                    parameters[0].Value = GetDataTable(list);
                    parameters[1].Value = sparkleId;
                    SqlHelper.ExecuteNonQuery(trans, CommandType.StoredProcedure, "SP_Update_Sparkle_Serial_Rel",
                        parameters);
                }
                
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
            }
            finally
            {
                sqlConnection.Close();
                trans.Dispose();
                sqlConnection.Dispose();   
            }
        }

        /// <summary>
        ///     数据
        /// </summary>
        /// <returns></returns>
        private static DataTable GetDataTable(IEnumerable<int> list)
        {
            var tb = new DataTable();
            tb.Columns.Add("Id", typeof(int));
            foreach (int i in list)
            {
                DataRow row = tb.NewRow();
                row[0] = i;
                tb.Rows.Add(row);
            }
            return tb;
        }

        //
    }
}
