using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;
using System.Data;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Model;
using System.IO;
using BitAuto.Utils;
using System.Net;
using System.Threading;
using BitAuto.Utils.Data;

namespace BitAuto.CarDataUpdate.Common
{
    /// <summary>
    /// 公用数据类
    /// </summary>
    public static class CommonData
    {
        private static string _applicationPath;
        private static Dictionary<int, List<int>> m_categroyTreeDic;
        private static Dictionary<int, List<int>> m_categroyPathDic;
        private static Dictionary<int, string> m_categoryStrPathDic;
        private static Dictionary<int, string> m_serialLevelDic;
        private static Dictionary<int, int> m_serialBrandDic;
        private static Dictionary<int, int> m_serialMasterBrandDic;
        private static Dictionary<int, int> m_serialProducerDic;

        private static Dictionary<int, int> m_brandMasterBrandDic;
        private static Dictionary<int, int> m_brandProducerDic;
        private static Dictionary<int, string> m_csPriceRangeDic;

        public static DataSet CNCAPData;
        // add by chengl May.14.2012
        public static DataSet NCAPData;

        #region 原News类变量
        private static Dictionary<string, int[]> m_kindCatesForSerial;

        private static Dictionary<string, int[]> m_KindCatesForInsurance;
        #endregion

        #region 原News类属性
        /// <summary>
        /// 子品牌类型分类
        /// </summary>
        public static Dictionary<string, int[]> KindCatesForSerial
        {
            get
            {
                if (m_kindCatesForSerial != null && m_kindCatesForSerial.Count > 1) return m_kindCatesForSerial;
                m_kindCatesForSerial = new Dictionary<string, int[]>();
                m_kindCatesForSerial["focus"] = new int[] { 152, 34, 148, 146, 198, 149, 123, 127, 13, 98, 214, 220, 14, 211, 213, 218, 15, 216, 212, 217, 24, 25, 26, 28, 197, 347, 381, 179, 88, 143, 142, 85, 173, 27, 187, 188, 180, 181, 185, 183, 184, 186, 5, 138, 136, 139, 380, 376, 137, 378, 379, 377, 70, 348, 74, 227, 275 };
                m_kindCatesForSerial["xinwen"] = new int[] { 152, 34, 148, 146, 198, 149, 123, 127, 13, 98, 214, 220, 14, 211, 213, 218, 15, 216, 212, 217 };
                m_kindCatesForSerial["daogou"] = new int[] { 4, 179 };
                m_kindCatesForSerial["shijia"] = new int[] { 29, 30 };
                m_kindCatesForSerial["hangqing"] = new int[] { 3, 16, 215 };
                m_kindCatesForSerial["yongche"] = new int[] { 88, 143, 142, 86, 85, 173, 56 };
                m_kindCatesForSerial["shipin"] = new int[] { 74, 70, 348 };
                m_kindCatesForSerial["pingce"] = new int[] { 29, 31 };      //分类29的文章做为评测是有条件的
                m_kindCatesForSerial["treepingce"] = new int[] { 29, 30, 31 };
                m_kindCatesForSerial["maintancenews"] = new int[] { 86 };
                m_kindCatesForSerial["anquan"] = new int[] { 116, 117 };
                m_kindCatesForSerial["keji"] = new int[] { 102 };
                m_kindCatesForSerial["pingjia"] = new int[] { 4, 179, 29, 30, 31 };
                m_kindCatesForSerial["gaizhuang"] = new int[] { 87 };
                return m_kindCatesForSerial;
            }
        }
        #region 方便CommonFunction中GetEditerByType方法引用，自定义该属性
        public static string GetEditorForUrl
        {
            get { return CommonData.CommonSettings.EidtorUserUrl; }
        }
        #endregion

        /// <summary>
        /// 保险分类
        /// </summary>
        public static Dictionary<string, int[]> KindCatesForInsurance
        {
            get
            {
                if (m_KindCatesForInsurance != null && m_KindCatesForInsurance.Count > 1) return m_KindCatesForInsurance;
                m_KindCatesForInsurance = new Dictionary<string, int[]>();
                m_KindCatesForInsurance["baoxian"] = new int[] { 125 };
                m_KindCatesForInsurance["daikuan"] = new int[] { 129 };

                return m_KindCatesForInsurance;
            }
        }
        #endregion

        /// <summary>
        /// 当前程序路径
        /// </summary>
        public static string ApplicationPath
        {
            get { return _applicationPath; }
        }

        /// <summary>
        /// 新闻分类树结构字典
        /// Dictionary<分类id, List<子分类id>>
        /// </summary>
        public static Dictionary<int, List<int>> CategoryTreeDic
        {
            get { return m_categroyTreeDic; }
        }
        /// <summary>
        /// 新闻分类的路径字典
        /// </summary>
        public static Dictionary<int, List<int>> CategoryPathDic
        {
            get { return m_categroyPathDic; }
        }
        /// <summary>
        /// 新闻分类的路径字符串字典
        /// </summary>
        public static Dictionary<int, string> CategoryStrPathDic
        {
            get { return m_categoryStrPathDic; }
        }
        /// <summary>
        /// 子品牌数据字典
        /// </summary>
        public static Dictionary<int, SerialInfo> SerialDic;
        /// <summary>
        /// 子品牌所属级别名称字典
        /// </summary>
        public static Dictionary<int, string> SerialLevelDic
        {
            get { return m_serialLevelDic; }
        }
        /// <summary>
        /// 子品牌所属级别id字典
        /// </summary>
        public static Dictionary<int, int> SerialLevelIdDic { get; set; }
        /// <summary>
        /// 子品牌所属品牌ID字典
        /// </summary>
        public static Dictionary<int, int> SerialBrandDic
        {
            get { return m_serialBrandDic; }
        }

        /// <summary>
        /// 子品牌所属主品牌ID字典
        /// </summary>
        public static Dictionary<int, int> SerialMasterBrandDic
        {
            get { return m_serialMasterBrandDic; }
        }

        /// <summary>
        /// 子品牌所属厂商ID字典
        /// </summary>
        public static Dictionary<int, int> SerialProducerDic
        {
            get { return m_serialProducerDic; }
        }

        /// <summary>
        /// 品牌所属主品牌的字典
        /// </summary>
        public static Dictionary<int, int> BrandMasterBrandDic
        {
            get { return m_brandMasterBrandDic; }
            set { m_brandMasterBrandDic = value; }
        }

        /// <summary>
        /// 品牌所属厂商的字典
        /// </summary>
        public static Dictionary<int, int> BrandProducerDic
        {
            get { return m_brandProducerDic; }
            set { m_brandProducerDic = value; }
        }

        /// <summary>
        /// 车系报价区间
        /// </summary>
        public static Dictionary<int, string> CsPriceRangeDic
        {
            get { return m_csPriceRangeDic; }
            set { m_csPriceRangeDic = value; }
        }
        /// <summary>
        /// 公用配置
        /// </summary>
        public static CommonSettings CommonSettings;

        public static PhotoImageConfig PhotoImageConfig;
        /// <summary>
        /// 数据库连接串配置
        /// </summary>
        public static ConnStringSettings ConnectionStringSettings;

        /// <summary>
        /// 车型新闻类型设置
        /// </summary>
        public static CarNewsTypeSettings CarNewsTypeSettings;
        /// <summary>
        /// cms分类相关配置
        /// </summary>
        public static NewsCategoryConfig NewsCategoryConfig;
        /// <summary>
        /// 视频分类配置
        /// </summary>
        public static VideoCategoryConfig VideoCategoryConfig;
        /// <summary>
        /// 视频分类数据
        /// </summary>
        public static List<VideoCategoryEntity> VideoCategoryList;

