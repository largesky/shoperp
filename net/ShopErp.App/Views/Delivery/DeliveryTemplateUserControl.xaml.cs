
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
using ShopErp.Domain;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// DeliveryTemplateUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DeliveryTemplateUserControl : UserControl
    {
        public DeliveryTemplateUserControl()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new DeliveryTemplateEditWindow();
                bool? ret = window.ShowDialog();
                DeliveryTemplate template = window.DeliveryTemplate;
                if (window.NeedUpdate && template.Id > 0)
                {
                    var list = this.lstDeliveryTemplates.ItemsSource.OfType<DeliveryTemplate>().ToList();
                    list.Add(template);
                    this.lstDeliveryTemplates.ItemsSource = null;
                    this.lstDeliveryTemplates.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                DeliveryTemplate vm = btn.DataContext as DeliveryTemplate;

                if (MessageBox.Show("是否删除:" + vm.Name, "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }

                if (MessageBox.Show("是否删除:" + vm.Name, "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                ServiceContainer.GetService<DeliveryTemplateService>().Delete(vm.Id);
                var list = this.lstDeliveryTemplates.ItemsSource.OfType<DeliveryTemplate>().ToList();
                list.RemoveAll(obj => obj.Id == vm.Id);
                this.lstDeliveryTemplates.ItemsSource = null;
                this.lstDeliveryTemplates.ItemsSource = list;
                MessageBox.Show("删除成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                DeliveryTemplate vm = btn.DataContext as DeliveryTemplate;
                var window = new DeliveryTemplateEditWindow { DeliveryTemplate = vm };
                bool? ret = window.ShowDialog();
                if (window.NeedUpdate)
                {
                    var list = this.lstDeliveryTemplates.ItemsSource;
                    this.lstDeliveryTemplates.ItemsSource = null;
                    this.lstDeliveryTemplates.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var templates = ServiceContainer.GetService<DeliveryTemplateService>().GetByAll().Datas
                    .OrderBy(obj => obj.DeliveryCompany).ToList();
                this.lstDeliveryTemplates.ItemsSource = templates;
                this.cbbDeliveryCompanies.ItemsSource = templates;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string oid = this.tbOrderId.Text.Trim();
                DeliveryTemplate dt = this.cbbDeliveryCompanies.SelectedItem as DeliveryTemplate;

                if (string.IsNullOrWhiteSpace(oid) || dt == null)
                {
                    throw new Exception("订单编号或者模板为空");
                }

                var order = ServiceContainer.GetService<OrderService>().GetById(long.Parse(oid));

                if (order == null)
                {
                    throw new Exception("订单不存在");
                }
                float money = ServiceContainer.GetService<DeliveryTemplateService>().ComputeDeliveryMoney(order.DeliveryCompany,
                    order.ReceiverAddress, order.Type == OrderType.SHUA, order.PopPayType,
                    order.Weight).data;
                MessageBox.Show(money.ToString("F2"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}