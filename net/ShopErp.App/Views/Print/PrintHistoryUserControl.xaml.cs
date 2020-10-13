
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
using ShopErp.App.Views.Extenstions;

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
            try
            {
                if (this.myLoaded)
                {
                    return;
                }
                this.myLoaded = true;
                var list = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name).ToList();
                list.Insert(0, "所有");
                this.cbbDeliverySourceTypes.Bind<WuliuPrintTemplateSourceType>();
                this.cbbDeliveryCompany.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ResetViewState(IEnumerable<PrintHistoryViewModel> ph)
        {
            foreach (var item in ph)
            {
                item.Background = null;
                WPFHelper.DoEvents();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sId = this.tbOrderId.Text.Trim();
                string deliveryNumber = this.tbDeliveryNumber.Text.Trim();
                WuliuPrintTemplateSourceType wuliuPrintTemplateSourceType = this.cbbDeliverySourceTypes.GetSelectedEnum<WuliuPrintTemplateSourceType>();
                DateTime startTime = this.dpStart.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpStart.Value.Value;
                DateTime endTime = this.dpEnd.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpEnd.Value.Value;
                long lId = string.IsNullOrWhiteSpace(sId) ? 0 : long.Parse(sId);
                if (lId < 1 && String.IsNullOrWhiteSpace(deliveryNumber) && startTime == Utils.DateTimeUtil.DbMinTime)
                {
                    MessageBox.Show("查询信息不能全为空");
                    return;
                }
                var item = ServiceContainer.GetService<PrintHistoryService>().GetByAll(lId, this.cbbDeliveryCompany.Text.Trim() == "所有" ? "" : this.cbbDeliveryCompany.Text.Trim(), deliveryNumber, wuliuPrintTemplateSourceType, startTime, endTime, 0, 0);
                var ps = item.Datas.Select(obj => new PrintHistoryViewModel(obj, null)).ToArray();
                for (int i = 0; i < ps.Length; i++)
                {
                    ps[i].Background = (i % 2 == 0) ? PrintHistoryViewModel.DEFAULTBACKGROUND_LIGHTGREEN : PrintHistoryViewModel.DEFAULTBACKGROUND_LIGHTPINK;
                    ps[i].IsChecked = DateTimeUtil.IsDbMinTime(ps[i].Source.UploadTime);
                }
                this.dgvItems.ItemsSource = ps;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var ps = this.dgvItems.ItemsSource as PrintHistoryViewModel[];
                if (ps == null || ps.Length < 1)
                {
                    MessageBox.Show("没有数据");
                }
                foreach (var v in ps)
                {
                    v.IsChecked = this.chkAll.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ps = this.dgvItems.ItemsSource as PrintHistoryViewModel[];
                if (ps == null || ps.Length < 1)
                {
                    MessageBox.Show("没有数据");
                }
                this.ResetViewState(ps);
                foreach (var item in ps)
                {
                    try
                    {
                        this.printHistoryService.Upload(item.Source);
                        item.Background = null;
                    }
                    catch (Exception ex)
                    {
                        item.Background = Brushes.Red;
                        MessageBox.Show(ex.Message);
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
                var ps = this.dgvItems.ItemsSource as PrintHistoryViewModel[];
                if (ps == null || ps.Length < 1)
                {
                    MessageBox.Show("没有数据");
                    return;
                }

                if (MessageBox.Show("是否删除打印历史?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                {
                    return;
                }
                this.ResetViewState(ps);
                foreach (var ph in ps)
                {
                    try
                    {
                        ServiceContainer.GetService<PrintHistoryService>().Delete(ph.Source.Id);
                    }
                    catch (Exception ex)
                    {
                        ph.Background = Brushes.Red;
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
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
                var ps = this.dgvItems.ItemsSource as PrintHistoryViewModel[];
                if (ps == null || ps.Length < 1)
                {
                    MessageBox.Show("没有数据");
                    return;
                }

                if (MessageBox.Show("是否重置?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) != MessageBoxResult.Yes)
                {
                    return;
                }

                OrderService os = ServiceContainer.GetService<OrderService>();

                this.ResetViewState(ps);
                WPFHelper.DoEvents();
                foreach (var item in ps)
                {
                    try
                    {
                        os.ResetPrintState(item.Source.OrderId);
                    }
                    catch (Exception ex)
                    {
                        item.Background = Brushes.Red;
                        MessageBox.Show(ex.Message);
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

        private void miSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ps = this.dgvItems.ItemsSource as PrintHistoryViewModel[];
                if (ps == null || ps.Length < 1)
                {
                    MessageBox.Show("没有数据");
                    return;
                }
                MenuItem mi = sender as MenuItem;
                var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
                var cells = dg.SelectedCells;
                if (cells.Count < 1)
                {
                    return;
                }
                var item = cells[0].Item as PrintHistoryViewModel;
                if (item == null)
                {
                    throw new Exception("数据对象不正确，应为：" + typeof(PrintHistoryViewModel).FullName);
                }
                bool isPre = mi.Header.ToString().Contains("向前选择");
                int index = Array.IndexOf(ps, item);
                for (int i = 0; i < ps.Length; i++)
                {
                    ps[i].IsChecked = isPre ? (i <= index ? true : false) : (i >= index ? true : false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}