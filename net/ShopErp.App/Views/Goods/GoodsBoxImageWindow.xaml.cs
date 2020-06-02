using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// GoodsBoxImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GoodsBoxImageWindow : Window
    {
        public ShopErp.Domain.Goods Goods { get; set; }

        public GoodsBoxImageWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Random r = new Random(DateTime.Now.Millisecond);
                //货号
                ShopErp.Domain.Vendor vendor = ServiceContainer.GetService<VendorService>().GetById(Goods.VendorId);
                if (vendor == null)
                {
                    throw new Exception("厂家获取为空，不能生成图片");
                }
                this.tbNumber.Text = vendor.Id.ToString("D4") + (Goods.Number.Length < 3 ? Goods.Number.PadRight(3, '0') : Goods.Number);
                //材质
                this.cbbParaMateria.Text = Goods.Material;

                //颜色
                string[] colors = Goods.Colors.Split(new char[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (colors.Length > 0)
                {
                    this.cbbParaColor.Text = colors[r.Next(0, colors.Length)];
                }
                else
                {
                    this.cbbParaColor.Text = "";
                }
                this.tbParaBrand.Text = LocalConfigService.GetValue(SystemNames.CONFIG_GOODS_BOX_IMAGE_BRAND, "花儿锦");
                this.checkBox.IsChecked = !(Goods.Type == GoodsType.GOODS_SHOES_FANBUXIE || Goods.Type == GoodsType.GOODS_SHOES_GAOBANGXIE || Goods.Type == GoodsType.GOODS_SHOES_TUOXIE || Goods.Type == GoodsType.GOODS_SHOES_YUNDONGXIE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void SaveJpg(string path, Grid grid)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(grid);
            JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
            jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                jpegBitmapEncoder.Save(fs);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.checkBox.Visibility = Visibility.Collapsed;
                string dir = this.Goods.ImageDir;
                string webdir = LocalConfigService.GetValue(ShopErp.Domain.SystemNames.CONFIG_WEB_IMAGE_DIR);
                var vendorPingying = ServiceContainer.GetService<VendorService>().GetVendorPingyingName(this.Goods.VendorId);

                if (string.IsNullOrWhiteSpace(webdir))
                {
                    throw new Exception("没有配置网络图片路径，请在系统中配置");
                }
                string fulldir = System.IO.Path.Combine(webdir, dir);
                if (System.IO.Directory.Exists(fulldir) == false)
                {
                    throw new Exception("文件夹路径不存在：" + fulldir);
                }

                if (string.IsNullOrWhiteSpace(this.cbbParaColor.Text.Trim()))
                {
                    throw new Exception("颜色信息为空");
                }

                if (string.IsNullOrWhiteSpace(this.cbbParaMateria.Text.Trim()))
                {
                    throw new Exception("材质信息为空");
                }

                if (string.IsNullOrWhiteSpace(this.cbbParaMeteriaButom.Text.Trim()))
                {
                    throw new Exception("鞋底信息为空");
                }

                if (string.IsNullOrWhiteSpace(this.tbParaHeight.Text.Trim()))
                {
                    throw new Exception("跟高信息为空");
                }

                if (string.IsNullOrWhiteSpace(this.tbParaHeightFront.Text.Trim()))
                {
                    throw new Exception("防水台信息为空");
                }

                if (string.IsNullOrWhiteSpace(vendorPingying))
                {
                    throw new Exception("厂家未配置拼单名称");
                }

                string ptDir = fulldir + "\\PT";
                System.IO.Directory.CreateDirectory(ptDir);
                System.IO.Directory.CreateDirectory(ptDir + "\\ZT");
                System.IO.Directory.CreateDirectory(ptDir + "\\YST");
                System.IO.Directory.CreateDirectory(ptDir + "\\XQT");
                string ptHeadersDir = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, "PTHeaders");
                if (System.IO.Directory.Exists(ptHeadersDir))
                {
                    string[] jpgs = System.IO.Directory.GetFiles(System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, "PTHeaders"));
                    foreach (var v in jpgs)
                    {
                        if (v.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || v.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                        {
                            var fi = new FileInfo(v);
                            System.IO.File.Copy(v, ptDir + "\\" + fi.Name, true);
                        }
                    }
                }
                SaveJpg(ptDir + "\\ZT\\XIEHE_" + vendorPingying + "&" + Goods.Number + ".jpg", this.dvXieHe);
                SaveJpg(ptDir + "\\11.jpg", this.dvDetail);
                System.IO.Directory.CreateDirectory(fulldir + "\\YT");
                LocalConfigService.UpdateValue(SystemNames.CONFIG_GOODS_BOX_IMAGE_BRAND, this.tbParaBrand.Text.Trim());
                MessageBox.Show("保存成功");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.checkBox.Visibility = Visibility.Visible;
            }
        }
    }
}
