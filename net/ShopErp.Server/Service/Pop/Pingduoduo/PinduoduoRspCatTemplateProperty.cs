using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pingduoduo
{
    class PinduoduoRspCatTemplateProperty 
    {
        public int value_type;

        public bool required;

        public long id;

        public string name;

        public string name_alias;

        public int choose_max_num;


        public PinduoduoRspCatTemplatePropertyValue[] values;
    }
}
