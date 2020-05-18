using FakeItEasy;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Wyam.Tests.Models;
using Kontent.Wyam.Tests.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Execution;
using Wyam.Testing.Documents;
using Xunit;

namespace Kontent.Wyam.Tests
{
    public class When_loading_typed_content_from_Kontent
    {
        [Fact]
        public void It_should_correctly_materialize_the_document()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Kontent( MockDeliveryClient.Create(responseJson, cfg => cfg
                .WithTypeProvider(new CustomTypeProvider())))
                .WithContentField(Article.BodyCopyCodename)
                .WithUrlField(Article.UrlPatternCodename);

            var context = Setup_ExecutionContext();

            // Act
            var result = sut.Execute(null, context).ToArray();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Should().BeOfType<TestDocument>();
            var article = result[0].AsKontent<Article>();

            article.Title.Should().StartWith("Coffee");
            article.System.Codename.Should().Be("coffee_beverages_explained");
            article.System.Type.Should().Be(Article.Codename);
        }

        [Fact]
        public void It_should_resolve_inline_content_types()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Kontent(MockDeliveryClient.Create(responseJson, cfg => cfg
                    .WithTypeProvider(new CustomTypeProvider())))
                    .WithContentField(Article.BodyCopyCodename)
                    .WithUrlField(Article.UrlPatternCodename);

            var context = Setup_ExecutionContext();

            // Act
            var result = sut.Execute(null, context).ToArray();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Should().BeOfType<TestDocument>();
            var article = result[0].AsKontent<Article>();

            article.Title.Should().StartWith("Coffee");
            article.BodyCopy.Blocks.Should().Contain(block => block is IInlineContentItem && ((IInlineContentItem)block).ContentItem is Tweet, "Inline content must be resolved");
        }

        private static IExecutionContext Setup_ExecutionContext()
        {
            var context = A.Fake<IExecutionContext>();
            A.CallTo(() => context.GetDocument(A<System.IO.Stream>.Ignored,
                    A<IEnumerable<KeyValuePair<string, object>>>.Ignored, true))
                .ReturnsLazily((Stream stream, IEnumerable<KeyValuePair<string, object>> meta, bool dispose) =>
                    new TestDocument(stream, meta));
            return context;
        }
    }
}
