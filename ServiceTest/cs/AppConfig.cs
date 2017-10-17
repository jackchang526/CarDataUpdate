using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ServiceTest.cs
{
    public static class AppConfig
    {
        public static string DataPath
        {
            get
            {
                return ConfigurationManager.AppSettings["DataPath"].Trim();
            }
        }
        public static string CarDataUpdateConnString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["CarDataUpdateConnString"].ConnectionString;
            }
        }
        public static string AutoStroageConnString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["AutoStroageConnString"].ConnectionString;
            }
        }
        public static string CarChannelConnString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["CarChannelConnString"].ConnectionString;
            }
        }
    }
}
