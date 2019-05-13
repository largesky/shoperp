
using ShopErp.App.Domain;
using ShopErp.App.ViewModels;

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using ShopErp.App.Utils;
using System.Printing;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Service.Print;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// PrintHistoryUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PrintHistoryUserControl : UserControl
    {
        private PrintHistoryService printHistoryService = ServiceContainer.GetService<PrintHistoryService>();

        private bool myLoaded = false;

        public PrintHistoryUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.myLoaded)
            {
                return;
            }
            this.myLoaded = true;
            var list = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name).ToList();
            list.Insert(0, "所有");
            this.cbbDeliveryCompany.ItemsSource = list;
        }

        private void ResetViewState(IEnumerable<PrintHistoryViewModel> ph)
        {
            foreach (var item in ph)
            {
                item.State = "";
                item.Background = null;
                WPFHelper.DoEvents();
            }
        }

        private PrintHistoryViewModel[] GetSelected(object sender)
        {
            PrintHistoryGroupViewModel vm = (sender as FrameworkElement).Tag as PrintHistoryGroupViewModel;
            PrintHistoryViewModel[] selected = vm.OrderViewModels.Where(obj => obj.IsChecked).ToArray();
            if (selected.Length < 1)
            {
                MessageBox.Show("请选择要相应的打印信息");
            }
            return selected;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sId = this.tbOrderId.Text.Trim();
                string deliveryNumber = this.tbDeliveryNumber.Text.Trim();
                DateTime startTime = this.dpStart.Value == null ? DateTime.MinValue : this.dpStart.Value.Value;
                DateTime endTime = this.dpEnd.Value == null ? DateTime.MinValue : this.dpEnd.Value.Value;
                long lId = string.IsNullOrWhiteSpace(sId) ? 0 : long.Parse(sId);
                if (lId < 1 && String.IsNullOrWhiteSpace(deliveryNumber) && startTime == DateTime.MinValue)
                {
                    MessageBox.Show("查询信息不能全为空");
                    return;
                }
                var item = ServiceContainer.GetService<PrintHistoryService>().GetByAll(lId, this.cbbDeliveryCompany.Text.Trim() == "所有" ? "" : this.cbbDeliveryCompany.Text.Trim(), deliveryNumber, 0, startTime, endTime, 0, 0);
                var group = item.Datas.GroupBy(obj => obj.DeliveryTemplate);
                var printGroup =
                    group.Select(
                        new Func<IGrouping<string, PrintHistory>, PrintHistoryGroupViewModel>(
                            (gs) => new PrintHistoryGroupViewModel(gs.ToArray())));
                this.tcOrderPages.ItemsSource = printGroup;
                if (printGroup.Count() > 0)
                {
                    this.tcOrderPages.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox fe = sender as CheckBox;
                if (fe == null)
                {
                    return;
                }

                PrintHistoryGroupViewModel vm = fe.Tag as PrintHistoryGroupViewModel;
                if (vm == null)
                {
                    return;
                }

                foreach (var item in vm.OrderViewModels)
                {
                    item.IsChecked = fe.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnMarkPopDelivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = this.GetSelected(sender);
                this.ResetViewState(selected);
                WPFHelper.DoEvents();
                foreach (var item in selected)
                {
                    try
                    {
                        item.Background = Brushes.Yellow;
                        item.State = "";
                        ServiceContainer.GetService<OrderService>().MarkPopDelivery(item.Source.OrderId, "");
                        ServiceContainer.GetService<PrintHistoryService>().Update(item.Source);
                        item.State = "标记发货成功";
                        item.Background = null;
                    }
                    catch (Exception ex)
                    {
                        item.State = ex.Message;
                        item.Background = Brushes.Red;
                    }
                    finally
                    {
                        WPFHelper.DoEvents();
                    }
                }
                MessageBox.Show("已完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show("标记发货失败:" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printHistorys = this.GetSelected(sender);
                if (printHistorys.Length < 1)
                {
                    return;
                }
                this.ResetViewState(printHistorys);
                foreach (var item in printHistorys)
                {
                    try
                    {
                        this.printHistoryService.Upload(item.Source);
                        item.State = "上传成功";
                        item.Background = null;
                    }
                    catch (Exception ex)
                    {
                        item.Background = Brushes.Red;
                        item.State = ex.Message;
                    }

                    WPFHelper.DoEvents();
                }
                MessageBox.Show("已完成");
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
                var printHistorys = this.GetSelected(sender);
                if (printHistorys.Length < 1)
                {
                    return;
                }

                if (MessageBox.Show("是否删除打印历史?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Error) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                this.ResetViewState(printHistorys);
                foreach (var ph in printHistorys)
                {
                    string state = "删除成功";
                    try
                    {
                        ServiceContainer.GetService<PrintHistoryService>().Delete(ph.Source.Id);
                    }
                    catch (Exception ex)
                    {
                        state = ex.Message;
                    }
                    finally
                    {
                        ph.State = state;
                    }
                    WPFHelper.DoEvents();
                }
                MessageBox.Show("已完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRestPrintState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("是否重置?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }

                OrderService os = ServiceContainer.GetService<OrderService>();
                var printHistorys = this.GetSelected(sender);
                if (printHistorys.Length < 1)
                {
                    return;
                }
                this.ResetViewState(printHistorys);
                WPFHelper.DoEvents();
                foreach (var item in printHistorys)
                {
                    try
                    {
                        os.ResetPrintState(item.Source.OrderId);
                        item.State = "重置成功";
                    }
                    catch (Exception ex)
                    {
                        item.State = ex.Message;
                    }
                    WPFHelper.DoEvents();
                }
                MessageBox.Show("已完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void miSelectPre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem mi = sender as MenuItem;
                var datas = mi.DataContext as PrintHistoryGroupViewModel;
                if (datas == null)
                {
                    throw new Exception("没有任何数据");
                }
                var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
                var cells = dg.SelectedCells;
                if (cells.Count < 1)
                {
                    return;
                }

                var item = cells[0].Item as PrintHistoryViewModel;
                if (item == null)
                {
                    throw new Exception("数据对象不正确");
                }

                int index = datas.OrderViewModels.IndexOf(item);

                for (int i = 0; i < datas.OrderViewModels.Count; i++)
                {
                    datas.OrderViewModels[i].IsChecked = i <= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void miSelectForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem mi = sender as MenuItem;
                var datas = mi.DataContext as PrintHistoryGroupViewModel;
                if (datas == null)
                {
                    throw new Exception("没有任何数据");
                }
                var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
                var cells = dg.SelectedCells;
                if (cells.Count < 1)
                {
                    return;
                }

                var item = cells[0].Item as PrintHistoryViewModel;
                if (item == null)
                {
                    throw new Exception("数据对象不正确");
                }

                int index = datas.OrderViewModels.IndexOf(item);

                for (int i = 0; i < datas.OrderViewModels.Count; i++)
                {
                    datas.OrderViewModels[i].IsChecked = i >= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}