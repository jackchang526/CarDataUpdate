using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    public class DeleteNewsTool
    {
        /// <summary>
        /// 删除去掉的分类,新闻：150，144，147，83，导购：102，115，120
        /// </summary>
        public static void DeleteNews()
        {
            Console.WriteLine("DeleteNews。。。");
            int[] xwCateIds = new int[] { 150, 144, 147, 83 };
            int[] dgCateIds = new int[] { 102, 115, 120 };
            //子品牌分类目录
            string basePath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\xinwen");
            string[] fileList = Directory.GetFiles(basePath, "*.xml", SearchOption.TopDirectoryOnly);
            int counter = 0;
            foreach (string fileName in fileList)
            {
                counter++;
                Console.WriteLine(counter + "/" + fileList.Length);
                Console.CursorTop -= 1;
                try
                {
                    DeleteNews(xwCateIds, fileName, "xinwen");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            basePath = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\daogou");
            fileList = Directory.GetFiles(basePath, "*.xml", SearchOption.TopDirectoryOnly);
            counter = 0;
            foreach (string fileName in fileList)
            {
                counter++;
                Console.WriteLine(counter + "/" + fileList.Length);
                Console.CursorTop -= 1;
                try
                {
                    DeleteNews(dgCateIds, fileName, "daogou");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Console.CursorTop += 1;
        }

        /// <summary>
        /// 按文件删除
        /// </summary>
        /// <param name="cateIds"></param>
        /// <param name="fileName"></param>
        public static void DeleteNews(int[] cateIds, string fileName, string newsType)
        {

            XmlDocument newsDoc = new XmlDocument();
            newsDoc.Load(fileName);
            XmlNodeList newsNodeList = newsDoc.SelectNodes("/root/listNews");
            int counter = 0;
            foreach (XmlNode newsNode in newsNodeList)
            {
                XmlNode catePathNode = newsNode.SelectSingleNode("CategoryPath");
                if (catePathNode == null)
                    continue;
                bool needDelete = ArrayContains(cateIds, catePathNode.InnerText);

                if (needDelete)
                {
                    newsNode.ParentNode.RemoveChild(newsNode);
                    counter++;
                }
            }

            if (counter > 0)
            {
                //记录新闻总数
                XmlNode countNode = newsDoc.SelectSingleNode("/root/newsAllCount/allcount");
                int newsNum = newsDoc.DocumentElement.ChildNodes.Count - 1;
                if (countNode != null)
                {
                    countNode.InnerText = newsNum.ToString();
                }
                newsDoc.Save(fileName);
                //更新新闻数量
                UpdateNewsNum(newsNum, newsType, fileName);
                Console.WriteLine(fileName);
            }
        }

        private static void UpdateNewsNum(int newsNum, string newsType, string fileName)
        {
            //分析子品牌ID与年款
            string fn = Path.GetFileNameWithoutExtension(fileName);
            fn = fn.Substring(16);
            int pos = fn.IndexOf("_");
            int serialId = 0;
            int carYear = 0;
            if (pos > -1)
            {
                //有年款
                serialId = Convert.ToInt32(fn.Substring(0, pos));
                carYear = Convert.ToInt32(fn.Substring(pos + 1));
            }
            else
                serialId = Convert.ToInt32(fn);
            string xmlPath = "/SerilaList/Serial[@id=" + serialId + "]";
            if (carYear > 0)
                xmlPath += "/Year[@year=" + carYear + "]";

            string numFile = Path.Combine(CommonData.CommonSettings.SavePath, "SerialNews\\newsNum.xml");
            XmlDocument numDoc = new XmlDocument();
            numDoc.Load(numFile);
            XmlElement sNode = (XmlElement)numDoc.SelectSingleNode(xmlPath);
            if (sNode != null)
            {
                sNode.SetAttribute(newsType, newsNum.ToString());
            }
            numDoc.Save(numFile);

        }

        /// <summary>
        /// 是否包含该分类
        /// </summary>
        /// <param name="cateIds"></param>
        /// <param name="idList"></param>
        /// <returns></returns>
        private static bool ArrayContains(int[] cateIds, string idList)
        {
            if (String.IsNullOrEmpty(idList))
                return false;

            char[] sepChars = new char[] { ',' };
            string[] cateIdList = idList.Split(sepChars, StringSplitOptions.RemoveEmptyEntries);
            bool isContains = false;
            //判断是否有分类符合删除标准
            foreach (string idStr in cateIdList)
            {
                int cateId = 0;
                bool isId = Int32.TryParse(idStr, out cateId);
                if (isId)
                {
                    foreach (int tmpCateId in cateIds)
                    {
                        if (cateId == tmpCateId)
                        {
                            isContains = true;
                            break;
                        }
                    }
                }

                if (isContains)
                    break;
            }
            return isContains;
        }
    }
}
