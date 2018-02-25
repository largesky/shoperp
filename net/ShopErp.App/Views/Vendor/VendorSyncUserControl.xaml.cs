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

        public VendorSyncUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
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
                this.sb = GetSpider();
                this.sb.Message += Sb_Message;
                this.sb.WaitingRetryMessage += Sb_WaitingRetryMessage;
                this.sb.Start += Sb_Start;
                this.sb.Stop += Sb_Stop;
                this.sb.Busy += Sb_Busy;
                this.sb.StartSyncVendor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "");
            }
        }

        private void Sb_Busy(object sender, EventArgs e)
        {

        }

        private void Sb_Stop(object sender, string e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.btnSpiderVendor.IsEnabled = true;
                this.sb = null;
                MessageBox.Show("更新完成");
            }));
        }

        private void Sb_Start(object sender, string e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.btnSpiderVendor.IsEnabled = false;
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
                this.sb.StopSyncVendor();
            }
        }

        private void AppenMessage(string message)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (this.tbMessage.LineCount > 10000)
                {
                    this.tbMessage.Clear();
                }
                this.tbMessage.AppendText(DateTime.Now + ":" + message + Environment.NewLine);
                this.tbMessage.ScrollToEnd();
            }));
        }

    }
}
