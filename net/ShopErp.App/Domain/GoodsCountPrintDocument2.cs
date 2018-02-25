using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ShopErp.Domain;

namespace ShopErp.App.Domain
{
    class GoodsCountPrintDocument2 : DocumentPaginator, IDocumentPaginatorSource
    {
        private const int LEFT_MARGIN = 20;
        private const int TOP_MARGIN = 40;
        private const int ITEM_MARGIN_HEIGHT = 4;
        private const int ITEM_MARGIN_WIDGHT = 2;

        List<KeyValuePair<DocumentPage, List<GoodsCount>>> pagesList =
            new List<KeyValuePair<DocumentPage, List<GoodsCount>>>();

        public override bool IsPageCountValid
        {
            get { return true; }
        }

        public override int PageCount
        {
            get { return this.pagesList.Count; }
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

        private FormattedText CreateText(string text, int weightValue = 15, string fontName = "楷体")
        {
            if (text == null)
            {
                text = "";
            }
            var item = new FormattedText(text, Thread.CurrentThread.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface(fontName), weightValue, Brushes.Black);
            return item;
        }

        public void SetGoodsCount(Dictionary<string, List<GoodsCount>> goodsCount, string name, string phone)
        {
            DateTime startTime = DateTime.Now;
            double PRINTABLE_PAGE_HEIGHT = this.PageSize.Height - 2 * TOP_MARGIN;
            double EACH_LINE_HEIGHT = CreateText("中文", 16, "黑体").Height + ITEM_MARGIN_HEIGHT * 2;
            Pen BLACK_PEN = new Pen(Brushes.Black, 2);
            double[] ITEM_WIDTHS = GetItemWidth();
            DrawingVisual currentDrawingVisual = null;
            double currentY = TOP_MARGIN + EACH_LINE_HEIGHT;
            DrawingContext currentRendor = null;
            List<GoodsCount> currentPageCount = new List<GoodsCount>();
            var goodsCounts = goodsCount.ToList();
            //生成分页
            for (int n = 0; n < goodsCounts.Count; n++)
            {
                var goods = goodsCounts[n];
                //计算当前厂家打印所需高度
                double height = EACH_LINE_HEIGHT * (goods.Value.Count + 1 + 1); //数量加标题 加一行空白

                if (PRINTABLE_PAGE_HEIGHT - currentY <= EACH_LINE_HEIGHT * 2)
                {
                    if (currentDrawingVisual == null)
                    {
                        throw new InvalidProgramException("当前需要换页，但是没有绘制对象,编程错误");
                    }
                    currentRendor.Close();
                    var page = new DocumentPage(currentDrawingVisual, this.PageSize,
                        new System.Windows.Rect(this.PageSize), new System.Windows.Rect(this.PageSize));
                    this.pagesList.Add(new KeyValuePair<DocumentPage, List<GoodsCount>>(page, currentPageCount));
                    currentDrawingVisual = null;
                    currentRendor = null;
                    currentPageCount = new List<GoodsCount>();
                }

                //如果当前没有最后一页则生成一页
                if (currentDrawingVisual == null)
                {
                    currentDrawingVisual = new DrawingVisual();
                    currentRendor = currentDrawingVisual.RenderOpen();
                    currentRendor.DrawRectangle(Brushes.White, null, new Rect(this.PageSize));
                    currentY = TOP_MARGIN + EACH_LINE_HEIGHT;
                    //绘制边框竖线
                    //currentRendor.DrawLine(BLACK_PEN, new Point(LEFT_MARGIN, TOP_MARGIN + EACH_LINE_HEIGHT), new Point(LEFT_MARGIN, this.PageSize.Height - TOP_MARGIN));
                    //currentRendor.DrawLine(BLACK_PEN, new Point(this.PageSize.Width - LEFT_MARGIN, TOP_MARGIN + EACH_LINE_HEIGHT), new Point(this.PageSize.Width - LEFT_MARGIN, this.PageSize.Height - TOP_MARGIN));

                    //currentRendor.DrawLine(BLACK_PEN, new Point(LEFT_MARGIN, TOP_MARGIN), new Point(this.PageSize.Width - LEFT_MARGIN, TOP_MARGIN));
                    //currentRendor.DrawLine(BLACK_PEN, new Point(LEFT_MARGIN, this.PageSize.Height - TOP_MARGIN), new Point(this.PageSize.Width - LEFT_MARGIN, this.PageSize.Height - TOP_MARGIN));
                }

                //绘制厂家上面的横线
                currentRendor.DrawLine(BLACK_PEN, new Point(LEFT_MARGIN, currentY),
                    new Point(this.PageSize.Width - LEFT_MARGIN, currentY));
                //绘制厂家信息
                string vendorStr = goods.Key;
                if (vendorStr.Length > 4)
                {
                    vendorStr = vendorStr.Substring(0, 4);
                }
                string vendor = vendorStr + "【" + goods.Value[0].Address + "】" + startTime.ToString("MM月dd日");
                var vendorTxt = this.CreateText(vendor, 15, "黑体");
                currentRendor.DrawText(vendorTxt, new Point(LEFT_MARGIN + 40, currentY + ITEM_MARGIN_HEIGHT));

                //前面打印日期
                var dateTxt = this.CreateText("日期", 15, "黑体");
                currentRendor.DrawText(dateTxt, new Point(LEFT_MARGIN, currentY + ITEM_MARGIN_HEIGHT));

                //后面打印数量
                double countX = this.GetPreWidth(ITEM_WIDTHS, 5);
                var countTxt = this.CreateText("数量", 15, "黑体");
                currentRendor.DrawText(countTxt, new Point(LEFT_MARGIN + countX, currentY + ITEM_MARGIN_HEIGHT));

                //后面打印联系电话
                var phoneTxt = this.CreateText(string.Format("【{0}】联系电话：{1}", name, phone), 15, "黑体");
                currentRendor.DrawText(phoneTxt,
                    new Point(LEFT_MARGIN + GetPreWidth(ITEM_WIDTHS, 6) + 15, currentY + ITEM_MARGIN_HEIGHT));
                //绘制厂家下面的横线
                currentRendor.DrawLine(BLACK_PEN, new Point(LEFT_MARGIN, currentY + EACH_LINE_HEIGHT),
                    new Point(this.PageSize.Width - LEFT_MARGIN, currentY + EACH_LINE_HEIGHT));
                double vendorStartY = currentY; //打印厂家开始起的坐标，
                //绘制数据
                for (int i = 0; i < goods.Value.Count; i++)
                {
                    var gc = goods.Value[i];
                    if (currentY + EACH_LINE_HEIGHT > this.PageSize.Height - TOP_MARGIN)
                    {
                        //打印时，已超出当前页
                        KeyValuePair<string, List<GoodsCount>> spiltedGcs =
                            new KeyValuePair<string, List<GoodsCount>>(gc.Vendor, new List<GoodsCount>());
                        for (; i < goods.Value.Count; i++)
                        {
                            spiltedGcs.Value.Add(goods.Value[i]);
                        }
                        goodsCounts.Insert(n + 1, spiltedGcs);
                        //结束当前页
                        break;
                    }
                    currentY += EACH_LINE_HEIGHT; //纵向坐标增加一行
                    currentPageCount.Add(gc);
                    //日期标记
                    string mark = "";
                    if (DateTime.Now.Subtract(gc.FirstPayTime).TotalHours >= 20)
                    {
                        mark = gc.FirstPayTime.ToString("dd") + " ";
                    }
                    if (gc.PopType == PopType.TMALL || gc.PopType == PopType.CHUCHUJIE)
                    {
                        mark += gc.PopType == PopType.TMALL ? "T" : "C";
                    }
                    if (string.IsNullOrWhiteSpace(mark) == false)
                    {
                        var markText = this.CreateText(mark);
                        currentRendor.DrawText(markText,
                            new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(ITEM_WIDTHS, 0),
                                currentY + ITEM_MARGIN_HEIGHT));
                    }

                    //厂家货号
                    var numberText = this.CreateText(gc.Number);
                    currentRendor.DrawText(numberText,
                        new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(ITEM_WIDTHS, 1),
                            currentY + ITEM_MARGIN_HEIGHT));

                    //版本
                    var edtioText = this.CreateText(gc.Edtion);
                    currentRendor.DrawText(edtioText,
                        new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(ITEM_WIDTHS, 2),
                            currentY + ITEM_MARGIN_HEIGHT));

                    //颜色 
                    var colorText = this.CreateText(gc.Color);
                    currentRendor.DrawText(colorText,
                        new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(ITEM_WIDTHS, 3),
                            currentY + ITEM_MARGIN_HEIGHT));

