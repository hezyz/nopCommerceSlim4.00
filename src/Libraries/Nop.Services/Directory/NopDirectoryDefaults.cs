namespace Nop.Services.Directory
{
    /// <summary>
    /// Represents default values related to directory services
    /// </summary>
    public static partial class NopDirectoryDefaults
    {
        #region Countries

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : show hidden records?
        /// </remarks>
        public static string CountriesAllCacheKey => "Nop.country.all-{0}-{1}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CountriesPatternCacheKey => "Nop.country.";

        #endregion

        #region States and provinces

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : country ID
        /// {1} : language ID
        /// {2} : show hidden records?
        /// </remarks>
        public static string StateProvincesAllCacheKey => "Nop.stateprovince.all-{0}-{1}-{2}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string StateProvincesPatternCacheKey => "Nop.stateprovince.";

        #endregion
    }
}