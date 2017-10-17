using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using System.Xml.Linq;
using System.Xml;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using BitAuto.Utils;
using BitAuto.Utils.Data;
using System.Net;
using System.Web;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 向晶赞推送子品牌相关数据
	/// </summary>
	public class SerialDataToJingZan
	{
		public void PostSerialAllDataToJingZan(int actionType)
		{
			Log.WriteLog(string.Format("start PostSerialAllDataToJingZan msg:[actionType:{0}]", actionType));
			string dataXmlPath = Path.Combine(CommonData.CommonSettings.SavePath, "MasterToBrandToSerialAllSaleAndLevel.xml");
			Log.WriteLog("load xml:" + dataXmlPath);
			try
			{
				XDocument doc = null;
				using (XmlReader reader = XmlReader.Create(dataXmlPath))
				{
					doc = XDocument.Load(reader);
				}
				Log.WriteLog("load xml succeed.");

				foreach (var serialNode in doc.Descendants("Serial"))
				{
					int csId = ConvertHelper.GetInteger(serialNode.Attribute("ID").Value);
					PostSerialDataToJingZan(actionType, csId);
				}
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(exp);
			}
			Log.WriteLog(string.Format("end PostSerialAllDataToJingZan msg:[actionType:{0}]", actionType));
		}

		/// <summary>
		/// 向晶赞推送子品牌相关数据
		/// </summary>
		/// <param name="actionType">
		/// 需求：1: 新增，2: 修改属性(但是不需要重新下载图片)，3: 修改素材图片，需要重新下载，4：删除
		/// 实现：1=子品牌新增消息；2=子品牌信息更新消息；3=图片消息；4=子品牌删除消息
		/// </param>
		/// <param name="csId">子品牌id</param>
		public void PostSerialDataToJingZan(int actionType, int csId)
		{

			if (string.IsNullOrEmpty(CommonData.CommonSettings.PostSerialDataToJingZanUrl))
			{
				Log.WriteLog(string.Format("clear PostSerialDataToJingZan, config:PostSerialDataToJingZanUrl is empty. msg:[actionType:{0}, csId:{1}]", actionType, csId));
				return;
			}

			Log.WriteLog(string.Format("start PostSerialDataToJingZan msg:[actionType:{0}, csId:{1}]", actionType, csId));

			XElement infoElem = new XElement(new XElement("product"
						, new XElement("status", actionType)
						, new XElement("id", csId)
						)
					);
			/*
			1、厂家指导价 (guidePrice)
			2、排量(emission)
			3、变速箱(gearbox)
			4、产地(origin)
			5、国别(country)
			* 以上数据暂时不提供，代码以注释。
			*/
			XDocument dataDoc = new XDocument(
				new XDeclaration("1.0", "utf-8", null)
				, new XElement("products"
					, infoElem
					)
				);

			if (actionType == 4) //actionType = 4 删除
			{
			}
			else //actionType = 其他值
			{
				Log.WriteLog("start 生成推送xml数据...");

				XmlDocument xmlDoc;
				XmlNode serialElem;
				string comparisonCsIds //竞品子品牌ids
					//, engine_Exhaust //排量
					//, transmissionTypes //变速箱
					, jiangjiaPrice//降价金额
					, photoImageUrl = string.Empty//白底图(6)或普通封面图(4)，普通封面图不一定是300*200
					//, refPrice = string.Empty
					, price = string.Empty;

				#region serialElem 国别、产地、价格、name等。可能会return
				xmlDoc = CommonFunction.GetXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, "MasterToBrandToSerialAllSaleAndLevel.xml"));
				if (xmlDoc == null)
				{
					Log.WriteLog("未成功加载MasterToBrandToSerialAllSaleAndLevel.xml");
					return;
				}

				serialElem = xmlDoc.SelectSingleNode(string.Format("Params/MasterBrand/Brand/Serial[@ID='{0}']", csId));
				if (serialElem == null)
				{
					Log.WriteLog("子品牌未找到msg:" + csId);
					return;
				}

				// add by chengl Jul.15.2014 只发送在销子品牌数据
				if (serialElem.Attributes["CsSaleState"].Value != "在销")
				{
					Log.WriteLog("子品牌非在销msg:" + csId);
					return;
				}
				#endregion

				#region photoImageUrl //白底图(6)或普通封面图(4)，普通封面图不一定是300*200
				//xmlDoc = CommonFunction.GetXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, "photoimage\\SerialCoverWithout.xml"));
				//if (xmlDoc == null)
				//{
				//	Log.WriteLog("未成功加载photoimage\\SerialCoverWithout.xml");
				//}
				xmlDoc = CommonFunction.GetXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, "photoimage\\SerialCoverImageAndCount.xml"));
				if (xmlDoc == null)
				{
					Log.WriteLog("未成功加载photoimage\\SerialCoverImageAndCount.xml");
				}


				XmlNode photoNode = xmlDoc.SelectSingleNode(string.Format("SerialList/Serial[@SerialId='{0}']", csId));
				if (photoNode != null)
				{
					if (photoNode.Attributes["ImageUrl2"] != null && !string.IsNullOrEmpty(photoNode.Attributes["ImageUrl2"].Value))
					{
						photoImageUrl = string.Format(photoNode.Attributes["ImageUrl2"].Value, "6");
					}
					else if (photoNode.Attributes["ImageUrl"] != null && !string.IsNullOrEmpty(photoNode.Attributes["ImageUrl"].Value))
					{
						photoImageUrl = string.Format(photoNode.Attributes["ImageUrl"].Value, "4");
					}
				}
				#endregion

				SqlParameter csSqlParam = new SqlParameter("@CsId", csId);

				#region comparisonCsIds、engine_Exhaust、transmissionTypes

				DataSet carDataSet = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text
					, @"select top(10) PCs_Id
