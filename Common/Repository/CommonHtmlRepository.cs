using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.Common.Repository
{
	public class CommonHtmlRepository
	{
		/// <summary>
		/// 添加或者更新块内容
		/// </summary>
		/// <param name="entity">实体</param>
		/// <returns>影响行数</returns>
		public static int UpdateCommonHtml(CommonHtmlEntity entity)
		{
			SqlParameter[] _params = { 
										 new SqlParameter("@Id", SqlDbType.Int),
										 new SqlParameter("@TypeId", SqlDbType.Int),
										 new SqlParameter("@TagId", SqlDbType.Int),
										 new SqlParameter("@BlockId", SqlDbType.Int),
										 new SqlParameter("@HtmlContent", SqlDbType.NVarChar),
										 new SqlParameter("@UpdateTime", SqlDbType.DateTime)
									 };
			_params[0].Value = entity.ID;
			_params[1].Value = (int)entity.TypeID;
			_params[2].Value = (int)entity.TagID;
			_params[3].Value = (int)entity.BlockID;
			_params[4].Value = entity.HtmlContent;
			_params[5].Value = entity.UpdateTime;
			return SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.StoredProcedure, "SP_UpdateCommonHtml", _params);
		}
		/// <summary>
		/// 删除块内容
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="typeId"></param>
		/// <param name="tagId"></param>
		/// <param name="blockId"></param>
		/// <returns></returns>
		public static int DeleteCommonHtml(int Id, int typeId, int tagId, int blockId)
		{
			string sql = @"DELETE FROM dbo.Car_CommonHtml WHERE ID=@Id AND TypeID=@TypeId AND TagID=@TagId AND BlockID=@BlockId";
			SqlParameter[] _params = { 
										 new SqlParameter("@Id", SqlDbType.Int),
										 new SqlParameter("@TypeId", SqlDbType.Int),
										 new SqlParameter("@TagId", SqlDbType.Int),
										 new SqlParameter("@BlockId", SqlDbType.Int)
									 };
			_params[0].Value = Id;
			_params[1].Value = typeId;
			_params[2].Value = tagId;
			_params[3].Value = blockId;
			return SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, _params);
		}
	}
}
