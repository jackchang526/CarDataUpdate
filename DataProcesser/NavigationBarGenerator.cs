using System;
using System.Collections.Generic;
using System.Text;
using BitAuto.Car.ContentLib.TemplateParse;
using System.IO;
using System.Xml;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Common.Utils;
using BitAuto.CarDataUpdate.Common;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 生成导航条
	/// </summary>
	public class NavigationBarGenerator
	{

		#region Constructor

		public NavigationBarGenerator()
		{
			this._dataDirectory = CommonData.CommonSettings.SavePath;
			this._connectionString = CommonData.ConnectionStringSettings.AutoStroageConnString;
			this._outputDicrectory = Path.Combine(this._dataDirectory, OUTPUT_DIRECTORY);

			Init();
		}

		#endregion


		#region Private Constant Fields

		/// <summary>
		/// 标签配置文件路径
		/// </summary>
		private const string TAG_Config_PATH = @"CarTree\treeTagUrlForHeader.xml";

		/// <summary>
		/// 导航条生成目录
		/// </summary>
		private const string OUTPUT_DIRECTORY = @"CarTree\NavigationBar";

		/// <summary>
		/// 模板文件目录
		/// </summary>
		private const string TEMPLATE_DIRECTORY = "TemplateFiles";

		/// <summary>
		/// 导航条模板文件名称
		/// </summary>
		private const string NAVIGATION_BAR_TEMPLATE = "车型频道导航条.htm";

		/// <summary>
		/// 标签默认显示个数
		/// </summary>
		private const int DEFAULT_TAG_SHOW_NUM = 10;

		/// <summary>
		/// 获取热搜内容的url
		/// </summary>
		private const string HOT_SEARCH_URL = "http://admin.bitauto.com/include/special/yc/00001/hotwords_Manual.shtml";

		#endregion


		#region Private Fields

		/// <summary>
		/// 数据存储目录
		/// </summary>
		private string _dataDirectory;

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		private string _connectionString;

		/// <summary>
		/// 标签显示数量
		/// </summary>
		private int _tagShowNum;

		/// <summary>
		/// 标签配置信息列表（包含Url生成规则）
		/// </summary>
		private List<TagConfigInfo> _tagConfigList;

		/// <summary>
		/// 模板引擎
		/// </summary>
		private TemplateParser _parser;

		/// <summary>
		/// 导航条生成目录全路径
		/// </summary>
		private string _outputDicrectory;

		/// <summary>
		/// 热搜内容
		/// </summary>
		private string _hotSearchContent;

		/// <summary>
		/// 标签信息列表
		/// </summary>
		List<TagInfo> tagList = null;

		/// <summary>
		/// 
		/// </summary>
		private string TagType;
		#endregion


		#region Private Methods

		private void Init()
		{
			_tagConfigList = GetTagConfigList(TAG_Config_PATH, out _tagShowNum);
			_parser = new TemplateParser(TEMPLATE_DIRECTORY);
			_hotSearchContent = HttpUtils.DownLoadString(HOT_SEARCH_URL);
		}

		/// <summary>
		/// 获得标签配置信息列表和标签显示数量
		/// </summary>
		/// <param name="tagConfigFilePath">标签配置文件目录</param>
		/// <param name="showNum">标签显示数量</param>
		/// <returns>标签列表</returns>
		private List<TagConfigInfo> GetTagConfigList(string tagConfigFilePath, out int showNum)
		{
			List<TagConfigInfo> tagConfigInfoList = new List<TagConfigInfo>();
			showNum = DEFAULT_TAG_SHOW_NUM;
			string fullPath = Path.Combine(_dataDirectory, tagConfigFilePath);
			if (File.Exists(fullPath))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(fullPath);

				XmlNode rootNode = xmlDocument.SelectSingleNode("/TagList");
				if (rootNode != null)
				{
					//读取最大显示标签数
					XmlAttribute showNumAttribute = rootNode.Attributes["outnum"];
					if (showNumAttribute != null)
					{
						int.TryParse(showNumAttribute.Value, out showNum);
					}

					//读取标签配置信息
					TagConfigInfo currentConfigInfo = null;
					foreach (XmlElement tagNode in rootNode.SelectNodes("./Tag"))
					{
						currentConfigInfo = new TagConfigInfo();
						currentConfigInfo.Type = tagNode.GetAttribute("type").Trim();
						currentConfigInfo.Title = tagNode.GetAttribute("title").Trim();
						currentConfigInfo.BaseUrl = tagNode.GetAttribute("baseUrl").Trim();
						currentConfigInfo.SearchFrameUrl = tagNode.GetAttribute("searchFrameUrl").Trim();
						currentConfigInfo.LogoCssName = tagNode.GetAttribute("logoCssName").Trim();
						currentConfigInfo.OutputDirectory = tagNode.GetAttribute("outputDirectory").Trim();
						currentConfigInfo.Target = tagNode.GetAttribute("target").Trim();
						bool isTree = true;
						bool.TryParse(tagNode.GetAttribute("isTree").Trim(), out isTree);
						currentConfigInfo.IsTree = isTree;
						foreach (XmlElement urlNode in tagNode.SelectNodes("./Url"))
						{
							currentConfigInfo.UrlRules.Add(urlNode.GetAttribute("type").Trim(),
								urlNode.GetAttribute("urlRule").Trim());
						}

						tagConfigInfoList.Add(currentConfigInfo);
					}
				}
			}
			return tagConfigInfoList;
		}

		/// <summary>
		/// 创建指定的目录
		/// 如果存在此目录跳过
		/// </summary>
		/// <param name="fullPath">目录全路径</param>
		/// <returns>目录信息</returns>
		private DirectoryInfo CreateDirecotry(string fullPath)
		{
			return IOUtils.CreateDirecotry(fullPath);
		}

		/// <summary>
		/// 获得标签信息列表
		/// </summary>
		/// <param name="tagType">标签类型</param>
		/// <param name="urlType">url类型</param>
		/// <param name="brand">品牌信息（如果是首页或搜索页，<paramref name="brand"/>参数值为null）</param>
		/// <returns></returns>
		private List<TagInfo> GetTagList(string tagType, string urlType, BrandBase brand)
		{
			//如果该标签类型已经经过排序，直接返回，否则排序
			if (tagType == TagType && tagList != null && tagList.Count > 0)
			{
				return tagList;
			}
			else
			{
				TagType = tagType;
			}

			List<TagConfigInfo> configList = this._tagConfigList;

			//找出当前标签的索引号
			int currentTagIndex = configList.FindIndex(delegate(TagConfigInfo config)
			{
				return config.Type == tagType;
			});

			//如果在显示的标签数之后，调整顺序
			int lastShowIndex = _tagShowNum - 1;
			if (currentTagIndex > lastShowIndex)
			{
				List<TagConfigInfo> sortedConfigList = new List<TagConfigInfo>(configList.Count);
				for (int i = 0; i < configList.Count; i++)
				{
					if (i == lastShowIndex)
					{
						sortedConfigList.Insert(lastShowIndex, configList[currentTagIndex]);
					}
					else if (i > lastShowIndex && i <= currentTagIndex)
					{
						sortedConfigList.Insert(i, configList[i - 1]);
					}
					else
					{
						sortedConfigList.Insert(i, configList[i]);
					}
				}
				configList = sortedConfigList;
			}

			tagList = new List<TagInfo>(configList.Count);
			foreach (TagConfigInfo tagConfig in configList)
			{
				string url;

				//当前频道的标签显示首页地址
				if (tagConfig.Type == tagType)
				{
					url = tagConfig.GetUrl(TagConfigInfo.HOME_URL_TYPE);
				}
				////如果当前是主品牌页，并且子品牌的个数大于零时
				////非经销商频道的主品牌页的经销商标签链接到它的第一个子品牌页
				//else if (tagType != "jingxiaoshang" && tagConfig.Type == "jingxiaoshang"
				//    && urlType == TagConfigInfo.MASTER_URL_TYPE
				//    && brand.ChildNodes.Count > 0)
				//{
				//    url = tagConfig.GetUrl(brand.ChildNodes[0]);
				//}
				//其它标签的地址和品牌对应
				else
				{
					url = tagConfig.GetUrl(urlType, brand);
				}

				tagList.Add(new TagInfo()
				{
					Type = tagConfig.Type,
					Title = tagConfig.Title,
					Url = url,
					Target = tagConfig.Target
				});
			}

			return tagList;
		}

		/// <summary>
		/// 获取生成文件名称
		/// 首页：Home.htm
		/// 主品牌/品牌/子品牌页：{Masterbrand/Brand/Serial}_{Id}.htm
		/// 搜索页：Serach.htm
		/// </summary>
		/// <param name="urlType">url类型</param>
		/// <param name="brand">品牌信息</param>
		/// <returns></returns>
		private string GetGenerateFileName(string urlType, BrandBase brand)
		{
			return string.Format("{0}{1}.htm", urlType,
				(brand != null ? "_" + brand.Id.ToString() : string.Empty));
		}

		/// <summary>
		/// 获得面包屑列表
		/// 按照频道、主品牌、品牌、子品牌的顺序
		/// </summary>
		/// <param name="tagConfig">标签配置信息</param>
		/// <param name="urlType">url类型</param>
		/// <param name="brand">品牌信息</param>
		/// <returns></returns>
		private List<CrumbInfo> GetCrumbs(TagConfigInfo tagConfig, string urlType, BrandBase brand)
		{
			//从当前品牌开始，依次把当前品牌以及它所有的父节点推入栈中
			Stack<BrandBase> brands = new Stack<BrandBase>();
			while (brand != null)
			{
				brands.Push(brand);
				brand = brand.ParentNode;
			}

			List<CrumbInfo> crumbList = new List<CrumbInfo>();
			//先添加频道
			crumbList.Add(new CrumbInfo()
			{
				DisplayName = tagConfig.Title == "报价" ? "汽车报价" : tagConfig.Title,
				Url = tagConfig.GetUrl(TagConfigInfo.HOME_URL_TYPE)
			});
			int level = 0;
			//自顶向下添加品牌
			foreach (BrandBase currentBrand in brands)
			{
				//如果品牌名（如果以“进口”开头，去掉“进口”二字再比较）和主品牌名相同，
				//并且主品牌只有一个品牌，去掉品牌名
				if (level == 1 && currentBrand.ParentNode.ChildNodes.Count == 1)
				{
					string removePerfix = "进口";
					string compareName = currentBrand.Name;
					if (compareName.StartsWith(removePerfix))
					{
						compareName = compareName.Substring(removePerfix.Length,
							compareName.Length - removePerfix.Length);
					}
					if (compareName == currentBrand.ParentNode.Name)
					{
						continue;
					}
				}

				crumbList.Add(new CrumbInfo()
				{
					DisplayName = currentBrand.Name,
					Url = tagConfig.GetUrl(currentBrand)
				});
				level++;
			}

			if (urlType == TagConfigInfo.SEARCH_URL_TYPE)
			{
				crumbList.Add(new CrumbInfo()
				{
					DisplayName = "选车"
				});
			}

			return crumbList;
		}


		/// <summary>
		/// 生成标签的静态文件
		/// </summary>
		/// <param name="tagConfig">标签配置信息</param>
		/// <param name="urlType">url类型</param>
		/// <param name="brand">品牌信息</param>
		/// <param name="outputFullDirectory">输出目录全路径</param>
		private void GenerateNavigationBarFile(TagConfigInfo tagConfig, string urlType,
			BrandBase brand, string outputFullDirectory)
		{
			////要注入模板的参数
			//IDictionary<string, object> currentContext = new Dictionary<string, object>();
			//currentContext.Add("tagConfig", tagConfig);
			//currentContext.Add("urlType", urlType);
			//currentContext.Add("crumbs", GetCrumbs(tagConfig, urlType, brand));
			//currentContext.Add("hotSearchContent", _hotSearchContent);
			//currentContext.Add("tagShowNum", this._tagShowNum);
			//currentContext.Add("tags", GetTagList(tagConfig.Type, urlType, brand));

			IDictionary<string, object> currentContext = GenerateNavigationBarParms(tagConfig, urlType, brand);

			using (FileStream fileStream = new FileStream(
				Path.Combine(outputFullDirectory, GetGenerateFileName(urlType, brand)),
				FileMode.Create, FileAccess.Write))
			{
				StreamWriter writer;
				if (tagConfig.Type == "jiangjia")
				{ writer = new StreamWriter(fileStream, new UTF8Encoding(true)); }
				else
				{ writer = new StreamWriter(fileStream); }
				// StreamWriter writer = new StreamWriter(fileStream);
				_parser.ParseTemplate(NAVIGATION_BAR_TEMPLATE, currentContext, writer);
				writer.Flush();
			}
		}

		/// <summary>
		/// 生成标签的静态文件
		/// </summary>
		/// <param name="tagConfig">标签配置信息</param>
		/// <param name="urlType">url类型</param>
		/// <param name="brand">品牌信息</param>
		/// <param name="outputFullDirectory">输出目录全路径</param>
		private void GenerateNavigationBarFileCommon(TagConfigInfo tagConfig, string urlType,
			BrandBase brand, string outputFullDirectory)
		{
			IDictionary<string, object> currentContext = GenerateNavigationBarParms(tagConfig, urlType, brand);

			using (FileStream fileStream = new FileStream(
				Path.Combine(outputFullDirectory, string.Concat("navBar_", tagConfig.Type, "_gb2312.shtml")),
				FileMode.Create, FileAccess.Write))
			{
				StreamWriter writer = new StreamWriter(fileStream, Encoding.GetEncoding("gb2312"));
				_parser.ParseTemplate(NAVIGATION_BAR_TEMPLATE, currentContext, writer);
				writer.Flush();
			}

			UTF8Encoding utf8 = new UTF8Encoding(false);
			using (FileStream fileStream = new FileStream(
				Path.Combine(outputFullDirectory, string.Concat("navBar_", tagConfig.Type, "_utf8.shtml")),
				FileMode.Create, FileAccess.Write))
			{
				StreamWriter writer = new StreamWriter(fileStream, utf8);
				_parser.ParseTemplate(NAVIGATION_BAR_TEMPLATE, currentContext, writer);
				writer.Flush();
			}
		}


		/// <summary>
		/// 为生成静态文件添加参数
		/// </summary>
		/// <param name="tagConfig"></param>
		/// <param name="urlType"></param>
		/// <param name="brand"></param>
		private IDictionary<string, object> GenerateNavigationBarParms(TagConfigInfo tagConfig, string urlType, BrandBase brand)
		{
			IDictionary<string, object> currentContext = new Dictionary<string, object>();
			currentContext.Add("tagConfig", tagConfig);
			currentContext.Add("urlType", urlType);
			currentContext.Add("crumbs", GetCrumbs(tagConfig, urlType, brand));
			currentContext.Add("hotSearchContent", _hotSearchContent);
			currentContext.Add("tagShowNum", this._tagShowNum);
			currentContext.Add("tags", GetTagList(tagConfig.Type, urlType, brand));
			currentContext.Add("isTree", tagConfig.IsTree);
			return currentContext;
		}
		/// <summary>
		/// 获得子节点集合
		/// </summary>
		/// <param name="parentNodes">父节点集合</param>
		/// <returns></returns>
		private IEnumerable<BrandBase> GetChildNodes(IEnumerable<BrandBase> parentNodes)
		{
			return BrandBase.GetChildNodes(parentNodes);
		}

		/// <summary>
		/// 获取主品牌、品牌、子品牌的三级树型结构
		/// </summary>
		/// <returns></returns>
		private List<BrandBase> GetBrandTree()
		{
			BrandDataService brandDataSerivce = new BrandDataService(_dataDirectory, _connectionString);
			return brandDataSerivce.GetBrandTree();
		}

		/// <summary>
		/// 生成导航条
		/// </summary>
		/// <param name="tagConfig">标签配置信息</param>
		/// <param name="brandTree">主品牌、品牌、子品牌的三级树型结构</param>
		private void GenerateCommon(TagConfigInfo tagConfig, List<BrandBase> brandTree)
		{
			string tagDirectory = string.IsNullOrEmpty(tagConfig.OutputDirectory) ?
				Path.Combine(_outputDicrectory, tagConfig.Type) :
				tagConfig.OutputDirectory;

			//生成首页
			string urlType = TagConfigInfo.HOME_URL_TYPE;
			DirectoryInfo outputDirectory = CreateDirecotry(tagDirectory);
			GenerateNavigationBarFileCommon(tagConfig, urlType, null, outputDirectory.FullName);
		}

		/// <summary>
		/// 生成导航条
		/// </summary>
		/// <param name="tagConfig">标签配置信息</param>
		/// <param name="brandTree">主品牌、品牌、子品牌的三级树型结构</param>
		private void Generate(TagConfigInfo tagConfig, List<BrandBase> brandTree)
		{
			string tagDirectory = string.IsNullOrEmpty(tagConfig.OutputDirectory) ?
				Path.Combine(_outputDicrectory, tagConfig.Type) :
				tagConfig.OutputDirectory;

			string urlType;
			DirectoryInfo outputDirectory;

			//生成首页
			urlType = TagConfigInfo.HOME_URL_TYPE;
			outputDirectory = CreateDirecotry(Path.Combine(tagDirectory, urlType));
			GenerateNavigationBarFile(tagConfig, urlType, null, outputDirectory.FullName);

			// add by chengl Oct.12.2013
			// 车型频道首页单独指定搜索框 内容(原Home.htm被所有树形页引用)
			if (tagConfig.Type == "chexing")
			{
				string path = AppDomain.CurrentDomain.BaseDirectory + "\\TemplateFiles\\车型频道首页搜索定制.htm";
				if (File.Exists(path))
				{
					string chexingDefaultSoBar = File.ReadAllText(path);
					if (!string.IsNullOrEmpty(chexingDefaultSoBar))
					{
						tagConfig.SerachFrameContent = chexingDefaultSoBar;
						//生成新测试首页
						urlType = TagConfigInfo.HOME_URL_TYPE;
						outputDirectory = CreateDirecotry(Path.Combine(tagDirectory, urlType + "New"));
						GenerateNavigationBarFile(tagConfig, urlType, null, outputDirectory.FullName);
					}
				}
			}

			// delete by chengl Jul.20.2012

			////生成主品牌
			//urlType = TagConfigInfo.MASTER_URL_TYPE;
			//outputDirectory = CreateDirecotry(Path.Combine(tagDirectory, urlType));
			//foreach (BrandBase masterBrand in brandTree)
			//{
			//    GenerateNavigationBarFile(tagConfig, urlType, masterBrand, outputDirectory.FullName);
			//}

			////生成品牌
			//urlType = TagConfigInfo.BRAND_URL_TYPE;
			//outputDirectory = CreateDirecotry(Path.Combine(tagDirectory, urlType));
			//foreach (BrandBase brand in GetChildNodes(brandTree))
			//{
			//    GenerateNavigationBarFile(tagConfig, urlType, brand, outputDirectory.FullName);
			//}

			////生成子品牌
			//urlType = TagConfigInfo.SERIAL_URL_TYPE;
			//outputDirectory = CreateDirecotry(Path.Combine(tagDirectory, urlType));
			//foreach (BrandBase serialBrand in GetChildNodes(GetChildNodes(brandTree)))
			//{
			//    GenerateNavigationBarFile(tagConfig, urlType, serialBrand, outputDirectory.FullName);
			//}

			////生成搜索
			//urlType = TagConfigInfo.SEARCH_URL_TYPE;
			//outputDirectory = CreateDirecotry(Path.Combine(tagDirectory, urlType));
			//GenerateNavigationBarFile(tagConfig, urlType, null, outputDirectory.FullName);
		}

		#endregion


		#region Public Methods

		public bool GenerateBySerial(string serialId)
		{
			bool success = false;
			List<BrandBase> brandTree = GetBrandTree();
			foreach (BrandBase serialBrand in GetChildNodes(GetChildNodes(brandTree)))
			{
				if (serialBrand.Id.ToString() == serialId)
				{
					foreach (TagConfigInfo tagConfig in this._tagConfigList)
					{
						if (tagConfig.IsTree)
						{
							string tagDirectory = string.IsNullOrEmpty(tagConfig.OutputDirectory) ?
								Path.Combine(_outputDicrectory, tagConfig.Type) : tagConfig.OutputDirectory;
							string urlType = TagConfigInfo.SERIAL_URL_TYPE;
							DirectoryInfo outputDirectory = CreateDirecotry(Path.Combine(tagDirectory, urlType));
							GenerateNavigationBarFile(tagConfig, urlType, serialBrand, outputDirectory.FullName);
						}
					}
					success = true;
					break;
				}
			}
			return success;
		}



		/// <summary>
		/// 生成所有标签的导航条
		/// </summary>
		public void Generate()
		{
			List<BrandBase> brandTree = GetBrandTree();

			//先按标签分目录
			foreach (TagConfigInfo tagConfig in this._tagConfigList)
			{
				if (tagConfig.IsTree)
				{
					Generate(tagConfig, brandTree);
				}
				else
				{
					GenerateCommon(tagConfig, brandTree);
				}
			}
		}

		/// <summary>
		/// 生成该标签的导航条
		/// </summary>
		/// <param name="tagType">标签名</param>
		public void Generate(string tagType)
		{
			List<BrandBase> brandTree = GetBrandTree();

			TagConfigInfo tagConfig = _tagConfigList.Find(delegate(TagConfigInfo config)
			{
				return config.Type == tagType;
			});

			if (tagConfig == null)
			{
				throw new Exception("不存在此标签");
			}
			if (tagConfig.IsTree)
			{
				Generate(tagConfig, brandTree);
			}
			else
			{
				GenerateCommon(tagConfig, brandTree);
			}
		}

		#endregion

	}
}
