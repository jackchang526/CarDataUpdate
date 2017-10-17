using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.NewsNumXml;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Common;
using System.Data.SqlClient;
using System.Data;
using System.Xml;

namespace BitAuto.CarDataUpdate.DataProcesser.NewsNumXml
{
	public abstract class NewsNumXmlBuilder : BaseBuilder
	{
		public override void BuilderDataOrHtml(int objId) { }

		protected void SerialNewsNumber(int serialId, CarNewsTypes carNewsType)
		{
			if (serialId <= 0)
				return;
			SqlParameter[] param = new SqlParameter[]{
				new SqlParameter("@SerialId", serialId), new SqlParameter("@CarNewsTypeId", (int)carNewsType)
			};

			DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.Text,
					"SELECT YearType, COUNT(1) AS Num FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND SerialId=@SerialId GROUP BY YearType",
					param);

			if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
				return;

			DataRowCollection rows = ds.Tables[0].Rows;

			//无年款 
			int newsCount = 0, yearCount = 0;
			int yearId = 0;
			foreach (DataRow row in rows)
			{
				yearId = ConvertHelper.GetInteger(row["YearType"]);
				yearCount = ConvertHelper.GetInteger(row["Num"]);
				newsCount += yearCount;

				if (yearId > 0)
				{
					// add by chengl Jun.8.2013
					// 更新子品牌年款新闻分类数量到数据库
					UpdateCarNewsCount(serialId, 3, yearId, (int)carNewsType, yearCount);
					NewsNumQueue.AddNewsNumMessage(new NewsNumMessage()
					{
						ObjId = serialId,
						NewsNumMsgType = NewsNumMsgTypes.SerialYear,
						SerialYear = yearId,
						CarNewsType = carNewsType,
						NewsCount = yearCount,
						UpdateTime = DateTime.Now
					});
				}
			}
			// add by chengl Jun.8.2013
			// 更新子品牌非年款新闻分类数量到数据库
			UpdateCarNewsCount(serialId, 3, 0, (int)carNewsType, newsCount);
			int pingceNewsId = 0;
			if (carNewsType == CarNewsTypes.pingce)
			{
				pingceNewsId = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
					"SELECT TOP(1) CmsNewsId FROM SerialNews WHERE CarNewsTypeId=@CarNewsTypeId AND SerialId=@SerialId ORDER BY PublishTime DESC", param)); 
			}
			NewsNumQueue.AddNewsNumMessage(new NewsNumMessage()
			{
				ObjId = serialId,
				NewsNumMsgType = NewsNumMsgTypes.Serial,
				CarNewsType = carNewsType,
				NewsCount = newsCount,
				UpdateTime = DateTime.Now,
				PingCeNewsId = pingceNewsId
			});
		}
		protected void BrandNewsNumber(int serialId, CarNewsTypes carNewsType)
		{
			if (!CommonData.SerialBrandDic.ContainsKey(serialId))
				return;
			int brandId = CommonData.SerialBrandDic[serialId];
			
			int newsCount = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.Text,
					"SELECT COUNT(1) FROM BrandNews WHERE CarNewsTypeId=@CarNewsTypeId AND BrandId=@BrandId",
					new SqlParameter("@BrandId", brandId), new SqlParameter("@CarNewsTypeId", (int)carNewsType)));

			// add by chengl Jun.8.2013
			// 更新品牌新闻分类数量到数据库
			UpdateCarNewsCount(brandId, 2, 0, (int)carNewsType, newsCount);

			NewsNumQueue.AddNewsNumMessage(new NewsNumMessage()
			{
				ObjId = brandId,
				NewsNumMsgType = NewsNumMsgTypes.Brand,
				CarNewsType = carNewsType,
				NewsCount = newsCount,
				UpdateTime = DateTime.Now
			});
		}
		protected void MasterBrandNewsNumber(int serialId, CarNewsTypes carNewsType)
		{
			if (!CommonData.SerialMasterBrandDic.ContainsKey(serialId))
				return;
			int masterBrandId = CommonData.SerialMasterBrandDic[serialId];

			int newsCount = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString,
					CommandType.Text,
					"SELECT COUNT(1) FROM MasterBrandNews WHERE CarNewsTypeId=@CarNewsTypeId AND MasterBrandId=@MasterBrandId",
					new SqlParameter("@MasterBrandId", masterBrandId), new SqlParameter("@CarNewsTypeId", (int)carNewsType)));

			// add by chengl Jun.8.2013
			// 更新主品牌新闻分类数量到数据库
			UpdateCarNewsCount(masterBrandId, 1, 0, (int)carNewsType, newsCount);

			NewsNumQueue.AddNewsNumMessage(new NewsNumMessage()
			{
				ObjId = masterBrandId,
				NewsNumMsgType = NewsNumMsgTypes.MasterBrand,
				CarNewsType = carNewsType,
				NewsCount = newsCount,
				UpdateTime = DateTime.Now
			});
		}

		/// <summary>
		/// add by chengl Jun.8.2013
		/// 更新车型频道主品牌、品牌、子品牌 数据库分类新闻数量
		/// </summary>
		/// <param name="id">主品牌ID、品牌ID、子品牌ID</param>
		/// <param name="idType">ID列的类型(1:主品牌ID、2:品牌ID、3:子品牌ID)</param>
		/// <param name="year">年款(没有传0)</param>
		/// <param name="carNewsTypeID">车型频道新闻类型ID</param>
		/// <param name="count">新闻分类数量</param>
		private void UpdateCarNewsCount(int id, int idType, int year, int carNewsTypeID, int count)
		{
			string spName = "CarNewsCountUpdate";
			SqlParameter[] parameters = { 
											new SqlParameter("@ID", SqlDbType.Int) ,
											new SqlParameter("@IDType", SqlDbType.SmallInt) ,
											new SqlParameter("@Year", SqlDbType.Int) ,
											new SqlParameter("@CarNewsTypeID", SqlDbType.Int) ,
											new SqlParameter("@Count", SqlDbType.Int) 
										};
			parameters[0].Value = id;
			parameters[1].Value = idType;
			parameters[2].Value = year;
			parameters[3].Value = carNewsTypeID;
			parameters[4].Value = count;
			int res = BitAuto.Utils.Data.SqlHelper.ExecuteNonQuery(
		   Common.CommonData.ConnectionStringSettings.CarDataUpdateConnString
		   , CommandType.StoredProcedure, spName, parameters);
		}
	}
	#region 仅子品牌
	/// <summary>
	/// 试驾
	/// </summary>
	public class ShiJiaNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.shijia);
		}
	}
	/// <summary>
	/// 改装
	/// </summary>
	public class GaiZhuangNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.gaizhuang);
		}
	}
	/// <summary>
	/// 安全
	/// </summary>
	public class AnQuanNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.anquan);
		}
	}
	/// <summary>
	/// 科技
	/// </summary>
	public class KeJiNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.keji);
		}
	}
	/// <summary>
	/// 评测
	/// </summary>
	public class PingCeNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.pingce);
		}
	}
	/// <summary>
	/// 保养
	/// </summary>
	public class BaoYangNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.baoyang);
		}
	} 
	#endregion

	#region 主品牌、品牌、子品牌
	/// <summary>
	/// 视频(主品牌、品牌、子品牌)
	/// </summary>
	public class VideoNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			SerialNewsNumber(objId);
			base.BrandNewsNumber(objId, CarNewsTypes.video);
			base.MasterBrandNewsNumber(objId, CarNewsTypes.video);
		}
		public void SerialNewsNumber(int serialId)
		{
			int count = 0;
			XmlReader reader = null;
			try
			{
				reader = XmlReader.Create(string.Format("{0}?articaltype=3&brandid={1}&shownews=0", CommonData.CommonSettings.NewsUrl, serialId.ToString()));
				if (reader.ReadToFollowing("allcount"))
				{
					count = ConvertHelper.GetInteger(reader.ReadString());
				}
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(exp);
			}
			finally
			{
				if (reader != null && reader.ReadState != ReadState.Closed)
					reader.Close();
			}

			NewsNumQueue.AddNewsNumMessage(new NewsNumMessage()
			{
				ObjId = serialId,
				NewsNumMsgType = NewsNumMsgTypes.Serial,
				CarNewsType = CarNewsTypes.video,
				NewsCount = count,
				UpdateTime = DateTime.Now
			});
		}
	}
	/// <summary>
	/// 导购(主品牌、品牌、子品牌)
	/// </summary>
	public class DaoGouNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.daogou);
			base.BrandNewsNumber(objId, CarNewsTypes.daogou);
			base.MasterBrandNewsNumber(objId, CarNewsTypes.daogou);
		}
	}
	/// <summary>
	/// 评测(主品牌、品牌、子品牌)
	/// </summary>
	public class TreePingCeNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.treepingce);
			base.BrandNewsNumber(objId, CarNewsTypes.treepingce);
			base.MasterBrandNewsNumber(objId, CarNewsTypes.treepingce);
		}
	}
	/// <summary>
	/// 用车(主品牌、品牌、子品牌)
	/// </summary>
	public class YongCheNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.yongche);
			base.BrandNewsNumber(objId, CarNewsTypes.yongche);
			base.MasterBrandNewsNumber(objId, CarNewsTypes.yongche);
		}
	}
	/// <summary>
	/// 行情(主品牌、品牌、子品牌)
	/// </summary>
	public class HangQingNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.hangqing);
			base.BrandNewsNumber(objId, CarNewsTypes.hangqing);
			base.MasterBrandNewsNumber(objId, CarNewsTypes.hangqing);
		}
	}
	/// <summary>
	/// 新闻(主品牌、品牌、子品牌)
	/// </summary>
	public class XinWenNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.xinwen);
			base.BrandNewsNumber(objId, CarNewsTypes.xinwen);
			base.MasterBrandNewsNumber(objId, CarNewsTypes.xinwen);
		}
	}
	/// <summary>
	/// 置换(品牌、子品牌)
	/// </summary>
	public class ZhiHuanNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.zhihuan);
			base.BrandNewsNumber(objId, CarNewsTypes.zhihuan);
		}
	}
	public class HuatiNewsNum : NewsNumXmlBuilder
	{
		public override void BuilderDataOrHtml(int objId)
		{
			base.SerialNewsNumber(objId, CarNewsTypes.huati);
			base.BrandNewsNumber(objId, CarNewsTypes.huati);
		}
	}
	#endregion
}
