
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for ReturnCashCompleteWindow.xaml
    /// </summary>
    public partial class ReturnCashCompleteWindow : Window
    {
        public ReturnCash ReturnCash { get; set; }

        public ReturnCashCompleteWindow()
        {
            InitializeComponent();
        }

        private void btnFail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string comm = this.tbComment.Text.Trim();
                if (string.IsNullOrWhiteSpace(comm))
                {
                    throw new Exception("处理失败描述为空");
                }
                this.Process(this.tbSerialNumber.Text.Trim(), comm, ReturnCashState.PROCESS_FAIL);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string seri = this.tbSerialNumber.Text.Trim();
                if (string.IsNullOrWhiteSpace(seri))
                {
                    throw new Exception("处理成功交易编号不能为空");
                }
                this.Process(seri, this.tbComment.Text.Trim(), ReturnCashState.COMPLETED);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Process(string serialNumber, string rcComment, ReturnCashState state)
        {
            try
            {
                this.ReturnCash.SerialNumber = serialNumber;
                this.ReturnCash.Comment = rcComment;
                this.ReturnCash.State = state;
                this.ReturnCash.ProcessTime = DateTime.Now;
                this.ReturnCash.ProcessOperator = OperatorService.LoginOperator.Number;
                this.ReturnCash.Image = new byte[0];
                ServiceContainer.GetService<ReturnCashService>().Update(this.ReturnCash);

                var or = ServiceContainer.GetService<OrderService>().GetById(this.ReturnCash.OrderId);
                if (or == null)
                {
                    throw new Exception("原始订单已在不存在");
                }

                var flag = or.PopFlag == ColorFlag.None ? ColorFlag.RED : or.PopFlag;
                string comment = "";
                if (state == ReturnCashState.PROCESS_FAIL)
                {
                    comment = or.PopSellerComment + "返现失败【" + OperatorService.LoginOperator.Number + rcComment + "】";
                }
                else
                {
                    comment = or.PopSellerComment + "返现成功【" + OperatorService.LoginOperator.Number + "】";
                }
                ServiceContainer.GetService<OrderService>().ModifyPopSellerComment(or.Id, flag, comment);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}