using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    /// <summary>
    ///     还关注子品牌
    ///     author:songcl date:2014-12-10
    /// </summary>
    public class SerialToAttention
    {
        public int CsID; // 当前子品牌
        public string ToCsAllSpell; // 还关注子品牌全拼
        public int ToCsID; // 还关注子品牌ID
        public string ToCsName; // 还关注子品牌名
        public string ToCsPic; // 还关注子品牌默认图
        public string ToCsPriceRange; // 还关注子品牌价格区间
        public string ToCsShowName; // 还关注子品牌显示名
        public int ToPvNum; // 还关注子品牌次数
    }
}
