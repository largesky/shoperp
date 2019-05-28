using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Utils
{
    class EnumerableKeySelector
    {
        private List<PropertyInfo> propertyInfoPaths = new List<PropertyInfo>();

        public EnumerableKeySelector(Type classType, string propertyName)
        {
            string[] names = propertyName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            PropertyInfo pi = classType.GetProperty(names[0]);
            if (pi == null)
            {
                throw new Exception("属性不存在：" + propertyName);
            }
            propertyInfoPaths.Add(pi);
            for (int i = 1; i < names.Length; i++)
            {
                pi = pi.PropertyType.GetProperty(names[i]);
                if (pi == null)
                {
                    throw new Exception("属性不存在：" + propertyName);
                }
                propertyInfoPaths.Add(pi);
            }
        }

        public object GetData(object vm)
        {
            object value = vm;
            for (int i = 0; i < propertyInfoPaths.Count; i++)
            {
                value = propertyInfoPaths[i].GetValue(value);
            }
            return value;
        }
    }
}
