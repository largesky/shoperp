using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopErp.App.Utils;

namespace ShopErp.App.Views.Extenstions
{
    public static class ComboBoxExtension
    {
        public static void Bind<T>(this System.Windows.Controls.ComboBox source)
        {
            String[] descriptions = EnumUtil.GetEnumDescriptions(typeof(T));
            source.ItemsSource = descriptions;
            if (descriptions.Length > 0 && source.SelectedItem == null && source.SelectedIndex < 0)
                source.SelectedIndex = 0;
        }

        public static T GetSelectedEnum<T>(this System.Windows.Controls.ComboBox source)
        {
            if (source.SelectedIndex < 0)
            {
                throw new Exception("请选择值");
            }

            return (T)Enum.GetValues(typeof(T)).GetValue(source.SelectedIndex);
        }

        public static void SetSelectedEnum(this System.Windows.Controls.ComboBox source, Enum value)
        {
            var ss = source.ItemsSource;
            string des = EnumUtil.GetEnumValueDescription(value);
            int i = 0;
            foreach (var item in ss)
            {
                if (item.Equals(des))
                {
                    source.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }
    }
}