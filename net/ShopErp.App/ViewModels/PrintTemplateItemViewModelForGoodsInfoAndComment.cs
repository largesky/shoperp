using ShopErp.App.Views.Print;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.ViewModels
{
    public class PrintTemplateItemViewModelForGoodsInfoAndComment : PrintTemplateItemViewModelCommon
    {
        public PrintTemplateItemViewModelForGoodsInfoAndComment(PrintTemplate template) : base(template)
        {
            this.PropertyUI = new PrintTemplateItemViewModelGoodsInfoAndCommentUserControl();
            this.PropertyUI.DataContext = this;
        }
    }
}
