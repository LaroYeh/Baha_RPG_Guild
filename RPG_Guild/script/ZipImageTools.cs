using System.Drawing; //壓縮圖片
using System.Drawing.Imaging; //壓縮圖片
using System.IO;

namespace BAHA_MsgSave
{  
    public class ZipImageTools { //預設       

        private int _newWidth;
        private int _newHeight;
        internal int newWidth //預設的寬度上限
        {
            get { return _newWidth; }
            set { _newWidth = value; }
        }
        internal int newHeight //預設的高度上限
        {
            get { return _newHeight; }
            set { _newHeight = value; }
        }

        internal byte[] ZipImg(byte[] bImg)
        {
            //參考http://demo.tc/post/95
            MemoryStream sImg = new MemoryStream(bImg);
            Image img = Image.FromStream(sImg);
            ImageFormat thisformate = img.RawFormat;
            int fixWidth = img.Width;
            int fixHeight = img.Height;

            //計算新的長寬
            if (img.Width > newWidth || img.Height > newHeight)
            {
                if (img.Width > img.Height)
                {
                    fixWidth = newWidth;
                    fixHeight = img.Height * newWidth / img.Width;
                }
                else
                {
                    fixWidth = img.Width * newHeight / img.Height;
                    fixHeight = newHeight;
                }
            }
            else
            {
                fixWidth = img.Width;
                fixHeight = img.Height;
            }
            sImg.Dispose(); //一定要釋放掉，stream不會縮水
            sImg = new MemoryStream();
            Bitmap imgOutput = new Bitmap(img, fixWidth, fixHeight);
            imgOutput.Save(sImg, ImageFormat.Jpeg);

            //MemoryStream convert to byte[]
            byte[] bytes = new byte[sImg.Length];
            bytes = sImg.ToArray();
            return bytes;
        }
        //預計，利用while判斷尺寸是否達到要求尺寸，但是這樣就不能適合直接指定長寬... 吧，待評估
    }
}
/*多載、多型、覆寫
 * 物件導向-多載、多型：http://a032332852.pixnet.net/blog/post/164613434-(9)-c%23-%E7%89%A9%E4%BB%B6%E5%B0%8E%E5%90%91-%E5%A4%9A%E8%BC%89%E3%80%81%E5%A4%9A%E5%9E%8B
 * 建構子中覆寫：https://dotblogs.com.tw/johnny/2014/12/15/147640
 * 【C#】Class類別–子類別覆寫父類別同名的屬性與方法：http://blog.xuite.net/xiaolian/blog/84417837-%E3%80%90C%23%E3%80%91Class%E9%A1%9E%E5%88%A5%E2%80%93%E5%AD%90%E9%A1%9E%E5%88%A5%E8%A6%86%E5%AF%AB%E7%88%B6%E9%A1%9E%E5%88%A5%E5%90%8C%E5%90%8D%E7%9A%84%E5%B1%AC%E6%80%A7%E8%88%87%E6%96%B9%E6%B3%95
 * 了解使用 Override 和 New 關鍵字的時機：https://msdn.microsoft.com/zh-tw/library/ms173153.aspx
 * C# - new和override的差異與目的：https://dotblogs.com.tw/skychang/2012/05/10/72114
     */