        /// <summary>
        /// 所有有视频的子品牌ID
        /// </summary>
        public static List<int> ListHasVideoSerialID;

        /// <summary>
        /// CmsNewsId与CarNewsType字典
        /// </summary>
        public static Dictionary<int, List<int>> CategoryCarNewsTypeDic;

        /// <summary>
        /// 城市与省字典
        /// </summary>
        public static Dictionary<int, int> CityAndProvinceDic;

        public static DataSet RainbowData;
        public static List<int> SerialKoubeiReport;
        public static List<int> HasSaleDataList;
        /// <summary>
        /// 是否记录正常日志
        /// </summary>
        public static bool IsWriteLog = true;
        // 子品牌论坛地址
        public static Dictionary<int, string> dicCsBBS;
        // 车型网友油耗
        public static Dictionary<int, string> dicCarFuel;
        // 车型报价区间
        public static Dictionary<int, string> dicCarPriceRange;
        // 车型封面图
        public static Dictionary<int, string> dicCarDefaultPhoto;
        // 子品牌封面
        public static Dictionary<int, string> dicSerialPhoto;
		// 子品新牌封面
		public static Dictionary<int, string> dicSerialNewPhoto;
        // 每个车型的行情价
        public static Dictionary<int, string> dicCarHangQingPrice;
        // add by chengl Jan.23.2013
        // 每个车型的全国降价
        public static Dictionary<int, string> dicCarJiangJiaPrice;
        //车型价格区间 原始数据
        public static Dictionary<int, Dictionary<string, decimal>> dictCarPriceData;
        //子品牌颜色
        public static Dictionary<int, Dictionary<string, string>> dictSerialColor;
        //口碑评分明细
        public static Dictionary<int, Dictionary<string, string>> _koubeiRatingDic = null;


        /// <summary>
        /// 口碑评分Url
        /// </summary>
        //private static string KoubeiRatingUrl = ConfigurationManager.AppSettings["KoubeiRatingDetailUrl"];

        /// <summary>
        /// 口碑评分明细本地路径
        /// </summary>
        public static string KoubeiRatingLocalUrl;
        static CommonData()
        {
            _applicationPath = AppDomain.CurrentDomain.BaseDirectory;
            SetIsWriteLog();
            CommonSettings = (CommonSettings)ConfigurationManager.GetSection("CommonSettings");
            ConnectionStringSettings = (ConnStringSettings)ConfigurationManager.GetSection("ConnectionStringSettings");
            CarNewsTypeSettings = (CarNewsTypeSettings)ConfigurationManager.GetSection("CarNewsTypeSettings");
            NewsCategoryConfig = (NewsCategoryConfig)ConfigurationManager.GetSection("NewsCategoryConfig");
            KoubeiRatingLocalUrl = Path.Combine(CommonSettings.SavePath, @"Koubei/SerialKoubeiRating.xml");
            PhotoImageConfig = (PhotoImageConfig)ConfigurationManager.GetSection("PhotoImageConfig");
        }
        public static void InitData()
        {
            SetIsWriteLog();
            //VideoCategoryConfig = (VideoCategoryConfig)ConfigurationManager.GetSection("VideoCategoryConfig");
            InitNewsCategoryTreeDic();
            InitCategoryCarNewsTypeDic();
            InitNewsCategoryDic();
            GetEditorInBeijing();
            GetSerialData();
            InitSerialDataDic();
            GetRainbowData();
            // GetAllSerialKouBeiReport();
            GetHasSaleDataSerial();
            GetCNCAPData();
            // add by chengl May.14.2012
            GetNCAPata();
            CityAndProvinceDic = CityInitData.GetRelationMap();
            RemoveCarNewsTypeSubCategory();
            //add by sk 2012.12.28
            // 子品牌论坛地址
            dicCsBBS = GetAllSerialBBSUrl();
            // 车型网友油耗
            dicCarFuel = GetAllCarNetfriendsFuel();
            // 车型报价区间
            dicCarPriceRange = GetAllCarPriceRange();
            dictCarPriceData = GetAllCarPriceRangeData();
            // 车型封面图
            dicCarDefaultPhoto = GetCarDefaultPhoto(2);
            // 子品牌封面
            dicSerialPhoto = GetAllSerialPicURL(false);
			// 子品牌新封面图
			dicSerialNewPhoto = GetAllSerialPicURL(true);
            //// 每个车型的行情价
            //dicCarHangQingPrice = GetAllCarHangQingPrice();
            // 每个车型的降价
            // dicCarJiangJiaPrice = GetAllCarJiangJia();
            VideoCategoryList = Common.Services.VideoService.GetVideoCategory();
            // 所有有视频的子品牌
            ListHasVideoSerialID = Common.Services.VideoService.GetAllHasVideoSerialID();

            dictSerialColor = Common.Services.SerialService.GetAllSerialColorNameRGB();

            InitKoubeiRatingDic();
            // 子品牌报价区间
            GetSerialPriceRange();
        }

        #region  车型基础信息
        /// <summary>
        /// 取所有车型基本数据
        /// </summary>
        /// <returns></returns>
        public static DataSet GetAllCarBaseInfo(int carId)
        {
            DataSet ds = new DataSet();
            string sqlCarBaseInfo = @"select cs.cs_id,cs.cs_name,cs.Cs_ShowName,
			cs.AllSpell,car.car_id,car.car_name,
			car.Car_ProduceState,car.Car_SaleState,
			car.car_ReferPrice,car.Car_YearType,cs.cs_CarLevel
			from car_basic car
			left join car_serial cs on car.cs_id=cs.cs_id
			where car.isState=1 and cs.isState=1 and car.car_id=@carid order by car.car_id";
            System.Data.SqlClient.SqlParameter[] parameters = { new System.Data.SqlClient.SqlParameter("@carid", SqlDbType.Int) };
            parameters[0].Value = carId;
            ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(ConnectionStringSettings.CarChannelConnString, CommandType.Text, sqlCarBaseInfo, parameters);
            return ds;
        }
        /// <summary>
        /// 根据车型ID取车型扩展参数
        /// </summary>
        /// <param name="carID"></param>
        /// <returns></returns>
        public static DataSet GetCarParamByCarID(int carID)
        {
            DataSet ds = new DataSet();
            string sqlCarParam = @" select cdb.CarId,cdb.ParamId,cdb.Pvalue,pl.AliasName
			from dbo.CarDataBase cdb
			left join paramList pl on cdb.paramid=pl.paramid
			where cdb.carid ={0} order by cdb.carid,cdb.ParamId ";
            ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, string.Format(sqlCarParam, carID));
            return ds;
        }

