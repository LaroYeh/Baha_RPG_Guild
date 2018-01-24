using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG_Guild_Lib.Lib
{
    interface ISetting
    {
        /// <summary>
        /// 全部設定恢復預設值
        /// </summary>
        void returnDefault();
        /// <summary>
        /// 哪些設定要恢復預設值? 可單一或多選
        /// </summary>
        /// <param name="Name"></param>
        void returnDefalut(List<string> Name);
    }
}
