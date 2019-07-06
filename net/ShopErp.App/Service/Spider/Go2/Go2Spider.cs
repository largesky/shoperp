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

        private void RaiseTimeMessage(int time)
        {
            for (int j = time; j >= 0 && this.IsStop == false; j--)
            {
                this.OnWaitingRetryMessage(string.Format("等待指定时间后重试:{0}/{1}", j, time));
                System.Threading.Thread.Sleep(1000);
            }
        }

        public Go2Spider(int waitTime, int perTime) : base(waitTime, perTime)
        {
        }

        private HtmlAgilityPack.HtmlDocument GetHtmlDocWithRetry(string url, string detectNode)
        {
            for (int i = 0; i < 100 && this.IsStop == false; i++)
            {
                string html = MsHttpRestful.GetUrlEncodeBodyReturnString(url, null);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                if (html.Contains("暂未找到相关的商家") || html.Contains("该商家暂无此类商品"))
                {
                    return doc;
                }
                var nodes = string.IsNullOrWhiteSpace(detectNode) ? null : doc.DocumentNode.SelectNodes(detectNode);
                if (html.Contains("服务器受不了咯") || ((string.IsNullOrWhiteSpace(detectNode) == false) && (nodes == null || nodes.Count < 1)))
                {
                    if (WaitTime > 0)
                    {
                        Debug.WriteLine(html);
                        this.OnBusy();
                        this.RaiseTimeMessage(WaitTime * (i + 1));
                    }
                    else
                    {
                        throw new Exception("获取GO2页面失败用户请求次数过多，请等待1分钟后重试");
                    }
                }
                else
                {
                    return doc;
                }
            }

            if (this.IsStop == true)
            {
                throw new Exception("用户已操作停止");
            }

            throw new Exception("获取GO2厂家查询页面失败");
        }

        public override Goods GetGoodsInfoByUrl(string url, ref string vendorHomePage, ref string videoUrl, bool raiseExceptionOnGoodsNotSale, bool getGoodsType)
        {
            Goods g = new Goods { Comment = "", CreateTime = DateTime.Now, Image = "", LastSellTime = DateTime.Now, Number = "", Price = 0, Type = 0, UpdateEnabled = true, UpdateTime = DateTime.Now, Url = url, VendorId = 0, Weight = 0, Id = 0, Colors = "", CreateOperator = "", Flag = ColorFlag.UN_LABEL, IgnoreEdtion = false, ImageDir = "", Material = "", Shops = new List<GoodsShop>(), Star = 0, VideoType = GoodsVideoType.NONE };
            var htmlDoc = this.GetHtmlDocWithRetry(url, "");

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

            if (getGoodsType)
            {
                g.Type = GetGoodsType(g.Url, vendorHomePage);
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
                g.Colors = "默认颜色";
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
            var doc = this.GetHtmlDocWithRetry(url, "");
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
                HtmlAgilityPack.HtmlDocument doc = GetHtmlDocWithRetry(pageUrl, "//div[@class='changepage clearfix']/p");
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
                    Vendor vendor = new Vendor { PingyingName = "", CreateTime = DateTime.Now, HomePage = "",  Id = 0, MarketAddress = "", Name = "", Comment = "", Alias = "" };
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
                        if (vendors.Any(obj => obj.HomePage.Equals(vendor.HomePage)))
                        {
                            this.OnMessage("当前抓取队列中已包含该厂家，将跳过:" + vendor.Name + "  " + vendor.HomePage);
                            Debug.WriteLine("当前抓取队列中已包含该厂家，将跳过:" + vendor.Name + "  " + vendor.HomePage);
                            continue;
                        }
                        vendors.Add(vendor);
                        this.OnMessage(string.Format("已下载:{0} {1}", vendors.Count, vendor.Name));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        throw;
                    }
                }
                currentPage++;
                //this.RaiseTimeMessage(PerTime);
            }
        }

        protected override void DoSyncVendor()
        {
            string urlChars = "9ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            List<Vendor> vendors = new List<Vendor>();

            foreach (var c in urlChars)
            {
                if (this.IsStop == false)
                    this.GetVendors(c, vendors);
            }

            if (this.IsStop == true)
            {
                throw new Exception("下载已经中止");
            }

            int count = 0;
            //合并更新
            var dbVendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;
            foreach (var v in vendors)
            {
                if (this.IsStop == true)
                {
                    throw new Exception("下载已经中止");
                }

                count++;
                var al = dbVendors.FirstOrDefault(obj => obj.HomePage.TrimEnd('/').Equals(v.HomePage, StringComparison.OrdinalIgnoreCase));
                if (al != null)
                {
                    if (v.Name != al.Name)
                    {
                        al.PingyingName = "";//厂家改了名称，拼音名称无效
                    }
                    else
                    {
                        v.PingyingName = al.PingyingName;
                    }
                    if (VendorService.Match(v, al) == false)
                    {
                        al.MarketAddress = v.MarketAddress;
                        al.Name = v.Name;
                        ServiceContainer.GetService<VendorService>().Update(v);
                        this.OnMessage(string.Format("已更新 {0}  {1} ", count, v.Name));
                    }
                    else
                    {
                        this.OnMessage(string.Format("未更新 {0}  {1} ", count, v.Name));
                    }

                    dbVendors.Remove(al);
                }
                else
                {
                    Debug.WriteLine("将增加" + v.Name + " " + v.HomePage);
                    ServiceContainer.GetService<VendorService>().Save(v);
                    this.OnMessage(string.Format("已增加 {0}  {1} ", count, v.Name + " " + v.HomePage));
                }
            }
        }

        private GoodsType GetGoodsType(string goodsUrl, string vendorHomePage)
        {
            string supplrUrl = vendorHomePage + "/supply.html";
            Uri uri = new Uri(goodsUrl);
            string html = MsHttpRestful.GetUrlEncodeBodyReturnString(supplrUrl, null);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//li[@class='second-nav-item']/a");
            if (nodes.Count < 1)
            {
                throw new Exception("无法自动分析商品类型，原因：网页中没有分类连接");
            }

            foreach (var v in nodes)
            {
                string onlick = v.GetAttributeValue("onclick", "");
                string value = v.InnerText;

                string cat = onlick.Replace("changeCat('", "").Replace("')", "").Replace(";", "");
                string type = value.Substring(0, value.IndexOf("(")).Trim();

                if (type.Contains("所有"))
                {
                    continue;
                }

                string lasthtml = "";
                int i = 1;
                while (true)
                {
                    string url = string.Format("{0}/products/supply-{1}-{2}-6", vendorHomePage, cat, i++);
                    string goodsHtml = MsHttpRestful.GetUrlEncodeBodyReturnString(url, null);

                    if (string.IsNullOrWhiteSpace(goodsHtml))
                    {
                        break;
                    }

                    if (goodsHtml.Contains(uri.AbsolutePath))
                    {
                        return FormatType(type);
                    }

                    if (lasthtml != "" && lasthtml == goodsHtml)
                    {
                        break;
                    }
                    lasthtml = goodsHtml;
                }
            }
            return GoodsType.GOODS_SHOES_NONE;
        }
    }
}
