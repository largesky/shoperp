using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.Common
{
    public class KeyValuePairClass<T, K>
    {
        public KeyValuePairClass()
        {
        }

        public KeyValuePairClass(T key, K value)
        {
            this.Key = key;
            this.Value = value;
        }

        public T Key { get; set; }

        public K Value { get; set; }
    }
}
