using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class CarInfoEntity
    {
        public int CarId { get; set; }
        public string CarName { get; set; }
        public int YearType { get; set; }
        public string SerialShowName { get; set; }
        public string SerialSEOName { get; set; }
        public int UnderPan_ForwardGearNum { get; set; }
        public string UnderPan_TransmissionType { get; set; }
    }
}
