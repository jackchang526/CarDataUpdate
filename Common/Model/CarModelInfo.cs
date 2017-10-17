using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class CarModelInfo
    {
        public double Height { get; set; }
        public double Weight { get; set; }
        public string ImageUrl { get; set; }

        /// <summary>
        /// 车辆内部空间枚举类型,值为 枚举CommonEnum.CarInnerSpaceType 中的一个
        /// </summary>
        public int Type { get; set; }
        public int ParaId { get; set; }
    }
}
