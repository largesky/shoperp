using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public class TaobaoKeyword
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public string Words { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string[] WordsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Words))
                {
                    return new string[0];
                }
                return Words.Split(',');
            }
        }

        public override string ToString()
        {
            DateTime dt = new DateTime(2000, 01, 01);

            return string.Format("{0} {1} {2}", Number, Start > dt ? Start.ToString("MM-dd") : "", End > dt ? End.ToString("MM-dd") : "");
        }
    }
}
