using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a bulk edit product model
    /// </summary>
    public partial class BulkEditProductModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Published")]
        public bool Published { get; set; }

        #endregion
    }
}