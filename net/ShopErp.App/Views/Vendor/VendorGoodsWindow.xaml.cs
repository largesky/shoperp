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
using System.Windows.Shapes;
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Goods;
using ShopErp.Domain;

namespace ShopErp.App.Views.Vendor
{
    /// <summary>
    /// VendorGoodsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VendorGoodsWindow : Window
    {
        public string VendorName { get; set; }

        public VendorGoodsWindow()
        {
            InitializeComponent();
        }

        private void VendorGoodsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.lstGoods.ItemsSource = ServiceContainer.GetService<GoodsService>()
                    .GetByAll(0, GoodsState.NONE, 0, DateTime.MinValue, DateTime.MinValue, VendorName, "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", "", "", 0, 0).Datas.Select(obj => new GoodsViewModel(obj));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                this.Close();
            }
        }
    }
}
