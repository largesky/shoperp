
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
    /// Interaction logic for CreateReturnCashWindow.xaml
    /// </summary>
    public partial class ReturnCashCreateWindow : Window
    {
        public Order Order { get; set; }

        public ReturnCash ReturnCash { get; set; }

        public ReturnCashCreateWindow()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Order.ShopId < 1 || this.Order.Id < 1 || string.IsNullOrWhiteSpace(this.Order.PopOrderId))
                {
                    throw new Exception("店铺编号，ERP订单编号，网店订单为空");
                }
                string accInfo = this.tbbAccountInfo.Text.Trim();
                float money = float.Parse(this.tbMoney.Text.Trim());
                string type = this.cbbTypes.Text.Trim();
                if (string.IsNullOrWhiteSpace(accInfo))
                {
                    throw new Exception("转账账号信息不能为空");
                }
                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new Exception("类不能为空");
                }
                ReturnCash rc = new ReturnCash
                {
                    AccountType = this.cbbAcountType.Text.Trim(),
                    AccountInfo = accInfo,
                    Comment = "",
                    CreateOperator = OperatorService.LoginOperator.Number,
                    CreateTime = DateTime.Now,
                    Image = new byte[0],
                    Id = 0,
                    Money = money,
                    OrderId = this.Order.Id,
                    PopOrderId = this.Order.PopOrderId,
                    ProcessOperator = "",
                    ProcessTime = Utils.DateTimeUtil.DbMinTime,
                    ShopId = this.Order.ShopId,
                    State = ReturnCashState.WAIT_PROCESS,
                    Type = type,
                    SerialNumber = "",
                };
                ServiceContainer.GetService<ReturnCashService>().Save(rc);
                MessageBox.Show("已成功");
                this.ReturnCash = rc;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}