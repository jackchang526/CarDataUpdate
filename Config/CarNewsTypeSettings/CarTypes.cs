using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Config
{
    [Flags]
    public enum CarTypes
    {
        /// <summary>
        /// 子品牌
        /// </summary>
        Serial = 1,
        /// <summary>
        /// 品牌
        /// </summary>
        Brand = 2,
        /// <summary>
        /// 主品牌
        /// </summary>
        MasterBrand = 4,
        /// <summary>
        /// 厂商
        /// </summary>
        Producer = 8,
        /// <summary>
        /// 级别
        /// </summary>
        Level = 16
    }
}
