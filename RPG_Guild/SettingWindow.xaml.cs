using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

//如果有增減任何項目，於開頭的載入、末端的儲存，都要記得添加

namespace BAHA_MsgSave
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingWindow
    {
        private Dictionary<string, string> _ExportSetting;
        /*ExportSetting中的Key值有
         * "SelectionPath_Path","OutputPath_Type","OutputPath_XPath", "OutputPath_FixPath"
         * ImageLimit_MaxCSSWidth", "ImageLimit_MaxCSSHeight", 
         * ImageLimit_MaxKilobyte", "ImageLimit_MaxZipedWidth", "ImageLimit_MaxZipedHeight" */


        public SettingWindow(Dictionary<string, string> ExportSetting)
        {
            InitializeComponent();
            _ExportSetting = ExportSetting;

            tbInputPath.Text = _ExportSetting["SelectionPath_Path"];
            if(_ExportSetting["OutputPath_Type"]=="XPath") {
                rbXPath.IsChecked =true; }
            else {
                rbFixPath.IsChecked = true; }

            tbOutputFolder.Text = _ExportSetting["OutputPath_XPath"];
            tbOutputPath.Text = _ExportSetting["OutputPath_FixPath"];

            //圖片
            tbImgMaxWidth.Text = _ExportSetting["ImageLimit_MaxCSSWidth"];
            tbImgMaxHeight.Text = _ExportSetting["ImageLimit_MaxCSSHeight"];
            tbImgMaxKilobyte.Text = _ExportSetting["ImageLimit_MaxKilobyte"];
            tbImgZipedMaxWidth.Text = _ExportSetting["ImageLimit_MaxZipedWidth"];
            tbImgZipedMaxHeight.Text = _ExportSetting["ImageLimit_MaxZipedHeight"];

            //Style
            if (_ExportSetting["Style_Type"] == "Simple")
            {
                rbSimpleHTMLStyle.IsChecked = true;
            }
            else
            {
                rbAdvancedHTMLStyle.IsChecked = true;
            }


            tbMSGBlockWidth.Text = _ExportSetting["Style_MSGBlockWidth"];

            tbOriStyleHead.Text = _ExportSetting["Style_OriStyleHead"];
            tbAfterStyleHead.Text = _ExportSetting["Style_AfterStyleHead"];
            tbOriStyleBody.Text = _ExportSetting["Style_OriStyleBody"];
            tbAfterStyleBody.Text = _ExportSetting["Style_AfterStyleBody"];
            tbStyleBahaAvatar.Text = _ExportSetting["Style_StyleBahaAvatar"];
            tbStyleImage.Text = _ExportSetting["Style_StyleImage"];

            tbUselessIconReply.Text = _ExportSetting["Style_UselessIconReply"];
            tbUselessIconDel.Text = _ExportSetting["Style_UselessIconDel"];
            tbUselessIconGuild.Text = _ExportSetting["Style_UselessIconGuild"];
        }

        private void btInputPath_Click(object sender, RoutedEventArgs e) //選擇 預設資料夾
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = tbInputPath.Text;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbInputPath.Text = dialog.SelectedPath; //顯示選取的路徑
            }
        }      
        private void btOutputPath_Click(object sender, RoutedEventArgs e) //選擇 輸出資料夾
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rbFixPath.IsChecked = true;
                tbOutputPath.Text = dialog.SelectedPath; //顯示選取的路徑
            }
        }

        private void setDefault_Click(object sender, RoutedEventArgs e) //儲存預設值，暫且每次新增
        {
            //防呆
            if(tbInputPath.Text=="")
            {
                MessageBox.Show("「輸入資料夾」不能留白喔", "再檢查一下?", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(rbXPath.IsChecked==true)
            {
                if (tbOutputFolder.Text == "")
                {
                    MessageBox.Show("要輸入資料夾名稱喔！", "再檢查一下?", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    //檢查資料夾是否已經存在，如無則建立

                    string strOutputPath = tbInputPath.Text + "\\" + tbOutputFolder.Text;
                    if (!System.IO.Directory.Exists(strOutputPath))
                    {
                        if (MessageBox.Show("「"+strOutputPath+"」的資料夾不存在，是否要馬上建立資料夾?", "tip", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Directory.CreateDirectory(strOutputPath);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            if (rbFixPath.IsChecked==true && tbOutputPath.Text =="")
            {
                MessageBox.Show("選「自選輸出路徑」時，一定要選擇輸出資料夾喔！", "再檢查一下?", MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            //開始儲存XML
            XmlDocument doc = new XmlDocument();
            XmlElement CustSet = doc.CreateElement("Custom_Setting");
            doc.AppendChild(CustSet);

            XmlElement nodeSelPath = doc.CreateElement("SelectionPath"); //「選擇資料夾」的初始路徑
            CustSet.AppendChild(nodeSelPath);
            nodeSelPath.SetAttribute("Path", tbInputPath.Text);
            
            //Output
            XmlElement nodeOutputPath = doc.CreateElement("OutputPath"); //「選擇資料夾」的初始路徑
            CustSet.AppendChild(nodeOutputPath);
            if (rbXPath.IsChecked == true)
            {
                nodeOutputPath.SetAttribute("Type", "XPath");
            }
            else
            {
                nodeOutputPath.SetAttribute("Type", "FixPath");
            }
            nodeOutputPath.SetAttribute("XPath", tbOutputFolder.Text); //選相對位置下的資料夾
            nodeOutputPath.SetAttribute("FixPath", tbOutputPath.Text); //選指定輸出位置 

            //Image
            XmlElement nodeImgSize = doc.CreateElement("ImageLimit");
            CustSet.AppendChild(nodeImgSize);
            nodeImgSize.SetAttribute("MaxCCSWidth", tbImgMaxWidth.Text);
            nodeImgSize.SetAttribute("MaxCCSHeight", tbImgMaxHeight.Text);
            nodeImgSize.SetAttribute("MaxKilobyte", tbImgMaxKilobyte.Text);
            nodeImgSize.SetAttribute("MaxZipedWidth", tbImgZipedMaxWidth.Text);
            nodeImgSize.SetAttribute("MaxZipedHeight", tbImgZipedMaxHeight.Text);

            //Style
            XmlElement style = doc.CreateElement("Style");
            CustSet.AppendChild(style);
            if (rbSimpleHTMLStyle.IsChecked == true)
            {
                style.SetAttribute("Type", "Simple");
            }
            else
            {
                style.SetAttribute("Type", "Advanced");
            }
            style.SetAttribute("MSGBlockWidth", tbMSGBlockWidth.Text);
            style.SetAttribute("OriStyleHead", tbOriStyleHead.Text);
            style.SetAttribute("AfterStyleHead", tbAfterStyleHead.Text);
            style.SetAttribute("OriStyleBody", tbOriStyleBody.Text);
            style.SetAttribute("AfterStyleHead", tbAfterStyleHead.Text);
            style.SetAttribute("OriStyleBody", tbOriStyleBody.Text);
            style.SetAttribute("AfterStyleBody", tbAfterStyleBody.Text);
            style.SetAttribute("StyleBahaAvatar", tbStyleBahaAvatar.Text);
            style.SetAttribute("StyleImage", tbStyleImage.Text);
            style.SetAttribute("UselessIconReply", tbUselessIconReply.Text);
            style.SetAttribute("UselessIconDel", tbUselessIconDel.Text);
            style.SetAttribute("UselessIconGuild", tbUselessIconGuild.Text);

            doc.Save("CustSet.xml"); //存檔

            this.DialogResult = true;
            this.Close();
        }
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        internal Dictionary<string, string> ApplySetting() {
            _ExportSetting["SelectionPath_Path"] = tbInputPath.Text;
            _ExportSetting["OutputPath_Type"] = rbXPath.IsChecked == true ? "XPath" : "FixPath";
            _ExportSetting["OutputPath_XPath"] = tbOutputFolder.Text;
            _ExportSetting["OutputPath_FixPath"] = tbOutputPath.Text;
            _ExportSetting["ImageLimit_MaxCSSWidth"] =tbImgMaxWidth.Text;
            _ExportSetting["ImageLimit_MaxCSSHeight"] =tbImgMaxHeight.Text;
            _ExportSetting["ImageLimit_MaxKilobyte"] =tbImgMaxKilobyte.Text;
            _ExportSetting["ImageLimit_MaxZipedWidth"] =tbImgZipedMaxWidth.Text;
            _ExportSetting["ImageLimit_MaxZipedHeight"] =tbImgZipedMaxHeight.Text;
            _ExportSetting["Style_Type"] = rbSimpleHTMLStyle.IsChecked == true ? "Simple" : "Advanced";
            _ExportSetting["Style_MSGBlockWidth"] = tbMSGBlockWidth.Text;
            _ExportSetting["Style_OriStyleHead"] = tbOriStyleHead.Text;
            _ExportSetting["Style_AfterStyleHead"] = tbAfterStyleHead.Text;
            _ExportSetting["Style_OriStyleBody"] = tbOriStyleBody.Text;
            _ExportSetting["Style_AfterStyleBody"] = tbAfterStyleBody.Text;
            _ExportSetting["Style_StyleBahaAvatar"] = tbStyleBahaAvatar.Text;
            _ExportSetting["Style_StyleImage"] = tbStyleImage.Text;
            _ExportSetting["Style_UselessIconReply"] = tbUselessIconReply.Text;
            _ExportSetting["Style_UselessIconDel"] = tbUselessIconDel.Text;
            _ExportSetting["Style_UselessIconGuild"] = tbUselessIconGuild.Text;

            return _ExportSetting;
        }

        private void rbAdvancedHTMLStyle_Checked(object sender, RoutedEventArgs e)
        {
            expanderStyleHTML.IsExpanded = true;
        }

    }
}

/*參考資料
 * 【XML】
 * https://dotblogs.com.tw/yc421206/archive/2010/08/10/17108.aspx
 * https://msdn.microsoft.com/zh-tw/library/system.xml.xmldocument(v=vs.110).aspx
 * TextBox單行：https://social.msdn.microsoft.com/Forums/vstudio/en-US/a845f31e-577c-4829-8166-75277c047955/enforcing-single-line-in-textblock?forum=wpf
 * 回傳資料給上一個視窗：https://dotblogs.com.tw/as15774/2012/02/02/67561
 * 取得MessageBox的Y/N：https://social.msdn.microsoft.com/Forums/vstudio/en-US/d3f223ac-7fca-486e-8939-adb46e9bf6c9/how-can-i-get-yesno-from-a-messagebox-in-wpf?forum=wpf
 
     */
