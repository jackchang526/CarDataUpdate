using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using ServiceTest.cs;
using System.Xml.Linq;
using System.Threading;
using System.Data.SqlClient;
using BitAuto.Utils.Data;
using BitAuto.Utils;

namespace ServiceTest
{
	public partial class Form2 : Form
	{
		public Form2()
		{
			InitializeComponent();
		}
		private void Form2_Load(object sender, EventArgs e)
		{
			XmlDocument doc = new XmlDocument();
			using (XmlReader reader = XmlReader.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config\\msgtypes.config")))
			{
				doc.Load(reader);
			}
			foreach( XmlNode node in doc.SelectNodes("//mstypes/add") )
			{
				this.comboBox1.Items.Add(new mstype() { name = node.Attributes["name"].Value, control = node.Attributes["control"].Value });
			}

			this.comboBox1.SelectedIndex = 0;
		}
        #region tabpage1 发消息
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            mstype t = (mstype)this.comboBox1.SelectedItem;

            this.panel1.Controls.Clear();

            Type type = Type.GetType(t.control, false, true);

            UserControl control = type.InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, null) as UserControl;
            control.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(control);
        } 
        #endregion



        #region tabpage2 生成晶赞数据
        private void tab2_button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(tab2_button1_ClickEvent));
            t.Start();
        }
        public void tab2_button1_ClickEvent()
        {
            int num, actionType;
            if (!int.TryParse(this.tab2_numbox.Text, out num) || num < 1)
            {
                MessageBox.Show("生成数量输入错误！");
                return;
            }
            if (!int.TryParse(this.tab2_state.Text, out actionType) || actionType < 1)
            {
                MessageBox.Show("状态输入错误！");
                return;
            }
            XDocument doc = null;
            XmlReader reader = null;
            try
            {
                tab2_msgbox.Invoke((MethodInvoker)delegate()
                {
                    tab2_msgbox.AppendText("start 加载xml文件...\r\n");
                });
                reader = XmlReader.Create(Path.Combine(AppConfig.DataPath, "MasterToBrandToSerialAllSaleAndLevel.xml"));
                doc = XDocument.Load(reader);
                tab2_msgbox.Invoke((MethodInvoker)delegate()
                {
                    tab2_msgbox.AppendText("end 加载xml文件...\r\n");
                });
            }
            catch (Exception exp)
            {
                MessageBox.Show("加载数据源异常！" + exp.ToString());
                return;
            }
            finally
            {
                if (reader != null && reader.ReadState != ReadState.Closed)
                    reader.Close();
            }

            if (doc == null)
                return;

            var csIds = (from item in doc.Descendants("Serial")
                         select Convert.ToInt32(item.Attribute("ID").Value)).ToArray();

            tab2_msgbox.Invoke((MethodInvoker)delegate()
            {
                tab2_msgbox.AppendText("共[" + csIds.Length + "]子品牌...\r\n");
            });

            if (num > csIds.Length)
                num = csIds.Length;

            tab2_msgbox.Invoke((MethodInvoker)delegate()
            {
                tab2_msgbox.AppendText("start 随机抽取[" + num + "]个子品牌...\r\n");
            });

            Random r = new Random();
            List<int> newCsIds = new List<int>(num);
            while (newCsIds.Count < num)
            {
                int csId = csIds[r.Next(0, num)];
                if (newCsIds.Contains(csId))
                    continue;
                else
                {
                    newCsIds.Add(csId);
                    string msg = string.Format("{0}/{1}...", newCsIds.Count, num);
                    tab2_msgbox.Invoke((MethodInvoker)delegate()
                    {
                        tab2_msgbox.Select(tab2_msgbox.GetFirstCharIndexOfCurrentLine(), tab2_msgbox.Lines[tab2_msgbox.Lines.Length - 1].Length);
                        tab2_msgbox.SelectedText = msg;
                    });
                }
            }

            tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("\r\n"); });

            tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("start 加载[SerialCoverWithout.xml]文件...\r\n"); });
            XmlDocument photoXml = new XmlDocument();
            using (XmlReader photoreader = XmlReader.Create(Path.Combine(AppConfig.DataPath, "photoimage\\SerialCoverWithout.xml")))
            {
                photoXml.Load(photoreader);
            }
            tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("end 加载[SerialCoverWithout.xml]文件...\r\n"); });

            tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("\r\n开始生成xml...\r\n"); });

            XDocument dataDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", null)
                , new XElement("products")
                );

            foreach (int csId in newCsIds)
            {
                tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("start " + csId.ToString()  +"...\r\n"); });

                XElement infoElem = new XElement(new XElement("product"
                            , new XElement("status", actionType)
                            , new XElement("id", csId)
                            )
                        );

                dataDoc.Root.Add(infoElem);

                /*
                1、厂家指导价 (guidePrice)
                2、排量(emission)
                3、变速箱(gearbox)
                4、产地(origin)
                5、国别(country)
                * 以上数据暂时不提供，代码以注释。
                */

                if (actionType == 4) //actionType = 4 删除
                {
                }
                else //actionType = 其他值
                {
                    XElement serialElem;
                    string comparisonCsIds //竞品子品牌ids
                        //, engine_Exhaust //排量
                        //, transmissionTypes //变速箱
                        , jiangjiaPrice//降价金额
                        , photoImageUrl = string.Empty//白底图(6)或普通封面图(4)，普通封面图不一定是300*200
                        //, refPrice = string.Empty
                        , price = string.Empty;

                    serialElem = doc.Descendants("Serial").First(ele => Convert.ToInt32(ele.Attribute("ID").Value) == csId);

                    #region photoImageUrl //白底图(6)或普通封面图(4)，普通封面图不一定是300*200

                    XmlNode photoNode = photoXml.SelectSingleNode(string.Format("SerialList/Serial[@SerialId='{0}']", csId));
                    if (photoNode != null)
                    {
                        if (photoNode.Attributes["ImageUrl2"] != null && !string.IsNullOrEmpty(photoNode.Attributes["ImageUrl2"].Value))
                        {
                            photoImageUrl = string.Format(photoNode.Attributes["ImageUrl2"].Value, "6");
                        }
                        else if (photoNode.Attributes["ImageUrl"] != null && !string.IsNullOrEmpty(photoNode.Attributes["ImageUrl"].Value))
                        {
                            photoImageUrl = string.Format(photoNode.Attributes["ImageUrl"].Value, "4");
                        }
                    }
                    #endregion

                    SqlParameter csSqlParam = new SqlParameter("@CsId", csId);

                    #region comparisonCsIds、engine_Exhaust、transmissionTypes

                    DataSet carDataSet = SqlHelper.ExecuteDataset(AppConfig.CarChannelConnString, CommandType.Text
                        , @"select top(10) PCs_Id
from Serial_To_Serial
where CS_Id=@CsId
order by Pv_Num desc;"
                        //select cei.Engine_Exhaust,cei.UnderPan_TransmissionType  
                        //from dbo.Car_Basic car  
                        //left join dbo.Car_Extend_Item cei on car.car_id = cei.car_id  
                        //left join Car_serial cs on car.cs_id = cs.cs_id  
                        //where car.isState=1 and cs.isState=1 and car.Car_SaleState<>'停销' and car.cs_id=@CsId order by Engine_Exhaust;"
                        , csSqlParam);

                    DataRowCollection rows = null;

                    #region comparisonCsIds
                    rows = carDataSet.Tables[0].Rows;
                    List<int> csids = new List<int>(rows.Count);
                    foreach (DataRow row in rows)
                    {
                        int compCsId = ConvertHelper.GetInteger(row["PCs_Id"].ToString());
                        if (compCsId > 0 && !csids.Contains(compCsId))
                        {
                            csids.Add(compCsId);
                        }
                    }
                    comparisonCsIds = publicmethod.joinList(csids);
                    #endregion

                    #region engine_Exhaust、transmissionTypes
                    /*
                rows = carDataSet.Tables[1].Rows;

                //排量列表
                List<string> exhaustList = new List<string>();
                //变速箱列表
                List<string> transList = new List<string>();
                foreach (DataRow row in rows)
                {
                    string tempEx = row["Engine_Exhaust"].ToString().Trim();
                    if (!exhaustList.Contains(tempEx))
                    {
                        exhaustList.Add(tempEx);
                    }
                    string tempTransmission = row["UnderPan_TransmissionType"].ToString().Trim();
                    if (tempTransmission.IndexOf("挡") >= 0)
                    {
                        tempTransmission = tempTransmission.Substring(tempTransmission.IndexOf("挡") + 1, tempTransmission.Length - tempTransmission.IndexOf("挡") - 1);
                    }
                    tempTransmission = tempTransmission.Replace("变速器", string.Empty).Replace("CVT", string.Empty);
                    if (transList.Count < 2)
                    {
                        if (tempTransmission.IndexOf("手动") == -1)
                            tempTransmission = "自动";
                        if (!transList.Contains(tempTransmission))
                            transList.Add(tempTransmission);
                    }
                }

                engine_Exhaust = String.Join(" ", exhaustList.ToArray() );

                transmissionTypes = String.Join(" ", transList.ToArray());
*/
                    #endregion
                    #endregion

                    #region jiangjiaPrice//降价金额

                    object jiangjiaObj = SqlHelper.ExecuteScalar(AppConfig.CarDataUpdateConnString, CommandType.Text
                        , "select MaxFavorablePrice from JiangJiaNewsSummary where serialid=@CsId and cityid=0"
                        , csSqlParam);

                    if (jiangjiaObj != null && !(jiangjiaObj is DBNull))
                    {
                        jiangjiaPrice = string.Format("直降{0}万", Convert.ToDecimal(jiangjiaObj).ToString("0.##"));
                    }
                    else
                    {
                        jiangjiaPrice = string.Empty;
                    }

                    #endregion

                    #region 生成xml

                    foreach (KeyValuePair<string, string> elem in new Dictionary<string, string>{
                    {"name",serialElem.Attribute("SerialSEOName").Value}
                    ,{"image", photoImageUrl}
                    ,{"landingPage", string.Format("http://car.bitauto.com/{0}/?WT.mc_jz=chexing", serialElem.Attribute("AllSpell").Value)}
                    ,{"imageWidth", "300"}
                    ,{"imageHeight", "200"}
                    ,{"brand", serialElem.Parent.Parent.Attribute("Name").Value}
                    ,{"category", serialElem.Attribute("CsLevel").Value}
                    ,{"subCategory", serialElem.Parent.Attribute("Name").Value}
                    ,{"thirdCategory", serialElem.Attribute("Name").Value}
                })
                    {
                        infoElem.Add(new XElement(elem.Key, elem.Value));
                    }

                    XElement moreAttr = new XElement("more_attributes");
                    infoElem.Add(moreAttr);

                    //decimal refMinPrice = ConvertHelper.GetDecimal(serialElem.Attributes["MinRP"].Value)
                    //    , refMaxPrice = ConvertHelper.GetDecimal(serialElem.Attributes["MaxRP"].Value);
                    decimal minPrice = ConvertHelper.GetDecimal(serialElem.Attribute("MinP").Value)
                        , maxPrice = ConvertHelper.GetDecimal(serialElem.Attribute("MaxP").Value);

                    //if (refMinPrice > 0 && refMaxPrice > 0)
                    //{
                    //    refPrice = string.Format("{0}万-{1}万", refMinPrice.ToString("#0.00"), refMaxPrice.ToString("#0.00"));
                    //}
                    if (minPrice > 0 && maxPrice > 0)
                    {
                        price = string.Format("{0}万-{1}万", minPrice.ToString("#0.00"), maxPrice.ToString("#0.00"));
                    }

                    foreach (var moreData in new List<TempSerialData>()
                {
                    new TempSerialData(){key="competingModelsId",name="竞品车型ID",value=comparisonCsIds}
                    ,new TempSerialData(){key="cut_Price",name="降价",value=jiangjiaPrice,url=string.Format("http://car.bitauto.com/{0}/jiangjia/?WT.mc_jz=jiangjia", serialElem.Attribute("AllSpell").Value)}

                    ,new TempSerialData(){key="quotedPrice",name="商家报价",value=price,url=string.Format("http://car.bitauto.com/{0}/baojia/?WT.mc_jz=baojia", serialElem.Attribute("AllSpell").Value)}

                    ////暂时不需要
                    //,new TempSerialData(){key="guidePrice",name="厂家指导价",value=refPrice}
                    //,new TempSerialData(){key="emission",name="排量",value=engine_Exhaust}
                    //,new TempSerialData(){key="gearbox",name="变速箱",value=transmissionTypes}
                    //,new TempSerialData(){key="origin",name="产地",value=serialElem.ParentNode.Attributes["Country"].Value}
                    //,new TempSerialData(){key="country",name="国别",value=serialElem.ParentNode.ParentNode.Attributes["Country"].Value}
                })
                    {
                        XElement tempElm = new XElement("attribute"
                            , new XElement("key", moreData.key)
                            , new XElement("name", moreData.name)
                            , new XElement("value", moreData.value)
                            );
                        if (!string.IsNullOrEmpty(moreData.url))
                        {
                            tempElm.Add(new XElement("url", moreData.url));
                        }
                        moreAttr.Add(tempElm);
                    }

                    #endregion
                }

                tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("end "+csId.ToString() + "...\r\n"); });
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMddHHmmssss") + ".xml");
            tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("start save path:" + path + "...\r\n"); });
            dataDoc.Save(path);
            tab2_msgbox.Invoke((MethodInvoker)delegate() { tab2_msgbox.AppendText("end save path:" + path + "...\r\n"); });
        }
        
        class TempSerialData
        {
            public string key;
            public string name;
            public string value;
            public string url;
        }
        #endregion

    }

}
