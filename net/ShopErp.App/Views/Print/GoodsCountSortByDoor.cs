 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopErp.Domain;

namespace ShopErp.App.Views.Print
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

            if (lhs.Area != rhs.Area)
            {
                return lhs.Area > rhs.Area ? 1 : -1;
            }

            if (lhs.LianLang != rhs.LianLang)
            {
                return lhs.LianLang ? -1 : 1;
            }

            if (lhs.Door != rhs.Door)
            {
                return lhs.Door > rhs.Door ? 1 : -1;
            }

            if (lhs.Street != rhs.Street)
            {
                return lhs.Street > rhs.Street ? 1 : -1;
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