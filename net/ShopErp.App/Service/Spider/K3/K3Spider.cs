using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopErp.App.Service.Net;
using ShopErp.Domain;

namespace ShopErp.App.Service.Spider.K3
{
    class K3Spider : SpiderBase
    {
        public override Goods GetGoodsInfoByUrl(string url, ref string vendorHomePage, ref string videoUrl, bool raiseExceptionOnGoodsNotSale)
        {
            Goods g = new Goods { Comment = "", CreateTime = DateTime.Now, Image = "", Number = "", Price = 0, Type = 0, UpdateEnabled = true, UpdateTime = DateTime.Now, Url = url, VendorId = 0, Weight = 0, Id = 0, Colors = "", CreateOperator = "", Flag = ColorFlag.UN_LABEL, IgnoreEdtion = false, ImageDir = "", Material = "", Shops = new List<GoodsShop>(), Star = 0, VideoType = GoodsVideoType.NONE, Shipper = "" };
            string html = MsHttpRestful.GetUrlEncodeBodyReturnString(url, null);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            //厂家网址
            var title = doc.DocumentNode.SelectSingleNode("//div[@class='name']/a");
            if (title == null)
            {
                throw new Exception("未找到名称 //div[@class='name']/a");
            }
            vendorHomePage = title.GetAttributeValue("href", "").TrimEnd('/');

            //货号
            var hnNumber = doc.DocumentNode.SelectSingleNode("//div[@class='huohao']");
            if (hnNumber == null)
            {
                throw new Exception("未找到货号 //div[@class='huohao']");
            }
            g.Number = hnNumber.InnerText.Split(new string[] { "&amp;" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            //价格
            var hnPrice = doc.DocumentNode.SelectSingleNode("//span[@class='sku-price']");
            if (hnPrice == null)
            {
                throw new Exception("未找到价格 //span[@class='sku-price']");
            }
            g.Price = float.Parse(hnPrice.InnerText.Trim());

            //商品图片
            var hnImages = doc.DocumentNode.SelectSingleNode("//ul[@class='tb-thumb']/li/div/a/img");
            if (hnImages == null)
            {
                throw new Exception("未找到主图 //ul[@class='tb-thumb']/li/div/a/img");
            }
            g.Image = hnImages.GetAttributeValue("src", "");

            //主图视频


            //颜色
            var hnColors = doc.DocumentNode.SelectNodes("//div[@class='default-color']/div/span/a");
            if (hnColors != null & hnColors.Count > 0)
            {
                string[] colors = hnColors.Select(obj => obj.InnerText.Trim()).ToArray();
                g.Colors = string.Join(",", colors);
            }
            //帮面材质
            var hnPropertys = doc.DocumentNode.SelectNodes("//div[@class='shoes_info']/span[@class='text_box']");
            foreach (var v in hnPropertys)
            {
                if (v.InnerText.Contains("帮面材质"))
                {
                    var hnA = v.SelectSingleNode("a");
                    if (hnA != null)
                    {
                        g.Material = hnA.InnerText.Trim();
                        break;
                    }
                }
            }
            return g;
        }

        public override Vendor GetVendorInfoByUrl(string url)
        {
            var vendor = new Vendor { Alias = "", AveragePrice = 0, Comment = "", Count = 0, CreateTime = DateTime.Now, HomePage = "", Id = 0, MarketAddress = "", MarketAddressShort = "", Name = "", PingyingName = "", Watch = false };
            string html = MsHttpRestful.GetUrlEncodeBodyReturnString(url, null);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var title = doc.DocumentNode.SelectSingleNode("//div[@class='name']/a");
            if (title == null)
            {
                throw new Exception("未找到名称 //div[@class='name']/a");
            }
            vendor.Name = title.InnerText.Trim();
            vendor.HomePage = title.GetAttributeValue("href", "").TrimEnd('/');

            var adds = doc.DocumentNode.SelectNodes("//div[@class = 'site_right']/div");
            if (adds == null || adds.Count < 1)
            {
                throw new Exception("未找到地址 div[@class = 'site_right']/div ");
            }
            foreach (var addN in adds)
            {
                var add = addN.InnerText.Trim().Replace(":", "").Replace("：", "").Replace(";", "").Replace("；", "").Replace("&nbsp", "");
                if (add.StartsWith("地址"))
                {
                    add = add.Substring(2);
                    vendor.MarketAddress = add.Trim();
                    break;
                }
            }

            return vendor;
        }
    }
}
