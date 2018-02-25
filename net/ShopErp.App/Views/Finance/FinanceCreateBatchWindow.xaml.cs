using ShopErp.App.Service.Restful;
using ShopErp.Domain;
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

namespace ShopErp.App.Views.Finance
{
    /// <summary>
    /// FinanceCreateBatchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FinanceCreateBatchWindow : Window
    {
        private System.Collections.ObjectModel.ObservableCollection<ShopErp.Domain.Finance> finances = new System.Collections.ObjectModel.ObservableCollection<ShopErp.Domain.Finance>();

        public FinanceType[] Types { get; set; }

        public FinanceAccount[] Accounts { get; set; }

        public FinanceCreateBatchWindow()
        {
            InitializeComponent();
            this.Types = ServiceContainer.GetService<FinanceTypeService>().GetByAll().ToArray().OrderBy(obj => obj.Mode).ToArray();
            this.Accounts = ServiceContainer.GetService<FinanceAccountService>().GetByAll().Datas.ToArray();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.finances.Add(new ShopErp.Domain.Finance { CreateTime = this.dpTime.Value.Value, Comment = "", CreateOperator = "", FinaceAccountId = 0, Id = 0, Money = 0, Opposite = "", Type = "" });
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var itemsUnSaved = this.finances.Where(obj => obj.Id < 1 && Math.Abs(obj.Money) >= 0.01).ToArray();
                var itemsUpdate = this.finances.Where(obj => obj.Id > 0).ToArray();
                foreach (var v in itemsUnSaved)
                {
                    if (v.FinaceAccountId < 1 || string.IsNullOrWhiteSpace(v.Type))
                    {
                        throw new Exception("类型为空，或者支出账户为空");
                    }
                    v.Money = Types.First(obj => obj.Name == v.Type).Mode == FinanceTypeMode.OUTPUT ? -1 * Math.Abs(v.Money) : Math.Abs(v.Money);
                    v.CreateTime = dpTime.Value.Value;
                    v.Comment = v.Comment ?? "";
                    v.CreateOperator = OperatorService.LoginOperator.Number;
                    v.Opposite = v.Opposite ?? "";
                    if (v.Id < 1)
                    {
                        v.Id = ServiceContainer.GetService<FinanceService>().Save(v);
                    }
                    else
                    {
                        ServiceContainer.GetService<FinanceService>().Update(v);
                    }
                }
                foreach (var v in itemsUpdate)
                {
                    ServiceContainer.GetService<FinanceService>().Update(v);
                }
                MessageBox.Show("保存成功");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvItems.SelectedCells.Count < 1)
                {
                    return;
                }
                var item = this.dgvItems.SelectedCells[0].Item as ShopErp.Domain.Finance;
                if (item == null)
                {
                    throw new Exception("程序错误，类型不为：" + typeof(ShopErp.Domain.Finance).FullName);
                }
                this.finances.Remove(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dpTime.Value = DateTime.Now.Date.AddHours(20);
            this.dgvItems.ItemsSource = this.finances;
            this.finances.Add(new ShopErp.Domain.Finance { CreateTime = this.dpTime.Value.Value, Type = "支出-货款" });
            this.finances.Add(new ShopErp.Domain.Finance { CreateTime = this.dpTime.Value.Value, Type = "支出-快递" });
            this.finances.Add(new ShopErp.Domain.Finance { CreateTime = this.dpTime.Value.Value, Type = "支出-耗材" });
            this.finances.Add(new ShopErp.Domain.Finance { CreateTime = this.dpTime.Value.Value, Type = "支出-人工", Money = 50 });
            this.finances.Add(new ShopErp.Domain.Finance { CreateTime = this.dpTime.Value.Value, Type = "收入-退货" });
        }
    }
}
