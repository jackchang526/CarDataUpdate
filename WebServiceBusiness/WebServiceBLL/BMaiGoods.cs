using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.WebServiceModel;
using BitAuto.CarDataUpdate.WebServiceDAL;
using System.Xml;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.WebServiceBLL
{
	public class BMaiGoods
	{

		#region

		public void AddYiPai(XElement bodyElement)
		{
			DMaiGoods.UpdateMaiGoodsYiPai(bodyElement, "add");
		}

		public void UpdateYiPai(XElement bodyElement)
		{
			DMaiGoods.UpdateMaiGoodsYiPai(bodyElement, "update");
		}

		public void DeleteYiPai(XElement bodyElement)
		{
			DMaiGoods.UpdateMaiGoodsYiPai(bodyElement, "delete");
		}

		public void StartSaleYiPai(XElement bodyElement)
		{
			DMaiGoods.UpdateMaiGoodsYiPai(bodyElement, "startsale");
		}

		public void StopSaleYiPai(XElement bodyElement)
		{
			DMaiGoods.UpdateMaiGoodsYiPai(bodyElement, "stopsale");
		}

		#endregion

		/// <summary>
		/// 新增
		/// </summary>
		public void Add(XElement bodyElement)
		{
			Update(bodyElement);
		}
		/// <summary>
		/// 更新
		/// </summary>
		public void Update(XElement bodyElement)
		{
			InsertMessageDbLog(bodyElement);

			GoodsSummary goods = GetEntity(bodyElement);

			if (goods != null)
			{
				try
				{
					bool isSuccess = DMaiGoods.UpdateMaiGoods(goods);
				}
				catch (Exception ex)
				{
					Log.WriteErrorLog("更新商品操作失败：msgxml：[" +
						((bodyElement != null && bodyElement.Document != null) ? bodyElement.Document.ToString() : string.Empty) +
						"];\r\nerrormsg:[" + ex.ToString() + "]");
				}
			}
			else
			{
				Log.WriteLog("没有获取到数据model：msgxml：" +
					((bodyElement != null && bodyElement.Document != null) ? bodyElement.Document.ToString() : string.Empty));
			}
		}
		/// <summary>
		/// 在售
		/// </summary>
		public void StartSale(XElement bodyElement)
		{
			InsertMessageDbLog(bodyElement);

			UpdateGoodsStatus(bodyElement, 1);
		}
		/// <summary>
		/// 停售
		/// </summary>
		public void StopSale(XElement bodyElement)
		{
			InsertMessageDbLog(bodyElement);

			UpdateGoodsStatus(bodyElement, 2);
		}
		private void UpdateGoodsStatus(XElement bodyElement, Int16 status)
		{
			string guId = GetBodyGuid(bodyElement);
			if (!string.IsNullOrWhiteSpace(guId))
			{
				DMaiGoods.UpdateGoodsSummaryGoodsStatus(Guid.Parse(guId), status);
			}
		}
		public static GoodsSummary GetEntity(XElement bodyElement)
		{
			string guid = GetBodyGuid(bodyElement);
			int appId = ConvertHelper.GetInteger(GetMessageAppId(bodyElement));
			if (string.IsNullOrWhiteSpace(guid))
			{
				Log.WriteLog("不包含EntityId：msgxml：" +
					((bodyElement != null && bodyElement.Document != null) ? bodyElement.Document.ToString() : string.Empty));
				return null;
			}

			XDocument entityXml = null;
			//易车惠 AppId区分 易湃  add by sk 2013.11.08
			if (appId == 11)
				entityXml = GetEntityXml(guid);
			else if (appId == 12)
			{
				// 易湃数据则消息直接入库
			}
			if (entityXml == null)
			{
				Log.WriteLog("未从接口中获取到数据实体xml：msgxml：" +
					((bodyElement != null && bodyElement.Document != null) ? bodyElement.Document.ToString() : string.Empty));
				return null;
			}

			GoodsSummary goods = null;

			try
			{
				goods = GoodsSummary.GetGoodsSummary(entityXml);
				if (goods == null)
				{
					Log.WriteLog("未从接口中获取到数据实体xml：msgxml：" +
						((bodyElement != null && bodyElement.Document != null) ? bodyElement.Document.ToString() : string.Empty));
					return null;
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("error!从xml实体转到model时出现异常：msgxml：[" +
					((bodyElement != null && bodyElement.Document != null) ? bodyElement.Document.ToString() : string.Empty) +
					"];\r\nentityXml:[" + entityXml.ToString() +
					"];\r\nerrormsg:[" + ex.ToString() + "]"
					);
				return null;
			}

			goods.GoodsGUID = Guid.Parse(guid);
			goods.AppId = appId;
			return goods;
		}

		public static XDocument GetEntityXml(string guid)
		{
			com.bitauto.mai.api.maiservice.MaiService service = null;
			try
			{
				service = new com.bitauto.mai.api.maiservice.MaiService();
				string xEleStr = service.GetGoodsInfByGUID(guid);
				if (!string.IsNullOrWhiteSpace(xEleStr))
				{
					return XDocument.Parse(xEleStr);
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("获取商品数据实体异常!msg:" + ex.ToString());
				return null;
			}
			finally
			{
				if (service != null)
					service.Dispose();
			}
		}
		private static string GetBodyGuid(XElement bodyElement)
		{
			if (bodyElement != null)
			{
				var ele = bodyElement.Element("EntityId");
				if (ele != null)
				{
					return ele.Value;
				}
			}
			return null;
		}

		private static string GetMessageAppId(XElement bodyElement)
		{
			if (bodyElement != null)
			{
				var ele = bodyElement.Parent.Element("Header").Element("AppId");
				if (ele != null)
				{
					return ele.Value;
				}
			}
			return null;
		}

		private static void InsertMessageDbLog(XElement bodyElement)
		{
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.LoadXml(bodyElement.Parent.ToString());
				CommonFunction.InsertContentMsgBodyLogDB(new Common.Model.ContentMessage()
				{
					From = "BAA",
					ContentType = "YiCheHui",
					IsDelete = false,
					UpdateTime = DateTime.Now,
					ContentId = 0,
					ContentBody = xDoc
				}
				, Guid.Parse(xDoc.SelectSingleNode("/Messages/Body/EntityId").InnerText));
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("写消息数据库日志出现异常！：msgxml：[" +
					((bodyElement != null) ? bodyElement.ToString() : string.Empty) +
					"];\r\nerrormsg:[" + ex.ToString() + "]");
			}
		}
	}
}
