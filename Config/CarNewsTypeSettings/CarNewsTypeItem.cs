using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BitAuto.CarDataUpdate.Config
{
    public class CarNewsTypeItem
    {
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// TypeStr
        /// </summary>
        public string TypeStr { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 分类ids
        /// </summary>
        private string _categoryIds;
        public string CategoryIds
        {
            get{return _categoryIds;}
            set
            {
                _categoryIds = value;
                if (string.IsNullOrEmpty(_categoryIds))
                {
                    _categoryIdList = new List<int>();
                }
                else
                {
                    int id;
                    string[] ids = _categoryIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> idList = new List<int>(ids.Length);
                    foreach (string idStr in ids)
                    {
                        if (int.TryParse(idStr.Trim(), out id))
                        {
                            if (idList.Contains(id))
                                continue;
                            idList.Add(id);
                        }
                    }
                    _categoryIdList = idList;
                }
            }
        }
        /// <summary>
        /// 分类集合
        /// </summary>
        private List<int> _categoryIdList;
        /// <summary>
        /// 新闻分类数组
        /// </summary>
        public List<int> CategoryIdList { get { return _categoryIdList; } }
        /// <summary>
        /// 处理类
        /// </summary>
        public CarNewsTypeBuilder[] Builders { get; set; }
        /// <summary>
        /// 关联类型
        /// </summary>
        public CarTypes CarTypes { get; set; }
        /// <summary>
        /// 配置字典
        /// </summary>
        public Dictionary<string, string> KeyValueDic { get; set; }
    }
}
