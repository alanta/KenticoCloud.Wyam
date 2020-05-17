using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Wyam.Metadata;
using System;
using System.ComponentModel;
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
        public ITypeProvider TypeProvider { get; set; }
        protected readonly Lazy<IDeliveryClient> Client;
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

            if (TypeProvider != null)
            {
                builder = builder.WithTypeProvider(TypeProvider);
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


        public virtual IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var items = Client.Value.GetItemsAsync(QueryParameters).Result;

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

                
                metadata.Add(metadataItem);
            }

            var content = string.IsNullOrWhiteSpace(ContentField) ? "" : item.GetString(ContentField);
            
            return context.GetDocument(context.GetContentStream(content), metadata);
        }

        public void UseModel<TModel>() where TModel : class
        {
            
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

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var items = Client.Value.GetItemsAsync<TPageModel>(QueryParameters).Result;

            foreach (var item in items.Items)
            {
                yield return CreateDocument(context, item);
            }
        }

        protected IDocument CreateDocument(IExecutionContext context, TPageModel item)
        {
            // TODO : fill the document
            return new KontentDocument<TPageModel>(item);
        }
    }

    public class KontentDocument<TPageModel> : CustomDocument
    {
        public KontentDocument(TPageModel item)
        {
            Model = item;
            
        }

        public TPageModel Model { get; }
    }

    internal static class BuilderExtensions
    {
        public static TBuilder Iif<TBuilder>(this TBuilder builder, bool predicate, Func<TBuilder, TBuilder> apply)
        {
            if (predicate)
                return apply(builder);

            return builder;
        }
    }
}
