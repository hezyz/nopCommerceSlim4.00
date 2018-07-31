using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Events;

namespace Nop.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //languages
        IConsumer<EntityInsertedEvent<Language>>,
        IConsumer<EntityUpdatedEvent<Language>>,
        IConsumer<EntityDeletedEvent<Language>>,
        //settings
        IConsumer<EntityUpdatedEvent<Setting>>,
        //vendors
        IConsumer<EntityInsertedEvent<Vendor>>,
        IConsumer<EntityUpdatedEvent<Vendor>>,
        IConsumer<EntityDeletedEvent<Vendor>>,
        //categories
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityUpdatedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Category>>,
        //product categories
        IConsumer<EntityInsertedEvent<ProductCategory>>,
        IConsumer<EntityUpdatedEvent<ProductCategory>>,
        IConsumer<EntityDeletedEvent<ProductCategory>>,
        //products
        IConsumer<EntityInsertedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Product>>,
        //related product
        IConsumer<EntityInsertedEvent<RelatedProduct>>,
        IConsumer<EntityUpdatedEvent<RelatedProduct>>,
        IConsumer<EntityDeletedEvent<RelatedProduct>>,
        //product tags
        IConsumer<EntityInsertedEvent<ProductTag>>,
        IConsumer<EntityUpdatedEvent<ProductTag>>,
        IConsumer<EntityDeletedEvent<ProductTag>>,
        //Topics
        IConsumer<EntityInsertedEvent<Topic>>,
        IConsumer<EntityUpdatedEvent<Topic>>,
        IConsumer<EntityDeletedEvent<Topic>>,
        //Picture
        IConsumer<EntityInsertedEvent<Picture>>,
        IConsumer<EntityUpdatedEvent<Picture>>,
        IConsumer<EntityDeletedEvent<Picture>>,
        //Product picture mapping
        IConsumer<EntityInsertedEvent<ProductPicture>>,
        IConsumer<EntityUpdatedEvent<ProductPicture>>,
        IConsumer<EntityDeletedEvent<ProductPicture>>,
        //Product review
        IConsumer<EntityDeletedEvent<ProductReview>>,
        //polls
        IConsumer<EntityInsertedEvent<Poll>>,
        IConsumer<EntityUpdatedEvent<Poll>>,
        IConsumer<EntityDeletedEvent<Poll>>,
        //blog posts
        IConsumer<EntityInsertedEvent<BlogPost>>,
        IConsumer<EntityUpdatedEvent<BlogPost>>,
        IConsumer<EntityDeletedEvent<BlogPost>>,
        //blog comments
        IConsumer<EntityDeletedEvent<BlogComment>>,
        //news items
        IConsumer<EntityInsertedEvent<NewsItem>>,
        IConsumer<EntityUpdatedEvent<NewsItem>>,
        IConsumer<EntityDeletedEvent<NewsItem>>,
        //news comments
        IConsumer<EntityDeletedEvent<NewsComment>>,
        //states/province
        IConsumer<EntityInsertedEvent<StateProvince>>,
        IConsumer<EntityUpdatedEvent<StateProvince>>,
        IConsumer<EntityDeletedEvent<StateProvince>>,
        //templates
        IConsumer<EntityInsertedEvent<CategoryTemplate>>,
        IConsumer<EntityUpdatedEvent<CategoryTemplate>>,
        IConsumer<EntityDeletedEvent<CategoryTemplate>>,
        IConsumer<EntityInsertedEvent<ProductTemplate>>,
        IConsumer<EntityUpdatedEvent<ProductTemplate>>,
        IConsumer<EntityDeletedEvent<ProductTemplate>>,
        IConsumer<EntityInsertedEvent<TopicTemplate>>,
        IConsumer<EntityUpdatedEvent<TopicTemplate>>,
        IConsumer<EntityDeletedEvent<TopicTemplate>>,
        //plugins
        IConsumer<PluginUpdatedEvent>
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public ModelCacheEventConsumer(CatalogSettings catalogSettings, IStaticCacheManager cacheManager)
        {
            this._cacheManager = cacheManager;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Cache keys 

        /// <summary>
        /// Key for categories on the search page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SEARCH_CATEGORIES_MODEL_KEY = "Nop.pres.search.categories-{0}-{1}-{2}";
        public const string SEARCH_CATEGORIES_PATTERN_KEY = "Nop.pres.search.categories";

        /// <summary>
        /// Key for VendorNavigationModel caching
        /// </summary>
        public const string VENDOR_NAVIGATION_MODEL_KEY = "Nop.pres.vendor.navigation";
        public const string VENDOR_NAVIGATION_PATTERN_KEY = "Nop.pres.vendor.navigation";

        /// <summary>
        /// Key for list of CategorySimpleModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_ALL_MODEL_KEY = "Nop.pres.category.all-{0}-{1}-{2}";
        public const string CATEGORY_ALL_PATTERN_KEY = "Nop.pres.category.all";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : comma separated list of customer roles
        /// {1} : current store ID
        /// {2} : category ID
        /// </remarks>
        public const string CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY = "Nop.pres.category.numberofproducts-{0}-{1}-{2}";
        public const string CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY = "Nop.pres.category.numberofproducts";

        /// <summary>
        /// Key for caching of a value indicating whether a category has featured products
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_HAS_FEATURED_PRODUCTS_KEY = "Nop.pres.category.hasfeaturedproducts-{0}-{1}-{2}";
        public const string CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "Nop.pres.category.hasfeaturedproducts";
        public const string CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY_BY_ID = "Nop.pres.category.hasfeaturedproducts-{0}-";

        /// <summary>
        /// Key for caching of category breadcrumb
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// </remarks>
        public const string CATEGORY_BREADCRUMB_KEY = "Nop.pres.category.breadcrumb-{0}-{1}-{2}-{3}";
        public const string CATEGORY_BREADCRUMB_PATTERN_KEY = "Nop.pres.category.breadcrumb";

        /// <summary>
        /// Key for caching of subcategories of certain category
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// {4} : is connection SSL secured (included in a category picture URL)
        /// </remarks>
        public const string CATEGORY_SUBCATEGORIES_KEY = "Nop.pres.category.subcategories-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string CATEGORY_SUBCATEGORIES_PATTERN_KEY = "Nop.pres.category.subcategories";

        /// <summary>
        /// Key for caching of categories displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : roles of the current user
        /// {1} : current store ID
        /// {2} : language ID
        /// {3} : is connection SSL secured (included in a category picture URL)
        /// </remarks>
        public const string CATEGORY_HOMEPAGE_KEY = "Nop.pres.category.homepage-{0}-{1}-{2}-{3}-{4}";
        public const string CATEGORY_HOMEPAGE_PATTERN_KEY = "Nop.pres.category.homepage";

        /// <summary>
        /// Key for ProductBreadcrumbModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// {2} : comma separated list of customer roles
        /// {3} : current store ID
        /// </remarks>
        public const string PRODUCT_BREADCRUMB_MODEL_KEY = "Nop.pres.product.breadcrumb-{0}-{1}-{2}-{3}";
        public const string PRODUCT_BREADCRUMB_PATTERN_KEY = "Nop.pres.product.breadcrumb";
        public const string PRODUCT_BREADCRUMB_PATTERN_KEY_BY_ID = "Nop.pres.product.breadcrumb-{0}-";

        /// <summary>
        /// Key for ProductTagModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// {2} : current store ID
        /// </remarks>
        public const string PRODUCTTAG_BY_PRODUCT_MODEL_KEY = "Nop.pres.producttag.byproduct-{0}-{1}-{2}";
        public const string PRODUCTTAG_BY_PRODUCT_PATTERN_KEY = "Nop.pres.producttag.byproduct";

        /// <summary>
        /// Key for PopularProductTagsModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTTAG_POPULAR_MODEL_KEY = "Nop.pres.producttag.popular-{0}-{1}";
        public const string PRODUCTTAG_POPULAR_PATTERN_KEY = "Nop.pres.producttag.popular";

        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic system name
        /// {1} : language id
        /// {2} : store id
        /// {3} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_MODEL_BY_SYSTEMNAME_KEY = "Nop.pres.topic.details.bysystemname-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic id
        /// {1} : language id
        /// {2} : store id
        /// {3} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_MODEL_BY_ID_KEY = "Nop.pres.topic.details.byid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic system name
        /// {1} : language id
        /// {2} : store id
        /// {3} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_SENAME_BY_SYSTEMNAME = "Nop.pres.topic.sename.bysystemname-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic system name
        /// {1} : language id
        /// {2} : store id
        /// {3} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_TITLE_BY_SYSTEMNAME = "Nop.pres.topic.title.bysystemname-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// {2} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_TOP_MENU_MODEL_KEY = "Nop.pres.topic.topmenu-{0}-{1}-{2}";
        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// {2} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_FOOTER_MODEL_KEY = "Nop.pres.topic.footer-{0}-{1}-{2}";
        public const string TOPIC_PATTERN_KEY = "Nop.pres.topic";

        /// <summary>
        /// Key for CategoryTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : category template id
        /// </remarks>
        public const string CATEGORY_TEMPLATE_MODEL_KEY = "Nop.pres.categorytemplate-{0}";
        public const string CATEGORY_TEMPLATE_PATTERN_KEY = "Nop.pres.categorytemplate";

        /// <summary>
        /// Key for ProductTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : product template id
        /// </remarks>
        public const string PRODUCT_TEMPLATE_MODEL_KEY = "Nop.pres.producttemplate-{0}";
        public const string PRODUCT_TEMPLATE_PATTERN_KEY = "Nop.pres.producttemplate";

        /// <summary>
        /// Key for TopicTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : topic template id
        /// </remarks>
        public const string TOPIC_TEMPLATE_MODEL_KEY = "Nop.pres.topictemplate-{0}";
        public const string TOPIC_TEMPLATE_PATTERN_KEY = "Nop.pres.topictemplate";

        /// <summary>
        /// Key for "related" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTS_RELATED_IDS_KEY = "Nop.pres.related-{0}-{1}";
        public const string PRODUCTS_RELATED_IDS_PATTERN_KEY = "Nop.pres.related";

        /// <summary>
        /// Key for default product picture caching (all pictures)
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : picture size
        /// {2} : isAssociatedProduct?
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string PRODUCT_DEFAULTPICTURE_MODEL_KEY = "Nop.pres.product.detailspictures-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string PRODUCT_DEFAULTPICTURE_PATTERN_KEY = "Nop.pres.product.detailspictures";
        public const string PRODUCT_DEFAULTPICTURE_PATTERN_KEY_BY_ID = "Nop.pres.product.detailspictures-{0}-";

        /// <summary>
        /// Key for product picture caching on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string PRODUCT_DETAILS_PICTURES_MODEL_KEY = "Nop.pres.product.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string PRODUCT_DETAILS_PICTURES_PATTERN_KEY = "Nop.pres.product.picture";
        public const string PRODUCT_DETAILS_PICTURES_PATTERN_KEY_BY_ID = "Nop.pres.product.picture-{0}-";

        /// <summary>
        /// Key for product reviews caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCT_REVIEWS_MODEL_KEY = "Nop.pres.product.reviews-{0}-{1}";
        public const string PRODUCT_REVIEWS_PATTERN_KEY = "Nop.pres.product.reviews";
        public const string PRODUCT_REVIEWS_PATTERN_KEY_BY_ID = "Nop.pres.product.reviews-{0}-";

        /// <summary>
        /// Key for category picture caching
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string CATEGORY_PICTURE_MODEL_KEY = "Nop.pres.category.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string CATEGORY_PICTURE_PATTERN_KEY = "Nop.pres.category.picture";
        public const string CATEGORY_PICTURE_PATTERN_KEY_BY_ID = "Nop.pres.category.picture-{0}-";

        /// <summary>
        /// Key for vendor picture caching
        /// </summary>
        /// <remarks>
        /// {0} : vendor id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string VENDOR_PICTURE_MODEL_KEY = "Nop.pres.vendor.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string VENDOR_PICTURE_PATTERN_KEY = "Nop.pres.vendor.picture";
        public const string VENDOR_PICTURE_PATTERN_KEY_BY_ID = "Nop.pres.vendor.picture-{0}-";

        /// <summary>
        /// Key for home page polls
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string HOMEPAGE_POLLS_MODEL_KEY = "Nop.pres.poll.homepage-{0}-{1}";
        /// <summary>
        /// Key for polls by system name
        /// </summary>
        /// <remarks>
        /// {0} : poll system name
        /// {1} : language ID
        /// {2} : current store ID
        /// </remarks>
        public const string POLL_BY_SYSTEMNAME_MODEL_KEY = "Nop.pres.poll.systemname-{0}-{1}-{2}";
        public const string POLLS_PATTERN_KEY = "Nop.pres.poll";

        /// <summary>
        /// Key for blog tag list model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string BLOG_TAGS_MODEL_KEY = "Nop.pres.blog.tags-{0}-{1}";
        /// <summary>
        /// Key for blog archive (years, months) block model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string BLOG_MONTHS_MODEL_KEY = "Nop.pres.blog.months-{0}-{1}";
        public const string BLOG_PATTERN_KEY = "Nop.pres.blog";
        /// <summary>
        /// Key for number of blog comments
        /// </summary>
        /// <remarks>
        /// {0} : blog post ID
        /// {1} : store ID
        /// {2} : are only approved comments?
        /// </remarks>
        public const string BLOG_COMMENTS_NUMBER_KEY = "Nop.pres.blog.comments.number-{0}-{1}-{2}";
        public const string BLOG_COMMENTS_PATTERN_KEY = "Nop.pres.blog.comments";

        /// <summary>
        /// Key for home page news
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string HOMEPAGE_NEWSMODEL_KEY = "Nop.pres.news.homepage-{0}-{1}";
        public const string NEWS_PATTERN_KEY = "Nop.pres.news";
        /// <summary>
        /// Key for number of news comments
        /// </summary>
        /// <remarks>
        /// {0} : news item ID
        /// {1} : store ID
        /// {2} : are only approved comments?
        /// </remarks>
        public const string NEWS_COMMENTS_NUMBER_KEY = "Nop.pres.news.comments.number-{0}-{1}-{2}";
        public const string NEWS_COMMENTS_PATTERN_KEY = "Nop.pres.news.comments";

        /// <summary>
        /// Key for states by country id
        /// </summary>
        /// <remarks>
        /// {0} : country ID
        /// {1} : "empty" or "select" item
        /// {2} : language ID
        /// </remarks>
        public const string STATEPROVINCES_BY_COUNTRY_MODEL_KEY = "Nop.pres.stateprovinces.bycountry-{0}-{1}-{2}";
        public const string STATEPROVINCES_PATTERN_KEY = "Nop.pres.stateprovinces";

       /// <summary>
        /// Key for logo
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : current theme
        /// {2} : is connection SSL secured (included in a picture URL)
        /// </remarks>
        public const string STORE_LOGO_PATH = "Nop.pres.logo-{0}-{1}-{2}";
        public const string STORE_LOGO_PATH_PATTERN_KEY = "Nop.pres.logo";

        /// <summary>
        /// Key for available languages
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// </remarks>
        public const string AVAILABLE_LANGUAGES_MODEL_KEY = "Nop.pres.languages.all-{0}";
        public const string AVAILABLE_LANGUAGES_PATTERN_KEY = "Nop.pres.languages";

        /// <summary>
        /// Key for sitemap on the sitemap page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SITEMAP_PAGE_MODEL_KEY = "Nop.pres.sitemap.page-{0}-{1}-{2}";
        /// <summary>
        /// Key for sitemap on the sitemap SEO page
        /// </summary>
        /// <remarks>
        /// {0} : sitemap identifier
        /// {1} : language id
        /// {2} : roles of the current user
        /// {3} : current store ID
        /// </remarks>
        public const string SITEMAP_SEO_MODEL_KEY = "Nop.pres.sitemap.seo-{0}-{1}-{2}-{3}";
        public const string SITEMAP_PATTERN_KEY = "Nop.pres.sitemap";

        /// <summary>
        /// Key for widget info
        /// </summary>
        /// <remarks>
        /// {0} : current customer ID
        /// {1} : current store ID
        /// {2} : widget zone
        /// {3} : current theme name
        /// </remarks>
        public const string WIDGET_MODEL_KEY = "Nop.pres.widget-{0}-{1}-{2}-{3}";
        public const string WIDGET_PATTERN_KEY = "Nop.pres.widget";

        #endregion

        #region Methods

        //languages
        public void HandleEvent(EntityInsertedEvent<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
        }

       
        public void HandleEvent(EntityUpdatedEvent<Setting> eventMessage)
        {
            //clear models which depend on settings
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY); //depends on CatalogSettings.NumberOfProductTags
            _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY); //depends on VendorSettings.VendorBlockItemsToDisplay
            _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY); //depends on CatalogSettings.ShowCategoryProductNumber and CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY); //depends on CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY); //depends on BlogSettings.NumberOfTags
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY); //depends on NewsSettings.MainPageNewsCount
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY); //depends on distinct sitemap settings
            _cacheManager.RemoveByPattern(WIDGET_PATTERN_KEY); //depends on WidgetSettings and certain settings of widgets
            _cacheManager.RemoveByPattern(STORE_LOGO_PATH_PATTERN_KEY); //depends on StoreInformationSettings.LogoPictureId
        }

        //vendors
        public void HandleEvent(EntityInsertedEvent<Vendor> eventMessage)
        {
            _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Vendor> eventMessage)
        {
            _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(VENDOR_PICTURE_PATTERN_KEY_BY_ID, eventMessage.Entity.Id));
        }
        public void HandleEvent(EntityDeletedEvent<Vendor> eventMessage)
        {
            _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY);
        }

        //categories
        public void HandleEvent(EntityInsertedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(CATEGORY_PICTURE_PATTERN_KEY_BY_ID, eventMessage.Entity.Id));
        }
        public void HandleEvent(EntityDeletedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_BREADCRUMB_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product categories
        public void HandleEvent(EntityInsertedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_BREADCRUMB_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
            if (_catalogSettings.ShowCategoryProductNumber)
            {
                //depends on CatalogSettings.ShowCategoryProductNumber (when enabled)
                //so there's no need to clear this cache in other cases
                _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            }
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY_BY_ID, eventMessage.Entity.CategoryId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_BREADCRUMB_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY_BY_ID, eventMessage.Entity.CategoryId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_BREADCRUMB_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
            if (_catalogSettings.ShowCategoryProductNumber)
            {
                //depends on CatalogSettings.ShowCategoryProductNumber (when enabled)
                //so there's no need to clear this cache in other cases
                _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            }
            _cacheManager.RemoveByPattern(CATEGORY_NUMBER_OF_PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY_BY_ID, eventMessage.Entity.CategoryId));
        }

        //products
        public void HandleEvent(EntityInsertedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_REVIEWS_PATTERN_KEY_BY_ID, eventMessage.Entity.Id));
            _cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product tags
        public void HandleEvent(EntityInsertedEvent<ProductTag> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<ProductTag> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<ProductTag> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }

        //related products
        public void HandleEvent(EntityInsertedEvent<RelatedProduct> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<RelatedProduct> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<RelatedProduct> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }

        //Topics
        public void HandleEvent(EntityInsertedEvent<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //Pictures
        public void HandleEvent(EntityInsertedEvent<Picture> eventMessage)
        {

        }
        public void HandleEvent(EntityUpdatedEvent<Picture> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<Picture> eventMessage)
        {

        }

        //Product picture mappings
        public void HandleEvent(EntityInsertedEvent<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_DEFAULTPICTURE_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_DETAILS_PICTURES_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_DEFAULTPICTURE_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_DETAILS_PICTURES_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_DEFAULTPICTURE_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_DETAILS_PICTURES_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
        }

        //Polls
        public void HandleEvent(EntityInsertedEvent<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }

        //Blog posts
        public void HandleEvent(EntityInsertedEvent<BlogPost> eventMessage)
        {
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<BlogPost> eventMessage)
        {
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<BlogPost> eventMessage)
        {
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }

        //Blog comments
        public void HandleEvent(EntityDeletedEvent<BlogComment> eventMessage)
        {
            _cacheManager.RemoveByPattern(BLOG_COMMENTS_PATTERN_KEY);
        }

        //News items
        public void HandleEvent(EntityInsertedEvent<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        //News comments
        public void HandleEvent(EntityDeletedEvent<NewsComment> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_COMMENTS_PATTERN_KEY);
        }

        //State/province
        public void HandleEvent(EntityInsertedEvent<StateProvince> eventMessage)
        {
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<StateProvince> eventMessage)
        {
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<StateProvince> eventMessage)
        {
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }

        //templates
        public void HandleEvent(EntityInsertedEvent<CategoryTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<CategoryTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<CategoryTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityInsertedEvent<ProductTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<ProductTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<ProductTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityInsertedEvent<TopicTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdatedEvent<TopicTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeletedEvent<TopicTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_TEMPLATE_PATTERN_KEY);
        }

        //product reviews
        public void HandleEvent(EntityDeletedEvent<ProductReview> eventMessage)
        {
            _cacheManager.RemoveByPattern(string.Format(PRODUCT_REVIEWS_PATTERN_KEY_BY_ID, eventMessage.Entity.ProductId));
        }

        /// <summary>
        /// Handle plugin updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(PluginUpdatedEvent eventMessage)
        {
            if (eventMessage.Plugin?.Instance() is IWidgetPlugin)
                _cacheManager.RemoveByPattern(WIDGET_PATTERN_KEY);
        }

        #endregion
    }
}