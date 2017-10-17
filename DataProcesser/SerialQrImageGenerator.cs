using BitAuto.CarDataUpdate.Common;
using BitAuto.CarDataUpdate.Common.Model;
using BitAuto.CarDataUpdate.Config;
using BitAuto.Services.FileServer.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using ThoughtWorks.QRCode.Codec;

namespace BitAuto.CarDataUpdate.DataProcesser
{
	/// <summary>
	/// 车系二维码生成
	/// </summary>
	public class SerialQrImageGenerator
	{
		private readonly string FileServerPath = "qrimages/";//分布式文件保存地址
		private readonly string LocalServerPath = "SerialQr\\";//本地文件保存地址
		private readonly string YiCheLogoPath = string.Empty; //易车logo地址
		private string serialMUrl = "http://car.m.yiche.com/{0}/";//二维码内容

		public SerialQrImageGenerator()
		{
			CommonSettings m_config = Common.CommonData.CommonSettings;
			FileServerPath = Path.Combine(m_config.FileServerPath, FileServerPath);
			YiCheLogoPath = m_config.YiCheLogoLocalPath;
		}

		/// <summary>
		/// 生成二维码，并保存到分布式
		/// </summary>
		/// <param name="serialId">车系id，全部生成传0</param>
		public void Generator(int serialId)
		{
			Log.WriteLog("开始生成车系二维码");
			List<SerialInfo> serialList = null;
			if (serialId > 0 && CommonData.SerialDic.Keys.Contains(serialId))
			{
				serialList = new List<SerialInfo>() { CommonData.SerialDic[serialId] };
			}
			else
			{
				serialList = new List<SerialInfo>(CommonData.SerialDic.Values);
			}
			string localFilePath = Path.Combine(CommonData.CommonSettings.SavePath, LocalServerPath);
			try
			{
				if (!Directory.Exists(localFilePath))
				{
					Directory.CreateDirectory(localFilePath);
				}
			}
			catch (Exception ex)
			{
				Log.WriteErrorLog("生成车系二维码文件夹错误：" + localFilePath + "\n\r" + ex.ToString());
			}
			
			foreach (SerialInfo serial in serialList)
			{
				try
				{
					string nasImageName = Path.Combine(FileServerPath, string.Format("{0}.png", serial.Id));
					if (!FileRepositoryService.ExistsFile(nasImageName))
					{
						string content = string.Format(serialMUrl, serial.AllSpell);

						Bitmap qrImage = CreateQRCodeWithLogo(content, YiCheLogoPath);
						string localImageName = Path.Combine(localFilePath, string.Format("{0}.png", serial.Id));
						qrImage.Save(localImageName);//保存本地
						//FileStream fs = new FileStream(qrImage., FileAccess.Read, false);
						SaveNas(nasImageName, localImageName);//保存分布式
						Log.WriteLog("生成二维码：csid=" + serial.Id);
					}
				}
				catch (Exception ex)
				{
					Log.WriteErrorLog("生成车系二维码错误：" + serial.Id + "\n\r" + ex.ToString());
				}
			}
			Log.WriteLog("生成车系二维码结束");
		}

		/// <summary>
		/// 保存分布式
		/// </summary>
		/// <param name="nasPathName">分布式路径</param>
		/// <param name="localFileName">本地路径</param>
		private void SaveNas(string nasPathName, string localFileName)
		{
			if (!FileRepositoryService.ExistsDirectory(FileServerPath))
			{
				FileRepositoryService.CreateDirectory(FileServerPath);
			}
			FileRepositoryService.CreateFile(nasPathName, localFileName);
		}

		/// <summary>
		/// 创建二维码
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public Bitmap Create(string content)
		{
			QRCodeEncoder qRCodeEncoder = new QRCodeEncoder();
			qRCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;//设置二维码编码格式 
			qRCodeEncoder.QRCodeScale = 3;//设置编码测量度
			qRCodeEncoder.QRCodeVersion = 6;//设置编码版本
			qRCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//设置错误校验 

			Bitmap image = qRCodeEncoder.Encode(content);
			return image;
		}

		/// <summary>
		/// 获取本地图片
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private Bitmap GetLocalLog(string fileName)
		{
			Bitmap newBmp = new Bitmap(fileName);
			return newBmp;
		}
		/// <summary>
		/// 生成带logo二维码
		/// </summary>
		/// <param name="content">二维码内容</param>
		/// <param name="logopath">logo地址</param>
		/// <returns></returns>
		public Bitmap CreateQRCodeWithLogo(string content, string logopath)
		{
			//生成二维码
			Bitmap qrcode = Create(content);

			//生成logo
			Bitmap logo = GetLocalLog(logopath);
			ImageUtility util = new ImageUtility();
			Bitmap finalImage = util.MergeQrImg(qrcode, logo);
			return finalImage;
		}
	}

