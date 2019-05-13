using System;
using System.IO;
using System.Windows.Media.Imaging;
using ShopErp.App.Domain;
using ShopErp.Domain;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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
            var af = template.AttachFiles.FirstOrDefault(obj => obj.Name == item.Format);
            if (af == null)
            {
                throw new Exception("图片不存在");
            }
            System.Drawing.Image i = null;
            using (var ms = new MemoryStream(af.Value, false))
            {
                i = System.Drawing.Image.FromStream(ms);
            }
            return i;
        }
    }
}
