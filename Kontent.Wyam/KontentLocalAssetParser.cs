using System.Collections.Generic;
using System.Linq;
using Kontent.Wyam.Models;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Kontent.Wyam
{
    /// <summary>
    /// Parses document content by replacing <c>!!local-assets/</c> paths with URLs to downloaded assets.
    /// URLs are matched by the file name of the asset.
    /// </summary>
    public class KontentLocalAssetParser : IModule
    {
        private string _folderPath = string.Empty;

        public KontentLocalAssetParser WithFolderPath(string folderPath)
        {
            _folderPath = folderPath + "/";
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (var doc in inputs)
            {
                var newDoc = doc;
                var content = doc.Content;

                var assets = doc.Metadata.Where(x => x.Value is List<Asset>).ToList();
                if (assets.Any())
                {
                    foreach (var metaAsset in assets)
                    {
                        var asset = (List<Asset>)metaAsset.Value;

                        foreach (var image in asset)
                        {
                            content = content.Replace($"!!local-assets/{image.Name}", $"/{_folderPath}{KontentAssetHelper.GetAssetFileName(image.Url)}");
                        }
                    }

                    newDoc = context.GetDocument(doc, context.GetContentStream(content));
                }

                yield return newDoc;
            }
        }
    }
}
