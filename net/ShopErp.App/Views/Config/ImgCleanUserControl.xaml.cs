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
        private System.Collections.ObjectModel.ObservableCollection<ImgCleanViewModel> dirs = new System.Collections.ObjectModel.ObservableCollection<ImgCleanViewModel>();

        public ImgCleanUserControl()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            this.dirs.Clear();
            this.dgvDirs.ItemsSource = this.dirs;
            QueryTask();
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
                IList<ShopErp.Domain.Goods> gus = ServiceContainer.GetService<GoodsService>().GetByAll(0, GoodsState.NONE, 0, Utils.DateTimeUtil.DbMinTime, Utils.DateTimeUtil.DbMinTime, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", "", "", 0, 0).Datas.ToList();
                IList<ShopErp.Domain.Vendor> vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;
                List<string> goodsDirs = new List<string>();
                int gusCount = gus.Count;
                string imgRootDir = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
                if (string.IsNullOrWhiteSpace(imgRootDir))
                {
                    throw new Exception("系统中没有配置图片文件夹");
                }
                //厂家以数字编号保存在下面，不可能是其它的字符
                List<string> vendorDirs = System.IO.Directory.GetDirectories(imgRootDir + "\\goods").Where(obj => new System.IO.DirectoryInfo(obj).Name.All(c => Char.IsDigit(c))).ToList();
                foreach (var d in vendorDirs)
                {
                    goodsDirs.AddRange(System.IO.Directory.GetDirectories(d));
                }

                foreach (string goodsDir in goodsDirs)
                {
                    ShopErp.Domain.Goods goods = null;
                    bool check = false;
                    string msg = "";
                    string[] ss = goodsDir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (ss.Length < 2)
                    {
                        msg = "错误：文件夹名称路径小于2层 ";
                    }
                    else
                    {
                        var vendor = vendors.FirstOrDefault(obj => obj.Id == long.Parse(ss[ss.Length - 2]));
                        if (vendor == null)
                        {
                            msg = "商品找不到对应厂家";
                            check = true;
                        }
                        else
                        {
                            goods = gus.FirstOrDefault(obj => obj.VendorId == vendor.Id && obj.Number.Equals(ss[ss.Length - 1], StringComparison.OrdinalIgnoreCase));
                            msg = goods == null ? "商品已不在系统中" : "商品存在";
                            check = goods == null;
                            if (goods != null)
                            {
                                gus.Remove(goods);
                            }
                        }
                    }
                    ImgCleanViewModel d = new ImgCleanViewModel { Check = check, Dir = goodsDir, Goods = goods, State = msg };
                    dirs.Add(d);
                    this.tbMsg.Text = string.Format("已完成解析:{0}/{1}", dirs.Count, this.dirs.Count);
                }

                foreach (var goods in gus)
                {
                    ImgCleanViewModel d = new ImgCleanViewModel { Check = false, Dir = "", Goods = goods, State = "不存在文件夹" };
                    dirs.Add(d);
                    this.tbMsg.Text = string.Format("已完成解析:{0}/{1}", dirs.Count, this.dirs.Count);

                }
                var g = this.dirs.GroupBy(obj => obj.State);
                string s = "文件夹总数：" + goodsDirs.Count + ",商品总数：" + gusCount + " 解析结果:" + string.Join(",", g.Select(obj => obj.Key + ":" + obj.Count()));
                this.tbMsg.Text = s;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }
    }
}