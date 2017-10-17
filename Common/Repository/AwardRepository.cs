using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.Common.Repository
{
    public static class AwardRepository
    {
        public static DataTable GetAwards(string awardIds)
        {
            string sql = "SELECT TOP 5 csa.Id,csa.LogoUrl,csa.AwardsName FROM dbo.Car_SerialAwards csa WHERE csa.Id IN (" + awardIds + ") ORDER BY csa.IndexOrder";
            var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString,
                CommandType.Text, sql);
            return ds.Tables[0];
        }

        public static DataTable GetIds(int csId)
        {
            string sql = @"SELECT DISTINCT(csa.Id)
FROM dbo.Car_SerialAwardsCar csac
INNER JOIN dbo.Car_SerialAwardsYear csay ON csay.Id = csac.AwardsYearId
INNER JOIN dbo.Car_SerialAwards csa ON csa.Id = csay.AwardsId
WHERE csac.SerialId = @CsId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@CsId", SqlDbType.Int, 4)
            };
            parameters[0].Value = csId;
            var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString,
                CommandType.Text, sql, parameters);
            return ds.Tables[0];
        }

        public static DataTable GetYears(int awardId, int serialId)
        {
            const string sql = @"SELECT csay.Id,csay.[Year]
FROM dbo.Car_SerialAwardsYear csay
INNER JOIN dbo.Car_SerialAwardsCar csac ON csac.AwardsYearId = csay.Id
WHERE csay.AwardsId = @AwardId AND csac.SerialId = @SerialId GROUP BY csay.id,csay.[Year]
ORDER BY csay.[Year] DESC";
            SqlParameter[] parameters =
            {
                new SqlParameter("@AwardId", SqlDbType.Int, 4),
                new SqlParameter("@SerialId", SqlDbType.Int, 4)
            };
            parameters[0].Value = awardId;
            parameters[1].Value = serialId;
            var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString,
                CommandType.Text, sql, parameters);
            return ds.Tables[0];
        }

        public static List<ChildAwardInfo> GetChildAwardInfos(int awardYearId,int csId)
        {
            const string sql = @"SELECT Top 5 csac.AwardsName,csac2.Remarks 
FROM dbo.Car_SerialAwardsChild csac
INNER JOIN dbo.Car_SerialAwardsCar csac2 ON csac2.ChildAwardsId = csac.Id
WHERE csac.AwardsYearId = @AwardYearId AND csac2.SerialId = @CsId
ORDER BY csac.UpdateTime DESC";
            SqlParameter[] parameters =
            {
                new SqlParameter("@AwardYearId", SqlDbType.Int, 4),
                new SqlParameter("@CsId", SqlDbType.Int, 4)
            };
            parameters[0].Value = awardYearId;
            parameters[1].Value = csId;
            var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString,
                CommandType.Text, sql, parameters);
            var dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }
            var childAwardInfos = new List<ChildAwardInfo>();
            foreach (DataRow row in dt.Rows)
            {
                var childAwardInfo = new ChildAwardInfo();
                childAwardInfo.ChildAwardName = row["AwardsName"].ToString();
                var childNameLength = childAwardInfo.ChildAwardName.Length;
                var remarks = row["Remarks"].ToString();
                if (childNameLength + remarks.Length > 30)
                {
                    remarks = remarks.Substring(0, 30 - childNameLength) + "...";
                }
                childAwardInfo.CarRemark = remarks;
                childAwardInfos.Add(childAwardInfo);
            }
            return childAwardInfos;
        }

        public static int GetCarsCount(int awardYearId)
        {
            const string sql = @"SELECT count(*) FROM dbo.Car_SerialAwardsCar csac WHERE csac.AwardsYearId = @AwardYearId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@AwardYearId", SqlDbType.Int, 4)
            };
            parameters[0].Value = awardYearId;
            var o = SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.AutoStroageConnString,
                CommandType.Text, sql, parameters);
            return Convert.ToInt32(o);
        }

        public static string GetRemarks(int yearId,int csId)
        {
            string sql = @"SELECT csac.Remarks
FROM dbo.Car_SerialAwardsCar csac
WHERE csac.AwardsYearId = @AwardsYearId AND csac.SerialId = @SerialId AND csac.ChildAwardsId = 0";
            SqlParameter[] parameters =
            {
                new SqlParameter("@AwardsYearId", SqlDbType.Int, 4),
                new SqlParameter("@SerialId", SqlDbType.Int, 4)
            };
            parameters[0].Value = yearId;
            parameters[1].Value = csId;
            var ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString,
                CommandType.Text, sql, parameters);
            var dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count == 0)
            {
                return string.Empty;
            }
            return dt.Rows[0]["Remarks"].ToString();
        }
    }
}
