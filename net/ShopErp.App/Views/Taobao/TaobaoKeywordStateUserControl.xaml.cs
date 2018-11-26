using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace ShopErp.App.Views.Taobao
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

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn { ColumnName = "关键词", Caption = "关键词", DataType = typeof(string), ReadOnly = true, DefaultValue = "" });
                dt.Columns.AddRange(this.dicKeywords.Keys.Select(obj => new DataColumn { Caption = obj.ToString("yyyy-MM-dd HH:mm:ss"), ColumnName = obj.ToString("MM-dd"), DataType = typeof(string), DefaultValue = "", ReadOnly = true }).ToArray());

                var sum = new List<string>();
                sum.Add("总数");
                foreach (var key in this.dicKeywords.Keys)
                {
                    sum.Add(this.dicKeywords[key].Sum(obj => obj.Total).ToString());
                }
                var row = dt.NewRow();
                row.ItemArray = sum.ToArray();
                dt.Rows.Add(row);

                foreach (var word in se.WordsArray)
                {
                    string[] words = word.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var datas = new string[dt.Columns.Count];
                    datas[0] = word;

                    for (int i = 1; i < datas.Length; i++)
                    {
                        var time = DateTime.Parse(dt.Columns[i].Caption);
                        var total = this.dicKeywords[time].Where(obj => TaobaoKeywordDetailService.Match(words, obj.Keywords)).Sum(obj => obj.Total);
                        datas[i] = total.ToString();
                    }
                    row = dt.NewRow();
                    row.ItemArray = datas;
                    dt.Rows.Add(row);
                }
                this.dgvItems.ItemsSource = dt.DefaultView;
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
    }
}