		/// <summary>
		/// 车款选配
		/// </summary>
		/// <param name="carId"></param>
		/// <returns></returns>
		public static DataSet GetCarOptionalForCompare(int carId)
		{
			string sql = @"select cdb.CarId,cdb.PropertyId ParamId,cdb.PropertyValue Pvalue,cdb.Price,pl.AliasName
						from dbo.CarDataBaseOptional cdb
						left join paramList pl on cdb.PropertyId=pl.paramid
						where carid=@carid
						order by Price";
			SqlParameter[] param = {
								   new SqlParameter("@carid",SqlDbType.Int)
								   };
			param[0].Value = carId;
			return SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sql, param);
		}
        #endregion

        #region 车型详细参数所需其他数据
        /// <summary>
        /// 取所有子品牌论坛地址
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllSerialBBSUrl()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(CommonSettings.BBSAllUrl);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int csid = int.Parse(dr["id"].ToString());
                        string bbsUrl = dr["url"].ToString();
                        if (!dic.ContainsKey(csid))
                        { dic.Add(csid, bbsUrl); }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dic;
        }
        /// <summary>
        /// 获取网友提交的油耗
        /// </summary>
        /// <param name="carId"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllCarNetfriendsFuel()
        {
            Dictionary<int, string> fuelDic = new Dictionary<int, string>();
            string xmlFile = Path.Combine(CommonSettings.SavePath, @"Koubei/AllCarFuelV2.xml");

            try
            {
                //XmlDocument fuelDoc = new XmlDocument();
                //fuelDoc.Load(xmlFile);
                XmlDocument fuelDoc = CommonFunction.GetLocalXmlDocument(xmlFile);

                XmlNodeList fuelList = fuelDoc.SelectNodes("//Trim");
                foreach (XmlElement fuelNode in fuelList)
                {
                    int fCarId = Convert.ToInt32(fuelNode.GetAttribute("Id"));
                    double averageFuel = ConvertHelper.GetDouble(fuelNode.GetAttribute("UserAvgTrimFuel"));
                    if (averageFuel == 0)
                    { fuelDic[fCarId] = "无"; }
                    else
                    { fuelDic[fCarId] = averageFuel.ToString(); }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }

            return fuelDic;
        }
        /// <summary>
        /// 取所有车型的报价区间(不分地区)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllCarPriceRange()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(CommonSettings.AllCarPriceNoZone);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int Id = ConvertHelper.GetInteger(dr["Id"]);
                        decimal min = Math.Round(ConvertHelper.GetDecimal(dr["MinPrice"]), 2);
                        decimal max = Math.Round(ConvertHelper.GetDecimal(dr["MaxPrice"]), 2);
                        if (!dic.ContainsKey(Id))
                        { dic.Add(Id, min.ToString() + "万-" + max.ToString() + "万"); }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dic;
        }
        /// <summary>
        /// 取所有车型的报价区间(不分地区)
        /// </summary>
        /// <returns>原始数据</returns>
        public static Dictionary<int, Dictionary<string, decimal>> GetAllCarPriceRangeData()
        {
            Dictionary<int, Dictionary<string, decimal>> dict = new Dictionary<int, Dictionary<string, decimal>>();
            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(CommonSettings.AllCarPriceNoZone);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int carId = ConvertHelper.GetInteger(dr["Id"]);
                        decimal min = Math.Round(ConvertHelper.GetDecimal(dr["MinPrice"]), 2);
                        decimal max = Math.Round(ConvertHelper.GetDecimal(dr["MaxPrice"]), 2);
                        if (!dict.ContainsKey(carId))
                        {
                            dict.Add(carId, new Dictionary<string, decimal> { { "MinPrice", min }, { "MaxPrice", max } });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dict;
        }
        /// <summary>
        /// 取车型的封面
        /// </summary>
        /// <param name="subfix">图片规格</param>
        /// <returns></returns>
        public static Dictionary<int, string> GetCarDefaultPhoto(int subfix)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            XmlDocument doc = new XmlDocument();
            //doc.Load(CommonSettings.PhotoSerialInterface + "?dataname=carcoverimage&showall=false&showfullurl=true&subfix=" + subfix.ToString());
            string path = Path.Combine(PhotoImageConfig.SavePath, @"CarCoverImage.xml");
            doc.Load(path);
            if (doc != null && doc.HasChildNodes)
            {
                XmlNodeList xnl = doc.SelectNodes("/ImageData/ImageList/ImageInfo");
                if (xnl != null && xnl.Count > 0)
                {
                    foreach (XmlNode xn in xnl)
                    {
                        int carid = 0;
                        if (int.TryParse(xn.Attributes["CarId"].Value.ToString(), out carid))
                        {
                            if (carid > 0)
                            {
                                if (!dic.ContainsKey(carid))
                                {
                                    dic.Add(carid, xn.Attributes["ImageUrl"].Value.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return dic;
        }
		///// <summary>
		///// 取所有子品牌封面字典 图片规格定位2 需要其他规格另行替换
		///// </summary>
		///// <param name="isUseNew">新白底图 或者 老非白底图</param>
		///// <returns></returns>
		//public static Dictionary<int, string> GetAllSerialPicURL(bool isUseNew)
		//{
		//	string localImagePath = Path.Combine(PhotoImageConfig.SavePath, @"SerialCoverWithout.xml");
		//	Dictionary<int, string> dic = new Dictionary<int, string>();
		//	DataSet ds = new DataSet();
		//	//ds.ReadXml(CommonSettings.SerialPicCount);
		//	ds.ReadXml(localImagePath);
		//	if (ds != null && ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
		//	{
		//		foreach (DataRow dr in ds.Tables[1].Rows)
		//		{
		//			int csid = int.Parse(dr["SerialId"].ToString());
		//			string csNewPic = dr["ImageUrl2"].ToString() != "" ? string.Format(dr["ImageUrl2"].ToString().Trim(), "2") : "";
		//			string csOldPic = dr["ImageUrl"].ToString() != "" ? string.Format(dr["ImageUrl"].ToString().Trim(), "2") : "";
		//			if (isUseNew)
		//			{
		//				// 新图的
		//				if (csNewPic != "" && !dic.ContainsKey(csid))
		//				{ dic.Add(csid, csNewPic); }
		//				else if (csOldPic != "" && !dic.ContainsKey(csid))
		//				{ dic.Add(csid, csOldPic); }
		//				else if(!dic.ContainsKey(csid))
		//				{ dic.Add(csid, CommonSettings.DefaultCarPic); }
		//			}
		//			else
		//			{
		//				// 老图
		//				if (csOldPic != "" && !dic.ContainsKey(csid))
		//				{ dic.Add(csid, csOldPic); }
		//				else if (csNewPic != "" && !dic.ContainsKey(csid))
		//				{ dic.Add(csid, csNewPic); }
		//				else if(!dic.ContainsKey(csid))
		//				{ dic.Add(csid, CommonSettings.DefaultCarPic); }
		//			}
		//		}
		//	}
		//	return dic;
		//}

		/// <summary>
		/// 取所有子品牌封面字典 图片规格定位2 需要其他规格另行替换
		/// </summary>
		/// <param name="isUseNew">新白底图 或者 老非白底图</param>
		/// <returns></returns>
		public static Dictionary<int, string> GetAllSerialPicURL(bool isUseNew)
		{
			string localImagePath = Path.Combine(PhotoImageConfig.SavePath, @"SerialCoverImageAndCount.xml");
			Dictionary<int, string> dic = new Dictionary<int, string>();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(localImagePath);
			if (xmlDoc != null)
			{
				XmlNodeList serialList = xmlDoc.SelectNodes("SerialList/Serial");
				if (serialList != null && serialList.Count > 0)
				{ 
					foreach(XmlNode serialNode in serialList)
					{
						int csid = ConvertHelper.GetInteger(serialNode.Attributes["SerialId"].Value);
						string newPic = serialNode.Attributes["ImageUrl2"].Value;
						newPic = string.IsNullOrWhiteSpace(newPic) ? string.Empty : string.Format(newPic, "2");
						string oldPic = serialNode.Attributes["ImageUrl"].Value;
						oldPic = string.IsNullOrWhiteSpace(oldPic) ? string.Empty : string.Format(oldPic, "2");
						if (isUseNew)
						{
							// 新图的
							if (!string.IsNullOrWhiteSpace(newPic) && !dic.ContainsKey(csid))
							{ dic.Add(csid, newPic); }
							else if (!string.IsNullOrWhiteSpace(oldPic) && !dic.ContainsKey(csid))
							{ dic.Add(csid, oldPic); }
							else if (!dic.ContainsKey(csid))
							{ dic.Add(csid, CommonSettings.DefaultCarPic); }
						}
						else
						{
							// 老图
							if (!string.IsNullOrWhiteSpace(oldPic) && !dic.ContainsKey(csid))
							{ dic.Add(csid, oldPic); }
							else if (!string.IsNullOrWhiteSpace(newPic) && !dic.ContainsKey(csid))
							{ dic.Add(csid, newPic); }
							else if (!dic.ContainsKey(csid))
							{ dic.Add(csid, CommonSettings.DefaultCarPic); }
						}
					}
				}
			}
			return dic;
		}

        /// <summary>
        /// 取所有车型行情价(李东)
        /// http://api.admin.bitauto.com/api/list/marketprice.aspx?serial=true
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllCarHangQingPrice()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            try
            {
                XmlDocument doc = GetAllCarHangQingPriceXml();
                if (doc != null)
                {
                    XmlNodeList xnl = doc.SelectNodes("/NewDataSet/Table");
                    if (xnl != null && xnl.Count > 0)
                    {
                        foreach (XmlNode xn in xnl)
                        {
                            int carid = 0;
                            decimal minP = 0;
                            decimal maxP = 0;
                            if (int.TryParse(xn.SelectSingleNode("CarTypeId").InnerText, out carid))
                            { }
                            if (decimal.TryParse(xn.SelectSingleNode("minPrice").InnerText, out minP))
                            { }
                            if (decimal.TryParse(xn.SelectSingleNode("maxPrice").InnerText, out maxP))
                            { }
                            if (carid > 0 && minP > 0 && maxP > 0 && !dic.ContainsKey(carid))
                            {
                                dic.Add(carid, minP.ToString("F2") + "万-" + maxP.ToString("F2") + "万");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return dic;
        }
        private static XmlDocument GetAllCarHangQingPriceXml()
        {
            XmlDocument doc = null;
            string backPath = Path.Combine(CommonData.CommonSettings.SavePath, "AllCarHangQingPrice.xml");
            try
            {
                doc = CommonFunction.GetXmlDocument(CommonSettings.CarHangQingPriceUrl);
                CommonFunction.SaveXMLDocument(doc, backPath);
                return doc;
            }
            catch
            {
                if (doc == null && File.Exists(backPath))
                    doc = CommonFunction.GetXmlDocument(backPath);
            }
            return doc;
        }
        #endregion

        /// <summary>
        /// 删除CarNewsTypeSettings.CategoryIds中的子分类
        /// </summary>
        private static void RemoveCarNewsTypeSubCategory()
        {
            foreach (CarNewsTypeItem carNewsTypeItem in CarNewsTypeSettings.CarNewsTypeList.Values)
            {
                List<int> newList = CommonFunction.RemoveCarNewsTypeSubCategory(carNewsTypeItem.CategoryIdList);
                if (newList.Count != carNewsTypeItem.CategoryIdList.Count)
                    carNewsTypeItem.CategoryIds = string.Join(",", newList.ConvertAll<string>(pInt => pInt.ToString()).ToArray());
            }
        }

        /// <summary>
        /// 初始化新闻的分类ID树结构字典
        /// </summary>
        public static void InitNewsCategoryTreeDic()
        {
            Dictionary<int, List<int>> tmpDic = null;

            try
            {
                int cateId, cateSubId;
                string xpath = "/NewDataSet/NewsCategory[newscategoryid!='{0}' and contains(newscategoryidpath,'/{0}/')]/newscategoryid";
                XmlDocument xmlDoc = CommonFunction.GetXmlDocument(CommonSettings.NewsCategoryUrl);
                XmlNodeList cateList = xmlDoc.SelectNodes("/NewDataSet/NewsCategory/newscategoryid");
                XmlNodeList cateSubList = null;
                List<int> subList = null;
                tmpDic = new Dictionary<int, List<int>>(cateList.Count);
                foreach (XmlNode cateNode in cateList)
                {
                    cateId = ConvertHelper.GetInteger(cateNode.InnerText);
                    if (cateId < 0)
                        continue;

                    cateSubList = xmlDoc.SelectNodes(string.Format(xpath, cateId));
                    subList = new List<int>(cateSubList.Count);
                    foreach (XmlNode subNode in cateSubList)
                    {
                        cateSubId = ConvertHelper.GetInteger(subNode.InnerText);
                        if (cateSubId < 0)
                            continue;
                        subList.Add(cateSubId);
                    }
                    tmpDic.Add(cateId, subList);
                }
            }
            catch (System.Exception ex)
            {
                //记录日志
                Log.WriteErrorLog("Initialize news category dictionary error:" + ex.Message);
            }
            m_categroyTreeDic = (tmpDic == null) ? new Dictionary<int, List<int>>() : tmpDic;
        }
        /// <summary>
        /// 初始化新闻的分类ID字典
        /// </summary>
        public static void InitNewsCategoryDic()
        {
            m_categroyPathDic = new Dictionary<int, List<int>>();
            m_categoryStrPathDic = new Dictionary<int, string>();

            try
            {
                XmlDocument xmlDoc = CommonFunction.GetXmlDocument(CommonSettings.NewsCategoryUrl);
                XmlNodeList cateList = xmlDoc.SelectNodes("/NewDataSet/NewsCategory");
                foreach (XmlElement cateNode in cateList)
                {
                    //分析分类ID，路径及根ID，并加入分类字典
                    XmlElement idNode = (XmlElement)cateNode.SelectSingleNode("newscategoryid");
                    if (idNode != null)
                    {
                        //取分类ID
                        int cateId = 0;
                        bool isId = Int32.TryParse(idNode.InnerText, out cateId);
                        if (isId)
                        {
                            //取分类的全路径信息
                            XmlElement pathNode = (XmlElement)cateNode.SelectSingleNode("newscategoryidpath");
                            if (pathNode != null)
                            {
                                //拆分并存入字典中
                                string catePath = pathNode.InnerText;
                                string[] cateIdPaths = catePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                List<int> idList = new List<int>();
                                foreach (string idStr in cateIdPaths)
                                {
                                    int tmpId = 0;
                                    bool isInt = Int32.TryParse(idStr, out tmpId);
                                    if (isInt)
                                        idList.Add(tmpId);
                                }
                                m_categroyPathDic[cateId] = idList;
                                m_categoryStrPathDic[cateId] = catePath;
                            }
                        }
                    }
                    else
                    {
                        //记录日志
                        Log.WriteErrorLog("One category no categoryid!");
                    }

                }
            }
            catch (System.Exception ex)
            {
                //记录日志
                Log.WriteErrorLog("Initialize news category dictionary error:" + ex.ToString());
            }
        }
        /// <summary>
        /// 从数据库获取所有子品牌信息
        /// </summary>
        /// <returns></returns>
        public static void GetSerialData()
        {
            if (String.IsNullOrEmpty(ConnectionStringSettings.AutoStroageConnString))
                throw new Exception("连接字符串错误！");
            string sqlStr = @"SELECT  cs_Id,csName,cb_Name,cs.allSpell,csShowName,cs_seoname,cs.carlevel,cs.CsSaleState
								FROM    Car_Serial cs
										LEFT JOIN Car_Brand cb ON cs.cb_Id = cb.cb_Id
								WHERE   cs.IsState = 0";
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr);
            SerialDic = new Dictionary<int, SerialInfo>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    SerialInfo csInfo = new SerialInfo();
                    csInfo.Id = ConvertHelper.GetInteger(row["cs_Id"]);
                    if (csInfo.Id > 0)
                    {
                        csInfo.Name = row["csName"].ToString().Trim();
                        csInfo.ShowName = row["csShowName"].ToString().Trim();
                        csInfo.SeoName = row["cs_seoname"].ToString().Trim();
                        csInfo.AllSpell = row["allSpell"].ToString().Trim().ToLower();
                        csInfo.BrandName = row["cb_Name"].ToString().Trim();
                        csInfo.CarLevel = ConvertHelper.GetInteger(row["carlevel"]);
                        csInfo.CsSaleState = row["CsSaleState"].ToString();
                        SerialDic[csInfo.Id] = csInfo;
                    }
                }
            }
        }
        /// <summary>
        /// 获取所有车型相关数据
        /// </summary>
        public static Dictionary<int, CarEntity> GetAllCarData()
        {
            if (String.IsNullOrEmpty(ConnectionStringSettings.AutoStroageConnString))
                throw new Exception("连接字符串错误！");
            Dictionary<int, CarEntity> dict = new Dictionary<int, CarEntity>();
            string sqlStr = @"select car_id,cs_id,car_yeartype from dbo.Car_relation WHERE IsState=0";
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    CarEntity car = new CarEntity();
                    car.CarId = ConvertHelper.GetInteger(dr["car_id"]);
                    car.CsId = ConvertHelper.GetInteger(dr["cs_id"]);
                    car.Year = ConvertHelper.GetInteger(dr["car_yeartype"]);
                    dict.Add(car.CarId, car);
                }
            }
            return dict;
        }
        /// <summary>
        /// 获取子品牌下所有车型
        /// </summary>
        /// <param name="serialId">子品牌ID</param>
        /// <returns></returns>
        public static Dictionary<int, CarEntity> GetCarDataBySerialId(int serialId)
        {
            if (String.IsNullOrEmpty(ConnectionStringSettings.AutoStroageConnString))
                throw new Exception("连接字符串错误！");
            Dictionary<int, CarEntity> dict = new Dictionary<int, CarEntity>();
            string sqlStr = @"select car_id,cs_id,car_yeartype from dbo.Car_relation WHERE IsState=0 AND cs_id=@serialId";
            System.Data.SqlClient.SqlParameter[] param = { new System.Data.SqlClient.SqlParameter("@serialId", SqlDbType.Int) };
            param[0].Value = serialId;
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr, param);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    CarEntity car = new CarEntity();
                    car.CarId = ConvertHelper.GetInteger(dr["car_id"]);
                    car.CsId = ConvertHelper.GetInteger(dr["cs_id"]);
                    car.Year = ConvertHelper.GetInteger(dr["car_yeartype"]);
                    dict.Add(car.CarId, car);
                }
            }
            return dict;
        }
        /// <summary>
        /// 取车型信息根据车型ID
        /// </summary>
        /// <param name="carId"></param>
        /// <returns></returns>
        public static CarEntity GetCarDataById(int carId)
        {
            if (String.IsNullOrEmpty(ConnectionStringSettings.AutoStroageConnString))
                throw new Exception("连接字符串错误！");
            Dictionary<int, CarEntity> dict = new Dictionary<int, CarEntity>();
            string sqlStr = @"select car_id,cs_id,car_yeartype from dbo.Car_relation WHERE IsState=0 AND car_id=@carid";
            System.Data.SqlClient.SqlParameter[] param = { new System.Data.SqlClient.SqlParameter("@carid", SqlDbType.Int) };
            param[0].Value = carId;
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr, param);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                CarEntity car = new CarEntity();
                car.CarId = ConvertHelper.GetInteger(dt.Rows[0]["car_id"]);
                car.CsId = ConvertHelper.GetInteger(dt.Rows[0]["cs_id"]);
                car.Year = ConvertHelper.GetInteger(dt.Rows[0]["car_yeartype"]);
                dict.Add(car.CarId, car);
                return car;
            }
            return null;
        }
        /// <summary>
        /// 初始化cmsnewsid与CarNewsType字典
        /// </summary>
        public static void InitCategoryCarNewsTypeDic()
        {
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionStringSettings.CarDataUpdateConnString, CommandType.Text, @"select Id from CarNewsType;select CarNewsTypeId, CmsCategoryId from CarNewsTypeDef ORDER BY CmsCategoryId");
            if (ds == null || ds.Tables.Count < 1)
            {
                CategoryCarNewsTypeDic = new Dictionary<int, List<int>>();
                return;
            }
            DataTable typeTable = ds.Tables[0];
            DataTable cateTable = ds.Tables[1];
            CategoryCarNewsTypeDic = new Dictionary<int, List<int>>(500);
            int cmsId = -1, tmpId = -1, typeId;
            List<int> typeList = null;
            foreach (DataRow cateRow in cateTable.Rows)
            {
                tmpId = ConvertHelper.GetInteger(cateRow["CmsCategoryId"]);
                typeId = ConvertHelper.GetInteger(cateRow["CarNewsTypeId"]);
                if (tmpId != cmsId)
                {
                    typeList = new List<int>(5);
                    typeList.Add(typeId);
                    CategoryCarNewsTypeDic[tmpId] = typeList;
                    cmsId = tmpId;
                    continue;
                }
                CategoryCarNewsTypeDic[tmpId].Add(typeId);
            }
        }
        /// <summary>
        /// 初始化子品牌关联品牌、主品牌、厂商、级别等数据
        /// </summary>
        public static void InitSerialDataDic()
        {
            m_serialLevelDic = new Dictionary<int, string>();
            m_serialBrandDic = new Dictionary<int, int>();
            m_serialMasterBrandDic = new Dictionary<int, int>();
            m_serialProducerDic = new Dictionary<int, int>();
            m_brandMasterBrandDic = new Dictionary<int, int>();
            m_brandProducerDic = new Dictionary<int, int>();
            SerialLevelIdDic = new Dictionary<int, int>();

            string sqlStr = "SELECT cs.cs_Id,cs.carlevel,cs.cb_Id,bs.bs_Id,cb.cp_Id FROM Car_Serial cs "
                + " LEFT JOIN Car_Brand cb ON cs.cb_Id=cb.cb_Id "
                + " LEFT JOIN Car_MasterBrand_Rel bs ON cs.cb_Id=bs.cb_Id "
                + " WHERE cs.IsState=0";
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr);
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int serialId = Convert.ToInt32(row["cs_id"]);
                    int brandId = Convert.ToInt32(row["cb_Id"]);
                    if (row["bs_Id"] == DBNull.Value)
                        continue;
                    int masterbrandId = Convert.ToInt32(row["bs_Id"]);
                    int producerId = Convert.ToInt32(row["cp_Id"]);
                    int levelClassId = Convert.ToInt32(row["carlevel"]);
                    m_serialLevelDic[serialId] = GetLevelName(levelClassId);
                    SerialLevelIdDic[serialId] = levelClassId;
                    m_serialBrandDic[serialId] = brandId;
                    m_serialMasterBrandDic[serialId] = masterbrandId;
                    m_serialProducerDic[serialId] = producerId;

                    if (!m_brandMasterBrandDic.ContainsKey(brandId))
                        m_brandMasterBrandDic[brandId] = masterbrandId;

                    if (!m_brandProducerDic.ContainsKey(brandId))
                        m_brandProducerDic[brandId] = producerId;
                }
            }
        }
        /// <summary>
        /// 获取级别的名称
        /// </summary>
        /// <param name="levelClassId"></param>
        /// <returns></returns>
        private static string GetLevelName(int levelClassId)
        {
            string levelName = "";
            switch (levelClassId)
            {
                case 321:
                    levelName = "微型车";
                    break;
                case 338:
                    levelName = "小型车";
                    break;
                case 339:
                    levelName = "紧凑型";
                    break;
                case 340:
                    levelName = "中型车";
                    break;
                case 341:
                    levelName = "中大型";
                    break;
                case 342:
                    levelName = "豪华型";
                    break;
                case 424:
                    levelName = "SUV";
                    break;
                case 425:
                    levelName = "MPV";
                    break;
                case 426:
                    levelName = "跑车";
                    break;
                case 428:
                    levelName = "其它";
                    break;
                case 481:
                    levelName = "概念车";
                    break;
                case 482:
                    levelName = "面包车";
                    break;
                case 483:
                    levelName = "皮卡";
                    break;
				case 486:
					levelName = "轻客";
					break;
				case 487:
					levelName = "客车";
					break;
				case 488:
					levelName = "卡车";
					break;
				case 489:
					levelName = "轻卡";
					break;
				case 490:
					levelName = "重卡";
					break;
            }
            return levelName;
        }
        /// <summary>
        /// 获取责任编辑接口数据
        /// </summary>
        public static void GetEditorInBeijing()
        {
            XmlReader reader = XmlReader.Create(CommonSettings.EidtorUserUrl + "?zb=1");
            XmlDocument newsDoc = new XmlDocument();
            try
            {
                newsDoc.Load(reader);
                XmlNodeList nameList = newsDoc.SelectNodes("/Root/User/UserName");
                List<string> names = new List<string>(nameList.Count);
                foreach (XmlElement nameNode in nameList)
                {
                    if (!String.IsNullOrEmpty(nameNode.InnerText))
                        names.Add(nameNode.InnerText.Trim());
                }
                CommonSettings.EditorInBeijing = names;
            }
            catch (Exception ex)
            {
                CommonSettings.EditorInBeijing = null;  //new List<string>();
                Log.WriteErrorLog(ex.ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        /// <summary>
        /// 获取彩虹条数据
        /// </summary>
        public static void GetRainbowData()
        {
            string sqlStr = "select csID,RainbowitemID,url from dbo.RainbowEdit";
            RainbowData = SqlHelper.ExecuteDataset(ConnectionStringSettings.CarChannelConnString, CommandType.Text, sqlStr);
        }
        /// <summary>
        /// 取所有有口碑报告的子品牌ID列表
        /// </summary>
        /// <returns></returns>
        public static void GetAllSerialKouBeiReport()
        {
            SerialKoubeiReport = new List<int>();
            try
            {
                XmlDocument doc = CommonFunction.GetXmlDocument(CommonSettings.AllSerialKouBeiReport);

                XmlNodeList xnl = doc.SelectNodes("/feed/entry");
                foreach (XmlElement xn in xnl)
                {
                    int csid = ConvertHelper.GetInteger(xn.GetAttribute("modelid"));

                    if (csid > 0 && !SerialKoubeiReport.Contains(csid))
                    {
                        SerialKoubeiReport.Add(csid);
                    }
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 取所有有销量数据的子品牌
        /// </summary>
        public static void GetHasSaleDataSerial()
        {
            HasSaleDataList = new List<int>();
            string fileName = Path.Combine(CommonSettings.SavePath, "ProduceAndSell\\BrandTree.xml");
            if (!File.Exists(fileName))
                return;
            XmlDocument treeDoc = new XmlDocument();
            treeDoc.Load(fileName);
            XmlNodeList serialNodeList = treeDoc.SelectNodes("/root/Producer/Brand/Serial");
            foreach (XmlElement serialNode in serialNodeList)
            {
                int serialId = ConvertHelper.GetInteger(serialNode.GetAttribute("id"));
                if (!CommonData.HasSaleDataList.Contains(serialId))
                    CommonData.HasSaleDataList.Add(serialId);
            }
        }
        /// <summary>
        /// 获取CNCAP星级数据
        /// </summary>
        public static void GetCNCAPData()
        {
            string sqlStr = "SELECT Cs_Id ,CarYear ,Pvalue FROM Car_SerialYearDataBase  where paramid=649";
            CNCAPData = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr);
        }

        /// <summary>
        /// 获取NCAP星级数据 add by chengl May.14.2012
        /// </summary>
        public static void GetNCAPata()
        {
            string sqlStr = "SELECT Cs_Id ,CarYear ,Pvalue FROM Car_SerialYearDataBase  where paramid=637";
            NCAPData = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString, CommandType.Text, sqlStr);
        }

        private static void SetIsWriteLog()
        {
            bool isLog;
            if (bool.TryParse(ConfigurationManager.AppSettings["IsWriteLog"], out isLog))
                IsWriteLog = isLog;
        }
		/* 有GetAllSerialImageCount代替 接口地址不一样，2017-04-24
        /// <summary>
        /// 获取所有子品牌的图片数量
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetAllSerialImageCount()
        {
            Dictionary<int, int> imgCountDic = new Dictionary<int, int>();
            try
            {
                XmlDocument doc = CommonFunction.GetXmlDocument("http://imgsvr.bitauto.com/photo/getseriallist.aspx");
                XmlNodeList serialNodeList = doc.SelectNodes("/SerialList/Serial");
                foreach (XmlElement serialNode in serialNodeList)
                {
                    int serialId = ConvertHelper.GetInteger(serialNode.GetAttribute("SerialId"));
                    if (serialId > 0)
                        imgCountDic[serialId] = ConvertHelper.GetInteger(serialNode.GetAttribute("ImageCount"));
                }
            }
            catch { }
            return imgCountDic;
        }
		*/
		/// <summary>
		/// 获取所有子品牌的图片数量
		/// </summary>
		/// <returns></returns>
		public static Dictionary<int, int> GetAllSerialImageCount()
		{
			Dictionary<int, int> imgCountDic = new Dictionary<int, int>();
			try
			{
				XmlDocument doc = CommonFunction.GetXmlDocument(PhotoImageConfig.SerialCoverImageAndCountPath);
				XmlNodeList serialNodeList = doc.SelectNodes("/SerialList/Serial");
				foreach (XmlElement serialNode in serialNodeList)
				{
					int serialId = ConvertHelper.GetInteger(serialNode.GetAttribute("SerialId"));
					if (serialId > 0)
						imgCountDic[serialId] = ConvertHelper.GetInteger(serialNode.GetAttribute("ImageCount"));
				}
			}
			catch { }
			return imgCountDic;
		}

        /// <summary>
        /// 获取所有子品牌的答疑数量
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetAllSerialAskCount()
        {
            Dictionary<int, int> askCountDic = new Dictionary<int, int>();
            try
            {
                XmlDocument doc = CommonFunction.GetXmlDocument("http://api.ask.bitauto.com/data/questioncount/getserial.xml");
                XmlNodeList serialNodeList = doc.SelectNodes("/root/serial");
                foreach (XmlElement serialNode in serialNodeList)
                {
                    int serialId = ConvertHelper.GetInteger(serialNode.GetAttribute("id"));
                    if (serialId > 0)
                        askCountDic[serialId] = ConvertHelper.GetInteger(serialNode.GetAttribute("count"));
                }
            }
            catch { }
            return askCountDic;
        }
        /// <summary>
        /// 获取所有子品牌口碑的数量
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetAllSerialKoubeiCount()
        {
            Dictionary<int, int> koubieCountDic = new Dictionary<int, int>();
            try
            {
                string xmlFile = Path.Combine(CommonSettings.SavePath, "AllSerialDianpingCount.xml");
                if (File.Exists(xmlFile))
                {
                    XmlDocument doc = CommonFunction.GetXmlDocument(xmlFile);
                    XmlNodeList serialNodeList = doc.SelectNodes("/root/model");
                    foreach (XmlElement serialNode in serialNodeList)
                    {
                        int serialId = ConvertHelper.GetInteger(serialNode.GetAttribute("id"));
                        if (serialId > 0)
                            koubieCountDic[serialId] = ConvertHelper.GetInteger(serialNode.GetAttribute("topics_count"));
                    }
                }
            }
            catch { }
            return koubieCountDic;
        }

        /// <summary>
        /// 获取所有子品牌视频的数量
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetAllSerialShipinCount()
        {
            Dictionary<int, int> shipinCountDic = new Dictionary<int, int>();
            try
            {
                string xmlFile = Path.Combine(CommonSettings.SavePath, "SerialVideoCount.xml");
                if (File.Exists(xmlFile))
                {
                    XmlDocument doc = CommonFunction.GetXmlDocument(xmlFile);
                    XmlNodeList serialNodeList = doc.SelectNodes("/Videos/Serial");
                    foreach (XmlElement serialNode in serialNodeList)
                    {
                        int serialId = ConvertHelper.GetInteger(serialNode.GetAttribute("ID"));
                        if (serialId > 0)
                            shipinCountDic[serialId] = ConvertHelper.GetInteger(serialNode.GetAttribute("NewsCount"));
                    }
                }
            }
            catch { }
            return shipinCountDic;
        }

        /// <summary>
        /// 取车型的全国降价
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAllCarJiangJia()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            string sql = @"SELECT CarId, MaxFavorablePrice 
								FROM  dbo.JiangJiaNewsCarSummary
								WHERE CityId=0 and MaxFavorablePrice>0";
            DataSet ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(
                CommonData.ConnectionStringSettings.CarDataUpdateConnString
                , CommandType.Text, sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int carid = int.Parse(dr["CarId"].ToString());
                    decimal maxP = decimal.Parse(dr["MaxFavorablePrice"].ToString());
                    if (!dic.ContainsKey(carid))
                    {
                        dic.Add(carid, maxP.ToString("F2") + "万");
                    }
                }
            }
            return dic;
        }

        /// <summary>
        ///     取所有子品牌还关注数据
        ///     author:songcl date:2014-12-10
        /// </summary>
        /// <returns></returns>
        public static DataSet GetAllSerialToAttention()
        {
            string sql = " select sts.CS_Id,sts.PCs_Id,sts.Pv_Num,cs.cs_name,cs.cs_showname,cs.allspell ";
            sql += " from dbo.Serial_To_Serial sts ";
            sql += " left join Car_Serial cs on sts.PCs_Id = cs.cs_id ";
            sql += " order by sts.CS_Id,sts.Pv_Num desc ";
            DataSet executeDataset = SqlHelper.ExecuteDataset(ConnectionStringSettings.CarChannelConnString,
                                                              CommandType.Text, sql);
            return executeDataset;
        }

        /// <summary>
        ///     取子品牌的还关注
        ///     author:songcl date:2014-12-10
        /// </summary>
        /// <param name="id">子品牌ID</param>
        /// <param name="top">取条数</param>
        public static List<SerialToAttention> GetSerialToAttentionByCsID(int id, int top)
        {
            var lsts = new List<SerialToAttention>();
            DataSet ds = GetAllSerialToAttention();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow[] drs = ds.Tables[0].Select(" cs_id = " + id.ToString(CultureInfo.InvariantCulture) + " ");
                if (drs.Length > 0)
                {
                    int loopCount = 0;
                    foreach (DataRow dr in drs)
                    {
                        if (loopCount >= top)
                        {
                            break;
                        }
                        var serialToAttention = new SerialToAttention();
                        serialToAttention.CsID = int.Parse(dr["cs_id"].ToString());
                        serialToAttention.ToCsID = int.Parse(dr["PCs_Id"].ToString());
                        serialToAttention.ToPvNum = int.Parse(dr["Pv_Num"].ToString());
                        serialToAttention.ToCsName = dr["cs_name"].ToString().Trim();
                        serialToAttention.ToCsShowName = dr["cs_showname"].ToString().Trim();
                        serialToAttention.ToCsAllSpell = dr["allspell"].ToString().Trim();
                        lsts.Add(serialToAttention);
                        loopCount++;
                    }
                }
            }
            return lsts;
        }

        private static DataSet GetAllCarsBySerialId(DataTable table)
        {
            SqlParameter[] parameters = { new SqlParameter("@tb", SqlDbType.Structured) };
            parameters[0].Value = table;
            DataSet executeDataset = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString,
                                                              CommandType.StoredProcedure, "GetCarInnerSpaceInfo",
                                                              parameters);
            return executeDataset;
        }

        /// <summary>
        ///     获取车内空间信息
        /// </summary>
        /// <param name="table"></param>
        /// <param name="currentCsId"></param>
        /// <returns></returns>
        public static List<CarInnerSpaceInfo> GetCarInnerSpaceInfoList(DataTable table, int currentCsId)
        {
            var carInnerSpaceInfos = new List<CarInnerSpaceInfo>();

            DataSet dataSet = GetAllCarsBySerialId(table);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var carInnerSpaceInfo = new CarInnerSpaceInfo();
                carInnerSpaceInfo.CarId = ConvertHelper.GetInteger(row["Car_Id"]);
                carInnerSpaceInfo.CsId = ConvertHelper.GetInteger(row["Cs_Id"]);
                carInnerSpaceInfo.FirstSeatToTop = ConvertHelper.GetDouble(row["FirstSeatToTop"]);
                carInnerSpaceInfo.SecondSeatToTop = ConvertHelper.GetDouble(row["SecondSeatToTop"]);
                carInnerSpaceInfo.FirstSeatDistance = ConvertHelper.GetDouble(row["FirstSeatDistance"]);
                carInnerSpaceInfo.ThirdSeatToTop = ConvertHelper.GetDouble(row["ThirdSeatToTop"]);
                carInnerSpaceInfo.CsShowName = row["csShowName"].ToString();
                carInnerSpaceInfo.SerialAllSpell = row["allSpell"].ToString();
                carInnerSpaceInfo.CarYear = ConvertHelper.GetInteger(row["Car_YearType"]);
                carInnerSpaceInfo.IsCurrent = carInnerSpaceInfo.CsId == currentCsId;
                carInnerSpaceInfo.CarName = row["Car_Name"].ToString();
                carInnerSpaceInfo.ReferPrice = ConvertHelper.GetDouble(row["car_ReferPrice"]);
                carInnerSpaceInfos.Add(carInnerSpaceInfo);
            }

            return carInnerSpaceInfos;
        }

        private static DataSet GetModelDateSetByCarId(int id)
        {
            SqlParameter[] parameters = { new SqlParameter("@id", SqlDbType.Int) };
            parameters[0].Value = id;
            const string sql =
                @"SELECT [Car_Id],[Type],[ParaId],[ModelName],[Height],[Weight],[ImageUrl],[IsState],[CreateTime],[UpdateTime]
  FROM [Car_relation_Model_Data] where Car_Id=@id and IsState=0 and ImageUrl!='' order by UpdateTime desc";
            DataSet executeDataset = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString,
                                                              CommandType.Text, sql, parameters);
            return executeDataset;
        }

        /// <summary>
        ///     获取内部空间模特数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<CarModelInfo> GetCarModelInfoList(int id)
        {
            var carModelInfos = new List<CarModelInfo>();

            DataSet ds = GetModelDateSetByCarId(id);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var carModelInfo = new CarModelInfo();
                carModelInfo.Height = ConvertHelper.GetDouble(row["Height"]);
                carModelInfo.Weight = ConvertHelper.GetDouble(row["Weight"]);
                //carModelInfo.ImageUrl = row["ImageUrl"] != null ? "/"+row["ImageUrl"]: "";//本地测试
                carModelInfo.ImageUrl = row["ImageUrl"] != null
                                            ? row["ImageUrl"].ToString()
                                                             .Replace("CarChannel/pic/CarReportPic/",
                                                                      "http://image.bitautoimg.com/newsimg-300-w0/CarChannel/pic/CarReportPic/")
                                            : ""; //线上
                carModelInfo.Type = ConvertHelper.GetInteger(row["Type"]);
                carModelInfo.ParaId = ConvertHelper.GetInteger(row["ParaId"]);
                carModelInfos.Add(carModelInfo);
            }
            return carModelInfos;
        }

        /// <summary>
        ///     获取车款后备箱信息
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private static DataSet GetCarBackBootInfo(DataTable table)
        {
            SqlParameter[] parameters = { new SqlParameter("@tb", SqlDbType.Structured) };
            parameters[0].Value = table;
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString,
                                                  CommandType.StoredProcedure,
                                                  "[GetCarBackBootInfo]", parameters);
            return ds;
        }

        /// <summary>
        ///     获取车款后备箱信息
        /// </summary>
        /// <param name="table"></param>
        /// <param name="currentCarId"></param>
        /// <returns></returns>
        public static List<CarBootInfo> GetCarBackBootInfoList(DataTable table, int currentCarId)
        {
            var carBootInfos = new List<CarBootInfo>();
            DataSet dataSet = GetCarBackBootInfo(table);
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var carBootInfo = new CarBootInfo();
                carBootInfo.CarId = ConvertHelper.GetInteger(row["CarId"]);
                carBootInfo.IsCurrent = carBootInfo.CarId == currentCarId;
                carBootInfo.ParamId = ConvertHelper.GetInteger(row["ParamId"]);
                carBootInfo.Pvalue = row["Pvalue"].ToString();
                carBootInfo.CsId = ConvertHelper.GetInteger(row["cs_id"]);
                carBootInfo.CsShowName = row["csShowName"].ToString();
                carBootInfo.SerialAllSpell = row["allSpell"].ToString();
                carBootInfo.CarYear = ConvertHelper.GetInteger(row["Car_YearType"]);
                carBootInfo.CarName = row["Car_Name"].ToString();
                carBootInfos.Add(carBootInfo);
            }

            return carBootInfos;
        }

        /// <summary>
        ///     获取看过还看得所有车
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static DataSet GetSerialToSeeAllCars(DataTable table)
        {
            SqlParameter[] parameters = { new SqlParameter("@tb", SqlDbType.Structured) };
            parameters[0].Value = table;
            DataSet executeDataset = SqlHelper.ExecuteDataset(ConnectionStringSettings.AutoStroageConnString,
                                                              CommandType.StoredProcedure, "GetSerialToSeeAllCars",
                                                              parameters);
            return executeDataset;
        }

        /// <summary>
        ///     看过还看得所有车
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<CarEntity> GetSerialToSeeAllCarsList(DataTable table)
        {
            var carEntities = new List<CarEntity>();
            DataSet dataSet = GetSerialToSeeAllCars(table);
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var carEntity = new CarEntity();
                carEntity.CarId = ConvertHelper.GetInteger(row["Car_Id"]);
                carEntity.CsId = ConvertHelper.GetInteger(row["Cs_Id"]);
                carEntity.Year = ConvertHelper.GetInteger(row["Car_YearType"]);
                carEntity.ReferPrice = ConvertHelper.GetDouble(row["car_ReferPrice"]);
                carEntities.Add(carEntity);
            }

            return carEntities;
        }

        private static void InitKoubeiRatingDic()
        {
            string KoubeiRatingUrl = ConfigurationManager.AppSettings["KoubeiRatingDetailUrl"];//设置成全局变量，在CarDataUpdateService有问题
            KoubeiRatingUrl = KoubeiRatingUrl.Replace("{yearmonth}", DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0'));
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(KoubeiRatingUrl);
                if (!xmlDoc.HasChildNodes)
                {
                    Log.WriteLog("口碑评分明细文档为空，Url：" + KoubeiRatingUrl);
                    return;
                }
                CommonFunction.SaveXMLDocument(xmlDoc, KoubeiRatingLocalUrl);
                var serialNodes = xmlDoc.SelectNodes("CarStatistics/Serial");
                if (serialNodes != null && serialNodes.Count > 0)
                {
                    _koubeiRatingDic = new Dictionary<int, Dictionary<string, string>>();
                    foreach (XmlNode serialNode in serialNodes)
                    {
                        var id = Int32.Parse(serialNode.Attributes["SerialId"].Value);
                        var MaxFuel = serialNode.Attributes["MaxFuel"].Value;
                        var MinFuel = serialNode.Attributes["MinFuel"].Value;
                        var Ranker = serialNode.Attributes["Ranker"].Value; // 同级别排名
                        var KongJian = serialNode.Attributes["KongJian"].Value;
                        var DongLi = serialNode.Attributes["DongLi"].Value;
                        var CaoKong = serialNode.Attributes["CaoKong"].Value;
                        var PeiZhi = serialNode.Attributes["PeiZhi"].Value;
                        var ShuShiDu = serialNode.Attributes["ShuShiDu"].Value;
                        var XingJiaBi = serialNode.Attributes["XingJiaBi"].Value;
                        var WaiGuan = serialNode.Attributes["WaiGuan"].Value;
                        var NeiShi = serialNode.Attributes["NeiShi"].Value;
                        var YouHao = serialNode.Attributes["YouHao"].Value;
                        var Ratings = serialNode.Attributes["Ratings"].Value;
                        var TopicCount = serialNode.Attributes["TopicCount"].Value;

                        var dic = new Dictionary<string, string>
                        {
                            {"MinFuel",MinFuel},
                            {"MaxFuel",MaxFuel},
                            {"Ranker", Ranker},
                            {"KongJian", KongJian},
                            {"DongLi", DongLi},
                            {"CaoKong", CaoKong},
                            {"PeiZhi", PeiZhi},
                            {"ShuShiDu", ShuShiDu},
                            {"XingJiaBi", XingJiaBi},
                            {"WaiGuan", WaiGuan},
                            {"NeiShi", NeiShi},
                            {"YouHao", YouHao},
                            {"Ratings",Ratings},
                            {"TopicCount",TopicCount}
                        };
                        _koubeiRatingDic.Add(id, dic);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("口碑评分明细文档错误," + ex.ToString());
            }
        }

        /// <summary>
        /// 取子品牌报价区间(不分地区)
        /// </summary>
        /// <returns></returns>
        private static Dictionary<int, string> GetSerialPriceRange()
        {
            DataSet ds = GetAllSerialPriceRange();
            m_csPriceRangeDic = new Dictionary<int, string>();
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
                            if (!m_csPriceRangeDic.ContainsKey(csid))
                            { m_csPriceRangeDic.Add(csid, result); }
                        }
                    }
                    catch
                    { }
                }
            }
            return m_csPriceRangeDic;
        }

        /// <summary>
        /// 取子品牌报价区间
        /// </summary>
        /// <returns></returns>
        private static DataSet GetAllSerialPriceRange()
        {
            DataSet ds = new DataSet();
            try
            {
                ds.ReadXml(CommonData.CommonSettings.PriceRangeInterface);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
                try
                {
                    ds.ReadXml(Path.Combine(CommonData.CommonSettings.SavePath, @"EP\cspricescope.xml"));
                }
                catch (Exception ex2)
                {
                    Log.WriteErrorLog(ex2.Message + ex2.StackTrace);
                }
            }
            return ds;
        }
    }
}