                    //尺码
                    var sizeText = this.CreateText(gc.Size);
                    currentRendor.DrawText(sizeText,
                        new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(ITEM_WIDTHS, 4),
                            currentY + ITEM_MARGIN_HEIGHT));

                    //单价
                    var priceText = this.CreateText(gc.Money.ToString("F0"));
                    //currentRendor.DrawText(priceText, new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(ITEM_WIDTHS, 5), currentY + ITEM_MARGIN_HEIGHT));

                    //数量
                    var countText = this.CreateText(gc.Count.ToString());
                    currentRendor.DrawText(countText,
                        new Point(LEFT_MARGIN + ITEM_MARGIN_WIDGHT + this.GetPreWidth(ITEM_WIDTHS, 5),
                            currentY + ITEM_MARGIN_HEIGHT));

                    //绘制下面的横线
                    currentRendor.DrawLine(BLACK_PEN, new Point(LEFT_MARGIN, currentY + EACH_LINE_HEIGHT),
                        new Point(this.PageSize.Width - LEFT_MARGIN, currentY + EACH_LINE_HEIGHT));
                }
                //绘制竖线 
                for (int k = 0; k < ITEM_WIDTHS.Length; k++)
                {
                    double x = LEFT_MARGIN + GetPreWidth(ITEM_WIDTHS, k) - 1;
                    double y = (k == 0 || k == ITEM_WIDTHS.Length - 1) ? vendorStartY : vendorStartY + EACH_LINE_HEIGHT;
                    currentRendor.DrawLine(BLACK_PEN, new Point(x, y), new Point(x, currentY + EACH_LINE_HEIGHT));
                }
                //空一行
                currentY += EACH_LINE_HEIGHT * 2;
            }

            //处理最后一页
            if (currentDrawingVisual != null)
            {
                currentRendor.Close();
                var page = new DocumentPage(currentDrawingVisual, this.PageSize, new System.Windows.Rect(this.PageSize),
                    new System.Windows.Rect(this.PageSize));
                this.pagesList.Add(new KeyValuePair<DocumentPage, List<GoodsCount>>(page, currentPageCount));
            }
        }

        public double[] GetItemWidth()
        {
            double[] widths = new double[10];

            widths[0] = 45; //最后日期
            widths[1] = 75; //货号
            widths[2] = 75; //版本
            widths[3] = 60; //颜色
            widths[4] = 60; //尺码
            //widths[5] = 30;//价格
            widths[5] = 30; //数量
            widths[6] = this.PageSize.Width - LEFT_MARGIN * 2 - widths.Sum();

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
            if (this.pagesList.Count < pageNumber)
            {
                throw new Exception("要打印的页不存在");
            }
            return this.pagesList[pageNumber].Key;
        }
    }
}