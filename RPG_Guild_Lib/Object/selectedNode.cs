using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace RPG_Guild_Lib.Object
{
    class selectedNode
    {
        NodeType nodetype;
        string value;

    }
    class MasterNode
    {

    }
    class ReplyNode
    {
        //預設格式
        //<div id="r-96260641" name="r-96260641" class="msgreport BC2"><a href="javascript:delReplyMsg(96260641)" class="msgdel AB1">刪除</a><a href="https://home.gamer.com.tw/home.php?owner=rola1989" target="_blank"><img data-gamercard-userid="rola1989" data-src="https://avatar2.bahamut.com.tw/avataruserpic/r/o/rola1989/rola1989_s.png" class="msghead gamercard lazyloaded" data-tooltipped="" aria-describedby="tippy-tooltip-356" data-original-title="html" src="https://avatar2.bahamut.com.tw/avataruserpic/r/o/rola1989/rola1989_s.png"></a><div><a href="http://home.gamer.com.tw/home.php?owner=rola1989" class="msgname AT1">羽翼</a>：但入口與回應之間的時間很短，像是省略了『吃』的動作，只是將冰從碗中放入另一個容器<span><a href="javascript:void(0)" onclick="iWantReply(25774317,1,'rola1989','羽翼')" title="回覆他"><img src="https://i2.bahamut.com.tw/spacer.gif" class="IMG-E26"></a></span><span class="ST1">08-13 16:48</span><span class="ST1">#356</span></div></div>
        string DivId; 
        string avatar; //說話人
        string innerHTML; //回文內容
        int floor; //幾樓

        HtmlNode node;
        ReplyNode(HtmlNode node) {
            //假想取得node
            //<div id="r-96260641" name="r-96260641" class="msgreport BC2"><a href="javascript:delReplyMsg(96260641)" class="msgdel AB1">刪除</a><a href="https://home.gamer.com.tw/home.php?owner=rola1989" target="_blank"><img data-gamercard-userid="rola1989" data-src="https://avatar2.bahamut.com.tw/avataruserpic/r/o/rola1989/rola1989_s.png" class="msghead gamercard lazyloaded" data-tooltipped="" aria-describedby="tippy-tooltip-356" data-original-title="html" src="https://avatar2.bahamut.com.tw/avataruserpic/r/o/rola1989/rola1989_s.png"></a><div><a href="http://home.gamer.com.tw/home.php?owner=rola1989" class="msgname AT1">羽翼</a>：但入口與回應之間的時間很短，像是省略了『吃』的動作，只是將冰從碗中放入另一個容器<span><a href="javascript:void(0)" onclick="iWantReply(25774317,1,'rola1989','羽翼')" title="回覆他"><img src="https://i2.bahamut.com.tw/spacer.gif" class="IMG-E26"></a></span><span class="ST1">08-13 16:48</span><span class="ST1">#356</span></div></div>

            avatar = "";
            innerHTML = node.SelectSingleNode("").ToString();
        }
        /*--特徵memo*/
        /*
         串主的頭像: $('.msgname.AT1')
         
         */

    }
}
