using ShopErp.App.Domain;
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
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for GoodsCountMarkConfigUserControl.xaml
    /// </summary>
    public partial class DeliveryCompanyUserControl : UserControl
    {
        private bool myLoaded = false;

        private System.Collections.ObjectModel.ObservableCollection<DeliveryCompany> deliveryCompanys =
            new System.Collections.ObjectModel.ObservableCollection<DeliveryCompany>();

        public DeliveryCompanyUserControl()
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
                LoadData();
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadData()
        {
            var gms = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas;
            this.deliveryCompanys.Clear();
            foreach (var v in gms)
            {
                this.deliveryCompanys.Add(v);
            }
            //查询那些没有记录的，但是模板中有的
            this.dgvItems.ItemsSource = this.deliveryCompanys;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.deliveryCompanys.Add(new DeliveryCompany
            {
                CreateTime = DateTime.Now,
                PaperMark = true,
                Id = 0,
                Name = "",
                PopMapJd = "",
                PopMapKuaidi100 = "",
                PopMapPingduoduo = "",
                PopMapTaobao = "",
                UpdateOperator = "",
                UpdateTime = DateTime.Now,
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvItems.SelectedCells.Count < 1)
                {
                    MessageBox.Show("没有选择数据");
                    return;
                }

                var item = this.dgvItems.SelectedCells[0].Item as DeliveryCompany;

                if (item == null)
                {
                    throw new InvalidProgramException("数据类型不为:" + typeof(DeliveryCompany).FullName);
                }

                if (MessageBox.Show("是否删除:" + item.Name, "", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                if (item.Id > 0)
                    ServiceContainer.GetService<DeliveryCompanyService>().Delete(item.Id);
                this.deliveryCompanys.Remove(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSaveAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.deliveryCompanys.Select(obj => obj.Name).Count() != this.deliveryCompanys.Count)
                {
                    throw new Exception("有快递公司同名");
                }

                if (this.deliveryCompanys.Any(obj => string.IsNullOrWhiteSpace(obj.Name)))
                {
                    throw new Exception("有快递公司名称为空");
                }

                foreach (var v in this.deliveryCompanys)
                {
                    if (v.Id > 0)
                    {
                        ServiceContainer.GetService<DeliveryCompanyService>().Update(v);
                    }
                    else
                    {
                        ServiceContainer.GetService<DeliveryCompanyService>().Save(v);
                    }
                }
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}