using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pingduoduo
{
    public class PingduoduoRspAccessToken : PingduoduoRspBase
    {
        public string access_token;

        public string refresh_token;

        public string owner_id;

        public string owner_name;
    }
}
