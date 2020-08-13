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

namespace ShopErp.App.Views.PageBar
{
    /// <summary>
    /// PageBarUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PageBarUserControl : UserControl
    {
        public static readonly DependencyProperty TotalProperty = DependencyProperty.Register("Total", typeof(int), typeof(PageBarUserControl), new PropertyMetadata(0));

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register("CurrentPage", typeof(int), typeof(PageBarUserControl), new PropertyMetadata(0));

        public static readonly DependencyProperty TotalPageProperty = DependencyProperty.Register("TotalPage", typeof(int), typeof(PageBarUserControl), new PropertyMetadata(0));

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register("PageSize", typeof(int), typeof(PageBarUserControl), new PropertyMetadata(20));

        public static readonly DependencyProperty CurrentCountProperty = DependencyProperty.Register("CurrentCount", typeof(int), typeof(PageBarUserControl), new PropertyMetadata(0));

        public static readonly DependencyProperty TitleMessageProperty = DependencyProperty.Register("TitleMessage", typeof(string), typeof(PageBarUserControl), new PropertyMetadata(""));

        private bool hasPaged = false;

        public int Total
        {
            get { return (int)this.GetValue(TotalProperty); }
            set { this.SetValue(TotalProperty, value); }
        }

        public int CurrentPage
        {
            get { return (int)this.GetValue(CurrentPageProperty); }
            set { this.SetValue(CurrentPageProperty, value); }
        }

        public int TotalPage
        {
            get { return (int)this.GetValue(TotalPageProperty); }
            set { this.SetValue(TotalPageProperty, value); }
        }

        public int PageSize
        {
            get { return (int)this.GetValue(PageSizeProperty); }
            set { this.SetValue(PageSizeProperty, value); }
        }

        public int CurrentCount
        {
            get { return (int)this.GetValue(CurrentCountProperty); }
            set { this.SetValue(CurrentCountProperty, value); }
        }

        public string TitleMessage
        {
            get { return (string)this.GetValue(TitleMessageProperty); }
            set { this.SetValue(TitleMessageProperty, value); }
        }

        public IDictionary<string, Object> Parameters { get; private set; }

        public event EventHandler<PageChangeEventArgs> PageChanging;

        public PageBarUserControl()
        {
            InitializeComponent();
            this.Parameters = new Dictionary<string, object>();
        }

        protected virtual void OnPageChaning(PageChangeEventArgs e)
        {
            if (this.PageChanging != null)
            {
                this.PageChanging(this, e);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TotalProperty)
            {
                this.UpdateTotal(this.Total);
            }
            else if (e.Property == PageSizeProperty)
            {
                this.UpdatePageSize(this.PageSize);
            }

            base.OnPropertyChanged(e);
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            this.GoToPage(1);
        }

        private void btnPre_Click(object sender, RoutedEventArgs e)
        {
            this.GoToPage(this.CurrentPage - 1);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.GoToPage(this.CurrentPage + 1);
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            this.GoToPage(this.TotalPage);
        }

        private void btnGoTo_Click(object sender, RoutedEventArgs e)
        {
            string page = this.tbGoToPgae.Text.Trim();
            int iPage = 0;
            try
            {
                iPage = int.Parse(page);
                if (iPage < 1)
                {
                    MessageBox.Show("页数必须为正数");
                    return;
                }

                if (iPage > this.TotalPage)
                {
                    MessageBox.Show("当面不能大于当前:" + this.TotalPage);
                    return;
                }
                this.GoToPage(iPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("请输入要查看的页:" + ex.Message);
                return;
            }
        }

        private void UpdateTotal(int total)
        {
            try
            {
                this.Total = total;
                //不需要分页
                if (this.PageSize == 0)
                {
                    this.TotalPage = this.Total > 0 ? 1 : 0;
                }
                else
                {
                    this.TotalPage = (this.Total + this.PageSize - 1) / this.PageSize;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "设置每页数量错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePageSize(int pageSize)
        {
            try
            {
                this.PageSize = pageSize;
                //不需要分页
                if (this.PageSize == 0)
                {
                    this.TotalPage = this.Total > 0 ? 1 : 0;
                }
                else
                {
                    this.TotalPage = (this.Total + this.PageSize - 1) / this.PageSize;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "设置每页数量错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToPage(int page)
        {
            if (page < 1 || (hasPaged && page > this.TotalPage))
            {
                return;
            }

            if (page == this.CurrentPage)
            {
                return;
            }

            try
            {
                this.OnPageChaning(new PageChangeEventArgs(page, this.PageSize, this.Parameters));
                this.CurrentPage = page;
                this.hasPaged = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void StartPage()
        {
            this.hasPaged = false;
            this.Total = -1;
            this.CurrentPage = 0;
            this.CurrentCount = 0;
            this.Dispatcher.BeginInvoke(new Action(() => this.GoToPage(1)));
        }
    }
}