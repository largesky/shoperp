using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopErp.App.Service.Restful;
using ShopErp.App.Service.Sync;
using ShopErp.Domain;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderSyncUserControl.xaml
    /// </summary>
    public partial class OrderSyncUserControl : UserControl
    {
        private bool myLoaded = false;
        private OrderSync orderSync = null;

        public OrderSyncUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoaded)
                {
                    return;
                }
                var ret = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled && obj.AppEnabled).ToList();
                ret.Insert(0, new Shop { Mark = "所有", Id = 0, Enabled = true });
                this.cbbShops.ItemsSource = ret;
                this.cbbShops.SelectedIndex = 0;
                this.dpStart.Value = DateTime.Now.AddDays(-30);
                this.dpEnd.Value = DateTime.Now;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            this.Start();
        }

        private void Start()
        {
            try
            {
                if (this.orderSync != null)
                {
                    this.orderSync.Stop();
                    return;
                }
                Shop[] shops = null;
                var selectedShop = this.cbbShops.SelectedItem as Shop;
                if (selectedShop == null || selectedShop.Id == 0)
                {
                    shops = this.cbbShops.ItemsSource.OfType<Shop>().Where(obj => obj.Id > 0).ToArray();
                }
                else
                {
                    shops = new Shop[] { selectedShop };
                }

                var popOrderId = this.tbPopOrderId.Text.Trim();
                if (string.IsNullOrWhiteSpace(popOrderId) == false && this.cbbShops.SelectedIndex < 1)
                {
                    throw new Exception("订单编号不为空，则必须选择一个店铺");
                }
                this.orderSync = new OrderSync(shops, this.dpStart.Value.Value, this.dpEnd.Value.Value, popOrderId);
                this.orderSync.SyncSarting += orderSync_SyncSarting;
                this.orderSync.Syncing += orderSync_Syncing;
                this.orderSync.SyncEnded += orderSync_SyncEnded;
                this.orderSync.StartUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void orderSync_SyncEnded(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("同步完成")));
            this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "开始同步"));
            orderSync.SyncSarting -= orderSync_SyncSarting;
            orderSync.SyncEnded -= orderSync_SyncEnded;
            orderSync.Syncing -= orderSync_SyncSarting;
            this.orderSync = null;
        }

        void orderSync_Syncing(object sender, SyncEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.tbMessage.LineCount > 10000)
                {
                    this.tbMessage.Text = "";
                }
                this.tbMessage.AppendText(DateTime.Now + ":" + e.Message + Environment.NewLine);
                this.tbMessage.ScrollToEnd();
            }));
        }

        void orderSync_SyncSarting(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "停止同步"));
        }
    }
}