using System;
using ShopErp.App.Service.Spider.Go2;
using ShopErp.Domain;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Spider
{
    public abstract class SpiderBase
    {
        public event EventHandler<string> Message;

        public event EventHandler<string> WaitingRetryMessage;

        public event EventHandler<string> Start;

        public event EventHandler<string> Stop;

        public event EventHandler Busy;

        public int WaitTime { get; set; }

        public int PerTime { get; set; }

        public bool IsStop { get; set; }

        protected void OnMessage(string message)
        {
            if (this.Message != null)
            {
                this.Message(this, message);
            }
        }

        protected void OnStart()
        {
            if (this.Start != null)
            {
                this.Start(this, null);
            }
        }

        protected void OnStop()
        {
            if (this.Stop != null)
            {
                this.Stop(this, null);
            }
        }

        protected void OnWaitingRetryMessage(string state)
        {
            if (this.WaitingRetryMessage != null)
            {
                this.WaitingRetryMessage(this, state);
            }
        }

        public void OnBusy()
        {
            if (this.Busy != null)
            {
                this.Busy(this, null);
            }
        }

        public SpiderBase(int waitTime, int perTime)
        {
            this.WaitTime = waitTime;
            this.PerTime = perTime;
        }

        public abstract bool AcceptUrl(Uri uri);

        public abstract Goods GetGoodsInfoByUrl(string url, ref string vendorHomePage, ref string videoUrl, bool raiseExceptionOnGoodsNotSale);

        public abstract Vendor GetVendorInfoByUrl(string url);

        public void StartSyncVendor()
        {
            Task.Factory.StartNew(SyncTask);
        }

        private void SyncTask()
        {
            try
            {
                this.IsStop = false;
                this.OnStart();
                DoSyncVendor();
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

        public void StopSyncVendor()
        {
            this.IsStop = true;
        }

        protected abstract void DoSyncVendor();

        public static SpiderBase CreateSpider(string url, int waitTime, int perTime)
        {
            if (url.ToLower().Contains("go2.cn"))
            {
                return new Go2Spider(waitTime, perTime);
            }
            throw new Exception("未知的爬虫类型");
        }

    }
}
