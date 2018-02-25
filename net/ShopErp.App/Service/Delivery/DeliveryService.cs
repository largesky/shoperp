using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Delivery
{
    public class DeliveryService
    {
        const string APIURL = "http://www.kuaidi100.com/query";
        const string APPCODE = "e652b80bcaf24ff5";
        private static Random r = new Random(456456);

        /// <summary>
        /// 查询物流记录
        /// </summary>
        /// <param name="company"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static DeliveryTransation Query(string company, string number)
        {
            String code = ServiceContainer.GetService<DeliveryCompanyService>().GetDeliveryCompany(company).PopMapKuaidi100;
            Dictionary<string, string> para = new Dictionary<string, string>();
            para["type"] = code;
            para["postid"] = number;
            para["temp"] = "0." + r.Next() + "" + r.Next();
            var content = MsHttpRestful.GetUrlEncodeBodyReturnString(APIURL, para, Encoding.UTF8, null, "http://www.kuaidi100.com/", "*/*");
            var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<Kuaidi100DeliveryResult>(content);
            var item = new DeliveryTransation();
            item.IsSigned = ret.ischeck == "1" ? true : false;
            if (ret.message.ToUpper().Equals("OK") == false)
            {
                throw new Exception(ret.message);
            }
            item.Items = new List<DeliveryTransationItem>();
            foreach (var o in ret.data)
            {
                item.Items.Add(new DeliveryTransationItem { Time = DateTime.Parse(o.ftime), Description = o.context });
            }

            return item;
        }
    }
}