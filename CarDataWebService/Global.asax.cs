using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace BitAuto.CarDataUpdate.WebService
{
	public class Global : System.Web.HttpApplication
	{

		protected void Application_Start(object sender, EventArgs e)
		{
			log4net.Config.XmlConfigurator.Configure();
			BitAuto.CarDataUpdate.WebService.DataSync.DataSyncProvider.IntiMethodInfos();
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{
			// 在出现未处理的错误时运行的代码
			Exception objErr = Server.GetLastError().GetBaseException();
			string err = "\r\nIP: " + BitAuto.Utils.WebUtil.GetClientIP() +
			"\r\nError in: " + Request.Url.ToString() +
			"\r\nRef: " + (Request.UrlReferrer == null ? "" : Request.UrlReferrer.ToString()) +
			"\r\nError Message: " + objErr.Message.ToString() +
			"\r\nStack Trace: " + objErr.StackTrace.ToString();
			Common.Log.WriteErrorLog(err);
		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}