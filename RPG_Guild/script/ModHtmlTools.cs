using System;
using System.IO;
using Microsoft.Win32;
using HtmlAgilityPack; //存取HTML的結構
using System.Text;
using System.Net; //使用WebClient
using System.Collections.Generic; //Dictionary<TKey, TValue>的使用

//Row 20, 150附近的程式碼直接遙控遠方控制項，未來希望能修正
namespace BAHA_MsgSave
{
    class ModHtmlTools
    {
        private Dictionary<string, string> _ExportSetting = new Dictionary<string, string>();
        private string InputHtmlPath;
        private string InputHtmlFile;
        private string relDirPath;
        private string OutputPath;
        /*ExportSetting中的Key值有
        * "SelectionPath_Path","OutputPath_Type","OutputPath_XPath", "OutputPath_FixPath"
        * ImageLimit_MaxCSSWidth", "ImageLimit_MaxCSSHeight", 
        * ImageLimit_MaxKilobyte", "ImageLimit_MaxZipedWidth", "ImageLimit_MaxZipedHeight" */


        public ModHtmlTools(string FileName, Dictionary<string, string> ExportSetting) {
            _ExportSetting = ExportSetting;
            InputHtmlPath = _ExportSetting["SelectionPath_Path"];
            InputHtmlFile = _ExportSetting["SelectionPath_Path"] + "\\" + FileName + ".html"; //來源檔案的完整路徑           
            relDirPath = "./" + FileName + "_files/"; //來源網頁放圖片與檔案的資料夾相對路徑
            OutputPath = _ExportSetting["SelectionPath_Path"] + "\\" + _ExportSetting["OutputPath_XPath"];
            if (_ExportSetting["OutputPath_Type"] != "XPath")
                OutputPath = _ExportSetting["SelectionPath_Path"] + "\\" + _ExportSetting["OutputPath_FixPath"];

            modHtml(FileName);
        }

        private void modHtml(string FileName)
        {
            //tmp
            Stream targetStream = File.Open(InputHtmlFile, FileMode.Open);
            HtmlDocument modLog = new HtmlDocument(); //結果                
            modLog.Load(targetStream, Encoding.UTF8);
            targetStream.Close();

            //mod target html
            //target <head>...</head>
            ModTargetHead(modLog);
            ModTargetBody(modLog); //target <body>...</body>
            ModTargetReplay(modLog, relDirPath); //target <replay> 移除不需要的物件，取消限制       
            ModTargetImgIcon(modLog, relDirPath); //target <img>...</img> 找到每串的開頭，將會隨時間改變的頭像用連結
            ModTargetImgToBinary(modLog, _ExportSetting["SelectionPath_Path"]);//target <img>...</img> 串中的圖檔存成二元碼


            //Modified SaveLog
            if (_ExportSetting["OutputPath_Type"] == "XPath") {
                string strOutputPath = _ExportSetting["SelectionPath_Path"] + "\\"+ _ExportSetting["OutputPath_XPath"];
                if (!System.IO.Directory.Exists(strOutputPath))
                {
                    Directory.CreateDirectory(strOutputPath);
                    if (!System.IO.File.Exists(strOutputPath + Path.GetFileName(InputHtmlFile)))
                        File.Create(strOutputPath).Close(); //如不存在則新建檔案
                }                
                modLog.Save(strOutputPath + "\\" + Path.GetFileName(InputHtmlFile), UTF8Encoding.UTF8);
            }
            else
            {
                string strOutputPath = _ExportSetting["OutputPath_FixPath"];
                if (!System.IO.Directory.Exists(strOutputPath))
                {
                    Directory.CreateDirectory(strOutputPath);
                    if (!System.IO.File.Exists(strOutputPath + Path.GetFileName(InputHtmlFile)))
                        File.Create(strOutputPath).Close(); //如不存在則新建檔案
                }
                modLog.Save(strOutputPath + "\\" + Path.GetFileName(InputHtmlFile), UTF8Encoding.UTF8);
            }
            //臨時性的方法，如更好的方法研究成功，建議改善
            ((MainWindow)System.Windows.Application.Current.MainWindow).showLog.Text += FileName + @"...完成!" + "\n";
            ((MainWindow)System.Windows.Application.Current.MainWindow).showLog.Foreground = System.Windows.Media.Brushes.ForestGreen;

        }

