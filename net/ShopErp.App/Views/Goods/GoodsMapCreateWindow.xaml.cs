
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Goods
{
    /// <summary>
    /// Interaction logic for GoodsAddMapWindow.xaml
    /// </summary>
    public partial class GoodsMapCreateWindow : Window
    {
        public long GoodsId { get; set; }

        public GoodsMapCreateWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var gu = ServiceContainer.GetService<GoodsService>().GetById(this.GoodsId);
                if (gu == null)
                {
                    throw new Exception("商品不存在");
                }
                var vendor = ServiceContainer.GetService<VendorService>().GetById(gu.VendorId);
                if (vendor == null)
                {
                    throw new Exception("厂家不存在");
                }

                this.tbVendorName.Text = vendor.Name;
                this.tbPrice.Text = gu.Price.ToString("F2");
                this.chkDeleteEdtion.IsChecked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string number = this.tbNumber.Text.Trim();
                string vendorName = this.tbVendorName.Text.Trim();
                float price = float.Parse(this.tbPrice.Text.Trim());

                if (string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(vendorName) || price < 0)
                {
                    throw new Exception("厂家名称，货号，价格不合法");
                }

                //检查是否存在
                var al = ServiceContainer.GetService<GoodsMapService>().GetByAll(vendorName, number, this.GoodsId, 0, 0).Datas;
                if (al != null && al.Count > 0)
                {
                    throw new Exception("相同的映射已经存在");
                }

                var vendor = ServiceContainer.GetService<VendorService>().GetByAll(vendorName, "", "", "", 0, 0).Datas;
                if (vendor == null || vendor.Count < 1)
                {
                    throw new Exception("厂家不存在");
                }

                if (vendor.Count > 1)
                {
                    throw new Exception("找到多个厂家");
                }

                var gu = ServiceContainer.GetService<GoodsService>().GetById(this.GoodsId);
                if (gu == null)
                {
                    throw new Exception("现在的商品不存在");
                }

                GoodsMap gm = new GoodsMap
                {
                    IgnoreEdtion = this.chkDeleteEdtion.IsChecked.Value,
                    Number = number,
                    TargetGoodsId = this.GoodsId,
                    Price = price,
                    VendorId = vendor[0].Id,
                    Id = 0,
                };

                ServiceContainer.GetService<GoodsMapService>().Save(gm);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}