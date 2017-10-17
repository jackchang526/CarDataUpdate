using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BitAuto.CarDataUpdate.Common;
using System.Data;
using BitAuto.Utils.Data;
using System.Data.SqlClient;
using BitAuto.Utils;
using System.IO;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.dealer.api.ReplaceNews;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 二手车置换信息
	/// </summary>
	public class CarReplacementInfo
	{
		//public event LogHandler Log;
		//private string _baseDir;

		//public CarReplacementInfo()
		//{
		//	_baseDir = Path.Combine(CommonData.CommonSettings.SavePath, "CarReplacementInfo");
		//	if (!Directory.Exists(_baseDir))
		//		Directory.CreateDirectory(_baseDir);
		//}

		//#region 更新二手车置换信息,参数子品牌id，传0则跑全部数据
		///// <summary>
		///// 更新二手车置换信息,参数子品牌id，传0则跑全部数据
		///// </summary>
		//public void UpdateInfo(int serialId)
		//{
		//	string autoDataXmlPath = Path.Combine(CommonData.CommonSettings.SavePath, "autodata.xml");
		//	if (!File.Exists(autoDataXmlPath))
		//	{
		//		OnLog("error autodata.xml 不存在！ msg：" + autoDataXmlPath, true);
		//		return;
		//	}
		//	string serialXmlPath = Path.Combine(_baseDir, "serial.xml");
		//	XmlNodeList serials = null;
		//	XmlDocument autoDataXml = CommonFunction.GetXmlDocument(autoDataXmlPath);
		//	XmlDocument serialXml = null;
		//	if (serialId < 1)
		//	{
		//		serials = autoDataXml.SelectNodes("Params/MasterBrand/Brand/Serial");
		//		serialXml = autoDataXml;
		//	}
		//	else
		//	{
		//		if (File.Exists(serialXmlPath))
		//		{
		//			serialXml = CommonFunction.GetXmlDocument(serialXmlPath);
		//		}
		//		else
		//		{
		//			serialXml = autoDataXml;
		//		}
		//		serials = serialXml.SelectNodes("Params/MasterBrand/Brand/Serial[@ID=" + serialId.ToString() + "]");
		//		if (serials.Count < 1)
		//		{
		//			OnLog("error 子品牌id不存在或不符合条件，条件为在销或待销！ serialId:" + serialId.ToString(), true);
		//			return;
		//		}
		//	}

		//	XmlReader reader = null;
		//	XmlElement serialElem = null;
		//	foreach (XmlNode autoSerial in serials)
		//	{
		//		try
		//		{
		//			int csId = ConvertHelper.GetInteger(autoSerial.Attributes["ID"].Value);
		//			OnLog("更新置换优惠信息！csid:" + csId, true);
		//			reader = XmlReader.Create(string.Format(CommonData.CommonSettings.GetCarReplacementUrl, csId));
		//			int num = UpdateData(csId, reader);

		//			#region Data\CarReplacementInfo\serial.xml
		//			serialElem = (XmlElement)serialXml.SelectSingleNode(string.Format("//Serial[@ID='{0}']", csId.ToString()));
		//			if (num > 0)
		//			{
		//				serialElem.SetAttribute("ZhiHuan", "1");
		//			}
		//			else
		//			{
		//				serialElem.SetAttribute("ZhiHuan", "0");
		//			} 
		//			#endregion
		//			OnLog("更新完成！", true);
		//		}
		//		finally
		//		{
		//			if (reader != null)
		//				reader.Close();
		//		}
		//	}

		//	CommonFunction.SaveXMLDocument(serialXml, serialXmlPath); 

		//}

		//private int UpdateData(int csId, XmlReader reader)
		//{
		//	DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text,
		//		"SELECT * FROM CarReplacementInfo WHERE SerialId = @SerialId",
		//		new SqlParameter("@SerialId", csId));
		//	if (ds == null || ds.Tables.Count < 0) { OnLog("未从数据库中读取到数据 csid:" + csId.ToString(), true); return 0; }

		//	DataTable dt = ds.Tables[0];
		//	int cityId;
		//	string samePrivilege = null, diffPrivilege = null, memo = null;
		//	string selectFormat = "CityId={0}";
		//	DataRow row=null;
		//	while(reader.ReadToFollowing("ReplacementInfo"))
		//	{
		//		cityId = ConvertHelper.GetInteger(reader["AreaId"]);
		//		if (cityId < 1) continue;

		//		samePrivilege = string.Empty;
		//		diffPrivilege = string.Empty;
		//		memo = string.Empty;
				
		//		using (XmlReader subReader = reader.ReadSubtree())
		//		{
		//			while (subReader.Read())
		//			{
		//				if (subReader.NodeType != XmlNodeType.Element) continue;
		//				switch (subReader.Name.ToLower())
		//				{
		//					case "samebrandprivilege": //同品牌置换优惠信息
		//						samePrivilege = subReader.ReadString(); 
		//						break;
		//					case "differentbrandprivilege"://非同品牌置换优惠信息
		//						diffPrivilege = subReader.ReadString(); 
		//						break;
		//					case "memo":
		//						memo = subReader.ReadString(); //备注
		//						break;
		//				}
		//			}
		//		}

		//		DataRow[] rows = dt.Select(string.Format(selectFormat, cityId));
		//		if (rows.Length > 0)
		//		{
		//			row = rows[0];
		//		}
		//		else
		//		{
		//			row = dt.NewRow();
		//			row["SerialId"] = csId;
		//			row["CityId"] = cityId;
		//			dt.Rows.Add(row);
		//		}
		//		row["SameBrandPrivilege"] = samePrivilege;
		//		row["DiffBrandPrivilege"] = diffPrivilege;
		//		row["Memo"] = memo;
		//	}
		//	DataRow[] delRows = dt.Select(string.Empty, string.Empty, DataViewRowState.Unchanged);
		//	if (delRows.Length >0)
		//	{
		//		for (int i = delRows.Length - 1; i >= 0; i--)
		//		{
		//			delRows[i].Delete();
		//		}
		//	}

		//	if (ds.HasChanges())
		//	{
		//		SqlConnection conn = null;
		//		try
		//		{
		//			conn = new SqlConnection(CommonData.ConnectionStringSettings.CarDataUpdateConnString);

		//			SqlCommand insert = new SqlCommand("INSERT INTO [CarReplacementInfo]([SerialId],[CityId],[SameBrandPrivilege],[DiffBrandPrivilege],[Memo]) VALUES(@SerialId,@CityId,@SameBrandPrivilege,@DiffBrandPrivilege,@Memo)", conn);
		//			insert.Parameters.Add("@SerialId", SqlDbType.Int, 4, "SerialId");
		//			insert.Parameters.Add("@CityId", SqlDbType.Int, 4, "CityId");
		//			insert.Parameters.Add("@SameBrandPrivilege", SqlDbType.VarChar, 100, "SameBrandPrivilege");
		//			insert.Parameters.Add("@DiffBrandPrivilege", SqlDbType.VarChar, 100, "DiffBrandPrivilege");
		//			insert.Parameters.Add("@Memo", SqlDbType.VarChar, -1, "Memo");

		//			SqlCommand update = new SqlCommand("UPDATE [CarReplacementInfo] SET [SameBrandPrivilege] = @SameBrandPrivilege, [DiffBrandPrivilege] = @DiffBrandPrivilege, [Memo]=@Memo WHERE Id=@Id", conn);
		//			update.Parameters.Add("@Id", SqlDbType.Int, 4, "Id");
		//			update.Parameters.Add("@SameBrandPrivilege", SqlDbType.VarChar, 100, "SameBrandPrivilege");
		//			update.Parameters.Add("@DiffBrandPrivilege", SqlDbType.VarChar, 100, "DiffBrandPrivilege");
		//			update.Parameters.Add("@Memo", SqlDbType.VarChar, -1, "Memo");

		//			SqlCommand delete = new SqlCommand("DELETE [CarReplacementInfo] WHERE Id=@Id", conn);
		//			delete.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int, 4, "Id"));

		//			SqlDataAdapter sda = new SqlDataAdapter() { InsertCommand = insert, UpdateCommand = update, DeleteCommand = delete };
		//			conn.Open();
		//			sda.Update(dt);
		//		}
		//		catch(Exception exp)
		//		{
		//			OnLog("error，更新数据库失败：csid:" + csId + " errormsg:" + exp.Message, true);
		//		}
		//		finally
		//		{
		//			if (conn != null && conn.State != ConnectionState.Closed)
		//				conn.Close();
		//		}

		//		dt.AcceptChanges();

		//		return dt.Rows.Count;
		//	}
		//	return 0;
		//} 

		//#endregion

		//#region 缓存置换品牌列表
		///// <summary>
		///// 缓存置换品牌列表
		///// </summary>
		//public void GetZhiHuanBrandList()
		//{
		//	CommonFunction.SaveXMLDocument(CommonData.CommonSettings.GetCarReplacementBrandUrl, Path.Combine(_baseDir, "brand.xml"));
		//}
		//#endregion

		//#region 经销商置换行情
		///// <summary>
		///// 获取经销商置换行情
		///// </summary>
		//public void GetDealerNews(int brandId)
		//{
		//	string filepath = Path.Combine(_baseDir, "brand.xml");
		//	if (!File.Exists(filepath))
		//	{
		//		OnLog("error 置换品牌列表xml不存在 msg:" + filepath, true);
		//		return;
		//	}

		//	XmlDocument doc = CommonFunction.GetXmlDocument(filepath);
		//	if(doc==null)
		//	{
		//		OnLog("error 加载置换品牌列表xml失败 msg:" + filepath, true);
		//		return;
		//	}
		//	OnLog("加载置换品牌列表xml成功 msg:" + filepath, true);

		//	XmlNodeList nodes = null;
		//	if (brandId > 0)
		//	{
		//		nodes = doc.SelectNodes("//ReplacementBrand[@Id='" + brandId.ToString() + "'][1]");
		//		if (nodes.Count < 1)
		//		{
		//			OnLog("指定品牌id不存在于置换品牌列表xml中 brandId:" + brandId.ToString() + " msg:" + filepath, true);
		//			return;
		//		}
		//	}
		//	else
		//	{
		//		nodes = doc.SelectNodes("//ReplacementBrand");
		//		if (nodes.Count < 1)
		//		{
		//			OnLog("置换品牌列表xml没有ReplacementBrand节点 msg:" + filepath, true);
		//			return;
		//		}
		//	}

		//	int curBrandId;
		//	foreach (XmlNode node in nodes)
		//	{
		//		//<ReplacementBrand Id="20008"/>
		//		curBrandId = ConvertHelper.GetInteger(node.Attributes["Id"].Value);
		//		OnLog("开始更新品牌经销商行情 brandId:" + curBrandId.ToString(), true);
		//		if (curBrandId > 0)
		//		{
		//			UpdateDealerNews(curBrandId);
		//		}
		//		else
		//		{
		//			OnLog("品牌id小于0 brandId:" + curBrandId.ToString(), true);
		//			continue;
		//		}
		//		OnLog("结束更新品牌经销商行情 brandId:" + curBrandId.ToString(), true);
		//	}
		//}
		///// <summary>
		///// 获取经销商置换行情
		///// </summary>
		//private void UpdateDealerNews(int brandId)
		//{
		//	OnLog("删除现有数据 brandId:" + brandId.ToString(), true);
		//	int count = SqlHelper.ExecuteNonQuery(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, "delete DealerReplaceNews where brandid=@brandid", new SqlParameter("@brandid", brandId));
		//	OnLog("影响行数" + count.ToString(), true);

		//	OnLog("从接口中获取数据...", true);
		//	DasSoapHeader soapHeader = new DasSoapHeader();
		//	soapHeader.TokenKey = new Guid("c69affd8-098f-4657-87c8-735a0c7f6d2d");
		//	soapHeader.AuthorizeCode = "asdf!@#$";
		//	ReplaceNews manage = new ReplaceNews();
		//	manage.DasSoapHeaderValue = soapHeader;
		//	DataTable dt = manage.GetNewsListBySerialID(brandId);
		//	if (dt == null || dt.Rows.Count < 1)
		//	{
		//		OnLog("未获取到数据...", true);
		//		return;
		//	}

		//	OnLog("更新数据库数据...", true);
		//	SqlConnection conn = null;
		//	try
		//	{
		//		conn = new SqlConnection(CommonData.ConnectionStringSettings.CarDataUpdateConnString);
		//		SqlCommand comm = conn.CreateCommand();
		//		comm.CommandText = "INSERT INTO [DealerReplaceNews]([BrandId],[CityId],[NewsId],[NewsTitle],[NewsUrl],[PublishTime]) VALUES(@BrandId,@CityId,@NewsId,@NewsTitle,@NewsUrl,@PublishTime)";
		//		comm.CommandType = CommandType.Text;

		//		comm.Parameters.Add(new SqlParameter("@BrandId", SqlDbType.Int, 4)).Value = brandId;

		//		SqlParameter cityIdParam = new SqlParameter("@CityId", SqlDbType.Int, 4);
		//		SqlParameter newsIdParam = new SqlParameter("@NewsId", SqlDbType.Int, 4);
		//		SqlParameter titleParam = new SqlParameter("@NewsTitle", SqlDbType.VarChar, 255);
		//		SqlParameter urlParam = new SqlParameter("@NewsUrl", SqlDbType.VarChar, 255);
		//		SqlParameter dateParam = new SqlParameter("@PublishTime", SqlDbType.DateTime);
				
		//		comm.Parameters.Add(cityIdParam);
		//		comm.Parameters.Add(newsIdParam);
		//		comm.Parameters.Add(titleParam);
		//		comm.Parameters.Add(urlParam);
		//		comm.Parameters.Add(dateParam);

		//		conn.Open();
		//		//int saveCount = 0,tempCityId = -1;
		//		int cityId;
		//		//foreach (DataRow newsRow in dt.Select(string.Empty, "cityID asc,NewsPubTime desc"))
		//		foreach (DataRow newsRow in dt.Rows)
		//		{
		//			cityId = ConvertHelper.GetInteger(newsRow["cityID"]);
		//			//if (tempCityId != cityId)
		//			//{
		//			//    tempCityId = cityId;
		//			//    //saveCount = 0;
		//			//}
		//			//if ((++saveCount) > 3)
		//			//    continue;

		//			cityIdParam.Value=cityId;
		//			newsIdParam.Value=ConvertHelper.GetInteger(newsRow["NewsId"]);
		//			titleParam.Value=newsRow["NewsTitle"].ToString();
		//			urlParam.Value=newsRow["NewsUrl"].ToString();
		//			dateParam.Value = ConvertHelper.GetDateTime(newsRow["NewsPubTime"].ToString());

		//			comm.ExecuteNonQuery();
		//		}
		//		OnLog("更新数据库数据 完成...", true);
		//	}
		//	catch (Exception exp)
		//	{
		//		OnLog("error 更新数据时出现异常 msg:" + exp.Message, true);
		//		Common.Log.WriteErrorLog(exp);
		//	}
		//	finally
		//	{
		//		if (conn != null && conn.State != ConnectionState.Closed)
		//			conn.Close();
		//	}
		//}
		//#endregion

		///// <summary>
		///// 写Log
		///// </summary>
		///// <param name="logText"></param>
		//public void OnLog(string logText, bool nextLine)
		//{
		//	if (Log != null)
		//		Log(this, new LogArgs(logText, nextLine));
		//}
	}
}
