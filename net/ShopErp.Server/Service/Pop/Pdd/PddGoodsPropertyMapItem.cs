using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddGoodsPropertyMapItem
    {
        public string PddName { get; set; }

        public string OtherPopName { get; set; }

        public string DefaultValue { get; set; }

        public Dictionary<string, string> SubValuesMap { get; private set; }

        public PddGoodsPropertyMapItem()
        {
            this.SubValuesMap = new Dictionary<string, string>();
        }

        public PddGoodsPropertyMapItem Add(string otherPopValue, string pddValue)
        {
            this.SubValuesMap.Add(otherPopValue, pddValue);
            return this;
        }


    }
}
