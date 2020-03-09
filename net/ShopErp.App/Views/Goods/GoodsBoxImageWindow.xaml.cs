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
            Random r = new Random(DateTime.Now.Millisecond);
            //检验
            this.tbCheck.Text = "检验：" + Service.Restful.OperatorService.LoginOperator.Number.Substring(2);
            //货号
            ShopErp.Domain.Vendor vendor = ServiceContainer.GetService<VendorService>().GetById(Goods.VendorId);
            if (vendor == null)
            {
                throw new Exception("厂家获取为空，不能生成图片");
            }
            string door = VendorService.FindDoor(vendor.MarketAddress);
            if (string.IsNullOrWhiteSpace(door))
            {
                door = "001";
            }
            string number = Goods.Number.Length < 3 ? Goods.Number.PadRight(3, '0') : Goods.Number;
            this.tbNumber.Text = "货号：" + door.Substring(door.Length - 3) + number.Substring(number.Length - 3);

            //尺码
            this.tbSize.Text = "尺码：" + r.Next(34, 39).ToString();

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
        }

        private void UI_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded == false)
            {
                return;
            }

            this.tbBrand.Text = "品牌：" + this.cbbParaColor.Text.Trim();
            this.tbMeteria.Text = "材质：" + this.cbbParaMateria.Text.Trim();
            this.tbColor.Text = "颜色：" + this.cbbParaColor.Text.Trim();

            this.tbMeteriaDetail.Text = this.cbbParaMateria.Text.Trim();
            this.tbMeteriaDetailButom.Text = this.cbbParaMeteriaButom.Text.Trim();
            this.tbHeight.Text = this.tbParaHeight.Text.Trim() + "厘米";
            this.tbHeightFront.Text = this.tbParaHeightFront.Text.Trim() + "厘米";
        }

        private void SaveJpg(string path, Grid grid)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Clear();
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

                if (string.IsNullOrWhiteSpace(this.tbHeight.Text.Trim()))
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
                if (System.IO.Directory.Exists(ptDir) == false)
                {
                    System.IO.Directory.CreateDirectory(ptDir);
                    System.IO.Directory.CreateDirectory(ptDir + "\\ZT");
                    System.IO.Directory.CreateDirectory(ptDir + "\\YST");
                    string ptHeadersDir = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, "PTHeaders");
                    if (System.IO.Directory.Exists(ptHeadersDir))
                    {
                        string[] jpgs = System.IO.Directory.GetFiles(System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA, "PTHeaders"));
                        foreach (var v in jpgs)
                        {
                            if (v.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || v.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                            {
                                var fi = new FileInfo(v);
                                System.IO.File.Copy(v, ptDir + "\\" + fi.Name);
                            }
                        }
                    }
                }
                System.IO.Directory.CreateDirectory(fulldir + "\\YT");
                SaveJpg(ptDir + "\\ZT\\XIEHE_" + vendorPingying + "&" + Goods.Number + ".jpg", this.dvXieHe);
                SaveJpg(ptDir + "\\11.jpg", this.dvDetail);
                LocalConfigService.UpdateValue(SystemNames.CONFIG_GOODS_BOX_IMAGE_BRAND, this.tbParaBrand.Text.Trim());
                MessageBox.Show("保存成功");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
