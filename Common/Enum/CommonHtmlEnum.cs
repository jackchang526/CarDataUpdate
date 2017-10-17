using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Enum
{
    public class CommonHtmlEnum
    {
        /// <summary>
        /// 数据 类型标示
        /// </summary>
        public enum TypeEnum
        {
            Master = 1,
            Brand = 2,
            Serial = 3,
            Car = 4,
            Other = 5
        }
        /// <summary>
        /// 页面 标示
        /// </summary>
        public enum TagIdEnum
        {
            /// <summary>
            /// 子品牌综述页（简称：综述页）
            /// </summary>
            SerialSummary = 1, //综述页
            //SerialSummaryOld = 2, //旧版综述页
            /// <summary>
            /// 主品牌综述页（简称：主品牌页）
            /// </summary>
            MasterBrandPage = 3, //主品牌页
            BrandPage = 4,	//品牌页
            MasterBrandPageOther = 5,//主品牌其他页面
            BrandPageOther = 6, //品牌其他页面
            WirelessSerialSummary = 7, //移动版 综述页
            WirelessSerialFocusSummary = 8, //移动版 综述页 要闻
            /// <summary>
            /// 车型频道首页
            /// </summary>
            CarDefault = 9,
            H5SerialSummary = 10, //H5综述页

            SerialPingCe = 11, //车型详解页 
            WirelessSerialSummaryV2 = 12, //移动版 综述页 口碑
            SerialSummaryNew = 13, //新版综述页
            MasterBrandPageV2 = 14,  //1200主品牌页 视频
            BrandPageV2 = 15,  //1200品牌页 视频
            KouBeiDuiBi=16 //口碑对比
        }
        /// <summary>
        /// 综述页 块 标示
        /// </summary>
        public enum BlockIdEnum
        {
            Pingce = 1,//车型详解
            FocusNews = 2,//焦点新闻
            HexinReport = 3,//核心关键报告
            KoubeiReport = 4,//口碑点评
            EditorComment = 5,//编辑评论
            Ask = 6,	//答疑
            Video = 7,//视频
            FocusNewsOld = 8,//旧焦点新闻
            KoubeiImpression = 9,//口碑印象
            KoubeiReportNew = 10, //新口碑块
            SuvReport = 11, //SUV核心报告参数数据
            FocusNewsForWaitSale = 12, //待销子品牌焦点新闻块
            CarInnerSpace = 13, //车款空间块
            SerialAward = 14, //车型奖项块
            FocusNewsForWireless = 15, //新版综述页 焦点新闻块
            H5Artical = 16, //第四级文章页面HTML
            EditorCommentV2 = 17,//编辑评论V2版
            H5KoubeiV2 = 18,//第四级网友口碑
            CompetitiveKoubei = 19,//综述页竞品口碑排名
            KouBeiTuiJian = 20,  //口碑推荐。综述页和车型详解页
            KouBeiRating = 21,  //口碑评分。车型详解页
            SerialDetailZone = 22,  //移动站空间详情信息展示
            AskNew = 23,  //答疑1200版
            KouBeiRatingV2 = 24,  //口碑评分。车型详解页V2
            CompetitiveKoubeiV2 = 25,//综述页竞品口碑排名
            KouBeiTuiJianV2 = 26,  //口碑推荐。综述页和车型详解页
            WangYouDianPing=27 //网友点评
        }
        /*
        /// <summary>
        /// 数据 类型标示
        /// </summary>
        public enum TypeEnumNew
        {
            Master = 6,
            Brand = 7,
            Serial = 8,
            Car = 9,
            Other = 10
        }

        /// <summary>
        /// 页面 标示
        /// </summary>
        public enum TagIdEnumNew
        {
            /// <summary>
            /// 子品牌综述页（简称：综述页）
            /// </summary>
            SerialSummary = 13,
            CarDefault = 14
        }

        /// <summary>
        /// 综述页 块 标示
        /// </summary>
        public enum BlockIdEnumNew
        {
            Ask = 23,  //答疑1200版
            SerialAward = 24,//奖项块
            Pingce = 25,//车型详解
            FocusNews = 26,//焦点新闻
            HexinReport = 27,//核心关键报告
            SuvReport = 28,//SUV核心报告参数数据
            CarInnerSpace = 29, //车款空间块
            EditorComment = 30,//编辑评论
            KoubeiReport = 31,//新口碑块
            Video = 32,//视频
            KoubeiImpression = 33,//口碑印象
            CompetitiveKoubei = 34,//综述页竞品口碑排名
            KoubeiRating = 35//口碑评分
        }
         * */
    }
}
