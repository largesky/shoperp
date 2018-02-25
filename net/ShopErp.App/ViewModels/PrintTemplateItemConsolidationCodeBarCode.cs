using ShopErp.App.Domain;
using ShopErp.App.Views.Print;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemConsolidationCodeBarCode : PrintTemplateItemViewModelForBarcode
    {
        public PrintTemplateItemConsolidationCodeBarCode(PrintTemplate template) :
            base(template)
        {
            this.PropertyUI = new PrintTemplateItemConsolidationCodeBarCodeUserControl();
            this.Value = "021D-123-789";
            this.Value1 = "否";
        }
    }
}