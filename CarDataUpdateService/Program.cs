using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace BitAuto.CarDataUpdate.Service
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "/debug")
            {
                Service1 srv = new Service1();
                srv.Start(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
				{ 
					new Service1() 
				};
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
