using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceModel
{
    /// <summary>
    /// 商品促销信息
    /// </summary>
    public class GoodsPromotion
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 商品Id
        /// </summary>
        public int GoodsId { get; set; }
        /// <summary>
        /// 促销名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public Int16 Type { get; set; }

        public static GoodsPromotion GetGoodsPromotion(XElement ele)
        {
            if (ele == null)
                return null;

            GoodsPromotion newPromotion = new GoodsPromotion();
            newPromotion.Name = ele.Element("Name").Value;
            newPromotion.Description = ele.Element("Description").Value;
            newPromotion.Type = Convert.ToInt16(ele.Element("Type").Value);
            return newPromotion;
        }
    }
}
