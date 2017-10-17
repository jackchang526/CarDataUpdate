using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using System.Data.SqlClient;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 口碑评分明细
    /// </summary>
    public class KoubeiRatingDetail
    {
        //public event LogHandler Log;

        #region sql
        string sql = @"IF EXISTS (SELECT Top 1 csId FROM Car_CsKouBeiBaseInfo WHERE CsID=@serialId)
                         BEGIN
	                        UPDATE Car_CsKouBeiBaseInfo set 
                            Rating=@Rating,
                            LevelRating=0,
                            RatingVariance=0,
	                        TotalCount=@TotalCount,
	                        KongJian=@KongJian,
	                        DongLi=@DongLi,
	                        CaoKong=@CaoKong,
	                        PeiZhi=@PeiZhi,
	                        ShuShiDu=@ShuShiDu,
	                        XingJiaBi=@XingJiaBi,
	                        WaiGuan=@WaiGuan,
	                        NeiShi=@NeiShi,
	                        YouHao=@YouHao,
	                        UpdateTime=GETDATE()
	                        where CsID=@SerialId
                         END
                         ELSE
                         BEGIN
	                        INSERT INTO [AutoCarChannel].[dbo].[Car_CsKouBeiBaseInfo]
                                   ([CsID]
                                   ,[Rating]
                                   ,[LevelRating]
                                   ,[RatingVariance]
                                   ,[TotalCount]
                                   ,[UpdateTime]
                                   ,[KongJian]
                                   ,[DongLi]
                                   ,[CaoKong]
                                   ,[PeiZhi]
                                   ,[ShuShiDu]
                                   ,[XingJiaBi]
                                   ,[WaiGuan]
                                   ,[NeiShi]
                                   ,[YouHao])
                             VALUES(
		                        @SerialId,
		                        @Rating,0,0,@TotalCount,GETDATE(),@KongJian,@DongLi,@CaoKong,@PeiZhi,@ShuShiDu,@XingJiaBi,@WaiGuan,@NeiShi,@YouHao
                             )
                         END";
        #endregion

        public void UpdateKoubeiRatingDetail()
        {
            Common.Log.WriteLog("开始更新口碑评分明细");
            //InitUrl();
            if (CommonData._koubeiRatingDic == null || CommonData._koubeiRatingDic.Count == 0)
            {
                Common.Log.WriteLog("口碑评分明细字典为空");
                return;
            }
            UpdateCar_CsKouBeiBaseInfo(CommonData._koubeiRatingDic);
            Common.Log.WriteLog("更新口碑评分明细完成");
        }

        /// <summary>
        /// 更新数据库
        /// </summary>
        /// <param name="serialId"></param>
        /// <param name="kongJian"></param>
        /// <param name="dongLi"></param>
        /// <param name="caoKong"></param>
        /// <param name="peiZhi"></param>
        /// <param name="shuShiDu"></param>
        /// <param name="xingJiaBi"></param>
        /// <param name="waiGuan"></param>
        /// <param name="neiShi"></param>
        /// <param name="youHao"></param>
        private void UpdateCar_CsKouBeiBaseInfo(Dictionary<int, Dictionary<string, string>> ratingDic)
        {
            if (ratingDic == null || ratingDic.Count == 0)
            {
                return;
            }
            SqlConnection conn = null;
            SqlCommand cmd = null;
            try
            {
                conn = new SqlConnection(CommonData.ConnectionStringSettings.CarChannelConnString);
                conn.Open();
                cmd = new SqlCommand(sql, conn);
                foreach(KeyValuePair<int,Dictionary<string, string>> kv in ratingDic)
                {
                    Dictionary<string, string> ratingDetailDic = kv.Value;
                    try
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@SerialId", System.Data.SqlDbType.Int);
                        cmd.Parameters.Add("@KongJian", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@DongLi", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@CaoKong", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@PeiZhi", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@ShuShiDu", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@XingJiaBi", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@WaiGuan", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@NeiShi", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@YouHao", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@Rating", System.Data.SqlDbType.Decimal);
                        cmd.Parameters.Add("@TotalCount", System.Data.SqlDbType.Int);

                        cmd.Parameters[0].Value = kv.Key;
                        cmd.Parameters[1].Value = ratingDetailDic["KongJian"];
                        cmd.Parameters[2].Value = ratingDetailDic["DongLi"];
                        cmd.Parameters[3].Value = ratingDetailDic["CaoKong"];
                        cmd.Parameters[4].Value = ratingDetailDic["PeiZhi"];
                        cmd.Parameters[5].Value = ratingDetailDic["ShuShiDu"];
                        cmd.Parameters[6].Value = ratingDetailDic["XingJiaBi"];
                        cmd.Parameters[7].Value = ratingDetailDic["WaiGuan"];
                        cmd.Parameters[8].Value = ratingDetailDic["NeiShi"];
                        cmd.Parameters[9].Value = ratingDetailDic["YouHao"];
                        cmd.Parameters[10].Value = ratingDetailDic["Ratings"];
                        cmd.Parameters[11].Value = ratingDetailDic["TopicCount"];

                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Common.Log.WriteLog("更新口碑评分明细错误，serialId：" + kv.Key + ";" + ex.ToString());
                    }
                }
                
            }
            catch (Exception ex)
            {
                Common.Log.WriteLog("更新口碑评分明细错误:" + ex.ToString());
            }
            finally
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                }
            }
        }
    }
}
