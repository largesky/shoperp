using ShopErp.App.Service.Excel;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
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
    /// TaobaoKeywordManagementUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TaobaoKeywordManagementUserControl : UserControl
    {
        public TaobaoKeywordManagementUserControl()
        {
            InitializeComponent();
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dgvKeyword.SelectedCells.Count < 1)
                {
                    MessageBox.Show("未选择数据");
                    return;
                }

                if (MessageBox.Show("确认删除关键词？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) != MessageBoxResult.Yes)
                {
                    return;
                }

                var items = this.dgvKeyword.SelectedCells.Select(obj => obj.Item as TaobaoKeywordDetail).Distinct().ToArray();
                ServiceContainer.GetService<TaobaoKeywordDetailService>().DeleteMulti(items.Select(obj => obj.Id).ToArray());
                ServiceContainer.GetService<TaobaoKeywordService>().UpdateStartAndEndTime();
                MessageBox.Show("删除成功");
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
                var allItems = ServiceContainer.GetService<TaobaoKeywordDetailService>().GetByAll(number, start, end, 0, 0).Datas;
                this.dgvKeyword.ItemsSource = allItems;
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
                ofd.Filter = "Excel 文件|*.xlsx;*.xls";
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
            var fileNames = files.Select(obj => new FileInfo(obj).Name.Split(new char[] { ' ', '-', '.' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
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
                string[] fileArray = new FileInfo(file).Name.Split(new char[] { ' ', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
                DateTime dt = new DateTime(int.Parse(fileArray[1]), int.Parse(fileArray[2]), int.Parse(fileArray[3]));
                List<TaobaoKeywordDetail> allKeywords = new List<TaobaoKeywordDetail>();
                var content = ExcelFile.Open(file).ReadFirstSheet().ToList();
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
                    var keywords = new TaobaoKeywordDetail { CreateTime = dt.AddHours(12), Number = fileArray[0] };
                    keywords.AddCat = int.Parse(content[i][addCatIndex]);
                    keywords.Collect = int.Parse(content[i][collectIndex]);
                    keywords.Keywords = content[i][keywordsIndex].Trim();
                    keywords.Sale = int.Parse(content[i][SaleIndex]);
                    keywords.Total = int.Parse(content[i][totalIndex]);
                    keywords.Rela = (keywords.Sale + keywords.Collect + keywords.AddCat) * 1.0F / (keywords.Total == 0 ? 1 : keywords.Total);
                    allKeywords.Add(keywords);
                }
                dicKeywords.Add(dt, allKeywords);
            }

            return dicKeywords;
        }
    }
}
