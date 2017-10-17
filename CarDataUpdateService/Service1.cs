using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Reflection;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Config;
using System.Configuration;
using BitAuto.CarDataUpdate.NewsProcesser;
using System.Messaging;
using System.Xml;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Model;
using System.Threading.Tasks;

namespace BitAuto.CarDataUpdate.Service
{
    partial class Service1 : ServiceBase
    {
        private MessageReceiver msgReceiver = null;
        private bool m_serviceStopping = false;
        private Dictionary<string, Task> dictWorkTask = null;
        private MessageQueueConfig config;

        public Service1()
        {
            log4net.Config.XmlConfigurator.Configure();
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        /// <summary>
        /// 服务开始执行
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Log.WriteLog("start service!");

            m_serviceStopping = false;
            try
            {
                //各业务消息队列路径配置
                config = ConfigurationManager.GetSection("MessageQueueConfig") as MessageQueueConfig;

                //消息队列分发主线程
                Thread mainThread = new Thread(new ThreadStart(MainMessageController));
                mainThread.Name = "MainThread";
                mainThread.Start();
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }

        protected override void OnStop()
        {
            m_serviceStopping = true;
            //等待任务结束
            //Task.WaitAll(dictWorkTask.Values.ToArray());
            Log.WriteLog("stop service!");
        }
        
        /// <summary>
        /// 主线程分发各业务消息队列
        /// </summary>
        private void MainMessageController()
        {
            //验证各业务消息队列配置是否正确
            if (config.ConfigList.Count <= 0)
            {
                Log.WriteErrorLog("各业务消息队列配置不正确");
                return;
            }
            try
            {
                Log.WriteLog("start main thread!");

                DateTime timer = DateTime.Now;
                TimeSpan interval = TimeSpan.FromHours(1);

                Log.WriteLog("start init common data!");
                Common.CommonData.InitData();
                Log.WriteLog("end init common data!");

                Log.WriteLog("start init DelayMessageList!");
                DelayProcesser.InitDelayMessageList();
                Log.WriteLog("end init DelayMessageList!");

                //处理各业务消息队列任务
                MultiTaskController();

                msgReceiver = new MessageReceiver();

                while (true && !m_serviceStopping)
                {
                    try
                    {
                        ContentMessage msg = msgReceiver.ReceiverMessage();
                        if (msg != null)
                        {
                            #region 一小时更新一次缓存数据
                            if (DateTime.Now - timer > interval)
                            {
                                timer = DateTime.Now;
                                Log.WriteLog("start reload common data!");
                                Common.CommonData.InitData();
                                Log.WriteLog("end reload common data!");
                            }
                            #endregion
                            string workType = msg.From;
                            if (config.ConfigList.ContainsKey(workType))
                                MessageService.SendMessage(config.ConfigList[workType].QueueName, msg.ContentBody);
                            else
                                Log.WriteErrorLog(string.Format("消息队列配置中业务标识：{0}，不存在。", workType));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteErrorLog(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog(ex.ToString());
            }
        }
        
        /// <summary>
        /// 多任务处理各业务消息队列
        /// </summary>
        private void MultiTaskController()
        {
            dictWorkTask = new Dictionary<string, Task>();
            foreach (MessageQueueSetting setting in config.ConfigList.Values)
            {
                dictWorkTask.Add(setting.BusinessName, Task.Factory.StartNew(obj => { WorkTask(obj); }, setting));
            }
        }
        
        /// <summary>
        /// 工作任务
        /// </summary>
        /// <param name="obj"></param>
        private void WorkTask(object obj)
        {
            MessageQueueSetting messageQueueSetting = (MessageQueueSetting)obj;
            Thread.CurrentThread.Name = messageQueueSetting.BusinessName;
            Log.WriteLog(string.Format("任务线程：{0}，已启动。", messageQueueSetting.BusinessName));
            msgReceiver = new MessageReceiver();
            BaseProcesser processer = null;
            ContentMessage msg = null;

            if (messageQueueSetting.BusinessName == "Photo")
            {
                CollectPhoteMessage(messageQueueSetting);
            }
            else
            {
                while (true && !m_serviceStopping)
                {
                    try
                    {
                        if (messageQueueSetting.BusinessName == "CMS")
                            msg = msgReceiver.GetDelayMessage();
                        if (msg == null)
                        {
                            XmlDocument receiverMessageXmlDoc = MessageService.ReceiverMessage(config.ConfigList[messageQueueSetting.BusinessName].QueueName);
                            if (receiverMessageXmlDoc != null)
                                msg = msgReceiver.TranslateToContentMessage(receiverMessageXmlDoc);
                        }
                        if (msg != null)
                        {
                            string contentType = string.Format("{0}_{1}", msg.From, msg.ContentType);
                            processer = ProcesserFactory.CreateProcesser(contentType);
                            if (processer == null)
                            {
                                Log.WriteLog("error: not found processer");
                                Thread.Sleep(1);
                                continue;
                            }
                            processer.Processer(msg);
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }


                    }
                    catch (Exception ex)
                    {
                        Log.WriteErrorLog(ex);
                    }
                    finally
                    {
                        msg = null;
                    }
                }
            }
        }

        private void CollectPhoteMessage(MessageQueueSetting messageQueueSetting)
        {
            Dictionary<string, List<ContentMessage>> photoMessagDic = new Dictionary<string, List<ContentMessage>>();
            Dictionary<string, List<int>> idDic = new Dictionary<string, List<int>>();

            while (true && !m_serviceStopping)
            {
                try
                {                    
                    XmlDocument receiverMessageXmlDoc = MessageService.ReceiverMessage(config.ConfigList[messageQueueSetting.BusinessName].QueueName);
                    
                    if (receiverMessageXmlDoc != null)
                    {
                        ContentMessage message = msgReceiver.TranslateToContentMessage(receiverMessageXmlDoc);
                        InsertMessageToDic(photoMessagDic, message, idDic);                        
                    }
                    else
                    {
                        foreach(string contenttype in photoMessagDic.Keys)
                        {
                            foreach (ContentMessage message in photoMessagDic[contenttype])
                            {
                                string contentType = string.Format("{0}_{1}", message.From, message.ContentType);
                                BaseProcesser processer = ProcesserFactory.CreateProcesser(contentType);
                                if (processer == null)
                                {
                                    Log.WriteLog("error: not found processer");
                                    Thread.Sleep(1);
                                    continue;
                                }
                                processer.Processer(message);
                            }                            
                        }
                        idDic.Clear();
                        photoMessagDic.Clear();                        
                        Thread.Sleep(5 * 60 * 1000);
                    }
                }
                catch (Exception e)
                {
                    Log.WriteErrorLog(e.Message);
                }
            }
        }

        private static void InsertMessageToDic(Dictionary<string, List<ContentMessage>> photoMessagDic, ContentMessage message, Dictionary<string, List<int>> idDic)
        {
            
            if (photoMessagDic.ContainsKey(message.ContentType))
            {
                if (!idDic[message.ContentType].Contains(message.ContentId))
                {
                    photoMessagDic[message.ContentType].Add(message);
                    idDic[message.ContentType].Add(message.ContentId);
                }
            }
            else
            {
                photoMessagDic.Add(message.ContentType, new List<ContentMessage> { message });
                idDic.Add(message.ContentType, new List<int> { message.ContentId });//排重字典
            }
        }
    }
}
