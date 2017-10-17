using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using System.Data.SqlClient;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Utils.Data;
using System.Data;
using System.Xml;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class CarNewsTypeUpdate
	{
		/// <summary>
		/// 删除并从新CarNewsType表数据
		/// </summary>
		public void DeleteAndInserCarNewsType()
		{
			OnLog("start DeleteAndInserCarNewsType!", true);
			foreach (int carNewsTypeId in CommonData.CarNewsTypeSettings.CarNewsTypeList.Keys)
			{
				ResetCarNewsType(carNewsTypeId);
				OnLog(string.Format("carNewsType:{0} succeed!", carNewsTypeId.ToString()), true);
			}
			OnLog("end DeleteAndInserCarNewsType!", true);
		}

		/// <summary>
		/// 删除并从新CarNewsType表数据
		/// </summary>
		public void UpdateCarNewsType(int carNewsTypeId)
		{
			if (carNewsTypeId < 1 || !CommonData.CarNewsTypeSettings.CarNewsTypeList.ContainsKey(carNewsTypeId))
			{
				OnLog("参数错误!，请输入有效的CarNewsTypeId", true);
				return;
			}

			CarNewsTypeItem carNewsTypeItem = CommonData.CarNewsTypeSettings.CarNewsTypeList[carNewsTypeId];
			List<int> oldCategoryList = (from cateItem in CommonData.CategoryCarNewsTypeDic
										 where cateItem.Value.Contains(carNewsTypeId)
										 orderby cateItem.Key ascending
										 select cateItem.Key).ToList();

			List<int> newCategoryList = carNewsTypeItem.CategoryIdList.Union(
				from cateId in carNewsTypeItem.CategoryIdList
				from subCateTree in CommonData.CategoryTreeDic
				from subCateId in subCateTree.Value
				where subCateTree.Key == cateId
				orderby subCateId ascending
				select subCateId
				).ToList();

			if (oldCategoryList.Count == newCategoryList.Count)
			{
				bool israturn=true;
				for (int i = 0; i < oldCategoryList.Count; i++)
				{
					if (oldCategoryList[i] != newCategoryList[i])
					{
						israturn = false;
						break;
					}
				}
				if (israturn)
				{
					OnLog("分类相同，操作终止！", true);
					return;
				}
			}

			OnLog("更新数据库中的分类！", true);
			ResetCarNewsType(carNewsTypeItem.Id);
			OnLog("更新完成！", true);

			OnLog("获取需删除的新闻id！", true);
			List<int> delNewsIds = DeleteCategory(oldCategoryList, newCategoryList);
			OnLog("获取数量:" + (delNewsIds == null ? "0" : delNewsIds.Count.ToString()), true);
			OnLog("获取新增的新闻id！", true);
			List<int> addNewsIds = AddCategory(oldCategoryList, newCategoryList);
			OnLog("获取数量:" + (addNewsIds == null ? "0" : addNewsIds.Count.ToString()), true);

			List<int> cmsNewsIds = new List<int>((delNewsIds == null ? 0 : delNewsIds.Count) + (addNewsIds == null ? 0 : addNewsIds.Count));
			if (cmsNewsIds.Capacity > 0)
			{
				if (delNewsIds != null && delNewsIds.Count > 0)
					cmsNewsIds.AddRange(delNewsIds);
				if (addNewsIds != null && addNewsIds.Count > 0)
					cmsNewsIds.AddRange(addNewsIds);

				foreach (int cmsid in cmsNewsIds.Distinct())
				{
					OnLog("发送消息至消息队列 msg:" + cmsid.ToString(), true);
					CommonFunction.SendQueueMessage(cmsid, DateTime.Now, "CMS", "News");
				}
			}
			OnLog("完成！", true);
		}
		/// <summary>
		/// 新增分类,返回新闻id列表
		/// </summary>
		private List<int> AddCategory(List<int> oldCategoryList, List<int> newCategoryList)
		{
			var addList = (from newId in newCategoryList
						   where !oldCategoryList.Contains(newId)
						   select newId).ToList();

			if (addList.Count > 0)
			{
				addList = CommonFunction.RemoveCarNewsTypeSubCategory(addList);
				string ids = string.Join(",", addList.ConvertAll(item => item.ToString()).ToArray());
				string urlFormat = CommonData.CommonSettings.NewsUrl + "?brandid={0}&natureType=0,1,5&getcount=1000&ismain=1&categoryId=" + ids;
				OnLog("获取新闻urlformat:" + urlFormat, true);

				List<int> result = new List<int>(CommonData.SerialDic.Count*100);
				XmlReader reader = null;
				foreach (int serialId in CommonData.SerialDic.Keys)
				{
					OnLog("获取新闻:serialId:" + serialId.ToString(), true);
					try
					{
						reader = XmlReader.Create(string.Format(urlFormat, serialId.ToString()));
						while (reader.ReadToFollowing("newsid"))
						{
							result.Add(ConvertHelper.GetInteger(reader.ReadString()));
						}
					}
					catch (Exception exp)
					{
						OnLog(exp.Message, true);
					}
					finally
					{
						if (reader != null)
							reader.Close();
					}
				}
				return result;
			}
			return null;

		}
		/// <summary>
		/// 删除分类,返回新闻id列表
		/// </summary>
		private List<int> DeleteCategory(List<int> oldCategoryList, List<int> newCategoryList)
		{
			var deleteList = (from oldId in oldCategoryList
							  where !newCategoryList.Contains(oldId)
							  select oldId).ToList();
			if (deleteList.Count > 0)
			{
				string ids = string.Join(",", deleteList.ConvertAll(item => item.ToString()).ToArray());
				DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.Text,
					string.Format(@"SELECT CmsNewsId FROM BrandNews WHERE CategoryId IN ({0}) 
UNION
SELECT CmsNewsId FROM LevelNews WHERE CategoryId IN ({0})
UNION
SELECT CmsNewsId FROM MasterBrandNews WHERE CategoryId IN ({0})
UNION
SELECT CmsNewsId FROM ProducerNews WHERE CategoryId IN ({0})
UNION
SELECT CmsNewsId FROM SerialNews WHERE CategoryId IN ({0})
", ids));

				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					List<int> result = new List<int>(ds.Tables[0].Rows.Count);
					foreach (DataRow row in ds.Tables[0].Rows)
					{
						result.Add(Convert.ToInt32(row["cmsnewsid"]));
					}
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// 根据配置，从新生成指定分类
		/// </summary>
		private void ResetCarNewsType(int carNewsTypeId)
		{
			if (carNewsTypeId < 1)
			{
				OnLog("参数错误!，请输入有效的CarNewsTypeId", true);
				return;
			}
			//删除CarNewsType
			OnLog("删除CarNewsTypeDef，CarNewsTypeId:" + carNewsTypeId.ToString(), true);
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, "delete from CarNewsTypeDef where CarNewsTypeId=" + carNewsTypeId.ToString());
			//删除CarNewsTypeDef
			OnLog("删除CarNewsType，CarNewsTypeId:" + carNewsTypeId.ToString(), true);
			SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, System.Data.CommandType.Text, "delete from CarNewsType where Id=" + carNewsTypeId.ToString());

			if (!CommonData.CarNewsTypeSettings.CarNewsTypeList.ContainsKey(carNewsTypeId))
			{
				OnLog(string.Format("配置中不包含carNewsTypeid={1}的配置" + carNewsTypeId.ToString()), true);
				return;
			}

			CarNewsTypeItem carNewsTypeItem = CommonData.CarNewsTypeSettings.CarNewsTypeList[carNewsTypeId];

			SqlConnection conn = null;

			try
			{
				conn = new SqlConnection(CommonData.ConnectionStringSettings.CarDataUpdateConnString);

				SqlCommand cmdInsertCarNewsType = new SqlCommand("insert into CarNewsType(Id, Name, TypeStr, [Description],CarTypes) values(@Id,@Name,@TypeStr,@Description,@CarTypes)", conn);
				SqlCommand cmdInsertCarNewsTypeDef = new SqlCommand("insert into CarNewsTypeDef(CarNewsTypeId,CmsCategoryId,IsDefine) values(@CarNewsTypeId,@CmsCategoryId,@IsDefine)", conn);
				//CarNewsType表参数
				cmdInsertCarNewsType.Parameters.Add("@Id", System.Data.SqlDbType.Int);
				cmdInsertCarNewsType.Parameters.Add("@Name", System.Data.SqlDbType.VarChar);
				cmdInsertCarNewsType.Parameters.Add("@TypeStr", System.Data.SqlDbType.VarChar);
				cmdInsertCarNewsType.Parameters.Add("@Description", System.Data.SqlDbType.NVarChar);
				cmdInsertCarNewsType.Parameters.Add("@CarTypes", System.Data.SqlDbType.Int);
				//CarNewsTypeDef表参数
				cmdInsertCarNewsTypeDef.Parameters.Add("@CarNewsTypeId", System.Data.SqlDbType.BigInt);
				cmdInsertCarNewsTypeDef.Parameters.Add("@CmsCategoryId", System.Data.SqlDbType.BigInt);
				cmdInsertCarNewsTypeDef.Parameters.Add("@IsDefine", System.Data.SqlDbType.Bit);

				conn.Open();

				#region 循环向CarNewsType表中插入数据

				cmdInsertCarNewsType.Parameters[0].Value = carNewsTypeItem.Id;
				cmdInsertCarNewsType.Parameters[1].Value = carNewsTypeItem.Name;
				cmdInsertCarNewsType.Parameters[2].Value = carNewsTypeItem.TypeStr;
				cmdInsertCarNewsType.Parameters[3].Value = carNewsTypeItem.Description;
				cmdInsertCarNewsType.Parameters[4].Value = (int)carNewsTypeItem.CarTypes;

				cmdInsertCarNewsType.ExecuteNonQuery();

				cmdInsertCarNewsTypeDef.Parameters[0].Value = carNewsTypeItem.Id;

				foreach (int cateId in carNewsTypeItem.CategoryIdList)
				{
					cmdInsertCarNewsTypeDef.Parameters[1].Value = cateId;
					cmdInsertCarNewsTypeDef.Parameters[2].Value = 1;
					cmdInsertCarNewsTypeDef.ExecuteNonQuery();

					if (CommonData.CategoryTreeDic.ContainsKey(cateId))
					{
						foreach (int subCateId in CommonData.CategoryTreeDic[cateId])
						{
							cmdInsertCarNewsTypeDef.Parameters[1].Value = subCateId;
							cmdInsertCarNewsTypeDef.Parameters[2].Value = 0;
							cmdInsertCarNewsTypeDef.ExecuteNonQuery();
						}
					}
				}
				#endregion

				OnLog("exec ResetCarNewsType succeed!msg:" + carNewsTypeId.ToString(), true);
			}
			catch (Exception ex)
			{
				OnLog(string.Format("exec ResetCarNewsType error! msg:{0} errormsg:{1}", carNewsTypeId.ToString(), ex.Message), true);
			}
			finally
			{
				if (conn != null && conn.State != System.Data.ConnectionState.Closed)
					conn.Close();
			}
		}

		#region 写日志
		public event LogHandler Log;
		/// <summary>
		/// 写Log
		/// </summary>
		/// <param name="logText"></param>
		public void OnLog(string logText, bool nextLine)
		{
			if (Log != null)
				Log(this, new LogArgs(logText, nextLine));
		} 
		#endregion
	}
}
