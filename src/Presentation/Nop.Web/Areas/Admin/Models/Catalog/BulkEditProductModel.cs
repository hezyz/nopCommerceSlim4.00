using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class BulkEditProductModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Published")]
        public bool Published { get; set; }
    }
}