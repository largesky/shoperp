using System;
using ShopErp.App.Service.Spider.Go2;
using ShopErp.Domain;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Spider
{
    public abstract class SpiderBase
    {
        public event EventHandler<string> Message;

        public event EventHandler<Vendor> VendorGeted;

        public event EventHandler Start;

        public event EventHandler Stop;

        public bool IsStop { get; set; }

        protected virtual void OnMessage(string message)
        {
            if (this.Message != null)
            {
                this.Message(this, message);
            }
        }

        protected virtual void OnStart()
        {
            if (this.Start != null)
            {
                this.Start(this, null);
            }
        }

        protected virtual void OnStop()
        {
            if (this.Stop != null)
            {
                this.Stop(this, null);
            }
        }

        protected virtual void OnVendorGeted(Vendor vendor)
        {
            if (this.VendorGeted != null)
            {
                this.VendorGeted(this, vendor);
            }
        }

        public abstract bool AcceptUrl(Uri uri);

        public abstract Goods GetGoodsInfoByUrl(string url, ref string vendorHomePage, ref string videoUrl, bool raiseExceptionOnGoodsNotSale, bool getGoodsType);

        public abstract Vendor GetVendorInfoByUrl(string url);

        protected abstract void GetVendors();

        public void StartGetVendors()
        {
            Task.Factory.StartNew(GetVendorsTask);
        }

        public void StopGetVendors()
        {
            this.IsStop = true;
        }

        private void GetVendorsTask()
        {
            try
            {
                this.IsStop = false;
                this.OnStart();
                GetVendors();
                this.OnMessage("已下载完成");
            }
            catch (Exception exception)
            {
                this.OnMessage(exception.Message);
            }
            finally
            {
                this.IsStop = true;
                this.OnStop();
            }
        }

        public static SpiderBase CreateSpider(string url)
        {
            if (url.ToLower().Contains("go2.cn"))
            {
                return new Go2Spider();
            }
            throw new Exception("未知的爬虫类型");
        }
    }
}
