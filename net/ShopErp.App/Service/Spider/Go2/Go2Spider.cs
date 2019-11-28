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

        private static string[] FormatColors(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new string[0];
            }
            string[] colors = str.Split(",，，.。。\\、＼:：： ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            return colors;
        }

        private static string[] FormatSizes(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new string[0];
            }

            string[] sizes = str.Split(",，，:：：.。。\\、＼ ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            return sizes;
        }

        private static GoodsType FormatType(string type)
        {
            if (type == "凉鞋")
            {
                return GoodsType.GOODS_SHOES_LIANGXIE;
            }
            if (type == "低帮鞋")
            {
                return GoodsType.GOODS_SHOES_DIBANGXIE;
            }
            if (type == "高帮鞋")
            {
                return GoodsType.GOODS_SHOES_GAOBANGXIE;
            }
            if (type == "拖鞋")
            {
                return GoodsType.GOODS_SHOES_TUOXIE;
            }
            if (type == "靴子")
            {
                return GoodsType.GOODS_SHOES_XUEZI;
            }
            if (type == "男鞋")
            {
                return GoodsType.GOODS_SHOES_NANXIE;
            }
            if (type == "帆布鞋")
            {
                return GoodsType.GOODS_SHOES_FANBUXIE;
            }
            if (type == "雨鞋")
            {
                return GoodsType.GOODS_SHOES_YUXIE;
            }

            return GoodsType.GOODS_SHOES_OTHER;
        }

        public override bool AcceptUrl(Uri uri)
        {
            return uri.Host.ToLower().Equals("www.go2.cn") || uri.Host.ToLower().Equals("z.go2.cn");
        }

        private HtmlAgilityPack.HtmlDocument GetHtmlDocWithRetry(string url)
        {
            string html = MsHttpRestful.GetUrlEncodeBodyReturnString(url, null);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        public override Goods GetGoodsInfoByUrl(string url, ref string vendorHomePage, ref string videoUrl, bool raiseExceptionOnGoodsNotSale, bool getGoodsType)
        {
            Goods g = new Goods { Comment = "", CreateTime = DateTime.Now, Image = "", LastSellTime = DateTime.Now, Number = "", Price = 0, Type = 0, UpdateEnabled = true, UpdateTime = DateTime.Now, Url = url, VendorId = 0, Weight = 0, Id = 0, Colors = "", CreateOperator = "", Flag = ColorFlag.UN_LABEL, IgnoreEdtion = false, ImageDir = "", Material = "", Shops = new List<GoodsShop>(), Star = 0, VideoType = GoodsVideoType.NONE};
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
            Vendor ven = new Vendor { AveragePrice = 0, Count = 1, CreateTime = DateTime.Now, HomePage = "", Id = 0, MarketAddress = "", Name = "", PingyingName = "", Watch = false, Comment = "", Alias = "" };
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
            ven.MarketAddress = add;
            ven.Name = vendorName;
            ven.HomePage = u;
            ven.HomePage = ven.HomePage.TrimEnd('/');
            return ven;
        }

        protected void GetVendors(char c, List<Vendor> vendors)
        {
            int totalPage = 10;
            int currentPage = 1;
            Dictionary<string, string> pa = new Dictionary<string, string>();
            while (currentPage <= totalPage && this.IsStop == false)
            {
                //从网络读取html
                string pageUrl = string.Format("http://www.go2.cn/supplier/yshy-0-0-0-0-{0}-0-all/page{1}.html", c, currentPage);
                HtmlAgilityPack.HtmlDocument doc = GetHtmlDocWithRetry(pageUrl);
                if (doc.DocumentNode.InnerText.Contains("暂未找到相关的商家"))
                {
                    return;
                }
                //获取总页数
                var changePageNode = doc.DocumentNode.SelectNodes("//div[@class='changepage clearfix']/p").Last();
                string txt = changePageNode.InnerText;
                string pageTxt = string.Join("", txt.ToArray().Where(cc => Char.IsDigit(cc)));
                totalPage = int.Parse(pageTxt);

                //解析厂家
                var vendorNodes = doc.DocumentNode.SelectNodes("//div[@class='certified-list-info pull-left']");
                foreach (var xe in vendorNodes)
                {
                    if (this.IsStop)
                    {
                        break;
                    }
                    Vendor vendor = new Vendor { PingyingName = "", CreateTime = DateTime.Now, HomePage = "", Id = 0, MarketAddress = "", Name = "", Comment = "", Alias = "" };
                    try
                    {
                        //名称Node
                        var nameAndHomePageNode = xe.SelectSingleNode("p[@class='clearfix']/a");
                        //拿货地址
                        var addN = xe.SelectNodes("p/span[@class='title']");

                        if (nameAndHomePageNode == null)
                        {
                            throw new Exception("HTML中未找到厂家名称连接结点");
                        }

                        if (addN == null)
                        {
                            throw new Exception("HTML中未找到地址结点");
                        }

                        //名称
                        vendor.Name = nameAndHomePageNode.InnerText.Trim();
                        //主页
                        vendor.HomePage = nameAndHomePageNode.GetAttributeValue("href", "").Trim().TrimEnd('/');

                        string add = addN.Last().InnerText.Trim();
                        if (add.Contains("拿货地址") == false)
                        {
                            throw new Exception("数据中不包含拿货地址:" + add);
                        }
                        //拿货地址
                        vendor.MarketAddress = add.Replace(" ", "").Replace("拿货地址", "").Replace(":", "").Replace("：", "").Trim();
                        //Debug.WriteLine(DateTime.Now + ":" + vendor.Name + "  " + vendor.HomePage + "  " + pageUrl);
                        if (vendors.Any(obj => obj.HomePage.Equals(vendor.HomePage)))
                        {
                            this.OnMessage("当前抓取队列中已包含该厂家，将跳过:" + vendor.Name + "  " + vendor.HomePage);
                            Debug.WriteLine("当前抓取队列中已包含该厂家，将跳过:" + vendor.Name + "  " + vendor.HomePage);
                        }
                        else
                        {
                            vendors.Add(vendor);
                            this.OnVendorGeted(vendor);
                            this.OnMessage(string.Format("已下载:{0} {1}", vendors.Count, vendor.Name));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        throw;
                    }
                }
                currentPage++;
            }
        }

        protected override void GetVendors()
        {
            string urlChars = "9ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            List<Vendor> vendors = new List<Vendor>();

            foreach (var c in urlChars)
            {
                if (this.IsStop == false)
                    this.GetVendors(c, vendors);
            }
        }
    }
}
