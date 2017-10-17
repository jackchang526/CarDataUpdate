using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using BitAuto.CarDataUpdate.Common.Utils;

namespace BitAuto.CarDataUpdate.Common.Model
{
    /// <summary>
    /// 树标签信息基类
    /// </summary>
    public class TagBase
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 标签类型
        /// </summary>
        public string Type { get; set; }

		/// <summary>
		/// 标签链接打开方式，_blank,_self,_top
		/// </summary>
		public string Target { get; set; }
    }


    /// <summary>
    /// 树标签信息
    /// </summary>
    public class TagInfo : TagBase
    {
        public string Url { get; set; }
    }


    /// <summary>
    /// 标签配置信息
    /// </summary>
    public class TagConfigInfo : TagBase
    {
        public TagConfigInfo()
        {
            this.UrlRules = new Dictionary<string, string>();
        }

        #region Public Constant Fileds

        /// <summary>
        /// 首页Url类型
        /// </summary>
        public const string HOME_URL_TYPE = "Home";

        /// <summary>
        /// 主品牌Url类型
        /// </summary>
        public const string MASTER_URL_TYPE = "Masterbrand";

        /// <summary>
        /// 品牌Url类型
        /// </summary>
        public const string BRAND_URL_TYPE = "Brand";

        /// <summary>
        /// 子品牌Url类型
        /// </summary>
        public const string SERIAL_URL_TYPE = "Serial";

        /// <summary>
        /// 搜索Url类型
        /// </summary>
        public const string SEARCH_URL_TYPE = "Search";

        #endregion


        #region Public Properties

        /// <summary>
        /// Url前缀
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// 获得搜索框内容的地址
        /// </summary>
        public string SearchFrameUrl
        {
            set
            {
                string url = value;
                if (!string.IsNullOrEmpty(url) && string.IsNullOrEmpty(SerachFrameContent))
                {
                    try
                    {
                        this.SerachFrameContent = HttpUtils.DownLoadString(url);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 搜索框内容
        /// </summary>
        public string SerachFrameContent { get; set; }

        /// <summary>
        /// Url规则，通过urlType取
        /// </summary>
        public Dictionary<string, string> UrlRules { get; set; }

        /// <summary>
        /// logo的样式名称
        /// </summary>
        public string LogoCssName { get; set; }

		/// <summary>
		/// 是否生成树
		/// </summary>
		public bool IsTree { get; set; }

        /// <summary>
        /// 标签文件输出目录
        /// </summary>
        public string OutputDirectory { get; set; }

        #endregion


        #region Public Mehtods

        /// <summary>
        /// 获得替换过变量的Url
        /// </summary>
        /// <param name="urlType">url类型</param>
        /// <param name="brand">品牌信息</param>
        /// <returns></returns>
        public string GetUrl(string urlType, BrandBase brand)
        {
            string url = string.Empty;
            if (!string.IsNullOrEmpty(urlType) && this.UrlRules.ContainsKey(urlType))
            {
                IDictionary<string, string> variables = new Dictionary<string, string>();
                //使用参数objId、objSpell替换url
                if (brand != null)
                {
                    variables.Add("objId", brand.Id.ToString());
                    variables.Add("objSpell", brand.AllSpell);
                }
                url = StringUtils.SubstituteVariables(this.BaseUrl
                       + this.UrlRules[urlType], variables);
            }

            return url;
        }

        /// <summary>
        /// 获得替换过变量的Url
        /// </summary>
        /// <param name="urlType">url类型</param>
        /// <returns></returns>
        public string GetUrl(string urlType)
        {
            return GetUrl(urlType, null);
        }

        /// <summary>
        /// 获得替换过变量的Url
        /// </summary>
        /// <param name="brand">品牌信息</param>
        /// <returns></returns>
        public string GetUrl(BrandBase brand)
        {
            if (brand == null)
            {
                throw new ArgumentNullException("brand", "品牌信息不能为null");
            }
            string urlType = GetUrlTypeByBrand(brand);
            return GetUrl(urlType, brand);
        }
		/// <summary>
		/// 获取面包屑的标题
		/// </summary>
		/// <returns></returns>
		public string GetCrumbTitle()
		{
			return this.Type == "baojia" ? "汽车报价" : this.Title;
		}
        #endregion


        #region Private Mehthods

        /// <summary>
        /// 根据品牌信息获得url类型
        /// </summary>
        /// <param name="brand">品牌信息</param>
        /// <returns></returns>
        private string GetUrlTypeByBrand(BrandBase brand)
        {
            string urlType = string.Empty;
            if (brand is MasterBrand)
            {
                urlType = TagConfigInfo.MASTER_URL_TYPE;
            }
            else if (brand is Brand)
            {
                urlType = TagConfigInfo.BRAND_URL_TYPE;
            }
            else if (brand is SerialBrand)
            {
                urlType = TagConfigInfo.SEARCH_URL_TYPE;
            }

            return urlType;
        }

        #endregion
    }

}
