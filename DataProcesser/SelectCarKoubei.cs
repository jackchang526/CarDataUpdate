using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
using BitAuto.CarDataUpdate.Common;
using System.Xml;
using ServiceStack.Redis;

namespace BitAuto.CarDataUpdate.DataProcesser
{
    /// <summary>
    /// 高级选车口碑相关
    /// </summary>
    public class SelectCarKoubei
    {
        //public event LogHandler Log;

        //配置的口碑印象词
        private string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\SelectCarImpression.config");
        //口碑印象字典url
        //private string KeibeiImpressionWordDicUrl = ConfigurationManager.AppSettings["KeibeiImpressionWordDicUrl"];
        //子品牌对应的口碑印象url
        private string KeibeiImpressionSerialWordsUrl = ConfigurationManager.AppSettings["KeibeiImpressionSerialWordsUrl"];

        /// <summary>
        /// 子品牌对应印象词字段(解析KeibeiImpressionSerialWordsUrl)
        /// </summary>
        private Dictionary<int, List<int>> SerialWordDic = new Dictionary<int, List<int>>();
        /// <summary>
        /// 印象概括词对应的二级印象
        /// </summary>
        private Dictionary<int, List<int>> ImpressioToSecWord = new Dictionary<int, List<int>>();

        /// <summary>
        /// 印象概括词对应的子品牌
        /// </summary>
        private Dictionary<int, List<int>> ImpressionSerialDic = new Dictionary<int, List<int>>();


        public void StoreKoubeiImpressionInRedis()
        {
            Common.Log.WriteLog("开始更新高级选车接口数据，存储口碑印象词到redis里");
            InitUrl();
            GetImpressionLevel();
            GetSerialWordDic();
            GetImpressionSerialDic();
            StoreDataIntoRedis();
            Common.Log.WriteLog("更新高级选车接口数据，存储口碑印象词到redis里，完成");
        }

        /// <summary>
        ///写redis
        /// </summary>
        private void StoreDataIntoRedis()
        {
            try
            {
                using (IRedisClient redis = RedisManager.Prcm.GetClient(),redisReadOnly = RedisManager.Prcm.GetReadOnlyClient())
                {
                    using (var tran = redis.CreateTransaction())
                    {
                        foreach (KeyValuePair<int, List<int>> kv in ImpressionSerialDic)
                        {
                            string key = RedisManager.PreKey + kv.Key.ToString();
                            List<string> newList = kv.Value.Select(x => x.ToString()).ToList<string>();
                            List<string> redisList = redisReadOnly.GetAllItemsFromSet(key).ToList<string>();

                            if (redisList == null || redisList.Count == 0)
                            {
                                tran.QueueCommand(x => x.AddRangeToSet(key, newList));
                            }
                            else
                            {
                                foreach (string item in redisList)
                                {
                                    if (!newList.Contains(item))
                                    {
                                        tran.QueueCommand(x => x.RemoveItemFromSet(key, item));
                                    }
                                }
                                foreach (string item in newList)
                                {
                                    if (!redisList.Contains(item))
                                    {
                                        tran.QueueCommand(x => x.AddItemToSet(key, item));
                                    }
                                }
                            }
                        }
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                //OnLog("高级选车接口，印象数据写redis错误" + ex.Message, true);
                Common.Log.WriteErrorLog("高级选车接口，印象数据写redis错误:" + ex.ToString());
            }
        }

        /// <summary>
        /// 印象词对应的子品牌
        /// </summary>
        private void GetImpressionSerialDic()
        {
            Dictionary<int, List<int>> tempImpressionSerialDic = new Dictionary<int, List<int>>();
            foreach (KeyValuePair<int, List<Int32>> swkv in SerialWordDic)
            {
                List<int> secWord = swkv.Value;
                for (int i = 0; i < secWord.Count; i++)
                {
                    foreach (KeyValuePair<int, List<Int32>> impkv in ImpressioToSecWord)
                    {
                        if(impkv.Value.Contains(secWord[i]))
                        {
                            if (tempImpressionSerialDic.ContainsKey(impkv.Key))
                            {
                                tempImpressionSerialDic[impkv.Key].Add(swkv.Key);
                            }
                            else
                            {
                                List<int> serialIdList = new List<int>();
                                serialIdList.Add(swkv.Key);
                                tempImpressionSerialDic.Add(impkv.Key, serialIdList);
                            }
                        }
                    }
                }
            }

            foreach(KeyValuePair<int,List<int>> kv in tempImpressionSerialDic)
            {
                ImpressionSerialDic.Add(kv.Key, kv.Value.Distinct().ToList<int>());
            }
        }

        /// <summary>
        /// 初始化url
        /// </summary>
        private void InitUrl()
        {
            DateTime yesterday = DateTime.Now.AddDays(-1);
            string year = yesterday.Year.ToString();
            string month = yesterday.Month.ToString();
            string day = yesterday.Day.ToString();
            KeibeiImpressionSerialWordsUrl = KeibeiImpressionSerialWordsUrl.Replace("{year}", year).Replace("{month}", month).Replace("{day}", day);
            //KeibeiImpressionWordDicUrl = KeibeiImpressionWordDicUrl.Replace("{year}", year.ToString()).Replace("{month}", month).Replace("{day}", day);
        }


        private void GetSerialWordDic()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(KeibeiImpressionSerialWordsUrl);
                XmlNodeList serialNodeList = xmlDoc.SelectNodes("SerialWords/Item");
                foreach (XmlNode serialNode in serialNodeList)
                {
                    int id = BitAuto.Utils.ConvertHelper.GetInteger(serialNode.Attributes["serialId"].Value);
                    List<string> wordList = serialNode.Attributes["words"].Value.ToString().Split(',').ToList();
                    List<int> list = new List<int>();
                    wordList.ForEach(x => list.Add(BitAuto.Utils.ConvertHelper.GetInteger(x)));
                    SerialWordDic.Add(id, list);
                }
            }
            catch (Exception ex)
            {
                //OnLog(KeibeiImpressionSerialWordsUrl + " 文件解析错误" + ex.Message, true);
                Common.Log.WriteErrorLog(KeibeiImpressionSerialWordsUrl + " 文件解析错误:" + ex.Message);
            }
        }

