using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Common.Utils;
using BitAuto.CarDataUpdate.Common.Model;
using System.Data;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.DataProcesser.cn.ucar.api;
using BitAuto.CarDataUpdate.DataProcesser.cn.ucar.api.CarPrice;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class UsedCarDataService
    {
        public UsedCarDataService()
        {
            _dataDirectory = CommonData.CommonSettings.SavePath;
            _outputDicrectoryFormat = Path.Combine(_dataDirectory, OUTPUT_DIRECTORY_FORMAT);
            _UsedCarDataCount = DEFAULT_USED_CAR_DATA_COUNT;
        }

        /// <summary>
        /// xml文件输出目录的格式化字符串
        /// </summary>
        public const string OUTPUT_DIRECTORY_FORMAT = @"UsedCarInfo\{0}_Right\Xml\";

        /// <summary>
        /// 缺省获取的二手车数据条数
        /// </summary>
        const int DEFAULT_USED_CAR_DATA_COUNT = 5;

        /// <summary>
        /// 数据存储目录
        /// </summary>
        private string _dataDirectory;

        /// <summary>
        /// 获取的答疑数据条数
        /// </summary>
        int _UsedCarDataCount;

        /// <summary>
        /// 答疑块生成目录全路径的格式化字符串
        /// </summary>
        private string _outputDicrectoryFormat;

        private CarSourceForBitAuto _usedCarDataWebService = null;
        private CarSourceForBitAuto GetUsedCarDataWebService()
        {
            if (_usedCarDataWebService == null)
            {
                _usedCarDataWebService = new CarSourceForBitAuto();
            }
            return _usedCarDataWebService;
        }

		///// <summary>
		///// 获取二手车数据的xml
		///// </summary>
		///// <param name="brandId"></param>
		///// <param name="count"></param>
		///// <returns></returns>
		//private XmlNode GetUsedCarData(int brandId, int count)
		//{
		//    CarSourceForBitAuto service = GetUsedCarDataWebService();
		//    return service.GetCarSourceList(brandId, 0, count);
		//}


		//private List<BrandBase> GetBrandTree()
		//{
		//    BrandDataService brandDataService = new BrandDataService(_dataDirectory);
		//    return brandDataService.GetBrandTree();
		//}

        /// <summary>
        /// 更新二手车数据
        /// </summary>
        public void UpdateUsedCarData()
        {
			//List<BrandBase> brandTree = GetBrandTree();
			//DirectoryInfo outputDirectory = null;

			////生成子品牌
			//outputDirectory = IOUtils.CreateDirecotry(
			//    string.Format(_outputDicrectoryFormat, BrandType.Serial.ToString()));
			//foreach (BrandBase serialBrand in BrandBase.GetChildNodes(
			//    BrandBase.GetChildNodes(brandTree)))
			//{
			//    UpdateXmlFileOfUsedCarData(BrandType.Serial, serialBrand,
			//        outputDirectory.FullName);
			//}

            // add by chengl Oct.10.2011
            UpdateUsedCarPriceData();
			// add by chengl Aug.16.2013
			UpdateUsedSerialPriceData();
        }


		///// <summary>
		///// 更新二手车数据的xml文件
		///// </summary>
		///// <param name="brandType"></param>
		///// <param name="brand"></param>
		///// <param name="outputFullDirectory"></param>
		//private void UpdateXmlFileOfUsedCarData(BrandType brandType, BrandBase brand,
		//    string outputFullDirectory)
		//{
		//    try
		//    {
		//        XmlNode dataSetNode = GetUsedCarData(brand.Id, _UsedCarDataCount);
		//        string filePath = Path.Combine(outputFullDirectory,
		//                string.Format("{0}.xml", brand.Id));

		//        //如果没有数据，不生成xml文件，如果存在以前生成的文件，删除
		//        if (dataSetNode.SelectNodes("./item").Count == 0)
		//        {
		//            if (File.Exists(filePath))
		//            {
		//                File.Delete(filePath);
		//            }
		//            return;
		//        }

		//        using (XmlWriter writer = new XmlTextWriter(filePath, Encoding.UTF8))
		//        {
		//            dataSetNode.WriteTo(writer);
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        Log.WriteErrorLog(ex.ToString());
		//    }
		//}

        /// <summary>
        /// 更新二手车车型报价区间数据源(韩剑)
        /// http://api.ucar.cn/CarBasicIno/ForXml/CarPrice.asmx?op=GetAllPriceRange
        /// add by chengl Oct.10.2011
        /// </summary>
        private void UpdateUsedCarPriceData()
        {
            try
            {
                CarPrice cp = new CarPrice();
                DataTable dt = new DataTable();
                dt = cp.GetAllPriceRange();
                string filePath = Path.Combine(_dataDirectory, "UsedCarInfo\\AllUCarPrice.Xml");
                using (XmlWriter writer = new XmlTextWriter(filePath, Encoding.UTF8))
                {
                    dt.WriteXml(writer);
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

		/// <summary>
		/// 子品牌二手车价格区间接口
		/// </summary>
		private void UpdateUsedSerialPriceData()
		{
			try
			{
				Common.Log.WriteLog("更新子品牌二手车价格区间接口开始。");
				string filePath = Path.Combine(_dataDirectory, "UsedCarInfo\\AllSerialPrice.Xml");
				XmlDocument doc = new XmlDocument();
				doc.Load(CommonData.CommonSettings.UcarSerialPrice);
				CommonFunction.SaveXMLDocument(doc, filePath);
				Common.Log.WriteLog("更新子品牌二手车价格区间接口结束。");
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog(ex.ToString());
			}
		}

    }
}
