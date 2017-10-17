using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Model
{
    /// <summary>
    /// 评测的标签 名及匹配规则
    /// </summary>
    public struct PingCeTag
    {
        public string tagName;
        public string tagRegularExpressions;
        public string url;
    }
}
