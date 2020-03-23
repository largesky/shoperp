using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;

namespace ShopErp.App.Service.Spider.Go2
{
    public class Go2Spider : SpiderBase
    {
        private readonly Dictionary<string, string> empty_value_dic = new Dictionary<string, string>();

        private HtmlAgilityPack.HtmlDocument GetHtmlDocWithRetry(string url)
        {
            string html = MsHttpRestful.GetUrlEncodeBodyReturnString(url, null);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        public override Goods GetGoodsInfoByUrl(string url, ref string vendorHomePage, ref string videoUrl, bool raiseExceptionOnGoodsNotSale)
        {
            Goods g = new Goods { Comment = "", CreateTime = DateTime.Now, Image = "", Number = "", Price = 0, Type = 0, UpdateEnabled = true, UpdateTime = DateTime.Now, Url = url, VendorId = 0, Weight = 0, Id = 0, Colors = "", CreateOperator = "", Flag = ColorFlag.UN_LABEL, IgnoreEdtion = false, ImageDir = "", Material = "", Shops = new List<GoodsShop>(), Star = 0, VideoType = GoodsVideoType.NONE, Shipper = "" };
            var htmlDoc = this.GetHtmlDocWithRetry(url);

            //商品已删除 
            if (htmlDoc.DocumentNode.InnerHtml.Contains("该商品不存在或已删除"))
            {
                throw new Exception("产品已从GO2删除");
            }

            //检查是否下架
            if (raiseExceptionOnGoodsNotSale)
            {
                var stateNode = htmlDoc.DocumentNode.SelectNodes("//p[@class='xiajia']");
                if (stateNode != null && stateNode.Count > 0 && stateNode.LastOrDefault().InnerText.Trim() == "本产品已下架，如有特殊需求请直接联系商家")
                {
                    throw new Exception("产品已从GO2下架");
                }

                stateNode = htmlDoc.DocumentNode.SelectNodes("//i[@class='icon icon-sm s_weihuo']");
                if (stateNode != null && stateNode.Count > 0)
                {
                    throw new Exception("产品属于店铺尾货");
                }
            }

            //获取厂家名称
            var vendorUrlNode = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='topbar-sup color-orange']");
            if (vendorUrlNode == null)
            {
                throw new Exception("解析厂家名称HTML失败");
            }
            vendorHomePage = vendorUrlNode.GetAttributeValue("href", "").TrimEnd('/');
            if (string.IsNullOrWhiteSpace(vendorHomePage))
            {
                throw new Exception("获取到的厂家主页为空");
            }

            //获取货号
            var numberNode = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='top-number color-orange']");
            if (numberNode == null)
            {
                throw new Exception("解析厂家货号HTML结点失败");
            }
            g.Number = numberNode.InnerText.Trim();
            if (g.Number.Contains("&"))
            {
                g.Number = g.Number.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            if (string.IsNullOrWhiteSpace(g.Number))
            {
                throw new Exception("获取到的货号为空");
            }

            //价格，在GO2网页中，价格结点中的TITLE属性是真正的价格，内部值在源网页中是假的，网页加载完成后有JS改正
            var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='supplie-price-top top-price color-orange']");
            if (priceNode == null)
            {
                priceNode = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='top-price color-orange sup-price-top-hx']");
                if (priceNode == null)
                {
                    throw new Exception("未找到价格结点：//span[@class='supplie-price-top top-price color-orange 或者" + Environment.NewLine + "//span[@class='top-price color-orange sup-price-top-hx']");
                }
            }
            if (priceNode.Attributes["title"] == null)
            {
                throw new Exception("价格结点没有title属性");
            }
            string price = priceNode.Attributes["title"].Value.Replace("&yen;", "");
            g.Price = float.Parse(price);

            //解析商品图片
            var imageNode = htmlDoc.DocumentNode.SelectNodes("//div[@class='big-img-box']/img");
            if (imageNode == null || imageNode.Count < 1)
            {
                throw new Exception("解析商品图片HTML结点失败");
            }
            g.Image = imageNode.FirstOrDefault().GetAttributeValue("src", "").Trim();
            if (string.IsNullOrWhiteSpace(g.Image))
            {
                throw new Exception("商品没有主图");
            }

            //主图视频
            var videoNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='video-flv']");
            if (videoNode == null)
            {
                g.VideoType = GoodsVideoType.NOT;
            }
            else
            {
                videoUrl = videoNode.GetAttributeValue("data-url", "");
                if (videoUrl.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    g.VideoType = GoodsVideoType.VIDEO;
                }
                else
                {
                    g.VideoType = GoodsVideoType.NOT;
                    videoUrl = "";
                }
            }

            //颜色
            var colorNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='details-item']/div/ul/li[@class='details-attribute-item props-color']");
            if (colorNode != null)
            {
                string color = colorNode.GetAttributeValue("title", "").Trim();
                var colors = color.Split(new char[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var nc = colors.Where(obj => obj.Any(c => Char.IsDigit(c)) == false || obj.Contains("色")).ToArray();
                g.Colors = string.Join(",", nc);
            }
            else
            {
                g.Colors = "";
            }

            //帮面材质
            var detailNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='details-item']/div/ul/li[@class='details-attribute-item']");
            if (detailNodes != null && detailNodes.Count > 0)
            {
                foreach (var dN in detailNodes)
                {
                    if (dN.InnerText.Contains("帮面材质"))
                    {
                        g.Material = dN.GetAttributeValue("title", "").Trim();
                        break;
                    }
                }
            }
            g.Material = g.Material ?? "";
            return g;
        }

        public override Vendor GetVendorInfoByUrl(string url)
        {
            Vendor ven = new Vendor { AveragePrice = 0, Count = 1, CreateTime = DateTime.Now, HomePage = "", Id = 0, MarketAddress = "", Name = "", PingyingName = "", Watch = false, Comment = "", Alias = "", MarketAddressShort = "" };
            var doc = this.GetHtmlDocWithRetry(url);
            //获取厂家名称url地址
            var vendorUrlNode = doc.DocumentNode.SelectSingleNode("//a[contains(@class,'merchant-title')]");
            if (vendorUrlNode == null)
            {
                throw new Exception("解析厂家名称HTML失败");
            }

            string vendorName = vendorUrlNode.InnerText.Trim();
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                throw new Exception("获取到的厂家名称为空");
            }
            string u = vendorUrlNode.Attributes["href"].Value.Trim();
            if (string.IsNullOrWhiteSpace(u))
            {
                throw new Exception("获取到的厂家网址为空");
            }

            var addNode = doc.DocumentNode.SelectSingleNode("//p[@class='merchant-address']");
            if (addNode == null)
            {
                throw new Exception("厂家拿货地址结点为空");
            }
            string add = addNode.InnerText.Trim();
            ven.MarketAddress = add.StartsWith("成都") ? add : "成都" + add;
            ven.Name = vendorName;
            ven.HomePage = u.TrimEnd('/');

            return ven;
        }
    }
}
