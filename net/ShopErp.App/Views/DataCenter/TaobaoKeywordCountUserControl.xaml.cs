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

namespace ShopErp.App.Views.DataCenter
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
                DateTime start = this.dpStart.Value == null ? DateTime.MinValue : this.dpStart.Value.Value;
                DateTime end = this.dpEnd.Value == null ? DateTime.MinValue : this.dpEnd.Value.Value;

                var s = ServiceContainer.GetService<SystemConfigService>();
                if (string.IsNullOrWhiteSpace(number) || (s.IsDBMinTime(start) && s.IsDBMinTime(end)))
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

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "OFFICE|*.xlsx";
                ofd.Multiselect = true;
                if (ofd.ShowDialog().Value == false)
                {
                    return;
                }
                var kk = ParseDic(ofd.FileNames.OrderBy(obj => obj).ToArray());
                var ss = ServiceContainer.GetService<TaobaoKeywordDetailService>();

                foreach (var vv in kk)
                {
                    if (vv.Value.Count < 1)
                    {
                        continue;
                    }
                    DateTime dt = vv.Key;

                    var items = ss.GetByAll(vv.Value[0].Number, dt, dt.AddHours(23).AddMinutes(29), 0, 0).Datas;
                    var unSaved = vv.Value.Where(obj => items.Any(v => obj.Keywords == v.Keywords) == false).ToArray();
                    if (unSaved.Length > 0)
                        ss.SaveMulti(unSaved);
                }
                ServiceContainer.GetService<TaobaoKeywordService>().UpdateStartAndEndTime();
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static int FindIndex(string[] source, string item)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (item == source[i])
                {
                    return i;
                }
            }
            throw new Exception("未在行中找到：" + item);
        }

        public static Dictionary<DateTime, List<TaobaoKeywordDetail>> ParseDic(string[] files)
        {
            //检查所有文件名称
            var fileNames = files.Select(obj => new FileInfo(obj).Name.Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            if (fileNames.Any(obj => obj.Length < 4))
            {
                throw new Exception("有文件名称不为四段格式");
            }

            if (fileNames.Select(obj => obj[0]).Distinct().Count() != 1)
            {
                throw new Exception("所有文件名称第一段 货号 不相同");
            }

            Dictionary<DateTime, List<TaobaoKeywordDetail>> dicKeywords = new Dictionary<DateTime, List<TaobaoKeywordDetail>>();
            foreach (var file in files)
            {
                string[] fileArray = new FileInfo(file).Name.Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
                DateTime dt = new DateTime(int.Parse(fileArray[1]), int.Parse(fileArray[2]), int.Parse(fileArray[3]));
                List<TaobaoKeywordDetail> allKeywords = new List<TaobaoKeywordDetail>();
                var content = Largesky.Excel.XlsxFileReader.Open(file).ReadFirstSheet().ToList();
                int index = content.FindIndex(obj => obj.FirstOrDefault(o => o == "来源名称") != null && obj.FirstOrDefault(o => o == "访客数") != null);
                if (index < 0)
                {
                    throw new Exception("未找到行内容包含 来源名称 访客数的行");
                }

                int keywordsIndex = FindIndex(content[index], "来源名称");
                int totalIndex = FindIndex(content[index], "访客数");
                int addCatIndex = FindIndex(content[index], "加购人数");
                int collectIndex = FindIndex(content[index], "收藏人数");
                int SaleIndex = FindIndex(content[index], "支付件数");

                for (int i = index + 1; i < content.Count; i++)
                {
                    //空行过滤
                    if (string.IsNullOrWhiteSpace(content[i][keywordsIndex]) || string.IsNullOrWhiteSpace(content[i][totalIndex]) || string.IsNullOrWhiteSpace(content[i][addCatIndex]) || string.IsNullOrWhiteSpace(content[i][collectIndex]) || string.IsNullOrWhiteSpace(content[i][SaleIndex]))
                    {
                        continue;
                    }

                    //过滤多的文件内容
                    if (content[i][keywordsIndex] == "来源名称")
                    {
                        continue;
                    }

                    if (allKeywords.FirstOrDefault(obj => obj.Keywords == content[i][keywordsIndex].Trim()) != null)
                    {
                        var keywords = allKeywords.FirstOrDefault(obj => obj.Keywords == content[i][keywordsIndex].Trim());
                        keywords.AddCat += int.Parse(content[i][addCatIndex]);
                        keywords.Collect += int.Parse(content[i][collectIndex]);
                        keywords.Keywords += content[i][keywordsIndex].Trim();
                        keywords.Sale += int.Parse(content[i][SaleIndex]);
                        keywords.Total += int.Parse(content[i][totalIndex]);
                        keywords.Rela = (keywords.Sale + keywords.Collect + keywords.AddCat) * 1.0F / (keywords.Total == 0 ? 1 : keywords.Total);
                    }
                    else
                    {
                        var keywords = new TaobaoKeywordDetail { CreateTime = dt.AddHours(12), Number = fileArray[0] };
                        keywords.AddCat = int.Parse(content[i][addCatIndex]);
                        keywords.Collect = int.Parse(content[i][collectIndex]);
                        keywords.Keywords = content[i][keywordsIndex].Trim();
                        keywords.Sale = int.Parse(content[i][SaleIndex]);
                        keywords.Total = int.Parse(content[i][totalIndex]);
                        keywords.Rela = (keywords.Sale + keywords.Collect + keywords.AddCat) * 1.0F / (keywords.Total == 0 ? 1 : keywords.Total);
                        allKeywords.Add(keywords);
                    }
                }
                dicKeywords.Add(dt, allKeywords);
            }

            return dicKeywords;
        }
    }
}
