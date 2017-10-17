using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Data;

using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.news;
using BitAuto.CarDataUpdate.DataProcesser.cn.com.baa.api;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.dealer;
using BitAuto.CarDataUpdate.DataProcesser.com.bitauto.imgsvr;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class MasterBrand
    {
        public event LogHandler Log;
        private string _RootPath = string.Empty;
        private string _NewsPath = string.Empty;
        private string _HangQing = string.Empty;
        private string _Daogou = string.Empty;
        private string _PingCe = string.Empty;
        private string _YongChe = string.Empty;
        private string _ShiPing = string.Empty;
        private string _Ask = string.Empty;
        private string _KouBei = string.Empty;
        private string _NewsNumberPath = "NewsNumber.xml";
        /*
         * nonewstype:2为不是外链新闻
         * include:是向下包含
         */
        private string _RequestCondition = "?nonewstype=2&getcount=1000&ismain=1&bigbrand={0}&categoryid={1}&include=1";

        /// <summary>
        /// 构造函数
        /// </summary>
        public MasterBrand()
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "MasterBrand");
            _NewsPath = Path.Combine(_RootPath, "News");
            _HangQing = Path.Combine(_RootPath, "HangQing");
            _Daogou = Path.Combine(_RootPath, "DaoGou");
            _PingCe = Path.Combine(_RootPath, "PingCe");
            _YongChe = Path.Combine(_RootPath, "YongChe");
            _ShiPing = Path.Combine(_RootPath, "Video\\New");
            _Ask = Path.Combine(_RootPath, "Ask");
            _KouBei = Path.Combine(_RootPath, "KouBei");
        }
        /// <summary>
        /// 得到主品牌新闻
        /// </summary>
        /// <param name="masterBrandId">主品牌ID</param>
        public void GetNews(int id) 
        {
            Dictionary<int, List<int>> masterDic = CommonFunction.GetMasterBrandDic();
            if (id != 0 && masterDic.ContainsKey(id))
            {
                List<int> brandList = masterDic[id];
                masterDic.Clear();
                masterDic[id] = brandList;
            }
            else if (id != 0 && !masterDic.ContainsKey(id))
                return;

            int counter = 0;
            Dictionary<int, int> masterNewsCount = new Dictionary<int, int>();
            foreach (int masterId in masterDic.Keys)
            {
                counter++;
                OnLog("Get master brand:" + masterId + " news(" + counter + "/" + masterDic.Count + ")...", false);
                if (!masterNewsCount.ContainsKey(masterId)) masterNewsCount[masterId] = 0;
                try
                {
                    string brandIdList = "";
                    foreach (int brandId in masterDic[masterId])
                    {
                        brandIdList += brandId + ",";
                    }
                    brandIdList = brandIdList.TrimEnd(new char[] { ',' });

                    string xmlFile = masterId + ".xml";
                    xmlFile = Path.Combine(_NewsPath, xmlFile);

                    string xmlUrl = CommonData.CommonSettings.NewsUrl + "?nonewstype=2&getcount=1000&include=1&ismain=1&bigbrand=" + brandIdList;
                    int NewsNumber = 0;
                    CommonFunction.SaveXMLDocument(xmlUrl, xmlFile, 1000, out NewsNumber);
                    masterNewsCount[masterId] = NewsNumber;
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
            CommonFunction.RecordNewsNumber(masterNewsCount, Path.Combine(_RootPath, _NewsNumberPath), "xinwen", "MasterBrand");
        }
        /// <summary>
        /// 得到主品牌的行情
        /// </summary>
        /// <param name="masterBrandId"></param>
        public void GetHangQingNews(int id) 
        {
            string type = "hangqing";
            SaveNewsContent(type, id,_HangQing);
        }
        /// <summary>
        /// 得到导购新闻
        /// </summary>
        /// <param name="id"></param>
        public void GetDaoGouNews(int id) 
        {
            string type = "daogou";
            SaveNewsContent(type, id,_Daogou);
        }
        /// <summary>
        /// 得到评测新闻
        /// </summary>
        /// <param name="id"></param>
        public void GetPingCe(int id) 
        {
            string type = "treepingce";
            SaveNewsContent(type, id,_PingCe);
        }
        /// <summary>
        /// 得到用车新闻
        /// </summary>
        /// <param name="id"></param>
        public void GetYongChe(int id) 
        {
            string type = "yongche";
            SaveNewsContent(type, id, _YongChe);
        }
        /// <summary>
        /// 得到视频新闻
        /// </summary>
        /// <param name="id"></param>
        public void GetShiPing(int id) 
        {
            Dictionary<int, List<int>> masterDic = CommonFunction.GetMasterBrandDic();
            if (id != 0 && masterDic.ContainsKey(id))
            {
                List<int> brandList = masterDic[id];
                masterDic.Clear();
                masterDic[id] = brandList;
            }
            else if (id != 0 && !masterDic.ContainsKey(id))
                return;

            int counter = 0;
            Dictionary<int, int> masterNewsCount = new Dictionary<int, int>();
            foreach (int masterId in masterDic.Keys)
            {
                counter++;
                OnLog("Get master brand:" + masterId + " videos(" + counter + "/" + masterDic.Count + ")...", false);
                if (!masterNewsCount.ContainsKey(masterId)) masterNewsCount[masterId] = 0;
                try
                {
                    string brandIdList = "";
                    foreach (int brandId in masterDic[masterId])
                    {
                        brandIdList += brandId + ",";
                    }
                    brandIdList = brandIdList.TrimEnd(new char[] { ',' });

                    string xmlFile = masterId + ".xml";
                    xmlFile = Path.Combine(_ShiPing, xmlFile);

                    string xmlUrl = CommonData.CommonSettings.NewsUrl + "?articaltype=3&getcount=10&include=1&bigbrand=" + brandIdList;
                    int NewsNumber = 0;
                    CommonFunction.SaveXMLDocument(xmlUrl, xmlFile, 10, out NewsNumber);
                    masterNewsCount[masterId] = NewsNumber;
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }

            CommonFunction.RecordNewsNumber(masterNewsCount, Path.Combine(_RootPath, _NewsNumberPath), "video", "MasterBrand");
        }
        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="logText"></param>
        public void OnLog(string logText, bool nextLine)
        {
            if (Log != null)
                Log(this, new LogArgs(logText, nextLine));
        }
        /// <summary>
        /// 保存新闻内容
        /// </summary>
        /// <param name="type"></param>
        private void SaveNewsContent(string type,int id,string filePath) 
        {

            string cateId = CommonFunction.joinStringArray(CommonData.KindCatesForSerial[type]);
            if (string.IsNullOrEmpty(cateId)) return;

            Dictionary<int, List<int>> masterDic = CommonFunction.GetMasterBrandDic();
            if (id != 0 && masterDic.ContainsKey(id))
            {
                List<int> brandList = masterDic[id];
                masterDic.Clear();
                masterDic[id] = brandList;
            }
            else if (id != 0 && !masterDic.ContainsKey(id))
                return;


            int counter = 0;
            Dictionary<int, int> masterNewsCount = new Dictionary<int, int>();
            foreach (int masterId in masterDic.Keys)
            {
                counter++;
                OnLog("Get master brand:" + masterId + " news " + type + " (" + counter + "/" + masterDic.Count + ")...", false);
                if (!masterNewsCount.ContainsKey(masterId)) masterNewsCount[masterId] = 0;
                try
                {
                    string brandIdList = CommonFunction.joinList(masterDic[masterId]);

                    string xmlFile = masterId + ".xml";
                    xmlFile = Path.Combine(filePath, xmlFile);

                    string xmlUrl = CommonData.CommonSettings.NewsUrl + string.Format(_RequestCondition, brandIdList, cateId);
                    int NewsNumber = 0;
                    CommonFunction.SaveXMLDocument(xmlUrl, xmlFile, 1000, out NewsNumber);
                    masterNewsCount[masterId] = NewsNumber;
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), true);
                }
            }
            CommonFunction.RecordNewsNumber(masterNewsCount, Path.Combine(_RootPath, _NewsNumberPath), type, "MasterBrand");
        }
        /// <summary>
        /// 得到主品牌的PV数据集
        /// </summary>
        /// <returns></returns>
        public DataSet GetMasterBrandPvCount()
        {
            string sql = @"select bs.bs_Id,bs.bs_Name,bs.bs_Country,bs.bs_seoname,bs.urlspell,uv.UVCount
                                        from dbo.Car_MasterBrand bs
                                        left join dbo.Car_MasterBrand_30UV uv on bs.bs_id=uv.bs_id
                                        where bs.isState=1 order by UVCount desc";
            try
            {
                using(DataSet ds = BitAuto.Utils.Data.SqlHelper.ExecuteDataset(CommonData.ConnectionStringSettings.CarChannelConnString, CommandType.Text, sql))
                {
                    if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1) return null;

                    return ds;
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 得到内容
        /// </summary>
        public void GetContent()
        {
            try
            {
                GetNews(0);
                GetHangQingNews(0);
                GetDaoGouNews(0);
                GetPingCe(0);
                GetYongChe(0);
                GetShiPing(0);
            }
            catch (System.Exception ex)
            {
                OnLog(ex.ToString(), true);
            }
        }
        
    }
}
