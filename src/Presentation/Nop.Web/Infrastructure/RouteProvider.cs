using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Web.Infrastructure
{
    /// <summary>
    /// Represents provider that provided basic routes
    /// </summary>
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //reorder routes so the most used ones are on top. It can improve performance

            //areas
            routeBuilder.MapRoute(name: "areaRoute", template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            //home page
            routeBuilder.MapLocalizedRoute("HomePage", "",
				new { controller = "Home", action = "Index" });

            //login
            routeBuilder.MapLocalizedRoute("Login", "login/",
				new { controller = "Customer", action = "Login" });

            //register
            routeBuilder.MapLocalizedRoute("Register", "register/",
				new { controller = "Customer", action = "Register" });

            //logout
            routeBuilder.MapLocalizedRoute("Logout", "logout/",
				new { controller = "Customer", action = "Logout" });

            //customer account links
            routeBuilder.MapLocalizedRoute("CustomerInfo", "customer/info",
				new { controller = "Customer", action = "Info" });

            routeBuilder.MapLocalizedRoute("CustomerAddresses", "customer/addresses",
				new { controller = "Customer", action = "Addresses" });

            //contact us
            routeBuilder.MapLocalizedRoute("ContactUs", "contactus",
				new { controller = "Common", action = "ContactUs" });

            //sitemap
            routeBuilder.MapLocalizedRoute("Sitemap", "sitemap",
				new { controller = "Common", action = "Sitemap" });

            //product search
            routeBuilder.MapLocalizedRoute("ProductSearch", "search/",
				new { controller = "Catalog", action = "Search" });

            routeBuilder.MapLocalizedRoute("ProductSearchAutoComplete", "catalog/searchtermautocomplete",
				new { controller = "Catalog", action = "SearchTermAutoComplete" });

            //change language (AJAX link)
            routeBuilder.MapLocalizedRoute("ChangeLanguage", "changelanguage/{langid:min(0)}",
				new { controller = "Common", action = "SetLanguage" });

            //recently viewed products
            routeBuilder.MapLocalizedRoute("RecentlyViewedProducts", "recentlyviewedproducts/",
				new { controller = "Product", action = "RecentlyViewedProducts" });

            //new products
            routeBuilder.MapLocalizedRoute("NewProducts", "newproducts/",
				new { controller = "Product", action = "NewProducts" });

            //blog
            routeBuilder.MapLocalizedRoute("Blog", "blog",
				new { controller = "Blog", action = "List" });

            //news
            routeBuilder.MapLocalizedRoute("NewsArchive", "news",
				new { controller = "News", action = "List" });

            //forum
            routeBuilder.MapLocalizedRoute("Boards", "boards",
				new { controller = "Boards", action = "Index" });

            //product tags
            routeBuilder.MapLocalizedRoute("ProductTagsAll", "producttag/all/",
				new { controller = "Catalog", action = "ProductTagsAll" });

            //product tags
            routeBuilder.MapLocalizedRoute("ProductsByTag", "producttag/{productTagId:min(0)}/{SeName?}",
				new { controller = "Catalog", action = "ProductsByTag" });

            //product email a friend
            routeBuilder.MapLocalizedRoute("ProductEmailAFriend", "productemailafriend/{productId:min(0)}",
				new { controller = "Product", action = "ProductEmailAFriend" });

            //reviews
            routeBuilder.MapLocalizedRoute("ProductReviews", "productreviews/{productId}",
				new { controller = "Product", action = "ProductReviews" });

            routeBuilder.MapLocalizedRoute("CustomerProductReviews", "customer/productreviews",
				new { controller = "Product", action = "CustomerProductReviews" });

            routeBuilder.MapLocalizedRoute("CustomerProductReviewsPaged", "customer/productreviews/page/{pageNumber:min(0)}",
				new { controller = "Product", action = "CustomerProductReviews" });

            //subscribe newsletters
            routeBuilder.MapLocalizedRoute("SubscribeNewsletter", "subscribenewsletter",
				new { controller = "Newsletter", action = "SubscribeNewsletter" });


            //register result page
            routeBuilder.MapLocalizedRoute("RegisterResult", "registerresult/{resultId:min(0)}",
				new { controller = "Customer", action = "RegisterResult" });

            //check username availability
            routeBuilder.MapLocalizedRoute("CheckUsernameAvailability", "customer/checkusernameavailability",
				new { controller = "Customer", action = "CheckUsernameAvailability" });

            //passwordrecovery
            routeBuilder.MapLocalizedRoute("PasswordRecovery", "passwordrecovery",
				new { controller = "Customer", action = "PasswordRecovery" });

            //password recovery confirmation
            routeBuilder.MapLocalizedRoute("PasswordRecoveryConfirm", "passwordrecovery/confirm",
				new { controller = "Customer", action = "PasswordRecoveryConfirm" });

            //topics
            routeBuilder.MapLocalizedRoute("TopicPopup", "t-popup/{SystemName}",
				new { controller = "Topic", action = "TopicDetailsPopup" });
            
            //blog
            routeBuilder.MapLocalizedRoute("BlogByTag", "blog/tag/{tag}",
				new { controller = "Blog", action = "BlogByTag" });

            routeBuilder.MapLocalizedRoute("BlogByMonth", "blog/month/{month}",
				new { controller = "Blog", action = "BlogByMonth" });

            //blog RSS
            routeBuilder.MapLocalizedRoute("BlogRSS", "blog/rss/{languageId:min(0)}",
				new { controller = "Blog", action = "ListRss" });

            //news RSS
            routeBuilder.MapLocalizedRoute("NewsRSS", "news/rss/{languageId:min(0)}",
				new { controller = "News", action = "ListRss" });

            //set review helpfulness (AJAX link)
            routeBuilder.MapRoute("SetProductReviewHelpfulness", "setproductreviewhelpfulness",
				new { controller = "Product", action = "SetProductReviewHelpfulness" });

            //customer account links
            routeBuilder.MapLocalizedRoute("CustomerChangePassword", "customer/changepassword",
				new { controller = "Customer", action = "ChangePassword" });

            routeBuilder.MapLocalizedRoute("CustomerAvatar", "customer/avatar",
				new { controller = "Customer", action = "Avatar" });

            routeBuilder.MapLocalizedRoute("AccountActivation", "customer/activation",
				new { controller = "Customer", action = "AccountActivation" });

            routeBuilder.MapLocalizedRoute("EmailRevalidation", "customer/revalidateemail",
				new { controller = "Customer", action = "EmailRevalidation" });

            routeBuilder.MapLocalizedRoute("CustomerForumSubscriptions", "boards/forumsubscriptions",
				new { controller = "Boards", action = "CustomerForumSubscriptions" });

            routeBuilder.MapLocalizedRoute("CustomerForumSubscriptionsPaged", "boards/forumsubscriptions/{pageNumber:regex(\\d*)}",
				new { controller = "Boards", action = "CustomerForumSubscriptions" });

            routeBuilder.MapLocalizedRoute("CustomerAddressEdit", "customer/addressedit/{addressId:min(0)}",
				new { controller = "Customer", action = "AddressEdit" });

            routeBuilder.MapLocalizedRoute("CustomerAddressAdd", "customer/addressadd",
				new { controller = "Customer", action = "AddressAdd" });

            //customer profile page
            routeBuilder.MapLocalizedRoute("CustomerProfile", "profile/{id:min(0)}",
				new { controller = "Profile", action = "Index" });

            routeBuilder.MapLocalizedRoute("CustomerProfilePaged", "profile/{id}/page/{pageNumber:min(0)}",
				new { controller = "Profile", action = "Index" });

            //poll vote AJAX link
            routeBuilder.MapLocalizedRoute("PollVote", "poll/vote",
				new { controller = "Poll", action = "Vote" });

            //new RSS
            routeBuilder.MapLocalizedRoute("NewProductsRSS", "newproducts/rss",
				new { controller = "Product", action = "NewProductsRss" });
            
            //get state list by country ID  (AJAX link)
            routeBuilder.MapRoute("GetStatesByCountryId", "country/getstatesbycountryid/",
				new { controller = "Country", action = "GetStatesByCountryId" });

            //EU Cookie law accept button handler (AJAX link)
            routeBuilder.MapRoute("EuCookieLawAccept", "eucookielawaccept",
				new { controller = "Common", action = "EuCookieLawAccept" });

            //authenticate topic AJAX link
            routeBuilder.MapLocalizedRoute("TopicAuthenticate", "topic/authenticate",
				new { controller = "Topic", action = "Authenticate" });

            //forums
            routeBuilder.MapLocalizedRoute("ActiveDiscussions", "boards/activediscussions",
				new { controller = "Boards", action = "ActiveDiscussions" });

            routeBuilder.MapLocalizedRoute("ActiveDiscussionsPaged", "boards/activediscussions/page/{pageNumber:regex(\\d*)}",
				new { controller = "Boards", action = "ActiveDiscussions" });

            routeBuilder.MapLocalizedRoute("ActiveDiscussionsRSS", "boards/activediscussionsrss",
				new { controller = "Boards", action = "ActiveDiscussionsRSS" });

            routeBuilder.MapLocalizedRoute("PostEdit", "boards/postedit/{id:min(0)}",
				new { controller = "Boards", action = "PostEdit" });

            routeBuilder.MapLocalizedRoute("PostDelete", "boards/postdelete/{id:min(0)}",
				new { controller = "Boards", action = "PostDelete" });

            routeBuilder.MapLocalizedRoute("PostCreate", "boards/postcreate/{id:min(0)}",
				new { controller = "Boards", action = "PostCreate" });

            routeBuilder.MapLocalizedRoute("PostCreateQuote", "boards/postcreate/{id:min(0)}/{quote:min(0)}",
				new { controller = "Boards", action = "PostCreate" });

            routeBuilder.MapLocalizedRoute("TopicEdit", "boards/topicedit/{id:min(0)}",
				new { controller = "Boards", action = "TopicEdit" });

            routeBuilder.MapLocalizedRoute("TopicDelete", "boards/topicdelete/{id:min(0)}",
				new { controller = "Boards", action = "TopicDelete" });

            routeBuilder.MapLocalizedRoute("TopicCreate", "boards/topiccreate/{id:min(0)}",
				new { controller = "Boards", action = "TopicCreate" });

            routeBuilder.MapLocalizedRoute("TopicMove", "boards/topicmove/{id:min(0)}",
				new { controller = "Boards", action = "TopicMove" });

            routeBuilder.MapLocalizedRoute("TopicWatch", "boards/topicwatch/{id:min(0)}",
				new { controller = "Boards", action = "TopicWatch" });

            routeBuilder.MapLocalizedRoute("TopicSlug", "boards/topic/{id:min(0)}/{slug?}",
				new { controller = "Boards", action = "Topic" });

            routeBuilder.MapLocalizedRoute("TopicSlugPaged", "boards/topic/{id:min(0)}/{slug?}/page/{pageNumber:regex(\\d*)}",
				new { controller = "Boards", action = "Topic" });

            routeBuilder.MapLocalizedRoute("ForumWatch", "boards/forumwatch/{id:min(0)}",
				new { controller = "Boards", action = "ForumWatch" });

            routeBuilder.MapLocalizedRoute("ForumRSS", "boards/forumrss/{id:min(0)}",
				new { controller = "Boards", action = "ForumRSS" });

            routeBuilder.MapLocalizedRoute("ForumSlug", "boards/forum/{id:min(0)}/{slug?}",
				new { controller = "Boards", action = "Forum" });

            routeBuilder.MapLocalizedRoute("ForumSlugPaged", "boards/forum/{id:min(0)}/{slug?}/page/{pageNumber:regex(\\d*)}",
				new { controller = "Boards", action = "Forum" });

            routeBuilder.MapLocalizedRoute("ForumGroupSlug", "boards/forumgroup/{id:min(0)}/{slug?}",
				new { controller = "Boards", action = "ForumGroup"});

            routeBuilder.MapLocalizedRoute("Search", "boards/search",
				new { controller = "Boards", action = "Search" });

            //private messages
            routeBuilder.MapLocalizedRoute("PrivateMessages", "privatemessages/{tab?}",
				new { controller = "PrivateMessages", action = "Index" });

            routeBuilder.MapLocalizedRoute("PrivateMessagesPaged", "privatemessages/{tab?}/page/{pageNumber:min(0)}",
				new { controller = "PrivateMessages", action = "Index" });

            routeBuilder.MapLocalizedRoute("PrivateMessagesInbox", "inboxupdate",
				new { controller = "PrivateMessages", action = "InboxUpdate" });

            routeBuilder.MapLocalizedRoute("PrivateMessagesSent", "sentupdate",
				new { controller = "PrivateMessages", action = "SentUpdate" });

            routeBuilder.MapLocalizedRoute("SendPM", "sendpm/{toCustomerId:min(0)}",
				new { controller = "PrivateMessages", action = "SendPM" });

            routeBuilder.MapLocalizedRoute("SendPMReply", "sendpm/{toCustomerId:min(0)}/{replyToMessageId:min(0)}",
				new { controller = "PrivateMessages", action = "SendPM" });

            routeBuilder.MapLocalizedRoute("ViewPM", "viewpm/{privateMessageId:min(0)}",
				new { controller = "PrivateMessages", action = "ViewPM" });

            routeBuilder.MapLocalizedRoute("DeletePM", "deletepm/{privateMessageId:min(0)}",
				new { controller = "PrivateMessages", action = "DeletePM" });

            //activate newsletters
            routeBuilder.MapLocalizedRoute("NewsletterActivation", "newsletter/subscriptionactivation/{token:guid}/{active}",
				new { controller = "Newsletter", action = "SubscriptionActivation" });

            //robots.txt
            routeBuilder.MapRoute("robots.txt", "robots.txt",
				new { controller = "Common", action = "RobotsTextFile" });

            //sitemap (XML)
            routeBuilder.MapLocalizedRoute("sitemap.xml", "sitemap.xml",
				new { controller = "Common", action = "SitemapXml" });

            routeBuilder.MapLocalizedRoute("sitemap-indexed.xml", "sitemap-{Id:min(0)}.xml",
				new { controller = "Common", action = "SitemapXml" });

            //store closed
            routeBuilder.MapLocalizedRoute("StoreClosed", "storeclosed",
				new { controller = "Common", action = "StoreClosed" });

            //install
            routeBuilder.MapRoute("Installation", "install",
				new { controller = "Install", action = "Index" });

            //error page
            routeBuilder.MapLocalizedRoute("Error", "error",
                new { controller = "Common", action = "Error" });

            //page not found
            routeBuilder.MapLocalizedRoute("PageNotFound", "page-not-found", 
                new { controller = "Common", action = "PageNotFound" });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return 0; }
        }

        #endregion
    }
}
