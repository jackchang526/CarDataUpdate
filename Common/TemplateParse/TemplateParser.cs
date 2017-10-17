using System;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Context;
using Commons.Collections;
using NVelocity.Runtime;
using System.Collections.Generic;

namespace BitAuto.Car.ContentLib.TemplateParse
{
    /// <summary>
    /// NVelocity模板工具类 NVelocityHelper
    /// </summary>
    public class TemplateParser
    {
        private VelocityEngine velocity = null;
        private string templateDir = null;

        #region 构造函数及初始化函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ps_TemplateDir">模板文件夹路径</param>
        public TemplateParser(string ps_TemplateDir)
        {
            templateDir = ps_TemplateDir;
            Init();
        }

        /// <summary>
        /// 初始化NVelocity模块 
        /// </summary>
        private void Init()
        {
            //创建VelocityEngine实例对象
            velocity = new VelocityEngine();

            //使用设置初始化VelocityEngine
            ExtendedProperties props = new ExtendedProperties();
            props.AddProperty(RuntimeConstants.RESOURCE_LOADER, "file");
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH,
                AppDomain.CurrentDomain.BaseDirectory + templateDir);
            props.AddProperty(RuntimeConstants.INPUT_ENCODING, "UTF-8");
            props.AddProperty(RuntimeConstants.OUTPUT_ENCODING, "UTF-8");
            velocity.Init(props);
        }

        #endregion

        /// <summary>
        /// 解析模板
        /// </summary>
        /// <param name="templateFileName">模板文件名称</param>
        /// <param name="context">上下文</param>
        /// <param name="writer">writer</param>
        public void ParseTemplate(string templateFileName, IDictionary<string, object> context, TextWriter writer)
        {
            IContext velocityContext = new VelocityContext();
            foreach (KeyValuePair<string, object> keyValue in context)
            {
                velocityContext.Put(keyValue.Key, keyValue.Value);
            }

            //从文件中读取模板
            Template template = velocity.GetTemplate(templateFileName);
            //合并模板
            template.Merge(velocityContext, writer);
        }

    }
}