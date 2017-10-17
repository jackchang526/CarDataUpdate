using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace BitAuto.CarDataUpdate.Config
{
	/// <summary>
	/// 公用配置
	/// </summary>
	public class CommonSettings : ConfigurationSection
	{
		/// <summary>
		/// 消息队列地址
		/// </summary>
		[ConfigurationProperty("QueueName", DefaultValue = @".\private$\ContentUpdateQueue")]
		public string QueueName
		{
			get { return (string)base["QueueName"]; }
		}
		/// <summary>
		/// 获取新闻的Url
		/// </summary>
		[ConfigurationProperty("NewsUrl", DefaultValue = "http://api.admin.bitauto.com/api/newslist.aspx")]
		public string NewsUrl
		{
			get { return (string)base["NewsUrl"]; }
		}
		/// <summary>
		/// 新闻内容的Url
		/// </summary>
		[ConfigurationProperty("NewsDetailUrl", DefaultValue = "http://api.admin.bitauto.com/api/list/newsdetails.aspx?newsid={0}")]
		public string NewsDetailUrl
		{
			get { return (string)base["NewsDetailUrl"]; }
		}
		/// <summary>
		/// 新闻分类的Url
		/// </summary>
		[ConfigurationProperty("NewsCategoryUrl", DefaultValue = "http://api.admin.bitauto.com/api/newslist.aspx?showcategory=1")]
		public string NewsCategoryUrl
		{
			get { return (string)base["NewsCategoryUrl"]; }
		}

		///// <summary>
		///// 更新的时间列表
		///// </summary>
		//[ConfigurationProperty("UpdateTimeList")]
		//public UpdateTimeCollection UpdateTimeList
		//{
		//    get { return (UpdateTimeCollection)base["UpdateTimeList"]; }
		//}

		/// <summary>
		/// 存储路径
		/// </summary>
		[ConfigurationProperty("SavePath", DefaultValue = "C:\\NewsContent")]
		public string SavePath
		{
			get { return (string)base["SavePath"]; }
		}

		[ConfigurationProperty("BuyCarServiceSavePath", DefaultValue = "")]
		public string BuyCarServiceSavePath
		{
			get { return (string)base["BuyCarServiceSavePath"]; }
		}
		/// <summary>
		/// 车型实拍图xml地址
		/// </summary>
		[ConfigurationProperty("CarRealPicPath", DefaultValue = "C:\\NewsContent")]
		public string CarRealPicPath
		{
			get { return (string)base["CarRealPicPath"]; }
		}

		/// <summary>
		/// 获取图片Url地址
		/// </summary>
		[ConfigurationProperty("ImageUrl", DefaultValue = "http://photo.bitauto.com/service/getserialcover.aspx")]
		public string ImageUrl
		{
			get { return (string)base["ImageUrl"]; }
		}

		/// <summary>
		/// 子品牌焦点图片接口地址
		/// </summary>
		[ConfigurationProperty("SerialFocusImageUrl", DefaultValue = "")]
		public string SerialFocusImageUrl
		{
			get { return (string)base["SerialFocusImageUrl"]; }
		}

		/// <summary>
		/// 主品牌，品牌，子品牌信息
		/// </summary>
		[ConfigurationProperty("AutoDataUrl", DefaultValue = "")]
		public string AutoDataUrl
		{
			get { return (string)base["AutoDataUrl"]; }
		}

		/// <summary>
		/// 子品牌答疑URL
		/// </summary>
		[ConfigurationProperty("AskEntriesUrl", DefaultValue = "")]
		public string AskEntriesUrl
		{
			get { return (string)base["AskEntriesUrl"]; }
		}
		/// <summary>
		/// 按车型级别取油耗与养车费用Url
		/// </summary>
		[ConfigurationProperty("LevelCarCostUrl", DefaultValue = "")]
		public string LevelCarCostUrl
		{
			get { return (string)base["LevelCarCostUrl"]; }
		}
		/// <summary>
		/// 城市列表Url
		/// </summary>
		[ConfigurationProperty("CityListUrl", DefaultValue = "")]
		public string CityListUrl
		{
			get { return (string)base["CityListUrl"]; }
		}
		/// <summary>
		/// 子品牌点评Url
		/// </summary>
		[ConfigurationProperty("SerialDianpingUrl", DefaultValue = "")]
		public string SerialDianpingUrl
		{
			get { return (string)base["SerialDianpingUrl"]; }
		}
		/// <summary>
		/// 取子品牌油耗区间(点评)
		/// </summary>
		[ConfigurationProperty("SerialYouHaoRangeNewUrl", DefaultValue = "")]
		public string SerialYouHaoRangeNewUrl
		{
			get { return (string)base["SerialYouHaoRangeNewUrl"]; }
		}
		/// <summary>
		/// 车型城市商家报价Url
		/// </summary>
		[ConfigurationProperty("CarCityPriceUrl", DefaultValue = "")]
		public string CarCityPriceUrl
		{
			get { return (string)base["CarCityPriceUrl"]; }
		}

		///// <summary>
		///// 二手车内容
		///// </summary>
		//[ConfigurationProperty("UCarUrl", DefaultValue = "")]
		//public string UCarUrl
		//{
		//	get { return (string)base["UCarUrl"]; }
		//}
		/// <summary>
		/// 全部品牌与子品牌的地址，包括停销的子品牌
		/// </summary>
		[ConfigurationProperty("AllAutoDataUrl", DefaultValue = "")]
		public string AllAutoDataUrl
		{
			get { return (string)base["AllAutoDataUrl"]; }
		}

		/// <summary>
		/// 全部品牌与子品牌的地址，包括停销的子品牌和级别
		/// </summary>
		[ConfigurationProperty("AllSaleAndLevelUrl", DefaultValue = "")]
		public string AllSaleAndLevelUrl
		{
			get { return (string)base["AllSaleAndLevelUrl"]; }
		}

		/// <summary>
		/// 获取品牌图片的地址
		/// </summary>
		[ConfigurationProperty("BrandImageUrl", DefaultValue = "")]
		public string BrandImageUrl
		{
			get { return (string)base["BrandImageUrl"]; }
		}

		/// <summary>
		/// 获取所有子品牌的论坛地址
		/// </summary>
		[ConfigurationProperty("GetSerialForumUrl", DefaultValue = "")]
		public string SerialForumUrl
		{
			get { return (string)base["GetSerialForumUrl"]; }
		}

		/// <summary>
		/// 获取所有车型的油耗
		/// </summary>
		[ConfigurationProperty("AllCarFuel", DefaultValue = "")]
		public string AllCarFuel
		{
			get { return (string)base["AllCarFuel"]; }
		}

		/// <summary>
		/// 获取所有子品牌口碑报告
		/// </summary>
		[ConfigurationProperty("AllSerialKouBeiReport", DefaultValue = "")]
		public string AllSerialKouBeiReport
		{
			get { return (string)base["AllSerialKouBeiReport"]; }
		}
		/// <summary>
		/// 获取编辑试驾评测地址
		/// </summary>
		[ConfigurationProperty("EditorCommentUrl", DefaultValue = "")]
		public string EditorCommentUrl
		{
			get { return (string)base["EditorCommentUrl"]; }
		}
        /// <summary>
        /// 获取编辑评测信息 H5
        /// </summary>
        [ConfigurationProperty("EditorCommentUrlNew", DefaultValue = "")]
        public string EditorCommentUrlNew
        {
            get { return (string)base["EditorCommentUrlNew"]; }
        }
		/// <summary>
		/// 获取编辑信息
		/// </summary>
		[ConfigurationProperty("EidtorUserUrl", DefaultValue = "")]
		public string EidtorUserUrl
		{
			get { return (string)base["EidtorUserUrl"]; }
		}
		/// <summary>
		/// 获取SellDataMapUrl
		/// </summary>
		[ConfigurationProperty("SellDataMapUrl", DefaultValue = "")]
		public string SellDataMapUrl
		{
			get { return (string)base["SellDataMapUrl"]; }
		}
		/// <summary>
		/// 获取CityValueSetUrl type=153citynav
		/// </summary>
		[ConfigurationProperty("CityValueSetUrl", DefaultValue = "")]
		public string CityValueSetUrl
		{
			get { return (string)base["CityValueSetUrl"]; }
		}
		/// <summary>
		/// 获取子品牌颜色图片地址格式化字符串
		/// serialid={0} showfullurl=true subfix={1} showall={2}
		/// </summary>
		[ConfigurationProperty("SerialColorPhotoUrl", DefaultValue = "")]
		public string SerialColorPhotoUrl
		{
			get { return (string)base["SerialColorPhotoUrl"]; }
		}
		///////////////////////
		/// <summary>
		/// 子品牌综述页图释默认子品牌颜色图片
		/// </summary>
		[ConfigurationProperty("SerialColorImageDefaultUrl", DefaultValue = "")]
		public string SerialColorImageDefaultUrl
		{
			get { return (string)base["SerialColorImageDefaultUrl"]; }
		}

		/// <summary>
		/// 子品牌综述页图释 右边默认图片地址
		/// </summary>
		[ConfigurationProperty("SerialOutSetDefaultWebPath", DefaultValue = "")]
		public string SerialOutSetDefaultWebPath
		{
			get { return (string)base["SerialOutSetDefaultWebPath"]; }
		}
		/// <summary>
		/// 子品牌综述页图释 右边图片地址
		/// </summary>
		[ConfigurationProperty("SerialOutSetWebPath", DefaultValue = "")]
		public string SerialOutSetWebPath
		{
			get { return (string)base["SerialOutSetWebPath"]; }
		}

		private List<String> m_editorInBeijing;

	    /// <summary>
	    /// 北京的责任编辑列表
	    /// </summary>
	    public List<string> EditorInBeijing
	    {
	        set { m_editorInBeijing = value; }
            get
            {
                if (m_editorInBeijing == null)
                {
                    XmlReader reader = XmlReader.Create(EidtorUserUrl + "?zb=1");
                    XmlDocument newsDoc = new XmlDocument();
                    try
                    {
                        newsDoc.Load(reader);
                        XmlNodeList nameList = newsDoc.SelectNodes("/Root/User/UserName");
                        m_editorInBeijing = new List<string>(nameList.Count);
                        foreach (XmlElement nameNode in nameList)
                        {
                            if (!String.IsNullOrEmpty(nameNode.InnerText))
                                m_editorInBeijing.Add(nameNode.InnerText.Trim());
                        }
                    }
                    catch
                    {
                        m_editorInBeijing = new List<string>();
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
                return m_editorInBeijing;
            }
	    }

	    ///// <summary>
		///// 子品牌置换优惠信息
		///// </summary>
		//[ConfigurationProperty("GetCarReplacementUrl", DefaultValue = "http://api.taoche.cn/CarSourceInterface/GetReplacementInfo.ashx?serialid={0}")]
		//public string GetCarReplacementUrl
		//{
		//	get { return (string)base["GetCarReplacementUrl"]; }
		//}
		///// <summary>
		///// 品牌置换列表
		///// </summary>
		//[ConfigurationProperty("GetCarReplacementBrandUrl", DefaultValue = "http://api.taoche.cn/CarSourceInterface/GetReplacementBrand.ashx")]
		//public string GetCarReplacementBrandUrl
		//{
		//	get { return (string)base["GetCarReplacementBrandUrl"]; }
		//}
		/// <summary>
		/// 子品牌论坛地址
		/// </summary>
		[ConfigurationProperty("BBSAllUrl", DefaultValue = "http://api.baa.bitauto.com/CarBrandToForumUrl.aspx")]
		public string BBSAllUrl
		{
			get { return (string)base["BBSAllUrl"]; }
		}
		/// <summary>
		/// 全部车型报价(部分地区) modified by sk 2015.11.30 全国最低报价
		/// </summary>
		[ConfigurationProperty("AllCarPriceNoZone", DefaultValue = "http://price.bitauto.com/interface/xml/carminprice.xml")]
		public string AllCarPriceNoZone
		{
			get { return (string)base["AllCarPriceNoZone"]; }
		}
		/// <summary>
		/// 图库子品牌接口
		/// </summary>
		[ConfigurationProperty("PhotoSerialInterface", DefaultValue = "http://imgsvr.bitauto.com/autochannel/SerialImageService.aspx")]
		public string PhotoSerialInterface
		{
			get { return (string)base["PhotoSerialInterface"]; }
		}
		/// <summary>
		/// 子品牌图片数 不再使用，有PhotoImage.config的SerialCoverImageAndCountPath接口替代
		/// </summary>
		[ConfigurationProperty("SerialPicCount", DefaultValue = "http://imgsvr.bitauto.com/photo/getseriallist.aspx")]
		public string SerialPicCount
		{
			get { return (string)base["SerialPicCount"]; }
		}
		/// <summary>
		/// 车型默认图
		/// </summary>
		[ConfigurationProperty("DefaultCarPic", DefaultValue = "http://image.bitautoimg.com/autoalbum/V2.1/images/150-100.gif")]
		public string DefaultCarPic
		{
			get { return (string)base["DefaultCarPic"]; }
        }
        /// <summary>
        /// 车型默认图
        /// </summary>
        [ConfigurationProperty("DefaultCarPic2", DefaultValue = "http://image.bitautoimg.com/autoalbum/V2.1/images/90-60.gif")]
        public string DefaultCarPic2
        {
            get { return (string)base["DefaultCarPic2"]; }
        }
		/// <summary>
		/// 取所有车型行情价
		/// </summary>
		[ConfigurationProperty("CarHangQingPriceUrl", DefaultValue = "http://api.admin.bitauto.com/api/list/marketprice.aspx?serial=true")]
		public string CarHangQingPriceUrl
		{
			get { return (string)base["CarHangQingPriceUrl"]; }
		}
		/// <summary>
		/// memcache有效时间
		/// </summary>
		[ConfigurationProperty("MemcacheDuration", DefaultValue = "604800000")]
		public string MemcacheDuration
		{
			get { return (string)base["MemcacheDuration"]; }
		}
		/// <summary>
		/// 通用导航头本地模板路径
		/// </summary>
		[ConfigurationProperty("CommonHeadLocalFilePath", DefaultValue = @"d:\TFSRoot2010\A3车型产品研发\新版车型库\Carchannelv2.1\StatisticCarChannel\StatisticCarChannel\StatisticCarChannel\Data\")]
		public string CommonHeadLocalFilePath
		{
			get { return (string)base["CommonHeadLocalFilePath"]; }
		}
		/// <summary>
		/// nas路径
		/// </summary>
		[ConfigurationProperty("StaticDataResNAS", DefaultValue = "\\192.168.0.40\\nas\\CarChannel\\")]
		public string StaticDataResNAS
		{
			get { return (string)base["StaticDataResNAS"]; }
		}
		/// <summary>
		/// 北京2010车展
		/// </summary>
		[ConfigurationProperty("BeiJing2010", DefaultValue = "http://car.bitauto.com/Interface/Exhibition/beijing_2010_AlbumSync.aspx")]
		public string BeiJing2010
		{
			get { return (string)base["BeiJing2010"]; }
		}
		/// <summary>
		/// 北京2010车展
		/// </summary>
		[ConfigurationProperty("PriceRangeInterface", DefaultValue = "http://price.bitauto.com/Interface/xml/cspricescope.xml")]
		public string PriceRangeInterface
		{
			get { return (string)base["PriceRangeInterface"]; }
		}

		/// <summary>
		/// 推送数据到晶赞的url地址
		/// cid=12 固定值；action：删除=1，其他是0，对方接口约定；
		/// </summary>
		[ConfigurationProperty("PostSerialDataToJingZanUrl")]
		public string PostSerialDataToJingZanUrl
		{
			get { return (string)base["PostSerialDataToJingZanUrl"]; }
		}

		/// <summary>
		/// 子品牌口碑报告
		/// </summary>
		[ConfigurationProperty("SerialKouBeiReportUrl", DefaultValue = "")]
		public string SerialKouBeiReportUrl
		{
			get { return (string)base["SerialKouBeiReportUrl"]; }
		}

		/// <summary>
		/// 子品牌口碑报告
		/// </summary>
		[ConfigurationProperty("SerialKouBeiReportUrlNew", DefaultValue = "")]
		public string SerialKouBeiReportUrlNew
		{
			get { return (string)base["SerialKouBeiReportUrlNew"]; }
		}

		/// <summary>
		/// 子品牌综述页 点评精选 标签 URL
		/// </summary>
		[ConfigurationProperty("KouBeiSerialDianpingTagsUrl", DefaultValue = "")]
		public string KouBeiSerialDianpingTagsUrl
		{
			get { return (string)base["KouBeiSerialDianpingTagsUrl"]; }
		}

		/// <summary>
		/// 二手车价格区间接口
		/// </summary>
		[ConfigurationProperty("UcarSerialPrice", DefaultValue = "")]
		public string UcarSerialPrice
		{
			get { return (string)base["UcarSerialPrice"]; }
		}

		/// <summary>
		/// 子品牌最热新闻视频
		/// </summary>
		[ConfigurationProperty("SerialCmsHotVideoUrl", DefaultValue = "")]
		public string SerialCmsHotVideoUrl
		{
			get { return (string)base["SerialCmsHotVideoUrl"]; }
		}

		/// <summary>
		/// 新 视频数据接口
		/// </summary>
		[ConfigurationProperty("VideoNewUrl", DefaultValue = "")]
		public string VideoNewUrl
		{
			get { return (string)base["VideoNewUrl"]; }
		}

		/// <summary>
		/// 新 最热视频接口
		/// </summary>
		[ConfigurationProperty("SerialCmsHotVideoNewUrl", DefaultValue = "")]
		public string SerialCmsHotVideoNewUrl
		{
			get { return (string)base["SerialCmsHotVideoNewUrl"]; }
		}

		/// <summary>
		/// 新 视频分类接口
		/// </summary>
		[ConfigurationProperty("VideoCategoryNewUrl", DefaultValue = "")]
		public string VideoCategoryNewUrl
		{
			get { return (string)base["VideoCategoryNewUrl"]; }
		}

		/// <summary>
		/// 有视频的子品牌ID
		/// </summary>
		[ConfigurationProperty("VideoHasSerialIDUrl", DefaultValue = "")]
		public string VideoHasSerialIDUrl
		{
			get { return (string)base["VideoHasSerialIDUrl"]; }
		}

		/// <summary>
		/// 车贷子品牌列表
		/// </summary>
		[ConfigurationProperty("CheDaiCsList", DefaultValue = "")]
		public string CheDaiCsList
		{
			get { return (string)base["CheDaiCsList"]; }
		}

		/// <summary>
		/// 子品牌好 中 差 口碑点评 地址
		/// </summary>
		[ConfigurationProperty("KouBeiSerialDianpingUrl", DefaultValue = "")]
		public string KouBeiSerialDianpingUrl
		{
			get { return (string)base["KouBeiSerialDianpingUrl"]; }
		}

		/// <summary>
		/// 车贷套餐
		/// </summary>
		[ConfigurationProperty("CarLoanPackageXmlUrl", DefaultValue = "")]
		public string CarLoanPackageXmlUrl
		{
			get { return (string)base["CarLoanPackageXmlUrl"]; }
		}

		/// <summary>
		/// 车型首页问答接口地址
		/// </summary>
		[ConfigurationProperty("AskDefaultUrl", DefaultValue = "")]
		public string AskDefaultUrl
		{
			get { return (string)base["AskDefaultUrl"]; }
		}

		/// <summary>
		/// 主品牌问答接口地址
		/// </summary>
		[ConfigurationProperty("AskMasterUrl", DefaultValue = "")]
		public string AskMasterUrl
		{
			get { return (string)base["AskMasterUrl"]; }
		}

		/// <summary>
		/// 子品牌问答接口地址
		/// </summary>
		[ConfigurationProperty("AskSerialUrl", DefaultValue = "")]
		public string AskSerialUrl
		{
			get { return (string)base["AskSerialUrl"]; }
		}

		/// <summary>
		/// 问答专家
		/// </summary>
		[ConfigurationProperty("AskExpertUrl", DefaultValue = "")]
		public string AskExpertUrl
		{
			get { return (string)base["AskExpertUrl"]; }
		}

		/// <summary>
		/// 综述页 团购线索
		/// </summary>
		[ConfigurationProperty("SerialTuanUrl", DefaultValue = "")]
		public string SerialTuanUrl
		{
			get { return (string)base["SerialTuanUrl"]; }
		}
		/// <summary>
        /// SUV 销量排行 URL
		/// </summary>
		[ConfigurationProperty("SUVSaleRankUrl", DefaultValue = "")]
		public string SUVSaleRankUrl
		{
			get { return (string)base["SUVSaleRankUrl"]; }
		}
        /// <summary>
        /// 获取第四级商配文章
        /// </summary>
        [ConfigurationProperty("SerialCommerceNewsUrl", DefaultValue = "")]
        public string SerialCommerceNewsUrl
        {
            get { return (string)base["SerialCommerceNewsUrl"]; }
        }
        /// <summary>
        /// 获取第四级后补商配新闻
        /// </summary>
        [ConfigurationProperty("BackupCommerceNewsUrl", DefaultValue = "")]
        public string BackupCommerceNewsUrl
        {
            get { return (string)base["BackupCommerceNewsUrl"]; }
        }

		[ConfigurationProperty("UserFuelUrl", DefaultValue = "")]
		public string UserFuelUrl
		{
			get { return (string)base["UserFuelUrl"]; }
		}
        /// <summary>
        /// 新车上市 后台配置文件路径
        /// </summary>
        [ConfigurationProperty("SerialMarketTimeConfigPath", DefaultValue = "")]
	    public string SerialMarketTimeConfigPath
	    {
	        get { return (string) base["SerialMarketTimeConfigPath"]; }
	    }

		/// <summary>
		/// 口碑评分明细路径
		/// </summary>
		[ConfigurationProperty("KoubeiDetailRatingUrl",DefaultValue="")]
		public string KoubeiDetailRatingUrl
		{
			get { return (string)base["KoubeiDetailRatingUrl"]; }
		}

		/// <summary>
		/// 口碑明细url
		/// </summary>
		[ConfigurationProperty("KoubeiDetailUrl", DefaultValue = "")]
		public string KoubeiDetailUrl
		{
			get { return (string)base["KoubeiDetailUrl"]; }
		}

		/// <summary>
		/// 分布式文件地址
		/// </summary>
		[ConfigurationProperty("FileServerPath", DefaultValue = "")]
		public string FileServerPath
		{
			get { return (string)base["FileServerPath"]; }
		}

		/// <summary>
		/// 易车Logo，本地地址
		/// </summary>
		[ConfigurationProperty("YiCheLogoLocalPath", DefaultValue = "")]
		public string YiCheLogoLocalPath
		{
			get { return (string)base["YiCheLogoLocalPath"]; }
		}
	}
}
