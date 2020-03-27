using System.Collections.Generic;
using System.Linq;
using Kontent.Wyam.Models;
using Newtonsoft.Json.Linq;

namespace Kontent.Wyam.Metadata
{
    /// <summary>
    /// Parses content item assets as a <see cref="List{Asset}">List&lt;Asset&gt;</see>.
    /// </summary>
    public static class AssetElementParser
    {
        public static bool TryParseMetadata(dynamic element, out KeyValuePair<string, object> metadata)
        {
            if (element.Value == null || !((IEnumerable<object>) element.Value.value).Any())
            {
                return false;
            }

            metadata = new KeyValuePair<string, object>(element.Name, (from arrayItem in (JArray) element.Value.value
                select new Asset
                {
                    Name = arrayItem["name"].Value<string>(),
                    Url = arrayItem["url"].Value<string>()
                }).ToList());
            return true;
        }
    }
}