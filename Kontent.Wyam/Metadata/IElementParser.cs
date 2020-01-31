using System.Collections.Generic;

namespace Kontent.Wyam.Metadata
{
    public interface IElementParser
    {
        void ParseMetadata(List<KeyValuePair<string, object>> metadata, dynamic element);
    }
}