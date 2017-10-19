using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using System.Data;
using System.Collections;
using System.Xml;
using BitAuto.Utils.Data;
using System.IO;
using System.Text.RegularExpressions;
using BitAuto.Utils;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Configuration;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class CommonNavigation
    {
        #region  公用变量
        // 子品牌数据
        private DataSet dsSerial = new DataSet();
        // 车型数据
        private DataSet dsCar = new DataSet();
        // 子品牌PV数据
        private Hashtable htSerialPV = new Hashtable();
        // 子品牌论坛地址
        private DataSet dsBBS = new DataSet();
        // 子品牌所有城市排行
        private DataSet dsEveryCityUV = new DataSet();
        // 子品牌CTCC
        private DataSet dsSerialCTCC = new DataSet();
        // 子品牌车展数据
        private Hashtable htSerialExhibition = new Hashtable();
        // 子品牌北京2010车展
        private Hashtable htSerialBeiJing2010 = new Hashtable();
        // 子品牌广州2010车展
        private Hashtable htSerialGuangZhou2010 = new Hashtable();
        // 子品牌 分类新闻的条数
        private XmlDocument docNewsCategory = new XmlDocument();
        // 子品牌 年款信息
        private DataSet dsSerialYear = new DataSet();
        // 是否有停销车型
        private DataSet dsNoSale = new DataSet();
        // 有保养信息的子品牌
        //private Hashtable htBaoYang = new Hashtable();
        // 有销量数据的子品牌
        // private Hashtable htProduceAndSell = new Hashtable();
        private List<int> listProduceAndSell = new List<int>();

        // 子品牌包含的停销车型的年款
        private Dictionary<int, List<int>> dicSerialNoSaleYear = new Dictionary<int, List<int>>();
        // 子品牌及车型导航头
        private string SerialHeadSummary = string.Empty;
        // 子品牌综述页导航头脚本 add by chengl Jul.27.2012
        private string SerialHeadSummaryJs = string.Empty;
        //private string serialHead = string.Empty;
        private string serialHeadNew = string.Empty;
        private string serialHeadNoCrumb = string.Empty;
        private string serialHeadForCMS = string.Empty;
        private string SerialHeadForUCar = string.Empty;
        private string serialHeadForAsk = string.Empty;
        //private string carHead = string.Empty;
        //private string serialHeadForYear = string.Empty;
        // 魔方计划页面迁移临时模板 add by chengl Jun.27.2014
        private string carHeadNew = string.Empty;
        private string serialHeadForYearNew = string.Empty;

        // 概念车带面包削 add by chengl May.15.2012
        private string serialHeadForConcept = string.Empty;
        // add by chengl Jul.8.2014 概念车新模板
        private string serialHeadForConceptNew = string.Empty;
        // 概念车不带面包屑 add by chengl May.15.2012
        private string serialHeadForNoCrumbConcept = string.Empty;
        private string noHead = "<!-- 概念车无导航 " + DateTime.Now.ToString() + " -->";
        // 热搜块内容
        private string SearchHot = string.Empty;
        //移动站车系导航头模板
        private string SerialHeadForM = string.Empty;

        #region 互联互通导航 V2版本模板
        private string SerialHeadSummaryV2 = string.Empty;
        private string SerialHeadSummaryJsV2 = string.Empty;
        private string serialHeadNewV2 = string.Empty;
        private string serialHeadNewOld = string.Empty;
        private string serialHeadNoCrumbV2 = string.Empty;
        private string serialHeadForCMSV2 = string.Empty;
        private string serialHeadForAskV2 = string.Empty;
        private string carHeadNewV2 = string.Empty;
        private string carHeadNewOld = string.Empty;
        private string serialHeadForYearNewV2 = string.Empty;
        private string serialHeadForConceptNewV2 = string.Empty;
        private string serialHeadForConceptNewOld = string.Empty;
        private string serialHeadForNoCrumbConceptV2 = string.Empty;
        private string SearchHotV2 = string.Empty;
        private Dictionary<int, string> dicSoBarV2 = new Dictionary<int, string>();
        #endregion

        private IRazorEngineService _razorEngineService;
        //2011,2012年的十佳车型数据
        // private Dictionary<int, List<int>> Top10CarList;

        // 年度10佳车 add by chengl Dec.5.2013
        private List<Define.BestTopCar> listBestTopCar;

        // 评测数据采用彩虹条 60 车型详解
        private Dictionary<int, string> dicPingCeFromRainbow = new Dictionary<int, string>();
        // 同时包含国产进口的主品牌
        private List<int> listMasterCpCountry = new List<int>();
        // 子品牌最高降幅
        private Dictionary<int, string> dicCsJiangJiaPrice = new Dictionary<int, string>();
        // 子品牌报价区间
        private Dictionary<int, string> dicCsPriceRange = new Dictionary<int, string>();
        // 子品牌搜索配置
        private Dictionary<int, string> dicSoBar = new Dictionary<int, string>();
        // 子品牌改款上市时间
        private Dictionary<int, SerialMarketTimeEntity> dictCarMarkTime = new Dictionary<int, SerialMarketTimeEntity>();

        private Dictionary<int, bool> dictHasSerialNews = new Dictionary<int, bool>();        

        // 子品牌易车惠
        // private Dictionary<int, string> dicCsGoods = new Dictionary<int, string>();

        // 车贷子品牌ID列表
        // add by chengl 增加子品牌
        // private List<int> listCheDaiCsID = new List<int>();
        #endregion

        public CommonNavigation()
        {
            this.InitData();
        }
        private int m_CsID = -1;
        /// <summary>
        /// 控制台制定子品牌ID参数
        /// </summary>
        public int CsIDFromArgs
        {
            get { return m_CsID; }
            set { m_CsID = value; }
        }

        private int m_TagID = -1;
        /// <summary>
        /// 控制台制定标签ID参数
        /// </summary>
        public int TagIDFromArgs
        {
            get { return m_TagID; }
            set { m_TagID = value; }
        }
        /// <summary>
        /// 保存目录
        /// </summary>
        public List<string> SaveStaticDataResNAS
        {
            get
            {
                List<string> listNasArray = new List<string>();
                string nasPathArray = CommonData.CommonSettings.StaticDataResNAS;
                string[] arrayPath = nasPathArray.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string path in arrayPath)
                {
                    if (!listNasArray.Contains(path.Trim()))
                    { listNasArray.Add(path.Trim()); }
                }
                return listNasArray;
            }
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            // 导航头
            SerialHeadSummary = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadSummary.htm");
            // 子品牌综述页导航头脚本 add by chengl Jul.27.2012
            SerialHeadSummaryJs = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadSummaryjs.htm");
            //serialHead = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHead.htm");
            serialHeadNew = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadNew.htm");
            serialHeadNoCrumb = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "serialHeadNoCrumb.htm");
            serialHeadForCMS = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadForCMS.htm");
            //serialHeadForYear = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadForYear.htm");
            serialHeadForYearNew = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadForYearNew.htm");
            SerialHeadForUCar = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadForUCar.htm");
            serialHeadForAsk = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "serialHeadForAsk.htm");
            //serialHeadForConcept = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "serialHeadForConcept.htm");
            serialHeadForConceptNew = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "serialHeadForConceptNew.htm");
            serialHeadForNoCrumbConcept = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadForNoCrumbConcept.htm");
            //carHead = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "CarHead.htm");
            carHeadNew = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "CarHeadNew.htm");
            SearchHot = CommonFunction.GetContentByUrl("http://admin.bitauto.com//include/special/yc/00001/hotwords_Manual.shtml", "utf-8");

            #region 1200 导航模板
            // 导航头
            SerialHeadSummaryV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/SerialHeadSummary.htm");
            SerialHeadSummaryJsV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/SerialHeadSummaryjs.htm");
            serialHeadNewV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/SerialHeadNew.htm");
            serialHeadNewOld = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/SerialHeadNewOld.htm");

            serialHeadNoCrumbV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/serialHeadNoCrumb.htm");
            serialHeadForCMSV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/SerialHeadForCMS.htm");
            serialHeadForYearNewV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/SerialHeadForYearNew.htm");

            serialHeadForAskV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/serialHeadForAsk.htm");
            //serialHeadForConceptV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "serialHeadForConcept.htm");
            serialHeadForConceptNewV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/serialHeadForConceptNew.htm");
            serialHeadForConceptNewOld = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/serialHeadForConceptNewOld.htm");

            serialHeadForNoCrumbConceptV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/SerialHeadForNoCrumbConcept.htm");
            //carHead = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "CarHead.htm");
            carHeadNewV2 = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/CarHeadNew.htm");
            carHeadNewOld = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "NavgationTemplates/CarHeadOld.htm");
            SearchHotV2 = CommonFunction.GetContentByUrl("http://admin.bitauto.com//include/special/yc/00001/hotwords_Manual.shtml", "utf-8");
            #endregion

            //add by sk 2016.04.07
            // 车系 移动站 导航头
            SerialHeadForM = CommonFunction.ReadFile(CommonData.CommonSettings.CommonHeadLocalFilePath + "SerialHeadForM.htm");

            // 子品牌信息
            dsSerial = CommonNavigationService.GetSerialAllInfoForCommonNavigation();
            // 子品牌PV信息
            htSerialPV = CommonNavigationService.GetAllSerialPV();
            // 子品牌论坛地址
            dsBBS = CommonNavigationService.BBSAllUrl();
            // 子品牌所有城市排行
            dsEveryCityUV = CommonNavigationService.GetSerialCityPVByLevel();
            // 子品牌CTCC
            dsSerialCTCC = CommonNavigationService.GetAllSerialCTCCURL();
            //// 子品牌 车展信息
            //htSerialExhibition = CommonNavigationService.GetSerialByExhibitionID(5);
            // 子品牌 分类新闻的条数
            docNewsCategory = CommonNavigationService.GetAllSerialNewsCategoryCount();

            //dictHasSerialNews = CommonNavigationService.HasAllSerialNews();

            //// 子品牌北京车展
            //GetExhibitionBeiJing2010XML();
            //// 子品牌2010广州车展
            //GetExhibitionGuangZhou2010XML();
            // 车型数据
            dsCar = CommonNavigationService.GetCarAllInfoForCommonNavigation();
            // 子品牌年款
            dsSerialYear = CommonNavigationService.GetAllCarYearByCsID();
            // 是否有停销车型
            dsNoSale = CommonNavigationService.GetHasNoSaleSerial();
            // 有保养信息的子品牌
            //htBaoYang = CommonNavigationService.GetAllSerialBaoYang();
            // 有销量信息的子品牌
            // htProduceAndSell = CommonNavigationService.GetAllSerialProduceAndSell();
            listProduceAndSell = CommonNavigationService.GetAllSerialSale();
            // add by chengl Jan.16.2012
            // 评测文章以彩虹条 60 车型详解
            dicPingCeFromRainbow = CommonNavigationService.GetPingceInfo();
            // add by chengl Jun.1.2012
            // 子品牌包含的停销车型的年款
            dicSerialNoSaleYear = CommonNavigationService.GetSerialNoSaleYear();
            //年度十佳车型
            // Top10CarList = CommonNavigationService.GetTop10CarDic();
            listBestTopCar = CommonNavigationService.GetAllBestTopCar();
            // 同时包含国产进口的主品牌
            listMasterCpCountry = CommonNavigationService.GetAllMasterBrandCpCountry();
            // 子品牌最高降幅
            dicCsJiangJiaPrice = CommonNavigationService.GetAllSerialJiangJia();
            // 子品牌报价区间
            dicCsPriceRange = CommonData.CsPriceRangeDic; //CommonNavigationService.GetSerialPriceRange();
            // 搜索条配置
            dicSoBar = CommonNavigationService.GetSoBarByTag();
            dicSoBarV2 = CommonNavigationService.GetSoBarByTagV2();
            // 车贷子品牌列表
            // listCheDaiCsID = CommonNavigationService.GetCheDaiCsList();
            // 子品牌全国易车惠信息
            // dicCsGoods = CommonNavigationService.GetAllSerialGoods();
            dictCarMarkTime = CommonNavigationService.GetSerialMarketTimeData();
            //RazorEngine 配置
            var templateConfig = new TemplateServiceConfiguration
            {
                //TemplateManager = new DelegateTemplateManager(controllerAndAction => File.ReadAllText(Path.Combine(_conventionFolderPath, controllerAndAction))),
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { })
            };
            _razorEngineService = RazorEngineService.Create(templateConfig);
            Engine.Razor = _razorEngineService;
        }

        #region 生成导航头
        /// <summary>
        /// 生成 移动站 车系 导航头
        /// </summary>
        /// <param name="serialId"></param>
        public void GenerateSerialNavigationM(int serialId)
        {
            try
            {
                if (dsSerial != null && dsSerial.Tables.Count > 0 && dsSerial.Tables[0].Rows.Count > 0)
                {
                    if (serialId <= 0) return;
                    DataRow[] drArr = dsSerial.Tables[0].Select(string.Format(" cs_id={0} ", serialId.ToString()));
                    if (drArr == null || drArr.Length <= 0) return;
                    DataRow dr = drArr[0];
                    //生成业务线导航 0：综述页，1：参数，2：图片，3：油耗，4：文章，5：口碑，8：报价，10：养护，11：经销商
                    Dictionary<int, string> dict = new Dictionary<int, string>() {
                        { 0, "MCsSummary" },
                        { 1, "MCsCompare" },
                        { 2, "MCsPhoto" },
                        { 8, "MCsPrice" },
                        { 5, "MCsKouBei" },
                        { 3, "MCsYouHao" },
                        { 4, "MCsWenZhang" },
                        { 10, "MCsMaintenance" },
                        { 11, "MCsDealer" },
                        { 12, "MCsVideo"}
                    };

                    foreach (KeyValuePair<int, string> kv in dict)
                    {
                        var model = new
                        {
                            TagName = kv.Value,
                            TagId = kv.Key,
                            BBSUrl = this.GetBBSURLByCsID(serialId),
                            Data = dr
                            //,AllSpell = dr["csAllSpell"],
                            //SerialId = dr["cs_id"]
                        };
                        //string result = Razor.Parse(SerialHeadForM, model);
                        //var result = Engine.Razor.RunCompile(SerialHeadForM, "templateKey", null, model);
                        var result = _razorEngineService.RunCompile(SerialHeadForM, "templateKey", null, model);

                        // CommonFunction.InsertCommonHead(serialId, kv.Value, result);
                        CommonFunction.InsertCommonHeadV2(serialId, kv.Value, result);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("生成移动站导航异常，serialid=" + serialId + ex.ToString());
            }
        }


        /// <summary>
        /// 生成子品牌导航条
        /// </summary>
        /// <param name="serialId"></param>
        public void GenerateSerialNavigation(int serialId)
        {
            string csHead = "";// GetCommonHTMLContentReplace(dr, serialHead);
            // string csHeadNoCrumb = "";// GetCommonHTMLContentReplace(dr, serialHeadNoCrumb);

            int tagID = 0;
            string tagMianBao = string.Empty;
            if (dsSerial != null && dsSerial.Tables.Count > 0 && dsSerial.Tables[0].Rows.Count > 0)
            {
                if (serialId <= 0) return;
                DataRow[] drArr = dsSerial.Tables[0].Select(string.Format(" cs_id={0} ", serialId.ToString()));
                if (drArr == null || drArr.Length <= 0) return;
                DataRow dr = drArr[0];

                // 子品牌 综述
                GenerateSerialHead(0, serialHeadForConceptNew, SerialHeadSummary, dr, false, "CsSummary");
                // 子品牌 综述脚本
                GenerateSerialHead(0, "", SerialHeadSummaryJs, dr, false, "CsSummaryJs");
                // 子品牌 参数配置
                // tagID = 1;
                GenerateSerialHead(1, serialHeadForConceptNew, serialHeadNew, dr, false, "CsCompare");
                // 子品牌 图片
                //tagID = 2;
                GenerateSerialHead(2, serialHeadForConceptNew, serialHeadNew, dr, false, "CsPhoto");
                // 子品牌 视频
                //tagID = 3;
                GenerateSerialHead(3, serialHeadForConceptNew, serialHeadNew, dr, false, "CsVideo");
                // 子品牌 口碑
                //tagID = 5;
                GenerateSerialHead(5, serialHeadForNoCrumbConcept, serialHeadNoCrumb, dr, false, "CsKouBei");
                // 子品牌 问答
                //tagID = 6;
                GenerateSerialHead(6, serialHeadForConceptNew, serialHeadForAsk, dr, false, "CsAsk");
                // 子品牌 油耗
                //tagID = 11;
                GenerateSerialHead(11, serialHeadForNoCrumbConcept, serialHeadNoCrumb, dr, false, "CsYouHao");
                // 子品牌 导购
                //tagID = 8;
                GenerateSerialHead(8, serialHeadForConceptNew, serialHeadNew, dr, false, "CsDaoGou");
                // 子品牌 安全
                //tagID = 10;
                GenerateSerialHead(10, serialHeadForConceptNew, serialHeadNew, dr, false, "CsAnQuan");
                // 子品牌 经销商报价
                //tagID = 12;
                GenerateSerialHead(12, serialHeadForConceptNew, serialHeadNew, dr, false, "CsPrice");
                // 子品牌 销售数据
                //tagID = 18;
                GenerateSerialHead(18, serialHeadForConceptNew, serialHeadNew, dr, false, "CsSellData");
                // 子品牌 区域车型页
                //tagID = 13;
                GenerateSerialHead(13, serialHeadForNoCrumbConcept, serialHeadNoCrumb, dr, false, "CsCity");
                // 子品牌 新闻
                //tagID = 19;
                GenerateSerialHead(19, serialHeadForConceptNew, serialHeadNew, dr, false, "CsXinWen");
                // 子品牌 行情
                //tagID = 20;
                //GenerateSerialHead(20, serialHeadForConcept, serialHead, dr, false, "CsHangQing");
                // 子品牌 用车
                //tagID = 21;
                GenerateSerialHead(21, serialHeadForConceptNew, serialHeadNew, dr, false, "CsYongChe");
                // 子品牌 评测
                //tagID = 22;
                GenerateSerialHead(22, serialHeadForConceptNew, serialHeadNew, dr, false, "CsPingCe");
                // 子品牌 CMS头
                //tagID = 23;
                GenerateSerialHead(23, serialHeadForNoCrumbConcept, serialHeadForCMS, dr, false, "CsCMSNews");
                // 子品牌 销售地图
                //tagID = 25;
                //GenerateSerialHead(25, serialHeadForConcept, serialHead, dr, false, "CsSellDataMap");
                // 子品牌 二手车
                //tagID = 26;
                // GenerateSerialHead(26, serialHeadForConcept, SerialHeadForUCar, dr, false, "CsUcar");
                // 子品牌 排行榜
                //tagID = 27;
                GenerateSerialHead(27, serialHeadForConceptNew, serialHeadNew, dr, false, "CsPaiHang");
                // 子品牌 试驾
                //tagID = 28;
                GenerateSerialHead(28, serialHeadForConceptNew, serialHeadNew, dr, false, "CsShiJia");
                // 子品牌 保养信息
                //tagID = 32;
                GenerateSerialHead(32, serialHeadForConceptNew, serialHeadNew, dr, false, "CsMaintenance");
                // 子品牌 改装
                //tagID = 34;
                GenerateSerialHead(34, serialHeadForConceptNew, serialHeadNew, dr, false, "CsGaiZhuang");
                // 子品牌 养车费用
                //tagID = 35;
                GenerateSerialHead(35, serialHeadForNoCrumbConcept, serialHeadNoCrumb, dr, false, "CsYangChe");
                // add by chengl Jun.12.2012
                // 子品牌 置换
                //tagID = 36;
                // GenerateSerialHead(36, serialHeadForConcept, serialHead, dr, false, "CsZhiHuan");
                // 子品牌 降价
                GenerateSerialHead(38, serialHeadForConceptNew, serialHeadNew, dr, false, "CsJiangJia");

                // 子品牌 车贷
                GenerateSerialHead(40, serialHeadForConceptNew, serialHeadNew, dr, false, "CsCheDai");

                // 子品牌 文章
                GenerateSerialHead(41, serialHeadForConceptNew, serialHeadNew, dr, false, "CsWenZhang");

                #region 年款
                // 子品牌 年款综述
                tagID = 0;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; <strong>{0}款</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsSummaryForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsSummaryForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款参数配置
                tagID = 1;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsCompareForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsCompareForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款图片
                tagID = 2;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsPhotoForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsPhotoForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款导购
                tagID = 8;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsDaoGouForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsDaoGouForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款安全 add by chengl Jun.12.2012
                tagID = 10;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsAnQuanForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsAnQuanForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款新闻
                tagID = 19;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsXinWenForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsXinWenForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                //// 子品牌 年款行情
                //tagID = 20;
                //csHead = serialHeadForYear.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                //csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                //CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsHangQingForYear", csHead);
                //// CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsHangQingForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款用车
                tagID = 21;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsYongCheForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsYongCheForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款评测
                tagID = 22;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsPingCeForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsPingCeForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款试驾
                tagID = 28;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsShiJiaForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsShiJiaForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款改装
                tagID = 34;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsGaiZhuangForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsGaiZhuangForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款文章
                tagID = 41;
                csHead = serialHeadForYearNew.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsWenZhangForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsWenZhangForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                #endregion
            }
            GenerateSerialNavigationV2(serialId);
        }

        /// <summary>
        /// 生成子品牌导航条
        /// </summary>
        /// <param name="serialId"></param>
        public void GenerateSerialNavigationV2(int serialId)
        {
            string csHead = "";// GetCommonHTMLContentReplace(dr, serialHead);
            // string csHeadNoCrumb = "";// GetCommonHTMLContentReplace(dr, serialHeadNoCrumb);

            int tagID = 0;
            string tagMianBao = string.Empty;
            if (dsSerial != null && dsSerial.Tables.Count > 0 && dsSerial.Tables[0].Rows.Count > 0)
            {
                if (serialId <= 0) return;
                DataRow[] drArr = dsSerial.Tables[0].Select(string.Format(" cs_id={0} ", serialId.ToString()));
                if (drArr == null || drArr.Length <= 0) return;
                DataRow dr = drArr[0];

                // 子品牌 综述
                GenerateSerialHeadV2(0, serialHeadForConceptNewV2, SerialHeadSummaryV2, dr, false, "CsSummary");
                // 子品牌 综述脚本
                GenerateSerialHeadV2(0, "", SerialHeadSummaryJsV2, dr, false, "CsSummaryJs");
                // 子品牌 参数配置
                // tagID = 1;
                GenerateSerialHeadV2(1, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsCompare");
                // 子品牌 图片
                //tagID = 2;
                GenerateSerialHeadV2(2, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsPhoto");
                // 子品牌 视频
                //tagID = 3;
                GenerateSerialHeadV2(3, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsVideo");
                // 子品牌 口碑
                //tagID = 5;
                GenerateSerialHeadV2(5, serialHeadForNoCrumbConceptV2, serialHeadNoCrumbV2, dr, false, "CsKouBei");
                // 子品牌 问答
                //tagID = 6;
                GenerateSerialHeadV2(6, serialHeadForConceptNewV2, serialHeadForAskV2, dr, false, "CsAsk");
                // 子品牌 油耗
                //tagID = 11;
                GenerateSerialHeadV2(11, serialHeadForNoCrumbConceptV2, serialHeadNoCrumbV2, dr, false, "CsYouHao");
                // 子品牌 导购
                //tagID = 8;
                GenerateSerialHeadV2(8, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsDaoGou");
                //// 子品牌 安全
                ////tagID = 10;
                //GenerateSerialHeadV2(10, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsAnQuan");
                // 子品牌 经销商报价
                //tagID = 12;
                GenerateSerialHeadV2(12, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsPrice");
                // 子品牌 销售数据
                //tagID = 18;
                GenerateSerialHeadV2(18, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsSellData");
                // 子品牌 区域车型页
                //tagID = 13;
                GenerateSerialHeadV2(13, serialHeadForNoCrumbConceptV2, serialHeadNoCrumbV2, dr, false, "CsCity");
                // 子品牌 新闻
                //tagID = 19;
                GenerateSerialHeadV2(19, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsXinWen");
                // 子品牌 行情
                //tagID = 20;
                //GenerateSerialHeadV2(20, serialHeadForConcept, serialHead, dr, false, "CsHangQing");
                // 子品牌 用车
                //tagID = 21;
                GenerateSerialHeadV2(21, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsYongChe");
                // 子品牌 评测
                //tagID = 22;
                GenerateSerialHeadV2(22, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsPingCe");
                // 子品牌 CMS头
                //tagID = 23;
                GenerateSerialHeadV2(23, serialHeadForNoCrumbConceptV2, serialHeadForCMSV2, dr, false, "CsCMSNews");
                // 子品牌 销售地图
                //tagID = 25;
                //GenerateSerialHeadV2(25, serialHeadForConcept, serialHead, dr, false, "CsSellDataMap");
                // 子品牌 二手车
                //tagID = 26;
                // GenerateSerialHeadV2(26, serialHeadForConcept, SerialHeadForUCar, dr, false, "CsUcar");
                // 子品牌 排行榜
                //tagID = 27;
                GenerateSerialHeadV2(27, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsPaiHang");
                // 子品牌 试驾
                //tagID = 28;
                GenerateSerialHeadV2(28, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsShiJia");
                // 子品牌 保养信息
                //tagID = 32;
                GenerateSerialHeadV2(32, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsMaintenance");
                //// 子品牌 改装
                ////tagID = 34;
                //GenerateSerialHeadV2(34, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsGaiZhuang");
                // 子品牌 养车费用
                //tagID = 35;
                GenerateSerialHeadV2(35, serialHeadForNoCrumbConceptV2, serialHeadNoCrumbV2, dr, false, "CsYangChe");
                // add by chengl Jun.12.2012
                // 子品牌 置换
                //tagID = 36;
                // GenerateSerialHeadV2(36, serialHeadForConcept, serialHead, dr, false, "CsZhiHuan");
                // 子品牌 降价
                GenerateSerialHeadV2(38, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsJiangJia");

                //// 子品牌 车贷
                //GenerateSerialHeadV2(40, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsCheDai");

                // 子品牌 文章
                GenerateSerialHeadV2(41, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsWenZhang");

                // 子品牌 科技
                GenerateSerialHeadV2(42, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsKeJi");

                // 子品牌 文化
                GenerateSerialHeadV2(43, serialHeadForConceptNewV2, serialHeadNewV2, dr, false, "CsWenHua");

                #region 年款
                // 子品牌 年款综述
                tagID = 0;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; <strong>{0}款</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsSummaryForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsSummaryForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款参数配置
                tagID = 1;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsCompareForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsCompareForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款图片
                tagID = 2;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsPhotoForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsPhotoForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款导购
                tagID = 8;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsDaoGouForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsDaoGouForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                //// 子品牌 年款安全 add by chengl Jun.12.2012
                //tagID = 10;
                //csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                //csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                //CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsAnQuanForYear", csHead);
                //// CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsAnQuanForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款新闻
                tagID = 19;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsXinWenForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsXinWenForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                //// 子品牌 年款行情
                //tagID = 20;
                //csHead = serialHeadForYear.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                //csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, true);
                //CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "CsHangQingForYear", csHead);
                //// CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsHangQingForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款用车
                tagID = 21;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsYongCheForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsYongCheForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款评测
                tagID = 22;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsPingCeForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsPingCeForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款试驾
                tagID = 28;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsShiJiaForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsShiJiaForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                //// 子品牌 年款改装
                //tagID = 34;
                //csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                //csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                //CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsGaiZhuangForYear", csHead);
                //// CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsGaiZhuangForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                // 子品牌 年款文章
                tagID = 41;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsWenZhangForYear", csHead);

                // 子品牌 年款科技
                tagID = 42;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsKeJiForYear", csHead);

                // 子品牌 年款文化
                tagID = 43;
                csHead = serialHeadForYearNewV2.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<a href=\"http://car.bitauto.com/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/{0}/\" target=\"_self\"> {0}款" + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, true);
                CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), "CsWenHuaForYear", csHead);
                // CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\CsWenZhangForYear\\" + dr["cs_id"].ToString() + ".shtml", Encoding.Unicode);

                #endregion


                return;

            }
        }
        /// <summary>
        /// 生成子品牌导航条下的信息条
        /// </summary>
        /// <param name="serialId"></param>
        public void GenerateSerialBarInfo(int serialId)
        {
            if (dsSerial != null && dsSerial.Tables.Count > 0 && dsSerial.Tables[0].Rows.Count > 0)
            {
                if (serialId <= 0) return;
                DataRow[] drArr = dsSerial.Tables[0].Select(string.Format(" cs_id={0}", serialId.ToString()));
                if (drArr == null || drArr.Length <= 0) return;
                DataRow dr = drArr[0];
                int csid = int.Parse(dr["cs_id"].ToString());
                string serialSpell = dr["csAllSpell"].ToString().Trim().ToLower();
                string[] htmlCode = new string[10];
                List<DataRow> listCar = new List<DataRow>();
                DataSet dsCar = null;
                #region 取相应车型数据
                if (dr["CsSaleState"].ToString().Trim() == "停销")
                {
                    dsCar = CommonNavigationService.GetNoSaleCarListByCsID(csid);
                    if (dsCar != null && dsCar.Tables.Count > 0 && dsCar.Tables[0].Rows.Count > 0)
                    {
                        int newYear = 0;
                        foreach (DataRow drCar in dsCar.Tables[0].Rows)
                        {
                            // 停销子品牌只取最新年款
                            int currentYear = 0;
                            if (int.TryParse(drCar["Car_YearType"].ToString(), out currentYear))
                            { }
                            if (currentYear < 1)
                            { break; }
                            if (newYear > 0 && currentYear > 0 && newYear != currentYear)
                            { break; }
                            newYear = currentYear;
                            listCar.Add(drCar);
                        }
                    }
                }
                else
                {
                    dsCar = CommonNavigationService.GetSaleCarByCsID(csid);
                    if (dsCar != null && dsCar.Tables.Count > 0 && dsCar.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow drCar in dsCar.Tables[0].Rows)
                        { listCar.Add(drCar); }
                    }
                }
                #endregion

                string serialPrice = "暂无报价";
                if (dr["CsSaleState"].ToString().Trim() == "停销")
                { serialPrice = "停售"; }
                else if (dr["CsSaleState"].ToString().Trim() == "待销")
                { serialPrice = "未上市"; }
                else
                { serialPrice = dicCsPriceRange.ContainsKey(csid) ? dicCsPriceRange[csid] : "暂无报价"; }

                if (listCar.Count > 0)
                {
                    //排量列表
                    List<string> exhaustList = new List<string>();
                    //变速箱列表
                    List<string> transList = new List<string>();
                    string serialReferPrice = "";
                    string serialExhaust = "";
                    string serialTransmission = "";
                    double maxPrice = Double.MinValue;
                    double minPrice = Double.MaxValue;
                    foreach (DataRow drCar in listCar)
                    {
                        #region 指导价
                        double referPrice = 0.0;
                        bool isDouble = Double.TryParse(drCar["car_ReferPrice"].ToString().Replace("万", ""), out referPrice);
                        if (isDouble)
                        {
                            if (referPrice > maxPrice)
                                maxPrice = referPrice;
                            if (referPrice < minPrice)
                                minPrice = referPrice;
                        }
                        #endregion

                        #region 排量
                        if (!exhaustList.Contains(drCar["Engine_Exhaust"].ToString().Trim()))
                        {
                            exhaustList.Add(drCar["Engine_Exhaust"].ToString().Trim());
                        }
                        #endregion

                        #region 变速器类型
                        string tempTransmission = drCar["UnderPan_TransmissionType"].ToString().Trim();
                        if (tempTransmission.IndexOf("挡") >= 0)
                        {
                            tempTransmission = tempTransmission.Substring(tempTransmission.IndexOf("挡") + 1, tempTransmission.Length - tempTransmission.IndexOf("挡") - 1);
                        }
                        tempTransmission = tempTransmission.Replace("变速器", "").Replace("CVT", "");
                        if (transList.Count < 2)
                        {
                            if (tempTransmission.IndexOf("手动") == -1)
                                tempTransmission = "自动";
                            if (!transList.Contains(tempTransmission))
                                transList.Add(tempTransmission);
                        }
                        #endregion
                    }

                    if (maxPrice == Double.MinValue && minPrice == Double.MaxValue)
                    { serialReferPrice = "暂无"; }
                    else
                    { serialReferPrice = minPrice + "万-" + maxPrice + "万"; }

                    exhaustList.Remove("");
                    transList.Remove("");

                    //生成在售的排量与变速箱列表
                    if (exhaustList.Count > 5)
                    { serialExhaust = String.Join("　", new string[] { exhaustList[0], exhaustList[1], exhaustList[2] + "…" + exhaustList[exhaustList.Count - 1] }); }
                    else
                    { serialExhaust = String.Join("　", exhaustList.ToArray()); }
                    if (transList.Count > 3)
                    { serialTransmission = transList[0] + "　" + transList[1] + "…" + transList[transList.Count - 1]; }
                    else
                    { serialTransmission = String.Join("　", transList.ToArray()); }

                    htmlCode[0] = "<!-- " + DateTime.Now.ToString() + " --><div class=\"line_box zs01\">";
                    htmlCode[1] = "<ul class=\"s\">";
                    htmlCode[2] = "<li class=\"s1\"><label>厂家指导价：</label>" + serialReferPrice + "</li>";
                    if (serialPrice == "暂无报价" || serialPrice == "停售" || serialPrice == "未上市")
                        htmlCode[3] = "<li class=\"s2\"><label>参考成交价：</label><span class=\"important\">" + serialPrice + "</span></li>";
                    else
                        htmlCode[3] = "<li class=\"s2\"><label>参考成交价：</label><span class=\"important\"><a rel=\"nofollow\" href=\"http://car.bitauto.com/" + serialSpell + "/baojia/\"  target=\"_blank\">" + serialPrice + "</a></span></li>";
                    //if (dicCsHangQingPrice != null && dicCsHangQingPrice.Count > 0 && dicCsHangQingPrice.ContainsKey(csid))
                    //{ htmlCode[4] = "<li class=\"s2\"><label>行情价：</label><span class=\"important\"><a target=\"_blank\" id=\"linkForHQPrice\" href=\"http://car.bitauto.com/" + serialSpell + "/hangqing/\">" + dicCsHangQingPrice[csid] + "</a></span></li>"; }
                    //else
                    //{ htmlCode[4] = ""; }
                    if (dicCsJiangJiaPrice != null && dicCsJiangJiaPrice.Count > 0 && dicCsJiangJiaPrice.ContainsKey(csid))
                    {
                        htmlCode[4] = "<li class=\"s5\"><label>直降：</label><span><a target=\"_blank\" href=\"http://jiangjia.bitauto.com/nb" + csid.ToString() + "/\">" + dicCsJiangJiaPrice[csid] + "</a></span></li>";
                    }
                    else
                    {
                        htmlCode[4] = "";
                    }
                    htmlCode[5] = "<li class=\"s3\"><label>排量：</label>" + serialExhaust + "</li>";
                    htmlCode[6] = "<li class=\"s4\"><label>变速箱：</label>" + serialTransmission + "</li>";
                    htmlCode[7] = "</ul>";
                    htmlCode[8] = "<div class=\"clear\"></div>";
                    htmlCode[9] = "</div>";

                    // 导航增加memcache缓存
                    // key : Car_CommonHead_{子目录}_{子品牌ID}
                    // 例如 : Car_CommonHead_CsSummary_1991s
                    // CommonFunction.MemcacheInsert("Car_CommonHead_SerialInfoBar_" + dr["cs_id"].ToString(), String.Concat(htmlCode), Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
                    CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), "SerialInfoBar", String.Concat(htmlCode));

                    CommonFunction.SaveFileContent(String.Concat(htmlCode), SaveStaticDataResNAS, "\\CommonHead\\SerialInfoBar\\" + dr["cs_id"].ToString() + ".shtml", System.Text.Encoding.Unicode);
                }

                listCar.Clear();
                dsCar.Clear();
            }
        }
        /// <summary>
        /// 生成车型导航条
        /// </summary>
        /// <param name="carId"></param>
        public void GenerateCarNavigation(int carId)
        {
            int serialId = 0;
            DataRow drCar = null,
                drCs = null;
            if (dsCar != null && dsCar.Tables.Count > 0 && dsCar.Tables[0].Rows.Count > 0)
            {
                DataRow[] drsCar = dsCar.Tables[0].Select(" car_id='" + carId + "' ");
                if (drsCar != null && drsCar.Length > 0)
                {
                    drCar = drsCar[0];
                    serialId = ConvertHelper.GetInteger(drCar["cs_id"]);
                }
            }
            if (dsSerial != null && dsSerial.Tables.Count > 0 && dsSerial.Tables[0].Rows.Count > 0)
            {
                DataRow[] drsCs = dsSerial.Tables[0].Select(" cs_id='" + serialId + "' ");
                if (drsCs != null && drsCs.Length > 0)
                {
                    drCs = drsCs[0];
                }
            }
            if (drCar == null || drCs == null) return;
            // modified by chengl Oct.14.2011
            int carid = int.Parse(drCar["car_id"].ToString());
            string subDir = Convert.ToString(carid / 1000);
            string carYear = "";
            string carHeadContent = "";
            //string carHeadContentTemp = carHead;
            //string carHeadContent = "";
            //carHeadContentTemp = carHeadContentTemp.Replace("#CarName#", drCar["car_name"].ToString().Trim());
            //carHeadContentTemp = carHeadContentTemp.Replace("#CarID#", drCar["car_id"].ToString().Trim());
            //string carYear = "";
            //if (drCar["car_YearType"].ToString().Trim().Length >= 4)
            //{
            //    carYear = " " + drCar["car_YearType"].ToString().Trim() + "款";
            //}
            //carHeadContentTemp = carHeadContentTemp.Replace("#CarYearType#", carYear);
            int tagID = 14;

            #region 车型临时
            string carHeadContentTempForSummary = carHeadNew;
            string carHeadContentForSummary = "";
            carHeadContentTempForSummary = carHeadContentTempForSummary.Replace("#CarName#", drCar["car_name"].ToString().Trim());
            carHeadContentTempForSummary = carHeadContentTempForSummary.Replace("#CarID#", drCar["car_id"].ToString().Trim());
            if (drCar["car_YearType"].ToString().Trim().Length >= 4)
            {
                carYear = " " + drCar["car_YearType"].ToString().Trim() + "款";
            }
            carHeadContentTempForSummary = carHeadContentTempForSummary.Replace("#CarYearType#", carYear);
            #endregion

            // 车型综述
            tagID = 14;
            carHeadContentForSummary = carHeadContentTempForSummary;
            carHeadContentForSummary = carHeadContentForSummary.Replace("#SummaryMianBao#", "<strong>" + drCar["car_name"].ToString().Trim() + "" + carYear + "</strong>");
            carHeadContentForSummary = GetCommonHTMLContentReplace(drCs, carHeadContentForSummary, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarSummary_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarSummary", carHeadContentForSummary);
            // CommonFunction.SaveFileContent(carHeadContentForSummary, SaveStaticDataResNAS, "\\CommonHead\\CarSummary\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            // 车型参数配置
            tagID = 15;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarCompare_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarCompare", carHeadContent);
            // CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarCompare\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            // 车型经销商报价
            tagID = 16;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarPrice_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarPrice", carHeadContent);
            CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarPrice\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", System.Text.Encoding.Unicode);

            // 车型图片
            tagID = 30;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarPhoto_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarPhoto", carHeadContent);
            // CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarPhoto\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            // 车型经销商降价
            tagID = 37;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarJiangJia_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarJiangJia", carHeadContent);
            CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarJiangJia\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", System.Text.Encoding.Unicode);

            //// 车型 车贷
            //tagID = 39;
            //carHeadContent = carHeadContentTemp;
            //carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            //carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            //CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarCheDai", carHeadContent);
            //// CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarJiangJia\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            //// 车型 文章
            //tagID = 41;
            //carHeadContent = carHeadContentTemp;
            //carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            //carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            //CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarWenZhang", carHeadContent);
            //CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarWenZhang\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);



            GenerateCarNavigationV2(carId);
        }

        /// <summary>
        /// 生成车型导航条
        /// </summary>
        /// <param name="carId"></param>
        public void GenerateCarNavigationV2(int carId)
        {
            int serialId = 0;
            DataRow drCar = null,
                drCs = null;
            if (dsCar != null && dsCar.Tables.Count > 0 && dsCar.Tables[0].Rows.Count > 0)
            {
                DataRow[] drsCar = dsCar.Tables[0].Select(" car_id='" + carId + "' ");
                if (drsCar != null && drsCar.Length > 0)
                {
                    drCar = drsCar[0];
                    serialId = ConvertHelper.GetInteger(drCar["cs_id"]);
                }
            }
            if (dsSerial != null && dsSerial.Tables.Count > 0 && dsSerial.Tables[0].Rows.Count > 0)
            {
                DataRow[] drsCs = dsSerial.Tables[0].Select(" cs_id='" + serialId + "' ");
                if (drsCs != null && drsCs.Length > 0)
                {
                    drCs = drsCs[0];
                }
            }
            if (drCar == null || drCs == null) return;
            // modified by chengl Oct.14.2011
            int carid = int.Parse(drCar["car_id"].ToString());
            string subDir = Convert.ToString(carid / 1000);
            string carYear = "";
            string carHeadContent = "";
            //string carHeadContentTemp = carHead;
            //string carHeadContent = "";
            //carHeadContentTemp = carHeadContentTemp.Replace("#CarName#", drCar["car_name"].ToString().Trim());
            //carHeadContentTemp = carHeadContentTemp.Replace("#CarID#", drCar["car_id"].ToString().Trim());
            //string carYear = "";
            //if (drCar["car_YearType"].ToString().Trim().Length >= 4)
            //{
            //    carYear = " " + drCar["car_YearType"].ToString().Trim() + "款";
            //}
            //carHeadContentTemp = carHeadContentTemp.Replace("#CarYearType#", carYear);
            int tagID = 14;

            #region 车型临时
            string carHeadContentTempForSummary = carHeadNewV2;
            string carHeadContentForSummary = "";
            carHeadContentTempForSummary = carHeadContentTempForSummary.Replace("#CarName#", drCar["car_name"].ToString().Trim());
            carHeadContentTempForSummary = carHeadContentTempForSummary.Replace("#CarID#", drCar["car_id"].ToString().Trim());
            if (drCar["car_YearType"].ToString().Trim().Length >= 4)
            {
                carYear = " " + drCar["car_YearType"].ToString().Trim() + "款";
            }
            carHeadContentTempForSummary = carHeadContentTempForSummary.Replace("#CarYearType#", carYear);

            string carHeadContentTempForSummaryOld = carHeadNewOld;
            string carHeadContentForSummaryOld = "";
            carHeadContentTempForSummaryOld = carHeadContentTempForSummaryOld.Replace("#CarName#", drCar["car_name"].ToString().Trim());
            carHeadContentTempForSummaryOld = carHeadContentTempForSummaryOld.Replace("#CarID#", drCar["car_id"].ToString().Trim());
            if (drCar["car_YearType"].ToString().Trim().Length >= 4)
            {
                carYear = " " + drCar["car_YearType"].ToString().Trim() + "款";
            }
            carHeadContentTempForSummaryOld = carHeadContentTempForSummaryOld.Replace("#CarYearType#", carYear);
            #endregion

            // 车型综述
            tagID = 14;
            carHeadContentForSummary = carHeadContentTempForSummary;
            carHeadContentForSummary = carHeadContentForSummary.Replace("#SummaryMianBao#", "<strong>" + drCar["car_name"].ToString().Trim() + "" + carYear + "</strong>");
            carHeadContentForSummary = GetCommonHTMLContentReplaceV2(drCs, carHeadContentForSummary, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarSummary_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHeadV2(int.Parse(drCar["car_id"].ToString().Trim()), "CarSummary", carHeadContentForSummary);
            // CommonFunction.SaveFileContent(carHeadContentForSummary, SaveStaticDataResNAS, "\\CommonHead\\CarSummary\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            // 车型参数配置
            tagID = 15;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplaceV2(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarCompare_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHeadV2(int.Parse(drCar["car_id"].ToString().Trim()), "CarCompare", carHeadContent);
            // CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarCompare\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            // 车型经销商报价
            tagID = 16;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplaceV2(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarPrice_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHeadV2(int.Parse(drCar["car_id"].ToString().Trim()), "CarPrice", carHeadContent);
            //CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarPrice\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", System.Text.Encoding.Unicode);

            // 车型图片
            tagID = 30;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplaceV2(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarPhoto_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHeadV2(int.Parse(drCar["car_id"].ToString().Trim()), "CarPhoto", carHeadContent);
            // CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarPhoto\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            // 车型经销商降价
            tagID = 37;
            carHeadContent = carHeadContentTempForSummary;
            carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            carHeadContent = GetCommonHTMLContentReplaceV2(drCs, carHeadContent, tagID, false);
            // 导航增加memcache缓存
            // key : Car_CommonHead_{车型目录}_{车型ID}
            // 例如 : Car_CommonHead_CarSummary_11991
            // CommonFunction.MemcacheInsert("Car_CommonHead_CarJiangJia_" + drCar["car_id"].ToString().Trim(), carHeadContent, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHeadV2(int.Parse(drCar["car_id"].ToString().Trim()), "CarJiangJia", carHeadContent);
            //CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarJiangJia\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", System.Text.Encoding.Unicode);

            //// 车型 车贷
            //tagID = 39;
            //carHeadContent = carHeadContentTemp;
            //carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            //carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            //CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarCheDai", carHeadContent);
            //// CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarJiangJia\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);

            //// 车型 文章
            //tagID = 41;
            //carHeadContent = carHeadContentTemp;
            //carHeadContent = carHeadContent.Replace("#SummaryMianBao#", "<a href=\"http://car.bitauto.com/" + drCs["csAllSpell"].ToString().Trim().ToLower() + "/m" + drCar["car_id"].ToString().Trim() + "/\" target=\"_self\">" + drCar["car_name"].ToString().Trim() + "" + carYear.Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
            //carHeadContent = GetCommonHTMLContentReplace(drCs, carHeadContent, tagID, false);
            //CommonFunction.InsertCommonHead(int.Parse(drCar["car_id"].ToString().Trim()), "CarWenZhang", carHeadContent);
            //CommonFunction.SaveFileContent(carHeadContent, SaveStaticDataResNAS, "\\CommonHead\\CarWenZhang\\" + subDir + "\\" + drCar["car_id"].ToString().Trim() + ".shtml", Encoding.Unicode);
        }

        /// <summary>
        /// 生成子品牌级别的各标签头
        /// </summary>
        /// <param name="tagID">标签ID</param>
        /// <param name="conceptHead">概念车的头模板</param>
        /// <param name="noConceptHead">非概念车头模板</param>
        /// <param name="dr">子品牌数据源</param>
        /// <param name="isYear">是否是年款</param>
        /// <param name="fileDir">头文件存储目录</param>
        private void GenerateSerialHead(int tagID, string conceptHead, string noConceptHead
            , DataRow dr, bool isYear, string fileDir)
        {
            // add by chengl Jul.27.2012
            // 当指定了标签ID 非此标签ID的不生成(-1 是默认值 生成全部 0 是子品牌综述页)
            if (TagIDFromArgs >= 0 && TagIDFromArgs != tagID)
            { return; }

            string csHead = noConceptHead;
            if (dr["cs_CarLevel"].ToString().Trim() == "概念车")
            {
                csHead = GetCommonHTMLContentReplace(dr, conceptHead, tagID, isYear);
            }
            else
            {
                if (tagID == 13)
                {
                    // 区域车型页
                    csHead = csHead.Replace("#JsForIsRegion#", "if(bit_IpRegion){bit_IpRegion='127.0.0.1:北京市;#CityID#,#CityName#,#CitySpell#';}else{var bit_IpRegion='127.0.0.1:北京市;#CityID#,#CityName#,#CitySpell#';}");
                }
                csHead = GetCommonHTMLContentReplace(dr, csHead, tagID, isYear, fileDir);
            }
            // 导航增加memcache缓存
            // key : Car_CommonHead_{子目录}_{子品牌ID}
            // 例如 : Car_CommonHead_CsSummary_1991
            // CommonFunction.MemcacheInsert("Car_CommonHead_" + fileDir + "_" + dr["cs_id"].ToString(), csHead, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHead(int.Parse(dr["cs_id"].ToString()), fileDir, csHead);

            /// 调试
            // object obj = CommonFunction.MemcacheGet("Car_CommonHead_" + fileDir + "_" + dr["cs_id"].ToString());
            // Log.WriteLog("Car_CommonHead_" + fileDir + "_" + dr["cs_id"].ToString() + ":\r\n" + obj.ToString() + "\r\n");
            ///  调试

            CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\" + fileDir + "\\" + dr["cs_id"].ToString() + ".shtml", System.Text.Encoding.Unicode);
        }
        /// <summary>
        /// 生成子品牌级别的各标签头
        /// </summary>
        /// <param name="tagID">标签ID</param>
        /// <param name="conceptHead">概念车的头模板</param>
        /// <param name="noConceptHead">非概念车头模板</param>
        /// <param name="dr">子品牌数据源</param>
        /// <param name="isYear">是否是年款</param>
        /// <param name="fileDir">头文件存储目录</param>
        private void GenerateSerialHeadV2(int tagID, string conceptHead, string noConceptHead
            , DataRow dr, bool isYear, string fileDir)
        {
            // add by chengl Jul.27.2012
            // 当指定了标签ID 非此标签ID的不生成(-1 是默认值 生成全部 0 是子品牌综述页)
            if (TagIDFromArgs >= 0 && TagIDFromArgs != tagID)
            { return; }

            string csHead = noConceptHead;
            if (dr["cs_CarLevel"].ToString().Trim() == "概念车")
            {
                csHead = GetCommonHTMLContentReplaceV2(dr, conceptHead, tagID, isYear);
            }
            else
            {
                if (tagID == 13)
                {
                    // 区域车型页
                    csHead = csHead.Replace("#JsForIsRegion#", "if(bit_IpRegion){bit_IpRegion='127.0.0.1:北京市;#CityID#,#CityName#,#CitySpell#';}else{var bit_IpRegion='127.0.0.1:北京市;#CityID#,#CityName#,#CitySpell#';}");
                }
                csHead = GetCommonHTMLContentReplaceV2(dr, csHead, tagID, isYear, fileDir);
            }
            // 导航增加memcache缓存
            // key : Car_CommonHead_{子目录}_{子品牌ID}
            // 例如 : Car_CommonHead_CsSummary_1991
            // CommonFunction.MemcacheInsert("Car_CommonHead_" + fileDir + "_" + dr["cs_id"].ToString(), csHead, Convert.ToInt64(CommonData.CommonSettings.MemcacheDuration));
            CommonFunction.InsertCommonHeadV2(int.Parse(dr["cs_id"].ToString()), fileDir, csHead);

            /// 调试
            // object obj = CommonFunction.MemcacheGet("Car_CommonHead_" + fileDir + "_" + dr["cs_id"].ToString());
            // Log.WriteLog("Car_CommonHead_" + fileDir + "_" + dr["cs_id"].ToString() + ":\r\n" + obj.ToString() + "\r\n");
            ///  调试

            //CommonFunction.SaveFileContent(csHead, SaveStaticDataResNAS, "\\CommonHead\\" + fileDir + "\\" + dr["cs_id"].ToString() + ".shtml", System.Text.Encoding.Unicode);
        }
        #endregion

        #region 车展
        /// <summary>
        /// 2010北京车展
        /// </summary>
        private void GetExhibitionBeiJing2010XML()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(CommonData.CommonSettings.BeiJing2010);
                if (doc != null && doc.HasChildNodes)
                {
                    XmlNodeList xnl = doc.SelectNodes("/root/MasterBrand/Brand/Serial");
                    if (xnl != null && xnl.Count > 0)
                    {
                        foreach (XmlNode xn in xnl)
                        {
                            if (!htSerialBeiJing2010.ContainsKey(xn.Attributes["ID"].Value.ToString()))
                            {
                                htSerialBeiJing2010.Add(xn.Attributes["ID"].Value.ToString(), 1);
                            }
                        }
                    }
                    // htSerialBeiJing2010
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 2010广州车展
        /// </summary>
        private void GetExhibitionGuangZhou2010XML()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(CommonData.CommonSettings.BeiJing2010 + "?eid=48");
                if (doc != null && doc.HasChildNodes)
                {
                    XmlNodeList xnl = doc.SelectNodes("/root/MasterBrand/Brand/Serial");
                    if (xnl != null && xnl.Count > 0)
                    {
                        foreach (XmlNode xn in xnl)
                        {
                            if (!htSerialGuangZhou2010.ContainsKey(xn.Attributes["ID"].Value.ToString()))
                            {
                                htSerialGuangZhou2010.Add(xn.Attributes["ID"].Value.ToString(), 1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region 模板替换
        /// <summary>
        /// 生成子品牌对应导航头通用内容
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="tempContent">模板内容(有面包削,无面包削)</param>
        /// <returns></returns>
		private string GetCommonHTMLContentReplace(DataRow dr, string tempContent, int tagID, bool isYear, string fileDir = "")
        {
            if (tempContent == "")
            { return ""; }
            int csid = int.Parse(dr["cs_id"].ToString());
            // 车型频道link基地址 "\" 或 "http://car.bitauto.com/"
            // 区域购买、CMS新闻、各个车展、Ucar 子品牌车贷
            string baseURL = "/";
            if (tagID == 13 || tagID == 23 || tagID == 24
                || tagID == 26 || tagID == 29 || tagID == 31 || tagID == 33 || tagID == 40)
            { baseURL = "http://car.bitauto.com/"; }
            // 窗口类型
            string urlTarget = "";
            if (tagID == 23)
            {
                // CMS 头新窗口打开
                urlTarget = " target=\"_blank\" ";
            }
            else
            {
                // 非CMS头
                urlTarget = " target=\"_self\" ";
            }

            string temp = tempContent;
            temp = temp.Replace("#DomainName#", baseURL);
            temp = temp.Replace("#TagID#", tagID.ToString());
            temp = temp.Replace("#CsID#", dr["cs_id"].ToString());
            // 索纳塔八的话 名称全是 索纳塔八
            if (dr["cs_id"].ToString() == "1568")
            {
                temp = temp.Replace("#CsShowName#", "索纳塔八");
                temp = temp.Replace("#CsName#", "索纳塔八");
            }
            else
            {
                temp = temp.Replace("#CsShowName#", dr["cs_ShowName"].ToString().Trim().Replace("·", "&bull;"));
                temp = temp.Replace("#CsName#", dr["cs_name"].ToString().Trim().Replace("·", "&bull;"));
            }

            // modified by chengl Jul.24.2012
            // 高总新需求
            // 主品牌下既有国产品牌又有进口品牌的，改为显示“进口”+“子品牌SEO名”；
            // 只有进口子品牌的不做修改，国产子品牌不做修改；
            // 只修改互联互通导航；
            // 包哈页有子品牌，年款，车款，相关页面。
            if (listMasterCpCountry.Contains(int.Parse(dr["bs_Id"].ToString()))
                && dr["Cp_Country"].ToString().Trim() != "中国")
            {
                temp = temp.Replace("#CsSpecialSEOName#", "进口" + dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;"));
            }
            else
            { temp = temp.Replace("#CsSpecialSEOName#", dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;")); }
            // temp = temp.Replace("#CsSEOName#", dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;"));

            // temp = temp.Replace("#CsNameEncode#", Server.UrlEncode(dr["cs_name"].Trim()));
            temp = temp.Replace("#CsAllSpell#", dr["csAllSpell"].ToString().Trim().ToLower());
            temp = temp.Replace("#MasterUrlSpell#", dr["urlspell"].ToString().Trim().ToLower());
            temp = temp.Replace("#MasterName#", dr["bs_Name"].ToString().Trim());
            temp = temp.Replace("#CbName#", dr["cb_name"].ToString().Trim().Replace("·", "&bull;"));
            temp = temp.Replace("#CbAllSpell#", dr["cbAllSpell"].ToString().Trim().ToLower());

            if (GetSerialSummaryMianBaoAD(fileDir, int.Parse(dr["cs_id"].ToString())) == "")
            {
                // 当没有广告的时候显示
                // 热搜块
                temp = temp.Replace("#SerachHotBlock#", SearchHot);
            }
            else
            {
                temp = temp.Replace("#SerachHotBlock#", "");
            }

            #region 用途
            string tempPurpose = "";
            if (dr["CsPurpose"].ToString().Trim() != "")
            {
                string[] arrPurpose = dr["CsPurpose"].ToString().Trim().Split(',');
                if (arrPurpose.Length > 0)
                {
                    int loop = 0;
                    for (int i = 0; i < arrPurpose.Length; i++)
                    {
                        if (arrPurpose[i] != "")
                        {
                            int iPurpose = 0;
                            if (int.TryParse(arrPurpose[i], out iPurpose))
                            {
                                if (iPurpose > 0 && loop < 2)
                                {
                                    string url = "";
                                    string pname = "";
                                    CommonNavigationService.GetPurposeNameAndURL(iPurpose, out url, out pname);
                                    tempPurpose += "<a href=\"" + baseURL + url + "\" target=\"_blank\">" + pname + "</a> ";
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            temp = temp.Replace("#CsPurpose#", tempPurpose);
            // modified by chengl Aug.12.2013
            // temp = temp.Replace("#EveryCityPV#", GetSerialPVForEveryCity(int.Parse(dr["cs_id"].ToString())));
            temp = temp.Replace("#YesterdaySort#", GetSerialTotalPV(int.Parse(dr["cs_id"].ToString())));
            temp = temp.Replace("#MasterID#", dr["bs_Id"].ToString().Trim());

            // 级别排行地址
            string levelSpell = CommonNavigationService.GetCsLevelSpell(dr["cs_CarLevel"].ToString().Trim());
            if (levelSpell != "")
            {
                //temp = temp.Replace("#CsLevelPaihang#"
                //	, string.Format("<a href=\"{0}{1}/paihang/\" target=\"_blank\">全国{2}关注</a>"
                //	, baseURL, levelSpell, dr["cs_CarLevel"].ToString().Trim()));
                temp = temp.Replace("#CsLevelPaihang#"
                    , string.Format("<a href=\"{0}{1}/paihang/\" target=\"_blank\" data-channelid=\"2.21.836\" title=\"最近7天关注度\">{2}周关注排行</a>"
                    , baseURL, levelSpell, dr["cs_CarLevel"].ToString().Trim()));
            }
            else
            {
                //temp = temp.Replace("#CsLevelPaihang#", string.Format("<a href=\"javascript:void(0);\" target=\"_self\">全国{0}关注</a>", dr["cs_CarLevel"].ToString().Trim()));
                temp = temp.Replace("#CsLevelPaihang#", string.Format("<a href=\"javascript:void(0);\" target=\"_self\" title=\"最近7天关注度\">{0}周关注排行</a>", dr["cs_CarLevel"].ToString().Trim()));
            }
            // temp = temp.Replace("#CsLevelSpell#", CommonNavigationService.GetCsLevelSpell(dr["cs_CarLevel"].ToString().Trim()));
            // temp = temp.Replace("#CsLevelName#", dr["cs_CarLevel"].ToString().Trim() == "概念车" ? "" : dr["cs_CarLevel"].ToString().Trim());
            temp = temp.Replace("#BBSURL#", GetBBSURLByCsID(int.Parse(dr["cs_id"].ToString())));
            // 头生成时间
            temp = temp.Replace("#GenerateDateTime#", DateTime.Now.ToString());

            #region 十佳车型
            bool hasBestTopCar = false;
            foreach (Define.BestTopCar btc in listBestTopCar)
            {
                if (btc.ListCsList.Contains(csid))
                {
                    hasBestTopCar = true;
                    temp = temp.Replace("#Top10CarLink#", String.Format("<a href=\"{0}\" target=\"_blank\" class=\"top10\" title=\"{1}\" alt=\"{1}\"></a>", btc.Link, btc.Title));
                    break;
                }
            }
            if (!hasBestTopCar)
            {
                temp = temp.Replace("#Top10CarLink#", "");
            }
            //int topYear = GetTopCarYear(csid);
            //string top10CarUrl = "";
            //switch (topYear)
            //{
            //    case 2011:
            //        top10CarUrl = "http://www.bitauto.com/top10cars/gd_2011/";
            //        break;
            //    case 2012:
            //        top10CarUrl = "http://www.bitauto.com/top10cars/";
            //        break;
            //}
            //if (String.IsNullOrEmpty(top10CarUrl))
            //    temp = temp.Replace("#Top10CarLink#", "");
            //else
            //    temp = temp.Replace("#Top10CarLink#", String.Format("<a href=\"{0}\" target=\"_blank\" class=\"top10\" title=\"{1}年度十佳车\" alt=\"{1}年度十佳车\"></a>", top10CarUrl, topYear));

            #endregion

            #region 面包屑广告

            //if (tagID == 0)
            //{
            temp = temp.Replace("#SerialMianBaoAD#", GetSerialSummaryMianBaoAD(fileDir, int.Parse(dr["cs_id"].ToString())));
            //}

            #endregion

            #region 搜索条 add by chengl Dec.4.2012

            if (dicSoBar.ContainsKey(tagID))
            {
                // 存在对应标签的
                temp = temp.Replace("#SoBar#", dicSoBar[tagID]);
            }
            else
            {
                // 不存在则取默认的
                temp = temp.Replace("#SoBar#", (dicSoBar.ContainsKey(-1) ? dicSoBar[-1] : ""));
            }

            #endregion

            #region 改款上市时间
            if (dictCarMarkTime.ContainsKey(csid))
            {
                DateTime marketTime;
                string dateFormat = "yy年MM月dd日";
                if (dictCarMarkTime[csid].MarketTime.Split('-').Length <= 2)
                {
                    dateFormat = "yy年MM月";
                }
                if (DateTime.TryParse(dictCarMarkTime[csid].MarketTime, out marketTime))
                {
                    string showInfo = string.Empty;
                    //int diff = CommonFunction.DateDiff("d", marketTime, DateTime.Now);
                    int diff = DateTime.Compare(marketTime.Date, DateTime.Now.Date);
                    string newsUrl = dictCarMarkTime[csid].Url;
                    if (diff > 0)
                    {
                        showInfo = string.Format("新款将于{0}上市", marketTime.ToString(dateFormat));
                    }
                    else if (diff < 0)
                    {
                        showInfo = string.Format("新款已于{0}上市", marketTime.ToString(dateFormat));
                    }
                    else if (diff == 0)
                    {
                        showInfo = "新款于今日上市";
                    }
                    if (string.IsNullOrEmpty(newsUrl))
                        temp = temp.Replace("#SerialMarketTime#", string.Format("<span class=\"forecast-lnk\">{0}</span>", showInfo));
                    else
                        temp = temp.Replace("#SerialMarketTime#", string.Format("<a href=\"{1}\" target=\"_blank\" class=\"forecast-lnk\">{0}</a>", showInfo, newsUrl));
                }
                else
                { temp = temp.Replace("#SerialMarketTime#", ""); }
            }
            else
            {
                temp = temp.Replace("#SerialMarketTime#", "");
            }
            #endregion

            #region 子品牌是否停销
            if (dr["CsSaleState"].ToString().Trim() == "停销")
            {
                temp = temp.Replace("#IsNoSale#", "<span style=\"color:#c00;font-weight:bold;\">停售</span> ");
            }
            else
            {
                temp = temp.Replace("#IsNoSale#", "");
            }
            #endregion

            #region CTCC
            string urlCTCC = GetSerialCTCCByCsID(int.Parse(dr["cs_id"].ToString()));
            if (urlCTCC != "")
            {
                temp = temp.Replace("#CTCC#", "<a target=\"_blank\" href=\"" + urlCTCC + "\">CTCC参赛车</a>");
            }
            else
            {
                temp = temp.Replace("#CTCC#", string.Empty);
            }
            #endregion

            #region 子品牌年款
            // 子品牌年款
            List<int> saleYear = new List<int>();
            DataRow[] drsYear = GetCarYearByCsID(int.Parse(dr["cs_id"].ToString()));
            StringBuilder sbCarYear = new StringBuilder();
            // add by chengl Dec.9.2011
            if (dr["CsSaleState"].ToString().Trim() == "待销")
            {
                // 待销 子品牌显示未上市
                sbCarYear.Append("<a " + urlTarget + " id=\"carYearList_all\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\">未上市</a>");
            }
            else if (dr["CsSaleState"].ToString().Trim() != "停销" && dr["CsSaleState"].ToString().Trim() != "待查")
            {
                sbCarYear.Append("<a rel=\"nofollow\" " + urlTarget + " id=\"carYearList_all\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\">全部在售</a>");
            }
            else { }

            if (drsYear != null && drsYear.Length > 0)
            {
                // modified by chengl Jul.10.2012
                int loopYear = 1;
                foreach (DataRow drYear in drsYear)
                {
                    // 在销年款
                    if (!saleYear.Contains(int.Parse(drYear["carYear"].ToString())))
                    { saleYear.Add(int.Parse(drYear["carYear"].ToString())); }
                    if (loopYear <= 3)
                    {
                        if (sbCarYear.Length > 0)
                        // { sbCarYear.Append(" | "); }
                        { sbCarYear.Append("<s>|</s>"); }
                        sbCarYear.Append("<a " + urlTarget + " data-channelid=\"2.21.837\"  id=\"carYearList_" + drYear["carYear"].ToString() + "\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/" + drYear["carYear"].ToString() + "/\">" + drYear["carYear"].ToString() + "款</a>");
                    }
                    loopYear++;
                }
            }
            // modified by chengl Jun.20.2011
            // 索纳塔八 不显示 停销年款
            if (IsHasNoSaleCarByCsID(int.Parse(dr["cs_id"].ToString())) && dr["cs_id"].ToString() != "1568")
            {
                if (dicSerialNoSaleYear.ContainsKey(csid))
                {
                    List<int> noSale = new List<int>();
                    foreach (int noSaleYear in dicSerialNoSaleYear[csid])
                    {
                        if (!saleYear.Contains(noSaleYear) && !noSale.Contains(noSaleYear))
                        { noSale.Add(noSaleYear); }
                    }
                    if (noSale.Count > 0)
                    {
                        if (sbCarYear.Length > 0)
                        { sbCarYear.Append("<s>|</s>"); }
                        sbCarYear.Append("<dl id=\"bt_car_spcar\" data-channelid=\"2.21.838\" class=\"\">");
                        sbCarYear.Append("<dt>停售年款<em></em></dt>");
                        sbCarYear.Append("<dd style=\"display: none;\">");
                        int loop = 1;
                        foreach (int noYear in noSale)
                        {
                            if (loop == noSale.Count)
                            {
                                sbCarYear.Append("<a class=\"last_a\" ");
                            }
                            else
                            {
                                sbCarYear.Append("<a ");
                            }
                            sbCarYear.Append(urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/" + noYear.ToString() + "\">" + noYear + "款</a>");
                            loop++;
                        }
                        sbCarYear.Append("</dd>");
                        sbCarYear.Append("</dl>");
                    }
                }

                //if (sbCarYear.Length > 0)
                //{ sbCarYear.Append(" | "); }
                //sbCarYear.Append("<a target=\"_blank\" href=\"http://www.cheyisou.com/chexing/" + HttpUtility.UrlEncode(dr["cs_ShowName"].ToString().Trim()) + "/1.html?para=os|0|en|utf8\">停售车款</a>");
            }
            temp = temp.Replace("#CarYearList#", sbCarYear.ToString());
            #endregion

            #region del by sk 2016.02.02 子品牌是否在车展中
            //if (tagID == 24)
            //{
            //	// 广州车展
            //	bool isInExhibition = CheckExhibition(int.Parse(dr["cs_id"].ToString()), htSerialExhibition);
            //	if (isInExhibition)
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag24#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/2009/" + dr["urlSpell"].ToString().Trim().ToLower() + "/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">广州车展</a></li>");
            //	}
            //	else
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag24#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/\" target=\"_blank\">广州车展</a></li>");
            //	}
            //}
            //else if (tagID == 29)
            //{
            //	// 北京2010车展
            //	bool isInBeiJing2010 = CheckExhibition(int.Parse(dr["cs_id"].ToString()), htSerialBeiJing2010);
            //	if (isInBeiJing2010)
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag29#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/beijing/2010/" + dr["urlSpell"].ToString().Trim().ToLower() + "/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">北京车展</a></li>");
            //	}
            //	else
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag29#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/beijing/\" target=\"_blank\">北京车展</a></li>");
            //	}
            //}
            //else if (tagID == 33)
            //{
            //	// 2010广州车展
            //	bool isInGuangZhou2010 = CheckExhibition(int.Parse(dr["cs_id"].ToString()), htSerialGuangZhou2010);
            //	if (isInGuangZhou2010)
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag33#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/2010/" + dr["urlSpell"].ToString().Trim().ToLower() + "/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">广州车展</a></li>");
            //	}
            //	else
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag33#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/\" target=\"_blank\">广州车展</a></li>");
            //	}
            //}
            //else //if (tagID == 34)
            //{
            //	// 2011 上海车展
            //	temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca\"><a target=\"_blank\" href=\"http://chezhan.bitauto.com/shanghai/\" >上海车展</a></li>");
            //}

            #endregion

            //#region 国产还是进口 标签头替换
            //if (dr["Cp_Country"].ToString().Trim() != "中国")
            //{
            //    temp = temp.Replace("#CpCountry#", "进口");
            //}
            //else
            //{
            //    temp = temp.Replace("#CpCountry#", "");
            //}
            //#endregion

            //#region 易车惠 add by Nov.7.2013 chengl

            //if(tagID==0)
            //{
            //    // 子品牌综述页有
            //    if(dicCsGoods.ContainsKey(csid))
            //    {
            //        temp = temp.Replace("#yichehui#", "<a  class=\"ad-yichehui\" target=\"_blank\" href=\"" + dicCsGoods[csid].Replace("/detail", "/all/detail") + "?WT.mc_id=car4" + "\" >易车惠有特价，立即查看&gt;&gt;</a>");
            //    }
            //    else
            //    {
            //         temp = temp.Replace("#yichehui#", "");
            //    }
            //}

            //#endregion

            #region 面包削主品牌和品牌重复
            string msBrand = dr["cb_name"].ToString().Trim();
            string masterOrBrand = "";
            if (dr["Cp_Country"].ToString().Trim() != "中国")
            {
                msBrand = "进口" + dr["cb_name"].ToString().Trim();
            }
            if (msBrand == dr["bs_Name"].ToString().Trim() || (dr["cb_name"].ToString().Trim() == dr["bs_Name"].ToString().Trim()))
            {
                // 重名留主品牌
                masterOrBrand = "<a href=\"" + baseURL + dr["urlspell"].ToString().Trim().ToLower() + "/\" target=\"_blank\">" + dr["bs_Name"].ToString().Trim() + "</a>";
            }
            else
            {
                // 不重名都保留
                masterOrBrand = "<a href=\"" + baseURL + dr["urlspell"].ToString().Trim().ToLower() + "/\" target=\"_blank\">" + dr["bs_Name"].ToString().Trim() + "</a> &gt; <a href=\"http://car.bitauto.com/" + dr["cbAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_blank\">" + msBrand + "</a>";
            }
            temp = temp.Replace("#MasterOrBrand#", masterOrBrand);
            #endregion

            #region 车贷or保养

            //// 如果是指定的子品牌ID 则显示车贷标签
            //if (listCheDaiCsID.Contains(csid))
            //{
            // modified by chengl Jan.3.2013 所有子品牌开放车贷标签
            // 如果有车贷
            temp = temp.Replace("#CheDaiStart#", "");
            temp = temp.Replace("#CheDaiEnd#", "");
            temp = temp.Replace("#BaoYangStart#", "");
            temp = temp.Replace("#BaoYangEnd#", "");
            temp = temp.Replace("#BaoYangCss#", "#ClassTag32#");
            temp = temp.Replace("#BaoYangChange#", "true");
            temp = temp.Replace("#WeiHuBaoYangHide#", "<a id=\"CN_ShowDDLinkBaoyang\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            //}
            //else
            //{
            //    // 如果没有车贷 显示保养
            //    temp = temp.Replace("#CheDaiStart#", "<!--");
            //    temp = temp.Replace("#CheDaiEnd#", "-->");
            //    temp = temp.Replace("#BaoYangStart#", "");
            //    temp = temp.Replace("#BaoYangEnd#", "");
            //    temp = temp.Replace("#BaoYangCss#", "");
            //    temp = temp.Replace("#BaoYangChange#", "false");
            //    temp = temp.Replace("#WeiHuBaoYangHide#", "");
            //}

            #endregion

            #region 保养
            //bool isHasBaoYang = false;
            //if (htBaoYang != null && htBaoYang.Count > 0)
            //{
            //	if (htBaoYang.ContainsKey(dr["cs_id"].ToString()))
            //	{ isHasBaoYang = true; }
            //}
            //if (isHasBaoYang)
            //{
            //	// 有保养信息
            //	// temp = temp.Replace("#WeiHuBaoYang#", "<a " + urlTarget + " class=\"w75\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">维修养护</a>");
            //	// temp = temp.Replace("#WeiHuBaoYangNew#", "<a id=\"CN_ShowDDLinkBaoyang\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            //	temp = temp.Replace("#WeiHuBaoYangNew#", "<a " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            //}
            //else
            //{
            //	// 无保养信息
            //	// temp = temp.Replace("#WeiHuBaoYang#", "维修养护");
            //	// modified by chengl May.10.2012
            //	temp = temp.Replace("#WeiHuBaoYangNew#", "<a class=\"nolink\">养护</a>");
            //}
            temp = temp.Replace("#WeiHuBaoYangNew#", "<a " + urlTarget + " data-channelid=\"2.21.790\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            #endregion

            #region 销量

            if (listProduceAndSell.Contains(csid))
            {
                // 有销量数据
                //temp = temp.Replace("#XiaoLiang#", "<a target=\"_blank\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/xiaoliang/\">" + dr["cs_seoname"] + "销量</a> <em>|</em> ");
                temp = temp.Replace("#XiaoLiang#", "<a target=\"_blank\" href=\"http://index.bitauto.com/xiaoliang/s" + dr["cs_id"] + "/\">销量指数</a> <em>|</em> ");
            }
            else
            {
                // 无销量数据
                temp = temp.Replace("#XiaoLiang#", "");
            }

            #endregion

            #region 导购 & 评测
            // modified by chengl 
            // string urlDaoGou = GetSerialDaoGouURLByCsID(int.Parse(dr["cs_id"].ToString()));
            string urlDaoGou = GetSerialPingCeURLByCsID(int.Parse(dr["cs_id"].ToString()));
            /*
             if (urlDaoGou != "")
            {
                if (isYear)
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag22# #ClassTag28# #ClassTag19#\"><a " + urlTarget + " href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">车型详解</a></li>");
                }
                else
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 非年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag22# #ClassTag28# #ClassTag19#\"><a " + urlTarget + " href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">车型详解</a></li>");
                }
            }
            else
            {
                // modified by chengl Jun.8.2011
                // temp = temp.Replace("#DaoGouURL#", "车型详解");
                // modified by chengl May.10.2012
                temp = temp.Replace("#DaoGouURL#", "<li><a class=\"nolink\">车型详解</a></li>");
            }
             */
            if (urlDaoGou != "")
            {
                if (isYear)
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag22#\"><a " + urlTarget + " data-channelid=\"2.21.784\" href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">车型详解</a></li>");
                }
                else
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 非年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag22#\"><a " + urlTarget + " data-channelid=\"2.21.784\" href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">车型详解</a></li>");
                }
            }
            else
            {
                // modified by chengl Jun.8.2011
                // temp = temp.Replace("#DaoGouURL#", "车型详解");
                // modified by chengl May.10.2012
                temp = temp.Replace("#DaoGouURL#", "<li><a data-channelid=\"2.21.784\" class=\"nolink\">车型详解</a></li>");
            }
            #endregion

            #region 文章

            string urlWenZhang = "{0}/#year#/wenzhang/";// GetSerialWenZhangURLByCsID(int.Parse(dr["cs_id"].ToString()));
            bool hasSerialNews = CommonNavigationService.HasSerialNews(ConvertHelper.GetInteger(dr["cs_id"]));
            if (hasSerialNews)
            {
                if (isYear)
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 年款
                    temp = temp.Replace("#WenZhangURL#", "<li class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag28# #ClassTag19# #ClassTag41#\"><a " + urlTarget + " data-channelid=\"2.21.785\" href=\"" + string.Format(baseURL + urlWenZhang, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">文章</a></li>");
                }
                else
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 非年款
                    temp = temp.Replace("#WenZhangURL#", "<li class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag28# #ClassTag19# #ClassTag41#\"><a " + urlTarget + " data-channelid=\"2.21.785\" href=\"" + string.Format(baseURL + urlWenZhang, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">文章</a></li>");
                }
            }
            else
            {
                temp = temp.Replace("#WenZhangURL#", "<li><a data-channelid=\"2.21.785\" class=\"nolink\">文章</a></li>");
            }

            #endregion

            #region 视频

            if (CommonData.ListHasVideoSerialID != null && CommonData.ListHasVideoSerialID.Count > 0)
            {
                if (CommonData.ListHasVideoSerialID.Contains(int.Parse(dr["cs_id"].ToString())))
                { temp = temp.Replace("#ShiPin#", "<a " + urlTarget + " data-channelid=\"2.21.783\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/shipin/\">视频</a>"); }
                else
                { temp = temp.Replace("#ShiPin#", "<a class=\"nolink\">视频</a>"); }
            }
            else
            {
                // 如果接口问题默认全部显示
                temp = temp.Replace("#ShiPin#", "<a " + urlTarget + " data-channelid=\"2.21.783\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/shipin/\">视频</a>");
            }

            #endregion

            #region 置换

            // modified by chengl Jul.26.2012 高总需求所有子品牌开放置换
            // temp = temp.Replace("#ZhiHuanURL#", "<a id=\"CN_ShowDDLinkZhiHuan\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/zhihuan/\">置换</a>");

            //// 如果子品牌有置换信息
            //if (listZhiHuanCsID.Contains(csid))
            //{
            //    // 有置换数据
            //    temp = temp.Replace("#ZhiHuanURL#", "<a id=\"CN_ShowDDLinkZhiHuan\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/zhihuan/\">置换</a>");
            //}
            //else
            //{
            //    // 无置换数据
            //    temp = temp.Replace("#ZhiHuanURL#", "<a class=\"nolink\">置换</a>");
            //}

            #endregion

            #region 新闻 行情 挪至更多标签内

            //// 新闻
            //string urlXinWen = GetSerialNewURLByName(int.Parse(dr["cs_id"].ToString()), "xinwen");
            //if (urlXinWen != "")
            //{
            //    if (isYear)
            //    {
            //        // 年款
            //        temp = temp.Replace("#XinWenURL#", "<a id=\"CN_ShowDDLinkXinWen\" " + urlTarget + " href=\"" + string.Format(baseURL + urlXinWen, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">新闻</a>");
            //    }
            //    else
            //    {
            //        // 非年款
            //        temp = temp.Replace("#XinWenURL#", "<a id=\"CN_ShowDDLinkXinWen\" " + urlTarget + " href=\"" + string.Format(baseURL + urlXinWen, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">新闻</a>");
            //    }
            //}
            //else
            //{
            //    temp = temp.Replace("#XinWenURL#", "");
            //}

            // 行情
            string urlHangQing = GetSerialNewURLByName(int.Parse(dr["cs_id"].ToString()), "hangqing");
            if (urlHangQing != "")
            {
                if (isYear)
                {
                    // 年款
                    temp = temp.Replace("#HangQingURL#", "<a id=\"CN_ShowDDLinkHangQing\" " + urlTarget + " href=\"" + string.Format(baseURL + urlHangQing, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">行情</a>");
                }
                else
                {
                    // 非年款
                    temp = temp.Replace("#HangQingURL#", "<a id=\"CN_ShowDDLinkHangQing\" " + urlTarget + " href=\"" + string.Format(baseURL + urlHangQing, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">行情</a>");
                }
            }
            else
            {
                temp = temp.Replace("#HangQingURL#", "");
            }

            #endregion

            #region 如果是年款 生成年款新闻js对象
            if (isYear)
            {
                string NewsCountJson = GetSerialYearNewCount(int.Parse(dr["cs_id"].ToString()));
                temp = temp.Replace("#SerialYearNewObj#", NewsCountJson == "" ? "null" : NewsCountJson);
            }
            #endregion

            #region 易车测试
            if (dr["bitautoTestURL"].ToString().Trim() != "")
            {
                temp = temp.Replace("#BitAutoTest#", "<em><a href=\"" + dr["bitautoTestURL"].ToString().Trim() + "\" target=\"_blank\" >易车测试车型</a></em>");
            }
            else
            {
                temp = temp.Replace("#BitAutoTest#", "");
            }
            #endregion

            #region 标签相关

            // 子品牌综述页面包削
            if (tagID == 0)
            {
                // 索纳塔八的话 名称全是 索纳塔八
                if (dr["cs_id"].ToString() == "1568")
                {
                    temp = temp.Replace("#SummaryMianBao#", "<strong>索纳塔八</strong>");
                }
                else
                {
                    temp = temp.Replace("#SummaryMianBao#", "<strong>" + dr["cs_ShowName"].ToString().Trim() + "</strong>");
                }
            }
            else
            {
                // 索纳塔八的话 名称全是 索纳塔八
                if (dr["cs_id"].ToString() == "1568")
                {
                    temp = temp.Replace("#SummaryMianBao#", "<a " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">索纳塔八</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                }
                else
                {
                    temp = temp.Replace("#SummaryMianBao#", "<a " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                }
            }

            string encoderStr = tagID == 17 ? "gb2312" : "utf-8";
            temp = temp.Replace("#EncoderStr#", encoderStr);

            temp = temp.Replace("#currentTagForStat#", CommonNavigationService.GetCurrentTagForStat(tagID));
            temp = temp.Replace("#TagName#", CommonNavigationService.GetTagNameByID(tagID));

            temp = temp.Replace("#JsForIsRegion#", "");
            temp = temp.Replace("#TagID#", tagID.ToString());

            // temp = temp.Replace("#ClassTag" + tagID.ToString() + "#", "current");
            // add by chengl Apr.26.2012
            temp = temp.Replace("#ClassTag" + tagID.ToString() + "#", "on");
            Regex regex = new Regex(@"#ClassTag(\d+)#", RegexOptions.IgnoreCase);
            temp = regex.Replace(temp, "");

            #endregion

            return temp;
        }

        /// <summary>
        /// 生成子品牌对应导航头通用内容
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="tempContent">模板内容(有面包削,无面包削)</param>
        /// <returns></returns>
		private string GetCommonHTMLContentReplaceV2(DataRow dr, string tempContent, int tagID, bool isYear, string fileDir = "")
        {
            if (tempContent == "")
            { return ""; }
            int csid = int.Parse(dr["cs_id"].ToString());
            // 车型频道link基地址 "\" 或 "http://car.bitauto.com/"
            // 区域购买、CMS新闻、各个车展、Ucar 子品牌车贷
            string baseURL = "/";
            if (tagID == 13 || tagID == 23 || tagID == 24
                || tagID == 26 || tagID == 29 || tagID == 31 || tagID == 33 || tagID == 40)
            { baseURL = "http://car.bitauto.com/"; }
            // 窗口类型
            string urlTarget = "";
            if (tagID == 23)
            {
                // CMS 头新窗口打开
                urlTarget = " target=\"_blank\" ";
            }
            else
            {
                // 非CMS头
                urlTarget = " target=\"_self\" ";
            }

            string temp = tempContent;
            temp = temp.Replace("#DomainName#", baseURL);
            temp = temp.Replace("#TagID#", tagID.ToString());
            temp = temp.Replace("#CsID#", dr["cs_id"].ToString());
            // 索纳塔八的话 名称全是 索纳塔八
            if (dr["cs_id"].ToString() == "1568")
            {
                temp = temp.Replace("#CsShowName#", "索纳塔八");
                temp = temp.Replace("#CsName#", "索纳塔八");
            }
            else
            {
                temp = temp.Replace("#CsShowName#", dr["cs_ShowName"].ToString().Trim().Replace("·", "&bull;"));
                temp = temp.Replace("#CsName#", dr["cs_name"].ToString().Trim().Replace("·", "&bull;"));
            }

            // modified by chengl Jul.24.2012
            // 高总新需求
            // 主品牌下既有国产品牌又有进口品牌的，改为显示“进口”+“子品牌SEO名”；
            // 只有进口子品牌的不做修改，国产子品牌不做修改；
            // 只修改互联互通导航；
            // 包哈页有子品牌，年款，车款，相关页面。
            if (listMasterCpCountry.Contains(int.Parse(dr["bs_Id"].ToString()))
                && dr["Cp_Country"].ToString().Trim() != "中国")
            {
                temp = temp.Replace("#CsSpecialSEOName#", "进口" + dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;"));
            }
            else
            { temp = temp.Replace("#CsSpecialSEOName#", dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;")); }
            // temp = temp.Replace("#CsSEOName#", dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;"));

            // temp = temp.Replace("#CsNameEncode#", Server.UrlEncode(dr["cs_name"].Trim()));
            temp = temp.Replace("#CsAllSpell#", dr["csAllSpell"].ToString().Trim().ToLower());
            temp = temp.Replace("#MasterUrlSpell#", dr["urlspell"].ToString().Trim().ToLower());
            temp = temp.Replace("#MasterName#", dr["bs_Name"].ToString().Trim());
            temp = temp.Replace("#CbName#", dr["cb_name"].ToString().Trim().Replace("·", "&bull;"));
            temp = temp.Replace("#CbAllSpell#", dr["cbAllSpell"].ToString().Trim().ToLower());

            if (GetSerialSummaryMianBaoADV2(fileDir, int.Parse(dr["cs_id"].ToString())) == "")
            {
                // 当没有广告的时候显示
                // 热搜块
                temp = temp.Replace("#SerachHotBlock#", SearchHot);
            }
            else
            {
                temp = temp.Replace("#SerachHotBlock#", "");
            }

            #region 用途
            string tempPurpose = "";
            if (dr["CsPurpose"].ToString().Trim() != "")
            {
                string[] arrPurpose = dr["CsPurpose"].ToString().Trim().Split(',');
                if (arrPurpose.Length > 0)
                {
                    int loop = 0;
                    for (int i = 0; i < arrPurpose.Length; i++)
                    {
                        if (arrPurpose[i] != "")
                        {
                            int iPurpose = 0;
                            if (int.TryParse(arrPurpose[i], out iPurpose))
                            {
                                if (iPurpose > 0 && loop < 2)
                                {
                                    string url = "";
                                    string pname = "";
                                    CommonNavigationService.GetPurposeNameAndURL(iPurpose, out url, out pname);
                                    tempPurpose += "<a href=\"" + baseURL + url + "\" target=\"_blank\">" + pname + "</a> ";
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            temp = temp.Replace("#CsPurpose#", tempPurpose);
            // modified by chengl Aug.12.2013
            // temp = temp.Replace("#EveryCityPV#", GetSerialPVForEveryCity(int.Parse(dr["cs_id"].ToString())));
            temp = temp.Replace("#YesterdaySort#", GetSerialTotalPV(int.Parse(dr["cs_id"].ToString())));
            temp = temp.Replace("#MasterID#", dr["bs_Id"].ToString().Trim());

            // 级别排行地址
            string levelSpell = CommonNavigationService.GetCsLevelSpell(dr["cs_CarLevel"].ToString().Trim());
            if (levelSpell != "")
            {
                //temp = temp.Replace("#CsLevelPaihang#"
                //	, string.Format("<a href=\"{0}{1}/paihang/\" target=\"_blank\">全国{2}关注</a>"
                //	, baseURL, levelSpell, dr["cs_CarLevel"].ToString().Trim()));
                temp = temp.Replace("#CsLevelPaihang#"
                    , string.Format("<a href=\"{0}{1}/paihang/\" target=\"_blank\" data-channelid=\"2.21.836\" title=\"关注排行\">{2}关注排行</a>"
                    , baseURL, levelSpell, dr["cs_CarLevel"].ToString().Trim()));
            }
            else
            {
                //temp = temp.Replace("#CsLevelPaihang#", string.Format("<a href=\"javascript:void(0);\" target=\"_self\">全国{0}关注</a>", dr["cs_CarLevel"].ToString().Trim()));
                temp = temp.Replace("#CsLevelPaihang#", string.Format("<a href=\"javascript:void(0);\" target=\"_self\" title=\"关注排行\">{0}关注排行</a>", dr["cs_CarLevel"].ToString().Trim()));
            }
            // temp = temp.Replace("#CsLevelSpell#", CommonNavigationService.GetCsLevelSpell(dr["cs_CarLevel"].ToString().Trim()));
            // temp = temp.Replace("#CsLevelName#", dr["cs_CarLevel"].ToString().Trim() == "概念车" ? "" : dr["cs_CarLevel"].ToString().Trim());
            #region 论坛标签替换 add by sk 2017.03.16
            string bbsUrl = GetBBSURLByCsID(int.Parse(dr["cs_id"].ToString()));
            temp = temp.Replace("#BBSURL#", bbsUrl);

            if (bbsUrl == "http://baa.bitauto.com/")
            {
                temp = temp.Replace("#BBSBarTag#", "<li class=\"#ClassTag7#\"><a data-channelid=\"2.21.793\" id=\"CN_ShowLastLink\" class=\"no-link\">社区</a></li>");
            }
            else
            {
                temp = temp.Replace("#BBSBarTag#", string.Format("<li class=\"#ClassTag7#\"><a data-channelid=\"2.21.793\" id=\"CN_ShowLastLink\" target=\"_blank\" href=\"{0}\">社区</a></li>", bbsUrl));
            }
            #endregion
            // 头生成时间
            temp = temp.Replace("#GenerateDateTime#", DateTime.Now.ToString());

            #region 十佳车型
            bool hasBestTopCar = false;
            foreach (Define.BestTopCar btc in listBestTopCar)
            {
                if (btc.ListCsList.Contains(csid))
                {
                    hasBestTopCar = true;
                    temp = temp.Replace("#Top10CarLink#", String.Format("<a href=\"{0}\" target=\"_blank\" class=\"top10\" title=\"{1}\" alt=\"{1}\"></a>", btc.Link, btc.Title));
                    break;
                }
            }
            if (!hasBestTopCar)
            {
                temp = temp.Replace("#Top10CarLink#", "");
            }
            //int topYear = GetTopCarYear(csid);
            //string top10CarUrl = "";
            //switch (topYear)
            //{
            //    case 2011:
            //        top10CarUrl = "http://www.bitauto.com/top10cars/gd_2011/";
            //        break;
            //    case 2012:
            //        top10CarUrl = "http://www.bitauto.com/top10cars/";
            //        break;
            //}
            //if (String.IsNullOrEmpty(top10CarUrl))
            //    temp = temp.Replace("#Top10CarLink#", "");
            //else
            //    temp = temp.Replace("#Top10CarLink#", String.Format("<a href=\"{0}\" target=\"_blank\" class=\"top10\" title=\"{1}年度十佳车\" alt=\"{1}年度十佳车\"></a>", top10CarUrl, topYear));

            #endregion

            #region 面包屑广告

            //if (tagID == 0)
            //{
            temp = temp.Replace("#SerialMianBaoAD#", GetSerialSummaryMianBaoADV2(fileDir, int.Parse(dr["cs_id"].ToString())));
            //兼容老标签
            temp = temp.Replace("#SerialMianBaoADV1#", GetSerialSummaryMianBaoAD(fileDir, int.Parse(dr["cs_id"].ToString())));
            //}

            #endregion

            #region 搜索条 add by chengl Dec.4.2012

            if (dicSoBarV2.ContainsKey(tagID))
            {
                // 存在对应标签的
                temp = temp.Replace("#SoBar#", dicSoBarV2[tagID]);
            }
            else
            {
                // 不存在则取默认的
                temp = temp.Replace("#SoBar#", (dicSoBarV2.ContainsKey(-1) ? dicSoBarV2[-1] : ""));
            }
            //兼容老标签
            if (dicSoBar.ContainsKey(tagID))
            {
                // 存在对应标签的
                temp = temp.Replace("#SoBarV1#", dicSoBar[tagID]);
            }
            else
            {
                // 不存在则取默认的
                temp = temp.Replace("#SoBarV1#", (dicSoBar.ContainsKey(-1) ? dicSoBar[-1] : ""));
            }

            #endregion

            #region 改款上市时间
            //if (dictCarMarkTime.ContainsKey(csid))
            //{
            //    DateTime marketTime;
            //    string dateFormat = "yy年MM月dd日";
            //    if (dictCarMarkTime[csid].MarketTime.Split('-').Length <= 2)
            //    {
            //        dateFormat = "yy年MM月";
            //    }
            //    if (DateTime.TryParse(dictCarMarkTime[csid].MarketTime, out marketTime))
            //    {
            //        string showInfo = string.Empty;
            //        //int diff = CommonFunction.DateDiff("d", marketTime, DateTime.Now);
            //        int diff = DateTime.Compare(marketTime.Date, DateTime.Now.Date);
            //        string newsUrl = dictCarMarkTime[csid].Url;
            //        if (diff > 0)
            //        {
            //            showInfo = string.Format("新款将于{0}上市", marketTime.ToString(dateFormat));
            //        }
            //        else if (diff < 0)
            //        {
            //            showInfo = string.Format("新款已于{0}上市", marketTime.ToString(dateFormat));
            //        }
            //        else if (diff == 0)
            //        {
            //            showInfo = "新款于今日上市";
            //        }
            //        if (string.IsNullOrEmpty(newsUrl))
            //        {
            //            temp = temp.Replace("#SerialMarketTime#",
            //                string.Format("<li class=\"will-sale\"><a class=\"btn btn-primary btn-sm\">{0}</a></li>",
            //                    showInfo));
            //            temp = temp.Replace("#SerialMarketTimeV1#",
            //                string.Format("<span class=\"forecast-lnk\">{0}</span>", showInfo));
            //        }
            //        else
            //        {
            //            temp = temp.Replace("#SerialMarketTime#",
            //                string.Format(
            //                    "<li class=\"will-sale\"><a class=\"btn btn-primary btn-sm\" href=\"{1}\" target=\"_blank\">{0}</a></li>",
            //                    showInfo, newsUrl));
            //            temp = temp.Replace("#SerialMarketTimeV1#",
            //                string.Format("<a href=\"{1}\" target=\"_blank\" class=\"forecast-lnk\">{0}</a>", showInfo,
            //                    newsUrl));
            //        }
            //    }
            //    else
            //    {
            //        temp = temp.Replace("#SerialMarketTime#", "");
            //        temp = temp.Replace("#SerialMarketTimeV1#", "");
            //    }
            //}
            //else
            //{
            //    temp = temp.Replace("#SerialMarketTime#", "");
            //    temp = temp.Replace("#SerialMarketTimeV1#", "");
            //}
            #endregion

            #region 综述页新车上市提示 20170920
            //待查 待销 停销 在销
            string showText = "";

            List<TimeTagEntity> carList = CommonNavigationService.GetAllCarBySerialId(csid);
            //在售
            if (dr["CsSaleState"].ToString().Trim() == "在销" || dr["CsSaleState"].ToString().Trim() == "停销")
            {
                //95在销 96停销  97待销

                //筛选待销车款
                IEnumerable<TimeTagEntity> newCarList=carList.Where(i => i.CarSaleState == 97);                
                //在销车系下有待销车款
                if (newCarList.Count() > 0)
                {
                    //待销车款中筛选填写了上市时间的待销车款
                    IEnumerable<TimeTagEntity> newCarMarketDateTimeList = newCarList.Where(a => DateTime.Compare(a.MarketDateTime, DateTime.MinValue) != 0);
                    //存在填写了上市时间的待销车款
                    if (newCarMarketDateTimeList.Count() > 0)
                    {
                        TimeTagEntity car = newCarMarketDateTimeList.First();//从已经填写的时间中选择最早的时间
                        //排除未上市车填写了过去的上市时间（这种情况属于数据错误，通过程序筛选控制）
                        if (DateTime.Compare(car.MarketDateTime, DateTime.Now) >= 0)
                        {
                            showText = "将于" + car.MarketDateTime.ToString("yy年MM月dd日") + "上市";
                        }                            
                    }
                    //没有填写上市时间
                    else
                    {
                        //判断车款是否有实拍图
                        int count = 0;
                        foreach (var item in newCarList)
                        {
                            XmlDocument xmlDoc = CommonFunction.GetLocalXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"PhotoImage\SerialCarReallyPic\{0}.xml", item.CarId)));
                            if (xmlDoc != null && xmlDoc.HasChildNodes)
                            {
                                XmlNode node = xmlDoc.SelectSingleNode("//Data//Total");
                                var countStr = node.InnerText;
                                int.TryParse(countStr, out count);
                                if (count > 0)
                                {
                                    showText = "新款即将上市";
                                    break;
                                }
                                else
                                {
                                    //是否有指导价
                                    if (item.ReferPrice != "")
                                    {
                                        showText = "新款即将上市";
                                        break;
                                    }
                                }
                            }                            
                        }   
                    }                   
                }
                //新车款上市初期
                else
                {
                    IEnumerable<TimeTagEntity> newCarMarketDateTimeList = carList.Where(a => DateTime.Compare(a.MarketDateTime, DateTime.MinValue) != 0);
                    if (newCarMarketDateTimeList.Count() > 0)
                    {
                        TimeTagEntity car = newCarMarketDateTimeList.First();//倒叙排列，取第一个即可
                        if (car != null)
                        {
                            int days = GetDaysAboutCurrentDateTime(car.MarketDateTime);
                            if (days >= 0 && days <= 30)
                            {
                                //只有一个年款    ***新车上市***
                                if (carList.GroupBy(i => i.CarYearType).Count() == 1)
                                {
                                    showText = "新车上市";
                                }
                                //不止一个年款    ***新款上市***
                                else
                                {
                                    showText = "新款上市";
                                }
                            }
                        }
                    }                                   
                }                
            }
            //待查 待销(未上市)
            else
            {
                //筛选填写了上市时间的待销车
                IEnumerable<TimeTagEntity> newCarList = carList.Where(a => DateTime.Compare(a.MarketDateTime, DateTime.MinValue) != 0);
                //存在填写了上市时间的待销车
                if (newCarList.Count() > 0)
                {
                    TimeTagEntity car = carList.First();//从已经填写的时间中选择最早的时间
                    //排除未上市车填写了过去的上市时间（这种情况属于数据错误，通过程序筛选控制）
                    if (DateTime.Compare(car.MarketDateTime, DateTime.Now) >= 0)
                    {
                        showText = "将于" + car.MarketDateTime.ToString("yy年MM月dd日") + "上市";
                    }
                }                             
                //没有上市时间，判断有没有实拍图、指导价
                else
                {
                    //查找实拍图
                    int count = 0;
                    foreach (var item in carList)
                    {
                        XmlDocument xmlDoc = CommonFunction.GetLocalXmlDocument(Path.Combine(CommonData.CommonSettings.SavePath, string.Format(@"PhotoImage\SerialCarReallyPic\{0}.xml", item.CarId)));
                        if (xmlDoc != null && xmlDoc.HasChildNodes)
                        {
                            XmlNode node = xmlDoc.SelectSingleNode("//Data//Total");
                            var countStr = node.InnerText;
                            int.TryParse(countStr, out count);
                            if (count > 0)
                            {
                                showText = "即将上市";
                                break;
                            }
                            //是否有指导价
                            else
                            {
                                //是否有指导价
                                if (item.ReferPrice != "")
                                {
                                    showText = "即将上市";
                                    break;
                                }
                            }
                        }                        
                    } 
                    
                }
            }

            temp = temp.Replace("#SerialMarketTime#",  string.IsNullOrWhiteSpace(showText)?"": string.Format("<li class=\"will-sale\"><a class=\"btn btn-primary btn-sm\">{0}</a></li>", showText));
            temp = temp.Replace("#SerialMarketTimeV1#", string.IsNullOrWhiteSpace(showText) ? "" : string.Format("<li class=\"will-sale\"><a class=\"btn btn-primary btn-sm\">{0}</a></li>", showText));

            #endregion

            #region 子品牌是否停销  
            if (dr["CsSaleState"].ToString().Trim() == "停销")
            {
                temp = temp.Replace("#IsNoSale#", "<span style=\"color:#c00;font-weight:bold;\">停售</span> ");
            }
            else
            {
                temp = temp.Replace("#IsNoSale#", "");
            }
            #endregion

            #region CTCC
            string urlCTCC = GetSerialCTCCByCsID(int.Parse(dr["cs_id"].ToString()));
            if (urlCTCC != "")
            {
                temp = temp.Replace("#CTCC#", "<a target=\"_blank\" href=\"" + urlCTCC + "\">CTCC参赛车</a>");
            }
            else
            {
                temp = temp.Replace("#CTCC#", string.Empty);
            }
            #endregion

            #region 子品牌年款
            // 子品牌年款
            List<int> saleYear = new List<int>();
            DataRow[] drsYear = GetCarYearByCsID(int.Parse(dr["cs_id"].ToString()));
            StringBuilder sbCarYear = new StringBuilder();
            StringBuilder sbCarYearV1 = new StringBuilder();
            // add by chengl Dec.9.2011
            if (dr["CsSaleState"].ToString().Trim() == "待销")
            {
                // 待销 子品牌显示未上市
                sbCarYear.Append("<li><a " + urlTarget + " id=\"carYearList_all\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\">未上市</a></li>");
                sbCarYearV1.Append("<a " + urlTarget + " id=\"carYearList_all\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\">未上市</a>");
            }
            else if (dr["CsSaleState"].ToString().Trim() != "停销" && dr["CsSaleState"].ToString().Trim() != "待查")
            {
                sbCarYear.Append("<li><a rel=\"nofollow\" " + urlTarget + " id=\"carYearList_all\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\">全部在售</a></li>");
                sbCarYearV1.Append("<a rel=\"nofollow\" " + urlTarget + " id=\"carYearList_all\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\">全部在售</a>");
            }
            else { }

            if (drsYear != null && drsYear.Length > 0)
            {
                // modified by chengl Jul.10.2012
                int loopYear = 1;
                foreach (DataRow drYear in drsYear)
                {
                    // 在销年款
                    if (!saleYear.Contains(int.Parse(drYear["carYear"].ToString())))
                    { saleYear.Add(int.Parse(drYear["carYear"].ToString())); }
                    if (loopYear <= 3)
                    {
                        if (sbCarYearV1.Length > 0)
                        // { sbCarYear.Append(" | "); }
                        { sbCarYearV1.Append("<s>|</s>"); }
                        sbCarYearV1.Append("<a " + urlTarget + " data-channelid=\"2.21.837\"  id=\"carYearList_" + drYear["carYear"].ToString() + "\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/" + drYear["carYear"].ToString() + "/\">" + drYear["carYear"].ToString() + "款</a>");

                        sbCarYear.Append("<li><a " + urlTarget + " data-channelid=\"2.21.837\"  id=\"carYearList_" + drYear["carYear"].ToString() + "\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/" + drYear["carYear"].ToString() + "/\">" + drYear["carYear"].ToString() + "款</a></li>");
                    }
                    loopYear++;
                }
            }
            // modified by chengl Jun.20.2011
            // 索纳塔八 不显示 停销年款
            if (IsHasNoSaleCarByCsID(int.Parse(dr["cs_id"].ToString())) && dr["cs_id"].ToString() != "1568")
            {
                if (dicSerialNoSaleYear.ContainsKey(csid))
                {
                    List<int> noSale = new List<int>();
                    foreach (int noSaleYear in dicSerialNoSaleYear[csid])
                    {
                        if (!saleYear.Contains(noSaleYear) && !noSale.Contains(noSaleYear))
                        { noSale.Add(noSaleYear); }
                    }
                    if (noSale.Count > 0)
                    {
                        if (sbCarYearV1.Length > 0)
                        { sbCarYearV1.Append("<s>|</s>"); }
                        sbCarYearV1.Append("<dl id=\"bt_car_spcar\" data-channelid=\"2.21.838\" class=\"\">");
                        sbCarYearV1.Append("<dt>停售年款<em></em></dt>");
                        sbCarYearV1.Append("<dd style=\"display: none;\">");

                        sbCarYear.Append("<li class=\"offsale-years drop-layer-box\">");
                        sbCarYear.Append("<a data-channelid=\"2.21.838\" class=\"\">停售年款</a>");
                        sbCarYear.Append("<div class=\"drop-layer\">");
                        int loop = 1;
                        foreach (int noYear in noSale)
                        {
                            if (loop == noSale.Count)
                            {
                                sbCarYearV1.Append("<a class=\"last_a\" ");
                            }
                            else
                            {
                                sbCarYearV1.Append("<a ");
                            }
                            sbCarYearV1.Append(urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/" + noYear.ToString() + "\">" + noYear + "款</a>");
                            sbCarYear.Append("<a ");
                            sbCarYear.Append(urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/" + noYear.ToString() + "\">" + noYear + "款</a>");
                            loop++;
                        }
                        sbCarYearV1.Append("</dd>");
                        sbCarYearV1.Append("</dl>");
                        sbCarYear.Append("</div>");
                        sbCarYear.Append("</li>");
                    }
                }

                //if (sbCarYear.Length > 0)
                //{ sbCarYear.Append(" | "); }
                //sbCarYear.Append("<a target=\"_blank\" href=\"http://www.cheyisou.com/chexing/" + HttpUtility.UrlEncode(dr["cs_ShowName"].ToString().Trim()) + "/1.html?para=os|0|en|utf8\">停售车款</a>");
            }
            temp = temp.Replace("#CarYearList#", sbCarYear.ToString());
            temp = temp.Replace("#CarYearListV1#", sbCarYearV1.ToString());
            #endregion

            #region del by sk 2016.02.02 子品牌是否在车展中
            //if (tagID == 24)
            //{
            //	// 广州车展
            //	bool isInExhibition = CheckExhibition(int.Parse(dr["cs_id"].ToString()), htSerialExhibition);
            //	if (isInExhibition)
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag24#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/2009/" + dr["urlSpell"].ToString().Trim().ToLower() + "/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">广州车展</a></li>");
            //	}
            //	else
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag24#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/\" target=\"_blank\">广州车展</a></li>");
            //	}
            //}
            //else if (tagID == 29)
            //{
            //	// 北京2010车展
            //	bool isInBeiJing2010 = CheckExhibition(int.Parse(dr["cs_id"].ToString()), htSerialBeiJing2010);
            //	if (isInBeiJing2010)
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag29#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/beijing/2010/" + dr["urlSpell"].ToString().Trim().ToLower() + "/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">北京车展</a></li>");
            //	}
            //	else
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag29#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/beijing/\" target=\"_blank\">北京车展</a></li>");
            //	}
            //}
            //else if (tagID == 33)
            //{
            //	// 2010广州车展
            //	bool isInGuangZhou2010 = CheckExhibition(int.Parse(dr["cs_id"].ToString()), htSerialGuangZhou2010);
            //	if (isInGuangZhou2010)
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag33#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/2010/" + dr["urlSpell"].ToString().Trim().ToLower() + "/" + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">广州车展</a></li>");
            //	}
            //	else
            //	{
            //		temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca #ClassTag33#\"><a " + urlTarget + " href=\"http://chezhan.bitauto.com/guangzhou-chezhan/\" target=\"_blank\">广州车展</a></li>");
            //	}
            //}
            //else //if (tagID == 34)
            //{
            //	// 2011 上海车展
            //	temp = temp.Replace("#ExhibitionTag#", "<li class=\"loca\"><a target=\"_blank\" href=\"http://chezhan.bitauto.com/shanghai/\" >上海车展</a></li>");
            //}

            #endregion

            //#region 国产还是进口 标签头替换
            //if (dr["Cp_Country"].ToString().Trim() != "中国")
            //{
            //    temp = temp.Replace("#CpCountry#", "进口");
            //}
            //else
            //{
            //    temp = temp.Replace("#CpCountry#", "");
            //}
            //#endregion

            //#region 易车惠 add by Nov.7.2013 chengl

            //if(tagID==0)
            //{
            //    // 子品牌综述页有
            //    if(dicCsGoods.ContainsKey(csid))
            //    {
            //        temp = temp.Replace("#yichehui#", "<a  class=\"ad-yichehui\" target=\"_blank\" href=\"" + dicCsGoods[csid].Replace("/detail", "/all/detail") + "?WT.mc_id=car4" + "\" >易车惠有特价，立即查看&gt;&gt;</a>");
            //    }
            //    else
            //    {
            //         temp = temp.Replace("#yichehui#", "");
            //    }
            //}

            //#endregion

            #region 面包削主品牌和品牌重复
            string msBrand = dr["cb_name"].ToString().Trim();
            string masterOrBrand = "";
            if (dr["Cp_Country"].ToString().Trim() != "中国")
            {
                msBrand = "进口" + dr["cb_name"].ToString().Trim();
            }
            if (msBrand == dr["bs_Name"].ToString().Trim() || (dr["cb_name"].ToString().Trim() == dr["bs_Name"].ToString().Trim()))
            {
                // 重名留主品牌
                masterOrBrand = "<a href=\"" + baseURL + dr["urlspell"].ToString().Trim().ToLower() + "/\" target=\"_blank\">" + dr["bs_Name"].ToString().Trim() + "</a>";
            }
            else
            {
                // 不重名都保留
                masterOrBrand = "<a href=\"" + baseURL + dr["urlspell"].ToString().Trim().ToLower() + "/\" target=\"_blank\">" + dr["bs_Name"].ToString().Trim() + "</a> &gt; <a href=\"http://car.bitauto.com/" + dr["cbAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_blank\">" + msBrand + "</a>";
            }
            temp = temp.Replace("#MasterOrBrand#", masterOrBrand);
            #endregion

            #region 车贷or保养

            //// 如果是指定的子品牌ID 则显示车贷标签
            //if (listCheDaiCsID.Contains(csid))
            //{
            // modified by chengl Jan.3.2013 所有子品牌开放车贷标签
            // 如果有车贷
            temp = temp.Replace("#CheDaiStart#", "");
            temp = temp.Replace("#CheDaiEnd#", "");
            temp = temp.Replace("#BaoYangStart#", "");
            temp = temp.Replace("#BaoYangEnd#", "");
            temp = temp.Replace("#BaoYangCss#", "#ClassTag32#");
            temp = temp.Replace("#BaoYangChange#", "true");
            temp = temp.Replace("#WeiHuBaoYangHide#", "<a id=\"CN_ShowDDLinkBaoyang\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            //}
            //else
            //{
            //    // 如果没有车贷 显示保养
            //    temp = temp.Replace("#CheDaiStart#", "<!--");
            //    temp = temp.Replace("#CheDaiEnd#", "-->");
            //    temp = temp.Replace("#BaoYangStart#", "");
            //    temp = temp.Replace("#BaoYangEnd#", "");
            //    temp = temp.Replace("#BaoYangCss#", "");
            //    temp = temp.Replace("#BaoYangChange#", "false");
            //    temp = temp.Replace("#WeiHuBaoYangHide#", "");
            //}

            #endregion

            #region 保养
            //bool isHasBaoYang = false;
            //if (htBaoYang != null && htBaoYang.Count > 0)
            //{
            //	if (htBaoYang.ContainsKey(dr["cs_id"].ToString()))
            //	{ isHasBaoYang = true; }
            //}
            //if (isHasBaoYang)
            //{
            //	// 有保养信息
            //	// temp = temp.Replace("#WeiHuBaoYang#", "<a " + urlTarget + " class=\"w75\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">维修养护</a>");
            //	// temp = temp.Replace("#WeiHuBaoYangNew#", "<a id=\"CN_ShowDDLinkBaoyang\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            //	temp = temp.Replace("#WeiHuBaoYangNew#", "<a " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            //}
            //else
            //{
            //	// 无保养信息
            //	// temp = temp.Replace("#WeiHuBaoYang#", "维修养护");
            //	// modified by chengl May.10.2012
            //	temp = temp.Replace("#WeiHuBaoYangNew#", "<a class=\"nolink\">养护</a>");
            //}
            temp = temp.Replace("#WeiHuBaoYangNew#", "<a " + urlTarget + " data-channelid=\"2.21.790\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/baoyang/\">养护</a>");
            #endregion

            #region 销量

            if (listProduceAndSell.Contains(csid))
            {
                // 有销量数据
                //temp = temp.Replace("#XiaoLiang#", "<a target=\"_blank\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/xiaoliang/\">" + dr["cs_seoname"] + "销量</a> <em>|</em> ");
                temp = temp.Replace("#XiaoLiang#", "<a target=\"_blank\" href=\"http://index.bitauto.com/xiaoliang/s" + dr["cs_id"] + "/\">销量指数</a> <em>|</em> ");
            }
            else
            {
                // 无销量数据
                temp = temp.Replace("#XiaoLiang#", "");
            }

            #endregion

            #region 导购 & 评测
            // modified by chengl 
            // string urlDaoGou = GetSerialDaoGouURLByCsID(int.Parse(dr["cs_id"].ToString()));
            string urlDaoGou = GetSerialPingCeURLByCsID(int.Parse(dr["cs_id"].ToString()));
            /*
             if (urlDaoGou != "")
            {
                if (isYear)
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag22# #ClassTag28# #ClassTag19#\"><a " + urlTarget + " href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">车型详解</a></li>");
                }
                else
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 非年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag22# #ClassTag28# #ClassTag19#\"><a " + urlTarget + " href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">车型详解</a></li>");
                }
            }
            else
            {
                // modified by chengl Jun.8.2011
                // temp = temp.Replace("#DaoGouURL#", "车型详解");
                // modified by chengl May.10.2012
                temp = temp.Replace("#DaoGouURL#", "<li><a class=\"nolink\">车型详解</a></li>");
            }
             */
            if (urlDaoGou != "")
            {
                if (isYear)
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag22#\"><a target=\"_self\" data-channelid=\"2.21.784\" href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">车型详解</a></li>");
                }
                else
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 非年款
                    temp = temp.Replace("#DaoGouURL#", "<li id=\"liDaoGou\" class=\"#ClassTag22#\"><a target=\"_self\" data-channelid=\"2.21.784\" href=\"" + string.Format(baseURL + urlDaoGou, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">车型详解</a></li>");
                }
            }
            else
            {
                // modified by chengl Jun.8.2011
                // temp = temp.Replace("#DaoGouURL#", "车型详解");
                // modified by chengl May.10.2012
                temp = temp.Replace("#DaoGouURL#", "<li><a data-channelid=\"2.21.784\" class=\"no-link\">车型详解</a></li>");
            }
            #endregion

            #region 文章

            string urlWenZhang = "{0}/#year#/wenzhang/";// GetSerialWenZhangURLByCsID(int.Parse(dr["cs_id"].ToString()));
            int serialId = ConvertHelper.GetInteger(dr["cs_id"]);
            bool hasSerialNews = CommonNavigationService.HasSerialNews(ConvertHelper.GetInteger(dr["cs_id"]));
            //dictHasSerialNews.ContainsKey(serialId) ? dictHasSerialNews[serialId] : false; 
            if (hasSerialNews)
            {
                if (isYear)
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 年款
                    temp = temp.Replace("#WenZhangURL#", "<li class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag28# #ClassTag19# #ClassTag41#\"><a " + urlTarget + " data-channelid=\"2.21.785\" href=\"" + string.Format(baseURL + urlWenZhang, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">文章</a></li>");
                }
                else
                {
                    // 增加安全 add by chengl Jun.12.2012
                    // 非年款
                    temp = temp.Replace("#WenZhangURL#", "<li class=\"#ClassTag10# #ClassTag8# #ClassTag34# #ClassTag21# #ClassTag28# #ClassTag19# #ClassTag41#\"><a " + urlTarget + " data-channelid=\"2.21.785\" href=\"" + string.Format(baseURL + urlWenZhang, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">文章</a></li>");
                }
            }
            else
            {
                temp = temp.Replace("#WenZhangURL#", "<li><a data-channelid=\"2.21.785\" class=\"no-link\">文章</a></li>");
            }

            #endregion

            #region 视频

            if (CommonData.ListHasVideoSerialID != null && CommonData.ListHasVideoSerialID.Count > 0)
            {
                if (CommonData.ListHasVideoSerialID.Contains(int.Parse(dr["cs_id"].ToString())))
                { temp = temp.Replace("#ShiPin#", "<a " + urlTarget + " data-channelid=\"2.21.783\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/shipin/\">视频</a>"); }
                else
                { temp = temp.Replace("#ShiPin#", "<a class=\"no-link\">视频</a>"); }
            }
            else
            {
                // 如果接口问题默认全部显示
                temp = temp.Replace("#ShiPin#", "<a " + urlTarget + " data-channelid=\"2.21.783\" href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/shipin/\">视频</a>");
            }

            #endregion

            #region 置换

            // modified by chengl Jul.26.2012 高总需求所有子品牌开放置换
            // temp = temp.Replace("#ZhiHuanURL#", "<a id=\"CN_ShowDDLinkZhiHuan\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/zhihuan/\">置换</a>");

            //// 如果子品牌有置换信息
            //if (listZhiHuanCsID.Contains(csid))
            //{
            //    // 有置换数据
            //    temp = temp.Replace("#ZhiHuanURL#", "<a id=\"CN_ShowDDLinkZhiHuan\" " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/zhihuan/\">置换</a>");
            //}
            //else
            //{
            //    // 无置换数据
            //    temp = temp.Replace("#ZhiHuanURL#", "<a class=\"nolink\">置换</a>");
            //}

            #endregion

            #region 新闻 行情 挪至更多标签内

            //// 新闻
            //string urlXinWen = GetSerialNewURLByName(int.Parse(dr["cs_id"].ToString()), "xinwen");
            //if (urlXinWen != "")
            //{
            //    if (isYear)
            //    {
            //        // 年款
            //        temp = temp.Replace("#XinWenURL#", "<a id=\"CN_ShowDDLinkXinWen\" " + urlTarget + " href=\"" + string.Format(baseURL + urlXinWen, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">新闻</a>");
            //    }
            //    else
            //    {
            //        // 非年款
            //        temp = temp.Replace("#XinWenURL#", "<a id=\"CN_ShowDDLinkXinWen\" " + urlTarget + " href=\"" + string.Format(baseURL + urlXinWen, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">新闻</a>");
            //    }
            //}
            //else
            //{
            //    temp = temp.Replace("#XinWenURL#", "");
            //}

            // 行情
            string urlHangQing = GetSerialNewURLByName(int.Parse(dr["cs_id"].ToString()), "hangqing");
            if (urlHangQing != "")
            {
                if (isYear)
                {
                    // 年款
                    temp = temp.Replace("#HangQingURL#", "<a id=\"CN_ShowDDLinkHangQing\" " + urlTarget + " href=\"" + string.Format(baseURL + urlHangQing, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#", "{0}") + "\">行情</a>");
                }
                else
                {
                    // 非年款
                    temp = temp.Replace("#HangQingURL#", "<a id=\"CN_ShowDDLinkHangQing\" " + urlTarget + " href=\"" + string.Format(baseURL + urlHangQing, dr["csAllSpell"].ToString().Trim().ToLower()).Replace("#year#/", string.Empty) + "\">行情</a>");
                }
            }
            else
            {
                temp = temp.Replace("#HangQingURL#", "");
            }

            #endregion

            #region 如果是年款 生成年款新闻js对象
            if (isYear)
            {
                string NewsCountJson = GetSerialYearNewCount(int.Parse(dr["cs_id"].ToString()));
                temp = temp.Replace("#SerialYearNewObj#", NewsCountJson == "" ? "null" : NewsCountJson);
            }
            #endregion

            #region 易车测试
            if (dr["bitautoTestURL"].ToString().Trim() != "")
            {
                temp = temp.Replace("#BitAutoTest#", "<em><a href=\"" + dr["bitautoTestURL"].ToString().Trim() + "\" target=\"_blank\" >易车测试车型</a></em>");
            }
            else
            {
                temp = temp.Replace("#BitAutoTest#", "");
            }
            #endregion

            #region 标签相关

            // 子品牌综述页面包削
            if (tagID == 0)
            {
                // 索纳塔八的话 名称全是 索纳塔八
                if (dr["cs_id"].ToString() == "1568")
                {
                    temp = temp.Replace("#SummaryMianBao#", "<strong>索纳塔八</strong>");
                }
                else
                {
                    temp = temp.Replace("#SummaryMianBao#", "<strong>" + dr["cs_ShowName"].ToString().Trim() + "</strong>");
                }
            }
            else
            {
                // 索纳塔八的话 名称全是 索纳塔八
                if (dr["cs_id"].ToString() == "1568")
                {
                    temp = temp.Replace("#SummaryMianBao#", "<a " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">索纳塔八</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                }
                else
                {
                    temp = temp.Replace("#SummaryMianBao#", "<a " + urlTarget + " href=\"" + baseURL + dr["csAllSpell"].ToString().Trim().ToLower() + "/\" target=\"_self\">" + dr["cs_ShowName"].ToString().Trim() + "</a> &gt; " + "<strong>" + CommonNavigationService.GetTagNameByID(tagID) + "</strong>");
                }
            }

            string encoderStr = tagID == 17 ? "gb2312" : "utf-8";
            temp = temp.Replace("#EncoderStr#", encoderStr);

            temp = temp.Replace("#currentTagForStat#", CommonNavigationService.GetCurrentTagForStat(tagID));
            temp = temp.Replace("#TagName#", CommonNavigationService.GetTagNameByID(tagID));

            temp = temp.Replace("#JsForIsRegion#", "");
            temp = temp.Replace("#TagID#", tagID.ToString());

            // temp = temp.Replace("#ClassTag" + tagID.ToString() + "#", "current");
            // add by chengl Apr.26.2012
            temp = temp.Replace("#ClassTag" + tagID.ToString() + "#", "active");
            temp = temp.Replace("#ClassTagV1" + tagID.ToString() + "#", "on");
            Regex regex = new Regex(@"#ClassTag(\d+)#", RegexOptions.IgnoreCase);
            temp = regex.Replace(temp, "");
            Regex regexV1 = new Regex(@"#ClassTagV1(\d+)#", RegexOptions.IgnoreCase);
            temp = regexV1.Replace(temp, "");
            #endregion
            ////车型详解导航头单独白色样式
            //if (tagID == 22)
            //{
            //    temp = temp.Replace("#white#", "white");
            //}
            //else
            //{
            //    temp = temp.Replace("#white#", "");
            //}

            return temp;
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 子品牌综述页面包屑广告
        /// </summary>
		/// 
        /// <param name="csID">子品牌ID</param>
        /// <returns></returns>
		private string GetSerialSummaryMianBaoAD(string fileDir, int csID)
        {
            string adStr = string.Empty;
            if (fileDir == "CsSummary")
                adStr = "<div id=\"divSerialSummaryMianBaoAD\" class=\"top_ad02\"><ins id=\"div_ba10f730-0c13-4dcf-aa81-8b5ccafc9e21\" type=\"ad_play_fs\" adplay_IP=\"\" adplay_AreaName=\"\"  adplay_CityName=\"\"    adplay_BrandID=\"" + csID.ToString() + "\"  adplay_BrandName=\"\"  adplay_BrandType=\"\"  adplay_BlockCode=\"ba10f730-0c13-4dcf-aa81-8b5ccafc9e21\"> </ins></div>";
            else
                adStr = "<div id=\"divSerialSummaryMianBaoAD\" class=\"top_ad02\"><ins id=\"div_ba10f730-0c13-4dcf-aa81-8b5ccafc9e21\" type=\"ad_play\" adplay_IP=\"\" adplay_AreaName=\"\"  adplay_CityName=\"\"    adplay_BrandID=\"" + csID.ToString() + "\"  adplay_BrandName=\"\"  adplay_BrandType=\"\"  adplay_BlockCode=\"ba10f730-0c13-4dcf-aa81-8b5ccafc9e21\"> </ins></div>";
            return adStr;
        }

        /// <summary>
        /// 子品牌综述页面包屑广告
        /// </summary>
        /// <param name="csID">子品牌ID</param>
        /// <returns></returns>
		private string GetSerialSummaryMianBaoADV2(string fileDir, int csID)
        {
            string adStr = string.Empty;
            if (fileDir == "CsSummary")
                adStr = "<div id=\"divSerialSummaryMianBaoAD\" class=\"top-ad\"><ins id=\"div_203c911e-1f73-4120-9226-429725211731\" data-type=\"ad_play\" data-adplay_IP=\"\" data-adplay_AreaName=\"\" data-adplay_CityName=\"\" data-adplay_BrandID=\"\" data-adplay_BrandName=\"\" data-adplay_BrandType=\"\" data-adplay_BlockCode=\"203c911e-1f73-4120-9226-429725211731\"> </ins></div>";
            else
                adStr = "<div id=\"divSerialSummaryMianBaoAD\" class=\"top-ad\"><ins id=\"div_ba10f730-0c13-4dcf-aa81-8b5ccafc9e21\" type=\"ad_play\" adplay_IP=\"\" adplay_AreaName=\"\"  adplay_CityName=\"\"    adplay_BrandID=\"" + csID.ToString() + "\"  adplay_BrandName=\"\"  adplay_BrandType=\"\"  adplay_BlockCode=\"ba10f730-0c13-4dcf-aa81-8b5ccafc9e21\"> </ins></div>";
            return adStr;
        }

        /// <summary>
        /// 子品牌城市UV排行
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private string GetSerialPVForEveryCity(int csID)
        {
            string csPVForEveryCity = "";
            if (dsEveryCityUV != null && dsEveryCityUV.Tables.Count > 1 && dsEveryCityUV.Tables[1].Rows.Count > 0)
            {
                DataRow[] drs = dsEveryCityUV.Tables[1].Select(" ID = '" + csID.ToString() + "' ");
                if (drs != null && drs.Length > 0)
                {
                    foreach (DataRow dr in drs)
                    {
                        if (csPVForEveryCity != "")
                        {
                            csPVForEveryCity += "|" + dr["cityID"].ToString() + ":" + dr["Sort"].ToString();
                        }
                        else
                        {
                            csPVForEveryCity += dr["cityID"].ToString() + ":" + dr["Sort"].ToString();
                        }
                    }
                }
            }
            return csPVForEveryCity;
        }
        /// <summary>
        /// 取全国子品牌级别排行
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private string GetSerialTotalPV(int csID)
        {
            string totalPV = "";
            if (htSerialPV != null && htSerialPV.Count > 0)
            {
                if (htSerialPV.ContainsKey(csID))
                {
                    totalPV = Convert.ToString(htSerialPV[csID]);
                }
            }
            return totalPV;
        }
        /// <summary>
        /// 取子品牌论坛地址
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private string GetBBSURLByCsID(int csID)
        {
            string bbsURL = "http://baa.bitauto.com/";
            if (dsBBS != null && dsBBS.Tables.Count > 0 && dsBBS.Tables[0].Rows.Count > 0)
            {
                DataRow[] drs = dsBBS.Tables[0].Select(" id = '" + csID.ToString() + "' ");
                if (drs != null && drs.Length > 0)
                {
                    bbsURL = drs[0]["url"].ToString().ToLower();
                }
            }
            return bbsURL;
        }
        ///// <summary>
        ///// 返回最佳车型的年份，如果不是十佳车型，返回0
        ///// </summary>
        ///// <param name="serialId"></param>
        //private int GetTopCarYear(int serialId)
        //{
        //    int year = 0;
        //    foreach (int tmpYear in Top10CarList.Keys)
        //    {
        //        if (Top10CarList[tmpYear].Contains(serialId))
        //        {
        //            year = tmpYear;
        //            break;
        //        }
        //    }
        //    return year;
        //}
        /// <summary>
        /// 取字频CTCC URL
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private string GetSerialCTCCByCsID(int csID)
        {
            string url = "";
            if (dsSerialCTCC != null && dsSerialCTCC.Tables.Count > 0 && dsSerialCTCC.Tables[0].Rows.Count > 0)
            {
                DataRow[] drs = dsSerialCTCC.Tables[0].Select(" csid=" + csID.ToString() + " ");
                if (drs != null && drs.Length > 0)
                {
                    url = drs[0]["url"].ToString();
                }
            }
            return url;
        }
        /// <summary>
        /// 是否在车展中
        /// </summary>
        /// <param name="csID">子品牌ID</param>
        /// <param name="ht">车展子品牌ID的集合</param>
        /// <returns></returns>
        private bool CheckExhibition(int csID, Hashtable ht)
        {
            bool isIn = false;
            if (ht.ContainsKey(csID.ToString()))
            {
                isIn = true;
            }
            return isIn;
        }
        /// <summary>
        /// 取子品牌所有年款的新闻数量
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private string GetSerialYearNewCount(int csID)
        {
            StringBuilder temp = new StringBuilder();
            if (docNewsCategory != null && docNewsCategory.HasChildNodes)
            {
                XmlNode xn = docNewsCategory.SelectSingleNode("/SerilaList/Serial[@id='" + csID.ToString() + "']");
                if (xn != null && xn.HasChildNodes)
                {
                    temp.Append("[");
                    foreach (XmlNode xnSub in xn.ChildNodes)
                    {
                        if (xnSub != null && xnSub.Attributes["xinwen"] != null && xnSub.Attributes["daogou"] != null && xnSub.Attributes["hangqing"] != null && xnSub.Attributes["yongche"] != null && xnSub.Attributes["shijia"] != null && xnSub.Attributes["pingce"] != null)
                        {
                            if (temp.Length > 1)
                            {
                                temp.Append(",");
                            }
                            temp.Append("{");
                            temp.Append("\"Year\":\"" + xnSub.Attributes["year"].Value.ToString() + "\",");
                            temp.Append("\"XW\":\"" + xnSub.Attributes["xinwen"].Value.ToString() + "\",");
                            temp.Append("\"DG\":\"" + xnSub.Attributes["daogou"].Value.ToString() + "\",");
                            temp.Append("\"HQ\":\"" + xnSub.Attributes["hangqing"].Value.ToString() + "\",");
                            temp.Append("\"YC\":\"" + xnSub.Attributes["yongche"].Value.ToString() + "\",");
                            temp.Append("\"SJ\":\"" + xnSub.Attributes["shijia"].Value.ToString() + "\",");
                            if (xnSub.Attributes["gaizhuang"] != null && xnSub.Attributes["gaizhuang"].ToString() != "")
                            { temp.Append("\"GZ\":\"" + xnSub.Attributes["gaizhuang"].Value.ToString() + "\","); }
                            else
                            { temp.Append("\"GZ\":\"0\","); }
                            temp.Append("\"PC\":\"" + xnSub.Attributes["pingce"].Value.ToString() + "\",");
                            if (xnSub.Attributes["pingceNewsId"] != null && xnSub.Attributes["pingceNewsId"].ToString() != "")
                            { temp.Append("\"PCid\":\"" + xnSub.Attributes["pingceNewsId"].Value.ToString() + "\""); }
                            else
                            { temp.Append("\"PCid\":\"0\""); }
                            temp.Append("}");
                        }
                    }
                    temp.Append("]");
                }
            }

            return temp.ToString();
        }
        /// <summary>
        /// 根据新闻名取新闻地址
        /// </summary>
        /// <param name="csID">子品牌ID</param>
        /// <param name="name">新闻分类名(新闻、行情)</param>
        /// <returns></returns>
        private string GetSerialNewURLByName(int csID, string name)
        {
            string url = "";
            if (docNewsCategory != null && docNewsCategory.HasChildNodes)
            {
                XmlNode xn = docNewsCategory.SelectSingleNode("/SerilaList/Serial[@id='" + csID.ToString() + "']");
                if (xn != null)
                {
                    if (xn.Attributes[name] != null && xn.Attributes[name].Value.ToString() != "0")
                    {
                        url = "{0}/#year#/" + name + "/";
                    }
                    else
                    { }
                }

            }
            return url;
        }

        /// <summary>
        /// 车型详解(评测) 地址
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private string GetSerialPingCeURLByCsID(int csID)
        {
            string url = "";
            // modified by chengl Jan.16.2012
            // 评测判断改至彩虹条
            if (dicPingCeFromRainbow.ContainsKey(csID))
            { url = "{0}/#year#/pingce/"; }

            return url;
        }


        /// <summary>
        /// 取子品牌导购标签URL
        /// modified by chengl 文章地址
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private string GetSerialWenZhangURLByCsID(int csID)
        {
            string url = "";
            if (docNewsCategory != null && docNewsCategory.HasChildNodes)
            {
                XmlNode xn = docNewsCategory.SelectSingleNode("/SerilaList/Serial[@id='" + csID.ToString() + "']");
                if (xn != null)
                {
                    //// modified by chengl Jan.16.2012
                    //// 评测判断改至彩虹条
                    //if (dicPingCeFromRainbow.ContainsKey(csID))
                    //{ url = "{0}/#year#/pingce/"; }
                    //// 评测
                    //if (xn.Attributes["pingce"] != null && xn.Attributes["pingce"].Value.ToString() != "0")
                    //{
                    //    //// modified by chengl Feb.9.2011
                    //    // 无论评测文章几篇都只显示到pingce目录
                    //    url = "http://car.bitauto.com/{0}/#year#/pingce/";

                    //    // if (xn.Attributes["pingce"].Value.ToString() == "1" && xn.Attributes["pingceNewsId"] != null && xn.Attributes["pingceNewsId"].ToString() != "")
                    //    //{
                    //    //// 只有1片评测
                    //    //url = "http://car.bitauto.com/{0}/pingce/p" + xn.Attributes["pingceNewsId"].Value.ToString().Trim() + "/";
                    //    //}
                    //    //else
                    //    //{
                    //    //    // 有多篇评测
                    //    //    url = "http://car.bitauto.com/{0}/#year#/pingce/";
                    //    //}
                    //}
                    if (xn.Attributes["shijia"] != null && xn.Attributes["shijia"].Value.ToString() != "0")
                    {
                        // 有试驾文章
                        url = "{0}/#year#/wenzhang/";
                    }
                    // 导购
                    else if (xn.Attributes["daogou"] != null && xn.Attributes["daogou"].Value.ToString() != "0")
                    {
                        // 导购
                        url = "{0}/#year#/wenzhang/";
                    }
                    // 新闻 和 行情 拿到更多标签分类中
                    //// 新闻
                    //else if (xn.Attributes["xinwen"] != null && xn.Attributes["xinwen"].Value.ToString() != "0")
                    //{
                    //    // 新闻
                    //    url = "http://car.bitauto.com/{0}/#year#/xinwen/";
                    //}
                    //// 行情
                    //else if (xn.Attributes["hangqing"] != null && xn.Attributes["hangqing"].Value.ToString() != "0")
                    //{
                    //    // 行情
                    //    url = "http://car.bitauto.com/{0}/#year#/hangqing/";
                    //}
                    // 用车
                    else if (xn.Attributes["yongche"] != null && xn.Attributes["yongche"].Value.ToString() != "0")
                    {
                        // 用车
                        url = "{0}/#year#/wenzhang/";
                    }
                    // 改装
                    else if (xn.Attributes["gaizhuang"] != null && xn.Attributes["gaizhuang"].Value.ToString() != "0")
                    {
                        // 改装
                        url = "{0}/#year#/wenzhang/";
                    }
                    // add by chengl Jun.12.2012 增加安全
                    else if (xn.Attributes["anquan"] != null && xn.Attributes["anquan"].Value.ToString() != "0")
                    {
                        // 安全
                        url = "{0}/#year#/wenzhang/";
                    }
                    else if (xn.Attributes["xinwen"] != null && xn.Attributes["xinwen"].Value.ToString() != "0")
                    {
                        // add by chengl Nov.19.2012
                        // 默认新闻状态
                        url = "{0}/#year#/wenzhang/";
                    }
                    else
                    { }
                }

            }
            return url;
        }
        /// <summary>
        /// 检查子品牌是否有停销车型
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private bool IsHasNoSaleCarByCsID(int csID)
        {
            bool isHasNoSale = false;
            if (dsNoSale != null && dsNoSale.Tables.Count > 0 && dsNoSale.Tables[0].Rows.Count > 0)
            {
                if (dsNoSale.Tables[0].Select("cs_id = '" + csID + "' ") != null && dsNoSale.Tables[0].Select("cs_id = '" + csID + "' ").Length > 0)
                { isHasNoSale = true; }
            }
            return isHasNoSale;
        }
        /// <summary>
        /// 取子品牌年款
        /// </summary>
        /// <param name="csID"></param>
        /// <returns></returns>
        private DataRow[] GetCarYearByCsID(int csID)
        {
            DataRow[] drs = null;
            if (dsSerialYear != null && dsSerialYear.Tables.Count > 0 && dsSerialYear.Tables[0].Rows.Count > 0)
            {
                drs = dsSerialYear.Tables[0].Select(" cs_id='" + csID + "' ");
            }
            return drs;
        }
        #endregion

        public int GetDaysAboutCurrentDateTime(DateTime dt)
        {
            DateTime currentDateTime = DateTime.Now;
            int days = (currentDateTime - dt).Days;
            return days;
        }
       
    }
}
