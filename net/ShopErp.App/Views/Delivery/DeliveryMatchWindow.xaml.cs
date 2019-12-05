using ShopErp.App.Service.Excel;
using ShopErp.Domain;
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
using System.Windows.Shapes;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryMatchWindow.xaml
    /// </summary>
    public partial class DeliveryMatchWindow : Window
    {
        public string[] Files { get; set; }

        public DeliveryOut[] DeliveryOuts { get; set; }

        public DeliveryMatchWindow()
        {
            InitializeComponent();
        }

        private string AnDetail(string[] deliveryNumbers)
        {
            var allNumbers = deliveryNumbers.Where(obj => string.IsNullOrWhiteSpace(obj) == false).ToList();
            allNumbers.Sort();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("文件内总数：" + allNumbers.Count());
            sb.AppendLine("有效总数：" + allNumbers.Distinct().Count());
            return sb.ToString();
        }

        private int GetIndex(string[] content, string str)
        {
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] != null && content[i].Contains(str))
                {
                    return i;
                }
            }
            return -1;
        }

        private string[] ReadFile(string file)
        {
            ExcelFile fr = ExcelFile.Open(file);
            string[][] contents = fr.ReadFirstSheet();

            if (contents.Length < 1)
            {
                throw new Exception("文件中没有内容:" + file);
            }

            int dnIndex = GetIndex(contents[0], "运单号");
            if (dnIndex < 0)
            {
                throw new Exception("文件中没有找到 运单号 列");
            }

            int wIndex = GetIndex(contents[0], "重量");
            if (wIndex < 0)
            {
                throw new Exception("文件中没有找到 重量 列:" + file);
            }
            List<string> fdis = new List<string>();
            for (int i = 1; i < contents.Length; i++)
            {
                string[] content = contents[i];
                if (dnIndex >= content.Length || wIndex >= content.Length)
                {
                    throw new Exception("第" + (i + 1) + "行数据不完整:" + file);
                }
                if (string.IsNullOrWhiteSpace(content[dnIndex]))
                {
                    continue;
                }
                fdis.Add(content[dnIndex]);
            }
            return fdis.ToArray();
        }

        private string[] ReadFiles(string[] files)
        {
            var fdis = new List<string>();
            foreach (var file in files)
            {
                try
                {
                    var fff = ReadFile(file);
                    fdis.AddRange(fff);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + Environment.NewLine + file, ex);
                }
            }
            return fdis.ToArray();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //分析统计
                var dnSystem = this.DeliveryOuts.Select(obj => obj.DeliveryNumber).Distinct().ToList();
                var dnFile = this.ReadFiles(this.Files).Distinct().ToList();
                List<DeliveryMatchDetail> matchDds = new List<DeliveryMatchDetail>();
                List<DeliveryMatchDetail> unmatchDds = new List<DeliveryMatchDetail>();
                int i = 0, j = 0;

                dnSystem.Sort();
                dnFile.Sort();

                for (; i < dnSystem.Count && j < dnFile.Count;)
                {
                    long diff = dnSystem[i].CompareTo(dnFile[j]);
                    if (diff == 0)
                    {
                        var ddd = new DeliveryMatchDetail
                        {
                            DeliveryNumberSystem = dnSystem[i],
                            DeliveryNumberFile = dnFile[j],
                        };
                        matchDds.Add(ddd);
                        i++;
                        j++;
                    }
                    else if (diff > 0)
                    {
                        var ddd = new DeliveryMatchDetail
                        {
                            DeliveryNumberSystem = "",
                            DeliveryNumberFile = dnFile[j],
                        };
                        j++;
                        unmatchDds.Add(ddd);
                    }
                    else
                    {
                        var ddd = new DeliveryMatchDetail
                        {
                            DeliveryNumberSystem = dnSystem[i],
                            DeliveryNumberFile = "",
                        };
                        i++;
                        unmatchDds.Add(ddd);
                    }
                }
                matchDds.AddRange(unmatchDds);
                for (i = 0; i < matchDds.Count; i++)
                {
                    matchDds[i].RowId = i + 1;
                }
                this.dgvMatch.ItemsSource = matchDds;

                if (unmatchDds.Count > 0)
                {
                    MessageBox.Show("数据未完全匹配", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("数据成功完全匹配", "恭喜", MessageBoxButton.OK, MessageBoxImage.None);
                }
                this.tbSuminfo.Text = string.Format("系统发货记录共：{0}条，文件发货记录共：{1}条", dnSystem.Count, dnFile.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
    }

    public class DeliveryMatchDetail
    {
        public int RowId { get; set; }
        public string DeliveryNumberSystem { get; set; }
        public string DeliveryNumberFile { get; set; }
    }

}