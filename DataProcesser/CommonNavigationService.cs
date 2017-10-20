using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.Utils.Data;
using System.Collections;
using System.IO;
using System.Xml;
using System.Data.SqlClient;
using BitAuto.Utils;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class CommonNavigationService
    {
        public static DataSet GetSerialAllInfoForCommonNavigation()
        {
            DataSet dsSerial = new DataSet();
            string sql = @"SELECT  cs.cs_id, cs.cs_name, cs.cs_ShowName, cs.cs_seoname,
                                    cs.allspell AS csAllSpell, cs.CsSaleState, cs.cs_CarLevel,
                                    bat.bitautoTestURL, cs.CsPurpose, cb.cb_Country AS Cp_Country, cb.cb_id, cb.cb_name,
                                    cb.allspell AS cbAllSpell, cmb.bs_Id, cmb.bs_Name, cmb.urlspell,csi.ReferPriceRange
                            FROM    Car_Serial cs
                                    LEFT JOIN Car_Brand cb ON cs.cb_id = cb.cb_id
                                    LEFT JOIN dbo.Car_MasterBrand_Rel cmbr ON cb.cb_id = cmbr.cb_id
                                    LEFT JOIN dbo.Car_MasterBrand cmb ON cmbr.bs_Id = cmb.bs_Id
                                    LEFT JOIN dbo.BitAutoTest bat ON cs.cs_id = bat.cs_id
                                    LEFT JOIN dbo.Car_Serial_Item csi ON csi.cs_Id= bat.cs_id
                            WHERE cs.isState = 1
                                    AND cb.isState = 1
                                    AND cmb.isState = 1";
            dsSerial = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
            return dsSerial;
        }
        /// <summary>
        /// 子品牌PV (UV)
        /// </summary>
        /// <returns></returns>
        public static Hashtable GetAllSerialPV()
        {
            Hashtable htPV = new Hashtable();
            // modified by chengl Nov.7.2012
            //			string sql = @"select t1.csID,t1.UVCount,cs.cs_CarLevel
            //								,row_number()over(partition by cs.cs_CarLevel 
            //								order by t1.UVCount desc)as rank
            //								from (
            //								select csID,sum(UVCount) as UVCount
            //								from StatisticSerialPVUVCity
            //								group by csID
            //								) t1
            //								left join Car_Serial cs on t1.csid=cs.cs_id ";
            string sql = @"SELECT  uv.cs_id AS csID, uv.UVCount, cs.cs_CarLevel,
									row_number() OVER ( PARTITION BY cs.cs_CarLevel ORDER BY uv.UVCount DESC )
									AS rank
							FROM    dbo.Car_Serial_30UV uv
									LEFT JOIN Car_Serial cs ON uv.cs_id = cs.cs_Id";

            DataSet ds = SqlHelper.ExecuteDataset(
                CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int csid = int.Parse(dr["csid"].ToString());
                    int rank = int.Parse(dr["rank"].ToString());
                    if (!htPV.ContainsKey(csid))
                    {
                        htPV.Add(csid, rank);
                    }
                }
            }
            return htPV;
        }
        /// <summary>
        /// 取子品牌按城市分PV排名
        /// </summary>
        /// <returns></returns>
        public static DataSet GetSerialCityPVByLevel()
        {
            DataSet dsPVCity = new DataSet();
            try
            {
                string fileName = Path.Combine(CommonData.CommonSettings.SavePath, "SerialCityPVXML.xml");
                if (File.Exists(fileName))
                {
                    dsPVCity.ReadXml(fileName);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dsPVCity;
        }
        /// <summary>
        /// 取有CTCC文章的子品牌
        /// </summary>
        /// <returns></returns>
        public static DataSet GetAllSerialCTCCURL()
        {
            DataSet ds = new DataSet();
            try
            {
                string sql = " select csid,url from dbo.RainbowEdit where RainbowItemID = 59 ";
                ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return ds;
        }
        /// <summary>
        /// 取车展子品牌
        /// </summary>
        /// <param name="exhibitionID">车展ID</param>
        /// <returns></returns>
        public static Hashtable GetSerialByExhibitionID(int exhibitionID)
        {
            Hashtable ht = new Hashtable();

            string sql = string.Format("select ExhibitionID,RelationCar from Exhibition_Relation_Car as erc , (select ID from CarExhibition where ID = {0} or parentID = {0}) as r where erc.ExhibitionID = r.ID", exhibitionID.ToString());
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                XmlDocument xml = new XmlDocument();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    try
                    {
                        xml.LoadXml("<root>" + ds.Tables[0].Rows[i]["RelationCar"].ToString() + "</root>");
                    }
                    catch (Exception ex)
                    {
                        Log.WriteErrorLog(ex.Message + ex.StackTrace);
                    }
                    if (xml != null && xml.HasChildNodes)
                    {
                        XmlNodeList xnl = xml.SelectNodes("root/MasterBrand/Serial");
                        if (xnl != null && xnl.Count > 0)
                        {
                            foreach (XmlNode xn in xnl)
                            {
                                if (!ht.ContainsKey(xn.Attributes["ID"].Value.ToString().Trim()))
                                {
                                    ht.Add(xn.Attributes["ID"].Value.ToString().Trim(), 1);
                                }
                            }
                        }
                    }

                }
            }
            return ht;
        }
        /// <summary>
        /// 取所有子品牌各个新闻分类文章数量
        /// </summary>
        /// <returns></returns>
        public static XmlDocument GetAllSerialNewsCategoryCount()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                string fileName = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\newsNum.xml");
                if (File.Exists(fileName))
                {
                    doc.Load(fileName);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return doc;
        }

        public static bool HasSerialNews(int serialId)
        {
            bool result = true;
            try
            {
                string sql = @"SELECT COUNT(1)
                                 FROM   dbo.Car_SerialNewsV2 sn
                                 WHERE  sn.SerialId = @SerialId
                                        AND sn.CopyRight = 0
                                        AND CategoryId IN (
                                        SELECT  CmsCategoryId
                                        FROM    [dbo].[CarNewsTypeDef]
                                        WHERE   CarNewsTypeId IN ( 1, 2, 3, 4, 8, 9, 10,11, 13 ))";
                SqlParameter[] parameters = { new SqlParameter("@SerialId", SqlDbType.Int) };
                parameters[0].Value = serialId;
                result = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, parameters)) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
            return result;
        }
        /// <summary>
        /// 所有车系是否有新闻
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, bool> HasAllSerialNews()
        {
            Dictionary<int, bool> dict = new Dictionary<int, bool>();
            try
            {
                foreach (int serialId in CommonData.SerialDic.Keys)
                {
                    string sql = @"SELECT COUNT(1)
                                 FROM   dbo.Car_SerialNewsV2 sn
                                 WHERE  sn.SerialId = @SerialId
                                        AND sn.CopyRight = 0
                                        AND CategoryId IN (
                                        SELECT  CmsCategoryId
                                        FROM    [dbo].[CarNewsTypeDef]
                                        WHERE   CarNewsTypeId IN ( 1, 2, 3, 4, 8, 9, 10,11, 13 ))";
                    SqlParameter[] parameters = { new SqlParameter("@SerialId", SqlDbType.Int) };
                    parameters[0].Value = serialId;
                    bool result = ConvertHelper.GetInteger(SqlHelper.ExecuteScalar(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql, parameters)) > 0 ? true : false;
                    if (!dict.ContainsKey(serialId))
                    {
                        dict[serialId] = result;
                    } 
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
            return dict;
        }
        /// <summary>
        /// 获取车型数据
        /// </summary>
        /// <returns></returns>
        public static DataSet GetCarAllInfoForCommonNavigation()
        {
            DataSet dsCar = new DataSet();
            try
            {
                string sql = " select car.car_id,car.car_name,cs.cs_id,car.car_YearType ";
                sql += " from dbo.Car_Basic car ";
                sql += "  left join car_serial cs on car.cs_id = cs.cs_id ";
                sql += " where car.isState=1 and cs.isState=1 order by car.car_id desc";
                dsCar = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dsCar;
        }
        /// <summary>
        /// 取所有子品牌年款
        /// </summary>
        /// <returns></returns>
        public static DataSet GetAllCarYearByCsID()
        {
            DataSet dsAllCarYear = new DataSet();
            try
            {
                string sql = " select car.cs_id,car.Car_YearType as caryear ";
                sql += " from dbo.Car_relation car ";
                sql += " left join Car_serial cs on car.cs_id = cs.cs_id ";
                sql += " where car.isState=0 and cs.isState=0 and car.car_SaleState<>96 and car.Car_YearType>0 ";
                sql += " group by car.cs_id,car.Car_YearType ";
                sql += " order by car.cs_id,car.Car_YearType desc ";
                dsAllCarYear = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dsAllCarYear;
        }
        /// <summary>
        /// 取所有有停销车型的子品牌
        /// </summary>
        /// <returns></returns>
        public static DataSet GetHasNoSaleSerial()
        {
            DataSet dsNoSale = new DataSet();
            try
            {
                string sql = " select car.cs_id ";
                sql += "from dbo.Car_relation car ";
                sql += "left join Car_serial cs on car.cs_id = cs.cs_id ";
                sql += "where car.isState=0 and cs.isState=0 and car.car_SaleState=96 ";
                sql += "group by car.cs_id ";
                dsNoSale = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dsNoSale;
        }
        /// <summary>
        /// 取所有有保养信息的子品牌
        /// </summary>
        /// <returns></returns>
        public static Hashtable GetAllSerialBaoYang()
        {
            Hashtable ht = new Hashtable();
            string dirPath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\Maintance\\Message\\");
            if (Directory.Exists(dirPath))
            {
                string[] fileNameList = Directory.GetFileSystemEntries(dirPath);
                foreach (string filename in fileNameList)
                {
                    int startIndex = filename.LastIndexOf('\\') + 1;
                    int ilength = filename.LastIndexOf('.');
                    string idString = filename.Substring(startIndex, ilength - startIndex);
                    if (!ht.ContainsKey(idString))
                    { ht.Add(idString, 0); }
                }
            }
            return ht;
        }
        ///// <summary>
        ///// 取所有有销量数据的子品牌
        ///// </summary>
        ///// <returns></returns>
        //public static Hashtable GetAllSerialProduceAndSell()
        //{
        //    Hashtable ht = new Hashtable();
        //    string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "ProduceAndSell\\BrandTree.xml");
        //    if (File.Exists(filePath))
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(filePath);
        //        XmlNodeList xnl = doc.SelectNodes("/root/Producer/Brand/Serial");
        //        if (xnl != null && xnl.Count > 0)
        //        {
        //            foreach (XmlNode xn in xnl)
        //            {
        //                if (!ht.ContainsKey(xn.Attributes["id"].Value.ToString()))
        //                {
        //                    ht.Add(xn.Attributes["id"].Value.ToString(), 0);
        //                }
        //            }
        //        }
        //    }
        //    return ht;
        //}

        /// <summary>
        /// 取所有有销量数据的子品牌 add by chengl Dec.26.2013
        /// 替换原有方法，数据源从表迁移至易湃接口
        /// </summary>
        /// <returns></returns>
        public static List<int> GetAllSerialSale()
        {
            List<int> listCsID = new List<int>();
            string filePath = Path.Combine(CommonData.CommonSettings.SavePath, "SaleCsIDList.xml");
            if (File.Exists(filePath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                XmlNodeList xnl = doc.SelectNodes("/Root/Cs");
                if (xnl != null && xnl.Count > 0)
                {
                    foreach (XmlNode xn in xnl)
                    {
                        if (!listCsID.Contains(int.Parse(xn.Attributes["ID"].Value.ToString())))
                        {
                            listCsID.Add(int.Parse(xn.Attributes["ID"].Value.ToString()));
                        }
                    }
                }
            }
            return listCsID;
        }

        /// <summary>
        /// 根据彩虹条ID
        /// </summary>
        /// <param name="rainbowItemID"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllSerialRainbowItemByRainbowItemID(int rainbowItemID)
        {
            Dictionary<int, string> dicRainbow = new Dictionary<int, string>();
            DataSet ds = new DataSet();
            string sql = " select csid,url from dbo.RainbowEdit where RainbowItemID =  " + rainbowItemID.ToString();
            ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int csid = int.Parse(dr["csid"].ToString());
                    string url = dr["url"].ToString().Trim();
                    if (!dicRainbow.ContainsKey(csid))
                    { dicRainbow.Add(csid, url); }
                }
            }
            return dicRainbow;
        }
        /// <summary>
        /// 获取子品牌车型详解
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetPingceInfo()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            string sql = "SELECT csid FROM CarPingceInfo cpi GROUP BY cpi.csid HAVING COUNT(csid)>0";
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql);
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                int csId = ConvertHelper.GetInteger(dr["csid"]);
                if (!dict.ContainsKey(csId))
                { dict.Add(csId, csId.ToString()); }
            }
            return dict;
        }
        /// <summary>
        /// 子品牌包含的停销车型的年款
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, List<int>> GetSerialNoSaleYear()
        {
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
            string sql = @"select car.cs_id,car.car_yeartype
								from car_basic car
								left join car_serial cs on car.cs_id=cs.cs_id
								where car.isState=1 and cs.isState=1
								and car.Car_SaleState='停销' and car.car_yeartype>0
								group by car.cs_id,car.car_yeartype
								order by car.cs_id,car.car_yeartype desc";
            DataSet ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int csid = int.Parse(dr["cs_id"].ToString());
                    int year = int.Parse(dr["car_yeartype"].ToString());
                    if (dic.ContainsKey(csid))
                    {
                        if (!dic[csid].Contains(year))
                        { dic[csid].Add(year); }
                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(year);
                        dic.Add(csid, list);
                    }
                }
            }
            ds.Clear();
            return dic;
        }
        /// <summary>
        /// 取停销子品牌的停销车型，按年款新到旧排序 排量升序
        /// </summary>
        /// <param name="csID">停销子品牌ID</param>
        /// <returns></returns>
        public static DataSet GetNoSaleCarListByCsID(int csID)
        {
            DataSet ds = new DataSet();
            string sql = @"select car.car_id,car.car_name,car.car_ReferPrice,car.Car_YearType,car.Car_ProduceState,car.Car_SaleState,cs.cs_id,cei.Engine_Exhaust,cei.UnderPan_TransmissionType  
								from dbo.Car_Basic car  
								left join dbo.Car_Extend_Item cei on car.car_id = cei.car_id  
								left join Car_serial cs on car.cs_id = cs.cs_id  
								where car.isState=1 and cs.isState=1 and car.cs_id=@csID and car.Car_SaleState='停销' and car.Car_YearType>0 order by cs.cs_id,car.Car_YearType desc,Engine_Exhaust";
            SqlParameter[] _param ={
                                      new SqlParameter("@csID",SqlDbType.Int)
                                  };
            _param[0].Value = csID;
            ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, _param);
            return ds;
        }
        /// <summary>
        /// 取在销子品牌的非停销车型 排量升序
        /// </summary>
        /// <param name="csID">在销子品牌ID</param>
        /// <returns></returns>
        public static DataSet GetSaleCarByCsID(int csID)
        {
            DataSet ds = new DataSet();
            string sql = @"select car.car_id,car.car_name,car.car_ReferPrice,car.Car_YearType,car.Car_ProduceState,car.Car_SaleState,cs.cs_id,cei.Engine_Exhaust,cei.UnderPan_TransmissionType  
									from dbo.Car_Basic car  
									left join dbo.Car_Extend_Item cei on car.car_id = cei.car_id  
									left join Car_serial cs on car.cs_id = cs.cs_id  
									where car.isState=1 and cs.isState=1 and car.Car_SaleState<>'停销' and car.cs_id=@csID order by Engine_Exhaust";
            SqlParameter[] _param ={
                                      new SqlParameter("@csID",SqlDbType.Int)
                                  };
            _param[0].Value = csID;
            ds = SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql, _param);
            return ds;
        }
        ///// <summary>
        ///// 获取每年的年试十佳车型
        ///// </summary>
        ///// <returns></returns>
        //public static Dictionary<int, List<int>> GetTop10CarDic()
        //{
        //    Dictionary<int, List<int>> topCarDic = new Dictionary<int, List<int>>();
        //    string xmlFile = Path.Combine(CommonData.CommonSettings.CommonHeadLocalFilePath, "Top10Car.xml");
        //    XmlDocument doc = new XmlDocument();
        //    try
        //    {
        //        doc.Load(xmlFile);
        //        XmlNodeList yearNodeList = doc.SelectNodes("/Root/Year");
        //        foreach (XmlElement yearNode in yearNodeList)
        //        {
        //            int year = 0;
        //            bool isYear = Int32.TryParse(yearNode.GetAttribute("id"), out year);
        //            if (!isYear || year == 0)
        //                continue;

        //            List<int> serialList = new List<int>(10);
        //            topCarDic[year] = serialList;
        //            //年度十佳车型
        //            XmlNodeList serialNodeList = yearNode.SelectNodes("Serial");

        //            foreach (XmlElement serialNode in serialNodeList)
        //            {
        //                int serialId = 0;
        //                bool isId = Int32.TryParse(serialNode.GetAttribute("id"), out serialId);
        //                if (!isId || serialId == 0)
        //                    continue;
        //                serialList.Add(serialId);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteErrorLog(ex.Message + ex.StackTrace);
        //    }
        //    return topCarDic;
        //}

        /// <summary>
        /// 年度10佳车 add by chengl 
        /// </summary>
        /// <returns></returns>
        public static List<Define.BestTopCar> GetAllBestTopCar()
        {
            List<Define.BestTopCar> list = new List<Define.BestTopCar>();
            string xmlFile = Path.Combine(CommonData.CommonSettings.CommonHeadLocalFilePath, "Top10Car.xml");
            XmlDocument doc = new XmlDocument();
            try
            {
                list = new List<Define.BestTopCar>();
                doc.Load(xmlFile);
                XmlNodeList adNodeList = doc.SelectNodes("/Root/TopCar");
                foreach (XmlElement adNode in adNodeList)
                {
                    Define.BestTopCar btc = new Define.BestTopCar();
                    btc.Title = "";
                    btc.Link = "";
                    btc.ListCsList = new List<int>();
                    btc.Title = adNode.GetAttribute("Title").ToString().Trim();
                    btc.Link = adNode.GetAttribute("Link").ToString().Trim();
                    XmlNodeList csNodeList = adNode.SelectNodes("Serial");
                    foreach (XmlElement csNode in csNodeList)
                    {
                        int csid = ConvertHelper.GetInteger(csNode.GetAttribute("id"));
                        if (!btc.ListCsList.Contains(csid))
                        { btc.ListCsList.Add(csid); }
                    }
                    list.Add(btc);
                }
            }
            catch (Exception ex)
            { Log.WriteErrorLog(ex.Message + ex.StackTrace); }
            return list;
        }

        /// <summary>
        /// 读取导航头搜索配置文件
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetSoBarByTag()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            string fileName = Path.Combine(CommonData.CommonSettings.CommonHeadLocalFilePath, "SoBarForNavigation.xml");
            if (File.Exists(fileName))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    if (doc != null && doc.HasChildNodes)
                    {
                        XmlNodeList xnl = doc.SelectNodes("/Root/SoBar");
                        if (xnl != null && xnl.Count > 0)
                        {
                            foreach (XmlNode xn in xnl)
                            {
                                string tags = xn.Attributes["TagIDs"].Value.ToString().Trim();
                                string soBar = xn.InnerText.ToString().Trim();
                                if (!string.IsNullOrEmpty(tags))
                                {
                                    string[] tagArr = tags.Split(',');
                                    foreach (string tag in tagArr)
                                    {
                                        int tagid = int.Parse(tag.Trim());
                                        if (!dic.ContainsKey(tagid))
                                        {
                                            dic.Add(tagid, soBar);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteErrorLog("读取导航头搜索配置文件：" + ex.Message + ex.StackTrace);
                }
            }
            return dic;
        }

        /// <summary>
        /// 读取导航头搜索配置文件
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetSoBarByTagV2()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            string fileName = Path.Combine(CommonData.CommonSettings.CommonHeadLocalFilePath, "NavgationTemplates/SoBarForNavigation.xml");
            if (File.Exists(fileName))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    if (doc != null && doc.HasChildNodes)
                    {
                        XmlNodeList xnl = doc.SelectNodes("/Root/SoBar");
                        if (xnl != null && xnl.Count > 0)
                        {
                            foreach (XmlNode xn in xnl)
                            {
                                string tags = xn.Attributes["TagIDs"].Value.ToString().Trim();
                                string soBar = xn.InnerText.ToString().Trim();
                                if (!string.IsNullOrEmpty(tags))
                                {
                                    string[] tagArr = tags.Split(',');
                                    foreach (string tag in tagArr)
                                    {
                                        int tagid = int.Parse(tag.Trim());
                                        if (!dic.ContainsKey(tagid))
                                        {
                                            dic.Add(tagid, soBar);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteErrorLog("读取导航头搜索配置文件：" + ex.Message + ex.StackTrace);
                }
            }
            return dic;
        }
        /*移到CommonData里
        /// <summary>
        /// 取子品牌报价区间(不分地区)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetSerialPriceRange()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            DataSet ds = GetAllSerialPriceRange();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {
                        string result = "";
                        int csid = int.Parse(dr["Id"].ToString());
                        decimal min = Math.Round(decimal.Parse(dr["MinPrice"].ToString()), 2);
                        decimal max = Math.Round(decimal.Parse(dr["MaxPrice"].ToString()), 2);
                        if (max > 1000)
                        {
                            result = min.ToString() + "万-" + Convert.ToInt16(max) + "万";
                        }
                        else
                        {
                            result = min.ToString() + "万-" + max.ToString() + "万";
                        }
                        if (csid > 0 && result != "")
                        {
                            if (!dic.ContainsKey(csid))
                            { dic.Add(csid, result); }
                        }
                    }
                    catch
                    { }
                }
            }
            return dic;
        }*/
        /// <summary>
        /// 取所有子品牌全国最高降幅
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllSerialJiangJia()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            string sql = @"SELECT SerialId, Num, MaxFavorablePrice FROM JiangJiaNewsSummary 
									WHERE CityId=0 and MaxFavorablePrice>0";
            DataSet ds = SqlHelper.ExecuteDataset(
                CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int csid = int.Parse(dr["SerialId"].ToString());
                    decimal maxP = decimal.Parse(dr["MaxFavorablePrice"].ToString());
                    if (!dic.ContainsKey(csid))
                    {
                        dic.Add(csid, maxP.ToString("F2") + "万");
                    }
                }
            }
            return dic;
        }
        /// <summary>
        /// 同时有国内和国外品牌的主品牌
        /// </summary>
        /// <returns></returns>
        public static List<int> GetAllMasterBrandCpCountry()
        {
            List<int> list = new List<int>();
            string sql = @"SELECT  cmbr.bs_id, cb.cb_Country AS CpCountry
                            FROM    dbo.Car_MasterBrand_Rel cmbr
                                    LEFT JOIN car_brand cb ON cmbr.cb_id = cb.cb_id
                            WHERE   cmbr.isState = 0
                                    AND cb.isState = 0
                            ORDER BY cmbr.bs_id, CpCountry";
            DataSet ds = SqlHelper.ExecuteDataset(
                CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                int bsid = 0;
                List<int> listTemp = new List<int>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (bsid != int.Parse(dr["bs_id"].ToString()))
                    {
                        // 厂商国别保护中国 并且也包含外国
                        if (listTemp.Contains(90) && listTemp.Count >= 2)
                        {
                            if (bsid > 0 && !list.Contains(bsid))
                            {
                                list.Add(bsid);
                            }
                        }
                        listTemp.Clear();
                    }
                    bsid = int.Parse(dr["bs_id"].ToString());
                    int cpCountry = ConvertHelper.GetInteger(dr["CpCountry"]);
                    if (cpCountry > 0 && !listTemp.Contains(cpCountry))
                    { listTemp.Add(cpCountry); }
                }
            }
            return list;
        }
        /* 移到CommonData里
        /// <summary>
        /// 取子品牌报价区间
        /// </summary>
        /// <returns></returns>
        public static DataSet GetAllSerialPriceRange()
        {
            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(CommonData.CommonSettings.PriceRangeInterface);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return ds;
        }
        */
        /// <summary>
        /// 取子品牌论坛地址
        /// </summary>
        /// <returns></returns>
        public static DataSet BBSAllUrl()
        {
            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(CommonData.CommonSettings.BBSAllUrl);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return ds;
        }
        /// <summary>
        /// 根据标签取标签面包削
        /// </summary>
        /// <param name="tagID"></param>
        /// <returns></returns>
        public static string GetTagNameByID(int tagID)
        {
            string tagName = "";
            switch (tagID)
            {
                case 0: tagName = ""; break;// 子品牌综述
                case 1: tagName = "参数"; break;
                case 2: tagName = "图片"; break;
                case 3: tagName = "视频"; break;
                case 4: tagName = "市场"; break;
                case 5: tagName = "口碑"; break;
                case 6: tagName = "问答"; break;
                case 7: tagName = "论坛"; break;
                case 8: tagName = "导购"; break;
                case 10: tagName = "安全"; break;
                case 11: tagName = "油耗"; break;
                case 12: tagName = "报价"; break;
                case 14: tagName = ""; break;// 车型综述
                case 15: tagName = "参数"; break;
                case 16: tagName = "报价"; break;
                case 17: tagName = "新闻"; break;
                case 18: tagName = "销量"; break;// 新闻
                case 19: tagName = "新闻"; break;// 新闻
                case 20: tagName = "行情"; break;
                case 21: tagName = "用车"; break;
                case 22: tagName = "详解"; break;
                // case 23: tagName = ""; break; // CMS不带面包削
                case 23: tagName = ""; break; // CMS不带面包削
                // case 24: tagName = "车展"; break; // 2009广州车展
                case 25: tagName = "上牌量地图"; break; // 上牌量地图
                case 26: tagName = "二手车"; break; // 二手车
                case 27: tagName = "排行榜"; break; // 子品牌排行榜
                case 28: tagName = "评测"; break; // 子品牌试驾
                case 29: tagName = "车展"; break; // 2010北京车展
                case 30: tagName = "图片"; break; // 车型图片
                case 31: tagName = "二手车"; break; // 二手车 车型
                case 32: tagName = "养护"; break; // 养护
                // case 33: tagName = "车展"; break; // 2010广州车展
                case 34: tagName = "改装"; break; // 改装
                case 35: tagName = "养车费用"; break; // 养车费用
                case 36: tagName = "置换"; break; // 置换
                case 37: tagName = "降价"; break; // 降价
                case 38: tagName = "降价"; break; // 降价
                case 39: tagName = "车贷"; break; // 车贷
                case 40: tagName = "车贷"; break; // 车贷
                case 41: tagName = "文章"; break; // 文章
                case 42: tagName = "科技"; break; // 科技
                case 43: tagName = "文化"; break; // 文化
                //case 1: tagName = ""; break;
                //case 1: tagName = ""; break;
                //case 1: tagName = ""; break;
                default: break;
            }
            return tagName;
        }
        /// <summary>
        /// 统计标签 取标签对应ID
        /// </summary>
        /// <param name="tagID"></param>
        /// <returns></returns>
        public static string GetCurrentTagForStat(int tagID)
        {
            string tagForStat = "0";
            // 综述
            if (tagID == 0)
            {
                tagForStat = "41";
            }
            else if (tagID == 14)
            {
                // 车型综述
                tagForStat = "56";
            }
            else if (tagID == 1)
            {
                // 参数
                tagForStat = "42";
            }
            else if (tagID == 15)
            {
                // 车型参数
                tagForStat = "57";
            }
            else if (tagID == 2)
            {
                // 图片
                tagForStat = "53";
            }
            else if (tagID == 3)
            {
                // 视频
                tagForStat = "43";
            }
            else if (tagID == 5)
            {
                // 口碑
                tagForStat = "45";
            }
            //else if (tagID == 19)
            //{
            //    // 新闻标签
            //    tagForStat = "44";
            //}
            else if (tagID == 6)
            {
                // 答疑
                tagForStat = "46";
            }
            else if (tagID == 7)
            {
                // 论坛
                tagForStat = "47";
            }
            else if (tagID == 8 || tagID == 19 || tagID == 20 || tagID == 21 || tagID == 22 || tagID == 28)
            {
                // 车型详解 导购 评测 试驾 用车 行情 新闻
                tagForStat = "48";
            }
            else if (tagID == 32)
            {
                // 维修养护
                tagForStat = "49";
            }
            else if (tagID == 11)
            {
                // 油耗
                tagForStat = "50";
            }
            else if (tagID == 12)
            {
                // 报价
                tagForStat = "51";
            }
            else if (tagID == 16)
            {
                // 车型报价
                tagForStat = "58";
            }
            else if (tagID == 13)
            {
                // 区域车型
                tagForStat = "52";
            }
            else if (tagID == 24 || tagID == 29 || tagID == 33)
            {
                // 车展(2009广州、2010北京、2010广州)
                tagForStat = "54";
            }
            else if (tagID == 26)
            {
                // Ucar
                tagForStat = "55";
            }
            else if (tagID == 30)
            {
                // 车型图片页
                tagForStat = "59";
            }
            else
            {
                tagForStat = "0";
            }
            return tagForStat;
        }
        /// <summary>
        /// 根据用途ID取用途名和用途URL
        /// </summary>
        /// <param name="iPurpose"></param>
        /// <param name="url"></param>
        /// <param name="purposeName"></param>
        public static void GetPurposeNameAndURL(int iPurpose, out string url, out string purposeName)
        {
            url = "";
            purposeName = "";
            switch (iPurpose)
            {
                case 460: url = "functionlist.html#yy"; purposeName = "越野"; break;
                case 461: url = "functionlist.html#ss"; purposeName = "时尚"; break;
                case 462: url = "functionlist.html#jy"; purposeName = "家用"; break;
                case 463: url = "functionlist.html#db"; purposeName = "代步"; break;
                case 464: url = "functionlist.html#xx"; purposeName = "休闲"; break;
                case 465: url = "functionlist.html#yd"; purposeName = "运动"; break;
                case 466: url = "functionlist.html#sw"; purposeName = "商务"; break;
                case 467: url = "functionlist.html#cr"; purposeName = "cross"; break;
                case 468: url = "functionlist.html#dgn"; purposeName = "多功能"; break;
                default: url = ""; purposeName = ""; break;
            }
        }
        /// <summary>
        /// 取级别对应链接
        /// </summary>
        /// <param name="leveName"></param>
        /// <returns></returns>
        public static string GetCsLevelSpell(string leveName)
        {
            string temp = "";
            switch (leveName)
            {
                case "微型车": temp = "weixingche"; break;
                case "小型车": temp = "xiaoxingche"; break;
                case "紧凑型车": temp = "jincouxingche"; break;
                case "中型车": temp = "zhongxingche"; break;
                case "中大型车": temp = "zhongdaxingche"; break;
                case "豪华车": temp = "haohuaxingche"; break;
                case "MPV": temp = "mpv"; break;
                case "SUV": temp = "suv"; break;
                case "跑车": temp = "paoche"; break;
                case "其它": temp = "qita"; break;
                case "面包车": temp = "mianbaoche"; break;
                case "皮卡": temp = "pika"; break;
                default: temp = ""; break;
            }
            return temp;
        }

        ///// <summary>
        ///// 取有车贷的子品牌列表
        ///// </summary>
        ///// <returns></returns>
        //public static List<int> GetCheDaiCsList()
        //{
        //    List<int> list = new List<int>();
        //    if (CommonData.CommonSettings.CheDaiCsList != "")
        //    {
        //        string[] arrayList = CommonData.CommonSettings.CheDaiCsList.Split(new char[] { ',' }
        //            , StringSplitOptions.RemoveEmptyEntries);
        //        if (arrayList.Length > 0)
        //        {
        //            foreach (string strID in arrayList)
        //            {
        //                int id = 0;
        //                if (int.TryParse(strID, out id))
        //                {
        //                    if (id > 0 && !list.Contains(id))
        //                    { list.Add(id); }
        //                }
        //            }
        //        }
        //    }
        //    return list;
        //}

        /// <summary>
        /// 取子品牌全国易车惠信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllSerialGoods()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            string sql = "SP_Mai_Goods_Summary_GetGoodsCsList";
            DataSet ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(
                CommonData.ConnectionStringSettings.CarDataUpdateConnString, CommandType.StoredProcedure, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int csid = int.Parse(dr["Cs_Id"].ToString());
                    string url = dr["GoodsUrl"].ToString().Trim();
                    if (!dic.ContainsKey(csid))
                    {
                        dic.Add(csid, url);
                    }
                }
            }
            return dic;
        }
        /// <summary>
        /// 获取 车系 改款上市时间
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, SerialMarketTimeEntity> GetSerialMarketTimeData()
        {
            Dictionary<int, SerialMarketTimeEntity> dict = new Dictionary<int, SerialMarketTimeEntity>();
            try
            {
                //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\SerialMarketTimeConfig.config");
                string filePath = CommonData.CommonSettings.SerialMarketTimeConfigPath;
                XDocument doc = XDocument.Load(filePath);
                foreach (XElement element in doc.Root.Elements("Item"))
                {
                    DateTime showTime = ConvertHelper.GetDateTime(element.Element("ShowTime").Value);
                    string marketTime = element.Element("MarketTime").Value;
                    if (CommonFunction.DateDiff("d", showTime, DateTime.Now) <= 0)
                    {
                        int serialId = ConvertHelper.GetInteger(element.Element("SerialId").Value);
                        string url = element.Element("Url").Value;
                        var entity = new SerialMarketTimeEntity() { MarketTime = marketTime, Url = url };
                        if (dict.ContainsKey(serialId))
                            dict[serialId] = entity;
                        else
                            dict.Add(serialId, entity);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
            return dict;
        }

        /// <summary>
        /// 根据车系ID获取车款列表
        /// </summary>
        /// <param name="serialId"></param>
        /// <returns></returns>
        public static List<TimeTagEntity> GetAllCarBySerialId(int serialId)
        {
       //     string sql = @"SELECT  car.car_id, car.car_name, car.car_ReferPrice, car.Car_YearType,
							//		car.Car_ProduceState, car.Car_SaleState, cs.cs_id, cei.Engine_Exhaust,
							//		cei.UnderPan_TransmissionType, ccp.PVSum AS Pv_SumNum, cs.cs_name,
							//		cs.allSpell,cei.Body_Type
							//FROM    dbo.Car_Basic car WITH ( NOLOCK )
							//		LEFT JOIN dbo.Car_Extend_Item cei WITH ( NOLOCK ) ON car.car_id = cei.car_id
							//		LEFT JOIN Car_serial cs WITH ( NOLOCK ) ON car.cs_id = cs.cs_id
							//		LEFT JOIN Car_Basic_PV ccp WITH ( NOLOCK ) ON car.Car_Id = ccp.CarId
							//WHERE   car.isState = 1
							//		AND cs.isState = 1
							//		AND cs.cs_Id = @serialId";

            string sql = @"select cr.cs_id,cr.car_id
                                ,convert(int,isnull(cdb1.Pvalue,0)) as mdyear 
								,convert(int,isnull(cdb2.Pvalue,0)) as mdmonth 
								,convert(int,isnull(cdb3.Pvalue,0)) as mdday 
								,cr.car_SaleState,cr.Car_YearType,cr.car_ReferPrice 
                                from dbo.Car_relation cr   
                                left join dbo.CarDataBase cdb1 on cdb1.carid = cr.car_id and cdb1.ParamId=385   
                                left join dbo.CarDataBase cdb2 on cdb2.carid = cr.car_id and cdb2.ParamId=384   
                                left join dbo.CarDataBase cdb3 on cdb3.carid = cr.car_id and cdb3.ParamId=383   
                                left join dbo.Car_Serial cs on cr.cs_id = cs.cs_id  
                                where cr.IsState=0 and cs.IsState=0 and cr.cs_Id=@serialId
                                order by mdyear desc ,mdmonth desc ,mdday desc";

            SqlParameter[] _params = {
                                         new SqlParameter("@serialId", SqlDbType.Int)
                                     };
            _params[0].Value = serialId;

            List<TimeTagEntity> list = new List<TimeTagEntity>();
            DataSet ds= BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, _params);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                TimeTagEntity entity = new TimeTagEntity();
                entity.CarId = ConvertHelper.GetInteger(row["car_id"]);
                int mdyear = ConvertHelper.GetInteger(row["mdyear"]);
                int mdmonth = ConvertHelper.GetInteger(row["mdmonth"]);
                int mdday = ConvertHelper.GetInteger(row["mdday"]);
                if (mdyear > 0 && mdmonth > 0 && mdday > 0)
                {
                    entity.MarketDateTime= new DateTime(mdyear, mdmonth, mdday);
                }
                entity.SerialId= ConvertHelper.GetInteger(row["cs_id"]);
                entity.CarYearType = ConvertHelper.GetInteger(row["Car_YearType"]);//Convert.ToInt32(row["Car_YearType"]);
                entity.CarSaleState = ConvertHelper.GetInteger(row["car_SaleState"]);
                entity.ReferPrice = ConvertHelper.GetString(row["car_ReferPrice"]);
                list.Add(entity);
            }

            return list;
        }       
    }
    public class SerialMarketTimeEntity
    {
        public string Url { get; set; }
        public string MarketTime { get; set; }
    }

    public class TimeTagEntity
    {
        public DateTime MarketDateTime { get; set; }

        public int CarId { get; set; }

        public int SerialId { get; set; }

        public int CarYearType { get; set; }

        public int CarSaleState { get; set; }

        public string ReferPrice { get; set; }
    }
}
