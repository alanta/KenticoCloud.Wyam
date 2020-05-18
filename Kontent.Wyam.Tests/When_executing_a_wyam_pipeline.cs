using FluentAssertions;
using Kontent.Wyam.Tests.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Execution;
using Xunit;

namespace Kontent.Wyam.Tests
{
    public class When_executing_a_wyam_pipeline
    {
        [Fact]
        public void It_should_correctly_copy_all_fields_into_the_document()
        {
            // Arrange

            // Act
            var sut = SetupWyamExecution( _ => Console.WriteLine() );
            sut.Execute();

            // Assert
            sut.Documents.Should().HaveCount(1);
            var metadata = sut.Documents.First().Metadata;

            metadata.Get("title").ToString().Should().Be("Coffee Beverages Explained");
            metadata.Get("body_copy").ToString().Should().Contain("Espresso");
        }

        [Fact]
        public void It_should_correctly_set_the_default_content()
        {
            // Arrange

            // Act
            var sut = SetupWyamExecution(

                // Assert    
                docs => docs.First().Content.Should().Contain("Espresso") 
                );
            sut.Execute();
        }

        public Engine SetupWyamExecution(Action<IReadOnlyList<IDocument>> test)
        {
            var engine = new global::Wyam.Core.Execution.Engine();
            var pipeline = new Pipeline(new[]{
                SetupKontentModule(),
                new TestModule(test)
            });

            engine.Pipelines.Add("test", pipeline);
            return engine;
        }


        private IModule SetupKontentModule()
        {
            var responseJsonPath = Path.Combine(Environment.CurrentDirectory, $"response{Path.DirectorySeparatorChar}getitems.json");
            var responseJson = File.ReadAllText(responseJsonPath);
            return new Kontent(MockDeliveryClient.Create(responseJson))
                .WithContentField("body_copy");
        }

        private class TestModule : IModule
        {
            private readonly Action<IReadOnlyList<IDocument>> _test;

            public TestModule(Action<IReadOnlyList<IDocument>> test)
            {
                _test = test;
            }
            public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
            {
                _test(inputs);
                return inputs;
            }
        }
    }
}
