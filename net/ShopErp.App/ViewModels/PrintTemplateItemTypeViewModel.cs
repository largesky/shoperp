using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemTypeViewModel
    {
        public String Type { get; protected set; }

        public static PrintTemplateItemTypeViewModel[] GetAllTypes()
        {
            var type = typeof(Service.Print.PrintTemplateItemType);
            var ff = type.GetFields().Where(f => f.FieldType == typeof(string)).ToArray();
            var vms = ff.Select(obj => new PrintTemplateItemTypeViewModel { Type = obj.GetRawConstantValue().ToString() }).ToArray();
            return vms;
        }
    }
}