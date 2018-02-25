using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse
{
    public class DataCollectionResponse<T> : ResponseBase
    {
        public int Total;

        public List<T> Datas = new List<T>();

        public T First { get { return this.Datas == null || this.Datas.Count < 1 ? default(T) : this.Datas[0]; } }

        public DataCollectionResponse()
        {
        }

        /// <summary>
        /// 参数可为空
        /// </summary>
        /// <param name="t"></param>
        public DataCollectionResponse(T t)
        {
            if (t == null)
            {
                this.Total = 0;
            }
            else
            {
                this.Total = 1;
                this.Datas.Add(t);
            }
        }

        public DataCollectionResponse(IEnumerable<T> source)
        {
            this.Total = source.Count();
            Datas.AddRange(source);
        }

        public DataCollectionResponse(IEnumerable<T> source, int total)
        {
            this.Total = total;
            Datas.AddRange(source);
        }
    }
}
