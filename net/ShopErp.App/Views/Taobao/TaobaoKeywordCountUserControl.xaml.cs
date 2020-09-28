using ShopErp.App.Service.Excel;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

namespace ShopErp.App.Views.Taobao
{
    /// <summary>
    /// TaobaoKeywordCountUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TaobaoKeywordCountUserControl : UserControl
    {
        private List<TaobaoKeywordDetail> allKeywords = null;

        private bool myLoad = false;

        public TaobaoKeywordCountUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoad)
                {
                    return;
                }
                this.dgvKeyword.ItemsSource = allKeywords;
                this.cbbKeyWords.ItemsSource = ServiceContainer.GetService<TaobaoKeywordService>().GetByAll().Datas;
                this.myLoad = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dgvKeyword.ItemsSource = this.allKeywords;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgvKeyword1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.dgvKeyword1.SelectedCells.Count < 1)
                {
                    return;
                }
                var item = this.dgvKeyword1.SelectedCells[0].Item as TaobaoKeywordDetail;
                if (string.IsNullOrWhiteSpace(item.Keywords))
                {
                    return;
                }
                string[] keys = item.Keywords.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                this.dgvKeyword.ItemsSource = this.allKeywords.Where(obj => TaobaoKeywordDetailService.Match(keys, obj.Keywords)).ToArray();
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
                this.cbbKeyWords.ItemsSource = null;
                this.cbbKeyWords.ItemsSource = ServiceContainer.GetService<TaobaoKeywordService>().GetByAll().Datas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var se = this.cbbKeyWords.SelectedItem as TaobaoKeyword;
                if (se == null)
                {
                    throw new Exception("没有选择数据");
                }
                string number = se.Number.Trim();
                DateTime start = this.dpStart.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpStart.Value.Value;
                DateTime end = this.dpEnd.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpEnd.Value.Value;

                var s = ServiceContainer.GetService<SystemConfigService>();
                if (string.IsNullOrWhiteSpace(number) || (Utils.DateTimeUtil.IsDbMinTime(start) && Utils.DateTimeUtil.IsDbMinTime(end)))
                {
                    throw new Exception("查询货号不能空，必须有起始时间");
                }
                this.allKeywords = new List<TaobaoKeywordDetail>();
                var allItems = ServiceContainer.GetService<TaobaoKeywordDetailService>().GetByAll(number, start, end, 0, 0).Datas;

                if (allItems.Count < 1)
                {
                    throw new Exception("未查询任何数据");
                }

                var min = allItems.Min(obj => obj.CreateTime).Date;
                var max = allItems.Max(obj => obj.CreateTime).Date;

                int countDay = max.Subtract(min).Days + 1;

                //合并所有关键词
                foreach (var v in allItems)
                {
                    var first = this.allKeywords.FirstOrDefault(obj => obj.Keywords == v.Keywords);
                    if (first != null)
                    {
                        first.AddCat += v.AddCat;
                        first.Collect += v.Collect;
                        first.Sale += v.Sale;
                        first.Total += v.Total;
                        first.Rela = (first.Sale + first.Collect + first.AddCat) * 1.0F / (first.Total == 0 ? 1 : first.Total);
                    }
                    else
                    {
                        this.allKeywords.Add(v);
                    }
                }
                //分析关键词
                var anlKeywords = se.WordsArray.Select(obj => new TaobaoKeywordDetail { Number = se.Number, Keywords = obj }).ToArray();
                foreach (var item in anlKeywords)
                {
                    string[] keys = item.Keywords.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var match = this.allKeywords.Where(obj => TaobaoKeywordDetailService.Match(keys, obj.Keywords)).ToArray();
                    item.Total = match.Sum(obj => obj.Total);
                    item.AddCat = match.Sum(obj => obj.AddCat);
                    item.Sale = match.Sum(obj => obj.Sale);
                    item.Collect = match.Sum(obj => obj.Collect);
                    item.Rela = (item.Sale + item.Collect + item.AddCat) * 1.0F / (item.Total == 0 ? 1 : item.Total);
                    item.DayEvg = 1.0F * item.Total / countDay;
                }
                this.dgvKeyword.ItemsSource = allKeywords;
                this.dgvKeyword1.ItemsSource = anlKeywords;
                int total = allKeywords.Select(obj => obj.Total).Sum();
                int collect = allKeywords.Select(obj => obj.Collect).Sum();
                int addCat = allKeywords.Select(obj => obj.AddCat).Sum();
                int sale = allKeywords.Select(obj => obj.Sale).Sum();
                this.tbSum.Text = string.Format("关键词数：{0}，访客数：{1}，收藏人数：{2}，加购人数：{3}，支付件数：{4}，相关性：{5:F4},数据起始：{6}  {7}", allKeywords.Count, total, collect, addCat, sale, 1.0F * (collect + addCat + sale) / total, min.ToString("yyyy-MM-dd"), max.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnShowUnMatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.allKeywords.Count < 1)
                {
                    throw new Exception("没有数据");
                }

                var se = this.cbbKeyWords.SelectedItem as TaobaoKeyword;
                if (se == null)
                {
                    throw new Exception("没有选择数据");
                }

                var ss = se.Words.Replace(",", "").Replace(" ", "");
                var unmatch = this.allKeywords.Where(obj => TaobaoKeywordDetailService.UnMatch(ss, obj.Keywords));
                this.dgvKeyword.ItemsSource = unmatch;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
