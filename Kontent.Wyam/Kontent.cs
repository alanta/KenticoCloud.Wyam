using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Builders.DeliveryClient;
using Kontent.Wyam.Metadata;
using System;
using System.Collections;
using System.IO;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Documents;

namespace Kontent.Wyam
{
    /// <summary>
    /// Retrieves content items from Kentico Cloud.
    /// </summary>
    public class Kontent : IModule
    {
        public string PreviewApiKey { get; set; }
        public string ProductionApiKey { get; set; }
        public string ProjectId { get; }
        public string ContentField { get; set; }
        public string UrlField { get; set; }
        protected readonly Lazy<IDeliveryClient> Client;
        internal List<Action<IOptionalClientSetup>> ConfigureClientActions = new List<Action<IOptionalClientSetup>>();
        public List<IQueryParameter> QueryParameters { get; } = new List<IQueryParameter>();

        public Kontent(IDeliveryClient client)
        {
            if( client == null )
                throw new ArgumentNullException($"{nameof(client)} must not be null");

            Client = new Lazy<IDeliveryClient>(() => client);
        }

        private IDeliveryClient CreateClient()
        {
            var builder = DeliveryClientBuilder
                .WithOptions(options =>
                {
                    var opt2 = options.WithProjectId(ProjectId);
                    
                    if (!string.IsNullOrWhiteSpace(PreviewApiKey))
                    {
                        return opt2.UsePreviewApi(PreviewApiKey).Build();
                    }

                    if (!string.IsNullOrEmpty(ProductionApiKey))
                    {
                        return opt2.UseProductionApi(ProductionApiKey).Build();
                    }

                    return opt2.UseProductionApi().Build();

                });

            foreach (var action in ConfigureClientActions)
            {
                action(builder);
            }

            return builder.Build();
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        public Kontent(string projectId) : this(projectId, string.Empty)
        {
            
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        public Kontent(string projectId, string previewApiKey)
        {
            ProjectId = projectId;
            PreviewApiKey = previewApiKey;
            Client = new Lazy<IDeliveryClient>(CreateClient);
        }


        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var items = Client.Value.GetItemsAsync(QueryParameters).Result;

            foreach (var item in items.Items)
            {
                yield return CreateDocument(context, item);
            }
        }

        protected IDocument CreateDocument(IExecutionContext context, ContentItem item)
        {
            var metadata = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(TypedContentExtensions.KontentItemKey, item),
                new KeyValuePair<string, object>("name", item.System.Name),
                new KeyValuePair<string, object>("codename", item.System.Codename)
            };

            foreach (var element in item.Elements)
            {
                string type = element.Value.type;

                KeyValuePair<string, object> metadataItem;
                switch (type)
                {
                    case "asset":
                        if (AssetElementParser.TryParseMetadata(element, out metadataItem)) metadata.Add(metadataItem);
                        break;
                    default:
                        if (DefaultElementParser.TryParseMetadata(element, out metadataItem))
                        {
                            if (string.Equals(metadataItem.Key, UrlField))
                            {
                                metadata.Add(new KeyValuePair<string, object>("url", metadataItem.Value));
                            }

                            metadata.Add(metadataItem);
                        }
                        break;
                }
            }

            var content = string.IsNullOrWhiteSpace(ContentField) ? "" : item.GetString(ContentField);
            
            return context.GetDocument(context.GetContentStream(content), metadata);
        }
    }
}
