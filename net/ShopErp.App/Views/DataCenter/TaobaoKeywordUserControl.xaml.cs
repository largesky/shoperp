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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopErp.App.Views.DataCenter
{
    /// <summary>
    /// TaobaoKeywordUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TaobaoKeywordUserControl : UserControl
    {
        private System.Collections.ObjectModel.ObservableCollection<TaobaoKeyword> keywords = new System.Collections.ObjectModel.ObservableCollection<TaobaoKeyword>();

        private bool myLoaded = false;

        public TaobaoKeywordUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoaded)
                    return;
                this.dgvWords.ItemsSource = this.keywords;
                this.btnRefresh_Click(null, null);
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var minTime = ServiceContainer.GetService<TaobaoKeywordService>().GetDBMinTime();
                this.keywords.Add(new TaobaoKeyword { Number = "", Words = "", Start = minTime, End = minTime });
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
                if (this.dgvWords.SelectedCells.Count < 1)
                {
                    return;
                }
                var k = this.dgvWords.SelectedCells[0].Item as TaobaoKeyword;
                if (k == null)
                {
                    throw new InvalidProgramException("数据类型不对");
                }
                this.keywords.Remove(k);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.keywords.Any(obj => string.IsNullOrWhiteSpace(obj.Number)))
                {
                    throw new Exception("有货号为空");
                }
                if (this.keywords.Select(obj => obj.Number).Distinct().Count() != this.keywords.Count)
                {
                    throw new Exception("有货号重复");
                }
                char[] ccs = { ' ', ',', '，' };
                foreach (var key in this.keywords)
                {
                    string[] keys = key.Words.Split(ccs, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
                    key.Words = string.Join(",", keys);
                    if (key.Id > 0)
                    {
                        ServiceContainer.GetService<TaobaoKeywordService>().Update(key);
                    }
                    else
                    {
                        key.Id = ServiceContainer.GetService<TaobaoKeywordService>().Save(key);
                    }
                }

                var todelete = ServiceContainer.GetService<TaobaoKeywordService>().GetByAll().Datas.Where(obj => this.keywords.FirstOrDefault(o => o.Id == obj.Id) == null).ToArray();
                foreach (var v in todelete)
                {
                    ServiceContainer.GetService<TaobaoKeywordService>().Delete(v.Id);
                }
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.keywords.Clear();
                var key = ServiceContainer.GetService<TaobaoKeywordService>().GetByAll().Datas;
                foreach (var k in key)
                {
                    this.keywords.Add(k);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