from Serial_To_Serial
where CS_Id=@CsId
order by Pv_Num desc;"
					//select cei.Engine_Exhaust,cei.UnderPan_TransmissionType  
					//from dbo.Car_Basic car  
					//left join dbo.Car_Extend_Item cei on car.car_id = cei.car_id  
					//left join Car_serial cs on car.cs_id = cs.cs_id  
					//where car.isState=1 and cs.isState=1 and car.Car_SaleState<>'停销' and car.cs_id=@CsId order by Engine_Exhaust;"
					, csSqlParam);

				DataRowCollection rows = null;

				#region comparisonCsIds
				rows = carDataSet.Tables[0].Rows;
				List<int> csids = new List<int>(rows.Count);
				foreach (DataRow row in rows)
				{
					int compCsId = ConvertHelper.GetInteger(row["PCs_Id"].ToString());
					if (compCsId > 0 && !csids.Contains(compCsId))
					{
						csids.Add(compCsId);
					}
				}
				comparisonCsIds = CommonFunction.joinList(csids);
				#endregion

				#region engine_Exhaust、transmissionTypes
				/*
                rows = carDataSet.Tables[1].Rows;

                //排量列表
                List<string> exhaustList = new List<string>();
                //变速箱列表
                List<string> transList = new List<string>();
                foreach (DataRow row in rows)
                {
                    string tempEx = row["Engine_Exhaust"].ToString().Trim();
                    if (!exhaustList.Contains(tempEx))
                    {
                        exhaustList.Add(tempEx);
                    }
                    string tempTransmission = row["UnderPan_TransmissionType"].ToString().Trim();
                    if (tempTransmission.IndexOf("挡") >= 0)
                    {
                        tempTransmission = tempTransmission.Substring(tempTransmission.IndexOf("挡") + 1, tempTransmission.Length - tempTransmission.IndexOf("挡") - 1);
                    }
                    tempTransmission = tempTransmission.Replace("变速器", string.Empty).Replace("CVT", string.Empty);
                    if (transList.Count < 2)
                    {
                        if (tempTransmission.IndexOf("手动") == -1)
                            tempTransmission = "自动";
                        if (!transList.Contains(tempTransmission))
                            transList.Add(tempTransmission);
                    }
                }

                engine_Exhaust = String.Join(" ", exhaustList.ToArray() );

                transmissionTypes = String.Join(" ", transList.ToArray());
*/
				#endregion
				#endregion

				#region jiangjiaPrice//降价金额

				object jiangjiaObj = SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text
					, "select MaxFavorablePrice from JiangJiaNewsSummary where serialid=@CsId and cityid=0"
					, csSqlParam);

				if (jiangjiaObj != null && !(jiangjiaObj is DBNull))
				{
					jiangjiaPrice = string.Format("直降{0}万", Convert.ToDecimal(jiangjiaObj).ToString("0.##"));
				}
				else
				{
					jiangjiaPrice = string.Empty;
				}

				#endregion

				#region 生成xml

				foreach (KeyValuePair<string, string> elem in new Dictionary<string, string>{
                    {"name",serialElem.Attributes["SerialSEOName"].Value}
                    ,{"image", photoImageUrl}
                    ,{"landingPage", string.Format("http://car.bitauto.com/{0}/?WT.mc_jz=chexing&sourceid=103", serialElem.Attributes["AllSpell"].Value)}
                    ,{"imageWidth", "300"}
                    ,{"imageHeight", "200"}
                    ,{"brand", serialElem.ParentNode.ParentNode.Attributes["Name"].Value}
                    ,{"category", serialElem.Attributes["CsLevel"].Value}
                    ,{"subCategory", serialElem.ParentNode.Attributes["Name"].Value}
                    ,{"thirdCategory", serialElem.Attributes["Name"].Value}
                })
				{
					infoElem.Add(new XElement(elem.Key, elem.Value));
				}

				XElement moreAttr = new XElement("more_attributes");

				//decimal refMinPrice = ConvertHelper.GetDecimal(serialElem.Attributes["MinRP"].Value)
				//    , refMaxPrice = ConvertHelper.GetDecimal(serialElem.Attributes["MaxRP"].Value);
				decimal minPrice = ConvertHelper.GetDecimal(serialElem.Attributes["MinP"].Value)
					, maxPrice = ConvertHelper.GetDecimal(serialElem.Attributes["MaxP"].Value);

				//if (refMinPrice > 0 && refMaxPrice > 0)
				//{
				//    refPrice = string.Format("{0}万-{1}万", refMinPrice.ToString("#0.00"), refMaxPrice.ToString("#0.00"));
				//}
				if (minPrice > 0 && maxPrice > 0)
				{
					price = string.Format("{0}万-{1}万", minPrice.ToString("#0.00"), maxPrice.ToString("#0.00"));
				}

				foreach (var moreData in new List<TempSerialData>()
                {
                    new TempSerialData(){key="competingModelsId",name="竞品车型ID",value=comparisonCsIds}
                    ,new TempSerialData(){key="cut_Price",name="降价",value=jiangjiaPrice,url=string.Format("http://car.bitauto.com/{0}/jiangjia/?WT.mc_jz=jiangjia&sourceid=103", serialElem.Attributes["AllSpell"].Value)}
                     ,new TempSerialData(){key="quotedPrice",name="商家报价",value=price,url=string.Format("http://car.bitauto.com/{0}/baojia/?WT.mc_jz=baojia&sourceid=103", serialElem.Attributes["AllSpell"].Value)}
					 ,new TempSerialData(){key="tijiaodealer",name="提交",value="0",url=string.Format("http://dealer.bitauto.com/zuidijia/nb{0}/?WT.mc_jz=xunjia&sourceid=103", serialElem.Attributes["ID"].Value)}

                    ////暂时不需要
                    //,new TempSerialData(){key="guidePrice",name="厂家指导价",value=refPrice}
                    //,new TempSerialData(){key="emission",name="排量",value=engine_Exhaust}
                    //,new TempSerialData(){key="gearbox",name="变速箱",value=transmissionTypes}
                    //,new TempSerialData(){key="origin",name="产地",value=serialElem.ParentNode.Attributes["Country"].Value}
                    //,new TempSerialData(){key="country",name="国别",value=serialElem.ParentNode.ParentNode.Attributes["Country"].Value}
                })
				{
					if (string.IsNullOrEmpty(moreData.value))//额外属性如果没有值，就不要将该节点记录进xml
						continue;

					XElement tempElm = new XElement("attribute"
						, new XElement("key", moreData.key)
						, new XElement("name", moreData.name)
						, new XElement("value", moreData.value)
						);
					if (!string.IsNullOrEmpty(moreData.url))
					{
						tempElm.Add(new XElement("url", moreData.url));
					}
					moreAttr.Add(tempElm);
				}
				if (moreAttr.HasElements)//额外属性如果子节点，就不要将该节点记录进xml
				{
					infoElem.Add(moreAttr);
				}

				#endregion

				Log.WriteLog("end 生成推送xml数据...");
			}

			try
			{
				byte[] postDataBytes = Encoding.UTF8.GetBytes(dataDoc.ToString(SaveOptions.DisableFormatting));
				string postUrl = string.Format(CommonData.CommonSettings.PostSerialDataToJingZanUrl, (actionType == 4 ? "1" : "0"));

				Log.WriteLog(string.Format("start post data msg:[url:{0},bytes:{1}]"
					, postUrl
					, postDataBytes.Length));

				byte[] resultBytes = null;
				using (WebClient clinet = new WebClient())
				{
					clinet.Encoding = Encoding.UTF8;
					// System.Net.ServicePointManager.Expect100Continue = false;
					resultBytes = clinet.UploadData(postUrl, WebRequestMethods.Http.Post, postDataBytes);
				}

				Log.WriteLog(string.Format("end post data msg:[{0}]"
					, (resultBytes == null || resultBytes.Length < 1 ? "没有数据返回" : Encoding.UTF8.GetString(resultBytes))));
			}
			catch (Exception exp)
			{
				Log.WriteErrorLog(exp);
			}
			Log.WriteLog(string.Format("end PostSerialDataToJingZan msg:[actionType:{0}, csId:{1}]", actionType, csId));
		}

		class TempSerialData
		{
			public string key;
			public string name;
			public string value;
			public string url;
		}
	}
}