	class ImageUtility
	{
		#region 合并用户QR图片和Logo

		/// <summary>
		/// 合并用户QR图片和用户头像
		/// </summary>
		/// <param name="qrImg">QR图片</param>
		/// <param name="logoImg">用户头像</param>
		/// <param name="n"></param>
		/// <returns></returns>
		public Bitmap MergeQrImg(Bitmap qrImg, Bitmap logoImg, double n = 0.23)
		{
			int margin = 10;
			float dpix = qrImg.HorizontalResolution;
			float dpiy = qrImg.VerticalResolution;
			var _newWidth = (10 * qrImg.Width - 36 * margin) * 1.0f / 36;
			var _headerImg = ZoomPic(logoImg, _newWidth / logoImg.Width);
			//处理Logo
			int newImgWidth = _headerImg.Width + margin;
			Bitmap headerBgImg = new Bitmap(newImgWidth, newImgWidth);
			headerBgImg.MakeTransparent();
			Graphics g = Graphics.FromImage(headerBgImg);
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			g.Clear(Color.Transparent);
			Pen p = new Pen(new SolidBrush(Color.White));
			Rectangle rect = new Rectangle(0, 0, newImgWidth - 1, newImgWidth - 1);
			using (GraphicsPath path = CreateRoundedRectanglePath(rect, 1))
			{
				g.DrawPath(p, path);
				g.FillPath(new SolidBrush(Color.White), path);
			}
			//画Logo
			Bitmap img1 = new Bitmap(_headerImg.Width, _headerImg.Width);
			Graphics g1 = Graphics.FromImage(img1);
			g1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g1.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			g1.Clear(Color.Transparent);
			Pen p1 = new Pen(new SolidBrush(Color.Gray));
			Rectangle rect1 = new Rectangle(0, 0, _headerImg.Width - 1, _headerImg.Width - 1);
			using (GraphicsPath path1 = CreateRoundedRectanglePath(rect1, 1))
			{
				g1.DrawPath(p1, path1);
				TextureBrush brush = new TextureBrush(_headerImg);
				g1.FillPath(brush, path1);
			}
			g1.Dispose();
			PointF center = new PointF((newImgWidth - _headerImg.Width) / 2, (newImgWidth - _headerImg.Height) / 2);
			g.DrawImage(img1, center.X, center.Y, _headerImg.Width, _headerImg.Height);
			g.Dispose();
			Bitmap backgroudImg = new Bitmap(qrImg.Width, qrImg.Height);
			backgroudImg.MakeTransparent();
			backgroudImg.SetResolution(dpix, dpiy);
			headerBgImg.SetResolution(dpix, dpiy);
			Graphics g2 = Graphics.FromImage(backgroudImg);
			g2.Clear(Color.Transparent);
			g2.DrawImage(qrImg, 0, 0);
			PointF center2 = new PointF((qrImg.Width - headerBgImg.Width) / 2, (qrImg.Height - headerBgImg.Height) / 2);
			g2.DrawImage(headerBgImg, center2);
			g2.Dispose();
			return backgroudImg;
		}
		#endregion

		#region 图形处理
		/// <summary>
		/// 创建圆角矩形
		/// </summary>
		/// <param name="rect">区域</param>
		/// <param name="cornerRadius">圆角角度</param>
		/// <returns></returns>
		private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
		{
			GraphicsPath roundedRect = new GraphicsPath();
			roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
			roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
			roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
			roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
			roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
			roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
			roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
			roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
			roundedRect.CloseFigure();
			return roundedRect;
		}
		/// <summary>
		/// 图片按比例缩放
		/// </summary>
		private Image ZoomPic(Image initImage, double n)
		{
			//缩略图宽、高计算
			double newWidth = initImage.Width;
			double newHeight = initImage.Height;
			newWidth = n * initImage.Width;
			newHeight = n * initImage.Height;
			//生成新图
			//新建一个bmp图片
			Image newImage = new Bitmap((int)newWidth, (int)newHeight);
			//新建一个画板
			Graphics newG = Graphics.FromImage(newImage);
			//设置质量
			newG.InterpolationMode = InterpolationMode.HighQualityBicubic;
			newG.SmoothingMode = SmoothingMode.HighQuality;
			//置背景色
			newG.Clear(Color.Transparent);
			//画图
			newG.DrawImage(initImage, new Rectangle(0, 0, newImage.Width, newImage.Height), new Rectangle(0, 0, initImage.Width, initImage.Height), GraphicsUnit.Pixel);
			newG.Dispose();
			return newImage;
		}
		#endregion
	}
}
