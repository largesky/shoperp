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
using ShopErp.App.Views.Extenstions;
 
 

namespace ShopErp.App.Views.Vendor
{
    /// <summary>
    /// Interaction logic for VendorEdtion.xaml
    /// </summary>
    public partial class VendorEditWindow : Window
    {
        public ShopErp.Domain.Vendor Vendor { get; set; }

        public VendorEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Vendor == null)
            {
                this.Vendor = new ShopErp.Domain.Vendor();
                this.Title = "添加厂家";
            }
            else
            {
                this.tbName.IsEnabled = false;
                this.Title = "编辑: " + this.Vendor.Name;
            }
            this.DataContext = this.Vendor;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(this.Vendor.Name))
            {
                MessageBox.Show("厂家名称为空");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.Vendor.PingyingName))
            {
                MessageBox.Show("厂家拼音名称为空");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.Vendor.MarketAddress))
            {
                MessageBox.Show("厂家拿货地址为空");
                return;
            }

            try
            {
                VendorService vs = ServiceContainer.GetService<VendorService>();
                if (Vendor.CreateTime <= new DateTime(1900, 01, 01))
                {
                    Vendor.CreateTime = DateTime.Now;
                }
                if (this.Vendor.Id < 1)
                {
                    vs.Save(this.Vendor);
                }
                else
                {
                    vs.Update(this.Vendor);
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}