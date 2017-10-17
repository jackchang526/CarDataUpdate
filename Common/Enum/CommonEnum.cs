namespace BitAuto.CarDataUpdate.Common.Enum
{
    /// <summary>
    /// 公共枚举类
    /// </summary>
    public class CommonEnum
    {
        /// <summary>
        /// 车辆内部空间枚举类型
        /// </summary>
        public enum CarInnerSpaceType
        {
            /// <summary>
            ///     第一排座椅
            /// </summary>
            FirstSeatToTop = 0,

            /// <summary>
            ///     第二排座椅
            /// </summary>
            SecondSeatToTop = 1,

            /// <summary>
            ///     第二排座椅距离第一排距离
            /// </summary>
            FirstSeatDistance = 2,

            /// <summary>
            ///     后备箱
            /// </summary>
            BackBoot = 3,

            /// <summary>
            ///     第三排座椅
            /// </summary>
            ThirdSeatToTop = 4
        }
    }
}