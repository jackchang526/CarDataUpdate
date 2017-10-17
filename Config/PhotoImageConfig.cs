using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Configuration;

namespace BitAuto.CarDataUpdate.Config
{
	public class PhotoImageConfig : ConfigurationSection
	{
		[ConfigurationProperty("SavePath")]
		public string SavePath
		{
			get { return (string)base["SavePath"]; }
		}
		[ConfigurationProperty("SerialColorPath")]
		public string SerialColorPath
		{
			get { return (string)base["SerialColorPath"]; }
		}
		[ConfigurationProperty("SerialColorAllPath")]
		public string SerialColorAllPath
		{
			get { return (string)base["SerialColorAllPath"]; }
		}
		[ConfigurationProperty("SerialPhotoListPath")]
		public string SerialPhotoListPath
		{
			get { return (string)base["SerialPhotoListPath"]; }
		}
		[ConfigurationProperty("SerialYearPath")]
		public string SerialYearPath
		{
			get { return (string)base["SerialYearPath"]; }
		}
		[ConfigurationProperty("SerialComparePath")]
		public string SerialComparePath
		{
			get { return (string)base["SerialComparePath"]; }
		}
		[ConfigurationProperty("SerialPhotoComparePath")]
		public string SerialPhotoComparePath
		{
			get { return (string)base["SerialPhotoComparePath"]; }
		}
		[ConfigurationProperty("CarPhotoComparePath")]
		public string CarPhotoComparePath
		{
			get { return (string)base["CarPhotoComparePath"]; }
		}
		[ConfigurationProperty("SerialClassPath")]
		public string SerialClassPath
		{
			get { return (string)base["SerialClassPath"]; }
		}
		[ConfigurationProperty("SerialCoverPath")]
		public string SerialCoverPath
		{
			get { return (string)base["SerialCoverPath"]; }
		}
		[ConfigurationProperty("SerialCoverWithoutPath")]
		public string SerialCoverWithoutPath
		{
			get { return (string)base["SerialCoverWithoutPath"]; }
		}
		[ConfigurationProperty("SerialCoverImageAndCountPath")]
		public string SerialCoverImageAndCountPath
		{
			get { return (string)base["SerialCoverImageAndCountPath"]; }
		}
		[ConfigurationProperty("SerialStandardImagePath")]
		public string SerialStandardImagePath
		{
			get { return (string)base["SerialStandardImagePath"]; }
		}
		[ConfigurationProperty("CarStandardImagePath")]
		public string CarStandardImagePath
		{
			get { return (string)base["CarStandardImagePath"]; }
		}
		[ConfigurationProperty("CarCoverImagePath")]
		public string CarCoverImagePath
		{
			get { return (string)base["CarCoverImagePath"]; }
		}
		[ConfigurationProperty("CarFocusImagePath")]
		public string CarFocusImagePath
		{
			get { return (string)base["CarFocusImagePath"]; }
		}
		[ConfigurationProperty("SerialYearFocusImagePath")]
		public string SerialYearFocusImagePath
		{
			get { return (string)base["SerialYearFocusImagePath"]; }
		}
		[ConfigurationProperty("SerialDefaultCarPath")]
		public string SerialDefaultCarPath
		{
			get { return (string)base["SerialDefaultCarPath"]; }
		}
		[ConfigurationProperty("SerialDefaultCarImagePath")]
		public string SerialDefaultCarImagePath
		{
			get { return (string)base["SerialDefaultCarImagePath"]; }
		}
		[ConfigurationProperty("SerialFocusImagePath")]
		public string SerialFocusImagePath
		{
			get { return (string)base["SerialFocusImagePath"]; }
		}
		[ConfigurationProperty("SerialColorCountPath")]
		public string SerialColorCountPath
		{
			get { return (string)base["SerialColorCountPath"]; }
		}
		//[ConfigurationProperty("SerialPhotoHtmlPath")]
		//public string SerialPhotoHtmlPath
		//{
		//	get { return (string)base["SerialPhotoHtmlPath"]; }
		//}
        [ConfigurationProperty("SerialPhotoHtmlPathNew")]
        public string SerialPhotoHtmlPathNew
        {
            get { return (string)base["SerialPhotoHtmlPathNew"]; }
        }
		//[ConfigurationProperty("SerialYearPhotoHtmlPath")]
		//public string SerialYearPhotoHtmlPath
		//{
		//	get { return (string)base["SerialYearPhotoHtmlPath"]; }
		//}
        [ConfigurationProperty("SerialYearPhotoHtmlPathNew")]
        public string SerialYearPhotoHtmlPathNew
        {
            get { return (string)base["SerialYearPhotoHtmlPathNew"]; }
        }
		//[ConfigurationProperty("CarPhotoHtmlPath")]
		//public string CarPhotoHtmlPath
		//{
		//	get { return (string)base["CarPhotoHtmlPath"]; }
		//}
        [ConfigurationProperty("CarPhotoHtmlPathNew")]
        public string CarPhotoHtmlPathNew
        {
            get { return (string)base["CarPhotoHtmlPathNew"]; }
        }
		[ConfigurationProperty("SerialPositionImagePath")]
		public string SerialPositionImagePath
		{
			get { return (string)base["SerialPositionImagePath"]; }
		}
		[ConfigurationProperty("SerialColorImagePath")]
		public string SerialColorImagePath
		{
			get { return (string)base["SerialColorImagePath"]; }
		}
		[ConfigurationProperty("SerialElevenImagePath")]
		public string SerialElevenImagePath
		{
			get { return (string)base["SerialElevenImagePath"]; }
		}
		[ConfigurationProperty("SerialDefaultCarFillImagePath")]
		public string SerialDefaultCarFillImagePath
		{
			get { return (string)base["SerialDefaultCarFillImagePath"]; }
		}
		[ConfigurationProperty("SerialReallyColorImagePath")]
		public string SerialReallyColorImagePath
		{
			get { return (string)base["SerialReallyColorImagePath"]; }
		}
		[ConfigurationProperty("CarImagesListInfoPath")]
		public string CarImagesListInfoPath
		{
			get { return (string)base["CarImagesListInfoPath"]; }
		}

		[ConfigurationProperty("SerialOfficalImagePath")]
		public string SerialOfficalImagePath
		{
			get { return (string)base["SerialOfficalImagePath"]; }
		}

		[ConfigurationProperty("SerialFourthStagePositionImagePath")]
		public string SerialFourthStagePositionImagePath
		{
			get
			{
				return (string)base["SerialFourthStagePositionImagePath"];
			}
		}


		[ConfigurationProperty("SerialFourthStageSourceImagePath")]
		public string SerialFourthStageSourceImagePath
		{
			get
			{
				return (string)base["SerialFourthStageSourceImagePath"];
			}
		}

        [ConfigurationProperty("CarImagesCountPath")]
        public string CarImagesCountPath
        {
            get
            {
                return (string)base["CarImagesCountPath"];
            }
        }

        [ConfigurationProperty("SerialCarReallyImagePath")]
        public string SerialCarReallyImagePath
        {
            get
            {
                return (string)base["SerialCarReallyImagePath"];
            }
        }
        //[ConfigurationProperty("SerialThreeStandardImagePath")]
        //public string SerialThreeStandardImagePath
        //{
        //    get { return (string)base["SerialThreeStandardImagePath"]; }
        //}
        //[ConfigurationProperty("SerialYearColorUrlPath")]
        //public string SerialYearColorUrlPath
        //{
        //    get { return (string)base["SerialYearColorUrlPath"]; }
        //}
    }
}
