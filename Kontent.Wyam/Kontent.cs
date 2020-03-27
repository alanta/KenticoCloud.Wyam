using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Kentico.Kontent.Delivery;
using Kontent.Wyam.Metadata;
using System;
using Wyam.Core.Documents;

namespace Kontent.Wyam
{
    /// <summary>
    /// Retrieves content items from Kentico Cloud.
    /// </summary>
    public class Kontent : IModule
    {
        private readonly IDeliveryClient _client;
        public List<IQueryParameter> QueryParameters { get; } = new List<IQueryParameter>();

        public string ContentField { get; set; }

        public Kontent(IDeliveryClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        private static IDeliveryClient CreateClient(string projectId)
        {
            return DeliveryClientBuilder
                .WithProjectId(projectId)
                .Build();
        }

        private static IDeliveryClient CreateClientWithPreview(string projectId, string previewApiKey)
        {
            return DeliveryClientBuilder.WithOptions(builder => builder
                    .WithProjectId(projectId)
                    .UsePreviewApi(previewApiKey)
                    .Build())
                .Build();
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        public Kontent(string projectId) 
            : this(CreateClient(projectId))
        {
            
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        public Kontent(string projectId, string previewApiKey) 
            : this(CreateClientWithPreview(projectId, previewApiKey))
        {
        }


        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var items = _client.GetItemsAsync(QueryParameters).Result;

            foreach (var item in items.Items)
            {
                yield return CreateDocument(context, item);
            }
        }

        protected virtual IDocument CreateDocument(IExecutionContext context, ContentItem item)
        {
            var metadata = new List<KeyValuePair<string, object>>
            {
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
                        if (DefaultElementParser.TryParseMetadata(element, out metadataItem)) metadata.Add(metadataItem);
                        break;
                }

                metadata.Add(metadataItem);
            }

            var content = string.IsNullOrWhiteSpace(ContentField) ? "" : item.GetString(ContentField);
            
            return context.GetDocument(context.GetContentStream(content), metadata);
        }
    }

    public class Kontent<TPageModel> : Kontent
    {
        /// <summary>
        /// Create a Kentico module using a specific Delivery client.
        /// </summary>
        /// <param name="client">A Kontent Delivery API client.</param>
        public Kontent(IDeliveryClient client) : base(client)
        {
            
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        public Kontent(string projectId) : base( projectId)
        {
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        /// <param name="previewApiKey">The preview API.</param>
        public Kontent(string projectId, string previewApiKey) : base(projectId, previewApiKey)
        {
        }

        protected override IDocument CreateDocument(IExecutionContext context, ContentItem item)
        {
            // TODO : fill the document
            return new KontentDocument<TPageModel>(item);
        }
    }

    public class KontentDocument<TPageModel> : CustomDocument
    {
        private readonly ContentItem _item;

        public KontentDocument(ContentItem item)
        {
            _item = item;
        }

        public TPageModel Model => _item.CastTo<TPageModel>();
    }
}
