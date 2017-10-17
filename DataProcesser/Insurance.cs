using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;
using System.IO;
using System.Xml;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 
    /// </summary>
    public class Insurance
    {
        #region del by lsf 2016-01-06
        /*
        public event LogHandler Log;
        private string _RootPath = string.Empty;
        private string _BaoXianFilePath = string.Empty;
        private string _DaiKuanFilePath = string.Empty;
        /// <summary>
        /// 构造函数
        /// </summary>
        public Insurance()
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "MaiChe");
            _BaoXianFilePath = Path.Combine(_RootPath, "baoxian.xml");
            _DaiKuanFilePath = Path.Combine(_RootPath, "daikuan.xml");
        }

        /// <summary>
        /// 得到新闻内容
        /// </summary>
        public void GetContent()
        {
            GetBaoXianNews();
            GetDaiKuanNews();
        }

        /// <summary>
        /// 得到保险新闻
        /// </summary>
        public void GetBaoXianNews()
        {
            string cateIdString = CommonFunction.joinStringArray(CommonData.KindCatesForInsurance["baoxian"]);
            string xmlUrl = CommonData.CommonSettings.NewsUrl + "?nonewstype=2&getcount=1000&ismain=1&categoryId=" + cateIdString;

            XmlDocument xmldoc = new XmlDocument();
            try
            {
                OnLog("		Start BaoXian News ...... ", false);
                xmldoc.Load(xmlUrl);
                CommonFunction.SaveXMLDocument(xmldoc, _BaoXianFilePath);
            }
            catch (System.Exception ex)
            {
                OnLog("Get BaoXian News Error:ErrorMess==>" + ex.Message, false);
            }
        }
        /// <summary>
        /// 得到贷款新闻
        /// </summary>
        public void GetDaiKuanNews()
        {
            string cateIdString = CommonFunction.joinStringArray(CommonData.KindCatesForInsurance["daikuan"]);
            string xmlUrl = CommonData.CommonSettings.NewsUrl + "?nonewstype=2&getcount=1000&ismain=1&categoryId=" + cateIdString;

            XmlDocument xmldoc = new XmlDocument();
            try
            {
                OnLog("Start DaiKuan News ...... ", false);
                xmldoc.Load(xmlUrl);
                CommonFunction.SaveXMLDocument(xmldoc, _DaiKuanFilePath);
            }
            catch (System.Exception ex)
            {
                OnLog("Get DaiKuan News Error:ErrorMess==>" + ex.Message, false);
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
         * */
        #endregion
        
    }
}
