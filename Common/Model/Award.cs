using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class Award
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LogoUrl { get; set; }

        public List<YearInfo> YearInfos { get; set; }
    }

    public class YearInfo
    {
        public int Id { get; set; }

        public string YearName { get; set; }

        public List<ChildAwardInfo> ChildAwardInfos { get; set; }
        /// <summary>
        /// 无子奖项时显示备注信息
        /// </summary>
        public string Remarks { get; set; }
    }

    public class ChildAwardInfo
    {
        public string ChildAwardName { get; set; }

        public string CarRemark { get; set; }
    }
}
