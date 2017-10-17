using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.Utils;
using System.Data;
using BitAuto.CarDataUpdate.Config;

namespace BitAuto.CarDataUpdate.Common
{
    public static class CityInitData
    {
        private static CommonSettings m_config;
        private static string _RootPath = string.Empty;
        private static string m_CityShortTitleRequestUrl = "http://api.admin.bitauto.com/api/common/cityvalueset.aspx?type=citymapping";
        /// <summary>
        /// 省份对应区域
        /// </summary>
        private static string m_ProvinceRelationAreaFile = string.Empty;
        /// <summary>
        /// 省份的保存地址
        /// </summary>
        private static string m_ProvinceSavePath = string.Empty;
        /// <summary>
        /// 省份与城市的原始关系文件
        /// </summary>
        private static string m_ProvinceRelationCityPath = string.Empty;
        /// <summary>
        /// 350个城市的保存地址
        /// </summary>
        private static string m_CitySavePath_350_City = string.Empty;

        static CityInitData()
        {
            m_config = Common.CommonData.CommonSettings;
            _RootPath = Path.Combine(m_config.SavePath, "City");
            m_CitySavePath_350_City = Path.Combine(_RootPath, "350city.xml");
            m_ProvinceSavePath = Path.Combine(_RootPath, "31Province.xml");
            m_ProvinceRelationCityPath = Path.Combine(_RootPath, "InitProvinceRelationCity.xml");
            m_ProvinceRelationAreaFile = Path.Combine(_RootPath, "provincerelationareafile.xml");
        }

