using System;
using System.IO;
using System.Windows.Media.Imaging;
using ShopErp.App.Domain;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    class OtherImage : IOtherFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.OTHER_IMAGE; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item)
        {
            if (template.AttachFiles.ContainsKey(item.Format) == false)
            {
                throw new Exception("图片不存在");
            }

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(template.AttachFiles[item.Format], false);
            bi.EndInit();
            return bi;
        }
    }
}
