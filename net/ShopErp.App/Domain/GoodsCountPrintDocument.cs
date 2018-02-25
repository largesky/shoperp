using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Domain
{
    class GoodsCountPrintDocument : DocumentPaginator, IDocumentPaginatorSource
    {
        private const int LEFT_MARGIN = 20;
        private const int TOP_MARGIN = 30;
        private const int ITEM_MARGIN_HEIGHT = 1;
        private const int ITEM_MARGIN_WIDGHT = 2;

        public GoodsCount[] GoodsCount { get; private set; }
        private int pageCount = -1;
        private int rowPerPage = -1;
        private DateTime time;

        public override bool IsPageCountValid
        {
            get { return pageCount > -1; }
        }

        public override int PageCount
        {
            get { return this.pageCount; }
        }

        public override System.Windows.Size PageSize { get; set; }

        public override IDocumentPaginatorSource Source
        {
            get { return this; }
        }

        public DocumentPaginator DocumentPaginator
        {
            get { return this; }
        }

        private FormattedText CreateText(string text, int weightValue = 14, string fontName = "楷体")
        {
            if (text == null)
            {
                text = "";
            }
            var item = new FormattedText(text, Thread.CurrentThread.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface(fontName), weightValue, Brushes.Black);
            return item;
        }

        public void SetGoodsCount(GoodsCount[] goodsCount)
        {
            this.GoodsCount = goodsCount;
            FormattedText text = CreateText("中文", 16, "黑体");
            this.rowPerPage = (int) ((this.PageSize.Height - TOP_MARGIN * 2) / (text.Height + ITEM_MARGIN_HEIGHT * 2)) -
                              2;
            this.pageCount = (this.GoodsCount.Length + this.rowPerPage - 1) / this.rowPerPage;
            this.time = DateTime.Now;
        }

        public double GetPageMoney(int pageNumber)
        {
            double money = 0;
            for (int i = 0; i < this.rowPerPage && this.rowPerPage * pageNumber + i < this.GoodsCount.Length; i++)
            {
                GoodsCount gc = this.GoodsCount[this.rowPerPage * pageNumber + i];
                money += gc.Money * gc.Count;
            }
            return money;
        }

        public int GetPageCount(int pageNumber)
        {
            int count = 0;
            for (int i = 0; i < this.rowPerPage && this.rowPerPage * pageNumber + i < this.GoodsCount.Length; i++)
            {
                GoodsCount gc = this.GoodsCount[this.rowPerPage * pageNumber + i];
                count += gc.Count;
            }
            return count;
        }

        public double GetTotalMoney()
        {
            double money = 0;
            foreach (var item in this.GoodsCount)
            {
                money += item.Money * item.Count;
            }
            return money;
        }

        public double[] GetItemWidth()
        {
            double[] widths = new double[10];

            widths[0] = 90; //门牌编号
            widths[1] = 60; //最后日期
            widths[2] = 80; //厂家名称
            widths[3] = 75; //货号
            widths[4] = 75; //版本
            widths[5] = 50; //颜色
            widths[6] = 60; //尺码
            widths[7] = 45; //价格
            widths[8] = 45; //数量
            widths[9] = this.PageSize.Width - LEFT_MARGIN * 2 - widths.Sum();

            return widths;
        }

        public double GetPreWidth(double[] width, int index)
        {
            double preWidth = 0;
            for (int i = 0; i < index; i++)
            {
                preWidth += width[i];
            }
            return preWidth;
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            DrawingVisual visual = new DrawingVisual();
            var rendor = visual.RenderOpen();

            rendor.DrawRectangle(Brushes.White, null, new Rect(this.PageSize));

            //绘制标题
            double money = GetPageMoney(pageNumber);
            double totalMoney = GetTotalMoney();
            string strTitle = string.Format("页码: {0}/{1} 总数量:{3} 本页金额:{4:F1} 打印时间:{5},打印人员:{6}", pageNumber + 1,
                this.pageCount, totalMoney, this.GetPageCount(pageNumber), money,
                this.time.ToString("yyyy-MM-dd HH:mm:ss"), OperatorService.LoginOperator.Number);
            FormattedText textTitle = CreateText(strTitle, 16, "黑体");
            rendor.DrawText(textTitle,
                new System.Windows.Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT, TOP_MARGIN + ITEM_MARGIN_HEIGHT));

            double eachItemHeight = textTitle.Height + ITEM_MARGIN_HEIGHT * 2;
            double[] itemWidth = this.GetItemWidth();

            Pen linePen = new Pen(Brushes.Black, 2);
            int currentPageCount = (pageNumber + 1) == this.PageCount
                ? (this.GoodsCount.Length - this.rowPerPage * pageNumber)
                : this.rowPerPage;

            //绘制网格横线
            for (int i = 0; i <= currentPageCount + 1; i++)
            {
                double y = TOP_MARGIN + eachItemHeight + eachItemHeight * i - 1;
                rendor.DrawLine(linePen, new Point(LEFT_MARGIN, y), new Point(this.PageSize.Width - LEFT_MARGIN, y));
            }

            //绘制网格竖线
            double lineHeight = currentPageCount * eachItemHeight + eachItemHeight;
            for (int i = 0; i <= itemWidth.Length; i++)
            {
                double x = LEFT_MARGIN + GetPreWidth(itemWidth, i) - 1;
                rendor.DrawLine(linePen, new Point(x, TOP_MARGIN + eachItemHeight - 1),
                    new Point(x, TOP_MARGIN + eachItemHeight + lineHeight));
            }

            //绘制标题
            var title = this.CreateText("门牌编号", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 0),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("日期", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 1),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("厂家名称", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 2),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("厂家货号", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 3),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("版本", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 4),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("颜色", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 5),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("尺码", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 6),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("价格", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 7),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("数量", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 8),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            title = this.CreateText("备注", 16, "黑体");
            rendor.DrawText(title,
                new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 9),
                    TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight));

            //绘制数据
            for (int i = 0; i < this.rowPerPage && this.rowPerPage * pageNumber + i < this.GoodsCount.Length; i++)
            {
                GoodsCount gc = this.GoodsCount[this.rowPerPage * pageNumber + i];

                double currentY = TOP_MARGIN + ITEM_MARGIN_HEIGHT + eachItemHeight * 2 + i * eachItemHeight;

                //厂家门牌号
                var doorText = this.CreateText(gc.Address);
                rendor.DrawText(doorText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 0), currentY));

                //日期标记
                string mark = "";
                if (DateTime.Now.Subtract(gc.FirstPayTime).TotalHours >= 20)
                {
                    mark = gc.FirstPayTime.ToString("dd-HH") + " ";
                }
                if (gc.PopType == PopType.TMALL || gc.PopType == PopType.CHUCHUJIE)
                {
                    mark += gc.PopType == PopType.TMALL ? "T" : "C";
                }
                if (string.IsNullOrWhiteSpace(mark) == false)
                {
                    var markText = this.CreateText(mark);
                    rendor.DrawText(markText,
                        new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 1), currentY));
                }

                //厂家名称
                string vendor = gc.Vendor;
                if (vendor.Length > 4)
                {
                    vendor = vendor.Substring(0, 4);
                }
                var vendorText = this.CreateText(vendor);
                rendor.DrawText(vendorText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 2), currentY));

                //厂家货号
                var numberText = this.CreateText(gc.Number);
                rendor.DrawText(numberText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 3), currentY));

                //版本
                var edtioText = this.CreateText(gc.Edtion);
                rendor.DrawText(edtioText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 4), currentY));

                //颜色 
                var colorText = this.CreateText(gc.Color);
                rendor.DrawText(colorText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 5), currentY));

                //尺码
                var sizeText = this.CreateText(gc.Size);
                rendor.DrawText(sizeText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 6), currentY));

                //单价
                var priceText = this.CreateText(gc.Money.ToString("F0"));
                rendor.DrawText(priceText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 7), currentY));

                //数量
                var countText = this.CreateText(gc.Count.ToString());
                rendor.DrawText(countText,
                    new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 8), currentY));

                //快递打印在备注那里
                if (string.IsNullOrWhiteSpace(gc.Comment) == false)
                {
                    var dText = this.CreateText(gc.Comment);
                    rendor.DrawText(dText,
                        new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(itemWidth, 9), currentY));
                }
            }
            rendor.Close();
            return new DocumentPage(visual, this.PageSize, new System.Windows.Rect(this.PageSize),new System.Windows.Rect(this.PageSize));
        }
    }
}