using FakeItEasy;
using FluentAssertions;
using Kontent.Wyam.Tests.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Execution;
using Xunit;

namespace Kontent.Wyam.Tests
{
    public class When_loading_untyped_content_from_Kontent
    {
        [Fact]
        public void It_should_correctly_copy_all_fields_into_the_document()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Kontent(MockDeliveryClient.Create(responseJson))
                .WithContentField("body_copy");

            var context = A.Fake<IExecutionContext>();

            // Act
            var result = sut.Execute(null, context).ToArray();

            // Assert
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void It_should_correctly_set_the_default_content()
        {
            // Arrange
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);

            var sut = new Kontent(MockDeliveryClient.Create(responseJson))
                .WithContentField("body_copy");

            var context = A.Fake<IExecutionContext>();
            // Act
            var result = sut.Execute(null, context).ToArray();

            // Assert
            A.CallTo(() =>
                    context.GetDocument(A<Stream>.Ignored, A<IEnumerable<KeyValuePair<string, object>>>.Ignored, true))
                .MustHaveHappened();
        }


    }
}

