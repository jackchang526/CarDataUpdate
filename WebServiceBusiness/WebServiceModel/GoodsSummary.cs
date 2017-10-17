using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BitAuto.CarDataUpdate.WebServiceModel
{
    /// <summary>
    /// 商品基本信息类
    /// </summary>
    public class GoodsSummary
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 商品的guid
        /// </summary>
        public Guid GoodsGUID { get; set; }
        /// <summary>
        /// 商品编号
        /// </summary>
        public string GoodsNumber { get; set; }
        /// <summary>
        /// 商品Url
        /// </summary>
        public string GoodsUrl { get; set; }
        /// <summary>
        /// 主品牌id
        /// </summary>
        public int Bs_Id { get; set; }
        /// <summary>
        /// 子品牌id
        /// </summary>
        public int Cs_Id { get; set; }
        /// <summary>
        /// 促销标题
        /// </summary>
        public string PromotTitle { get; set; }
        /// <summary>
        /// 封图
        /// </summary>
        public string CoverImageUrl { get; set; }
        /// <summary>
        /// 活动开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 活动结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 促销方式
        /// </summary>
        public Int16 PromotWay { get; set; }
        /// <summary>
        /// 排序值
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// 订单提交方式1：厂商、2：经销商
        /// </summary>
        public int OrderBelongTo { get; set; }
        /// <summary>
        /// 商品创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 商品状态0草稿箱、1在售、2下架
        /// </summary>
        public Int16 GoodsStatus { get; set; }
        /// <summary>
        /// 最小厂家指导价 Decimal(18,2)
        /// </summary>
        public Decimal MinMarketPrice { get; set; }
        /// <summary>
        /// 最小易车优惠 Decimal(18,2)
        /// </summary>
        public Decimal MinBitautoPrice { get; set; }
		/// <summary>
		/// 消息体标示 AppId
		/// </summary>
		public int AppId { get; set; }

        /// <summary>
        /// 关联车型
        /// </summary>
        public List<GoodsAreaCar> GoodsAreaCars { get; set; }
        /// <summary>
        /// 关联促销
        /// </summary>
        public List<GoodsPromotion> GoodsPromotions { get; set; }

        /// <summary>
        /// 根据XDocument获取数据实体
        /// </summary>
        public static GoodsSummary GetGoodsSummary(XDocument xDoc)
        {
            if (xDoc != null && xDoc.Root!=null)
            {
                return GetGoodsSummary(xDoc.Root.Element("GoodsSummary"));
            }
            return null;
        }
        /// <summary>
        /// 根据XElement获取数据实体
        /// </summary>
        public static GoodsSummary GetGoodsSummary(XElement xEle)
        {
            if (xEle == null)
                return null;

            var result = new GoodsSummary();
            result.Id = Convert.ToInt32(xEle.Element("GoodsId").Value);
            result.GoodsUrl = xEle.Element("GoodsUrl").Value;
            result.GoodsNumber = xEle.Element("GoodsNumber").Value;
            result.Bs_Id = Convert.ToInt32(xEle.Element("Bs_Id").Value);
            result.Cs_Id = Convert.ToInt32(xEle.Element("Cs_Id").Value);
            result.PromotTitle = xEle.Element("PromotTitle").Value;
            result.StartTime = Convert.ToDateTime(xEle.Element("StartTime").Value);
            if (!string.IsNullOrWhiteSpace(xEle.Element("EndTime").Value))
            {
                result.EndTime = Convert.ToDateTime(xEle.Element("EndTime").Value);
            }
            result.PromotWay = Convert.ToInt16(xEle.Element("PromotWay").Value);
            result.CoverImageUrl = xEle.Element("CoverImageUrl").Value;
            result.CreateTime = Convert.ToDateTime(xEle.Element("CreateTime").Value);
            result.OrderBelongTo = Convert.ToInt16(xEle.Element("OrderBelongTo").Value);
            result.DisplayOrder = Convert.ToInt32(xEle.Element("DisplayOrder").Value);
            result.GoodsStatus = Convert.ToInt16(xEle.Element("GoodsStatus").Value);

            result.GoodsPromotions = new List<GoodsPromotion>();
            foreach (var promotionEle in from promotions in xEle.Elements("GoodsPromotion")
                                         from items in promotions.Elements("Items")
                                         from item in items.Elements("Item")
                                         select item)
            {
                GoodsPromotion pItem = GoodsPromotion.GetGoodsPromotion(promotionEle);
                if (pItem == null)
                    continue;
                pItem.GoodsId = result.Id;
                result.GoodsPromotions.Add(pItem);
            }

            result.GoodsAreaCars = new List<GoodsAreaCar>();
            foreach (var areaCarEle in from goodsArea in xEle.Elements("GoodsArea")
                                       from items in goodsArea.Elements("Items")
                                       from item in items.Elements("Item")
                                         select item)
            {
                foreach (var carItem in GoodsAreaCar.GetGoodsAreaCars(areaCarEle))
                {
                    carItem.GoodsId = result.Id;
                    carItem.Bs_Id = result.Bs_Id;
                    carItem.Cs_Id = result.Cs_Id;
                    result.GoodsAreaCars.Add(carItem);

                    if (result.MinBitautoPrice > carItem.BitautoPrice)
                        result.MinBitautoPrice = carItem.BitautoPrice;
                    if (result.MinMarketPrice > carItem.MarketPrice)
                        result.MinMarketPrice = carItem.MarketPrice;
                }
            }


            return result;
        }
    }
}
