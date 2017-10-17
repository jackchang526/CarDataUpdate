using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BitAuto.CarDataUpdate.Config.Tools
{
    public class FunctionCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FunctionItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FunctionItem)element).FunctionName;
        }

        public FunctionItem this[int index]
        {
            get
            {
                return (FunctionItem)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }


        new public FunctionItem this[string key]
        {
            get
            {
                return (FunctionItem)base.BaseGet(key);
            }
        }
    }
}
