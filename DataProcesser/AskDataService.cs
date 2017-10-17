using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using System.IO;
using BitAuto.CarDataUpdate.Common;
using System.Xml;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 答疑数据服务类
    /// </summary>
    public class AskDataService
    {
        #region  del by lsf 2016-01-06
        /*  
        public AskDataService(string dataDirectory)
            : this(dataDirectory, DEFAULT_ASK_DATA_COUNT)
        { }

        public AskDataService(string dataDirectory, int askDataCount)
        {
            _dataDirectory = dataDirectory;
            _outputDicrectoryFormat = Path.Combine(_dataDirectory, OUTPUT_DIRECTORY_FORMAT);
            _askDataCount = askDataCount;
            InitAskDataAPIFormats();
        }

        private void InitAskDataAPIFormats()
        {
            _askDataAPIFormats.Add(BrandType.Masterbrand.ToString(),
                "http://ask.bitauto.com/api/GetAllCategoryQuestion.ashx?masterbrandid={0}&max={1}");
            _askDataAPIFormats.Add(BrandType.Brand.ToString(),
                "http://ask.bitauto.com/api/GetAllCategoryQuestion.ashx?brandid={0}&max={1}");
            _askDataAPIFormats.Add(BrandType.Serial.ToString(),
                "http://ask.bitauto.com/api/GetAllCategoryQuestion.ashx?serialid={0}&max={1}&experttop=3");
        }

        /// <summary>
        /// 缺省获取的答疑数据条数
        /// </summary>
        private const int DEFAULT_ASK_DATA_COUNT = 6;

        /// <summary>
        /// xml文件输出目录的格式化字符串
        /// </summary>
        public const string OUTPUT_DIRECTORY_FORMAT = @"AskForCar\{0}\Xml\";

        /// <summary>
        /// 初始化答疑数据接口的格式化字符串
        /// </summary>
        private Dictionary<string, string> _askDataAPIFormats = new Dictionary<string, string>();

        /// <summary>
        /// 数据存储目录
        /// </summary>
        private string _dataDirectory;

        /// <summary>
        /// 答疑块生成目录全路径的格式化字符串
        /// </summary>
        private string _outputDicrectoryFormat;

        /// <summary>
        /// 获取的答疑数据条数
        /// </summary>
        int _askDataCount;
        
        /// <summary>
        /// 更新答疑数据
        /// </summary>
        public void UpdateAskData()
        {
            List<BrandBase> brandTree = GetBrandTree();
            DirectoryInfo outputDirectory = null;

            //生成主品牌
            outputDirectory = CommonFunction.CreateDirecotry(
                string.Format(_outputDicrectoryFormat, BrandType.Masterbrand.ToString()));
            foreach (BrandBase masterBrand in brandTree)
            {
                UpdateXmlFileOfAskData(BrandType.Masterbrand, masterBrand,
                    outputDirectory.FullName);
            }

            //生成品牌
            outputDirectory = CommonFunction.CreateDirecotry(
                string.Format(_outputDicrectoryFormat, BrandType.Brand.ToString()));
            foreach (BrandBase brand in BrandBase.GetChildNodes(brandTree))
            {
                UpdateXmlFileOfAskData(BrandType.Brand, brand,
                    outputDirectory.FullName);
            }

            //生成子品牌
            outputDirectory = CommonFunction.CreateDirecotry(
                string.Format(_outputDicrectoryFormat, BrandType.Serial.ToString()));
            foreach (BrandBase serialBrand in BrandBase.GetChildNodes(
                BrandBase.GetChildNodes(brandTree)))
            {
                UpdateXmlFileOfAskData(BrandType.Serial, serialBrand,
                    outputDirectory.FullName);
            }
        }

        /// <summary>
        /// 更新答疑数据的xml文件
        /// </summary>
        /// <param name="brandType">品牌类型</param>
        /// <param name="brand">品牌信息</param>
        /// <param name="outputFullDirectory">输出目录</param>
        private void UpdateXmlFileOfAskData(BrandType brandType, BrandBase brand,
            string outputFullDirectory)
        {
            try
            {
                string api = GetAskDataAPI(brandType, brand.Id, _askDataCount);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(api);
                string filePath = Path.Combine(outputFullDirectory,
                    string.Format("{0}.xml", brand.Id));
                xmlDocument.Save(filePath);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }

        }

        private List<BrandBase> GetBrandTree()
        {
            BrandDataService brandDataService = new BrandDataService(_dataDirectory);
            return brandDataService.GetBrandTree();
        }

        /// <summary>
        /// 获得答疑数据接口的url地址
        /// </summary>
        /// <param name="brandType">品牌类型</param>
        /// <param name="brandId">品牌id</param>
        /// <param name="askDataCount">获取的答疑数据条数</param>
        /// <returns></returns>
        private string GetAskDataAPI(BrandType brandType, int brandId, int askDataCount)
        {
            string brandTypeName = brandType.ToString();
            string api = string.Empty;
            if (_askDataAPIFormats.ContainsKey(brandTypeName))
            {
                api = string.Format(_askDataAPIFormats[brandTypeName], brandId, askDataCount);
            }
            return api;
        }
         * */
        #endregion
        
    }
}
