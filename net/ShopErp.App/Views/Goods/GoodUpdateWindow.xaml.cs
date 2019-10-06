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

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodUploadUpdateWindow.xaml
    /// </summary>
    public partial class GoodUpdateWindow : Window
    {
        private GoodsService shoesService = ServiceContainer.GetService<GoodsService>();

        private System.Collections.ObjectModel.ObservableCollection<GoodUpdateViewModel> shoes =
            new System.Collections.ObjectModel.ObservableCollection<GoodUpdateViewModel>();

        private int current = 0;
        private bool isStop = false;
        private bool isRunning = false;
        private SpiderBase sb = SpiderBase.CreateSpider("go2.cn");


        public GoodUpdateWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dgvShoes.ItemsSource = this.shoes;
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
                this.shoes.Clear();

                string strId = this.tbIdOrUrl.Text.Trim();
                if (string.IsNullOrWhiteSpace(strId) == false)
                {
                    int id = 0;
                    int.TryParse(strId, out id);
                    if (id > 0)
                    {
                        var d = this.shoesService.GetById(id);
                        if (d != null)
                        {
                            this.shoes.Add(new GoodUpdateViewModel { Source = d });
                        }
                    }
                    else
                    {
                        var d = this.shoesService.GetByAll(0, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, strId, "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", 0, 0).Datas;
                        foreach (var gu in d)
                        {
                            this.shoes.Add(new GoodUpdateViewModel { Source = gu });
                        }
                    }
                    return;
                }

                int pageIndex = 0;
                do
                {
                    var data = this.shoesService.GetByAll(0, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", pageIndex, 500);
                    if (data.Datas.Count < 1)
                    {
                        break;
                    }
                    foreach (var d in data.Datas)
                    {
                        this.shoes.Add(new GoodUpdateViewModel { Source = d });
                    }
                    this.tbProgress.Text = "已经下载:" + (pageIndex + 1) + "/" + (data.Total + 499) / 500 + "页，当前共:" + this.shoes.Count;
                    if (this.shoes.Count > 20)
                    {
                        this.dgvShoes.ScrollIntoView(this.shoes.Last());
                    }
                    WPFHelper.DoEvents();
                } while (++pageIndex > 0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("NO_MORE_DATA") == false)
                    MessageBox.Show(ex.Message);
            }
            finally
            {
                this.tbProgress.Text = "读取到数据:" + this.shoes.Count;
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
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "停止"));
                string vendorHomePage = "", goodsVideoUrl = "";
                foreach (var gu in shoes)
                {
                    if (this.isStop)
                    {
                        break;
                    }

                    if (gu.Source.UpdateEnabled == false)
                    {
                        continue;
                    }

                    string state = null;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(gu.Source.Url))
                        {
                            throw new Exception("商品没有URL地址");
                        }
                        var g = this.sb.GetGoodsInfoByUrl(gu.Source.Url, ref vendorHomePage, ref goodsVideoUrl, true, false);
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
                        lock (this.shoes)
                        {
                            current++;
                        }
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.tbProgress.Text = "已经更新:" + current + "/" + this.shoes.Count;
                            gu.State = state;
                        }));
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
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "更新商品"));
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.isStop = false;
                this.isRunning = true;
                this.current = 0;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "停止"));
                string dir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");

                if (string.IsNullOrWhiteSpace(dir))
                {
                    throw new Exception("没有配置图片文件夹");
                }

                foreach (var gu in shoes)
                {
                    if (this.isStop)
                    {
                        break;
                    }

                    if (gu.Source.UpdateEnabled == false)
                    {
                        continue;
                    }

                    if (gu.Source.VideoType != GoodsVideoType.VIDEO)
                    {
                        continue;
                    }

                    string state = null;
                    try
                    {
                        string fullDir = System.IO.Path.Combine(dir, gu.Source.ImageDir);
                        string[] videos = Directory.GetFiles(fullDir, "*.mp4");
                        if (videos.Length > 0)
                        {
                            FileInfo fileInfo = new FileInfo(videos[0]);
                            string newPath = System.IO.Path.Combine(fullDir, gu.Source.Number + ".mp4");
                            File.Move(videos[0], newPath);
                            state = "已处理";
                        }
                        else
                        {
                            state = "已检查，未处理";
                        }
                    }
                    catch (Exception ex)
                    {
                        state = "错误:" + ex.Message;
                    }
                    finally
                    {
                        lock (this.shoes)
                        {
                            current++;
                        }
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.tbProgress.Text = "已经更新:" + current + "/" + this.shoes.Count;
                            gu.State = state;
                        }));
                        WPFHelper.DoEvents();
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
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "更新商品"));
            }
        }
    }
}