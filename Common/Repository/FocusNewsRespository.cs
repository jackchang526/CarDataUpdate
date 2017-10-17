using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Config;
using System.Web;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Common.Repository
{
	public class FocusNewsRespository
	{
		/// <summary>
		/// 获取编辑排序新闻
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
		public static DataSet GetOrderNewsData(int serialId)
		{
			string sql = @"SELECT Title,FilePath,CategoryId,PublishTime,CmsNewsId,FaceTitle,OrderNumber FROM NewsBlockOrder WHERE ObjId=@ObjId and BlockType=@BlockType and StartTime<=@NowTime AND EndTime >=@NowTime AND OrderNumber>0 ORDER BY OrderNumber ASC";
			SqlParameter[] _params ={ 
										new SqlParameter("@ObjId", serialId) , 
										new SqlParameter("@BlockType",  (int)NewsBlockOrderTypes.serialfocus) , 
										new SqlParameter("@NowTime", DateTime.Now.Date)
									};
			return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, _params);
		}
		/// <summary>
		/// 获取焦点新闻  
		/// </summary>
		/// <param name="serialId"></param>
		/// <returns></returns>
//        public static DataSet GetFocusNewsData(int serialId)
//        { 
//            //modified by sk 2013.08.06 视频分类排后 不足7条时才展示
//            string sql = @"SELECT sn.Title, sn.FilePath, sn.PublishTime, sn.CmsNewsId, sn.CreativeType, n.FaceTitle, n.CommentNum,n.FirstPicUrl,n.Author,
//									   sn.CategoryId, CASE 
//														   WHEN categoryid IN (74,70,348,67,358,359,325,326,327,328) THEN 2
//														   ELSE 1
//													  END subnewstype
//								FROM   SerialNews sn
//									   LEFT JOIN News n
//											ON  sn.CarNewsId = n.ID
//								WHERE  sn.CarNewsTypeId = 1
//									   AND sn.SerialId = @SerialId
//								ORDER BY subnewstype ASC,
//									   sn.PublishTime DESC";

//            return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, new SqlParameter("@SerialId", serialId));
//        }
        /// <summary>
        /// desc:1200改版新闻
        /// date:2017-1-10
        /// author:zf
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        public static DataSet GetFocusNewsData(int serialId)
        {
            int[] curArrCategoryIds = GetCarNewsType(1);   //1为焦点新闻
            SqlParameter[] param = new SqlParameter[]
            {
                new SqlParameter("@SerialId",serialId),
                new SqlParameter("@CategoryIds", (curArrCategoryIds == null ? "" : string.Join(",", curArrCategoryIds)))
            };
            string sql = @"SELECT n.Title, n.Url AS FilePath,sn.PublishTime, n.NewsId AS CmsNewsId,
                            sn.CopyRight AS CreativeType, n.ShortTitle AS FaceTitle, n.CommentCount AS CommentNum,ImageConverUrl AS FirstPicUrl,n.Author, sn.CategoryId, 
                            CASE  WHEN sn.categoryid IN (74,70,348,67,358,359,325,326,327,328) THEN 2
											  ELSE 1
													  END subnewstype
								  FROM    dbo.Car_SerialNewsV2 sn
                        INNER JOIN dbo.Car_NewsV2 n ON sn.CarNewsId = n.Id
                        INNER JOIN dbo.func_splitid_clustered(@CategoryIds,',') ct ON sn.CategoryId = ct.c1
                WHERE   sn.SerialId = @SerialId and  sn.CopyRight = 0
								ORDER BY subnewstype ASC,sn.PublishTime DESC";

            return SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, param);
        }
        public static int[] GetCarNewsType(int carNewsType)
        {
            string sql = @"SELECT CmsCategoryId
							FROM    [dbo].[CarNewsTypeDef]
							WHERE   CarNewsTypeId = @CarNewsTypeId";
            SqlParameter[] param = { new SqlParameter("@CarNewsTypeId",SqlDbType.Int) };
            param[0].Value = carNewsType;
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, param);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                int[] result = ds.Tables[0].AsEnumerable().Select(row => ConvertHelper.GetInteger(row["CmsCategoryId"])).ToArray();
                if ( result.Length > 0)
                {
                    return result;
                }
            }
            return null;
        }
	}
}
