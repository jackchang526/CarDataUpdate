using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;

namespace BitAuto.CarDataUpdate.WebServiceModel
{
    /// <summary>
    /// 商品车型区域信息
    /// </summary>
    public class GoodsAreaCar
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
        /// 省份id，0=全国
        /// </summary>
        public int ProvinceId { get; set; }
        /// <summary>
        /// 城市id，0=全国
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// 主品牌id
        /// </summary>
        public int Bs_Id { get; set; }
        /// <summary>
        /// 子品牌id
        /// </summary>
        public int Cs_Id { get; set; }
        /// <summary>
        /// 车型id
        /// </summary>
        public int Car_Id { get; set; }
        /// <summary>
        /// 厂家指导价
        /// </summary>
        public Decimal MarketPrice { get; set; }
        /// <summary>
        /// 易车优惠 Decimal(18,2)
        /// </summary>
        public Decimal BitautoPrice { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int TotalStock { get; set; }
        /// <summary>
        /// 销量数据
        /// </summary>
        public int SalesCount { get; set; }

        public static IEnumerable<GoodsAreaCar> GetGoodsAreaCars(XElement ele)
        {
            if (ele == null)
               yield return null;

            int provinceId = Convert.ToInt32(ele.Element("ProvinceId").Value);
            int cityId = Convert.ToInt32(ele.Element("CityId").Value);

            foreach (var carEle in from cars in ele.Elements("GoodsCars")
                                   from items in cars.Elements("Items")
                                   from car in items.Elements("Item")
                                   select car)
            {
                GoodsAreaCar newItem = new GoodsAreaCar();
                newItem.ProvinceId = provinceId;
                newItem.CityId = cityId;
                newItem.Car_Id = Convert.ToInt32(carEle.Element("Car_Id").Value);
                newItem.MarketPrice = Convert.ToDecimal(carEle.Element("MarketPrice").Value);
                newItem.BitautoPrice = Convert.ToDecimal(carEle.Element("BitautoPrice").Value);
                newItem.TotalStock = Convert.ToInt32(carEle.Element("TotalStock").Value);
                newItem.SalesCount = Convert.ToInt32(carEle.Element("SalesCount").Value);
                yield return newItem;
            }
        }
    }
}
