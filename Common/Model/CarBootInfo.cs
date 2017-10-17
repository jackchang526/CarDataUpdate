using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class CarBootInfo
    {
        public int CsId { get; set; }
        public string CsShowName { get; set; }
        public int CarId { get; set; }
        public int ParamId { get; set; }
        public string Pvalue { get; set; }
        public bool IsCurrent { get; set; }
        public string SerialAllSpell { get; set; }
        public int CarYear { get; set; }
        public string CarName { get; set; }
    }
}
