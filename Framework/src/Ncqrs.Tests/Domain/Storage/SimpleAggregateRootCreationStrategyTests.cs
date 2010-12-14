using System;
using FluentAssertions;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using NUnit.Framework;
using System.IO;

namespace Ncqrs.Tests.Domain.Storage
{
    [TestFixture]
    public class SimpleAggregateRootCreationStrategyTests
    {
        public class AggRootWithDefaultCtor : AggregateRoot
        {}

        public class AggRootWithoutDefaultCtor : AggregateRoot
        {
            public AggRootWithoutDefaultCtor(string foo)
            {}
        }

        [Test]
        public void Creating_without_default_ctor_should_return_null()
        {
            var wrongType = typeof (AggRootWithoutDefaultCtor);
            var creator = new SimpleAggregateRootCreationStrategy();
            
            var result = creator.CreateAggregateRoot(wrongType);

            result.Should().BeNull();
        }

        [Test]
        public void Creating_non_aggregate_root_type_should_return_null()
        {
            var wrongType = typeof(Stream);
            var creator = new SimpleAggregateRootCreationStrategy();

            var result = creator.CreateAggregateRoot(wrongType);

            result.Should().BeNull();
        }

        [Test]
        public void Creating_instance_should_succeed_when_type_is_subclass_of_AggregateRoot_and_has_default_ctor()
        {
            var correctType = typeof (AggRootWithDefaultCtor);
            var creator = new SimpleAggregateRootCreationStrategy();

            creator.CreateAggregateRoot(correctType);
        }

        [Test]
        public void Creation_result_by_correct_type_should_not_be_null()
        {
            var correctType = typeof(AggRootWithDefaultCtor);
            var creator = new SimpleAggregateRootCreationStrategy();

            var result = creator.CreateAggregateRoot(correctType);
            result.Should().NotBeNull();
        }

        [Test]
        public void Creation_result_by_correct_type_should_be_of_specified_type()
        {
            var correctType = typeof(AggRootWithDefaultCtor);
            var creator = new SimpleAggregateRootCreationStrategy();

            var result = creator.CreateAggregateRoot(correctType);
            result.Should().BeOfType<AggRootWithDefaultCtor>();
        }
    }
}