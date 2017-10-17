using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    /// <summary>
    /// 品牌类型
    /// </summary>
    public enum BrandType
    {
        /// <summary>
        /// 主品牌
        /// </summary>
        Masterbrand = 1,

        /// <summary>
        /// 品牌
        /// </summary>
        Brand = 2,

        /// <summary>
        /// 子品牌
        /// </summary>
        Serial = 3
    }

    /// <summary>
    /// 品牌信息基类
    /// </summary>
    public abstract class BrandBase
    {

        public BrandBase()
        {
            this.ChildNodes = new List<BrandBase>();
        }

        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 全拼
        /// </summary>
        public string AllSpell { get; set; }

        /// <summary>
        /// SEO名称
        /// </summary>
        public string SEOName { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public BrandBase ParentNode { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<BrandBase> ChildNodes { get; set; }

        /// <summary>
        /// 品牌类型
        /// </summary>
        public abstract BrandType Type { get; }

        /// <summary>
        /// 获得子节点
        /// </summary>
        /// <param name="parentNodes"></param>
        /// <returns></returns>
        public static IEnumerable<BrandBase> GetChildNodes(IEnumerable<BrandBase> parentNodes)
        {
            foreach (BrandBase parentNode in parentNodes)
            {
                foreach (BrandBase childNode in parentNode.ChildNodes)
                {
                    yield return childNode;
                }
            }
        }
    }


    /// <summary>
    /// 主品牌信息
    /// </summary>
    public class MasterBrand : BrandBase
    {
        public override BrandType Type
        {
            get { return BrandType.Masterbrand; }
        }
    }


    /// <summary>
    /// 品牌信息
    /// </summary>
    public class Brand : BrandBase
    {
        public override BrandType Type
        {
            get { return BrandType.Brand; }
        }
    }


    /// <summary>
    /// 子品牌信息
    /// </summary>
    public class SerialBrand : BrandBase
    {
        public override BrandType Type
        {
            get { return BrandType.Serial; }
        }
    }
}
