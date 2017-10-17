using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Model;
using System.IO;
using BitAuto.CarDataUpdate.Common.Utils;
using System.Xml;
using BitAuto.Car.ContentLib.TemplateParse;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Services;
using BitAuto.CarDataUpdate.Common.Enum;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    ///
    /// </summary>
    public class AskHtmlChunkGenerator
    {
        #region  del by lsf 2016-01-06
        /*
        public AskHtmlChunkGenerator()
        {
            _dataDirectory = CommonData.CommonSettings.SavePath;
            _xmlDicrectoryFormat = Path.Combine(_dataDirectory,
                AskDataService.OUTPUT_DIRECTORY_FORMAT);
            _outputDicrectoryFormat = Path.Combine(_dataDirectory,
                OUTPUT_DIRECTORY_FORMAT);
            _parser = new TemplateParser(TEMPLATE_DIRECTORY);
        }

        /// <summary>
        /// html文件输出目录的格式化字符串
        /// </summary>
        public const string OUTPUT_DIRECTORY_FORMAT = @"AskForCar\{0}\Html\";

        /// <summary>
        /// 模板文件目录
        /// </summary>
        private const string TEMPLATE_DIRECTORY = "TemplateFiles";

        /// <summary>
        /// 导航条模板文件名称
        /// </summary>
        private const string ASK_CHUNK_TEMPLATE = "答疑数据块.htm";

        private string _xmlDicrectoryFormat;

        private string _outputDicrectoryFormat;

        /// <summary>
        /// 数据存储目录
        /// </summary>
        private string _dataDirectory;

        /// <summary>
        /// 模板引擎
        /// </summary>
        private TemplateParser _parser;

        private QuestionInfo[] GetQuestions(string xmlFilePath)
        {
            XmlDocument askXmlDocument = new XmlDocument();
            askXmlDocument.Load(xmlFilePath);
            return QuestionInfo.ConvertToQuestions(askXmlDocument);
        }

        /// <summary>
        /// 根据xml文件和品牌信息生成答疑的数据块
        /// </summary>
        /// <param name="outputDirectoryPath"></param>
        /// <param name="xmlFilePath"></param>
        /// <param name="brand"></param>
        private void GenerateAskHtmlChunk(string outputDirectoryPath, string xmlFilePath, BrandBase brand)
        {
            //要注入模板的参数
            IDictionary<string, object> currentContext = new Dictionary<string, object>();
            currentContext.Add("brand", brand);

            XmlDocument askXmlDocument = CommonFunction.GetXmlDocument(xmlFilePath);

            currentContext.Add("alls", QuestionInfo.ConvertToQuestions(askXmlDocument, "all"));//全部
            //currentContext.Add("rewards", QuestionInfo.ConvertToQuestions(askXmlDocument, "reward"));//悬赏
            currentContext.Add("unsolveds", QuestionInfo.ConvertToQuestions(askXmlDocument, "unsolved"));//未解决
            currentContext.Add("solveds", QuestionInfo.ConvertToQuestions(askXmlDocument, "solved"));//解决
            //currentContext.Add("unanswereds", QuestionInfo.ConvertToQuestions(askXmlDocument, "unanswered"));//零回复

            currentContext.Add("isold", "1");//解决

            string htmlFileName = string.Format("{0}.htm", Path.GetFileNameWithoutExtension(xmlFilePath));
            string htmlFilePath = Path.Combine(outputDirectoryPath, htmlFileName);
            using (FileStream fileStream = new FileStream(htmlFilePath,
                FileMode.Create, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(fileStream);
                _parser.ParseTemplate(ASK_CHUNK_TEMPLATE, currentContext, writer);
                writer.Flush();
            }

            if (brand.Type == BrandType.Serial)
            {
                currentContext["isold"]="0";

                currentContext.Add("askbrandurl", CommonFunction.GetXmlElementInnerText(askXmlDocument, "/root/@url", string.Empty));
                currentContext.Add("askbrandsolvedurl", CommonFunction.GetXmlElementInnerText(askXmlDocument, "/root/@solvedurl", string.Empty));
                currentContext.Add("askbrandunsolvedurl", CommonFunction.GetXmlElementInnerText(askXmlDocument, "/root/@unsolvedurl", string.Empty));
                currentContext.Add("askbrandunansweredurl", CommonFunction.GetXmlElementInnerText(askXmlDocument, "/root/@unansweredurl", string.Empty));

                currentContext.Add("experts", AskExpertInfo.ConvertToExperts(askXmlDocument));

                StringBuilder html = new StringBuilder();
                using (StringWriter writer = new StringWriter(html))
                {
                    _parser.ParseTemplate(ASK_CHUNK_TEMPLATE, currentContext, writer);
                }

                CommonHtmlService.UpdateCommonHtml(new CommonHtmlEntity()
                {
                    ID = brand.Id,
                    BlockID = CommonHtmlEnum.BlockIdEnum.Ask,
                    TagID = CommonHtmlEnum.TagIdEnum.SerialSummary,
                    TypeID = CommonHtmlEnum.TypeEnum.Serial,
                    UpdateTime = DateTime.Now,
                    HtmlContent = html.ToString()
                });
            }
        }
        
        /// <summary>
        /// 生成答疑的数据块
        /// </summary>
        /// <param name="brandType"></param>
        /// <param name="brands"></param>
        private void GenerateAskHtmlChunksByBrandType(BrandType brandType, IEnumerable<BrandBase> brands)
        {
            string brandTypeString = brandType.ToString();
            DirectoryInfo outputDirectory = IOUtils.CreateDirecotry(
                string.Format(_outputDicrectoryFormat, brandTypeString));
            foreach (BrandBase brand in brands)
            {
                string xmlDirectoryPath = string.Format(_xmlDicrectoryFormat, brandTypeString);
                string fileName = string.Format("{0}.xml", brand.Id);
                string xmlFilePath = Path.Combine(xmlDirectoryPath, fileName);
                if (File.Exists(xmlFilePath))
                {
                    GenerateAskHtmlChunk(outputDirectory.FullName, xmlFilePath, brand);
                }
            }
        }
        
        /// <summary>
        /// 生成答疑的数据块
        /// </summary>
        public void Generate()
        {
            BrandDataService brandDataService = new BrandDataService(_dataDirectory);
            List<BrandBase> brandTree = brandDataService.GetBrandTree();

            //生成主品牌
            GenerateAskHtmlChunksByBrandType(BrandType.Masterbrand, brandTree);

            //生成品牌
            GenerateAskHtmlChunksByBrandType(BrandType.Brand, BrandBase.GetChildNodes(brandTree));

            //生成子品牌
            GenerateAskHtmlChunksByBrandType(BrandType.Serial, BrandBase.GetChildNodes(
                BrandBase.GetChildNodes(brandTree)));
        }

        /// <summary>
        /// 生成答疑的数据块
        /// </summary>
        public void Generate(BrandType brandType, int objId)
        {
            BrandDataService brandDataService = new BrandDataService(_dataDirectory);
            List<BrandBase> brandTree = brandDataService.GetBrandTree(brandType, objId);

            switch (brandType)
            {
                case BrandType.Serial:
                    //生成子品牌
                    GenerateAskHtmlChunksByBrandType(BrandType.Serial, BrandBase.GetChildNodes(
                        BrandBase.GetChildNodes(brandTree)));
                    break;
                case BrandType.Masterbrand:
                    //生成主品牌
                    GenerateAskHtmlChunksByBrandType(BrandType.Masterbrand, brandTree);
                    break;
                case BrandType.Brand:
                    //生成品牌
                    GenerateAskHtmlChunksByBrandType(BrandType.Brand, BrandBase.GetChildNodes(brandTree));
                    break;
            }
        }
         * */
        #endregion
        
    }
}
