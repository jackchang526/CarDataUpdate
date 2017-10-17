using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Repository;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.Common.Services
{
	public class CommonHtmlService
	{
		/// <summary>
		/// 添加或者更新块内容
		/// </summary>
		/// <param name="entity">实体</param>
		/// <returns></returns>
		public static bool UpdateCommonHtml(CommonHtmlEntity entity)
		{
			try
			{
				int result = CommonHtmlRepository.UpdateCommonHtml(entity);
				return result > 0 ? true : false;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
				return false;
			}
		}
		/// <summary>
		/// 删除块内容
		/// </summary>
		/// <param name="Id">ID(主品牌、品牌、子品牌)</param>
		/// <param name="typeId">id类型</param>
		/// <param name="tagId">页面标示</param>
		/// <param name="blockId">块标示</param>
		/// <returns></returns>
		public static bool DeleteCommonHtml(int Id, CommonHtmlEnum.TypeEnum typeId, CommonHtmlEnum.TagIdEnum tagId, CommonHtmlEnum.BlockIdEnum blockId)
		{
			try
			{
				int result = CommonHtmlRepository.DeleteCommonHtml(Id, (int)typeId, (int)tagId, (int)blockId);
				return result > 0 ? true : false;
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
				return false;
			}
		}
	}
}
