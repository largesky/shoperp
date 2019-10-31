using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Restful
{
    class WuliuPrintTemplateService : ServiceBase<WuliuPrintTemplate>
    {
        public DataCollectionResponse<WuliuPrintTemplate> GetWuliuPrintTemplates(Shop shop, string cpCode)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["cpCode"] = cpCode;
            var datas = DoPost<DataCollectionResponse<WuliuPrintTemplate>>(para);
            return datas;
        }
    }
}
