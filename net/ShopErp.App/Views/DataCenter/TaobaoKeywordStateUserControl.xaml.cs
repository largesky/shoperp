using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopErp.App.Views.DataCenter
{
    /// <summary>
    /// TaobaoKeywordStateUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TaobaoKeywordStateUserControl : UserControl
    {
        private SortedDictionary<DateTime, List<TaobaoKeywordDetail>> dicKeywords;

        private bool myLoaded = false;

        public TaobaoKeywordStateUserControl()
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
                var dts = new DataTypeSelectViewModel[] { new DataTypeSelectViewModel(true, "总数", System.Drawing.Color.Red), new DataTypeSelectViewModel(false, "支付件数", System.Drawing.Color.Yellow), new DataTypeSelectViewModel(false, "购物车", System.Drawing.Color.Green), new DataTypeSelectViewModel(false, "收藏", System.Drawing.Color.Blue), new DataTypeSelectViewModel(false, "相关性", System.Drawing.Color.Pink) };
                DependencyPropertyDescriptor notiy = DependencyPropertyDescriptor.FromProperty(DataTypeSelectViewModel.IsCheckedProperty, typeof(DataTypeSelectViewModel));
                foreach (var np in dts)
                {
                    notiy.AddValueChanged(np, ViewModelCheckedHandler);
                }
                this.lbDataTypes.ItemsSource = dts;
                this.cbbCharType.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
                this.cbbCharType.SelectedItem = SeriesChartType.Column;
                this.cbbKeyWords.ItemsSource = ServiceContainer.GetService<TaobaoKeywordService>().GetByAll().Datas;
                this.myLoaded = true;
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
                DateTime start = this.dpStart.Value == null ? DateTime.MinValue : this.dpStart.Value.Value;
                DateTime end = this.dpEnd.Value == null ? DateTime.MinValue : this.dpEnd.Value.Value;

                var s = ServiceContainer.GetService<SystemConfigService>();
                if (string.IsNullOrWhiteSpace(number) || (s.IsDBMinTime(start) && s.IsDBMinTime(end)))
                {
                    throw new Exception("查询货号不能空，必须有起始时间");
                }
                this.dicKeywords = new SortedDictionary<DateTime, List<TaobaoKeywordDetail>>();
                var allItems = ServiceContainer.GetService<TaobaoKeywordDetailService>().GetByAll(number, start, end, 0, 0).Datas;
                foreach (var v in allItems)
                {
                    if (this.dicKeywords.ContainsKey(v.CreateTime) == false)
                    {
                        this.dicKeywords[v.CreateTime] = new List<TaobaoKeywordDetail>();
                    }
                    this.dicKeywords[v.CreateTime].Add(v);
                }
                this.Create();
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
                this.cbbKeyWords.SelectedItem = null;
                this.cbbKeyWords.ItemsSource = ServiceContainer.GetService<TaobaoKeywordService>().GetByAll().Datas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbKeyword_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count < 1)
                {
                    this.lbKeywords.ItemsSource = null;
                    this.chart1.Series.Clear();
                    return;
                }
                this.lbKeywords.ItemsSource = (e.AddedItems[0] as TaobaoKeyword).WordsArray;
                this.Create();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbCharType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                foreach (var v in this.chart1.Series)
                {
                    v.ChartType = (SeriesChartType)this.cbbCharType.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ViewModelCheckedHandler(object sender, EventArgs e)
        {
            this.Create();
        }


        private void Create()
        {
            try
            {
                this.chart1.Series.Clear();
                if (this.dicKeywords == null || this.dicKeywords.Count < 1)
                {
                    return;
                }
                var dts = this.lbDataTypes.ItemsSource.OfType<DataTypeSelectViewModel>().Where(obj => obj.IsChecked).ToArray();
                if (dts.Count() < 1)
                {
                    return;
                }
                string keywords = this.lbKeywords.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(keywords))
                {
                    return;
                }
                foreach (var dt in dts)
                {
                    var s = new Series(dt.Name) { Color = dt.Color, ChartType = (SeriesChartType)this.cbbCharType.SelectedItem };
                    int i = 1;
                    foreach (var parire in this.dicKeywords)
                    {
                        var dp = new DataPoint { XValue = i++, YValues = new double[] { 0 }, AxisLabel = parire.Key.ToString("MM-dd"), Label = "0" };
                        var items = parire.Value.Where(obj => obj.Keywords.Contains(keywords)).ToArray();
                        if (items.Length > 0)
                        {
                            if (dt.Name == "总数")
                            {
                                dp.YValues[0] = items.Sum(obj => obj.Total);
                            }
                            else if (dt.Name == "支付件数")
                            {
                                dp.YValues[0] = items.Sum(obj => obj.Sale);
                            }
                            else if (dt.Name == "购物车")
                            {
                                dp.YValues[0] = items.Sum(obj => obj.AddCat);
                            }
                            else if (dt.Name == "收藏")
                            {
                                dp.YValues[0] = items.Sum(obj => obj.Collect);
                            }
                            else if (dt.Name == "相关性")
                            {
                                dp.YValues[0] = (items.Sum(obj => obj.Sale) + items.Sum(obj => obj.Collect) + items.Sum(obj => obj.AddCat)) * 1.0F / (items.Sum(obj => obj.Total) == 0 ? 1 : items.Sum(obj => obj.Total));
                            }
                            else
                            {
                                throw new Exception("未识别的数据类型");
                            }
                            dp.Label = dp.YValues[0].ToString("F2");
                        }
                        s.Points.Add(dp);
                    }
                    this.chart1.Series.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lbKeywords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Create();
        }

        private void cbbKeyWords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count < 1)
                {
                    this.lbKeywords.ItemsSource = null;
                    this.chart1.Series.Clear();
                    return;
                }
                this.lbKeywords.ItemsSource = (e.AddedItems[0] as TaobaoKeyword).WordsArray;
                this.Create();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