        private void ModTargetHead(HtmlDocument modLog) {
            //target <head>...</head>
            HtmlNode nodeHead = modLog.DocumentNode.SelectNodes("//head")[0];
            HtmlNode nodeNewHead = HtmlNode.CreateNode(@"<head>
    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
    <title>RPG之幻想國度 - 巴哈姆特</title>
    <link href='https://i2.bahamut.com.tw/css/guild.css' rel='stylesheet' type='text/css'>
    <script src='https://i2.bahamut.com.tw/js/plugins/lazysizes-1.3.1.min.js' async=''></script>
    <style>.msgreport {width:"+ _ExportSetting["Style_MSGBlockWidth"] + @"px}</style>
<head>");
            if (_ExportSetting["Style_Type"] == "Advanced") //如果是進階，則改寫預設值
            {
                nodeHead = modLog.DocumentNode.SelectNodes(_ExportSetting["Style_OriStyleHead"])[0];
                nodeNewHead = HtmlNode.CreateNode(_ExportSetting["Style_AfterStyleHead"]);
            }
            nodeHead.ParentNode.ReplaceChild(nodeNewHead, nodeHead);
        }
        private void ModTargetBody(HtmlDocument modLog) {
            //target <body>...</body>
            HtmlNode nodeBody = modLog.DocumentNode.SelectNodes("//body")[0];
            HtmlNode nodeNewBody = modLog.DocumentNode.SelectNodes("//div[@id='BH-master']")[0];
            if (_ExportSetting["Style_Type"] == "Advanced") //如果是進階，則改寫預設值
            {
                nodeBody = modLog.DocumentNode.SelectNodes(_ExportSetting["Style_OriStyleBody"])[0];
                nodeNewBody = modLog.DocumentNode.SelectNodes(_ExportSetting["Style_AfterStyleBody"])[0];
            }
            nodeBody.RemoveAll();
            nodeBody.AppendChild(nodeNewBody);
        }
        private void ModTargetReplay(HtmlDocument modLog, string relDirPath) //target <replay> 移除不需要的物件，取消限制  
        {
            if (_ExportSetting["Style_Type"] == "Simple") //如果用預設...
            {
                //移除「回覆對方的泡泡框、Lock圖片、刪除按鈕、公會圖片」
                try
                {
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@title='回覆他']|//div[@id='MSG-box2']/script"))
                    {
                        node.RemoveAll();
                    }
                }
                catch (Exception ex) { }
                try {
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@src='" + relDirPath + "icon_lock.gif']"))
                    {
                        node.RemoveAll();
                    }
                } catch (Exception ex) { }

                try
                {
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@class='msgdel AB1']")) //「刪除」的按鈕，如果沒有入串，則不會出現
                    {
                        node.RemoveAll();
                    }
                }
                catch (Exception ex) { }
                try
                {
                    //foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@href='http://guild.gamer.com.tw/guild.php?sn=3014']"))
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//a[@href='http://guild.gamer.com.tw/guild.php?sn=3014'] | //a[@href='https://guild.gamer.com.tw/guild.php?sn=3014']")) //2018.4.22 適應https
                    {
                        node.RemoveAll();
                    }
                }
                catch (Exception ex) { }
            }
            else //如果是進階...
            {
                //移除「回覆對方的泡泡框、Lock圖片、刪除按鈕、公會圖片」
                try
                {
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes(_ExportSetting["Style_UselessIconReply"]))
                    {
                        node.RemoveAll();
                    }
                }
                catch (Exception ex) { }
                try
                {
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes("//img[@src='" + relDirPath + "icon_lock.gif']"))
                    {
                        node.RemoveAll();
                    }
                }
                catch (Exception ex) { }
                try
                {
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes(_ExportSetting["Style_UselessIconDel"])) //「刪除」的按鈕，如果沒有入串，則不會出現
                    {
                        node.RemoveAll();
                    }
                }
                catch (Exception ex) { }
                try
                {
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes(_ExportSetting["Style_UselessIconGuild"]))
                    {
                        node.RemoveAll();
                    }
                }
                catch (Exception ex) { }
            }
        }
        private void ModTargetImgIcon(HtmlDocument modLog, string relDirPath) //找到每串的開頭，將會隨時間改變的頭像用連結
        {
            string nodeXPath = "//div[@class='msgright']/a[@class='msgname AT1']/img|//div[@class='msgreport BC2']/a/img";
            if (_ExportSetting["Style_Type"] == "Advanced") //如果是進階，則改寫預設值
                {
                nodeXPath = _ExportSetting["Style_StyleBahaAvatar"];
            }
            int i = 0;

            foreach (HtmlNode node in modLog.DocumentNode.SelectNodes(nodeXPath))
            {
                try { 
                    string srcPath = node.Attributes["src"].Value;
                    srcPath = srcPath.Remove(0, relDirPath.Length);
                    char[] c_srcPath = srcPath.ToCharArray(0, 2);
                    string UID = srcPath.Split('_')[0];
                    
                    node.SetAttributeValue("src", "https://avatar2.bahamut.com.tw/avataruserpic/" + c_srcPath[0] + "/" + c_srcPath[1] + "/" + UID + "/" + UID + "_s.png");
                    i++;
                }
                catch (Exception ex)
                {

                } //公會圖會抓不到，這邊不處理
            }
        }
        private void ModTargetImgToBinary(HtmlDocument modLog, string SourceHtmlDir) //target <img>...</img> 串中的圖檔存成二元碼
        {
            try //如果整串都沒有圖，這個會找不到
            {
                string nodeXPath = "//img[@style='max-width:120px;max-height:120px;']";
                if (_ExportSetting["Style_Type"] == "Advanced") //如果是進階，則改寫預設值
                {
                    nodeXPath = _ExportSetting["Style_StyleImage"];
                }
                
                    foreach (HtmlNode node in modLog.DocumentNode.SelectNodes(nodeXPath))
                {
                    node.SetAttributeValue("Style", "max-width:"+ _ExportSetting["ImageLimit_MaxCSSWidth"] + "px;"+ "max-height:" + _ExportSetting["ImageLimit_MaxCSSHeight"] + "px;");
                    /*---------轉二位元----------*/
                    //gamer的是data-src, 會下載下來的是src
                    string strDataSrc = node.GetAttributeValue("data-src", "");
                    string strSrc = node.GetAttributeValue("src", "");
                    if (strSrc != "") //如果放在資料夾的
                    {
                        strSrc = SourceHtmlDir + "\\" + strSrc.Remove(0, 2).Replace('/', '\\');

                        byte[] imageBytes = System.IO.File.ReadAllBytes(strSrc);
                        int MaxBytes = Convert.ToInt32(_ExportSetting["ImageLimit_MaxKilobyte"])*1024;
                        if (imageBytes.Length > MaxBytes) //如果圖片尺寸超過設定值就壓縮
                        {
                            ZipImageTools img = new ZipImageTools();
                            img.newHeight = Convert.ToInt32(_ExportSetting["ImageLimit_MaxZipedHeight"]);
                            img.newWidth = Convert.ToInt32(_ExportSetting["ImageLimit_MaxZipedWidth"]);
                            imageBytes = img.ZipImg(imageBytes);
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
                            int MaxBytes = Convert.ToInt32(_ExportSetting["ImageLimit_MaxKilobyte"]) * 1024;
                            if (imageBytes.Length > MaxBytes) //如果圖片尺寸超過設定值就壓縮
                            {
                                ZipImageTools img = new ZipImageTools();
                                img.newHeight = Convert.ToInt32(_ExportSetting["ImageLimit_MaxZipedHeight"]);
                                img.newWidth = Convert.ToInt32(_ExportSetting["ImageLimit_MaxZipedWidth"]);
                                imageBytes = img.ZipImg(imageBytes);
                            }
                            node.SetAttributeValue("data-src", "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes));
                        }
                        catch (WebException ex)
                        {
                            //臨時性的方法，如更好的方法研究成功，建議改善
                            ((MainWindow)System.Windows.Application.Current.MainWindow).showLog.Text += "※部分圖案已失連" + "\n";
                            ((MainWindow)System.Windows.Application.Current.MainWindow).showLog.Foreground = System.Windows.Media.Brushes.OrangeRed;
                        }

                    }
                    /*--------------------------*/
                }
            }
            catch (Exception ex){ }
        }

    }
}

/*參考資料：
 * How to access WPF MainWindow Controls from others class：http://stackoverflow.com/questions/17001486/how-to-access-wpf-mainwindow-controls-from-my-own-cs-file
 * Change WPF mainwindow label from another class and separate thread
：http://stackoverflow.com/questions/15425495/change-wpf-mainwindow-label-from-another-class-and-separate-thread
 *  */
