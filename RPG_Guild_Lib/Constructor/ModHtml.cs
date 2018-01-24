using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace RPG_Guild_Lib
{
    class ReadHtml : IReadHtml
    {

        public Dictionary<NodeType, HtmlNode> DefineNode(HtmlNode Node, string XPath)
        {

            Dictionary<NodeType, HtmlNode> Nodes = new Dictionary<NodeType, HtmlNode>();
            HtmlNodeCollection iNode = Node.SelectNodes(XPath);

            if (Nodes.Count > 0)
            {
                foreach (var i in iNode)
                {
                    Nodes.Add(NodeType.MarkPic, i);

                }
            }

            return Nodes;

            throw new NotImplementedException();
        }

        public HtmlNode MarkPic(HtmlNode node)
        {
            throw new NotImplementedException();
        }

        public HtmlNode MasterAvatar(HtmlNode node)
        {
            throw new NotImplementedException();
        }

        public HtmlNode MasterMsg(HtmlNode node)
        {
            throw new NotImplementedException();
        }

        public HtmlNode ReplyAvatar(HtmlNode node)
        {
            throw new NotImplementedException();
        }

        public HtmlNode ReplyMsg(HtmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
