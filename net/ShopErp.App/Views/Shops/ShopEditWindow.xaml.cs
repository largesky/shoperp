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
using ShopErp.Domain;


namespace ShopErp.App.Views.Shops
{
    /// <summary>
    /// ShopEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShopEditWindow : Window
    {
        public Shop Shop { get; set; }

        public ShopEditWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbPopTypes.Bind<PopType>();
            if (this.Shop == null)
            {
                this.Shop = new Shop
                {
                    AppAccessToken = "",
                    PopType = PopType.None,
                    AppKey = "",
                    AppSecret = "",
                    CommissionPer = 0,
                    CreateTime = DateTime.Now,
                    Enabled = true,
                    FirstDeliveryHours = 72,
                    Mark = "",
                    PopSellerId = "",
                    PopSellerNumberId = "",
                    SecondDeliveryHours = 0,
                    ShippingHours = 24,
                    UpdateTime = DateTime.Now,
                    Id = 0,
                    AppCallbackUrl = "",
                    AppRefreshToken = "",
                    LastUpdateOperator = "",
                    AppEnabled = false,
                    WuliuEnabled = false,
                    PopTalkId = "",
                    PopShopName = "",
                };
            }
            this.cbbPopTypes.SetSelectedEnum(this.Shop.PopType);
            this.DataContext = this.Shop;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Shop.PopType = this.cbbPopTypes.GetSelectedEnum<PopType>();
                if (this.Shop.PopType == PopType.None)
                {
                    throw new Exception("未选择平台类型");
                }

                if (string.IsNullOrWhiteSpace(this.Shop.PopSellerId))
                {
                    throw new Exception("未填写店铺账号");
                }
                this.Shop.UpdateTime = DateTime.Now;
                this.Shop.LastUpdateOperator = OperatorService.LoginOperator.Number;
                if (this.Shop.Id > 0)
                {
                    ServiceContainer.GetService<ShopService>().Update(this.Shop);
                }
                else
                {
                    this.Shop.CreateTime = DateTime.Now;
                    ServiceContainer.GetService<ShopService>().Save(this.Shop);
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