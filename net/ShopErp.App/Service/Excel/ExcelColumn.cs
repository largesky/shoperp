using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Excel
{
    public class ExcelColumn
    {
        public string Name { get; set; }

        public bool IsNumber { get; set; }


        public ExcelColumn(string name, bool isNumber)
        {
            this.Name = name;
            this.IsNumber = isNumber;
        }
    }
}