        /// <summary>
        /// 得到350城市的ID
        /// </summary>
        /// <returns></returns>
        public static List<int> Get350CityIdList()
        {
            if (!File.Exists(m_CitySavePath_350_City)) return null;

            try
            {
                XmlDocument xmlDoc = CommonFunction.GetXmlDocument(m_CitySavePath_350_City);

                XmlNodeList xNodeList = xmlDoc.SelectNodes("CityValueSet/CityItem");
                if (xNodeList == null || xNodeList.Count < 1) return null;

                List<int> cityIdList = new List<int>();

                foreach (XmlNode xNode in xNodeList)
                {
                    int cityId = ConvertHelper.GetInteger(xNode.SelectSingleNode("CityId").InnerText);
                    if (cityId > 0)
                        cityIdList.Add(cityId);
                }

                return cityIdList;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 得到省份的链接
        /// </summary>
        /// <returns></returns>
        public static List<int> GetProvinceIdList()
        {
            if (!File.Exists(m_ProvinceSavePath)) return null;

            try
            {
                XmlDocument xmlDoc = CommonFunction.GetXmlDocument(m_ProvinceSavePath);

                XmlNodeList xNodeList = xmlDoc.SelectNodes("CityValueSet/CityItem");
                if (xNodeList == null || xNodeList.Count < 1) return null;

                List<int> cityIdList = new List<int>();

                foreach (XmlNode xNode in xNodeList)
                {
                    int cityId = ConvertHelper.GetInteger(xNode.SelectSingleNode("CityId").InnerText);
                    cityIdList.Add(cityId);
                }

                return cityIdList;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取城市的相应中心城市
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, City> GetCityTo30City()
        {
            Dictionary<int, City> to30Dic = new Dictionary<int, City>();
            if (to30Dic != null)
            {
                DataSet ds = new DataSet();
                ds.ReadXml(m_CityShortTitleRequestUrl);
                Dictionary<int, City> tmpDic = new Dictionary<int, City>();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int cityId = ConvertHelper.GetInteger(row["CityId"]);
                    int centerId = ConvertHelper.GetInteger(row["Memo1"]);
                    if (cityId == centerId)
                    {
                        City city = new City();
                        string cityName = row["CityName"].ToString().Trim();
                        string citySpell = row["EngName"].ToString().Trim();
                        int cityLevel = ConvertHelper.GetInteger(row["Level"]);
                        city.CityEName = citySpell;
                        city.CityName = cityName;
                        city.CityId = cityId;
                        city.CityLevel = cityLevel;
                        tmpDic[cityId] = city;
                    }
                }
                //生成对应关系
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int cityId = ConvertHelper.GetInteger(row["CityId"]);
                    int centerId = ConvertHelper.GetInteger(row["Memo1"]);
                    if (centerId != 0 && cityId != centerId && tmpDic.ContainsKey(centerId))
                    {
                        to30Dic[cityId] = tmpDic[centerId];
                    }
                }
            }
            return to30Dic;
        }


        /// <summary>
        /// 得到91个城市的城市列表,原来是91个城市，现在这个数量不一定，这个方法获取的是重点卫星城的字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, City> Get91CityDic()
        {
            XmlDocument cityDoc = new XmlDocument();
            Dictionary<int, City> cityDic = new Dictionary<int, City>();
            try
            {
                string cityFile = Path.Combine(m_config.SavePath, "city.xml");
                cityDoc = CommonFunction.GetXmlDocument(cityFile);

                XmlNodeList cityList = cityDoc.SelectNodes("/CityValueSet/CityItem[Memo1=1]");
                //城市列表
                foreach (XmlElement cityNode in cityList)
                {
                    int cityId = Convert.ToInt32(cityNode.SelectSingleNode("CityId").InnerText);
                    string cityName = cityNode.SelectSingleNode("CityName").InnerText;
                    int cityLevel = Convert.ToInt32(cityNode.SelectSingleNode("Level").InnerText);
                    cityDic[cityId] = new City(cityId, cityName);
                    cityDic[cityId].CityLevel = cityLevel;
                }
            }
            catch (Exception ex)
            {
            }

            return cityDic;
        }

        /// <summary>
        /// 得到区域和省市的对应关系
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAreaAndProvinceRelation()
        {
            //如果对应的文件不存在
            if (!File.Exists(m_ProvinceRelationAreaFile)) return null;

            try
            {
                //加载关联文件
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc = CommonFunction.GetXmlDocument(m_ProvinceRelationAreaFile);
                if (xmlDoc == null) return null;
                //结点列表
                XmlNodeList xNodeList = xmlDoc.SelectNodes("config/areaList/area");
                if (xNodeList == null || xNodeList.Count < 1) return null;
                int counter = 0;
                Dictionary<int, string> relationList = new Dictionary<int, string>();
                foreach (XmlElement entity in xNodeList)
                {
                    string provinceIdString = entity.SelectSingleNode("provinceId").InnerText;
                    if (string.IsNullOrEmpty(provinceIdString)) continue;
                    string areaName = entity.GetAttribute("name");
                    string value = areaName + "|" + counter.ToString();
                    //匹配省份ID
                    foreach (string id in provinceIdString.Split(','))
                    {
                        if (relationList.ContainsKey(ConvertHelper.GetInteger(id))) continue;

                        relationList.Add(ConvertHelper.GetInteger(id), value);
                    }
                    counter++;
                }

                return relationList;

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 得到对应关系
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetRelationMap()
        {
            if (!File.Exists(m_ProvinceRelationCityPath)) return null;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc = CommonFunction.GetXmlDocument(m_ProvinceRelationCityPath);

                if (xmlDoc == null) return null;
                XmlNodeList xNodeList = xmlDoc.SelectNodes("CityValueSet/CityItem");
                if (xNodeList == null || xNodeList.Count < 1) return null;
                Dictionary<int, int> relationMap = new Dictionary<int, int>();
                //循环匹配省份
                foreach (XmlNode xNode in xNodeList)
                {
                    int isProv = ConvertHelper.GetInteger(xNode.SelectSingleNode("Memo2").InnerText);
                    if (isProv == 0) continue;
                    int cityId = ConvertHelper.GetInteger(xNode.SelectSingleNode("Memo1").InnerText);
                    int provinceId =GetProvinceId(xNode, xmlDoc);
                    if (provinceId == 0 || relationMap.ContainsKey(cityId)) continue;
                    //添加对照关系
                    relationMap.Add(cityId, provinceId);
                }


                return relationMap;
            }
            catch
            {
                return null;
            }
        }

        #region 原Common.cs文件方法

        /// <summary>
        /// 得到城市字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> GetCityIdDic()
        {
            XmlDocument cityDoc = new XmlDocument();
            string cityFile = Path.Combine(Common.CommonData.CommonSettings.SavePath, "city.xml");
            cityDoc.Load(cityFile);

            XmlNodeList cityList = cityDoc.SelectNodes("/CityValueSet/CityItem");
            Dictionary<int, int> cityDic = new Dictionary<int, int>();
            //城市列表
            foreach (XmlElement cityNode in cityList)
            {
                int cityId = Convert.ToInt32(cityNode.SelectSingleNode("CityId").InnerText);
                if (cityDic.ContainsKey(cityId)) continue;
                cityDic.Add(cityId, 0);
            }
            return cityDic;
        }
        /// <summary>
        /// 得到城市字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, City> GetCityDic()
        {
            try
            {
                XmlDocument cityDoc = new XmlDocument();
                string cityFile = Path.Combine(Common.CommonData.CommonSettings.SavePath, "city.xml");
                cityDoc.Load(cityFile);

                XmlNodeList cityList = cityDoc.SelectNodes("/CityValueSet/CityItem");
                Dictionary<int, City> cityDic = new Dictionary<int, City>();
                //城市列表
                foreach (XmlElement cityNode in cityList)
                {
                    int cityId = Convert.ToInt32(cityNode.SelectSingleNode("CityId").InnerText);
                    if (cityDic.ContainsKey(cityId)) continue;

                    City cityEntity = new City();
                    cityEntity.CityId = cityId;
                    cityEntity.CityName = cityNode.SelectSingleNode("CityName").InnerText;
                    cityEntity.CityEName = cityNode.SelectSingleNode("EngName").InnerText;
                    cityEntity.CityLevel = ConvertHelper.GetInteger(cityNode.SelectSingleNode("Level").InnerText);
                    cityDic.Add(cityId, cityEntity);
                }
                return cityDic;
            }
            catch
            {
                return null;
            }
        } 
        #endregion

        /// <summary>
        /// 得到省份ID
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="xmlDoc"></param>
        private static int GetProvinceId(XmlNode xNode, XmlDocument xmlDoc)
        {
            int parentId = ConvertHelper.GetInteger(xNode.SelectSingleNode("ParentId").InnerText);
            if (parentId == 0) return 0;
            XmlNode parentNode = xmlDoc.SelectSingleNode("CityValueSet/CityItem[CityId=" + parentId + "]");
            if (parentNode == null) return 0;
            int isProv = ConvertHelper.GetInteger(parentNode.SelectSingleNode("Memo2").InnerText);
            int cityId = ConvertHelper.GetInteger(parentNode.SelectSingleNode("Memo1").InnerText);
            if (isProv == 0) return cityId;

            return GetProvinceId(parentNode, xmlDoc);
        }
    }
}
