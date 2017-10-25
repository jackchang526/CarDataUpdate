using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Config.Tools;
using System.IO;
using System.Xml;
using BitAuto.CarDataUpdate.DataProcesser;
using BitAuto.Utils;
using BitAuto.CarDataUpdate.Common.Interface;
using BitAuto.CarDataUpdate.Common.Model;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using BitAuto.CarDataUpdate.DataProcesser.Services;

namespace BitAuto.CarDataUpdate.Tools
{
    /// <summary>
    /// 定时服务控制类
    /// </summary>
    public class Controller
    {
        public event LogHandler Log;
        private ToolsSettings _ToolsSettings;
        private string[] funcArgs;

        public Controller()
        {
            _ToolsSettings = (ToolsSettings)System.Configuration.ConfigurationManager.GetSection("ToolsSettings");
        }

        private void Execute(List<string> para, string threadName)
        {
            var type = GetType();
            foreach (var item in para)
            {
                try
                {
                    type.InvokeMember(item,
                        BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.DeclaredOnly |
                        BindingFlags.Instance,
                        null, this, null);
                }
                catch (Exception ex)
                {
                    OnLog(string.Format("调用[{0}]时出现异常！error:{1}", item, ex.Message), true);
                    Common.Log.WriteErrorLog(string.Format("调用[{0}]时出现异常！error:{1}", item, ex.Message));
                }
            }
            Common.Log.WriteLog(threadName + " over:" + DateTime.Now.ToShortTimeString());
        }

        /// <summary>
        /// </summary>
        public void Execute(string[] args)
        {
            if (args == null || args.Length <= 0)
            {
                var xmlPath = AppDomain.CurrentDomain.BaseDirectory + "\\Config\\ThreadTask.config";
                if (File.Exists(xmlPath))
                {
                    #region
                    Common.Log.WriteLog("开始 执行定时处理！");
                    var watch = new Stopwatch();
                    watch.Start();

                    var root = XElement.Load(xmlPath);
                    var threadElements = root.Descendants("thread");
                    var threadList = threadElements as XElement[] ?? threadElements.ToArray();

                    var taskList = new Task[root.Descendants("thread").Count()];
                    for (var i = 0; i < taskList.Length; i++)
                    {
                        #region 组织当前线程需要处理的任务列表

                        var threadName = "";
                        if (threadList[i].Attribute("name") != null)
                        {
                            threadName = threadList[i].Attribute("name").Value;
                        }
                        var tasks = threadList[i].Descendants("task");
                        var paraList = (from task in tasks where task != null select task.Value).ToList();

                        #endregion

                        Common.Log.WriteLog(threadName + " start:" + DateTime.Now.ToShortTimeString());
                        taskList[i] = Task.Factory.StartNew(delegate { Execute(paraList, threadName); });
                    }
                    Task.WaitAll(taskList);
                    watch.Stop();
                    Common.Log.WriteLog("This process takes :" + watch.Elapsed);
                    OnLog("开始 结束定时处理！", true);

                    #endregion
                }
            }
            else
            {
                var functionName = args[0].Trim('/');
                if (string.IsNullOrEmpty(functionName))
                {
                    OnLog("没有方法名！", true);
                    return;
                }
                Type type = GetType();
                MethodInfo methodInfo = type.GetMethod(functionName);
                if (methodInfo == null)
                {
                    OnLog("没有方法名！", true);
                    return;
                }
                //ParameterInfo[] parameters = methodInfo.GetParameters();
                //if (parameters.Length != args.Length - 1)
                //{
                //    OnLog("参数个数不同！", true);
                //    return;
                //}
                funcArgs = new string[args.Length - 1];
                for (var i = 0; i < funcArgs.Length; i++)
                {
                    funcArgs[i] = args[i + 1].Trim('/');
                }
                try
                {
                    type.InvokeMember(functionName,
                        BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.DeclaredOnly |
                        BindingFlags.Instance,
                        null, this, null);
                }
                catch (Exception exp)
                {
                    OnLog(string.Format("调用[{0}]是出现异常！error:{1}", string.Join(",", args), exp.Message), true);
                    Common.Log.WriteErrorLog(exp);
                }
            }
        }

        #region 口碑数据生成

        /// <summary>
        /// 取子品牌油耗区间(点评) modified by sk 2016-12-26
        /// </summary>
        public void GenerateAllSerialFuel()
        {
            //try
            //{
            //    Common.Log.WriteLog("更新子品牌油耗区间开始...");
            //    string xmlFile = Path.Combine(CommonData.CommonSettings.SavePath, "AllSerialFuel.xml");
            //    XmlDocument xmlDoc = new XmlDocument();
            //    xmlDoc.Load(CommonData.CommonSettings.SerialYouHaoRangeNewUrl);
            //    CommonFunction.SaveXMLDocument(xmlDoc, xmlFile);
            //    Common.Log.WriteLog("更新子品牌油耗区间结束。");
            //}
            //catch (System.Exception ex)
            //{
            //    Common.Log.WriteErrorLog(ex);
            //}
        }

        #endregion

        #region 根据图库接口生成静态文件

        /// <summary>
        /// 根据图库接口生成静态文件
        /// </summary>
        public void GeneratePhotoImage()
        {
            PhotoImageService photo = new PhotoImageService();
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool serialSuccess = Int32.TryParse(funcArgs[0], out serialId);
                if (serialSuccess && serialId > 0)
                {
                    OnLog("子品牌：" + serialId, true);
                    photo.SerialColor(serialId);
                    photo.SerialColorAll(serialId);
                    photo.SerialPhotoList(serialId);
                    photo.SerialPhotoCompare(serialId);
                    photo.SerialClass(serialId);
                    //photo.SerialStandardImage(serialId);
                    photo.SerialFocusImage(serialId);
                    //photo.SerialColorCount(serialId);
                    //photo.SerialPhotoHtml(serialId);
                    photo.SerialPhotoHtmlNew(serialId);
                    photo.SerialPositionImage(serialId);
                    photo.SerialColorImage(serialId);
                    photo.SerialElevenImage(serialId);
                    photo.SerialDefaultCarFillImage(serialId);
                    photo.SerialReallyColorImage(serialId);
                    photo.SerialOfficalImage(serialId);
                    photo.SerialBrandFourthStageImage(serialId); //生成第四极图片
                    //photo.SerialThreeStandardImage(serialId);
                    //photo.SerialYearColorUrl(serialId);
                    photo.SerialYearFocusImage(serialId, 0);
                    photo.SerialSlidePageImage(serialId);//子品牌幻灯页图片
                }
                if (funcArgs.Length <= 1) return;
                int carId = 0;
                bool carSuccess = Int32.TryParse(funcArgs[1], out carId);
                if (carSuccess && carId > 0)
                {
                    CarEntity car = CommonData.GetCarDataById(carId);
                    if (car != null)
                    {
                        OnLog(string.Format("子品牌:{0} 车型:{1} 年款:{2}", car.CsId, car.CarId, car.Year), true);
                        if (car.Year > 0)
                        {
                            photo.SerialYear(car.CsId, car.Year);
                            //photo.SerialYearFocusImage(car.CsId, car.Year);
                            //photo.SerialYearPhotoHtml(car.CsId, car.Year);
                            photo.SerialYearPhotoHtmlNew(car.CsId, car.Year);
                        }
                        //photo.CarStandardImage(car.CsId, car.CarId);
                        //photo.CarFocusImage(car.CsId, car.CarId, car.Year);
                        photo.SerialDefaultCarImage(car.CsId, car.CarId);
                        //photo.CarPhotoHtml(car.CarId);
                        photo.CarPhotoHtmlNew(car.CarId);
                        //保存车款实拍图
                        photo.SerialCarReallyImage(car.CsId, car.CarId);
                    }
                }
                return;
            }

