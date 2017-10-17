using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Model;

namespace BitAuto.CarDataUpdate.Common.Interface
{
	public delegate void DelayEventHandler(BaseProcesser sender, ContentMessage msg);
    public abstract class BaseProcesser
    {
		public event DelayEventHandler DelayEvent;
        public abstract void Processer(ContentMessage msg);
		public virtual void OnDelayEvent(BaseProcesser sender, ContentMessage msg)
		{
			if (DelayEvent != null)
				DelayEvent(sender, msg);
		}
    }
}
