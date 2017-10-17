using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BitAuto.CarDataUpdate.Config
{
    public class CarNewsTypeSettings
    {
        public Dictionary<int, CarNewsTypeItem> CarNewsTypeList { get; set; }
    }
}
