using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Kentico.Kontent.Delivery;
using Kontent.Wyam.Metadata;
using System;

namespace Kontent.Wyam
{
    /// <summary>
    /// Retrieves content items from Kentico Cloud.
    /// </summary>
    public class Kontent : IModule
    {
        private readonly IDeliveryClient _client;
        private readonly List<IQueryParameter> QueryParameters = new List<IQueryParameter>();

        private string contentField;
        private readonly IElementParserFactory _elementParserFactory;

        public Kontent(IDeliveryClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _elementParserFactory = new ElementParserFactory();
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


        /// <summary>
        /// Sets the content type to retrieve.
        /// </summary>
        /// <param name="contentType">Code name of the content type to retrieve.</param>
        /// <returns></returns>
        public Kontent WithContentType(string contentType)
        {
            QueryParameters.Add(new EqualsFilter("system.type", contentType));
            return this;
        }

        /// <summary>
        /// Sets the main content field. This is case sensitive.
        /// </summary>
        /// <param name="field">Field</param>
        /// <returns></returns>
        public Kontent WithContentField(string field)
        {
            contentField = field;
            return this;
        }

        /// <summary>
        /// Sets the ordering for retrieved content items.
        /// </summary>
        /// <param name="field">Field to order by</param>
        /// <param name="sortOrder">Sort order</param>
        /// <returns></returns>
        public Kontent OrderBy(string field, SortOrder sortOrder)
        {
            QueryParameters.Add(new OrderParameter(field, (Kentico.Kontent.Delivery.SortOrder)sortOrder));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if( string.IsNullOrWhiteSpace(contentField))
                throw new InvalidOperationException("Content field not set.");

            var items = _client.GetItemsAsync(QueryParameters).Result;

            foreach (var item in items.Items)
            {
                var metadata = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("name", item.System.Name),
                    new KeyValuePair<string, object>("codename", item.System.Codename)
                };

                foreach (var element in item.Elements)
                {
                    string type = element.Value.type;

                    var parser = _elementParserFactory.GetParser(type);
                    parser.ParseMetadata(metadata, element);
                }

                var content = item.GetString(contentField);
                var doc = context.GetDocument(context.GetContentStream(content), metadata);

                yield return doc;
            }
        }
    }

    public class Kontent<TPageModel> : IModule
    {
        private readonly IDeliveryClient _client;
        private readonly List<IQueryParameter> _queryParameters = new List<IQueryParameter>();

        private string _contentField;
        private readonly IElementParserFactory _elementParserFactory;

        public Kontent()
        {
            _elementParserFactory = new ElementParserFactory();
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        public Kontent(string projectId) : this()
        {
            _client = DeliveryClientBuilder
                    .WithProjectId(projectId)
                    .Build();
        }

        /// <summary>
        /// Specifies the project ID to use for retrieving content items from Kentico Cloud.
        /// <seealso cref="!:https://developer.Kontent.com/docs/using-delivery-api#section-getting-project-id" />
        /// </summary>
        /// <param name="projectId">Kentico Cloud project ID</param>
        public Kontent(string projectId, string previewApiKey) : this()
        {
            _client = DeliveryClientBuilder.WithOptions(builder => builder
                        .WithProjectId(projectId)
                        .UsePreviewApi(previewApiKey)
                        .Build())
                    .Build();
        }


        /// <summary>
        /// Sets the content type to retrieve.
        /// </summary>
        /// <param name="contentType">Code name of the content type to retrieve.</param>
        /// <returns></returns>
        public Kontent<TPageModel> WithContentType(string contentType)
        {
            _queryParameters.Add(new EqualsFilter("system.type", contentType));
            return this;
        }

        /// <summary>
        /// Sets the main content field.
        /// </summary>
        /// <param name="field">Field</param>
        /// <returns></returns>
        public Kontent<TPageModel> WithContentField(string field)
        {
            _contentField = field;
            return this;
        }

        /// <summary>
        /// Sets the ordering for retrieved content items.
        /// </summary>
        /// <param name="field">Field to order by</param>
        /// <param name="sortOrder">Sort order</param>
        /// <returns></returns>
        public Kontent<TPageModel> OrderBy(string field, SortOrder sortOrder)
        {
            _queryParameters.Add(new OrderParameter(field, (Kentico.Kontent.Delivery.SortOrder)sortOrder));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var items = _client.GetItemsAsync(_queryParameters).Result;

            foreach (var item in items.Items)
            {
                var metadata = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("name", item.System.Name),
                    new KeyValuePair<string, object>("codename", item.System.Codename)
                };

                foreach (var element in item.Elements)
                {
                    string type = element.Value.type;

                    var parser = _elementParserFactory.GetParser(type);
                    parser.ParseMetadata(metadata, element);
                }

                var content = item.GetString(_contentField);
                var doc = context.GetDocument(context.GetContentStream(content), metadata);

                yield return doc;
            }
        }
    }
}
