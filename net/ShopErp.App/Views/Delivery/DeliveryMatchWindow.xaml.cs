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
            var allNumbers = deliveryNumbers.Where(obj => string.IsNullOrWhiteSpace(obj) == false);
            List<string> eNumbers = new List<string>(); //电子面单没有段号，
            List<long> dNumbers = new List<long>(); //货到付款订单
            foreach (var s in allNumbers)
            {
                if (s.StartsWith("D", StringComparison.OrdinalIgnoreCase))
                {
                    dNumbers.Add(long.Parse(s.TrimStart('D', 'd')));
                }
                else
                {
                    eNumbers.Add(s);
                }
            }
            //计算普通起始单号，个数
            eNumbers.Sort();
            dNumbers.Sort();
            var start = dNumbers.Select(obj => obj / 1000);
            Dictionary<long, int> ccD = new Dictionary<long, int>();
            foreach (var l in dNumbers.Distinct())
            {
                var lt = l / 1000;
                if (ccD.ContainsKey(lt))
                {
                    ccD[lt]++;
                }
                else
                {
                    ccD[lt] = 1;
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("文件内总数：" + allNumbers.Count());
            sb.AppendLine("有效总数：" + allNumbers.Distinct().Count());
            sb.AppendLine(string.Format("普通面单代收:总数 {0},有效总数 {1}", dNumbers.Count, dNumbers.Distinct().Count()));
            sb.AppendLine(string.Format("电子面单:总数 {0},有效总数 {1}", eNumbers.Count, eNumbers.Distinct().Count()));
            sb.AppendLine("普通面单代收明细:");
            foreach (var c in ccD)
            {
                sb.AppendLine(c.Key + "001  个数:" + c.Value);
            }
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

        private FileDeliveryInfo[] ReadFile(string file)
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
            List<FileDeliveryInfo> fdis = new List<FileDeliveryInfo>();
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

                try
                {
                    var fdi = new FileDeliveryInfo
                    {
                        DeliveryNumber = content[dnIndex],
                        Weight = float.Parse(content[wIndex]),
                        Money = 0,
                    };
                    fdis.Add(fdi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return fdis.ToArray();
        }

        private FileDeliveryInfo[] ReadFiles(string[] files)
        {
            var fdis = new List<FileDeliveryInfo>();
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
                var fdis = this.ReadFiles(this.Files);
                var dnSystem = this.DeliveryOuts.Select(obj => obj.DeliveryNumber).ToArray();
                var dnFile = fdis.Select(obj => obj.DeliveryNumber).ToArray();

                string str = AnDetail(dnSystem);
                this.tbSystemDetail.Text = str;

                str = AnDetail(dnFile);
                this.tbFileDetail.Text = str;

                List<DeliveryMatchDetail> matchDds = new List<DeliveryMatchDetail>();
                List<DeliveryMatchDetail> unmatchDds = new List<DeliveryMatchDetail>();
                var dnnSystem = this.DeliveryOuts.Distinct(new DeliveryOutComparer())
                    .OrderBy(obj => long.Parse(obj.DeliveryNumber.TrimStart('D', 'd'))).ToArray();
                var dnnFile = fdis.Distinct(new FileDeliveryInfoComparer())
                    .OrderBy(obj => long.Parse(obj.DeliveryNumber.TrimStart('D', 'd'))).ToArray();

                int i = 0, j = 0;
                for (; i < dnnSystem.Length && j < dnnFile.Length;)
                {
                    long diff = long.Parse(dnnSystem[i].DeliveryNumber.TrimStart('D', 'd')) -
                                long.Parse(dnnFile[j].DeliveryNumber.TrimStart('D', 'd'));
                    if (diff == 0)
                    {
                        var ddd = new DeliveryMatchDetail
                        {
                            OrderId = dnnSystem[i].OrderId,
                            DeliveryNumberSystem = dnnSystem[i].DeliveryNumber,
                            WeightSystem = dnnSystem[i].Weight,
                            DeliveryNumberFile = dnnFile[j].DeliveryNumber,
                            WeightFile = dnnFile[j].Weight,
                            MoneyFile = dnnFile[j].Money,
                        };
                        matchDds.Add(ddd);
                        i++;
                        j++;
                    }
                    else if (diff > 0)
                    {
                        var ddd = new DeliveryMatchDetail
                        {
                            OrderId = "",
                            DeliveryNumberSystem = "",
                            WeightSystem = 0,
                            DeliveryNumberFile = dnnFile[j].DeliveryNumber,
                            WeightFile = dnnFile[j].Weight,
                            MoneyFile = dnnFile[j].Money,
                        };
                        j++;
                        unmatchDds.Add(ddd);
                    }
                    else
                    {
                        var ddd = new DeliveryMatchDetail
                        {
                            OrderId = dnnSystem[i].OrderId,
                            DeliveryNumberSystem = dnnSystem[i].DeliveryNumber,
                            WeightSystem = dnnSystem[i].Weight,
                            DeliveryNumberFile = "",
                            WeightFile = 0,
                            MoneyFile = 0,
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
    }


    public class FileDeliveryInfo
    {
        public string DeliveryNumber { get; set; }

        public float Weight { get; set; }

        public float Money { get; set; }
    }

    public class DeliveryMatchDetail
    {
        public int RowId { get; set; }

        public string OrderId { get; set; }

        public float WeightSystem { get; set; }

        public string DeliveryNumberSystem { get; set; }

        public float WeightFile { get; set; }

        public string DeliveryNumberFile { get; set; }

        public float MoneyFile { get; set; }

        public float WeightDiff
        {
            get { return WeightSystem - WeightFile; }
        }
    }


    public class DeliveryOutComparer : IEqualityComparer<DeliveryOut>
    {
        public bool Equals(DeliveryOut x, DeliveryOut y)
        {
            return x.DeliveryNumber == y.DeliveryNumber;
        }

        public int GetHashCode(DeliveryOut obj)
        {
            return obj.DeliveryNumber.GetHashCode();
        }
    }

    public class FileDeliveryInfoComparer : IEqualityComparer<FileDeliveryInfo>
    {
        public bool Equals(FileDeliveryInfo x, FileDeliveryInfo y)
        {
            return x.DeliveryNumber == y.DeliveryNumber;
        }

        public int GetHashCode(FileDeliveryInfo obj)
        {
            return obj.DeliveryNumber.GetHashCode();
        }
    }
}