using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductOverviewModel : BaseNopEntityModel
    {
        public ProductOverviewModel()
        {
            DefaultPictureModel = new PictureModel();
            ReviewOverviewModel = new ProductReviewOverviewModel();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }

        public ProductType ProductType { get; set; }

        public bool MarkAsNew { get; set; }

        //picture
        public PictureModel DefaultPictureModel { get; set; }
        //review
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }
    }
}