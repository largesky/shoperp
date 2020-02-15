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
using ShopErp.Domain;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// DeliveryTemplateEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DeliveryTemplateEditWindow : Window
    {
        public bool NeedUpdate { get; set; }

        public DeliveryTemplate DeliveryTemplate { get; set; }

        private System.Collections.ObjectModel.ObservableCollection<DeliveryTemplateArea> templateAreas =
            new System.Collections.ObjectModel.ObservableCollection<DeliveryTemplateArea>();


        public DeliveryTemplateEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.DeliveryTemplate == null)
                {
                    this.Title = "新增运费模板";
                    this.DeliveryTemplate = new DeliveryTemplate() { Areas = new List<DeliveryTemplateArea>(), UpdateOperator = "", UpdateTime = DateTime.Now, CreateTime = DateTime.Now };
                }
                else
                {
                    this.Title = "编辑运费模板";
                }
                this.DataContext = this.DeliveryTemplate;
                this.cbbDeliveryCompanies.DataContext = this.DeliveryTemplate;
                this.cbbDeliveryCompanies.ItemsSource = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name).ToList();
                if (string.IsNullOrWhiteSpace(this.DeliveryTemplate.DeliveryCompany))
                {
                    this.cbbDeliveryCompanies.SelectedIndex = 0;
                }
                else
                {
                    this.cbbDeliveryCompanies.SelectedItem = this.DeliveryTemplate.DeliveryCompany;
                }
                foreach (var item in this.DeliveryTemplate.Areas)
                {
                    this.templateAreas.Add(item);
                }
                this.dgvAreas.ItemsSource = this.templateAreas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.DeliveryTemplate.Name) ||
                string.IsNullOrWhiteSpace(this.DeliveryTemplate.DeliveryCompany))
            {
                MessageBox.Show("请输入运费模板名称与物流公司");
                return;
            }

            if (this.DeliveryTemplate.HotPaperUse == false && this.DeliveryTemplate.NormalPaperUse == false)
            {
                MessageBox.Show("电子面单或者普通面单必须选择一种或者两种");
                return;
            }

            if (this.DeliveryTemplate.OnlinePayTypeUse == false && this.DeliveryTemplate.CodPayTypeUse == false)
            {
                MessageBox.Show("在线支付或者货到付款必须选择一种或者两种");
                return;
            }

            if (this.templateAreas.Count(obj => string.IsNullOrWhiteSpace(obj.Areas)) > 1)
            {
                MessageBox.Show("默认为空的运费条目只能有一条");
                return;
            }

            try
            {
                this.DeliveryTemplate.Areas = this.templateAreas;
                if (this.DeliveryTemplate.Id > 0)
                {
                    ServiceContainer.GetService<DeliveryTemplateService>().Update(this.DeliveryTemplate);
                }
                else
                {
                    this.DeliveryTemplate.Id = ServiceContainer.GetService<DeliveryTemplateService>().Save(this.DeliveryTemplate);
                }

                this.NeedUpdate = true;
                MessageBox.Show("已成功保存");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddArea_Click(object sender, RoutedEventArgs e)
        {
            this.templateAreas.Add(new DeliveryTemplateArea { Areas = "" });
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            if (tb == null)
            {
                throw new Exception("Edit_Click错误，事件源不是TextBlock");
            }
            DataGridCell dd = tb.Parent as DataGridCell;
            DeliveryTemplateArea vm = dd.DataContext as DeliveryTemplateArea;
            var window = new DeliveryTemplateProvincesSeletorWindow { Province = vm.Areas };
            bool? ret = window.ShowDialog();
            if (ret != null && ret.Value)
            {
                vm.Areas = window.Province;
                this.dgvAreas.ItemsSource = null;
                this.dgvAreas.ItemsSource = this.templateAreas;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            if (tb == null)
            {
                throw new Exception("Edit_Click错误，事件源不是TextBlock");
            }
            DataGridCell dd = tb.Parent as DataGridCell;
            DeliveryTemplateArea vm = dd.DataContext as DeliveryTemplateArea;

            if (MessageBox.Show("删除:" + vm.Areas, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                MessageBoxResult.Yes)
            {
                return;
            }

            this.templateAreas.Remove(vm);
        }
    }
}