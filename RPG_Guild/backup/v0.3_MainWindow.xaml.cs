using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

//test
using HtmlAgilityPack;
using System.Net;

using System.Drawing; //壓縮圖片
using System.Drawing.Imaging; //壓縮圖片

namespace RPG_Guild
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string SourceHtmlDir;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void selSaveLog_Click(object sender, RoutedEventArgs e)
        {
            tbFinTxt.Text = "Please Wait.";
            tbFinTxt.Foreground = System.Windows.Media.Brushes.Black;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\Users\Cichol\Desktop\RPG公會\對串保存\";
            //Desktop Path: Environment.GetFolderPath(Environment.SpecialFolder.Desktop
            openFileDialog1.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    modHtml(openFileDialog1);
                    tbFinTxt.Text = "Done";
                    tbFinTxt.Foreground = System.Windows.Media.Brushes.ForestGreen;
                    //this.Close(); //轉換後自動關閉
                }
                catch (Exception ex)
                {
                    tbFinTxt.Text = "Oops 出錯囉: " + ex.Message;
                    tbFinTxt.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }

        /*--階段的按鈕--*/
        private void SourceDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() ==System.Windows.Forms.DialogResult.OK)
            {
                SourceHtmlDir = dialog.SelectedPath; //將選取的路徑給static的SourceHtmlDir
                string[] filePaths = Directory.GetFiles(dialog.SelectedPath, "*.html", SearchOption.TopDirectoryOnly);

                foreach (string s in filePaths)
                {
                    string path = Path.GetFileNameWithoutExtension(s);
                    showFileList.Items.Add(path);
                }
                lbDesc.Content = "Step 2. Choose a file.";
            }

        }

        private void modOne_Click(object sender, RoutedEventArgs e)
        {
            showLog.Text ="處理中...\n";
            modHtml(SourceHtmlDir + "\\" + showFileList.SelectedItem + ".html");
        }

        private void showFileList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try {
                if (showFileList.HasItems) {
                    showLog.Text = "處理中...\n";
                    modHtml(SourceHtmlDir + "\\" + showFileList.SelectedItem + ".html");
                    tbFinTxt.Text = "Done";
                    tbFinTxt.Foreground = System.Windows.Media.Brushes.ForestGreen;

                }
            }
            catch (Exception ex) {
                tbFinTxt.Text = "Oops 出錯囉: " + ex.Message;
                tbFinTxt.Foreground = System.Windows.Media.Brushes.Red;
            }
        }



        /*--主要轉換的功能--*/
        private void modHtml(OpenFileDialog openFileDialog1)
        {
            string SourceHtmlDir =Path.GetDirectoryName(openFileDialog1.FileName)+"\\";
            string SourceHtmlFile = Path.GetFileName(openFileDialog1.FileName);
            string FileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName); ;
            string SubFileName = Path.GetFileName(openFileDialog1.FileName).Split('.')[1];
            string relDirPath = "./" + FileName + "_files/"; //來源網頁放圖片與檔案的資料夾相對路徑

            showLog.Text = "S_Dir: " + SourceHtmlDir + "\n" + "S_File: " + SourceHtmlFile + "\n" + "FileName: " + FileName + "\n" + "SubName: " + SubFileName;
            //↑似乎能弄成讀入檔案的相關class

            //tmp
            Stream targetStream = File.Open(openFileDialog1.FileName, FileMode.Open);
            HtmlDocument modLog = new HtmlDocument(); //結果                
            modLog.Load(targetStream, Encoding.UTF8);
            targetStream.Close();

            //mod target html
            //target <head>...</head>
            HtmlNode nodeHead = modLog.DocumentNode.SelectNodes("//head")[0];
            HtmlNode nodeNewHead = HtmlNode.CreateNode(@"<head>
    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
    <title>RPG之幻想國度 - 巴哈姆特</title>
    <link href='https://i2.bahamut.com.tw/css/guild.css' rel='stylesheet' type='text/css'>
    <script src='https://i2.bahamut.com.tw/js/plugins/lazysizes-1.3.1.min.js' async=''></script>
    <style>.msgreport {width:1024px}</style>
<head>");
            nodeHead.ParentNode.ReplaceChild(nodeNewHead, nodeHead);

            //target <body>...</body>
            HtmlNode nodeBody = modLog.DocumentNode.SelectNodes("//body")[0];
            HtmlNode nodeNewBody = modLog.DocumentNode.SelectNodes("//div[@id='BH-master']")[0];
            nodeBody.RemoveAll();
            nodeBody.AppendChild(nodeNewBody);

            //target <replay> 移除不需要的物件，取消限制       
            //回覆對方的泡泡框、Lock圖片、公會圖片、翻除按鈕            
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@title='回覆他']"))
            {
                node.RemoveAll();
            }
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@src='" + relDirPath + "icon_lock.gif']"))
            {
                node.RemoveAll();
            }
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@class='msgdel AB1']"))
            {
                node.RemoveAll();
            }
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@href='http://guild.gamer.com.tw/guild.php?sn=3014']"))
            {
                node.RemoveAll();
            }

            //target <img>...</img> 會隨時間改變的頭像則用連結
            //1.找到每串的開頭，然後找到圖片
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//div[@class='msgright']/a[@class='msgname AT1']/img|//div[@class='msgreport BC2']/a/img"))
            {
                string srcPath = node.Attributes["src"].Value;
                srcPath = srcPath.Remove(0, relDirPath.Length);
                char[] c_srcPath = srcPath.ToCharArray(0, 2);
                string UID = srcPath.Split('_')[0];

                node.SetAttributeValue("src", "https://avatar2.bahamut.com.tw/avataruserpic/" + c_srcPath[0] + "/" + c_srcPath[1] + "/" + UID + "/" + UID + "_s.png");
            }

            //target <img>...</img> 串中的圖檔存成二元碼
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@style='max-width:120px;max-height:120px;']"))
            {
                node.SetAttributeValue("Style", "max-width:932px;");
                /*---------轉二位元----------*/
                //gamer的是data-src, 會下載下來的是src
                string strDataSrc = node.GetAttributeValue("data-src", "");
                string strSrc = node.GetAttributeValue("src", "");
                if (strSrc != "") //如果放在資料夾的
                {
                    strSrc = SourceHtmlDir + strSrc.Remove(0, 2).Replace('/', '\\');
                    byte[] imageBytes = System.IO.File.ReadAllBytes(strSrc);
                    if (imageBytes.Length > 524288) //如果圖片尺寸超過500KB就壓縮
                    {
                        imageBytes = fixLargeImg(imageBytes);
                    }
                    node.SetAttributeValue("data-src", "");
                    node.SetAttributeValue("src", "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes)); //似乎png跟jpg都能用, 尚未確認gif是否可行
                }
                else //不是放在資料夾的，通常是網址直連
                {
                    WebClient myWebClient = new WebClient();
                    byte[] imageBytes = myWebClient.DownloadData(strDataSrc);
                    if (imageBytes.Length > 524288) //如果圖片尺寸超過500KB就壓縮
                    {
                        imageBytes = fixLargeImg(imageBytes);
                    }
                    node.SetAttributeValue("data-src", "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes));
                }
                /*--------------------------*/
            }


            //Modified SaveLog
            string strOutputPath = SourceHtmlDir + @"output";
            if (!System.IO.Directory.Exists(strOutputPath))
            {
                Directory.CreateDirectory(strOutputPath);
                if (!System.IO.File.Exists(strOutputPath + openFileDialog1.SafeFileName))
                    File.Create(strOutputPath).Close(); //如不存在則新建檔案
            }

            modLog.Save(@"C:\Users\Cichol\Desktop\RPG公會\對串保存\output\" + openFileDialog1.SafeFileName, UTF8Encoding.UTF8);
        }
        private void modHtml(String FilePath)
        {
            string SourceHtmlFile = Path.GetFileName(FilePath);
            string FileName = Path.GetFileNameWithoutExtension(FilePath); ;
            string SubFileName = Path.GetFileName(FilePath).Split('.')[1];
            string relDirPath = "./" + FileName + "_files/"; //來源網頁放圖片與檔案的資料夾相對路徑

            showLog.Text += FileName +@"...OK!"+"\n";
            //↑似乎能弄成讀入檔案的相關class

            //tmp
            Stream targetStream = File.Open(FilePath, FileMode.Open);
            HtmlDocument modLog = new HtmlDocument(); //結果                
            modLog.Load(targetStream, Encoding.UTF8);
            targetStream.Close();

            //mod target html
            //target <head>...</head>
            HtmlNode nodeHead = modLog.DocumentNode.SelectNodes("//head")[0];
            HtmlNode nodeNewHead = HtmlNode.CreateNode(@"<head>
    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
    <title>RPG之幻想國度 - 巴哈姆特</title>
    <link href='https://i2.bahamut.com.tw/css/guild.css' rel='stylesheet' type='text/css'>
    <script src='https://i2.bahamut.com.tw/js/plugins/lazysizes-1.3.1.min.js' async=''></script>
    <style>.msgreport {width:1024px}</style>
<head>");
            nodeHead.ParentNode.ReplaceChild(nodeNewHead, nodeHead);

            //target <body>...</body>
            HtmlNode nodeBody = modLog.DocumentNode.SelectNodes("//body")[0];
            HtmlNode nodeNewBody = modLog.DocumentNode.SelectNodes("//div[@id='BH-master']")[0];
            nodeBody.RemoveAll();
            nodeBody.AppendChild(nodeNewBody);

            //target <replay> 移除不需要的物件，取消限制       
            //回覆對方的泡泡框、Lock圖片、公會圖片、翻除按鈕            
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@title='回覆他']"))
            {
                node.RemoveAll();
            }
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@src='" + relDirPath + "icon_lock.gif']"))
            {
                node.RemoveAll();
            }
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@class='msgdel AB1']"))
            {
                node.RemoveAll();
            }
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@href='http://guild.gamer.com.tw/guild.php?sn=3014']"))
            {
                node.RemoveAll();
            }

            //target <img>...</img> 會隨時間改變的頭像則用連結
            //1.找到每串的開頭，然後找到圖片
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//div[@class='msgright']/a[@class='msgname AT1']/img|//div[@class='msgreport BC2']/a/img"))
            {
                string srcPath = node.Attributes["src"].Value;
                srcPath = srcPath.Remove(0, relDirPath.Length);
                char[] c_srcPath = srcPath.ToCharArray(0, 2);
                string UID = srcPath.Split('_')[0];

                node.SetAttributeValue("src", "https://avatar2.bahamut.com.tw/avataruserpic/" + c_srcPath[0] + "/" + c_srcPath[1] + "/" + UID + "/" + UID + "_s.png");
            }

            //target <img>...</img> 串中的圖檔存成二元碼
            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@style='max-width:120px;max-height:120px;']"))
            {
                node.SetAttributeValue("Style", "max-width:932px;");
                /*---------轉二位元----------*/
                //gamer的是data-src, 會下載下來的是src
                string strDataSrc = node.GetAttributeValue("data-src", "");
                string strSrc = node.GetAttributeValue("src", "");
                if (strSrc != "") //如果放在資料夾的
                {
                    strSrc = SourceHtmlDir+"\\" + strSrc.Remove(0, 2).Replace('/', '\\');

//                    byte[] imageBytes = System.IO.File.ReadAllBytes(@"C:\Users\Cichol\Desktop\RPG公會\對串保存\20161004【商業區】【預約】某個便利商店_files\638DpMAxm7zXp5hZrvKf.jpg");
                    byte[] imageBytes = System.IO.File.ReadAllBytes(strSrc);
                    if (imageBytes.Length > 524288) //如果圖片尺寸超過500KB就壓縮
                    {
                        imageBytes = fixLargeImg(imageBytes); //暫停使用
                    }
                    node.SetAttributeValue("data-src", "");
                    node.SetAttributeValue("src", "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes)); //似乎png跟jpg都能用, 尚未確認gif是否可行
                }
                else //不是放在資料夾的，通常是網址直連
                {
                    WebClient myWebClient = new WebClient();
                    byte[] imageBytes = myWebClient.DownloadData(strDataSrc);
                    if (imageBytes.Length > 524288) //如果圖片尺寸超過500KB就壓縮
                    {
                        imageBytes = fixLargeImg(imageBytes);
                    }
                    node.SetAttributeValue("data-src", "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes));
                }
                /*--------------------------*/
            }


            //Modified SaveLog
            string strOutputPath = SourceHtmlDir + @"\output";
            if (!System.IO.Directory.Exists(strOutputPath))
            {
                Directory.CreateDirectory(strOutputPath);
                if (!System.IO.File.Exists(strOutputPath + Path.GetFileName(FilePath)))
                    File.Create(strOutputPath).Close(); //如不存在則新建檔案
            }

            modLog.Save(strOutputPath+ "\\" + Path.GetFileName(FilePath), UTF8Encoding.UTF8);
        }
        /*--副功能--*/
        private byte[] fixLargeImg(byte[] bImg) //壓縮超過500KB的圖片
        {
            //參考http://demo.tc/post/95
            Stream sImg = new MemoryStream(bImg);
            Image img = Image.FromStream(sImg);
            ImageFormat thisformate = img.RawFormat;
            int fixWidth = img.Width;
            int fixHeight = img.Height;

            //計算新的長寬
            if (img.Width > img.Height)
            {
                fixWidth = 1024;
                fixHeight = img.Height * 1024 / img.Width;
            }
            else
            {
                fixWidth = img.Width * 1024 / img.Height;
                fixHeight = 1024;
            }

            sImg.Dispose(); //一定要釋放掉，stream不會縮水
            sImg = new MemoryStream();
            Bitmap imgOutput = new Bitmap(img, fixWidth, fixHeight);
            imgOutput.Save(sImg, ImageFormat.Jpeg);
            
            /*到前面為止都沒問題, 似乎是stream轉byte[]出問題*/

            //            return bImg;
                        return StreamToBytes(sImg);

            //return ReadFully(sImg);
        }
        public byte[] StreamToBytes(Stream stream) //將stream轉byte[]
        {
            
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Dispose();
            return bytes;
        }
    }
}
// 2016-11-22  290那列動到fixLargeImg()，不知道為什麼有異常