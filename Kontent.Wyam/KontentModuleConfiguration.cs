using Kentico.Kontent.Delivery;

namespace Kontent.Wyam
{
    public static class KontentModuleConfiguration
    {
        /// <summary>
        /// Sets the content type to retrieve.
        /// </summary>
        /// <param name="contentType">Code name of the content type to retrieve.</param>
        /// <returns></returns>
        public static Kontent WithContentType(this Kontent module, string contentType)
        {
            module.QueryParameters.Add(new EqualsFilter("system.type", contentType));
            return module;
        }

        /// <summary>
        /// Sets the main content field. This is case sensitive.
        /// </summary>
        /// <param name="field">Field</param>
        /// <returns></returns>
        public static Kontent WithContentField(this Kontent module, string field)
        {
            module.ContentField = field;
            return module;
        }

        /// <summary>
        /// Sets the ordering for retrieved content items.
        /// </summary>
        /// <param name="field">Field to order by</param>
        /// <param name="sortOrder">Sort order</param>
        /// <returns></returns>
        public static Kontent OrderBy(this Kontent module, string field, SortOrder sortOrder)
        {
            module.QueryParameters.Add(new OrderParameter(field, (Kentico.Kontent.Delivery.Abstractions.SortOrder)sortOrder));
            return module;
        }
    }
}