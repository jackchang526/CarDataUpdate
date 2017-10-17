using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Enum;
using BitAuto.Utils;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class CommonNavigationV2
    {
        // 子品牌信息
        private DataSet _dsSerial;
        // 子品牌论坛地址
        private DataSet _dsBbs;
        private DataSet _dsCar;
        private DataSet _dsSerialYear;
        private DataSet _dsNoSale;
        // 同时包含国产进口的主品牌
        private List<int> _listMasterCpCountry;
        // 子品牌包含的停销车型的年款
        private Dictionary<int, List<int>> _dicSerialNoSaleYear;
        //十佳车型数据
        private List<Define.BestTopCar> _listBestTopCar;

        private Dictionary<int, string> _dicSoBarV2;
        private Dictionary<int, SerialMarketTimeEntity> _dictCarMarkTime;
        private IRazorEngineService _razorEngineService;
        public CommonNavigationV2()
        {
            InitData();
        }

        private void InitData()
        {
            // 子品牌信息
            _dsSerial = CommonNavigationService.GetSerialAllInfoForCommonNavigation();
            // 子品牌论坛地址
            _dsBbs = CommonNavigationService.BBSAllUrl();
            // 车型数据
            _dsCar = CommonNavigationService.GetCarAllInfoForCommonNavigation();
            // 子品牌年款
            _dsSerialYear = CommonNavigationService.GetAllCarYearByCsID();
            // 是否有停销车型
            _dsNoSale = CommonNavigationService.GetHasNoSaleSerial();
            // 子品牌包含的停销车型的年款
            _dicSerialNoSaleYear = CommonNavigationService.GetSerialNoSaleYear();
            //年度十佳车型
            _listBestTopCar = CommonNavigationService.GetAllBestTopCar();
            // 同时包含国产进口的主品牌
            _listMasterCpCountry = CommonNavigationService.GetAllMasterBrandCpCountry();

            _dictCarMarkTime = CommonNavigationService.GetSerialMarketTimeData();
            // 搜索条配置
            _dicSoBarV2 = CommonNavigationService.GetSoBarByTagV2();
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


        public void GenerateSerialNavigationByRazor(int serialId)
        {

            Dictionary<string, string> dictTemplate = new Dictionary<string, string>();

            List<TagEntity> serialList = new List<TagEntity>();
            serialList.Add(new TagEntity() { TagId = 0, TagName = "CsSummary" });
            serialList.Add(new TagEntity() { TagId = 0, TagName = "CsSummaryJs" });
            serialList.Add(new TagEntity() { TagId = 1, TagName = "CsCompare" });
            serialList.Add(new TagEntity() { TagId = 2, TagName = "CsPhoto" });
            serialList.Add(new TagEntity() { TagId = 3, TagName = "CsVideo" });
            serialList.Add(new TagEntity() { TagId = 5, TagName = "CsKouBei" });
            serialList.Add(new TagEntity() { TagId = 6, TagName = "CsAsk" });
            serialList.Add(new TagEntity() { TagId = 11, TagName = "CsYouHao" });
            serialList.Add(new TagEntity() { TagId = 8, TagName = "CsDaoGou" });
            serialList.Add(new TagEntity() { TagId = 10, TagName = "CsAnQuan" });
            serialList.Add(new TagEntity() { TagId = 12, TagName = "CsPrice" });
            serialList.Add(new TagEntity() { TagId = 18, TagName = "CsSellData" });
            serialList.Add(new TagEntity() { TagId = 13, TagName = "CsCity" });
            serialList.Add(new TagEntity() { TagId = 19, TagName = "CsXinWen" });
            serialList.Add(new TagEntity() { TagId = 21, TagName = "CsYongChe" });
            serialList.Add(new TagEntity() { TagId = 22, TagName = "CsPingCe" });
            serialList.Add(new TagEntity() { TagId = 23, TagName = "CsCMSNews" });
            serialList.Add(new TagEntity() { TagId = 27, TagName = "CsPaiHang" });
            serialList.Add(new TagEntity() { TagId = 28, TagName = "CsShiJia" });
            serialList.Add(new TagEntity() { TagId = 32, TagName = "CsMaintenance" });
            serialList.Add(new TagEntity() { TagId = 34, TagName = "CsGaiZhuang" });
            serialList.Add(new TagEntity() { TagId = 35, TagName = "CsYangChe" });
            serialList.Add(new TagEntity() { TagId = 38, TagName = "CsJiangJia" });
            serialList.Add(new TagEntity() { TagId = 40, TagName = "CsCheDai" });
            serialList.Add(new TagEntity() { TagId = 41, TagName = "CsWenZhang" });
            List<TagEntity> yearList = new List<TagEntity>();

            yearList.Add(new TagEntity() { TagId = 0, TagName = "CsSummaryForYear" });
            yearList.Add(new TagEntity() { TagId = 1, TagName = "CsCompareForYear" });
            yearList.Add(new TagEntity() { TagId = 2, TagName = "CsPhotoForYear" });
            yearList.Add(new TagEntity() { TagId = 8, TagName = "CsDaoGouForYear" });
            yearList.Add(new TagEntity() { TagId = 10, TagName = "CsAnQuanForYear" });
            yearList.Add(new TagEntity() { TagId = 19, TagName = "CsXinWenForYear" });
            yearList.Add(new TagEntity() { TagId = 21, TagName = "CsYongCheForYear" });
            yearList.Add(new TagEntity() { TagId = 22, TagName = "CsPingCeForYear" });
            yearList.Add(new TagEntity() { TagId = 28, TagName = "CsShiJiaForYear" });
            yearList.Add(new TagEntity() { TagId = 34, TagName = "CsGaiZhuangForYear" });
            yearList.Add(new TagEntity() { TagId = 41, TagName = "CsWenZhangForYear" });

            if (_dsSerial != null && _dsSerial.Tables.Count > 0 && _dsSerial.Tables[0].Rows.Count > 0)
            {
                if (serialId <= 0) return;
                DataRow[] drArr = _dsSerial.Tables[0].Select(string.Format(" cs_id={0} ", serialId.ToString()));
                if (drArr == null || drArr.Length <= 0) return;
                DataRow dr = drArr[0];

                foreach (TagEntity entity in serialList)
                {
                    string template = CommonFunction.ReadFile(
                        Path.Combine(CommonData.CommonSettings.CommonHeadLocalFilePath, entity.TemplatePath));
                    var model = new
                    {
                        TagName = entity.TagName,
                        TagId = entity.TagId,
                        BBSUrl = this.GetBBSURLByCsID(serialId),
                        Data = dr
                    };
                    var result = _razorEngineService.RunCompile(template, "serialTemplate", null, model);

                    //CommonFunction.InsertCommonHeadV2(serialId, entity.TagName, result);
                }
            }
        }

        private SerialVmEntity GetSerialViewModel(DataRow dr)
        {
            int masterId = ConvertHelper.GetInteger(dr["bs_Id"]);
            SerialVmEntity entity =new SerialVmEntity();
            entity.SerialId = ConvertHelper.GetInteger(dr["cs_id"]);
            entity.CsName = dr["cs_name"].ToString().Trim().Replace("·", "&bull;");
            entity.CsShowName = dr["cs_ShowName"].ToString().Trim().Replace("·", "&bull;");

            if (_listMasterCpCountry.Contains(masterId)
                && dr["Cp_Country"].ToString().Trim() != "中国")
            {
                entity.CsSpecialSEOName = "进口" + dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;");
            }
            else
            {
                entity.CsSpecialSEOName = dr["cs_seoname"].ToString().Trim().Replace("·", "&bull;");
            }
            entity.CsAllSpell = dr["csAllSpell"].ToString().Trim().ToLower();
            entity.MasterUrlSpell = dr["urlspell"].ToString().Trim().ToLower();
            entity.MasterName = dr["bs_Name"].ToString().Trim();
            entity.CbName = dr["cb_name"].ToString().Trim().Replace("·", "&bull;");
            entity.CbAllSpell = dr["cbAllSpell"].ToString().Trim().ToLower();
            entity.MasterId = masterId;
            entity.BBSURL = GetBBSURLByCsID(int.Parse(dr["cs_id"].ToString()));
            //entity.SoBar=

            return entity;
        }

        #region 获取数据
        private string GetBBSURLByCsID(int csID)
        {
            string bbsURL = "http://baa.bitauto.com/";
            if (_dsBbs != null && _dsBbs.Tables.Count > 0 && _dsBbs.Tables[0].Rows.Count > 0)
            {
                DataRow[] drs = _dsBbs.Tables[0].Select(" id = '" + csID.ToString() + "' ");
                if (drs != null && drs.Length > 0)
                {
                    bbsURL = drs[0]["url"].ToString().ToLower();
                }
            }
            return bbsURL;
        }
        #endregion
    }

    public class SerialVmEntity
    {
        public int SerialId { get; set; }
        public string CsShowName { get; set; }
        public string CsName { get; set; }
        public string CsSpecialSEOName { get; set; }
        public string CsAllSpell { get; set; }
        public string CsSaleState { get; set; }
        public int MasterId { get; set; }
        public string MasterUrlSpell { get; set; }
        public string MasterName { get; set; }
        public string CbName { get; set; }
        public string CbAllSpell { get; set; }

        public string MasterOrBrand { get; set; }

        public string SerachHotBlock { get; set; }
        public string BBSURL { get; set; }

        public string PingceUrl { get; set; }
        public string WenzhangUrl { get; set; }
        public bool HasVideo { get; set; }
    }
}
