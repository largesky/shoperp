using System;
using System.Windows;

namespace ShopErp.App.Service.Print
{
    [Serializable]
    public class PrintTemplateItem
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 该项编号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// x坐标
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// y坐标
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// 字体大小
        /// </summary>
        public double FontSize { get; set; }

        /// <summary>
        /// 文字对齐
        /// </summary>
        public TextAlignment TextAlignment { get; set; }


        /// <summary>
        /// 文本缩放格式
        /// 无，水平，垂直，水平与垂直
        /// </summary>
        public string ScaleFormat { get; set; }

        /// <summary>
        /// 格式化数据的格式
        /// </summary>
        public string Format { get; set; }


        /// <summary>
        /// 透明程度
        /// </summary>
        public double Opacity { get; set; }

        /// <summary>
        /// 该项的值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 该项的值1
        /// </summary>
        public string Value1 { get; set; }



        /// <summary>
        /// 运行时的储存数据对象，不会序列化
        /// </summary>
        [NonSerialized]
        public object RunTimeTag = null;


        public PrintTemplateItem()
        {
            this.Opacity = 1;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PrintTemplateItem;
            if (other == null)
            {
                return false;
            }

            return this.Id == other.Id && this.Type == other.Type;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
