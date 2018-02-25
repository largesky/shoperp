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
using ShopErp.App.Service.Restful;

namespace ShopErp.App.Views.Finance
{
    /// <summary>
    /// FinanceAccountUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class FinanceAccountUserControl : UserControl
    {
        public FinanceAccountUserControl()
        {
            InitializeComponent();
        }

        private void BtnQuery_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var ret = ServiceContainer.GetService<FinanceAccountService>().GetByAll().Datas;
                this.tbTotal.Text = "总金额：" + ret.Select(obj => obj.Money).Sum();
                this.dgvAccounts.ItemsSource = ret;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
