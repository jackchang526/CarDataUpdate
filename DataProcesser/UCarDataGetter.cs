//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Xml;

//using BitAuto.CarDataUpdate.DataProcesser.cn.ucar.www;
//using BitAuto.CarDataUpdate.Common;

//namespace BitAuto.CarDataUpdate.DataProcesser
//{
//	public class UCarDataGetter
//	{
//		/// <summary>
//		/// 获取二手车所有子品牌的车型数据
//		/// 删除了，不使用此方法 del by chengl Apr.24.2014
//		/// </summary>
//		public static void GetUCarAllSerialCarAmount()
//		{
//			BitAutoCarSource ucarObj = new BitAutoCarSource();
//			SerialCarAmount[] carAmounts = ucarObj.GetAllSerialCarAmount();
//			Dictionary<int, int> amountDic = new Dictionary<int, int>();
//			foreach(SerialCarAmount amountObj in carAmounts)
//			{
//				amountDic[amountObj.SerialId] = amountObj.CarAmount;
//			}

//			//二手车子品牌Xml文档
//			XmlDocument ucarDoc = new XmlDocument();
//			XmlElement root = ucarDoc.CreateElement("Params");
//			root.SetAttribute("Time", DateTime.Now.ToString());
//			ucarDoc.AppendChild(root);
//			XmlDeclaration declarEle = ucarDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
//			ucarDoc.InsertBefore(declarEle, root);


//			//取子品牌文件
//			string allDataFile = Path.Combine(CommonData.CommonSettings.SavePath, "AllAutoData.xml");
//			XmlDocument xmlDoc = new XmlDocument();
//			xmlDoc.Load(allDataFile);

//			XmlNodeList masterBrandNodeList = xmlDoc.SelectNodes("/Params/MasterBrand");
//			foreach(XmlElement masterNode in masterBrandNodeList)
//			{
//				XmlElement ucarMasterNode = GetUCarMasterNode(ucarDoc, masterNode, amountDic);
//				if (ucarMasterNode.ChildNodes.Count > 0)
//					root.AppendChild(ucarMasterNode);				
//			}

//			string ucarFileName = Path.Combine(CommonData.CommonSettings.SavePath, "UCarAmountData.xml");
//			ucarDoc.Save(ucarFileName);
//		}

//		private static XmlElement GetUCarMasterNode(XmlDocument ucarDoc, XmlElement masterNode, Dictionary<int, int> amountDic)
//		{
//			XmlElement masterRoot = ucarDoc.CreateElement("MasterBrand");
//			int masterUCarCount = 0;
//			XmlNodeList brandNodeList = masterNode.SelectNodes("Brand");
//			foreach(XmlElement brandNode in brandNodeList)
//			{
//				XmlElement brandRoot = ucarDoc.CreateElement("Brand");
//				XmlNodeList serialNodeList = brandNode.SelectNodes("Serial");
//				//品牌二手车数量
//				int brandUCarCount = 0;
//				foreach(XmlElement serialNode in serialNodeList)
//				{
//					int serialId = Convert.ToInt32(serialNode.GetAttribute("ID"));
//					if(amountDic.ContainsKey(serialId) && amountDic[serialId] > 0)
//					{
//						XmlElement ucarSerialNode = ucarDoc.CreateElement("Serial");
//						ucarSerialNode.SetAttribute("ID", serialNode.GetAttribute("ID"));
//						ucarSerialNode.SetAttribute("Name", serialNode.GetAttribute("Name"));
//						ucarSerialNode.SetAttribute("ShowName", serialNode.GetAttribute("ShowName"));
//						ucarSerialNode.SetAttribute("SerialSEOName", serialNode.GetAttribute("SerialSEOName"));
//						ucarSerialNode.SetAttribute("CsSaleState", serialNode.GetAttribute("CsSaleState"));
//						ucarSerialNode.SetAttribute("AllSpell", serialNode.GetAttribute("AllSpell"));
//						ucarSerialNode.SetAttribute("UCarAmount", amountDic[serialId].ToString());
//						brandRoot.AppendChild(ucarSerialNode);
//						brandUCarCount += amountDic[serialId];
//					}
//				}

//				if(brandUCarCount > 0 )
//				{
//					brandRoot.SetAttribute("ID", brandNode.GetAttribute("ID"));
//					brandRoot.SetAttribute("Name", brandNode.GetAttribute("Name"));
//					brandRoot.SetAttribute("BrandSEOName", brandNode.GetAttribute("BrandSEOName"));
//					brandRoot.SetAttribute("AllSpell", brandNode.GetAttribute("AllSpell"));
//					brandRoot.SetAttribute("UCarAmount", brandUCarCount.ToString());
//					masterUCarCount += brandUCarCount;
//					masterRoot.AppendChild(brandRoot);
//				}
//			}

//			if (masterUCarCount > 0)
//			{
//				masterRoot.SetAttribute("ID", masterNode.GetAttribute("ID"));
//				masterRoot.SetAttribute("Name", masterNode.GetAttribute("Name"));
//				masterRoot.SetAttribute("MasterSEOName", masterNode.GetAttribute("MasterSEOName"));
//				masterRoot.SetAttribute("Spell", masterNode.GetAttribute("Spell"));
//				masterRoot.SetAttribute("AllSpell", masterNode.GetAttribute("AllSpell"));
//				masterRoot.SetAttribute("UCarAmount", masterUCarCount.ToString());
//			}

//			return masterRoot;
//		}

//	}
//}
