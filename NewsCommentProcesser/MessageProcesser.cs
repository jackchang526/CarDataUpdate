using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common;
using BitAuto.Utils;
using System.Data;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.NewsCommentProcesser.com.bitauto.news;
using System.Data.SqlClient;

namespace BitAuto.CarDataUpdate.NewsCommentProcesser
{
    public class MessageProcesser:BaseProcesser
    {
        private NewsService _newsService;
        private NewsService NewsService
        {
            get { if (_newsService == null) { _newsService = new NewsService(); } return _newsService; }
        }
        public override void Processer(Common.Model.ContentMessage msg)
        {
            if (msg == null || msg.ContentBody==null)
            {
                Log.WriteLog("error, ContentMessage is null from NewsCommentProcesser!");
                return;
            }

            Log.WriteLog("start processer newscomment!");

            string[] newsIds = CommonFunction.GetXmlElementInnerText(msg.ContentBody, "/MessageBody/NewsId", string.Empty).Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

            int[] query = (from newsIdStr in newsIds
                        let newsId = ConvertHelper.GetInteger(newsIdStr.Trim())
                        where newsId > 0
                        select newsId).Distinct().ToArray();
            StringBuilder ids = new StringBuilder();
            foreach (int id in query)
            {
                ids.Append(",").Append(id.ToString());
            }
            if (ids.Length > 0)
            {
                string whereId = ids.Remove(0, 1).ToString();

                DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, string.Format("select [CmsNewsId],[Num] from NewsCommentNum where cmsnewsid in ({0})", whereId));

                if (ds == null || ds.Tables.Count <= 0)
                {
                    Log.WriteLog("error, ExecuteDataset!");
                }
                else
                {
                    DataTable idTable = null;
                    try
                    {
                        Log.WriteLog("get newsservice commentnum!");
						System.Net.ServicePointManager.Expect100Continue = false;
                        idTable = this.NewsService.SortNewsByComments(query);
                    }
                    catch (Exception exp)
                    {
                        Log.WriteErrorLog(exp);
                    }
                    if (idTable == null || idTable.Rows.Count <= 0)
                    {
                        return;
                    }

                    Log.WriteLog("get newsservice count:" + idTable.Rows.Count.ToString() + "!");

                    DataTable dt = ds.Tables[0];
                    DataRow[] rows = null;
                    DataRow curRow = null;
                    int newsId;
                    foreach (DataRow idRow in idTable.Rows)
                    {
                        newsId = ConvertHelper.GetInteger(idRow["ID"]);
                        rows = dt.Select("cmsnewsid=" + newsId.ToString());
                        if (rows == null || rows.Length <= 0)
                        {
                            curRow = dt.NewRow();
                            dt.Rows.Add(curRow);
                        }
                        else
                        {
                            curRow = rows[0];
                        }
                        curRow["CmsNewsId"] = newsId;
                        curRow["Num"] = ConvertHelper.GetInteger(idRow["CommentCount"]);
                    }
                    SqlConnection conn=null;
                    try
                    {
                        Log.WriteLog("start exec sqlupdate");
                        conn = new SqlConnection(CommonData.ConnectionStringSettings.CarDataUpdateConnString);
                        SqlCommand insertCommand = new SqlCommand("insert into NewsCommentNum([CmsNewsId],[Num]) values(@CmsNewsId,@Num)", conn);
                        insertCommand.Parameters.Add("@CmsNewsId", SqlDbType.Int, 4, "CmsNewsId");
                        insertCommand.Parameters.Add("@Num", SqlDbType.Int, 4, "Num");

                        SqlCommand updateCommand = new SqlCommand("update NewsCommentNum set [Num]=@Num where CmsNewsId=@CmsNewsId", conn);
                        updateCommand.Parameters.Add("@CmsNewsId", SqlDbType.Int, 4, "CmsNewsId");
                        updateCommand.Parameters.Add("@Num", SqlDbType.Int, 4, "Num");

                        SqlHelper.UpdateDataset(insertCommand, new SqlCommand(), updateCommand, ds, dt.TableName);

                        Log.WriteLog("end exec update succeed!");

                        Log.WriteLog("start exec news update!");

                        SqlHelper.ExecuteNonQuery(conn, CommandType.Text, string.Format("UPDATE news SET CommentNum = a.num FROM NewsCommentNum AS a WHERE news.CmsNewsId=a.cmsnewsid AND a.cmsnewsid IN ({0})", whereId));

                        Log.WriteLog("end exec news update succeed!");
                    }
                    catch (Exception exp)
                    {
                        Log.WriteErrorLog(exp);
                    }
                    finally
                    {
                        if (conn != null && conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                    Log.WriteLog("end exec update!");
                }
            }
            else
            {
                Log.WriteLog("error, no ids!");
            }
            Log.WriteLog("end processer newscomment!");
        }
    }
}
