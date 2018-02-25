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
using ShopErp.Domain;


namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// OrderEdit.xaml 的交互逻辑
    /// </summary>
    public partial class OrderCommentAndFlagEditWindow : Window
    {
        public ColorFlag Flag { get; set; }

        public string Comment { get; set; }

        public OrderCommentAndFlagEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.tbComment.Text = Comment;
            if (Flag == ColorFlag.BLUE)
            {
                this.rbBlue.IsChecked = true;
            }
            else if (Flag == ColorFlag.GREEN)
            {
                this.rbGreen.IsChecked = true;
            }
            else if (Flag == ColorFlag.PINK)
            {
                this.rbPink.IsChecked = true;
            }
            else if (Flag == ColorFlag.RED)
            {
                this.rbRed.IsChecked = true;
            }
            else if (Flag == ColorFlag.YELLOW)
            {
                this.rbYellow.IsChecked = true;
            }
            else
            {
                this.rbRed.IsChecked = true;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Flag = ColorFlag.UN_LABEL;
            if (this.rbBlue.IsChecked.Value)
            {
                Flag = ColorFlag.BLUE;
            }
            if (this.rbGreen.IsChecked.Value)
            {
                Flag = ColorFlag.GREEN;
            }
            if (this.rbPink.IsChecked.Value)
            {
                Flag = ColorFlag.PINK;
            }
            if (this.rbRed.IsChecked.Value)
            {
                Flag = ColorFlag.RED;
            }
            if (this.rbYellow.IsChecked.Value)
            {
                Flag = ColorFlag.YELLOW;
            }
            if (this.rbUnLable.IsChecked.Value)
            {
                Flag = ColorFlag.UN_LABEL;
            }
            this.Comment = this.tbComment.Text.Trim();
            this.DialogResult = true;
        }
    }
}