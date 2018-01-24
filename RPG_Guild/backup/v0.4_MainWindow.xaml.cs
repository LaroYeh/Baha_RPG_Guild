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
        string[] filePaths;

        public MainWindow()
        {
            InitializeComponent();
        }
        /* //以開啟單一檔案的方式執行轉換，不過需要大修，已不再使用。能作為新功能的參考
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
        */
       
        /*--階段的按鈕--*/
        private void SourceDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() ==System.Windows.Forms.DialogResult.OK)
            {
                SourceHtmlDir = dialog.SelectedPath; //將選取的路徑給static的SourceHtmlDir
                filePaths = Directory.GetFiles(dialog.SelectedPath, "*.html", SearchOption.TopDirectoryOnly);
                Array.Sort(filePaths);
                Array.Reverse(filePaths);
                foreach (string s in filePaths)
                {
                    string path = Path.GetFileNameWithoutExtension(s);
                    showFileList.Items.Add(path);
                }
                lbDesc.Content = "步驟2. 雙擊列表中的檔案，或是點【全部轉換】";
            }

        }

        private void showFileList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (showFileList.HasItems)
            {
                showLog.Text = "";
                modHtml(SourceHtmlDir + "\\" + showFileList.SelectedItem + ".html");

            }
            try {

            }
            catch (Exception ex) {
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
                        if (File.Exists(SourceHtmlDir + "\\output" + "\\" + Path.GetFileName(file)))
                        {
                            showLog.Text += "忽略：" + Path.GetFileName(file) + "...檔案已存在.\n";
                            countIgnore++;
                        }
                        else
                        {
                            modHtml(file);
                            countMod++;
                        }
                    }
                    catch (Exception ex)
                    {
                        showLog.Text += "轉換失敗: " + file + "\n";
                        showLog.Foreground = System.Windows.Media.Brushes.Red;
                        continue;
                    }
                }
                showLog.Text += @"========================================" + "\n 總計： 忽略 " + countIgnore + " 個檔案，轉換 " + countMod + " 檔案。";
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

            //showLog.Text = "S_Dir: " + SourceHtmlDir + "\n" + "S_File: " + SourceHtmlFile + "\n" + "FileName: " + FileName + "\n" + "SubName: " + SubFileName;
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
            string strOutputPath = SourceHtmlDir+"\\" + @"\output";
            if (!System.IO.Directory.Exists(strOutputPath))
            {
                Directory.CreateDirectory(strOutputPath);
                if (!System.IO.File.Exists(strOutputPath + openFileDialog1.SafeFileName))
                    File.Create(strOutputPath).Close(); //如不存在則新建檔案
            }

            modLog.Save(strOutputPath+"\\" + openFileDialog1.SafeFileName, UTF8Encoding.UTF8);
        }
        private void modHtml(String FilePath)
        {
            string SourceHtmlFile = Path.GetFileName(FilePath);
            string FileName = Path.GetFileNameWithoutExtension(FilePath); ;
            string SubFileName = Path.GetFileName(FilePath).Split('.')[1];
            string relDirPath = "./" + FileName + "_files/"; //來源網頁放圖片與檔案的資料夾相對路徑

            showLog.Text += FileName +@"...完成!"+"\n";
            showLog.Foreground = System.Windows.Media.Brushes.ForestGreen;
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
            try {
                foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@class='msgdel AB1']")) //「刪除」的按鈕，如果沒有入串，則不會出現
                {
                    node.RemoveAll();
                }
            }
            catch (Exception ex) {  }
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
            try //如果整串都沒有圖，這個會找不到
            {
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
                        try//如果無法下載圖片就放棄轉換，維持失連
                        { 
                            byte[] imageBytes = myWebClient.DownloadData(strDataSrc);

                            if (imageBytes.Length > 524288) //如果圖片尺寸超過500KB就壓縮
                            {
                                imageBytes = fixLargeImg(imageBytes);
                            }
                            node.SetAttributeValue("data-src", "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes));
                        }
                        catch (WebException ex)
                        {
                            showLog.Text += "※部分圖案已失連"+"\n";
                            showLog.Foreground = System.Windows.Media.Brushes.OrangeRed;
                        }

                    }
                    /*--------------------------*/
                }
            }
            catch (Exception ex) {
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
            MemoryStream sImg = new MemoryStream(bImg);
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

            byte[] bytes = new byte[sImg.Length];
            bytes = sImg.ToArray();
            return bytes;

            //return StreamToBytes(sImg);
        }
        private byte[] StreamToBytes(Stream stream) //將stream轉byte[]
        {
            
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Dispose();
            return bytes;
        }


    }
}
// 2016-11-22  290那列動到fixLargeImg()，不知道為什麼有異常