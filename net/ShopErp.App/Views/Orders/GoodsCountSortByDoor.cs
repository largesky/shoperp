
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopErp.Domain;

namespace ShopErp.App.Views.Orders
{
    public class GoodsCountSortByDoor : IComparer<GoodsCount>
    {
        public int Compare(GoodsCount lhs, GoodsCount rhs)
        {
            if (lhs == null && rhs == null)
            {
                return 0;
            }

            if (lhs == null)
            {
                return -1;
            }

            if (rhs == null)
            {
                return 1;
            }

            if (lhs.Address.Equals(rhs.Address, StringComparison.OrdinalIgnoreCase)==false)
            {
                return lhs.Address.CompareTo(rhs.Address);
            }

            if (lhs.Number.Equals(rhs.Number, StringComparison.OrdinalIgnoreCase) == false)
            {
                return lhs.Number.CompareTo(rhs.Number);
            }

            if (lhs.Edtion.Equals(rhs.Edtion, StringComparison.OrdinalIgnoreCase) == false)
            {
                return lhs.Edtion.CompareTo(rhs.Edtion);
            }

            if (lhs.Color.Equals(rhs.Color, StringComparison.OrdinalIgnoreCase) == false)
            {
                return lhs.Color.CompareTo(rhs.Color);
            }

            return lhs.Size.CompareTo(rhs.Size);
        }
    }
}