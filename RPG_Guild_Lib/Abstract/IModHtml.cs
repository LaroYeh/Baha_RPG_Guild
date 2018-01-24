using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack; //存取HTML的結構


namespace RPG_Guild_Lib
{
    //1. 讀取網頁，將要挑選的元素進行分類
    interface IReadHtml
    {

        Dictionary<NodeType, HtmlNode> DefineNode(HtmlNode Node, string XPath);
        //1.1 方
        //分類的詳細方法
        #region 分類元素
        //標誌圖
        HtmlNode MarkPic(HtmlNode node);
        //首文 - 造型
        HtmlNode MasterAvatar(HtmlNode node);
        //首文 - 內文
        HtmlNode MasterMsg(HtmlNode node);
        //留言 - 造型
        HtmlNode ReplyAvatar(HtmlNode node);
        //留言 - 內文
        HtmlNode ReplyMsg(HtmlNode node);
        #endregion
    }

    //2. 將分類過的Node，依不同特性再進行處理
    interface IModifyNode
    {
        #region 再處理區(功能待重整)
        List<HtmlNode> ModTargetReplay(List<HtmlNode> ReplyNode); //target <replay>...</replay> 取消圖片大小的限制);
        HtmlNode ModAvatarToHyperLink(HtmlNode AvatarNode); //找到每個Reply的開頭，將會隨時間改變的頭像用連結 (※之後再做出讓使用者能選擇1.存連結 2.圖檔寫死)
        HtmlNode ModViedoToHyperLink(HtmlNode ViedoNode); //因產出尺寸考量，影片轉為包含網址的預覽圖
        HtmlNode ModTargetImgToBinary(HtmlNode ImgNode); //target <img>...</img> 串中的圖檔存成二元碼
        #endregion

    }

    //3. 重組成新的網頁
    interface IRemakeHtml
    {
        #region 重新組立
        HtmlDocument HtmlFramework();
        HtmlNode NewHead(HtmlNode HeadNode); //target <head>...</head> & CSS

        // 理論上，新的寫法應該不用下面這個...，因為不需要的部分 = 建立Framework時就沒有產生
        // HtmlNode NewBody(HtmlNode BodyNode); //target <body>...</body>
        #endregion
    }
    enum DocPart { Script, Head, Body }
    enum NodeType { MarkPic, MasterAvatar, MasterMsg, ReplyAvatar, ReplyMsg }
}
