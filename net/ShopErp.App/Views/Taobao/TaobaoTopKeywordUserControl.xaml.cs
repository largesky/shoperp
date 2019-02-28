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

namespace ShopErp.App.Views.Taobao
{
    /// <summary>
    /// TaobaoTopKeywordUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TaobaoTopKeywordUserControl : UserControl
    {
        public TaobaoTopKeywordUserControl()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string json = this.tbJson.Text.Trim();
                if (string.IsNullOrWhiteSpace(json))
                {
                    MessageBox.Show("内容为空");
                    return;
                }

                var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoTopKeywordResponse>(json);
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = "xlsx";
                sfd.Filter = "*.xlsx|Office 2007 文件";
                if (sfd.ShowDialog().Value == false)
                {
                    return;
                }

                //准备数据

                Dictionary<string, string[][]> sheetDatas = new Dictionary<string, string[][]>();

                string[][] hot = new string[resp.data.hotList.Length + 1][];
                hot[0] = new string[] { "排行", "关键词", "搜索人气", "点击率", "支付转化率" };
                for (int i = 1; i < hot.Length; i++)
                {
                    hot[i] = new string[5];
                    hot[i][0] = resp.data.hotList[i - 1].hotSearchRank.ToString();
                    hot[i][1] = resp.data.hotList[i - 1].searchWord.Trim();
                    hot[i][2] = resp.data.hotList[i - 1].seIpvUvHits.ToString();
                    hot[i][3] = resp.data.hotList[i - 1].clickRate.ToString("F2");
                    hot[i][4] = resp.data.hotList[i - 1].payRate.ToString("F2");
                }
                sheetDatas.Add("热搜", hot);

                string[][] soar = new string[resp.data.soarList.Length + 1][];
                soar[0] = new string[] { "排行", "关键词", "增长幅度", "搜索人气", "点击率", "支付转化率" };
                for (int i = 1; i < soar.Length; i++)
                {
                    soar[i] = new string[6];
                    soar[i][0] = resp.data.soarList[i - 1].soarRank.ToString();
                    soar[i][1] = resp.data.soarList[i - 1].searchWord.Trim();
                    soar[i][2] = resp.data.soarList[i - 1].seRiseRate.ToString("F2");
                    soar[i][3] = resp.data.soarList[i - 1].seIpvUvHits.ToString();
                    soar[i][4] = resp.data.soarList[i - 1].clickRate.ToString("F2");
                    soar[i][5] = resp.data.soarList[i - 1].payRate.ToString("F2");
                }
                sheetDatas.Add("飙升", soar);
                Service.Excel.ExcelFile.WriteXlsx(sfd.FileName, sheetDatas);
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
