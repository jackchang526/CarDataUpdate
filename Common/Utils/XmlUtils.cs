using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BitAuto.CarDataUpdate.Common.Utils
{
    public class XmlUtils
    {
        /// <summary>
        /// 获得子节点的InnerText
        /// </summary>
        /// <param name="parentNode">父节点</param>
        /// <param name="childNodeName">子节点名称</param>
        /// <returns></returns>
        public static string GetChildNodeInnerText(XmlElement parentNode, string childNodeName)
        {
            string innerText = null;
            if (parentNode != null)
            {
                XmlNode child = parentNode.SelectSingleNode(string.Format("./{0}",
                    childNodeName));
                if (child != null)
                {
                    innerText = child.InnerText;
                }
            }
            return innerText;
        }
    }
}
