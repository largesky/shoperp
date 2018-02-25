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

namespace ShopErp.App.Views.Config
{
    /// <summary>
    /// SystemCleanUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class SystemCleanUserControl : UserControl
    {
        private bool myLoaded = false;
        List<SystemCleanViewModel> items = new List<SystemCleanViewModel>();

        public SystemCleanUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoaded)
                {
                    return;
                }
                this.dpTime.SelectedDate = DateTime.Now.AddMonths(-6);

                this.IsEnabled = OperatorService.LoginOperator.Rights.Contains("数据清理");
                if (this.IsEnabled == false)
                {
                    return;
                }
                items.Add(new SystemCleanViewModel
                {
                    State = "待处理",
                    TableName = typeof(DeliveryIn).Name.ToLower(),
                    Time = "",
                    Title = "收件记录"
                });
                items.Add(new SystemCleanViewModel
                {
                    State = "待处理",
                    TableName = typeof(DeliveryOut).Name.ToLower(),
                    Time = "",
                    Title = "发货记录"
                });
                items.Add(new SystemCleanViewModel
                {
                    State = "待处理",
                    TableName = typeof(PrintHistory).Name.ToLower(),
                    Time = "",
                    Title = "打印历史"
                });
                items.Add(new SystemCleanViewModel
                {
                    State = "待处理",
                    TableName = typeof(ReturnCash).Name.ToLower(),
                    Time = "",
                    Title = "好评返现"
                });
                items.Add(new SystemCleanViewModel { State = "待处理", TableName = typeof(ShopErp.Domain.Finance).Name.ToLower(), Time = "", Title = "日常记账" });
                items.Add(new SystemCleanViewModel { State = "待处理", TableName = typeof(Order).Name.ToLower(), Time = "", Title = "订单" });
                items.Add(new SystemCleanViewModel
                {
                    State = "待处理",
                    TableName = typeof(OrderModifyHistory).Name.ToLower(),
                    Time = "",
                    Title = "订单修改记录"
                });
                items.Add(new SystemCleanViewModel
                {
                    State = "待处理",
                    TableName = typeof(OrderReturn).Name.ToLower(),
                    Time = "",
                    Title = "退货记录"
                });

                foreach (var item in items)
                {
                    item.Count = new SystemCleanService().GetTableCountAll(item.TableName).data;
                }
                this.dgvItems.ItemsSource = items;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClean_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item.Time))
                    {
                        item.State = "时间不能为空";
                        return;
                    }
                    var dt = DateTime.Parse(item.Time);
                    if (DateTime.Now.Subtract(dt).TotalDays < 180)
                    {
                        item.State = "清理时间必须是6个月以前的数据";
                        return;
                    }
                }

                if (MessageBox.Show("是否要开始清理?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }

                foreach (var item in items)
                {
                    item.State = string.Format("已清理数据：{0}", ServiceContainer.GetService<SystemCleanService>().DeleteTableData(item.TableName, DateTime.Parse(item.Time)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dpTime_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                foreach (var item in items)
                {
                    item.Time = this.dpTime.SelectedDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}