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

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class UsedCarHtmlChunkGenerator
    {

        /// <summary>
        /// 数据存储目录
        /// </summary>
        private string _dataDirectory;
        /// <summary>
        /// 导航条模板文件名称
        /// </summary>
        private const string USED_CAR_CHUNK_TEMPLATE = "二手车数据块.htm";
        /// <summary>
        /// html文件输出目录的格式化字符串
        /// </summary>
        public const string OUTPUT_DIRECTORY_FORMAT = @"UsedCarInfo\{0}_Right\Html\";

        /// <summary>
        /// 模板文件目录
        /// </summary>
        private const string TEMPLATE_DIRECTORY = "TemplateFiles";
        private string _xmlDicrectoryFormat;

        private string _outputDicrectoryFormat;
        /// <summary>
        /// 模板引擎
        /// </summary>
        private TemplateParser _parser;


		//public UsedCarHtmlChunkGenerator()
		//{
		//    _dataDirectory = CommonData.CommonSettings.SavePath;
		//    _xmlDicrectoryFormat = Path.Combine(_dataDirectory,
		//        UsedCarDataService.OUTPUT_DIRECTORY_FORMAT);
		//    _outputDicrectoryFormat = Path.Combine(_dataDirectory,
		//        OUTPUT_DIRECTORY_FORMAT);
		//    _parser = new TemplateParser(TEMPLATE_DIRECTORY);
		//}

		///// <summary>
		///// 生成二手车的数据块
		///// </summary>
		//public void Generate()
		//{
		//    BrandDataService brandDataService = new BrandDataService(_dataDirectory);
		//    List<BrandBase> brandTree = brandDataService.GetBrandTree();

		//    //生成子品牌
		//    GenerateUsedCarHtmlChunksByBrandType(BrandType.Serial, BrandBase.GetChildNodes(
		//        BrandBase.GetChildNodes(brandTree)));
		//}

		///// <summary>
		///// 生成二手车的数据块
		///// </summary>
		///// <param name="brandType">名牌类型</param>
		///// <param name="brands">品牌信息</param>
		//private void GenerateUsedCarHtmlChunksByBrandType(BrandType brandType,
		//    IEnumerable<BrandBase> brands)
		//{
		//    string brandTypeString = brandType.ToString();
		//    DirectoryInfo outputDirectory = IOUtils.CreateDirecotry(
		//        string.Format(_outputDicrectoryFormat, brandTypeString));
		//    foreach (BrandBase brand in brands)
		//    {
		//        string xmlDirectoryPath = string.Format(_xmlDicrectoryFormat, brandTypeString);
		//        string fileName = string.Format("{0}.xml", brand.Id);
		//        string xmlFilePath = Path.Combine(xmlDirectoryPath, fileName);
		//        GenerateUsedCarHtmlChunk(outputDirectory.FullName, xmlFilePath, brand);
		//    }
		//}

		///// <summary>
		///// 根据xml文件和品牌信息生成二手车的数据块
		///// </summary>
		///// <param name="outputDirectoryPath"></param>
		///// <param name="xmlFilePath"></param>
		///// <param name="brand"></param>
		//private void GenerateUsedCarHtmlChunk(string outputDirectoryPath, string xmlFilePath, BrandBase brand)
		//{
		//    string htmlFileName = string.Format("{0}.htm", Path.GetFileNameWithoutExtension(xmlFilePath));
		//    string htmlFilePath = Path.Combine(outputDirectoryPath, htmlFileName);

		//    //如果没有数据文件，不生成html文件，如果存在以前生成的文件，删除
		//    if (!File.Exists(xmlFilePath))
		//    {
		//        if (File.Exists(htmlFilePath))
		//        {
		//            File.Delete(htmlFilePath);
		//        }
		//        return;
		//    }

		//    //要注入模板的参数
		//    IDictionary<string, object> currentContext = new Dictionary<string, object>();
		//    currentContext.Add("brand", brand);
		//    currentContext.Add("usedCarInfos", GetUsedCarInfos(xmlFilePath));

		//    using (FileStream fileStream = new FileStream(htmlFilePath,
		//        FileMode.Create, FileAccess.Write))
		//    {
		//        StreamWriter writer = new StreamWriter(fileStream);
		//        _parser.ParseTemplate(USED_CAR_CHUNK_TEMPLATE, currentContext, writer);
		//        writer.Flush();
		//    }
		//}

		///// <summary>
		///// 从xml文件中读取二手车信息
		///// </summary>
		///// <param name="xmlFilePath">xml文件路径</param>
		///// <returns></returns>
		//private UsedCarInfo[] GetUsedCarInfos(string xmlFilePath)
		//{
		//    XmlDocument askXmlDocument = new XmlDocument();
		//    askXmlDocument.Load(xmlFilePath);
		//    return UsedCarInfo.ConvertToUsedCarInfos(askXmlDocument.DocumentElement);
		//}
    }
}
