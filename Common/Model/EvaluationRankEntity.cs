using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    public class EvaluationRankEntity
    {
        public int StyleId { get; set; }
        public int PropertyId { get; set; }
        public double PropertyValue { get; set; }
        public int EvaluationId { get; set; }
        public string EditorsName { get; set; }
        public string Weather { get; set; }
        public string Wind { get; set; }
        public string Temperature { get; set; }
        public string StyleName { get; set; }
        public string MasterBrandName { get; set; }
        public string ModelName { get; set; }
        public int Year { get; set; }
        public string ModelDisplayName { get; set; }
        public string ModelLevel { get; set; }
        /// <summary>
        /// 级别内排行名次
        /// </summary>
        public int LevelNum { get; set; }
        /// <summary>
        /// 总名次
        /// </summary>
        public int RankNum { get; set; }
        /// <summary>
        /// 当前车款标识
        /// </summary>
        public bool CurrentFlag { get; set; }
        public string Unit { get; set; }
        public string TabText { get; set; }

        public string FuelType { get; set; }

        public string ModelAllSpell { get; set; }
    }
}