        private void GetImpressionLevel()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    //OnLog(@"config\SelectCarImpression.config 印象词配置文件不存在", true);
                    Common.Log.WriteLog(@"config\SelectCarImpression.config 印象词配置文件不存在");
                    return;
                }
                XmlDocument ConfigDic = new XmlDocument();
                ConfigDic.Load(filePath);
                XmlNodeList wordNoedList = ConfigDic.SelectNodes("root/word");
                foreach (XmlNode wordNode in wordNoedList)
                {
                    int id = BitAuto.Utils.ConvertHelper.GetInteger(wordNode.Attributes["id"].Value);
                    List<int> dicList = new List<int>();
                    XmlNodeList itemNodeList = wordNode.SelectNodes("item");
                    foreach (XmlNode itemNode in itemNodeList)
                    {
                        int itemId = BitAuto.Utils.ConvertHelper.GetInteger(itemNode.Attributes["id"].Value);
                        dicList.Add(itemId);
                    }
                    ImpressioToSecWord.Add(id, dicList);
                }
            }
            catch (Exception ex)
            {
                //OnLog(@"config\SelectCarImpression.config 文件解析错误" + ex.Message, true);
                Common.Log.WriteErrorLog(@"config\SelectCarImpression.config 文件解析错误" + ex.Message);
            }
        }

        /*
        /// <summary>
        /// 概括词对应一级词汇，再查找二级词汇
        /// </summary>
        private void GetImpressionLevel()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    OnLog(@"config\SelectCarImpression.config 印象词配置文件不存在", true);
                    return;
                }
                XmlDocument ConfigDic = new XmlDocument();
                ConfigDic.Load(filePath);
                XmlDocument wordDicDoc = new XmlDocument();
                wordDicDoc.Load(KeibeiImpressionWordDicUrl);
                XmlNodeList wordNoedList = ConfigDic.SelectNodes("root/word");
                foreach (XmlNode wordNode in wordNoedList)
                {
                    int id = Convert.ToInt32(wordNode.Attributes["id"].Value);
                    List<int> dicList = new List<int>();
                    XmlNodeList itemNodeList = wordNode.SelectNodes("item");
                    foreach (XmlNode itemNode in itemNodeList)
                    {
                        int itemId = Convert.ToInt32(itemNode.Attributes["id"].Value);

                        XmlNodeList dicNodeList = wordDicDoc.SelectNodes("WordDic/Item[@parentId='" + itemId + "']");
                        foreach (XmlNode dicNode in dicNodeList)
                        {
                            int dicId = Convert.ToInt32(dicNode.Attributes["Id"].Value);
                            dicList.Add(dicId);
                        }
                    }
                    ImpressioToSecWord.Add(id, dicList);
                }
            }
            catch (Exception ex)
            {
                OnLog(@"config\SelectCarImpression.config 文件解析错误" + ex.Message, true);
            }
        }
         * */
    }
}
