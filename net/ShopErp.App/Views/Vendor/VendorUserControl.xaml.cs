﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Service.Restful;
using ShopErp.App.Service.Spider;
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;


namespace ShopErp.App.Views.Vendor
{
    /// <summary>
    /// VendorUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class VendorUserControl : UserControl
    {
        private VendorService vendorService = ServiceContainer.GetService<VendorService>();

        public VendorUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.pbBar.Parameters.Clear();
            this.pbBar.Parameters.Add("name", this.tbName.Text.Trim());
            this.pbBar.Parameters.Add("pingyingName", tbPingyingName.Text.Trim());
            this.pbBar.Parameters.Add("marketAddress", this.tbAddress.Text.Trim());
            this.pbBar.StartPage();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new VendorEditWindow();
                window.ShowDialog();
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
                if (this.dgvVendors.SelectedCells.Count < 1)
                {
                    MessageBox.Show("请选择厂家");
                    return;
                }
                var item = this.dgvVendors.SelectedCells[0].Item as ShopErp.Domain.Vendor;
                if (item == null)
                {
                    MessageBox.Show("请选择厂家");
                    return;
                }
                ICloneable cItem = item as ICloneable;
                var window = new VendorEditWindow { Vendor = (cItem.Clone() as ShopErp.Domain.Vendor) };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PageBarUserControl_PageChanging(object sender, PageBar.PageChangeEventArgs e)
        {
            var vendors = this.vendorService.GetByAll(e.GetParameter<string>("name"), e.GetParameter<string>("pingyingName"), "", e.GetParameter<string>("marketAddress"), e.CurrentPage - 1, e.PageSize);
            this.dgvVendors.ItemsSource = vendors.Datas;
            this.pbBar.Total = vendors.Total;
            this.pbBar.CurrentCount = vendors.Datas.Count;
        }

        private void Open_Click(object sender, EventArgs e)
        {
            try
            {
                TextBlock tb = sender as TextBlock;
                if (tb == null)
                {
                    throw new Exception("Edit_Click错误，事件源不是TextBlock");
                }
                var vendor = tb.DataContext as ShopErp.Domain.Vendor;
                if (string.IsNullOrWhiteSpace(vendor.HomePage.Trim()) == false)
                    Process.Start(vendor.HomePage.Trim());
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
                if (this.dgvVendors.SelectedCells.Count < 1)
                {
                    throw new Exception("未选择厂家");
                }

                var vendor = this.dgvVendors.SelectedCells[0].Item as ShopErp.Domain.Vendor;

                if (vendor.Count > 0)
                {
                    throw new Exception("厂家还存在商品，需要先删除商品");
                }
                if (MessageBox.Show("确认删除厂家:" + vendor.Name, "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                this.vendorService.Delete(vendor.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnQuery2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vendorIds = this.vendorService.GetAllVendorIdHasGoods().Datas;
                var vendors = this.vendorService.GetByAll("", "", "", "", 0, 0).Datas;
                var ve = vendors.Where(obj => vendorIds.Contains(obj.Id) == false).ToArray();
                this.dgvVendors.ItemsSource = ve;
                this.pbBar.Total = ve.Length;
                this.pbBar.CurrentCount = ve.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DgvVendors_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                var vendor = e.Row.Item as ShopErp.Domain.Vendor;
                if (vendor == null)
                {
                    throw new InvalidProgramException("类型数据不为：" + typeof(ShopErp.Domain.Vendor));
                }

                var tb = e.EditingElement as TextBox;
                if (tb == null)
                {
                    throw new InvalidProgramException("类型控件不为：" + typeof(TextBox));
                }
                string nv = tb.Text.Trim();
                if (e.Column.Header.ToString().Contains("拼音名称(可编辑)"))
                {
                    vendor.PingyingName = nv;
                }
                else if (e.Column.Header.ToString().Contains("备注"))
                {
                    vendor.Comment = nv;
                }
                else if (e.Column.Header.ToString().Contains("市场地址(可编辑)"))
                {
                    vendor.MarketAddress = nv;
                }
                else if (e.Column.Header.ToString().Contains("市场地址简写(可编辑)"))
                {
                    vendor.MarketAddressShort = nv;
                }
                else if (e.Column.Header.ToString().Contains("同一厂家别名"))
                {
                    vendor.Alias = nv;
                }
                else
                {
                    throw new Exception("不能编辑该列数据");
                }

                ServiceContainer.GetService<VendorService>().Update(vendor);
            }
            catch (Exception exception)
            {
                e.Cancel = true;
                MessageBox.Show(exception.Message);
            }
        }

        private void dgvVendors_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = this.dgvVendors.InputHitTest(e.GetPosition(this.dgvVendors)) as FrameworkElement;
                if (item == null)
                {
                    throw new InvalidProgramException("控件类型不是：FrameworkElement");
                }
                var cell = item.Parent as DataGridCell;
                if (cell == null)
                {
                    return;
                }
                var header = cell.Column.Header.ToString();
                if (header.Contains("名称") == false)
                {
                    return;
                }
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    new Goods.GoodsCreateWindow().Show();
                }
                else
                {
                    var vendor = item.DataContext as ShopErp.Domain.Vendor;
                    if (vendor == null)
                    {
                        throw new InvalidProgramException("数据类型不是：Vendor");
                    }
                    new VendorGoodsWindow() { VendorName = vendor.Name }.Show();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void BtnGenMarketAddressShort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vendors = ServiceContainer.GetService<VendorService>().GetByAll("", "", "", "", 0, 0).Datas;
                foreach (var v in vendors)
                {
                    v.MarketAddressShort = VendorService.FormatVendorDoor(v.MarketAddress);
                    if (v.MarketAddress.Contains("温州") || v.MarketAddress.Contains("晋江") || v.MarketAddress.Contains("温岭"))
                    {

                    }
                    else
                    {
                        if (v.MarketAddress.StartsWith("成都") == false)
                        {
                            v.MarketAddress = "成都 " + v.MarketAddress;
                        }
                    }
                    ServiceContainer.GetService<VendorService>().Update(v);
                }
                MessageBox.Show("更新成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdateVendor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vendors = this.dgvVendors.ItemsSource as List<ShopErp.Domain.Vendor>;
                if (vendors == null || vendors.Count < 1)
                {
                    throw new Exception("没有厂家数据，请先查询");
                }

                var cookie = CefCookieVisitor.GetCookieValue(".go2.cn");
                int count = 0;
                foreach (var v in vendors)
                {
                    if (string.IsNullOrWhiteSpace(v.HomePage))
                    {
                        continue;
                    }
                    var sb = SpiderBase.CreateSpider(v.HomePage);
                    try
                    {
                        var nv = sb.GetVendorInfoByUrl(v.HomePage + '/', cookie);
                        bool needUpdate = false;
                        if (v.Name.Equals(nv.Name, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            v.Name = nv.Name;
                            v.PingyingName = "";
                            needUpdate = true;
                        }
                        if (v.MarketAddress != nv.MarketAddress)
                        {
                            v.MarketAddress = nv.MarketAddress;
                            if (v.MarketAddress.Contains("成都"))
                            {
                                v.MarketAddressShort = VendorService.FormatVendorDoor(v.MarketAddress);
                            }
                            needUpdate = true;
                        }
                        if (needUpdate)
                        {
                            ServiceContainer.GetService<VendorService>().Update(v);
                        }
                        v.Comment = needUpdate ? "更新成功" : "不需要更新";
                    }
                    catch (Exception ee)
                    {
                        v.Comment += ee.Message;
                    }
                    this.tbMsg.Text = String.Format("已经更新：{0}/{1},等待10秒后更新下一个", ++count, vendors.Count);
                    for (int i = 0; i < 10; i++)
                    {
                        WPFHelper.DoEvents();
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}