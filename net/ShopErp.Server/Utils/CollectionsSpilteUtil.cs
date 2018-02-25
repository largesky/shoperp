using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Utils
{
    public class CollectionsSpilteUtil
    {
        public static T[][] Spilte<T>(T[] source, int targetGroupCount)
        {
            if (source == null)
            {
                throw new Exception("分割数组失败，source为NULL");
            }

            targetGroupCount = source.Length < targetGroupCount ? source.Length : targetGroupCount;
            int mode = source.Length % targetGroupCount;
            int div = source.Length / targetGroupCount;
            T[][] newArray = new T[targetGroupCount][];

            for (int i = 0; i < targetGroupCount; i++)
            {
                int len = div + (i < mode ? 1 : 0);
                int j = div * i + (i < mode ? i : mode);
                newArray[i] = new T[len];

                for (int k = 0; k < len; k++)
                {
                    newArray[i][k] = source[j + k];
                }
            }

            return newArray;
        }
    }
}
