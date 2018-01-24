using System;
using System.IO;
using System.Windows;
using System.Collections.Generic; //Dictionary<TKey, TValue>的使用

using HtmlAgilityPack; //HTML存取
using System.Xml;


//尚未撰寫：點轉檔前，確認有無檔案，並彈出確認視窗

namespace RPG_Guild
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //string SourceHtmlDir;
        string[] filePaths;
        private Dictionary<string, string> ExportSetting = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadExportSetting();
        }


        /*--階段的按鈕--*/
        private void setDefault_Click(object sender, RoutedEventArgs e) //開啟設定
        {
            SettingWindow sw = new SettingWindow(ExportSetting);
            //取得目前視窗的位置
            sw.Left = this.Left + 50;
            sw.Top = this.Top + 50;
            // sw.ShowDialog(); //顯示
            if (sw.ShowDialog() == true) //從設定中取值
            {
                ExportSetting = sw.ApplySetting();
                selSourceDir.IsEnabled = true;
                modAll.IsEnabled = true;
                showFileList(ExportSetting["SelectionPath_Path"]);
            }
        }
        private void SourceDir_Click(object sender, RoutedEventArgs e)
        {
            /*--作用區域--*/
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = ExportSetting["SelectionPath_Path"];
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbFileList.Items.Clear();
                ExportSetting["SelectionPath_Path"] = dialog.SelectedPath; //取得選擇的路徑
                filePaths = Directory.GetFiles(dialog.SelectedPath, "*.html", SearchOption.TopDirectoryOnly);
                Array.Sort(filePaths);
                Array.Reverse(filePaths);
                foreach (string s in filePaths)
                {
                    string path = Path.GetFileNameWithoutExtension(s);
                    lbFileList.Items.Add(path);
                }
                lbDesc.Content = "雙擊列表中的檔案，或是點【全部轉換】";
            }

        }

        private void lbFileList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (lbFileList.HasItems)
                {
                    string FileName = lbFileList.SelectedItem.ToString() + @".html";
                    if (File.Exists(getCurrentOutputPath() + "\\" + Path.GetFileName(FileName)))
                    {
                        showLog.Text = "";
                        showLog.Text += "忽略：" + Path.GetFileName(FileName) + "...檔案已存在.\n";
                    }
                    else
                    {
                         if (!System.IO.Directory.Exists(getCurrentOutputPath()))
                        {
                            if (MessageBox.Show("「" + getCurrentOutputPath() + "」的資料夾不存在，是否要馬上建立資料夾?", "tip", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                            {
                                Directory.CreateDirectory(getCurrentOutputPath());
                            }
                            else
                            {
                                return;
                            }
                        }
                        showLog.Text = "";

                        ModHtmlTools mht = new ModHtmlTools(lbFileList.SelectedItem.ToString(), ExportSetting);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
                showLog.Text = "嗚喔，出錯了: " + ex.Message;
                showLog.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        private void modAll_Click(object sender, RoutedEventArgs e)
        {

            if (filePaths == null || filePaths.Length == 0) {
                showLog.Text = "選擇好【存串資料夾】了嗎?";
                showLog.Foreground = System.Windows.Media.Brushes.OrangeRed;
            }
            else {
                showLog.Text = "";
                int countIgnore = 0;
                int countMod = 0;

                foreach (string file in filePaths)
                {
                    
                    try
                    {
                        if (File.Exists(getCurrentOutputPath() + "\\" + Path.GetFileName(file)))
                        {
                            showLog.Text += "忽略：" + Path.GetFileName(file) + "...檔案已存在.\n";
                            countIgnore++;
                        }
                        else
                        {
                            if (!System.IO.Directory.Exists(getCurrentOutputPath()))
                            {
                                if (MessageBox.Show("「" + getCurrentOutputPath() + "」的資料夾不存在，是否要馬上建立資料夾?", "tip", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                                {
                                    Directory.CreateDirectory(getCurrentOutputPath());
                                }
                                else
                                {
                                    return;
                                }
                            }
                            ModHtmlTools mht = new ModHtmlTools(Path.GetFileNameWithoutExtension(file), ExportSetting);
                            countMod++;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());    
                        showLog.Text += "轉換失敗: " + file + "\n";
                        showLog.Foreground = System.Windows.Media.Brushes.Red;
                        continue;
                    }
                }
                showLog.Text += @"========================================" + "\n 總計： 忽略 " + countIgnore + " 個檔案，轉換 " + countMod + " 檔案。";
                //                je.Close();
            }
        }




        /*--預設值--*/
        private void LoadExportSetting() //讀取之前存在XML中的輸出設定 
        {
            //如果已經有設定過，則載入預設值
            try
            {
                XmlDocument getDefault = new XmlDocument();
                getDefault.Load("CustSet.xml"); //找不到檔案會有Exception

                XmlNode node = getDefault.SelectSingleNode("Custom_Setting/SelectionPath");//選擇節點
                XmlElement elm = (XmlElement)node;
                ExportSetting.Add("SelectionPath_Path", elm.GetAttribute("Path"));

                node = getDefault.SelectSingleNode("Custom_Setting/OutputPath");//選擇節點
                elm = (XmlElement)node;

                ExportSetting.Add("OutputPath_Type", elm.GetAttribute("Type"));
                ExportSetting.Add("OutputPath_XPath", elm.GetAttribute("XPath"));
                ExportSetting.Add("OutputPath_FixPath", elm.GetAttribute("FixPath"));

                node = getDefault.SelectSingleNode("Custom_Setting/ImageLimit");//選擇節點
                elm = (XmlElement)node;
                ExportSetting.Add("ImageLimit_MaxCSSWidth", elm.GetAttribute("MaxCCSWidth"));
                ExportSetting.Add("ImageLimit_MaxCSSHeight", elm.GetAttribute("MaxCCSHeight"));
                ExportSetting.Add("ImageLimit_MaxKilobyte", elm.GetAttribute("MaxKilobyte"));
                ExportSetting.Add("ImageLimit_MaxZipedWidth", elm.GetAttribute("MaxZipedWidth"));
                ExportSetting.Add("ImageLimit_MaxZipedHeight", elm.GetAttribute("MaxZipedHeight"));

                showFileList(ExportSetting["SelectionPath_Path"]);  //已經有預設值的會直接載入FileList
            }
            catch (FileNotFoundException ex)
            {
                //如果找不到，就給予預設值
                ExportSetting.Add("SelectionPath_Path", "");
                ExportSetting.Add("OutputPath_Type", "XPath"); //選擇相對路徑還是絕對路徑
                ExportSetting.Add("OutputPath_XPath", "output"); //相對路徑的資料夾名稱
                ExportSetting.Add("OutputPath_FixPath", ""); //絕對路徑
                ExportSetting.Add("ImageLimit_MaxCSSWidth", "932"); //網頁顯示的圖片寬度
                ExportSetting.Add("ImageLimit_MaxCSSHeight", "512"); //網頁顯示的圖片高度
                ExportSetting.Add("ImageLimit_MaxKilobyte", "256"); //當大於多少KB的圖片會進行壓縮
                ExportSetting.Add("ImageLimit_MaxZipedWidth", "1024");
                ExportSetting.Add("ImageLimit_MaxZipedHeight", "768");

                selSourceDir.IsEnabled = false;
                modAll.IsEnabled = false;

            }
        }

        /*--副程式--*/
        private void showFileList(string SelectedPath) {
            lbFileList.Items.Clear();
            filePaths = Directory.GetFiles(SelectedPath, "*.html", SearchOption.TopDirectoryOnly);
            Array.Sort(filePaths);
            Array.Reverse(filePaths);
            foreach (string s in filePaths)
            {
                string path = Path.GetFileNameWithoutExtension(s);
                lbFileList.Items.Add(path);
            }
            lbDesc.Content = "雙擊列表中的檔案，或是點【全部轉換】";
        }
        private string getCurrentOutputPath() //取得當下的輸出位置
        {
            if (ExportSetting["OutputPath_Type"] == "XPath")
            {
                return ExportSetting["SelectionPath_Path"] + "\\" + ExportSetting["OutputPath_XPath"];
            }
            else {
                return ExportSetting["OutputPath_FixPath"];
            }
        }

        /*--測試區--*/
    }
}

/*參考資料
 * OPEN 2nd Windows: http://stackoverflow.com/questions/11133947/how-to-open-second-window-from-first-window-in-wpf
 * 開新視窗之ShowDialog：http://oblivious9.pixnet.net/blog/post/192683977-c%23-%E5%A4%9A%E5%80%8Bform%E9%96%93%E5%88%87%E6%8F%9B
 * 預設視窗開啟時的位置：http://stackoverflow.com/questions/2734810/how-to-set-the-location-of-a-wpf-window
 * //測試中方法
 * 當畫面卡住時GIF繼續執行：http://stackoverflow.com/questions/15010166/wpf-how-to-show-gif-animation-when-gui-is-freezy
 * 用backgroundwork做多執行續：http://stackoverflow.com/questions/5483565/how-to-use-wpf-background-worker
 *  * http://charlesbc.blogspot.tw/2011/06/wpfui.html
 * 
 *格式設定：http://stackoverflow.com/questions/8995678/how-to-set-a-default-margin-for-all-the-controls-on-all-my-wpf-windows
 
*/
