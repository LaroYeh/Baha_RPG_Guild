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
        static string testImagePath = @"C:\Users\Cichol\Desktop\RPG公會\對串保存\rola1989_s.png";
        static string testPath = @"C:\Users\Cichol\Desktop\RPG公會\對串保存\";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void selSaveLog_Click(object sender, RoutedEventArgs e)
        {
            string strInputPath = @"C:\Users\Cichol\Desktop\RPG公會\對串保存\";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = strInputPath;
            //Desktop Path: Environment.GetFolderPath(Environment.SpecialFolder.Desktop
            openFileDialog1.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    string[] FileName = openFileDialog1.SafeFileName.Split('.');
                    string relDirPath = "./" + FileName[0] + "_files/";
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
                    foreach(HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@title='回覆他']"))
                    {
                        node.RemoveAll();
                    }
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@src='"+relDirPath+"icon_lock.gif']"))
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
                        srcPath =srcPath.Remove(0, relDirPath.Length);
                        char[]  c_srcPath= srcPath.ToCharArray(0, 2);
                        string UID = srcPath.Split('_')[0];

                        node.SetAttributeValue("src", "https://avatar2.bahamut.com.tw/avataruserpic/" + c_srcPath[0] + "/" + c_srcPath[1] + "/" + UID + "/" + UID + "_s.png");
                    }                    
                    /*圖檔直接存放的有總數疊加導致尺寸過大的問題，尚未優化，沒寫完
                    HtmlNode tmpnode = HtmlNode.CreateNode(@"<script>
                        var Robin = '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';
                        function custImage(BahaId) {
                            return Robin;
                        }
                        </script>");
                    nodeNewHead.AppendChild(tmpnode);
                    */

                    //target <img>...</img> 串中的圖檔存成二元碼
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@style='max-width:120px;max-height:120px;']"))
                    {                        
                        node.SetAttributeValue("Style", "max-width:932px;");
                        /*---------轉二位元----------*/
                        //gamer的是data-src, 會下載下來的是src
                        string strDataSrc = node.GetAttributeValue("data-src", "");
                        string strSrc = node.GetAttributeValue("src","");
                        if (strSrc != "") //如果放在資料夾的
                        {
                            strSrc = strInputPath + strSrc.Remove(0, 2).Replace('/', '\\');
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
                    string strOutputPath = strInputPath + @"output\";
                    if(!System.IO.Directory.Exists(strOutputPath))
                    {
                        Directory.CreateDirectory(strOutputPath);
                        if (!System.IO.File.Exists(strOutputPath + openFileDialog1.SafeFileName))
                            File.Create(strOutputPath).Close(); //如不存在則新建檔案
                    }
                    
                    modLog.Save(@"C:\Users\Cichol\Desktop\RPG公會\對串保存\output\" + openFileDialog1.SafeFileName, UTF8Encoding.UTF8);
                    this.Close(); //轉換後自動關閉
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Oops 出錯囉: " + ex.Message);
                }
            }
        }
        private byte[] fixLargeImg(byte[] bImg) {
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
            return StreamToBytes(sImg);
        }
        public byte[] StreamToBytes(Stream stream) //將stream轉byte[]
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
    }
}
