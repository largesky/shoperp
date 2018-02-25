using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public class FinanceAccount
    {
        public long Id { get; set; }

        public FinanceAccountType Type { get; set; }

        public string Name { get; set; }

        public string Number { get; set; }

        public float Money { set; get; }

        public DateTime CreateTime { get; set; }

        public int Order { get; set; }

        public string ShortInfo { get; set; }
    }
}
