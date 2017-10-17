using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Interface
{
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : Attribute
    {
        private string _description;
        public DescriptionAttribute(string description)
        {
            _description = description;
        }
        public string Description
        {
            get
            {
                return _description;
            }
        }
    }
}
