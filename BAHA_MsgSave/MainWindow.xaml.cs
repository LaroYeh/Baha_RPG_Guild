using System;
using System.IO;
using System.Windows;
using System.Collections.Generic; //Dictionary<TKey, TValue>的使用
using System.Xml;
using System.Threading.Tasks;
using System.Threading;
using RPG_Guild_Lib.Lib;
using System.Collections.ObjectModel;

//尚未撰寫：點轉檔前，確認有無檔案，並彈出確認視窗

namespace BAHA_MsgSave
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

            #region 測試DataGrid
            /*
            //            ObservableCollection<testfile> memberData = new ObservableCollection<testfile>();
            List<string> LStr = new List<string>() { { "Baha" }, { "Plurk" } };
            testCombo.ItemsSource = LStr;

            List<testfile> memberData = new List<testfile>();
            memberData.Add(new testfile() {
                FileName = "test1", info = "進度100%", From = "Plurk"
            });
            memberData.Add(new testfile() {
                FileName = "test2", info="進度35%",
                From = "Baha"
            });
            memberData.Add(new testfile()
            {
                FileName = "test3",
                info = "進度0%",
                From = "Baha"
            });
            FileGrid.DataContext = memberData;*/
            #endregion
        }
        #region 測試DataGrid用class
        //參考資料
        //在DataGrid建立下拉選單 http://www.cnblogs.com/gnielee/archive/2010/04/16/wpf4-datagrid-control-basic.html
        //DataContext的多種寫法 http://www.huanlintalk.com/2012/12/wpf-datacontext.html
        //http://blog.csdn.net/rabbitsoft_1987/article/details/22896073
        //https://stackoverflow.com/questions/19798183/wpf-binding-in-datagrid-to-datacontext
        public enum DownloadFrom { Baha, Plurk}
        public class testfile {
            public string FileName { get; set; }
            public string From;
            public string info { get; set; }
        }
            public enum SexOpt { Male, Female };

    public class Member
    {
        public string Name { get; set; }
        public string Age { get; set; }
        public SexOpt Sex { get; set; }
        public bool Pass { get; set; }
        public Uri Email { get; set; }
    }
        #endregion

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
                ExportSetting["SelectionPath_Path"] = dialog.SelectedPath; //取得選擇的路徑
                showFileList(ExportSetting["SelectionPath_Path"]);
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
                        #region 過渡期的，改變說明的方法 20170819
                        showLog.Text = mht.getInfo()[0];
                        if (mht.getInfo()[1] == "ForestGreen")
                        {
                            showLog.Foreground = System.Windows.Media.Brushes.ForestGreen;
                        }
                        else if (mht.getInfo()[1] == "OrangeRed")
                        {
                            showLog.Foreground = System.Windows.Media.Brushes.OrangeRed;
                        }
                        #endregion
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



        #region 預設值
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

                //Image
                node = getDefault.SelectSingleNode("Custom_Setting/ImageLimit");//選擇節點
                elm = (XmlElement)node;
                ExportSetting.Add("ImageLimit_MaxCSSWidth", elm.GetAttribute("MaxCCSWidth"));
                ExportSetting.Add("ImageLimit_MaxCSSHeight", elm.GetAttribute("MaxCCSHeight"));
                ExportSetting.Add("ImageLimit_MaxKilobyte", elm.GetAttribute("MaxKilobyte"));
                ExportSetting.Add("ImageLimit_MaxZipedWidth", elm.GetAttribute("MaxZipedWidth"));
                ExportSetting.Add("ImageLimit_MaxZipedHeight", elm.GetAttribute("MaxZipedHeight"));

                //Style
                node = getDefault.SelectSingleNode("Custom_Setting/Style");//選擇節點
                elm = (XmlElement)node;
                ExportSetting.Add("Style_Type", elm.GetAttribute("Type"));
                ExportSetting.Add("Style_MSGBlockWidth", elm.GetAttribute("MSGBlockWidth"));
                ExportSetting.Add("Style_OriStyleHead", elm.GetAttribute("OriStyleHead"));
                ExportSetting.Add("Style_AfterStyleHead", elm.GetAttribute("AfterStyleHead"));
                ExportSetting.Add("Style_OriStyleBody", elm.GetAttribute("OriStyleBody"));
                ExportSetting.Add("Style_AfterStyleBody", elm.GetAttribute("AfterStyleBody"));
                ExportSetting.Add("Style_StyleBahaAvatar", elm.GetAttribute("StyleBahaAvatar"));
                ExportSetting.Add("Style_StyleImage", elm.GetAttribute("StyleImage"));
                ExportSetting.Add("Style_UselessIconReply", elm.GetAttribute("UselessIconReply"));
                ExportSetting.Add("Style_UselessIconDel", elm.GetAttribute("UselessIconDel"));
                ExportSetting.Add("Style_UselessIconGuild", elm.GetAttribute("UselessIconGuild"));


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
                ExportSetting.Add("Style_Type", "Simple"); //預設
                ExportSetting.Add("Style_MSGBlockWidth", "932"); //文字區塊顯示寬度
                ExportSetting.Add("Style_OriStyleHead", "//head");
                ExportSetting.Add("Style_AfterStyleHead", @"<head>
    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
    <title>RPG之幻想國度 - 巴哈姆特</title>
    <link href='https://i2.bahamut.com.tw/css/guild.css' rel='stylesheet' type='text/css'>
    <script src='https://i2.bahamut.com.tw/js/plugins/lazysizes-1.3.1.min.js' async=''></script>
    <style>.msgreport {width:932px}</style>
<head>");
                ExportSetting.Add("Style_OriStyleBody", "//body");
                ExportSetting.Add("Style_AfterStyleBody", "//div[@id='BH-master']");
                ExportSetting.Add("Style_StyleBahaAvatar", "//div[@class='msgright']/a[@class='msgname AT1']/img|//div[@class='msgreport BC2']/a/img");
                ExportSetting.Add("Style_StyleImage", "//img[@style='max-width:120px;max-height:120px;']");
                ExportSetting.Add("Style_UselessIconReply", "//a[@title='回覆他']|//div[@id='MSG-box2']/script");
                ExportSetting.Add("Style_UselessIconDel", "//a[@class='msgdel AB1']");
                ExportSetting.Add("Style_UselessIconGuild", "//a[@href='http://guild.gamer.com.tw/guild.php?sn=3014']");

                selSourceDir.IsEnabled = false;
                modAll.IsEnabled = false;

            }
        }
        #endregion
        
        #region 副程式
        private void showFileList(string SelectedPath)
        {
            lbFileList.Items.Clear();
            var _filelist = new List<string>();
            filePaths = Directory.GetFiles(SelectedPath, "*.html", SearchOption.TopDirectoryOnly);
            Array.Sort(filePaths);
            Array.Reverse(filePaths);
            foreach (string s in filePaths)
            {
                string path = Path.GetFileNameWithoutExtension(s);
                #region v0.6的ListBox
                lbFileList.Items.Add(path);
                #endregion
                #region v0.7的DataGrid
                _filelist.Add(path);
                #endregion

            }
            FileGrid.DataContext = _filelist;
            lbDesc.Content = "雙擊列表中的檔案，或是點【全部轉換】";
        }
        private string getCurrentOutputPath() //取得當下的輸出位置
        {
            if (ExportSetting["OutputPath_Type"] == "XPath")
            {
                return ExportSetting["SelectionPath_Path"] + "\\" + ExportSetting["OutputPath_XPath"];
            }
            else
            {
                return ExportSetting["OutputPath_FixPath"];
            }
        }
        #endregion

        #region Task測試區
        public Task<string> ConvertFile(string str)
        {
            return Task.Factory.StartNew(() => this._ConvertFile(str));
        }
        public string _ConvertFile(string para1) {
            Thread.Sleep(2000); //模擬轉檔時會停頓的時間
            return para1;
        }

        private async void test_Click(object sender, RoutedEventArgs e)
        {
            ////沒使用async & await
            //showLog.Text = "轉檔開始(2秒/個)\r\n";
            //for (int i = 0; i < 3; i++)
            //{
            //    showLog.Text += "開始第" + (int)(i + 1) + "個轉檔...";
            //    Thread.Sleep(2000); //模擬轉檔時會停頓的時間
            //    showLog.Text += "End\r\n";
            //}
            //showLog.Text += "轉檔全部結束\r\n";

            //使用async & await
            showLog.Text = "轉檔開始(2秒/個)\r\n";
            for (int i = 0; i < 3; i++)
            {
                showLog.Text += "開始第" + (int)(i + 1) + "個轉檔...";
                showLog.Text += await ConvertFile("End\r\n");
            }
            showLog.Text += "轉檔全部結束\r\n";

            //用預設方法回傳字串

        }
        #endregion
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