            OnLog("开始时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), true);
            OnLog("start  SerialCompare 对比子品牌", true);
            photo.SerialCompare();
            OnLog("start  SerialCover 子品牌封面", true);
            photo.SerialCover();
            OnLog("start  SerialCoverImageAndCount 非白底封面", true);
            //photo.SerialCoverWithout();
            photo.SerialCoverImageAndCount();
            OnLog("start  CarCoverImage 车型封面", true);
            photo.CarCoverImage();
            OnLog("start  SerialDefaultCar 取子品牌默认车型", true);
            photo.SerialDefaultCar();
            OnLog("start  CarImageCount 生成车款所有车款的数量", true);
            photo.CarImageCount();
            OnLog("开始子品牌：", true);
            Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
            foreach (SerialInfo serial in dict.Values)
            {
                OnLog("子品牌：" + serial.ShowName, true);
                photo.SerialColor(serial.Id);
                photo.SerialColorAll(serial.Id);
                photo.SerialPhotoList(serial.Id);
                //modified by sk 接口改为按车款获取
                //photo.SerialPhotoCompare(serial.Id);
                photo.SerialClass(serial.Id);
                //photo.SerialStandardImage(serial.Id);
                photo.SerialFocusImage(serial.Id);
                //photo.SerialColorCount(serial.Id);
                //photo.SerialPhotoHtml(serial.Id);
                photo.SerialPhotoHtmlNew(serial.Id);
                photo.SerialPositionImage(serial.Id);
                photo.SerialColorImage(serial.Id);
                photo.SerialElevenImage(serial.Id);
                photo.SerialDefaultCarFillImage(serial.Id);
                photo.SerialReallyColorImage(serial.Id);
                photo.SerialOfficalImage(serial.Id); //子品牌官方图
                photo.SerialBrandFourthStageImage(serial.Id); //生成第四极图片
                //photo.SerialThreeStandardImage(serial.Id);
                //photo.SerialYearColorUrl(serial.Id);
                photo.SerialYearFocusImage(serial.Id, 0);
                photo.SerialSlidePageImage(serial.Id);//子品牌幻灯页图片
            }


            OnLog("已经生成子品牌：" + dict.Count, true);
            OnLog("开始子品牌车型年款：", true);
            Dictionary<int, CarEntity> dictCar = CommonData.GetAllCarData();
            foreach (CarEntity car in dictCar.Values)
            {
                OnLog(string.Format("子品牌:{0} 车型:{1} 年款:{2}", car.CsId, car.CarId, car.Year), true);
                if (car.Year > 0)
                {
                    photo.SerialYear(car.CsId, car.Year);
                    //photo.SerialYearFocusImage(car.CsId, car.Year);
                    //photo.SerialYearPhotoHtml(car.CsId, car.Year);
                    photo.SerialYearPhotoHtmlNew(car.CsId, car.Year);
                }
                //photo.CarStandardImage(car.CsId, car.CarId);
                //photo.CarFocusImage(car.CsId, car.CarId, car.Year);
                photo.SerialDefaultCarImage(car.CsId, car.CarId);
                //photo.CarPhotoHtml(car.CarId);
                photo.CarPhotoHtmlNew(car.CarId);
                photo.CarPhotoCompare(car.CarId);
                photo.CarImagesListInfo(car.CsId, car.CarId);
                //保存车款实拍图
                photo.SerialCarReallyImage(car.CsId, car.CarId);
            }
            OnLog("已经生成子品牌车型：" + dictCar.Count, true);
            OnLog("结束时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), true);
        }

        #endregion
        /*
        /// <summary>
        /// 共同的方法执行
        /// </summary>
        public void CommonGenrate()
        {
            //易车惠品牌旗舰店
            GenerateFlagShipXML();
            //生成报价区间
            GeneratePriceRangeXML();

            //高级选车工具全量更新数据
            GenerateUpdateCarDataForSelectToolV2();

            //子品牌综述页 车款列表
            //GenerateSerialCarList();
            //生成主品牌视频块
            //GenerateMasterVideo();
            //生成品牌视频块
            //GenerateBrandVideo();
            GenerateSerialSUVPramater();
            //子品牌综述页 奖项块
            GenerateSerialAward();
            // 易湃接口暂时在这每天只需1次
            EPProcesserList();

            //生成报价区间 子品牌城市排行
            GenerateSerialCityPriceRank();

            //生成问答块HTML
            GenerateDefaultAskHtml();
            GenerateMasterAskHtml();
            GenerateSerialAskHtml();
            ////生成团购url 数据
            //GenerateSerialTurnUrlXml();

            //生成关键报告--内部空间
            //GenerateKongJianHtml();
            GetDetailZoneHTML();

            //生成SUV 销量排行 
            GenerateSUVSaleRankXml();

            //第四级，生成子品牌商配新闻XML
            GenerateSerialCommerceNewsData();

            //生成第四级子品牌文章页面HTML 2015-08-10
            //GenerateH5ArticalHtml();

            //更新口碑印象对应的子品牌到redis
            StoreKoubeiImpressionInRedis();

            //更新口碑详细评分
            UpdateKoubeiRaingDetail();

            //生成车款 口碑 xml文件
            GetCarKoubeiHtml();

            //子品牌的竞品口碑排名
            //GetSerialCompetitiveKoubeiHtml();

            GenerateCarCompareXml();
        }
		*/
        /*
		public void ImportVideoData()
		{
			try
			{
				string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos.txt");
				if (!File.Exists(fileName)) { Common.Log.WriteLog("视频ID文件不存在。"); return; }
				using (StreamReader sr = new StreamReader(fileName))
				{
					string sLine = "";
					while (sLine != null)
					{
						sLine = sr.ReadLine();
						string vGuid = sLine;
						Guid guid = Guid.Empty;
						if (!string.IsNullOrEmpty(vGuid) && Guid.TryParse(vGuid, out guid))
						{
							Common.Log.WriteLog("开始导入视频数据 视频ID:" + vGuid);
							Common.Services.VideoService.UpdateVideo(vGuid);
						}
						//string[] arr = sLine.Split(' ');
						//int videoId = ConvertHelper.GetInteger(arr[0]);
						//Guid videoGuid = Guid.Empty;
						//if (Guid.TryParse(arr[1], out videoGuid))
						//{
						//    Common.Log.WriteLog("开始更新VideoGuid 视频ID:" + videoId);
						//    Common.Repository.VideoRepository.UpdateVideoGuidByVideoId(videoId, arr[1]);
						//}
						//else { Common.Log.WriteLog("更新VideoGuid出错 视频ID=:" + videoId + " VideoGuid=:" + arr[1]); }
					}
				}
			}
			catch (Exception ex)
			{
				Common.Log.WriteLog(ex.ToString());
			}
		}
         */
        /// <summary>
        /// 主品牌视频块 生成
        /// </summary>
        public void GenerateMasterVideo()
        {
            try
            {
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int mId = ConvertHelper.GetInteger(funcArgs[0]);
                    List<int> masterList = null;
                    if (mId == 0)
                        masterList = BitAuto.CarDataUpdate.Common.Services.MasterBrandService.GetMasterBrandIdList();//CommonData.BrandMasterBrandDic.Select(p => p.Value).Distinct().ToList();
                    else
                    {
                        masterList = new List<int>();
                        masterList.Add(mId);
                    }
                    BitAuto.CarDataUpdate.HtmlBuilder.MasterVideoHtmlBuilder mvhb = new HtmlBuilder.MasterVideoHtmlBuilder();
                    Common.Log.WriteLog("更新品牌视频块 start");
                    foreach (int masterId in masterList)
                    {
                        Common.Log.WriteLog("更新品牌视频块 masterId=" + masterId);
                        mvhb.BuilderDataOrHtml(masterId);
                    }
                    Common.Log.WriteLog("更新品牌视频块 end");
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /// <summary>
        /// 品牌视频块 生成
        /// </summary>
        public void GenerateBrandVideo()
        {
            try
            {
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int mId = ConvertHelper.GetInteger(funcArgs[0]);
                    List<int> brandList = null;
                    if (mId == 0)
                        brandList = BitAuto.CarDataUpdate.Common.Services.BrandService.GetBrandIdList();//CommonData.BrandMasterBrandDic.Select(p => p.Key).Distinct().ToList();
                    else
                    {
                        brandList = new List<int>();
                        brandList.Add(mId);
                    }
                    BitAuto.CarDataUpdate.HtmlBuilder.BrandVideoHtmlBuilder bvhb = new HtmlBuilder.BrandVideoHtmlBuilder();
                    Common.Log.WriteLog("更新品牌视频块 start");
                    foreach (int brandId in brandList)
                    {
                        Common.Log.WriteLog("更新品牌视频块 brandId=" + brandId);
                        bvhb.BuilderDataOrHtml(brandId);
                    }
                    Common.Log.WriteLog("更新品牌视频块 end");
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
        /*
		#region 子品牌综述页 车款列表
		/// <summary>
		/// 生成子品牌综述页 车款列表 html
		/// 朗逸：2370 科鲁兹：2608 起亚K2：3398 长城C30: 3023 凯越：2388 英菲尼迪M：2196
		/// </summary>
		public void GenerateSerialCarList()
		{
			try
			{
				if (funcArgs != null && funcArgs.Length > 0)
				{
					string serialIds = funcArgs[0];
					string[] serialIdArray = serialIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string serialId in serialIdArray)
					{
						Serial.MakeSerialCarListHtml(ConvertHelper.GetInteger(serialId));
					}
					return;
				}
				int[] specialSerialIdsArray = { 2370, 2608, 3398, 3023, 2388, 2196 };
				Array.ForEach(specialSerialIdsArray, serialId => { Serial.MakeSerialCarListHtml(serialId); });
				//Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
				//foreach (SerialInfo serial in dict.Values)
				//{
				//    Serial.MakeSerialCarListHtml(serial.Id);
				//}
			}
			catch (Exception ex)
			{
				OnLog(ex.ToString(), true);
			}
		}
		#endregion
		 */

        #region 需要定时执行方法

        /// <summary>
        /// 生成主品牌 品牌旗舰店Url（易车惠）
        /// </summary>
        public void GenerateFlagShipXML()
        {
            try
            {
                OnLog("start 生成品牌旗舰店Url", true);
                string yicheHuiFlagShip = ConfigurationManager.AppSettings["YicheHuiFlagShip"];
                string content = CommonFunction.GetContentByUrl(yicheHuiFlagShip, "utf-8");
                string filePath = Path.Combine(Common.CommonData.CommonSettings.SavePath, "Yichehui\\FlagshipUrl.json");
                CommonFunction.SaveFileContent(content, filePath, "utf-8");
                OnLog("end 生成品牌旗舰店Url", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// 生成 子品牌 、车款 的报价区间 （易湃）
        /// </summary>
        public void GeneratePriceRangeXML()
        {
            //生成子品牌报价区间
            OnLog("开始生成子品牌报价区间", true);
            var serialService = new SerialService();
            serialService.GenerateSerialPriceRange();
            //生成车款报价区间
            OnLog("开始生成车款报价区间", true);
            var carService = new CarService();
            carService.GenerateCarPriceRange();
        }

        /*
		/// <summary>
		/// 生成子品牌 团购数据文件
		/// </summary>
		public void GenerateSerialTurnUrlXml()
		{
			try
			{
				var tuan = new SerialTuanService();
				tuan.GenerateSerialTuanURL();
			}
			catch (Exception ex)
			{
				Common.Log.WriteErrorLog(ex);
			}
		}*/

        /// <summary>
        /// 生成SUV销量排行
        /// </summary>
        public void GenerateSUVSaleRankXml()
        {
            try
            {
                OnLog("start 生成SUV销量排行", true);
                var suv = new SUVSaleRankService();
                suv.Generate();
                OnLog("end 生成SUV销量排行", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// 生成 报价区间 子品牌城市排行 xml
        /// </summary>
        public void GenerateSerialCityPriceRank()
        {
            try
            {
                SerialCityPriceRank.GenerateSerialCityPriceRank();
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// 生成综述页 SUV级别 参数块内容
        /// </summary>
        public void GenerateSerialSUVPramater()
        {
            try
            {
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始更新综述页SUV参数块", true);
                BitAuto.CarDataUpdate.HtmlBuilder.SerialSUVPramatersHtmlBuilder suvBuilder =
                    new HtmlBuilder.SerialSUVPramatersHtmlBuilder();
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int serialId = 0;
                    serialId = ConvertHelper.GetInteger(funcArgs[0]);
                    OnLog("更新综述页SUV参数块,serialId:" + serialId, true);
                    suvBuilder.BuilderDataOrHtml(serialId);
                    OnLog("更新综述页SUV参数块结束", true);
                    return;
                }
                foreach (SerialInfo serial in dict.Where(p => p.Value.CarLevel == 424).Select(p => p.Value))
                {
                    int serialId = serial.Id;
                    OnLog("更新综述页SUV参数块,serialId:" + serialId, true);
                    suvBuilder.BuilderDataOrHtml(serialId);
                }
                OnLog("更新综述页SUV参数块结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("更新综述页SUV参数块异常：" + ex.ToString());
            }
        }

        public void GenerateSerialAward()
        {
            try
            {
                var serialAwardBuilder = new HtmlBuilder.SerialAwardHtmlBuilder();
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int serialId = 0;
                    serialId = ConvertHelper.GetInteger(funcArgs[0]);
                    OnLog("开始更新综述页奖项块,serialId:" + serialId, true);
                    serialAwardBuilder.BuilderDataOrHtml(serialId);
                    OnLog("更新综述页奖项块结束", true);
                    return;
                }
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始更新综述页奖项块", true);
                foreach (SerialInfo serial in dict.Select(p => p.Value))
                {
                    int serialId = serial.Id;
                    serialAwardBuilder.BuilderDataOrHtml(serialId);
                }
                OnLog("更新综述页奖项块结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("更新综述页奖项块异常：" + ex);
            }
        }

        /// <summary>
        /// 更新子品牌级别排行指数memcache
        /// </summary>
        public void ReWriteIndexSerialLevelRank()
        {
            try
            {
                OnLog("开始更新子品牌级别排行指数memcache", true);
                RewriteMemCache.ReWriteIndexSerialLevelRank();
                OnLog("更新子品牌级别排行指数memcache结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("更新子品牌级别排行指数memcache异常：" + ex.ToString());
            }
        }

        /// <summary>
        /// 更新车型参数memcache
        /// </summary>
        public void RewriteCarMemCache()
        {
            try
            {
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始更新车型参数Memcache", true);
                int carId = 0;
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    carId = ConvertHelper.GetInteger(funcArgs[0]);
                    OnLog("更新车型参数Memcache,车型ID:" + carId, true);
                    RewriteMemCache.RewriteCarCompareMemCache(carId);
                    OnLog("更新车型参数Memcache结束", true);
                    return;
                }
                foreach (SerialInfo serial in dict.Values)
                {
                    int serialId = serial.Id;
                    Dictionary<int, CarEntity> dictCar = CommonData.GetCarDataBySerialId(serialId);
                    foreach (CarEntity car in dictCar.Values)
                    {
                        OnLog("更新车型参数Memcache,车型ID:" + car.CarId, true);
                        RewriteMemCache.RewriteCarCompareMemCache(car.CarId);
                    }
                }
                OnLog("更新车型参数Memcache结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("更新车型参数Memcache异常：" + ex.ToString());
            }
        }

        /// <summary>
        /// 生成通用导航头
        /// </summary>
        public void GenerateCommonNavigation()
        {
            try
            {
                CommonNavigation nav = new CommonNavigation();
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始生成通用导航头", true);

                int sId = 0;
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    if (Int32.TryParse(funcArgs[0], out sId))
                    {
                    }
                }

                foreach (SerialInfo serial in dict.Values)
                {
                    // 如果传了子品牌ID，则不是此子品牌的不生成
                    if (sId > 0 && serial.Id != sId)
                    {
                        continue;
                    }

                    int serialId = serial.Id;
                    OnLog("正在生成通用导航头,子品牌：" + serialId, true);
                    //nav.GenerateSerialNavigation(serialId);
                    //nav.GenerateSerialBarInfo(serialId);

                    nav.GenerateSerialNavigationV2(serialId);

                    nav.GenerateSerialNavigationM(serialId);
                    Dictionary<int, CarEntity> dictCar = CommonData.GetCarDataBySerialId(serialId);
                    foreach (CarEntity car in dictCar.Values)
                    {
                        // nav.GenerateCarNavigation(car.CarId);
                        nav.GenerateCarNavigationV2(car.CarId);
                    }
                }
                OnLog("生成通用导航头结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("生成通用导航头异常：" + ex.ToString());
            }
        }

        /// <summary>
        /// 该方法用来将图库接口数以XML格式保存在本地，不需要每天执行，不要配置在ThreadTask.config文件中，需要是单独执行 20170930
        /// </summary>
        public void SaveSerialCarReallyImage()
        {
            try
            {
                PhotoImageService photo = new PhotoImageService();
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始保存图片数据", true);

                int sId = 0;
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    if (Int32.TryParse(funcArgs[0], out sId))
                    {
                    }
                }
                foreach (SerialInfo serial in dict.Values)
                {
                    // 如果传了子品牌ID，则不是此子品牌的不生成
                    if (sId > 0 && serial.Id != sId)
                    {
                        continue;
                    }
                    int serialId = serial.Id;
                    OnLog("开始保存图片数据,子品牌：" + serialId, true);
                    
                    Dictionary<int, CarEntity> dictCar = CommonData.GetCarDataBySerialId(serialId);
                    foreach (CarEntity car in dictCar.Values)
                    {
                        photo.SerialCarReallyImage(serialId,car.CarId);
                    }
                }
                OnLog("保存图片数据结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("保存图片数据异常：" + ex.ToString());
            }
        }
        /// <summary>
        /// 仅供上线使用生成综述页焦点区图片
        /// </summary>
        public void SaveSerialFocusAndSlideImage()
        {
            try
            {
                PhotoImageService photo = new PhotoImageService();
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始生成图片数据", true);

                int sId = 0;
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    if (Int32.TryParse(funcArgs[0], out sId))
                    {
                    }
                }
                foreach (SerialInfo serial in dict.Values)
                {
                    // 如果传了子品牌ID，则不是此子品牌的不生成
                    if (sId > 0 && serial.Id != sId)
                    {
                        continue;
                    }
                    int serialId = serial.Id;

                    OnLog("开始生成焦点区图片,子品牌：" + serialId, true);
                    photo.SerialFocusImage(serialId);
                    photo.SerialSlidePageImage(serialId);//子品牌幻灯页图片;
                }
                OnLog("保存图片数据结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("保存图片数据异常：" + ex.ToString());
            }
        }

        /// <summary>
        /// 生成1200版 互联互通导航
        /// </summary>
        public void GenerateCommonNavigationV2()
        {
            try
            {
                CommonNavigation nav = new CommonNavigation();
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始生成通用导航头", true);

                int sId = 0;
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    if (Int32.TryParse(funcArgs[0], out sId))
                    {
                    }
                }

                foreach (SerialInfo serial in dict.Values)
                {
                    // 如果传了子品牌ID，则不是此子品牌的不生成
                    if (sId > 0 && serial.Id != sId)
                    {
                        continue;
                    }

                    int serialId = serial.Id;
                    OnLog("正在生成通用导航头,子品牌：" + serialId, true);
                    nav.GenerateSerialNavigationV2(serialId);
                    Dictionary<int, CarEntity> dictCar = CommonData.GetCarDataBySerialId(serialId);
                    foreach (CarEntity car in dictCar.Values)
                    {
                        nav.GenerateCarNavigationV2(car.CarId);
                    }
                }
                OnLog("生成通用导航头结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("生成通用导航头异常：" + ex.ToString());
            }
        }

        public void GenerateCommonNavigationM()
        {
            try
            {
                CommonNavigation nav = new CommonNavigation();
                Dictionary<int, SerialInfo> dict = CommonData.SerialDic;
                OnLog("开始生成M站通用导航头", true);

                int sId = 0;
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    if (Int32.TryParse(funcArgs[0], out sId))
                    {
                    }
                }

                foreach (SerialInfo serial in dict.Values)
                {
                    // 如果传了子品牌ID，则不是此子品牌的不生成
                    if (sId > 0 && serial.Id != sId)
                    {
                        continue;
                    }

                    int serialId = serial.Id;
                    OnLog("正在生成M站通用导航头,子品牌：" + serialId, true);
                    nav.GenerateSerialNavigationM(serialId);

                }
                OnLog("生成M站通用导航头结束", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog("生成M站通用导航头异常：" + ex.ToString());
            }
        }
        /// <summary>
        /// 获取城市信息
        /// </summary>
        [DescriptionAttribute("GetCityContent 说明：获取城市信息")]
        public void GetCityContent()
        {
            CityProcesser cityService = new CityProcesser(0);
            cityService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetCityContent......", true);
            cityService.GetContent();
        }

        /// <summary>
        ///无参数时执行获取全部
        /// </summary>
        [DescriptionAttribute("GetContent 说明：无参数时执行获取全部")]
        public void GetContent()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetContent......", true);
            getter.GetContent();
        }

        /// <summary>
        /// 获取车型级别的所有新闻内容
        /// </summary>
        //[DescriptionAttribute("GetAllLevelNews 说明：获取车型级别的所有新闻内容")]
        //public void GetAllLevelNews()
        //{
        //    ContentGetter getter = new ContentGetter();
        //    getter.Log += new LogHandler(getter_Log);
        //    OnLog("GetAllLevelNews方法开始执行......", true);
        //    getter.GetAllLevelNews();
        //}
        /// <summary>
        /// 获取所有子品牌的基本信息
        /// </summary>
        [DescriptionAttribute("UpdateBrandTree 说明：获取所有子品牌的基本信息")]
        public void UpdateBrandTree()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec UpdateBrandTree......", true);
            getter.UpdateBrandTree();
        }

        /*
		/// <summary>
		/// 获取车型级别的所有视频内容
		/// </summary>
		[DescriptionAttribute("GetAllLeveVideos 说明：获取车型级别的所有视频内容")]
		public void GetAllLeveVideos()
		{
			ContentGetter getter = new ContentGetter();
			getter.Log += new LogHandler(getter_Log);
			OnLog("start exec GetAllLeveVideos......", true);
			getter.GetAllLeveVideos();
		}
        
		/// <summary>
		/// 获取车型级别的油耗与养车费用内容
		/// </summary>
		[DescriptionAttribute("GetAllLeveCost 说明：获取车型级别的油耗与养车费用内容")]
		public void GetAllLeveCost()
		{
			ContentGetter getter = new ContentGetter();
			getter.Log += new LogHandler(getter_Log);
			OnLog("start exec GetAllLeveCost......", true);
			getter.GetAllLeveCost();
		}
        
		/// <summary>
		/// 获取图片的ID与Url对照内容
		/// </summary>
		[DescriptionAttribute("GetImgUrl 说明：获取图片的ID与Url对照内容")]
		public void GetImgUrl()
		{
			ContentGetter getter = new ContentGetter();
			getter.Log += new LogHandler(getter_Log);
			OnLog("start exec GetImgUrl......", true);
			getter.GetImgUrl();
		}
        
		/// <summary>
		/// 获取所有子品牌的点评数量
		/// </summary>
		[DescriptionAttribute("GetAllSerialDianpingCount 说明：获取所有子品牌的点评数量")]
		public void GetAllSerialDianpingCount()
		{
			ContentGetter getter = new ContentGetter();
			getter.Log += new LogHandler(getter_Log);
			OnLog("start exec GetAllSerialDianpingCount......", true);
			getter.GetAllSerialDianpingCount();
		}
		 *  * */

        /// <summary>
        /// 更新城市列表
        /// </summary>
        [DescriptionAttribute("UpdateCityList 说明：更新城市列表")]
        public void UpdateCityList()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec UpdateCityList......", true);
            getter.UpdateCityList();
        }

        /// <summary>
        /// 更新子品牌论坛URL列表
        /// </summary>
        [DescriptionAttribute("GetAllSerialForumUrl 说明：更新子品牌论坛URL列表")]
        public void GetAllSerialForumUrl()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetAllSerialForumUrl......", true);
            getter.GetAllSerialForumUrl();
        }

        /// <summary>
        /// 更新所有车型的油耗信息
        /// </summary>
        [DescriptionAttribute("GetAllCarFuel 说明：更新所有车型的油耗信息")]
        public void GetAllCarFuel()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetAllCarFuel......", true);
            getter.GetAllCarFuel();
        }

        /// <summary>
        /// 统计所有子品牌的计划购买的人名
        /// </summary>
        //[DescriptionAttribute("GetSerialsIntensionPersion 说明：统计所有子品牌的计划购买的人名")]
        //public void GetSerialsIntensionPersion()
        //{
        //    ContentGetter getter = new ContentGetter();
        //    getter.Log += new LogHandler(getter_Log);
        //    OnLog("GetSerialsIntensionPersion方法开始执行......", true);
        //    getter.GetSerialsIntensionPersion();
        //}
        /// <summary>
        /// 获取所有子品牌的区域车型页的行情与促销新闻
        /// </summary>
        [DescriptionAttribute("UpdateSerialCityNews 说明：获取所有子品牌的区域车型页的行情与促销新闻")]
        public void UpdateSerialCityNews()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec UpdateSerialCityNews......", true);
            getter.UpdateSerialCityNews();
        }

        /// <summary>
        /// 更新车展新闻 2017-04-25注释掉
        /// </summary>
        //[DescriptionAttribute("GetCarShowTopNews 说明：更新车展新闻")]
        //public void GetCarShowTopNews()
        //{
        //    ContentGetter getter = new ContentGetter();
        //    getter.Log += new LogHandler(getter_Log);
        //    OnLog("start exec GetCarShowTopNews......", true);
        //    getter.GetCarShowTopNews();
        //}

        /// <summary>
        /// 更新车展品牌的默认图
        /// </summary>
        [DescriptionAttribute("GetGuangzhouCarshowBrandImage 说明：更新车展品牌的默认图")]
        public void GetGuangzhouCarshowBrandImage()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetGuangzhouCarshowBrandImage......", true);
            getter.GetGuangzhouCarshowBrandImage();
        }

        /// <summary>
        /// 更新车展子品牌的点评信息
        /// </summary>
        [DescriptionAttribute("GetCarshowSerialDianping 说明：更新车展子品牌的点评信息")]
        public void GetCarshowSerialDianping()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetCarshowSerialDianping......", true);
            getter.GetCarshowSerialDianping();
        }

        /*
		/// <summary>
		/// 统计所有子品牌的各类新闻的数量
		/// </summary>
		[DescriptionAttribute("StatisSerialsNewsCount 说明：统计所有子品牌的各类新闻的数量")]
		public void StatisSerialsNewsCount()
		{
			ContentGetter getter = new ContentGetter();
			getter.Log += new LogHandler(getter_Log);
			OnLog("start exec StatisSerialsNewsCount......", true);
			getter.StatisSerialsNewsCount();
		}*/

        /// <summary>
        /// 获取所有子品牌的视频数量
        /// </summary>
        [DescriptionAttribute("GetAllSerialVideoCount 说明：获取所有子品牌的视频数量")]
        public void GetAllSerialVideoCount()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetAllSerialVideoCount......", true);
            // getter.GetAllSerialVideoCount();
        }

        /// <summary>
        /// 获取所有子品牌的好中差的点评,后面可选[/<id>],id为品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetSerialDianping 说明：获取所有子品牌的好中差的点评,后面可选[/<id>],id为品牌ID，如果取全部，id=0")]
        public void GetSerialDianping()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialDianping......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                    getter.GetSerialDianping(serialId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 更新品牌图片,后面可选[/<id>],id为品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetBrandImages 说明：更新品牌图片,后面可选[/<id>],id为品牌ID，如果取全部，id=0")]
        public void GetBrandImages()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetBrandImages......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int brandId = 0;
                bool success = Int32.TryParse(funcArgs[0], out brandId);
                if (success)
                    getter.GetBrandImages(brandId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 更新品牌的答疑内容,后面可选[/<id>],id为品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetBrandAskEntries 说明：更新品牌的答疑内容,后面可选[/<id>],id为品牌ID，如果取全部，id=0")]
        public void GetBrandAskEntries()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetBrandAskEntries......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int brandId = 0;
                bool success = Int32.TryParse(funcArgs[0], out brandId);
                if (success)
                    getter.GetBrandAskEntries(brandId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 更新品牌论坛信息,后面可选[/<id>],id为品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetBrandForumInfo 说明：更新品牌论坛信息,后面可选[/<id>],id为品牌ID，如果取全部，id=0")]
        public void GetBrandForumInfo()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetBrandForumInfo......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int brandId = 0;
                bool success = Int32.TryParse(funcArgs[0], out brandId);
                if (success)
                    getter.GetBrandForumInfo(brandId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        ///// <summary>
        ///// 更新品牌二手车信息,后面可选[/<id>],id为品牌ID，如果取全部，id=0
        ///// </summary>
        //[DescriptionAttribute("GetBrandUsecar 说明：更新品牌二手车信息,后面可选[/<id>],id为品牌ID，如果取全部，id=0")]
        //public void GetBrandUsecar()
        //{
        //    ContentGetter getter = new ContentGetter();
        //    getter.Log += new LogHandler(getter_Log);
        //    OnLog("start exec GetBrandUsecar......", true);
        //    if (funcArgs != null && funcArgs.Length > 0)
        //    {
        //        int brandId = 0;
        //        bool success = Int32.TryParse(funcArgs[0], out brandId);
        //        if (success)
        //            getter.GetBrandUsecar(brandId);
        //        else
        //            Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
        //    }
        //}
        /// <summary>
        /// 更新厂商新闻,后面可选[/<id>],id为厂商ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetProducerNews 说明：更新厂商新闻,后面可选[/<id>],id为厂商ID，如果取全部，id=0")]
        public void GetProducerNews()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetProducerNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int pId = 0;
                bool success = Int32.TryParse(funcArgs[0], out pId);
                if (success)
                    getter.GetProducerNews(pId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /*
		/// <summary>
		/// 更新主品牌答疑,后面可选[/<id>],id为主品牌ID，如果取全部，id=0
		/// </summary>
		[DescriptionAttribute("GetMasterBrandAskEntries 说明：更新主品牌答疑,后面可选[/<id>],id为主品牌ID，如果取全部，id=0")]
		public void GetMasterBrandAskEntries()
		{
			ContentGetter getter = new ContentGetter();
			getter.Log += new LogHandler(getter_Log);
			OnLog("start exec GetMasterBrandAskEntries......", true);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int mId = 0;
				bool success = Int32.TryParse(funcArgs[0], out mId);
				if (success)
					getter.GetMasterBrandAskEntries(mId);
				else
					Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
			}
		}*/
        ///// <summary>
        ///// 更新品牌二手车信息,后面可选[/<id>],id为品牌ID，如果取全部，id=0
        ///// </summary>
        //[DescriptionAttribute("GetMasterbrandUsecar 说明：更新品牌二手车信息,后面可选[/<id>],id为品牌ID，如果取全部，id=0")]
        //public void GetMasterbrandUsecar()
        //{
        //    ContentGetter getter = new ContentGetter();
        //    getter.Log += new LogHandler(getter_Log);
        //    OnLog("start exec GetMasterbrandUsecar......", true);
        //    if (funcArgs != null && funcArgs.Length > 0)
        //    {
        //        int mId = 0;
        //        bool success = Int32.TryParse(funcArgs[0], out mId);
        //        if (success)
        //            getter.GetMasterbrandUsecar(mId);
        //        else
        //            Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
        //    }
        //}
        /// <summary>
        ///更新主品牌论坛信息,后面可选[/<id>],id为主品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetMasterbrandForumInfo 说明：更新主品牌论坛信息,后面可选[/<id>],id为主品牌ID，如果取全部，id=0")]
        public void GetMasterbrandForumInfo()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetMasterbrandForumInfo......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int mId = 0;
                bool success = Int32.TryParse(funcArgs[0], out mId);
                if (success)
                    getter.GetMasterbrandForumInfo(mId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 更新子品牌焦点图片,后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetSerialFocusImage 说明：更新子品牌焦点图片,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialFocusImage()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialFocusImage......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                    getter.GetSerialFocusImage(serialId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 更新子品牌的视频内容，后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetSerialVideo 说明：更新子品牌的视频内容，后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialVideo()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialVideo......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    getter.GetSerialVideo(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                getter.GetSerialVideo(0);
            }
        }

        [DescriptionAttribute("GetSerialVideo 说明：更新子品牌的视频内容，后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialKoubeiHtml()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialKoubeiHtml......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    getter.GenerateSerialKoubeiHtml(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                getter.GenerateSerialKoubeiHtml(0);
            }
        }


        [DescriptionAttribute("GetCarKoubeiHtml 说明：更新车款的口碑内容，后面可选[/<id>],id为车款ID，如果取全部，id=0")]
        public void GetCarKoubeiHtml()
        {
            var getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetCarKoubeiHtml......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int carId = 0;
                bool success = Int32.TryParse(funcArgs[0], out carId);
                if (success)
                {
                    getter.GenerateCarKoubeiHtml(carId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                getter.GenerateCarKoubeiHtml(0);
            }
        }

        /// <summary>
        /// 更新子品牌论坛信息,后面可选[/<id>],id为主品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetSerialForumSubject 说明：更新子品牌论坛信息,后面可选[/<id>],id为主品牌ID，如果取全部，id=0")]
        public void GetSerialForumSubject()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialForumSubject......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    getter.GetSerialForumSubject(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /*
		/// <summary>
		/// 更新子品牌的城市行情新闻，后面可选[/<id>],id为子品牌ID，如果取全部，id=0
		/// </summary>
		[DescriptionAttribute("GetSerialCityHangqingNews 说明：更新子品牌的城市行情新闻，后面可选[/<id>],第一个参数id为子品牌ID，第二个参数id为城市ID，如果取全部，id=0,")]
		public void GetSerialCityHangqingNews()
		{
			ContentGetter getter = new ContentGetter();
			getter.Log += new LogHandler(getter_Log);
			OnLog("start exec GetSerialCityHangqingNews......", true);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int serialId = 0;
				int cityId = 0;
				bool success = Int32.TryParse(funcArgs[0], out serialId);
				Int32.TryParse(funcArgs[1], out cityId);
				if (success)
					getter.GetSerialCityHangqingNews(serialId, cityId);
				else
					Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
			}
		}*/

        /// <summary>
        /// 更新子品牌的热点新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetSerialHotNews 说明：更新子品牌的热点新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialHotNews()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialHotNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                    getter.GetSerialHotNews(serialId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 更新子品牌答疑,后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetSerialAskEntries 说明：更新子品牌答疑,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialAskEntries()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialAskEntries......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                    getter.GetSerialAskEntries(serialId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        ///// <summary>
        ///// 更新UCar二手车信息,后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        ///// </summary>
        //[DescriptionAttribute("GetUcarInfo 说明：更新UCar二手车信息,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        //public void GetUcarInfo()
        //{
        //    ContentGetter getter = new ContentGetter();
        //    getter.Log += new LogHandler(getter_Log);
        //    OnLog("start exec GetUcarInfo......", true);
        //    if (funcArgs != null && funcArgs.Length > 0)
        //    {
        //        int serialId = 0;
        //        bool success = Int32.TryParse(funcArgs[0], out serialId);
        //        if (success)
        //            getter.GetUcarInfo(serialId);
        //        else
        //            Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
        //    }
        //}
        /// <summary>
        /// 更新城市新闻信息,后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("StatisticsCityNews 说明：更新城市新闻信息,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void StatisticsCityNews()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec StatisticsCityNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                    getter.StatisticsCityNews(serialId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 从口碑获取子品牌的网友印象及优缺点,后面可选[/<id>],id为子品牌ID，如果取全部，id=0 
        /// </summary>
        [DescriptionAttribute("GetSerialKoubeiImpression 说明：从口碑获取子品牌的网友印象及优缺点,后面可选[/<id>],id为子品牌ID，如果取全部，id=0 ")]
        public void GetSerialKoubeiImpression()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialKoubeiImpression......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int sId = 0;
                bool success = Int32.TryParse(funcArgs[0], out sId);
                if (success)
                    getter.GetSerialKoubeiImpression(sId);
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到用于Tree的子品牌服务
        /// </summary>
        [DescriptionAttribute("GetSerialsNewsCountInTree 说明：得到用于Tree的子品牌服务")]
        public void GetSerialsNewsCountInTree()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialsNewsCountInTree......", true);
            getter.GetSerialsNewsCountInTree();
        }

        /// <summary>
        /// 更新车型左侧树形xml
        /// </summary>
        [DescriptionAttribute("UpdateTreeData 说明：更新车型左侧树形xml")]
        public void UpdateTreeData()
        {
            // 树形导航生成挪到 Auto_Console_BuildJSFile 项目，每天生成5次 6、9、12、15、18
            CarTreeXmlDataGetter getter = new CarTreeXmlDataGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec UpdateTreeData......", true);
            getter.UpdateTreeData();
        }

        /// <summary>
        /// 更新子品牌的城市新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetCityNews 说明：更新子品牌的城市新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetCityNews()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetCityNews......", true);
            GetCityNews(getter, funcArgs[0]);
        }

        /// <summary>
        /// 生成车型按级别与报价分的块
        /// </summary>
        [DescriptionAttribute("GenGroupHtml 说明：生成车型按级别与报价分的块")]
        public void GenGroupHtml()
        {
            CarGroupByLevelAndPrice cgbla = new CarGroupByLevelAndPrice(CommonData.CommonSettings);
            OnLog("start exec GenGroupHtml......", true);
            cgbla.GenGroupHtml();
        }

        /// <summary>
        /// 产销数据新闻
        /// </summary>
        [DescriptionAttribute("GetCarProduceAndSellData 说明：产销数据新闻")]
        public void GetCarProduceAndSellData()
        {
            CarProduceAndSellData carProduceAndSellData = new CarProduceAndSellData();
            OnLog("start exec GetCarProduceAndSellData......", true);

            UpdateSellBrandTree();
            carProduceAndSellData.GetPsData();
            UpdateSellDataMap();
            UpdateAllCarData();
        }

        /// <summary>
        /// 获取所产销数据包括的厂商，品牌，子品牌信息
        /// </summary>
        [DescriptionAttribute("UpdateSellBrandTree 说明：获取所产销数据包括的厂商，品牌，子品牌信息")]
        public void UpdateSellBrandTree()
        {
            OnLog("start exec UpdateSellBrandTree......", true);
            CarProduceAndSellData carProduceAndSellData = new CarProduceAndSellData();
            carProduceAndSellData.UpdateSellBrandTree();
        }

        /// <summary>
        /// 生成产销量地图数据
        /// </summary>
        [DescriptionAttribute("UpdateSellDataMap 说明：生成产销量地图数据")]
        public void UpdateSellDataMap()
        {
            OnLog("start exec UpdateSellDataMap......", true);
            CarProduceAndSellData carProduceAndSellData = new CarProduceAndSellData();
            carProduceAndSellData.UpdateSellDataMap();
        }

        /// <summary>
        /// 生成产销量车辆数据
        /// </summary>
        [DescriptionAttribute("UpdateAllCarData 说明：生成产销量车辆数据")]
        public void UpdateAllCarData()
        {
            OnLog("start exec UpdateAllCarData......", true);
            CarProduceAndSellData carProduceAndSellData = new CarProduceAndSellData();
            carProduceAndSellData.UpdateAllCarData();
        }

        /// <summary>
        /// 生成产销量车辆数据，按月生成。格式为:yyyy-MM-dd
        /// </summary>
        [DescriptionAttribute("UpdateCarData 说明：生成产销量车辆数据，按月生成。格式为:yyyy-MM-dd")]
        public void UpdateCarData()
        {
            OnLog("start exec UpdateCarData......", true);
            CarProduceAndSellData carProduceAndSellData = new CarProduceAndSellData();
            carProduceAndSellData.UpdateCarData(funcArgs[0]);
        }

        /// <summary>
        /// 获取产销数据与新闻内容,后面可选:[/producer:<id>][/brand:<id>][/serial:<id>][/all:0]
        /// </summary>
        [DescriptionAttribute("GetPsData 说明：获取产销数据与新闻内容,后面可选:[/producer:<id>][/brand:<id>][/serial:<id>][/all:0]")]
        public void GetPsData()
        {
            CarProduceAndSellData carProduceAndSellData = new CarProduceAndSellData();
            GetPsData(carProduceAndSellData, funcArgs[0]);
        }

        ///// <summary>
        ///// 获取二手车所有子品牌的车型数据
        ///// </summary>
        //[DescriptionAttribute("GetUCarAllSerialCarAmount 说明：获取二手车所有子品牌的车型数据")]
        //public void GetUCarAllSerialCarAmount()
        //{
        //	OnLog("start exec GetUCarAllSerialCarAmount......", true);
        //	// del by chengl Apr.24.2014 二手车接口失效 http://www.ucar.cn/webservice/buycar/BitAutoCarSource.asmx
        //	// 没有树形二手车页，不需要请求接口
        //	// UCarDataGetter.GetUCarAllSerialCarAmount();
        //}
        /*
		/// <summary>
		/// 保存视频排序的文件
		/// </summary>
		[DescriptionAttribute("SaveVideoOrder 说明：保存视频排序的文件")]
		public void SaveVideoOrder()
		{
			VideoOrder vo = new VideoOrder();
			vo.Log += new LogHandler(getter_Log);
			OnLog("start exec SaveVideoOrder......", true);
			vo.SaveVideoOrder();
		}*/

        /// <summary>
        /// 保存口碑和答疑
        /// </summary>
        [DescriptionAttribute("SaveAskAndKouBei 说明：保存口碑和答疑")]
        public void SaveAskAndKouBei()
        {
            AskAndKouBei aakb = new AskAndKouBei();
            aakb.Log += new LogHandler(getter_Log);
            OnLog("start exec SaveAskAndKouBei......", true);
            aakb.SaveAskAndKouBei();
        }

        /// <summary>
        /// 子品牌获取全部
        /// </summary>
        [DescriptionAttribute("Serial_GetContent 说明：子品牌获取全部")]
        public void Serial_GetContent()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec Serial_GetContent......", true);
            serialService.GetContent();
            serialService.GetXiaoLiaoTreeXml();
        }

        /// <summary>
        /// 更新子品牌各城市的车型报价,后面可选[/<id>],id为子品牌ID，如果取全部，id=0
        /// </summary>
        [DescriptionAttribute("GetCarCityPriceBySerial 说明：更新子品牌各城市的车型报价,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetCarCityPriceBySerial()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetCarCityPriceBySerial......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetCarCityPriceBySerial(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 生成子品牌焦点区新闻HTML
        /// </summary>
        [DescriptionAttribute("GetFocusNewsHTML 说明：生成子品牌焦点区新闻HTML,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetFocusNewsHTML()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetFocusNewsHTML......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetFocusNewsHTML(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /*
		/// <summary>
		/// 生成子品牌买车必看HTML
		/// 同时生成车型移动站-子品牌综述页-买车必看新闻源数据xml
		/// add by chengl Jun.14.2012
		/// </summary>
		[DescriptionAttribute("GetWatchMustsHTML 说明：生成子品牌买车必看HTML，同时生成车型移动站-子品牌综述页-买车必看新闻源数据xml,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
		public void GetWatchMustsHTML()
		{
			Serial serialService = new Serial();
			serialService.Log += new LogHandler(getter_Log);
			OnLog("start exec GetWatchMustsHTML......", true);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int serialId = 0;
				bool success = Int32.TryParse(funcArgs[0], out serialId);
				if (success)
				{
					serialService.GetWatchMustsHTML(serialId);
				}
				else
					Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
			}
		}*/

        /// <summary>
        /// 生成子品牌综述页车型详解块的Html
        /// </summary>
        [DescriptionAttribute("GetPinceBlockHTML 说明：生成子品牌综述页车型详解块的Html,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetPinceBlockHTML()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetPinceBlockHTML......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetPinceBlockHTML(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                // modified by chengl May.22.2012
                // 当没有传入子品牌ID时 取全部子品牌
                serialService.GetPinceBlockHTML(0);
            }
        }

        /*
		/// <summary>
		/// 得到保养信息
		/// </summary>
		[DescriptionAttribute("GetMaintanceMessage 说明：得到保养信息,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
		public void GetMaintanceMessage()
		{
			Serial serialService = new Serial();
			serialService.Log += new LogHandler(getter_Log);
			OnLog("start exec GetMaintanceMessage......", true);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int serialId = 0;
				bool success = Int32.TryParse(funcArgs[0], out serialId);
				if (success)
				{
					serialService.GetMaintanceMessage(serialId);
					serialService.GetMaintancePriceMessage(serialId);
				}
				else
					Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
			}
		}*/

        /// <summary>
        /// 得到子品牌评测图片块
        /// </summary>
        [DescriptionAttribute("GetPingCeImageXml 说明：得到子品牌评测图片块,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetPingCeImageXml()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetPingCeImageXml......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetPingCeImageXml(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        #region 子品牌相关新闻

        /// <summary>
        /// 得到子品牌焦点新闻
        /// </summary>
        [DescriptionAttribute("GetSerialFocusNews 说明：得到得到子品牌焦点新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialFocusNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialFocusNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetFocusNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                serialService.GetFocusNewsHTML(0);
            }
        }

        /// <summary>
        /// 得到子品牌新闻
        /// </summary>
        [DescriptionAttribute("GetSerialNews 说明：得到得到子品牌新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌导购新闻
        /// </summary>
        [DescriptionAttribute("GetSerialDaoGouNews 说明：得到子品牌导购新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialDaoGouNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialDaoGouNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetDaoGouNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌评测新闻
        /// </summary>
        [DescriptionAttribute("GetSerialPingCeNews 说明：得到得到子品牌评测新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialPingCeNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialPingCeNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetPingCeNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌树形评测新闻
        /// </summary>
        [DescriptionAttribute("GetSerialTreePingceNews 说明：得到子品牌树形评测新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialTreePingceNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialTreePingceNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetTreePingCe(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌安全新闻
        /// </summary>
        [DescriptionAttribute("GetSerialAnQuanNews 说明：得到子品牌安全新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialAnQuanNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialAnQuanNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetSecurity(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌试驾新闻
        /// </summary>
        [DescriptionAttribute("GetSerialShiJiaNews 说明：得到子品牌试驾新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialShiJiaNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialShiJiaNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetShiJiaNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌用车新闻
        /// </summary>
        [DescriptionAttribute("GetSerialYongCheNews 说明：得到子品牌用车新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialYongCheNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialYongCheNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetYongCheNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子科技新闻
        /// </summary>
        [DescriptionAttribute("GetSerialKeJiNews 说明：得到子科技新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialKeJiNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialKeJiNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetTechnology(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌改装新闻
        /// </summary>
        [DescriptionAttribute("GetSerialGaiZhuangNews 说明：得到子品牌改装新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialGaiZhuangNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialGaiZhuangNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetGaiZhuang(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌行情新闻
        /// </summary>
        [DescriptionAttribute("GetSerialHangQingNews 说明：得到子品牌行情新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialHangQingNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialHangQingNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.GetHangQingNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /// <summary>
        /// 得到子品牌全部新闻
        /// </summary>
        [DescriptionAttribute("GetSerialAllNews 说明：得到子品牌全部新闻,后面可选[/<id>],id为子品牌ID,不能传0")]
        public void GetSerialAllNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialAllNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success && serialId > 0)
                {
                    serialService.GetAllNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        #endregion

        /// <summary>
        /// 跑百度阿拉丁的接口
        /// </summary>
        [DescriptionAttribute("ForBaiduAlding 说明：跑百度阿拉丁的接口")]
        public void ForBaiduAlding()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec ForBaiduAlding......", true);
            serialService.ForBaiduAlding();
        }

        /// <summary>
        /// 得到没有新闻的子品牌列表
        /// </summary>
        [DescriptionAttribute("GetNoNewsSerialByType 说明：得到没有新闻的子品牌列表")]
        public void GetNoNewsSerialByType()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetNoNewsSerialByType......", true);
            serialService.GetNoNewsSerialByType(funcArgs[0]);
        }

        /// <summary>
        /// 得到销量的XML文件
        /// </summary>
        [DescriptionAttribute("GetXiaoLiaoTreeXml 说明：得到销量的XML文件")]
        public void GetXiaoLiaoTreeXml()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec GetXiaoLiaoTreeXml......", true);
            serialService.GetXiaoLiaoTreeXml();
        }

        /// <summary>
        /// 获取有评测文章，但没有在彩虹条中设置的子品牌
        /// </summary>
        [DescriptionAttribute("GetPingceNewsNotInRainbow 说明：获取有评测文章，但没有在彩虹条中设置的子品牌")]
        public void GetPingceNewsNotInRainbow()
        {
            new Serial().GetPingceNewsNotInRainbow();
        }

        /// <summary>
        /// 得到有维修保养信息的子品牌列表
        /// </summary>
        [DescriptionAttribute("GetHasMaintanceSerial 说明：得到有维修保养信息的子品牌列表")]
        public void GetHasMaintanceSerial()
        {
            new Serial().GetHasMaintanceSerial();
        }

        /*
		#region 车型前台没有有效的调用 GetCityHangQingDefaultPageSpan方法生成的文件
		//车型前台没有有效的调用 → 2012-3-14
		/// <summary>
		/// 得到城市行情脚本块
		/// </summary>
		//[DescriptionAttribute("GetCityHangQingDefaultPageSpan 说明：得到城市行情脚本块,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
		public void GetCityHangQingDefaultPageSpan()
		{
			HangQing hq = new HangQing();
			hq.Log += new LogHandler(getter_Log);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int sId = 0;
				bool success = Int32.TryParse(funcArgs[0], out sId);
				if (success)
					hq.GetCityHangQingDefaultPageSpan(sId);
				else
					Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
			}
			else
			{
				hq.GetCityHangQingDefaultPageSpan(0);
			}
		}
		#endregion
		/// <summary>
		/// 得到城市行情的新闻
		/// </summary>
		[DescriptionAttribute("Get350CityHangQingNews 说明：得到城市行情的新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
		public void Get350CityHangQingNews()
		{
			HangQing hq = new HangQing();
			hq.Log += new LogHandler(getter_Log);
			OnLog("start exec Get350CityHangQingNews......", true);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int sId = 0;
				bool success = Int32.TryParse(funcArgs[0], out sId);
				if (success)
					hq.Get350CityHangQingNews(sId);
				else
					Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
			}
			else
			{
				hq.Get350CityHangQingNews(0);
			}
		}
        
		/// <summary>
		/// 得到城市行情的新闻
		/// </summary>
		[DescriptionAttribute("GetProvinceHangQingNews 说明：省份和城市的行情新闻,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
		public void GetProvinceHangQingNews()
		{
			HangQing hq = new HangQing();
			hq.Log += new LogHandler(getter_Log);
			OnLog("start exec GetProvinceHangQingNews......", true);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int sId = 0;
				bool success = Int32.TryParse(funcArgs[0], out sId);
				if (success)
					hq.GetProvinceHangQingNews(sId);
				else
					Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
			}
			else
				hq.GetProvinceHangQingNews(0);
		}
		 /// <summary>
		/// 保险获取全部内容
		/// </summary>
		[DescriptionAttribute("Inusure_GetContent 说明：保险获取全部内容")]
		public void Inusure_GetContent()
		{
			Insurance inusure = new Insurance();
			inusure.Log += new LogHandler(getter_Log);
			OnLog("start exec Inusure_GetContent......", true);
			inusure.GetContent();
		}
		 */
        ///// <summary>
        ///// 获取买车网信息
        ///// </summary>
        //[DescriptionAttribute("MaiCheSite_GetContent 说明：获取买车网信息")]
        //public void MaiCheSite_GetContent()
        //{
        //	MaiCheSite mc = new MaiCheSite();
        //	mc.Log += new LogHandler(getter_Log);
        //	OnLog("start exec MaiCheSite_GetContent......", true);
        //	mc.GetContent();
        //}
        /// <summary>
        /// 生成树形的导航
        /// </summary>
        [DescriptionAttribute("NavigationBar_Generate 说明：生成树形的导航")]
        public void NavigationBar_Generate()
        {
            NavigationBarGenerator generator = new NavigationBarGenerator();
            OnLog("start exec NavigationBar_Generate......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                if (string.IsNullOrEmpty(funcArgs[0]) || funcArgs[0] == "0")
                {
                    generator.Generate();
                }
                else
                {
                    generator.Generate(funcArgs[0]);
                }
            }
            else
            {
                generator.Generate();
            }
        }

        /// <summary>
        /// 为子品牌生成所有频道导航条
        /// </summary>
        [DescriptionAttribute("NavigationBar_GenerateBySerial 说明：为子品牌生成所有频道导航条")]
        public void NavigationBar_GenerateBySerial()
        {
            NavigationBarGenerator generator = new NavigationBarGenerator();
            OnLog("start exec NavigationBar_GenerateBySerial......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                if (!generator.GenerateBySerial(funcArgs[0]))
                {
                    Console.WriteLine("没有找到该子品牌！");
                }
            }
            else
            {
                Console.WriteLine("缺少子品牌参数！");
            }
        }

        /*
		/// <summary>
		/// 更新答疑数据
		/// </summary>
		[DescriptionAttribute("UpdateAskData 说明：更新答疑数据")]
		public void UpdateAskData()
		{
			AskDataService askDataService = new AskDataService(CommonData.CommonSettings.SavePath);
			OnLog("start exec UpdateAskData......", true);
			askDataService.UpdateAskData();
		}
         
		/// <summary>
		/// 生成答疑的数据块
		/// </summary>
		[DescriptionAttribute("Ask_Generate 说明：生成答疑的数据块")]
		public void Ask_Generate()
		{
			AskHtmlChunkGenerator askGenerator = new AskHtmlChunkGenerator();
			OnLog("start exec Ask_Generate......", true);
			askGenerator.Generate();
		}
		*/

        /// <summary>
        /// 更新二手车数据
        /// </summary>
        [DescriptionAttribute("UpdateUsedCarData 说明：更新二手车数据")]
        public void UpdateUsedCarData()
        {
            UsedCarDataService usedCarDataService = new UsedCarDataService();
            OnLog("start exec UpdateUsedCarData......", true);
            usedCarDataService.UpdateUsedCarData();
        }

        ///// <summary>
        ///// 生成二手车的数据块
        ///// </summary>
        //[DescriptionAttribute("Ucar_Generate 说明：生成二手车的数据块")]
        //public void Ucar_Generate()
        //{
        //    UsedCarHtmlChunkGenerator ucarGenerator = new UsedCarHtmlChunkGenerator();
        //    OnLog("start exec Ucar_Generate......", true);
        //    ucarGenerator.Generate();
        //}

        /// <summary>
        /// 生成子品牌年款外围尺寸xml
        /// </summary>
        [DescriptionAttribute("CreateSerialOutSetXml 说明：生成子品牌年款外围尺寸xml")]
        public void CreateSerialOutSetXml()
        {
            SerialOutSet sos = new SerialOutSet();
            sos.Log += new LogHandler(getter_Log);
            OnLog("start exec CreateSerialOutSetXml......", true);
            sos.CreateSerialOutSetXml(); // 生成数据 Data\SerialSet\SerialOutSet.xml
        }

        /*
		/// <summary>
		/// 子品牌综述页 图释
		/// </summary>
		[DescriptionAttribute("MakeSerialImageCarsHTMLALL 说明：子品牌综述页 图释")]
		public void MakeSerialImageCarsHTMLALL()
		{
			SerialColorImage serialColorImage = new SerialColorImage();
			serialColorImage.Log += new LogHandler(getter_Log);
			OnLog("start exec MakeSerialImageCarsHTMLALL......", true);
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int sId = 0;
				bool success = Int32.TryParse(funcArgs[0], out sId);
				if (success && sId != 0)
					serialColorImage.MakeSerialImageCarsHTML(sId);
				else
					//子品牌综述页 图释
					OnLog("exec MakeSerialImageCarsHTMLALL argument error! argument:" + funcArgs[0], true);

			}
			else
			{
				serialColorImage.MakeSerialImageCarsHTMLALL();
			}

			OnLog("执行了:MakeSerialImageCarsHTMLALL", true);
		}*/


        /// <summary>
        /// 生成编辑试驾评价
        /// </summary>
        //[DescriptionAttribute("CreateEditorComment 说明：生成编辑试驾评价")]
        //public void CreateEditorComment()
        //{
        //    EditorComment ec = new EditorComment();
        //    ec.Log += new LogHandler(getter_Log);
        //    OnLog("CreateEditorComment方法开始执行......", true);
        //    if (funcArgs != null && funcArgs.Length > 0)
        //    {
        //        if (funcArgs[0] == "createeditorcomment")//生成编辑试驾评价
        //        {
        //            int sId = 0;
        //            bool success = Int32.TryParse(funcArgs[1], out sId);
        //            if (success)
        //                ec.CreateEditorComment(sId);
        //            else
        //                ec.CreateEditorComment(0);
        //        }
        //    }
        //    else
        //    {
        //        ec.CreateEditorComment(0);
        //    }
        //    OnLog("执行了:CreateEditorComment", true);
        //}
        /// <summary>
        /// 生成编辑试驾评价
        /// </summary>
        [DescriptionAttribute("CreateEditorComment 说明：生成编辑试驾评价")]
        public void CreateEditorComment()
        {
            EditorComment ec = new EditorComment();
            ec.Log += new LogHandler(getter_Log);
            OnLog("start exec CreateEditorComment......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int sId = 0;
                bool success = Int32.TryParse(funcArgs[0], out sId);
                if (success)
                    ec.CreateEditorComment(sId);
                else
                    ec.CreateEditorComment(0);
            }
            else
            {
                ec.CreateEditorComment(0);
            }
            OnLog("执行了:CreateEditorComment", true);
        }

        /// <summary>
        /// 获取经销商google地图坐标
        /// </summary>
        [DescriptionAttribute("GetVendorListMapInfor 说明：获取经销商google地图坐标")]
        public void GetVendorListMapInfor()
        {
            VendorListMapInfor vendorListMapInfor = new VendorListMapInfor();
            vendorListMapInfor.Log += new LogHandler(getter_Log);
            OnLog("start exec GetVendorListMapInfor......", true);
            vendorListMapInfor.GetVendorListMapInfor();
        }

        /// <summary>
        /// 创建核心看点html
        /// </summary>
        [DescriptionAttribute("GetHeXinKanDianHTML 说明：创建核心看点html")]
        public void GetHeXinKanDianHTML()
        {
            HeXinKanDian obj = new HeXinKanDian();
            obj.Log += new LogHandler(getter_Log);
            OnLog("start exec GetHeXinKanDianHTML......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = ConvertHelper.GetInteger(funcArgs[0]);
                if (serialId > 0)
                    obj.GetHTML(serialId);
                else
                    OnLog("exec GetHeXinKanDianHTML argument error! argument:" + funcArgs[0], true);
            }
            else
            {
                obj.GetHTML();
            }
        }

        /// <summary>
        /// 创建移动站空间详情
        /// </summary>
        [DescriptionAttribute("GetHeXinKanDianHTML 说明：创建移动站空间详情html")]
        public void GetDetailZoneHTML()
        {
            SerialDetailZoneM obj = new SerialDetailZoneM();
            obj.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialDetailZoneHTML......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = ConvertHelper.GetInteger(funcArgs[0]);
                if (serialId > 0)
                    obj.GetHTML(serialId);
                else
                    OnLog("exec GetSerialDetailZoneHTML argument error! argument:" + funcArgs[0], true);
            }
            else
            {
                obj.GetHTML();
            }
        }

        ///// <summary>
        ///// 获取二手车置换信息
        ///// </summary>
        //[DescriptionAttribute("CarReplacementInfo 说明：获取二手车置换信息，可传子品牌id")]
        //public void CarReplacementInfo()
        //{
        //	CarReplacementInfo obj = new CarReplacementInfo();
        //	obj.Log += new LogHandler(getter_Log);
        //	OnLog("start exec CarReplacementInfo......", true);
        //	if (funcArgs != null && funcArgs.Length > 0)
        //	{
        //		int serialId = ConvertHelper.GetInteger(funcArgs[0]);
        //		if (serialId > 0)
        //			obj.UpdateInfo(serialId);
        //		else
        //			OnLog("exec CarReplacementInfo argument error! argument:" + funcArgs[0], true);
        //	}
        //	else
        //	{
        //		obj.UpdateInfo(0);
        //	}
        //	OnLog("end exec CarReplacementInfo......", true);
        //}
        ///// <summary>
        ///// 获取经销商置换行情
        ///// </summary>
        //[DescriptionAttribute("GetZhiHuanDealerNews 说明：获取经销商置换行情，可传品牌id")]
        //public void GetZhiHuanDealerNews()
        //{
        //	CarReplacementInfo obj = new CarReplacementInfo();
        //	obj.Log += new LogHandler(getter_Log);
        //	OnLog("start exec GetZhiHuanDealerNews......", true);
        //	if (funcArgs != null && funcArgs.Length > 0)
        //	{
        //		int brandId = ConvertHelper.GetInteger(funcArgs[0]);
        //		if (brandId > 0)
        //			obj.GetDealerNews(brandId);
        //		else
        //			OnLog("exec GetZhiHuanDealerNews argument error! argument:" + funcArgs[0], true);
        //	}
        //	else
        //	{
        //		obj.GetDealerNews(0);
        //	}
        //	OnLog("end exec GetZhiHuanDealerNews......", true);
        //}
        ///// <summary>
        ///// 获取置换品牌列表
        ///// </summary>
        //[DescriptionAttribute("GetZhiHuanBrandList 说明：获取置换品牌列表")]
        //public void GetZhiHuanBrandList()
        //{
        //	CarReplacementInfo obj = new CarReplacementInfo();
        //	obj.Log += new LogHandler(getter_Log);
        //	OnLog("start exec GetZhiHuanBrandList......", true);
        //	obj.GetZhiHuanBrandList();
        //	OnLog("end exec GetZhiHuanBrandList......", true);
        //}

        /// <summary>
        /// 清除过期的子品牌焦点新闻顺序设置
        /// </summary>
        [DescriptionAttribute("ClearTimeoutSerialFocusNews 说明：清除过期的子品牌焦点新闻顺序设置,后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void ClearTimeoutSerialFocusNews()
        {
            Serial serialService = new Serial();
            serialService.Log += new LogHandler(getter_Log);
            OnLog("start exec ClearTimeoutSerialFocusNews......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    serialService.ClearTimeoutFocusNews(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                serialService.ClearTimeoutFocusNews(0);
            }
            OnLog("end exec ClearTimeoutSerialFocusNews......", true);
        }

        ///// <summary>
        ///// 降价新闻计划任务，更新过期新闻
        ///// </summary>
        //[DescriptionAttribute("JiangJiaNewsPlanTask 说明：降价新闻计划任务，更新过期新闻")]
        //public void JiangJiaNewsPlanTask()
        //{
        //	JiangJiaNews jiangjiaNews = new JiangJiaNews();
        //	OnLog("start exec JiangJiaNewsPlanTask......", true);
        //	jiangjiaNews.PlanTask();
        //	OnLog("end exec JiangJiaNewsPlanTask......", true);
        //}

        /// <summary>
        /// 根据彩虹条，生成子品牌车型详解数据
        /// </summary>
        [DescriptionAttribute("GetSerialPingCeData 说明：根据彩虹条，生成子品牌车型详解数据，参数为子品牌id：0：为全部，可不传")]
        public void GetSerialPingCeData()
        {
            SerialPingCeData pingceData = new SerialPingCeData();
            pingceData.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialPingCeData...", true);

            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    pingceData.GetSerialPingCeData(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                pingceData.GetSerialPingCeData(0);
            }

            OnLog("end exec GetSerialPingCeData...", true);
        }

        /// <summary>
        /// 所有易湃接口执行方法列表
        /// </summary>
        [DescriptionAttribute("EPProcesserList 说明：所有易湃接口执行方法列表")]
        public void EPProcesserList()
        {
            EPProcesser epp = new EPProcesser();
            epp.Log += new LogHandler(getter_Log);
            OnLog("start exec EPProcesserList...", true);

            epp.EPProcesserList();

            OnLog("end exec EPProcesserList end...", true);
        }
        /*
        /// <summary>
        ///     生成车型首页问答的HTML块
        ///     author:songcl date:2014-11-27
        /// </summary>
        [Description("GenerateDefaultAskHtml 说明:生成车型首页问答的HTML块")]
        public void GenerateDefaultAskHtml()
        {
            try
            {
                var getter = new ContentGetter();
                getter.Log += getter_Log;
                OnLog("start exec GenerateDefaultAskHtml......", true);
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int id = 0;
                    bool success = Int32.TryParse(funcArgs[0], out id);
                    if (success)
                    {
                        getter.BuilderDefaultAskHtml();
                    }
                    else
                        Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
                }
                else
                {
                    getter.BuilderDefaultAskHtml();
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        ///     生成主品牌问答的HTML块
        ///     author:songcl date:2014-11-27
        /// </summary>
        [Description("GenerateMasterAskHtml 说明:生成主品牌问答的HTML块")]
        public void GenerateMasterAskHtml()
        {
            try
            {
                var getter = new ContentGetter();
                getter.Log += getter_Log;
                OnLog("start exec GenerateMasterAskHtml......", true);
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int id = 0;
                    bool success = Int32.TryParse(funcArgs[0], out id);
                    if (success)
                    {
                        getter.BuilderMasterAskHtml(id);
                    }
                    else
                        Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
                }
                else
                {
                    getter.BuilderMasterAskHtml(0);
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        ///     生成子品牌问答的HTML块
        ///     author:songcl date:2014-11-27
        /// </summary>
        [Description("GenerateSerialAskHtml 说明:生成子品牌问答的HTML块")]
        public void GenerateSerialAskHtml()
        {
            try
            {
                var getter = new ContentGetter();
                getter.Log += getter_Log;
                OnLog("start exec GenerateSerialAskHtml......", true);
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int id = 0;
                    bool success = Int32.TryParse(funcArgs[0], out id);
                    if (success)
                    {
                        getter.BuilderSerialAskHtml(id);
                    }
                    else
                        Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
                }
                else
                {
                    getter.BuilderSerialAskHtml(0);
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
        }
		*/
        [Description("GenerateKongJianHtml 说明:生成子品牌综述页关键报告中的车内空间HTML块")]
        public void GenerateKongJianHtml()
        {
            var getter = new ContentGetter();
            getter.Log += getter_Log;
            OnLog("start exec GenerateKongJianHtml......", true);
            try
            {
                if (funcArgs != null && funcArgs.Length > 0)
                {
                    int id = 0;
                    bool success = Int32.TryParse(funcArgs[0], out id);
                    if (success)
                    {
                        getter.BuilderKongJianHtml(id);
                    }
                    else
                        Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
                }
                else
                {
                    getter.BuilderKongJianHtml(0);
                }
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex);
            }
        }

        [Description("GenerateH5ArticalXml 说明:生成第四级子品牌新闻XML")]
        public void GenerateH5ArticalXml()
        {
            var getter = new ContentGetter();
            getter.Log += getter_Log;
            OnLog("start exec GenerateH5ArticalXml......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int id = 0;
                bool success = Int32.TryParse(funcArgs[0], out id);
                if (success)
                {
                    getter.BuildH5ArticalXml(id);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                getter.BuildH5ArticalXml(0);
            }
        }

        #endregion

        #region 系统方法，不需要每次都执行

        /// <summary>
        /// 生成CityGroupJsonUTF8.js的类
        /// </summary>
        [DescriptionAttribute("CreateCityGroupJsonUTF8JS 说明：生成CityGroupJsonUTF8.js的类")]
        public void CreateCityGroupJsonUTF8JS()
        {
            CityGroupJsonUTF8JS cityGroupJsonUTF8JS = new CityGroupJsonUTF8JS();
            cityGroupJsonUTF8JS.Log += new LogHandler(getter_Log);
            cityGroupJsonUTF8JS.CreateCityGroupJsonUTF8JS();
        }

        /// <summary>
        /// 显示帮助
        /// </summary>
        [DescriptionAttribute("ShowHelp 说明：显示帮助")]
        public void ShowHelp()
        {
            object[] objs = null;
            Type thisType = this.GetType();
            MethodInfo[] methods = thisType.GetMethods();
            foreach (MethodInfo method in methods)
            {
                objs = method.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (objs != null && objs.Length > 0)
                {
                    System.Console.WriteLine(((DescriptionAttribute)objs[0]).Description);
                }
            }
            Console.ReadLine();
        }

        /// <summary>
        /// 删除去掉的分类,新闻：150，144，147，83，导购：102，115，120
        /// </summary>
        /// <param name="para1"></param>
        public void DeleteNews(string para1)
        {
            if (para1 == "deletenews")
            {
                DeleteNewsTool.DeleteNews();
            }
        }

        /// <summary>
        /// 得到车展相关内容
        /// </summary>
        [DescriptionAttribute("Exhi_GetContent 说明：得到车展相关内容")]
        public void Exhi_GetContent()
        {
            Exhibition exhi = new Exhibition();
            exhi.Log += new LogHandler(getter_Log);
            OnLog("start exec Exhi_GetContent......", true);
            exhi.GetContent();
        }

        /// <summary>
        /// 从新生成DeleteAndInserCarNewsType表
        /// </summary>
        [DescriptionAttribute("DeleteAndInserCarNewsType 说明：从新生成DeleteAndInserCarNewsType表")]
        public void DeleteAndInserCarNewsType()
        {
            CarNewsTypeUpdate cntu = new CarNewsTypeUpdate();
            cntu.Log += new LogHandler(getter_Log);
            cntu.DeleteAndInserCarNewsType();
        }

        /// <summary>
        /// 更新CarNewsType表，需要传参数CarNewsTypeId
        /// </summary>
        [DescriptionAttribute("更新CarNewsType表  说明：需要传参数CarNewsTypeId")]
        public void UpdateCarNewsType()
        {
            int carNewsTypeId = 0;
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int tmpId = ConvertHelper.GetInteger(funcArgs[0]);
                if (CommonData.CarNewsTypeSettings.CarNewsTypeList.ContainsKey(tmpId))
                    carNewsTypeId = tmpId;
            }
            if (carNewsTypeId < 1)
            {
                OnLog("参数错误!，请输入有效的CarNewsTypeId", true);
                return;
            }

            CarNewsTypeUpdate cntu = new CarNewsTypeUpdate();
            cntu.Log += new LogHandler(getter_Log);
            cntu.UpdateCarNewsType(carNewsTypeId);
        }

        /// <summary>
        /// 向晶赞推送子品牌相关数据
        /// </summary>
        [DescriptionAttribute("向晶赞推送子品牌相关数据 说明：需2个参数。1,子品牌id,0为全部;2,操作代码 1=子品牌新增；2=子品牌信息更新；3=图片更新；4=子品牌删除")]
        public void PostSerialAllDataToJingZan()
        {
            int actionType = 0, csid = 0;
            if (funcArgs != null && funcArgs.Length > 1)
            {
                csid = ConvertHelper.GetInteger(funcArgs[0]);
                actionType = ConvertHelper.GetInteger(funcArgs[1]);
            }
            if (actionType < 1 || actionType > 4)
            {
                OnLog("参数错误!，第二个参数可选范围1到4", true);
                return;
            }
            if (csid > 0)
            {
                new SerialDataToJingZan().PostSerialDataToJingZan(actionType, csid);
            }
            else
            {
                new SerialDataToJingZan().PostSerialAllDataToJingZan(actionType);
            }
        }

        /// <summary>
        /// 保存口碑和答疑
        /// </summary>
        [DescriptionAttribute("SerialKouBeiReport 说明：获取子品牌口碑报告，参数为子品牌id：0：为全部")]
        public void SerialKouBeiReport()
        {
            AskAndKouBei aakb = new AskAndKouBei();
            aakb.Log += new LogHandler(getter_Log);
            OnLog("start exec SerialKouBeiReport......", true);

            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    aakb.SaveKouBeiReport(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                Console.WriteLine("请输入子品牌id或0!");
            }
            OnLog("end exec SerialKouBeiReport......", true);
        }

        #endregion

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="logText"></param>
        public void OnLog(string logText, bool nextLine)
        {
            if (Log != null)
                Log(this, new LogArgs(logText, nextLine));
        }

        void getter_Log(object sender, LogArgs e)
        {
            if (Log != null)
                Log(sender, e);
        }

        /// <summary>
        /// 获取城市新闻
        /// </summary>
        /// <param name="para2"></param>
        static void GetCityNews(ContentGetter getter, string para2)
        {
            int serialId = 0;
            bool success = Int32.TryParse(para2, out serialId);
            if (success)
            {
                if (serialId == 0)
                    getter.UpdateSerialCityNews();
                else
                    getter.UpdateSerialCityNews(serialId);
            }
            else
                Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
        }

        /// <summary>
        /// 获取产销数据
        /// </summary>
        /// <param name="para2"></param>
        static void GetPsData(CarProduceAndSellData carProduceAndSellData, string para2)
        {
            if (para2 == "0")
                carProduceAndSellData.GetPsData();
            else
            {
                string[] p2List = para2.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (p2List.Length == 2)
                {
                    int id = 0;
                    bool success = Int32.TryParse(p2List[1], out id);
                    if (success)
                    {
                        if (p2List[0] == "producer")
                            carProduceAndSellData.UpdateSellProducerNews(id);
                        else if (p2List[0] == "brand")
                            carProduceAndSellData.UpdateSellBrandNews(id);
                        else if (p2List[0] == "serial")
                            carProduceAndSellData.UpdateSellSerialNews(id);
                        else if (p2List[0] == "all")
                            carProduceAndSellData.UpdateSellNews();
                    }
                    else
                        Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
        }

        /*
		/// <summary>
		/// 生成答疑的数据块
		/// </summary>
		[DescriptionAttribute("UpdateCarLoanPackageXml 说明：更新车贷套餐数据")]
		public void UpdateCarLoanPackageXml()
		{
			InsuranceAndLoan.UpdateCarLoanPackageXml();
		}
		*/

        /// <summary>
        /// 第四级，生成子品牌商配新闻XML
        /// </summary>
        [DescriptionAttribute("GenerateSerialCommerceNewsData 说明：生成第四级商配文章，参数为子品牌id：0：为全部，可不传")]
        public void GenerateSerialCommerceNewsData()
        {
            SerialCommerceNews newsData = new SerialCommerceNews();
            newsData.Log += new LogHandler(getter_Log);
            OnLog("start exec GenerateSerialCommerceNewsData...", true);

            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    newsData.GetSerialSerialCommerceNewsData(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                newsData.GetSerialSerialCommerceNewsData(0);
            }

            OnLog("end exec GenerateSerialCommerceNewsData...", true);
        }

        /// <summary>
        /// 生成高级选车工具全量数据
        /// </summary>
        public void GenerateUpdateCarDataForSelectToolV2()
        {
            try
            {
                OnLog("start 生成高级选车工具全量数据", true);
                UpdateCarDataForSelectToolV2.UpdateCarPriceForSelect();
                OnLog("end 生成高级选车工具全量数据", true);
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex);
            }
        }
        /*
        [DescriptionAttribute("GetSerialCompetitiveKoubei 说明：更新子品牌的竞品口碑排名，后面可选[/<id>],id为子品牌ID，如果取全部，id=0")]
        public void GetSerialCompetitiveKoubeiHtml()
        {
            ContentGetter getter = new ContentGetter();
            getter.Log += new LogHandler(getter_Log);
            OnLog("start exec GetSerialCompetitiveKoubeiHtml......", true);
            if (funcArgs != null && funcArgs.Length > 0)
            {
                int serialId = 0;
                bool success = Int32.TryParse(funcArgs[0], out serialId);
                if (success)
                {
                    getter.GenerateSerialCompetitiveKoubeiHtml(serialId);
                }
                else
                    Console.WriteLine("不能识别的参数，查看帮助请加参数 /help!");
            }
            else
            {
                getter.GenerateSerialCompetitiveKoubeiHtml(0);
            }
        }
		*/
        [Description("UpdateSparkleSerialRel 说明:更新亮点配置关系数据")]
        public void UpdateSparkleSerialRel()
        {
            SparkleSerialRelUpdate sparkleSerialRelUpdate = new SparkleSerialRelUpdate();
            sparkleSerialRelUpdate.UpdateSparkleSerialRel();
        }

        [Description("StoreKoubeiImpressionInRedis 说明：把口碑印象对应的子品牌存储到redis里")]
        public void StoreKoubeiImpressionInRedis()
        {
            SelectCarKoubei selectCarKoubei = new SelectCarKoubei();
            selectCarKoubei.StoreKoubeiImpressionInRedis();
        }

        [Description("UpdateKoubeiRaingDetail 说明：更新口碑详细评分")]
        public void UpdateKoubeiRaingDetail()
        {
            KoubeiRatingDetail koubeiRatingDetail = new KoubeiRatingDetail();
            koubeiRatingDetail.UpdateKoubeiRatingDetail();
        }

        [Description("GenerateCarCompareXml 说明：生成车款参数对比文档（第四级）")]
        public void GenerateCarCompareXml()
        {
            CarDataCompare carDataCompare = new CarDataCompare();
            carDataCompare.GenerateDataXml(funcArgs);
        }

        /// <summary>
        /// 生成移动端车系网址二维码
        /// PC端车系综述页用
        /// </summary>
        [Description("GenerateSerialQr 说明：生成移动端车系网址二维码")]
        public void GenerateSerialQr()
        {
            int serialId = 0;
            if (funcArgs != null && funcArgs.Length > 0)
            {
                bool success = Int32.TryParse(funcArgs[0], out serialId);
            }
            SerialQrImageGenerator s = new SerialQrImageGenerator();
            s.Generator(serialId);
        }

        /// <summary>
        /// 精真估5年旧车保值率
        /// </summary>
        [Description("GetBaoZhiLv 说明：精真估车系5年旧车保值率")]
        public void GetBaoZhiLv()
        {
            SerialBaoZhiLv serialBaoZhiLv = new SerialBaoZhiLv();
            serialBaoZhiLv.GetBaoZhiLv();
        }
        /// <summary>
        /// 更新评测后台StyleJoinBrand表数据
        /// </summary>
        [Description("UpdateStyleJoinBrand 说明：每日更新评测后台StyleJoinBrand表数据")]
        public void UpdateStyleJoinBrand()
        {
            bool success = CarsEvaluationStyleJoinBrand.UpdateStyleJoinBrand();
            if (!success)
            {
                Common.Log.WriteErrorLog("更新评测后台StyleJoinBrand表数据失败");
            }
        }
        [Description("GetEditUser 说明：保存编辑接口数据")]
        public void GetEditUser()
        {
            try
            {
                Common.Log.WriteLog("保存编辑接口数据开始");
                string fileName = Path.Combine(CommonData.CommonSettings.SavePath, "EidtorUserUrl.xml");
                XmlDocument xmlDoc = CommonFunction.GetXmlDocument(CommonData.CommonSettings.EidtorUserUrl);
                xmlDoc.Save(fileName);
                Common.Log.WriteLog("保存编辑接口数据结束");
            }
            catch (Exception ex)
            {
                Common.Log.WriteErrorLog(ex.ToString());
            }
		}

		/// <summary>
		/// 请求车款数据同步接口
		/// </summary>
		[Description("SyncCarData 说明：请求Carser接口，更新车款相关数据，在洗数据时用")]
		public void SyncCarData()
		{
			Common.Log.WriteLog("请求carser接口开始：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			List<int> carIdList = new List<int>();
			if (funcArgs != null && funcArgs.Length > 0)
			{
				int carid = ConvertHelper.GetInteger(funcArgs[0]);
				if (carid > 0)
				{
					carIdList.Add(carid);
				}
			}
			if (carIdList.Count == 0)
			{
				carIdList = CommonData.GetAllCarData().Keys.ToList() ;
				carIdList.Sort((x, y) => y - x);
			}
			RequestCarserInterface request = new RequestCarserInterface();
			request.RequestCarSer(carIdList);
			Common.Log.WriteLog("请求carser接口结束：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
		}

        /// <summary>
		/// 车系销售排行
		/// </summary>
		[Description("GetSerialSaleRank 说明：车系销售排行")]
        public void GetSerialSaleRank()
        {
            Common.Log.WriteLog("请求车系销售排行开始");
            SerialSaleRank serialSaleRank = new SerialSaleRank();
            serialSaleRank.GetSaleRank();
            Common.Log.WriteLog("请求车系销售排行结束");
        }
    }
}