using System;
using ShopErp.App.Service.Spider.Go2;
using ShopErp.Domain;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Spider
{
    public abstract class SpiderBase
    {
        public abstract Goods GetGoodsInfoByUrl(string url, ref string vendorHomePage, ref string videoUrl, bool raiseExceptionOnGoodsNotSale);

        public abstract Vendor GetVendorInfoByUrl(string url);

        public static SpiderBase CreateSpider(string url)
        {
            if (url.ToLower().Contains("go2.cn"))
            {
                return new Go2Spider();
            }

            if (url.ToLower().Contains("k3.cn"))
            {
                return new K3.K3Spider();
            }

            throw new Exception("未知的爬虫类型");
        }
    }
}
