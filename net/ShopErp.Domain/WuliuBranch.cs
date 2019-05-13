using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public class WuliuBranch
    {
        /// <summary>
        /// 公司类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 网点编号
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 网点名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 可用数量
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>
        /// 发货人姓名，暂时无法获取
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// 发货人电话，暂时无法获取
        /// </summary>
        public string SenderPhone { get; set; }

        public string SenderAddress { get; set; }

        public override string ToString()
        {
            string[] adds = SenderAddress.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string add = "";
            if (adds.Length >= 3)
            {
                for (int i = 2; i < adds.Length; i++)
                {
                    add += adds[i] + " ";
                }
            }
            if (string.IsNullOrWhiteSpace(add))
            {
                add = SenderAddress;
            }
            return string.Format("{0}-{1}-{2}", Type, Quantity, add.Trim());
        }
    }
}
