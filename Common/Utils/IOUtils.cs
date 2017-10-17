using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BitAuto.CarDataUpdate.Common.Utils
{
    public static class IOUtils
    {
        /// <summary>
        /// 创建指定的目录
        /// 如果存在此目录跳过
        /// </summary>
        /// <param name="fullPath">目录全路径</param>
        /// <returns>目录信息</returns>
        public static DirectoryInfo CreateDirecotry(string fullPath)
        {
            return !Directory.Exists(fullPath) ?
                Directory.CreateDirectory(fullPath) : new DirectoryInfo(fullPath);
        }
    }
}
