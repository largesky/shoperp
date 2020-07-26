using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopErp.App.Log;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Service.Sync
{
    public class OrderSync
    {
        public const string UPDATE_RET_NOEXIST = "本地订单中不存在";
        public const string UPDATE_RET_NOUPDATED = "订单在平台上未更新";
        public const string UPDATE_RET_NOUPDATEDCONTENT = "订单在平台上更新时间已变，但内容未变";
        public const string UPDATE_RET_UPDATED = "已更新订单";

        public event EventHandler<SyncEventArgs> Syncing;

        public event EventHandler SyncSarting;

        public event EventHandler SyncEnded;

        private static OrderUpdateService ous = ServiceContainer.GetService<OrderUpdateService>();
        private static ShopService ss = ServiceContainer.GetService<ShopService>();
        public string PopOrderId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        private List<Shop> shops = new List<Shop>();
        private bool isStop = false;
        private object running_lock = new object();
        private Task task = null;
        private int updateTotalCount = 0;
        private int updateCount = 0;

        protected void OnSync(SyncEventArgs e)
        {
            if (this.Syncing != null)
            {
                this.Syncing(this, e);
            }
        }

        protected void OnSyncStarting()
        {
            if (this.SyncSarting != null)
            {
                this.SyncSarting(this, new EventArgs());
            }
        }

        protected void OnSyncEnded()
        {
            if (this.SyncEnded != null)
            {
                this.SyncEnded(this, new EventArgs());
            }
        }

        public OrderSync(Shop[] shops, DateTime startTime, DateTime endTime, string popOrderId)
        {
            this.PopOrderId = popOrderId;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.shops.AddRange(shops);
        }

        public void StartUpdate()
        {
            if (this.task != null)
            {
                throw new Exception("已有任务运行中，不能再开启任务");
            }
            this.task = Task.Factory.StartNew(SyncTask);
        }

        public void Stop()
        {
            isStop = true;
        }

        private void SyncTask()
        {
            try
            {
                this.OnSyncStarting();
                var orders = ous.GetByAll(this.shops.Select(obj => obj.Id).ToArray(), this.PopOrderId, StartTime, EndTime, 0, 0).Datas.Where(obj => string.IsNullOrWhiteSpace(obj.PopOrderId) == false).ToList();
                this.updateTotalCount = orders.Count;
                if (orders.Count < 1)
                {
                    this.OnSync(new SyncEventArgs { Message = "没有查询到任何订单" });
                }
                else
                {
                    SyncOrders(orders.ToArray());
                }
            }
            catch (Exception ex)
            {
                Logger.Log("更新订单错误", ex);
                this.OnSync(new SyncEventArgs { Message = "发生错误:" + ex.Message });
            }
            finally
            {
                this.isStop = true;
                this.OnSyncEnded();
            }
        }

        private void SyncOrders(OrderUpdate[] orders)
        {
            foreach (var v in orders)
            {
                string error = "";
                try
                {
                    if (string.IsNullOrWhiteSpace(v.PopOrderId))
                    {
                        continue;
                    }
                    if (this.isStop)
                    {
                        break;
                    }
                    try
                    {
                        error = ous.Update(v);
                    }
                    catch (Exception ex)
                    {
                        this.OnSync(new SyncEventArgs { Message = string.Format("下载订单:{0}出错,{1}", v.PopOrderId, ex.Message + ex.StackTrace) });
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                finally
                {
                    lock (this.running_lock)
                    {
                        updateCount++;
                        this.OnSync(new SyncEventArgs { Message = string.Format("下载更新进度:{0}/{1},订单编号:{2},结果:{3}", updateCount, this.updateTotalCount, v.PopOrderId, string.IsNullOrWhiteSpace(v.PopOrderId) ? "订单不需要更新" : error) });
                    }
                }

            }
        }
    }
}
