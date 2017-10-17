using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BitAuto.CarDataUpdate.Common;
using System.Configuration;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Utils;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Model;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 
    /// </summary>
    public class HangQing
    {
        #region del by lsf 2016-01-06
        /*
        public event LogHandler Log;

        private string _HangQingSpanUrl = "http://{0}.bitauto.com/cheshi/";
        private string _RootPath = string.Empty;
        //行情首页的脚本块
        private string _DefaultPageSpan = string.Empty;
        //350城市的脚本块
        private string _350CityHangqingNews = string.Empty;
        //省列表地址
        private string _ProvinceHangqingNews = string.Empty;
        

        /// <summary>
        /// 行情构造函数
        /// </summary>
        public HangQing()
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "City");
            _DefaultPageSpan = Path.Combine(_RootPath, "hangqing\\defaultpage");
            _350CityHangqingNews = Path.Combine(_RootPath, "hangqing\\citynews");
            _ProvinceHangqingNews = Path.Combine(_RootPath, "hangqing\\provincenews");
            
        }
         * /
        /*
        /// <summary>
        /// 得到城市行情的新闻
        /// </summary>
        /// <param name="cityId"></param>
        public void GetProvinceHangQingNews(int provinceId)
        {
            List<int> provinceIdList = new List<int>();
            List<int> eixtsIdList = CityInitData.GetProvinceIdList();
            if (provinceId > 0 && !eixtsIdList.Contains(provinceId))
            {
                OnLog("		The cityId is unvalid....", true);
                return;
            }
            else if (provinceId > 0)
            {
                provinceIdList.Add(provinceId);
            }
            else
            {
                provinceIdList = eixtsIdList;
            }
            //分类ID
            string cateIdString = CommonFunction.joinStringArray(CommonData.KindCatesForSerial["hangqing"]);
            int sCounter = 0;
            //循环得到城市ID列表
            foreach (int entity in provinceIdList)
            {
                sCounter++;
                OnLog("		Get City News:" + entity + " " + "HangQing" + "(" + sCounter + "/" + provinceIdList.Count + ")...", false);
                string xmlUrl = CommonData.CommonSettings.NewsUrl + "?provinceid=" + entity + "&nonewstype=2&getcount=1000&ismain=1&categoryId=" + cateIdString;
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(xmlUrl);
                    int newsCount = ConvertHelper.GetInteger(xmlDoc.SelectSingleNode("NewDataSet/newsAllCount/allcount").InnerText);
                    //如果没有新闻则返回
                    if (newsCount == 0) continue;
                    //if (newsCount > 1000) newsCount = 1000;

                    new CityProcesser(0).SaveCityNewsNumber(entity, "province", newsCount, "hangqing");

                    string xmlFileName = entity + ".xml";
                    xmlFileName = Path.Combine(_ProvinceHangqingNews, xmlFileName);
                    //保存新闻
                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFileName);
                }
                catch (Exception ex)
                {
                    OnLog("		Get City News:" + entity + " " + "HangQing" + "(" + sCounter + "/" + provinceIdList.Count + "),ErrorMess" + ex.Message, false);
                    continue;
                }
            }
        }
        
        /// <summary>
        /// 得到城市行情的新闻
        /// </summary>
        /// <param name="cityId"></param>
        public void Get350CityHangQingNews(int cityId)
        {
            List<int> cityIdList = new List<int>();
            List<int> eixtsIdList = CityInitData.Get350CityIdList();
            if (cityId > 0 && !eixtsIdList.Contains(cityId))
            {
                OnLog("		The cityId is unvalid....", true);
                return;
            }
            else if (cityId > 0)
            {
                cityIdList.Add(cityId);
            }
            else
            {
                cityIdList = eixtsIdList;
            }
            //分类ID
            string cateIdString = CommonFunction.joinStringArray(CommonData.KindCatesForSerial["hangqing"]);
            int sCounter = 0;
            //循环得到城市ID列表
            foreach (int entity in cityIdList)
            {
                sCounter++;
                OnLog("     Get City News:" + entity + " " + "HangQing" + "(" + sCounter + "/" + cityIdList.Count + ")...", false);
                string xmlUrl = CommonData.CommonSettings.NewsUrl + "?cityid=" + entity + "&nonewstype=2&getcount=1000&ismain=1&categoryId=" + cateIdString;
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(xmlUrl);
                    int newsCount = ConvertHelper.GetInteger(xmlDoc.SelectSingleNode("NewDataSet/newsAllCount/allcount").InnerText);
                    //如果没有新闻则返回
                    if (newsCount == 0) continue;
                    //if (newsCount > 1000) newsCount = 1000;

                    new CityProcesser(0).SaveCityNewsNumber(entity, "city", newsCount, "hangqing");

                    string xmlFileName = entity + ".xml";
                    xmlFileName = Path.Combine(_350CityHangqingNews, xmlFileName);
                    //保存新闻
                    CommonFunction.SaveXMLDocument(xmlDoc, xmlFileName);
                }
                catch (Exception ex)
                {
                    OnLog("     Get City News:" + entity + " " + "HangQing" + "(" + sCounter + "/" + cityIdList.Count + "),ErrorMess" + ex.Message, false);
                    continue;
                }
            }
        }
        
        /// <summary>
        /// 得到城市行情脚本块
        /// </summary>
        public void GetCityHangQingDefaultPageSpan(int cityId)
        {
            //得到城市列表
            Dictionary<int, City> cityList = CityInitData.GetCityDic();
            if (cityList == null || cityList.Count < 1)
            {
                OnLog("     Not get city list!", true);
                return;
            }
            if (cityId > 0 && !cityList.ContainsKey(cityId))
            {
                OnLog("     Not get the CityID,ID为" + cityId, true);
                return;
            }
            //得到城市全拼列表
            List<string> cityEnglistName = new List<string>();
            if (cityId > 0)
            {
                cityEnglistName.Add(cityList[cityId].CityEName);
            }
            else
            {
                foreach (KeyValuePair<int, City> entityCity in cityList)
                {
                    cityEnglistName.Add(entityCity.Value.CityEName);
                }
            }
            //循环得到内容
            foreach (string cityAllSpell in cityEnglistName)
            {
                OnLog("     Get City HangQing Default PageSpan:" + cityAllSpell, true);
                try
                {
                    //当前链接
                    string Url = string.Format(_HangQingSpanUrl, cityAllSpell);//得到内容的ID

                    //页面内容
                    string pageContent = CommonFunction.GetContentByUrl(Url, "gb2312");

                    if (string.IsNullOrEmpty(pageContent)) continue;
                    string contentStartTag = "<!--car_city_hangqing_start 标记注释，请不要删除-->";
                    string contentEndTag = "<!--标记注释，请不要删除car_city_hangqing_end-->";
                    //得到内容所在的索引
                    int contentStartIndex = pageContent.IndexOf(contentStartTag);
                    int contentEndIndex = pageContent.IndexOf(contentEndTag);
                    //如果没有得到开始标记和结束标记，或者开始标记的位置大于结束标记
                    if (contentStartIndex < 0 || contentEndIndex < 0 || contentEndIndex <= contentStartIndex) continue;
                    //得到中间的内容
                    string content = pageContent.Substring(contentStartIndex + contentStartTag.Length, contentEndIndex - contentStartIndex - contentStartTag.Length);
                    //移除开始标签
                    string removeContentStartTag = "<!--not_in_hangqing_ad_satrt 标记注释，请不要删除-->";
                    string removeContetnEndTag = "<!--标记注释，请不要删除not_in_hangqing_ad_end-->";
                    int removeContentStartIndex = content.IndexOf(removeContentStartTag);
                    while (removeContentStartIndex > 0)
                    {
                        int removeContentEndIndex = content.IndexOf(removeContetnEndTag);
                        //得到页面的内容
                        content = content.Remove(removeContentStartIndex, removeContentEndIndex + removeContetnEndTag.Length - removeContentStartIndex);
                        removeContentStartIndex = content.IndexOf(removeContentStartTag);
                    }

                    if (string.IsNullOrEmpty(content)) continue;
                    string filepath = Path.Combine(_DefaultPageSpan, cityAllSpell.ToLower() + ".txt");
                    //保存文件内容
                    CommonFunction.SaveFileContent(content, filepath, "gb2312");
                }
                catch (System.Exception ex)
                {
                    OnLog(ex.ToString(), false);
                }
                OnLog("     End City HangQing Default PageSpan:" + cityAllSpell, true);
            }

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
         *  * */
        #endregion
        
    }
}
