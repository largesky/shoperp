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
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Views.Config
{
    /// <summary>
    /// ImgCleanUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ImgCleanUserControl : UserControl
    {
        private bool isStop = false;
        private Task task = null;
        private IList<ShopErp.Domain.Goods> gus = null;
        private IList<ShopErp.Domain.Vendor> vendors = null;
        private int count;

        private System.Collections.ObjectModel.ObservableCollection<ImgCleanViewModel> dirs =
            new System.Collections.ObjectModel.ObservableCollection<ImgCleanViewModel>();

        public ImgCleanUserControl()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (this.task != null)
            {
                MessageBox.Show("正在检查中");
                return;
            }
            this.dirs.Clear();
            this.dgvDirs.ItemsSource = this.dirs;
            Task.Factory.StartNew(QueryTask);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.isStop = true;
        }

        private void btnClean_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("是否删除文件夹?", "警告", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                foreach (var v in this.dirs)
                {
                    if (v.Check)
                    {
                        if (System.IO.Directory.Exists(v.Dir) == false)
                        {
                            v.State += "  文件夹不存在";
                        }
                        else
                        {
                            System.IO.Directory.Delete(v.Dir, true);
                            v.State += "  删除成功";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void QueryTask()
        {
            try
            {
                this.count = 0;
                this.isStop = false;
                this.gus = ServiceContainer.GetService<GoodsService>().GetByAll(0, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", 0, 0).Datas;
                this.vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;

                string dir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
                if (string.IsNullOrWhiteSpace(dir))
                {
                    throw new Exception("系统中没有配置图片文件夹");
                }
                List<string> dirs = new List<string>();
                string[] vendorDirs = System.IO.Directory.GetDirectories(dir + "\\goods").Where(obj => new System.IO.DirectoryInfo(obj).Name.All(c => Char.IsDigit(c))).ToArray();
                foreach (var d in vendorDirs)
                {
                    if (this.isStop)
                    {
                        return;
                    }

                    string[] numberDirs = System.IO.Directory.GetDirectories(d).Where(obj => new System.IO.DirectoryInfo(obj).Name.All(c => Char.IsDigit(c))).ToArray();
                    foreach (var md in numberDirs)
                    {
                        if (this.isStop)
                        {
                            return;
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.dirs.Add(new ImgCleanViewModel { Dir = md, State = "" });
                        }));
                    }
                }
                var dirsArray = CollectionsSpilteUtil.Spilte(this.dirs.ToArray(), 30);
                Task.WaitAll(dirsArray.Select(obj => Task.Factory.StartNew(() => QueryTask(obj))).ToArray());
                this.Dispatcher.Invoke(new Action(() =>
                {
                    var g = this.dirs.GroupBy(obj => obj.State);
                    string s = "文件夹总数：" + this.dirs.Count + ",商品总数：" + this.gus.Count + " 解析结果:" + string.Join(",", g.Select(obj => obj.Key + ":" + obj.Count()));
                    this.tbMsg.Text = s;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.isStop = true;
                this.task = null;
            }
        }

        private void QueryTask(ImgCleanViewModel[] dirs)
        {
            try
            {
                foreach (var d in dirs)
                {
                    if (this.isStop)
                    {
                        break;
                    }

                    string msg = "";
                    bool check = false;
                    int index = d.Dir.LastIndexOf('\\');
                    if (index < 0)
                    {
                        msg = "错误：路径不包含 \\ ";
                    }
                    else
                    {
                        string[] ss = d.Dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        if (ss.Length < 2)
                        {
                            msg = "错误：文件夹名称路径小于2层 ";
                        }
                        else
                        {
                            var vendor = this.vendors.FirstOrDefault(obj => obj.Id == long.Parse(ss[ss.Length - 2]));
                            if (vendor == null)
                            {
                                msg = "商品找不到对应厂家";
                                check = true;
                            }
                            else
                            {
                                var gu = this.gus.FirstOrDefault(obj => obj.VendorId == vendor.Id && obj.Number.Equals(ss[ss.Length - 1], StringComparison.OrdinalIgnoreCase));
                                msg = gu == null ? "商品已不在系统中" : "商品存在";
                                check = gu == null;
                            }
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            lock (this.dirs)
                            {
                                count++;
                                d.State = msg;
                                d.Check = check;
                                this.tbMsg.Text = string.Format("已完成解析:{0}/{1}", count, this.dirs.Count);
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}