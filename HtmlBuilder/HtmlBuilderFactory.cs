using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Config;
using BitAuto.CarDataUpdate.Common;
using System.Reflection;

namespace BitAuto.CarDataUpdate.HtmlBuilder
{
    public static class HtmlBuilderFactory
    {
        public static BaseBuilder[] GetHtmlBuilders(int newsType)
        {
            if (CommonData.CarNewsTypeSettings.CarNewsTypeList.ContainsKey(newsType))
            {
                CarNewsTypeItem newsTypeItem = CommonData.CarNewsTypeSettings.CarNewsTypeList[newsType];
                List<BaseBuilder> result = new List<BaseBuilder>(newsTypeItem.Builders.Length);
                foreach (CarNewsTypeBuilder builder in newsTypeItem.Builders)
                {
                    Type type = Type.GetType(builder.Type, false, true);
                    if (type == null)
                        continue;
                    BaseBuilder classBuilder = type.InvokeMember(null, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null) as BaseBuilder;
                    if (classBuilder != null)
                        result.Add(classBuilder);
                }
                return result.ToArray();
            }
            return null;
        }
    }
}
