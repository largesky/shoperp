using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Restful
{
    class TaobaoKeywordDetailService : ServiceBase<TaobaoKeywordDetail>
    {
        public DataCollectionResponse<TaobaoKeywordDetail> GetByAll(string number, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();

            para["number"] = number;
            para["start"] = start;
            para["end"] = end;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<TaobaoKeywordDetail>>(para);
        }

        public void SaveMulti(TaobaoKeywordDetail[] values)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["values"] = values;
            DoPost<ResponseBase>(para);
        }

        public static bool Match(string[] words, string wordStr)
        {
            bool ret = words.All(obj => wordStr.IndexOf(obj, StringComparison.OrdinalIgnoreCase) >= 0);
            return ret;
        }

        public static bool UnMatch(string words, string wordStr)
        {
            if ((words == null || words.Length == 1) & string.IsNullOrWhiteSpace(wordStr) == false)
            {
                return true;
            }

            if (words == null || words.Length < 1)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(wordStr))
            {
                return false;
            }

            if ("鞋子女冬" == wordStr)
            {
                System.Console.WriteLine("ffff");
            }

            StringBuilder sb = new StringBuilder(wordStr);

            foreach (var word in words)
            {
                sb.Replace(word, ' ');
            }

            return string.IsNullOrWhiteSpace(sb.ToString()) == false;
        }

    }
}
