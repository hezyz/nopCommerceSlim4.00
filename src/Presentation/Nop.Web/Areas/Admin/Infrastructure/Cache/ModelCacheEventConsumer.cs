using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Web.Areas.Admin.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer: 
        //settings
        IConsumer<EntityUpdatedEvent<Setting>>,
        //categories
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityUpdatedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Category>>,
        //vendors
        IConsumer<EntityInsertedEvent<Vendor>>,
        IConsumer<EntityUpdatedEvent<Vendor>>,
        IConsumer<EntityDeletedEvent<Vendor>>
    {
        /// <summary>
        /// Key for nopCommerce.com news cache
        /// </summary>
        public const string OFFICIAL_NEWS_MODEL_KEY = "Nop.pres.admin.official.news";
        public const string OFFICIAL_NEWS_PATTERN_KEY = "Nop.pres.admin.official.news";
        
        /// <summary>
        /// Key for categories caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public const string CATEGORIES_LIST_KEY = "Nop.pres.admin.categories.list-{0}";
        public const string CATEGORIES_LIST_PATTERN_KEY = "Nop.pres.admin.categories.list";

        /// <summary>
        /// Key for vendors caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public const string VENDORS_LIST_KEY = "Nop.pres.admin.vendors.list-{0}";
        public const string VENDORS_LIST_PATTERN_KEY = "Nop.pres.admin.vendors.list";

        private readonly ICacheManager _cacheManager;
        
        public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            this._cacheManager = cacheManager;
        }

        public void HandleEvent(EntityUpdatedEvent<Setting> eventMessage)
        {
            //clear models which depend on settings
            _cacheManager.RemoveByPattern(OFFICIAL_NEWS_PATTERN_KEY); //depends on AdminAreaSettings.HideAdvertisementsOnAdminArea
        }
        

        //categories
        public void HandleEvent(EntityInsertedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORIES_LIST_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORIES_LIST_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORIES_LIST_PATTERN_KEY);
        }

        //vendors
        public void HandleEvent(EntityInsertedEvent<Vendor> eventMessage)
        {
            _cacheManager.RemoveByPattern(VENDORS_LIST_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Vendor> eventMessage)
        {
            _cacheManager.RemoveByPattern(VENDORS_LIST_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<Vendor> eventMessage)
        {
            _cacheManager.RemoveByPattern(VENDORS_LIST_PATTERN_KEY);
        }
    }
}