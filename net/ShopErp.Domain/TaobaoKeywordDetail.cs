using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public class TaobaoKeywordDetail
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public string Keywords { get; set; }

        public int Total { get; set; }

        public float DayEvg { get; set; }

        public int AddCat { get; set; }

        public int Collect { get; set; }

        public int Sale { get; set; }

        public float Rela { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
