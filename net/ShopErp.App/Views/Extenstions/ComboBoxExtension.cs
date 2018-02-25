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
            String[] descriptions = EnumUtil.GetEnumDescriptions<T>();
            source.ItemsSource = descriptions;
            if (descriptions.Length > 0)
                source.SelectedIndex = 0;
        }

        public static T GetSelectedEnum<T>(this System.Windows.Controls.ComboBox source)
        {
            if (source.SelectedIndex < 0)
            {
                throw new Exception("请选择值");
            }

            return (T) Enum.GetValues(typeof(T)).GetValue(source.SelectedIndex);
        }

        public static void SetSelectedEnum(this System.Windows.Controls.ComboBox source, Enum value)
        {
            var s = source.ItemsSource;
            int i = 0;
            foreach (var item in s)
            {
                if (item.Equals(value))
                {
                    source.SelectedIndex = i;
                    return;
                }
                i++;
            }

            string des = EnumUtil.GetEnumValueDescription(value);
            i = 0;
            foreach (var item in s)
            {
                if (item.Equals(des))
                {
                    source.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }


        public static T GetSelectedEnum<T>(this System.Windows.Controls.ComboBox source, int selectedIndex = -1)
        {
            if (source.SelectedIndex < 0)
            {
                throw new Exception("请选择值");
            }
            return (T) Enum.GetValues(typeof(T)).GetValue(selectedIndex < 0 ? source.SelectedIndex : selectedIndex);
        }
    }
}