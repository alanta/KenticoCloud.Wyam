using Kentico.Kontent.Delivery.Abstractions;
using System;
using Wyam.Common.Documents;

namespace Kontent.Wyam
{
    public static class TypedContentExtensions
    {
        public const string KontentItemKey = "KONTENT";

        public static TModel AsKontent<TModel>(this IDocument document)
        {
            if (document.TryGetValue(KontentItemKey, out ContentItem contentItem))
            {
                return contentItem.CastTo<TModel>();
            }

            throw new InvalidOperationException($"This is not a Kontent document: {document.Source}");
        }
    }
}
