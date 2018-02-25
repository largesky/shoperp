
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

namespace ShopErp.App.Views.Finance
{
    /// <summary>
    /// Interaction logic for FinanceEdit.xaml
    /// </summary>
    public partial class FinanceEdit : Window
    {
        public ShopErp.Domain.Finance Finance { get; set; }

        public FinanceEdit()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dt = this.cbbTypes.SelectedItem as FinanceType;
                if (dt == null)
                {
                    throw new Exception("请选择类型");
                }
                string type = this.cbbTypes.Text;
                DateTime time = this.dpTime.Value != null ? this.dpTime.Value.Value : DateTime.MinValue;
                float money = Math.Abs(float.Parse(this.tbMoney.Text.Trim())) * (dt.Mode == FinanceTypeMode.INPUT ? 1 : -1);
                long ac = this.cbbAccount.SelectedItem is FinanceAccount ? (this.cbbAccount.SelectedItem as FinanceAccount).Id : 0;
                long ac2 = this.cbbAccount2.SelectedItem is FinanceAccount ? (this.cbbAccount2.SelectedItem as FinanceAccount).Id : 0;
                string comment = this.tbComment.Text.Trim();
                ServiceContainer.GetService<FinanceService>().Create(type, time, money, ac, ac2, comment, this.tbOpposite.Text.Trim());
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbTypes.ItemsSource = ServiceContainer.GetService<FinanceTypeService>().GetByAll().OrderBy(obj => obj.Mode).ToArray();
            this.cbbAccount.ItemsSource = ServiceContainer.GetService<FinanceAccountService>().GetByAll().Datas;
            this.cbbAccount2.ItemsSource = ServiceContainer.GetService<FinanceAccountService>().GetByAll().Datas.Where(obj => obj.Type == FinanceAccountType.BANK);
            this.dpTime.Value = DateTime.Now.Date.AddHours(20);
        }
    }
}