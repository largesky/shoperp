
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

namespace ShopErp.App.Views.Finance
{
    /// <summary>
    /// Interaction logic for TypeConfigWindow.xaml
    /// </summary>
    public partial class FinanceTypeConfigWindow : Window
    {
        private System.Collections.ObjectModel.ObservableCollection<FinanceType> types =
            new System.Collections.ObjectModel.ObservableCollection<FinanceType>();

        public FinanceTypeConfigWindow()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = this.tbName.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("名称不能为空");
                    return;
                }

                if (this.types.Any(obj => obj.Name == name))
                {
                    MessageBox.Show("已经存在");
                    return;
                }
                ServiceContainer.GetService<FinanceTypeService>().Save(new FinanceType { Name = this.cbbTypes.Text + "-" + name, Mode = this.cbbTypes.GetSelectedEnum<FinanceTypeMode>() });
                this.Window_Loaded(null, null);
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
                if (this.dgvFinaceTypes.SelectedCells.Count < 1)
                {
                    MessageBox.Show("请选择要删除的类型");
                    return;
                }

                var ft = this.dgvFinaceTypes.SelectedCells[0].Item as FinanceType;
                if (MessageBox.Show("是否删除:" + ft.Name, "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                ServiceContainer.GetService<FinanceTypeService>().Delete(ft.Id);
                this.Window_Loaded(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbTypes.Bind<FinanceTypeMode>();
            this.dgvFinaceTypes.ItemsSource = ServiceContainer.GetService<FinanceTypeService>().GetByAll().OrderBy(obj => obj.Mode).ToArray();
        }
    }
}