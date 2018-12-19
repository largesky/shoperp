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

            System.Drawing.Image i = null;
            using (var ms = new MemoryStream(template.AttachFiles[item.Format], false))
            {
                i = System.Drawing.Image.FromStream(ms);
            }
            return i;
        }
    }
}
