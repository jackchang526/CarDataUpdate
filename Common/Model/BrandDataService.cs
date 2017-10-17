using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using BitAuto.Utils;

namespace BitAuto.CarDataUpdate.Common.Model
{
    /// <summary>
    /// 品牌数据服务类
    /// </summary>
    public class BrandDataService
    {
      
        #region Constructor

        public BrandDataService(string dataDirectory)
            : this(dataDirectory, string.Empty)
        { }

        public BrandDataService(string dataDirectory, string connectionString)
        {
            this._dataDirectory = dataDirectory;
            this._connectionString = connectionString;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// 数据存储目录
        /// </summary>
        private string _dataDirectory;

        /// <summary>
        /// 数据库连接字符串名称
        /// </summary>
        private string _connectionString;

        #endregion

        #region Private Methods

        /// <summary>
        /// 将xml节点转换成品牌信息
        /// </summary>
        /// <typeparam name="T">品牌类型</typeparam>
        /// <param name="brandNode">品牌的xml节点</param>
        /// <returns>品牌信息</returns>
        private T ConvertToBrand<T>(XmlElement brandNode)
            where T : BrandBase, new()
        {
            return new T()
            {
                Id = Convert.ToInt32(brandNode.GetAttribute("ID")),
                Name = brandNode.GetAttribute("Name").Trim(),
                AllSpell = brandNode.GetAttribute("AllSpell").Trim()
            };
        }

        /// <summary>
        /// 将xml节点转换成品牌信息
        /// </summary>
        /// <typeparam name="T">品牌类型</typeparam>
        /// <param name="brandNode">品牌的xml节点</param>
        /// <returns>品牌信息</returns>
        private T ConvertToBrand<T>(XElement brandNode)
            where T : BrandBase, new()
        {
            return new T()
            {
                Id = Convert.ToInt32(brandNode.Attribute("ID").Value),
                Name = brandNode.Attribute("Name").Value.Trim(),
                AllSpell = brandNode.Attribute("AllSpell").Value.Trim()
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取主品牌、品牌、子品牌的三级树型结构
        /// </summary>
        /// <returns></returns>
        public List<BrandBase> GetBrandTree()
        {
            List<BrandBase> brandTree = new List<BrandBase>();
            //读取xml文件
            string autoDataFile = Path.Combine(this._dataDirectory, "AllAutoData.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(autoDataFile);
            //构建三层的树型结构
            foreach (XmlElement masterNode in xmlDoc.SelectNodes("/Params/MasterBrand"))
            {
                MasterBrand currentMasterBrand = ConvertToBrand<MasterBrand>(masterNode);
                currentMasterBrand.SEOName = masterNode.GetAttribute("MasterSEOName");
                brandTree.Add(currentMasterBrand);

                foreach (XmlElement brandNode in masterNode.SelectNodes("./Brand"))
                {
                    Brand currentBrand = ConvertToBrand<Brand>(brandNode);
                    currentBrand.SEOName = brandNode.GetAttribute("BrandSEOName");
                    currentBrand.ParentNode = currentMasterBrand;
                    currentMasterBrand.ChildNodes.Add(currentBrand);

                    foreach (XmlElement serialNode in brandNode.SelectNodes("./Serial"))
                    {
                        SerialBrand currentSerialBrand = ConvertToBrand<SerialBrand>(serialNode);
                        currentSerialBrand.SEOName = serialNode.GetAttribute("SerialSEOName");
                        currentSerialBrand.ParentNode = currentBrand;
                        currentBrand.ChildNodes.Add(currentSerialBrand);
                    }
                }
            }
            return brandTree;
        }

        public List<BrandBase> GetBrandTree(BrandType brandType, int objId)
        {
            string eleName = string.Empty;
            switch (brandType)
            {
                case BrandType.Serial:
                    eleName = "Serial";
                    break;
                case BrandType.Masterbrand:
                    eleName = "MasterBrand";
                    break;
                case BrandType.Brand:
                    eleName = "Brand";
                    break;
            }

            return GetBrandTree(brandType, eleName, objId);
        }
        public List<BrandBase> GetBrandTree(BrandType brandType, string elementName, int objId)
        {
            List<BrandBase> brandTree = new List<BrandBase>();

            XDocument doc = XDocument.Load(Path.Combine(this._dataDirectory, "AllAutoData.xml"));

            foreach (var curEle in doc.Descendants(elementName))
            {
                if (ConvertHelper.GetInteger(curEle.Attribute("ID").Value) == objId)
                {
                    XElement tempEle = brandType == BrandType.Masterbrand? curEle : brandType == BrandType.Brand?curEle.Parent:curEle.Parent.Parent;

                    MasterBrand currentMasterBrand = ConvertToBrand<MasterBrand>(tempEle);
                    currentMasterBrand.SEOName = tempEle.Attribute("MasterSEOName").Value;

                    brandTree.Add(currentMasterBrand);

                    if (brandType == BrandType.Masterbrand)
                    {
                        break;
                    }

                    tempEle = brandType == BrandType.Brand ? curEle : curEle.Parent;
                    Brand currentBrand = ConvertToBrand<Brand>(tempEle);
                    currentBrand.SEOName = tempEle.Attribute("BrandSEOName").Value;
                    currentBrand.ParentNode = currentMasterBrand;
                    currentMasterBrand.ChildNodes.Add(currentBrand);

                    if (brandType == BrandType.Brand)
                    {
                        break;
                    }

                    SerialBrand currentSerialBrand = ConvertToBrand<SerialBrand>(curEle);
                    currentSerialBrand.SEOName = curEle.Attribute("SerialSEOName").Value;
                    currentSerialBrand.ParentNode = currentBrand;
                    currentBrand.ChildNodes.Add(currentSerialBrand);

                    break;
                }
            }
            return brandTree;
        }
        #endregion
    }
}
