using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using mshtml;
using ShopErp.App.Service.Spider;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Service;
using System.IO;
using System.Windows.Media.TextFormatting;
using System.Threading;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodUploadUpdateWindow.xaml
    /// </summary>
    public partial class GoodUpdateUserControl : UserControl
    {
        private GoodsService shoesService = ServiceContainer.GetService<GoodsService>();
        private GoodUpdateViewModel[] goods = null;

        private int current = 0;
        private bool isStop = false;
        private bool isRunning = false;
        private int waitSeconds = 0;
        private SpiderBase sb = SpiderBase.CreateSpider("go2.cn");


        public GoodUpdateUserControl()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, EventArgs e)
        {
            try
            {
                TextBlock tb = sender as TextBlock;
                if (tb == null)
                {
                    throw new Exception("Edit_Click错误，事件源不是TextBlock");
                }
                GoodUpdateViewModel shoes = tb.DataContext as GoodUpdateViewModel;
                Process.Start(shoes.Source.Url.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string vendorAdd = this.tbVendorAddress.Text.Trim();
                this.waitSeconds = int.Parse(this.tbWaitSeconds.Text.Trim());
                var shoes = this.shoesService.GetByAll(0, GoodsState.NONE, 0, Utils.DateTimeUtil.DbMinTime, this.dpEnd.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpEnd.Value.Value, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "Id ASC", vendorAdd, "", 0, 0).Datas.Where(obj => obj.UpdateEnabled && string.IsNullOrWhiteSpace(obj.Url) == false).ToArray();
                if (this.chkFullUpate.IsChecked.Value == false)
                {
                    DateTime max = shoes.Select(obj => obj.UpdateTime).Max();
                    long goodsId = shoes.First(obj => obj.UpdateTime == max).Id;
                    shoes = shoes.Where(obj => obj.Id >= goodsId).ToArray();
                }
                this.goods = shoes.Select(obj => new GoodUpdateViewModel() { Source = obj }).ToArray();
                this.dgvShoes.ItemsSource = this.goods;
                this.tbProgress.Text = "读取到数据:" + this.goods.Length;
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("NO_MORE_DATA") == false)
                    MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (this.isRunning)
            {
                this.isStop = true;
            }
            else
            {
                Task.Factory.StartNew(UpdateTask);
            }
        }

        private void UpdateTask()
        {
            try
            {
                this.isStop = false;
                this.isRunning = true;
                this.current = 0;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "停止更新"));
                string vendorHomePage = "", goodsVideoUrl = "";
                foreach (var gu in goods)
                {
                    if (this.isStop)
                    {
                        break;
                    }
                    string state = null;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(gu.Source.Url))
                        {
                            throw new Exception("商品没有URL地址");
                        }
                        var g = this.sb.GetGoodsInfoByUrl(gu.Source.Url, ref vendorHomePage, ref goodsVideoUrl, true);
                        if (g == null)
                        {
                            throw new Exception("获取商品方法返回NULL");
                        }

                        gu.Source.Colors = g.Colors;
                        gu.Source.UpdateTime = DateTime.Now;
                        gu.Source.Material = g.Material;
                        if (gu.Source.VideoType != g.VideoType)
                        {
                            gu.Source.VideoType = g.VideoType;
                            GoodsService.SaveVideo(gu.Source, goodsVideoUrl);
                        }
                        if (gu.Source.Price > g.Price)
                            gu.Source.Price = g.Price;
                        this.shoesService.Update(gu.Source);
                        state = "更新成功";
                    }
                    catch (Exception ex)
                    {
                        state = "错误:" + ex.Message;
                    }
                    finally
                    {
                        current++;
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.tbProgress.Text = "已经更新:" + current + "/" + this.goods.Length + " 等待下次更新...";
                            gu.State = state;
                        }));
                        Thread.Sleep(this.waitSeconds * 1000);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.isRunning = false;
                this.isStop = true;
                this.Dispatcher.BeginInvoke(new Action(() => { this.btnUpdate.Content = "更新商品"; MessageBox.Show("更新完成"); }));
            }
        }
    }
}