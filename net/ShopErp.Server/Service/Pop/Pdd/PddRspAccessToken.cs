using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    public class PddRspAccessToken : PddRspBase
    {
        public string access_token;

        public string refresh_token;

        public string owner_id;

        public string owner_name;
    }
}
