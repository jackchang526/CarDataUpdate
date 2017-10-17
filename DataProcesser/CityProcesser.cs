using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using BitAuto.Utils;
using BitAuto.Utils.Data;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	public class CityProcesser
	{
        private string _RootPath = string.Empty;
        private string m_ProvinceListRequestUrl = "http://api.admin.bitauto.com/api/Common/CityValueSet.aspx?type=province";
        private string m_ProvinceRelationCityRequestUrl = "http://api.admin.bitauto.com/api/common/cityvalueset.aspx?type=administrativearea";
        private string m_CityShortTitleRequestUrl = "http://api.admin.bitauto.com/api/common/cityvalueset.aspx?type=citymapping";
        private string m_ProvinceRelationAreaUrl = "http://api.admin.bitauto.com/api/Common/bigarea.xml";
        public event LogHandler Log;

        /// <summary>
        /// 350�����еı����ַ
        /// </summary>
        private string m_CitySavePath_350_City = string.Empty;
        /// <summary>
        ///Class1.cs ʡ�ݵı����ַ
        /// </summary>
        private string m_ProvinceSavePath = string.Empty;
        /// <summary>
        /// ���к�ʡ�ֵ���������
        /// </summary>
        private string m_CityAndProvinceNewsNumberPath = string.Empty;
        /// <summary>
        /// ʡ������е�ԭʼ��ϵ�ļ�
        /// </summary>
        private string m_ProvinceRelationCityPath = string.Empty;
        /// <summary>
        /// ʡ�еĹ�ϵͼ
        /// </summary>
        private string m_NeedRelationProvinceAndCity = string.Empty;
        /// <summary>
        /// ���м��XML
        /// </summary>
        private string m_CityShortTitleFile = string.Empty;
        /// <summary>
        /// ʡ�ݶ�Ӧ����
        /// </summary>
        private string m_ProvinceRelationAreaFile = string.Empty;
        /// <summary>
        /// 91�����е��ļ��б�
        /// </summary>
        private string m_NinetyCityFile = string.Empty;

		
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="initFile"></param>
        public CityProcesser(int initFile)
        {
            _RootPath = Path.Combine(CommonData.CommonSettings.SavePath, "City");
            m_CitySavePath_350_City = Path.Combine(_RootPath, "350city.xml");
            m_ProvinceSavePath = Path.Combine(_RootPath, "31Province.xml");
            m_ProvinceRelationCityPath = Path.Combine(_RootPath, "InitProvinceRelationCity.xml");
            m_NeedRelationProvinceAndCity = Path.Combine(_RootPath, "needrelationmap.xml");
            m_CityAndProvinceNewsNumberPath = Path.Combine(_RootPath, "cityandprovincenewsnumber.xml");
            m_CityShortTitleFile = Path.Combine(_RootPath, "cityshortTitleRequestUrl.xml");
            m_ProvinceRelationAreaFile = Path.Combine(_RootPath, "provincerelationareafile.xml");
            m_NinetyCityFile = Path.Combine(_RootPath, "91city.xml");
        }

		
        /// <summary>
        /// ����350������
        /// </summary>
        public void Save_350_City()
        {
            OnLog("     start exec Save_350_City......", true);
			CommonFunction.SaveXMLDocument(CommonData.CommonSettings.CityValueSetUrl, m_CitySavePath_350_City);
            OnLog("     end exec Save_350_City!", true);
        }
        /// <summary>
        /// ����ʡ
        /// </summary>
        public void Save_Province()
        {
            OnLog("     start exec Save_Province......", true);
            CommonFunction.SaveXMLDocument(m_ProvinceListRequestUrl, m_ProvinceSavePath);
            OnLog("     end exec Save_Province!", true);
        }
        /// <summary>
        /// ����ԭʼ��ʡ�ж��չ�ϵ����
        /// </summary>
        public void Save_InitProvinceRelationCity()
        {
            OnLog("     start exec Save_InitProvinceRelationCity......", true);
            CommonFunction.SaveXMLDocument(m_ProvinceRelationCityRequestUrl, m_ProvinceRelationCityPath);
            OnLog("     end exec Save_InitProvinceRelationCity!", true);
        }
        /// <summary>
        /// ������еļ��
        /// </summary>
        public void Save_CityShortTitle()
        {
            OnLog("     start exec Save_CityShortTitle......", true);
            CommonFunction.SaveXMLDocument(m_CityShortTitleRequestUrl, m_CityShortTitleFile);
            OnLog("     end exec Save_CityShortTitle!", true);
        }
        /// <summary>
        /// ʡ�ݹ���������ļ�
        /// </summary>
        public void Save_ProvinceRelationAreaFile()
        {
            CommonFunction.SaveXMLDocument(m_ProvinceRelationAreaUrl, m_ProvinceRelationAreaFile);
        }
       
        /// <summary>
        /// ���������������
        /// </summary>
        /// <param name="id">����ID</param>
        /// <param name="type">��������</param>
        /// <param name="number">��������</param>
        public void SaveCityNewsNumber(int id, string type, int number,string elementtype)
        {
            string path = string.Empty;//��ѯ·��
            string createElementName = string.Empty;
            if (!File.Exists(m_NeedRelationProvinceAndCity) && !File.Exists(m_CityAndProvinceNewsNumberPath)) return;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                if (!File.Exists(m_CityAndProvinceNewsNumberPath))
                    xmlDoc = CommonFunction.GetXmlDocument(m_NeedRelationProvinceAndCity);
                else
                    xmlDoc = CommonFunction.GetXmlDocument(m_CityAndProvinceNewsNumberPath);
                if (xmlDoc == null) return;
                //�����������Ϊ����
                if (type == "city")
                {
                    path = "root/Province/City[@ID={0}]";
                }
                else
                {
                    path = "root/Province[@ID={0}]";
                }
                XmlNode xNode = xmlDoc.SelectSingleNode(string.Format(path, id.ToString()));
                if (xNode == null) return;
                ((XmlElement)xNode).SetAttribute(elementtype, number.ToString());

                CommonFunction.SaveXMLDocument(xmlDoc, m_CityAndProvinceNewsNumberPath);
            }
            catch 
            {
                return;
            }
        }
        /// <summary>
        /// �õ�ʡ�����е�XML�ṹ
        /// </summary>
        /// <returns></returns>
        public XmlDocument GetProvinceContainsCityIdXmlStruct()
        {
            if (!File.Exists(m_NeedRelationProvinceAndCity)) return null;

            try
            {
                XmlDocument resultXml = new XmlDocument();
                resultXml = CommonFunction.GetXmlDocument(m_NeedRelationProvinceAndCity);
                return resultXml;
            }
            catch
            {
                return null;
            }
        }
       
        /// <summary>
        /// ͬ�������ϵ������
        /// </summary>
        public void SyncRelationToNumber()
        {
            //��������ļ�������
            if (!File.Exists(m_NeedRelationProvinceAndCity)
                || !File.Exists(m_CityAndProvinceNewsNumberPath))
                return;

            try
            {
                OnLog("     start exec SyncRelationToNumber......", true);
                XmlDocument relationMap = new XmlDocument();
                relationMap = CommonFunction.GetXmlDocument(m_NeedRelationProvinceAndCity);
                XmlDocument numberXml = new XmlDocument();
                numberXml = CommonFunction.GetXmlDocument(m_CityAndProvinceNewsNumberPath);

                if (relationMap == null || numberXml == null) return;

                //�õ���ϵ�б�ĸ����
                XmlNode rootNode = relationMap.SelectSingleNode("root");
                if (rootNode == null || rootNode.ChildNodes.Count < 1) return;
                //ѭ��ʡ��
                foreach (XmlElement provEntity in rootNode.ChildNodes)
                {
                    XmlNode numProvEntity = numberXml.SelectSingleNode("root/Province[@ID=" + provEntity.GetAttribute("ID") + "]");
                    //����������У����������ʡ
                    if (numProvEntity == null)
                    {
                        XmlElement newProvEntity = numberXml.CreateElement("Province");
                        foreach (XmlAttribute attr in provEntity.Attributes)
                        {
                            newProvEntity.SetAttribute(attr.Name, attr.Value);
                        }
                        numberXml.SelectSingleNode("root").AppendChild(newProvEntity);
                    }
                    else
                    {
                        foreach (XmlAttribute attr in provEntity.Attributes)
                        {
                            if (((XmlElement)numProvEntity).GetAttribute(attr.Name) != attr.Value)
                            {
                                ((XmlElement)numProvEntity).SetAttribute(attr.Name, attr.Value); 
                            }
                        }
                    }
                    if (provEntity.ChildNodes.Count < 1) continue;
                    //ѭ�����е��б�
                    foreach (XmlElement cityEntity in provEntity.ChildNodes)
                    {
                        //���ж������еĳ��н�㣬�Ƿ�����ͬ�Ĳ�νṹ
                        XmlNode numCityEntity = numberXml.SelectSingleNode("root/Province/City[@ID=" + cityEntity.GetAttribute("ID") + "]");
                        if (numCityEntity == null)
                        {
                            XmlElement newCityEntity = numberXml.CreateElement("City");
                            foreach (XmlAttribute attr in cityEntity.Attributes)
                            {
                                newCityEntity.SetAttribute(attr.Name, attr.Value);
                            }
                            //��û�еĳ��н����ӵ���Ӧ��ʡ����
                            numberXml.SelectSingleNode("root/Province[@ID=" + provEntity.GetAttribute("ID") + "]").AppendChild(newCityEntity);
                            continue;
                        }
                        //������еĲ㼶��ϵ�����仯
                        if (((XmlElement)numCityEntity.ParentNode).GetAttribute("ID") != provEntity.GetAttribute("ID"))
                        {
                            XmlNode parentNode = numCityEntity.ParentNode;
                            parentNode.RemoveChild(numCityEntity);

                            numberXml.SelectSingleNode("root/Province[@ID=" + provEntity.GetAttribute("ID") + "]").AppendChild(numCityEntity);
                        }
                        //ƥ����е�����
                        foreach (XmlAttribute attr in cityEntity.Attributes)
                        {
                            if (((XmlElement)numCityEntity).GetAttribute(attr.Name) != attr.Value)
                            {
                                ((XmlElement)numCityEntity).SetAttribute(attr.Name, attr.Value);
                            }
                        }

                    }

                }

                CommonFunction.SaveXMLDocument(numberXml, m_CityAndProvinceNewsNumberPath);
                OnLog("     end exec SyncRelationToNumber!", true);
            }
            catch(Exception ex)
            {
                OnLog(ex.Message,true);
            }
        }
        /// <summary>
        /// �õ�����
        /// </summary>
        public void GetContent()
        {
            //����ҳ���¼
            Save_Province();
            Save_350_City();
            Save_CityShortTitle();
            Save_InitProvinceRelationCity();
			// del by chengl Apr.24.2014 �ӿڲ����ڣ������ļ�δʹ�� data/city/provincerelationareafile.xml
            // Save_ProvinceRelationAreaFile();
            GetNeedRelation();
            //ͬ����ϵ
            SyncRelationToNumber();
        }
        /// <summary>
        /// �õ�ʡ��ID
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="xmlDoc"></param>
        private int GetProvinceId(XmlNode xNode, XmlDocument xmlDoc)
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

        /// <summary>
        /// �õ���Ҫ�ĳ�����ʡ�ݵĶ�Ӧ��ϵ��
        /// </summary>
        public void GetNeedRelation()
        {
            OnLog("     start exec GetNeedRelation......",true);
            Dictionary<int, int> RelationMap = CityInitData.GetRelationMap();//�õ���Ӧ��ϵ
            if (!File.Exists(m_ProvinceSavePath)
                || !File.Exists(m_CitySavePath_350_City)
                || !File.Exists(m_CityShortTitleFile)
                || RelationMap == null || RelationMap.Count < 1) return;
            try
            {
                XmlDocument provinceXml = new XmlDocument();
                provinceXml = CommonFunction.GetXmlDocument(m_ProvinceSavePath);
                XmlDocument cityXml = new XmlDocument();
                cityXml = CommonFunction.GetXmlDocument(m_CitySavePath_350_City);
                XmlDocument cityShortNameXml = new XmlDocument();
                cityShortNameXml = CommonFunction.GetXmlDocument(m_CityShortTitleFile);
                //�õ�������ʡ�ݵĶ�Ӧ��ϵ
                Dictionary<int, string> areaRelationProvince = new Dictionary<int, string>();
                areaRelationProvince = CityInitData.GetAreaAndProvinceRelation();

                if (provinceXml == null || cityXml == null || cityShortNameXml == null) return;
                //���XML�ĵ�
                XmlDocument resultXml = new XmlDocument();
                XmlNode root = resultXml.CreateElement("root");
                resultXml.AppendChild(root);
                XmlDeclaration xmlDeclar = resultXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                resultXml.InsertBefore(xmlDeclar, root);
                //ѡ���ȫ��
                XmlNode quanguoNode = provinceXml.SelectSingleNode("CityValueSet/CityItem[CityId=0]");
                if (quanguoNode != null)
                {
                    XmlNode provNode = resultXml.CreateElement("Province");
                    XmlElement provElem = (XmlElement)provNode;
                    string provname = quanguoNode.SelectSingleNode("CityName").InnerText;
                    string provengname = quanguoNode.SelectSingleNode("EngName").InnerText;
                    provElem.SetAttribute("ID", "0");
                    provElem.SetAttribute("Name", provname);
                    provElem.SetAttribute("EngName", provengname);
                    root.AppendChild(provNode);
                }

                //����Ԫ��
                XmlNodeList cityNodeList = cityXml.SelectNodes("CityValueSet/CityItem");
                if (cityNodeList == null || cityNodeList.Count < 1) return;
                //ƥ�������Ϣ
                foreach (XmlElement cityElem in cityNodeList)
                {
                    int cityId = ConvertHelper.GetInteger(cityElem.SelectSingleNode("CityId").InnerText);
                    if (!RelationMap.ContainsKey(cityId)) continue;
                    int provinceId = RelationMap[cityId];
                    XmlNode initProvNode = provinceXml.SelectSingleNode("CityValueSet/CityItem[CityId=" + provinceId + "]");
                    if (initProvNode == null) continue;
                    XmlNode provNode = resultXml.SelectSingleNode("root/Province[@ID=" + provinceId + "]");
                    //���ʡ���㼶
                    if (provNode == null)
                    {
                        provNode = resultXml.CreateElement("Province");
                        XmlElement provElem = (XmlElement)provNode;
                        string provname = initProvNode.SelectSingleNode("CityName").InnerText;
                        string provengname = initProvNode.SelectSingleNode("EngName").InnerText;
                        string areaAttribute = (areaRelationProvince == null || !areaRelationProvince.ContainsKey(provinceId))
                                                ? "" : areaRelationProvince[provinceId];
                        provElem.SetAttribute("ID", provinceId.ToString());
                        provElem.SetAttribute("Name", provname);
                        provElem.SetAttribute("EngName", provengname);
                        if (!string.IsNullOrEmpty(areaAttribute))
                        {
                            string[] areaAttributeArray = areaAttribute.Split('|');
                            provElem.SetAttribute("Area", areaAttributeArray[0]);
                            provElem.SetAttribute("AreaOrder", areaAttributeArray[1]);
                        }
                        root.AppendChild(provNode);
                    }

                    string name = cityElem.SelectSingleNode("CityName").InnerText;
                    //�õ����еļ��
                    XmlNode shortNameNode = cityShortNameXml.SelectSingleNode("CityValueSet/CityItem[CityId=" + cityId + "]");
                    if (shortNameNode != null)
                    {
                        name = string.IsNullOrEmpty(shortNameNode.SelectSingleNode("Memo3").InnerText)
                            ? shortNameNode.SelectSingleNode("CityName").InnerText : shortNameNode.SelectSingleNode("Memo3").InnerText;
                    }

                    string engname = cityElem.SelectSingleNode("EngName").InnerText;
                    int CityOrder = cityElem.SelectSingleNode("Memo2") == null ? 0 : ConvertHelper.GetInteger(cityElem.SelectSingleNode("Memo2").InnerText);
                    int ParentCityOrder = cityElem.SelectSingleNode("Memo3") == null ? 0 : ConvertHelper.GetInteger(cityElem.SelectSingleNode("Memo3").InnerText);
                    XmlNode cityNode = resultXml.CreateElement("City");
                    XmlElement newcityElem = (XmlElement)cityNode;
                    newcityElem.SetAttribute("ID", cityId.ToString());
                    newcityElem.SetAttribute("Name", name);
                    newcityElem.SetAttribute("EngName", engname);
                    if (CityOrder > 0)
                        newcityElem.SetAttribute("CityOrder", CityOrder.ToString());
                    if (ParentCityOrder > 0)
                        newcityElem.SetAttribute("ParentCityOrder", ParentCityOrder.ToString());

                    provNode.AppendChild(cityNode);

                }

                CommonFunction.SaveXMLDocument(resultXml, m_NeedRelationProvinceAndCity);
                OnLog("     end exec GetNeedRelation!", true);
            }
            catch(Exception ex)
            {
                OnLog(ex.Message,true);
            }
        }

        /// <summary>
        /// дLog
        /// </summary>
        /// <param name="logText"></param>
        public void OnLog(string logText, bool nextLine)
        {
            if (Log != null)
                Log(this, new LogArgs(logText, nextLine));
        }
        
	}
}
