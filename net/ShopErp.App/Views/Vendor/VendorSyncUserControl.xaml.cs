using ShopErp.App.Service.Spider;
using ShopErp.App.Service.Spider.Go2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using mshtml;

namespace ShopErp.App.Views.Vendor
{
    /// <summary>
    /// Interaction logic for GoodsSpiderUC.xaml
    /// </summary>
    public partial class VendorSyncUserControl : UserControl
    {
        private SpiderBase sb;

        private System.Collections.ObjectModel.ObservableCollection<ShopErp.Domain.Vendor> vendors = new System.Collections.ObjectModel.ObservableCollection<ShopErp.Domain.Vendor>();

        public VendorSyncUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.dgvVendors.ItemsSource = this.vendors;
        }

        private void wb1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            try
            {
                string content = "window.onerror=noError; function fixahref(){$(\"a\").each(function(index){$(this).attr(\"target\", \"_self\"); }); } function noError(){ return true; }";
                HTMLDocument doc2 = this.wb1.Document as HTMLDocument;
                IHTMLElementCollection nodes = doc2.getElementsByTagName("head");
                IHTMLScriptElement injectNode = (IHTMLScriptElement)doc2.createElement("SCRIPT");
                injectNode.type = "text/javascript";
                injectNode.text = content;

                foreach (IHTMLElement elem in nodes)
                {
                    HTMLHeadElement head = (HTMLHeadElement)elem;
                    head.appendChild((IHTMLDOMNode)injectNode);
                    head.appendChild((IHTMLDOMNode)injectNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "页面加载完成，无法注入JS代码");
            }
        }

        private SpiderBase GetSpider()
        {
            return SpiderBase.CreateSpider("go2.cn", int.Parse(this.tbWaitTime.Text.Trim()), int.Parse(this.tbPerTime.Text.Trim()));
        }

        private void btnSpiderVendor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.vendors.Clear();
                this.sb = GetSpider();
                this.sb.Message += Sb_Message;
                this.sb.WaitingRetryMessage += Sb_WaitingRetryMessage;
                this.sb.Start += Sb_Start;
                this.sb.Stop += Sb_Stop;
                this.sb.Busy += Sb_Busy;
                this.sb.VendorGeted += Sb_VendorGeted;
                this.sb.StartGetVendors();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "");
            }
        }

        private void Sb_VendorGeted(object sender, ShopErp.Domain.Vendor e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.vendors.Add(e);
            }));
        }

        private void Sb_Busy(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => this.wb1.Refresh());
        }

        private void Sb_Stop(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.btnSpiderVendor.IsEnabled = true;
                this.btnStopSpider.IsEnabled = false;
                this.sb = null;
            }));
        }

        private void Sb_Start(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.btnSpiderVendor.IsEnabled = false;
                this.btnStopSpider.IsEnabled = true;
            }));
        }

        private void Sb_WaitingRetryMessage(object sender, string e)
        {
            this.AppenMessage(e);
        }

        private void Sb_Message(object sender, string e)
        {
            this.AppenMessage(e);
        }

        private void btnStopSpider_Click(object sender, RoutedEventArgs e)
        {
            if (this.sb != null)
            {
                this.sb.StopGetVendors();
            }
        }

        private void AppenMessage(string message)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.tbMessage.Text = message;
            }));
        }

    }
}
