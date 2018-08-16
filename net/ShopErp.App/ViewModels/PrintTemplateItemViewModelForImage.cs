using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.App.Views.Print;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ShopErp.App.ViewModels
{
    public class PrintTemplateItemViewModelForImage : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForImage(Service.Print.PrintTemplate template)
            : base(template)
        {
            this.PropertyUI = new PrintTemplateItemImageUserControl();
            this.PropertyUI.DataContext = this;
            this.PreviewValue = new Image();
        }

        protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PrintTemplateItemViewModelCommon.FormatProperty &&
                string.IsNullOrWhiteSpace(e.NewValue.ToString()) == false)
            {
                if (this.PreviewValue is Image == false)
                {
                    this.PreviewValue = new Image();
                }
                var image = this.PreviewValue as Image;
                //检查文件名称是否是GUID
                try
                {
                    if (this.Template.AttachFiles.ContainsKey(e.NewValue.ToString()))
                    {
                        var bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = new MemoryStream(this.Template.AttachFiles[e.NewValue.ToString()], false);
                        bi.EndInit();
                        image.Source = bi;
                        return;
                    }
                }
                catch
                {
                }

                //选择的是文件
                string file = e.NewValue.ToString();
                if (File.Exists(file) == false)
                {
                    throw new Exception("图片不存在");
                }
                FileInfo fi = new FileInfo(file);
                if (this.Template.AttachFiles == null)
                {
                    this.Template.AttachFiles = new Dictionary<string, byte[]>();
                }
                this.Template.AttachFiles[this.Data.Id.ToString()] = File.ReadAllBytes(file);
                this.Format = this.Data.Id.ToString(); //设置format属性，将再次引发该事件，从而刷新图片
                this.Data.Format = this.Data.Id.ToString();
                return;
            }
            base.OnPropertyChanged(e);
        }
    }
}