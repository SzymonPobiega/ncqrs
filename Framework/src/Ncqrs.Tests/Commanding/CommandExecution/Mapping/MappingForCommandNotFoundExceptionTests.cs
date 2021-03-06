﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using NUnit.Framework;
using Rhino.Mocks;

namespace Ncqrs.Tests.Commanding.CommandExecution.Mapping
{
    public class MappingForCommandNotFoundExceptionTests
    {
        [Test]
        public void Constructing_an_instance_should_initialize_the_message()
        {
            String message = "Hello world";
            ICommand aCommand = MockRepository.GenerateMock<ICommand>();

            var target = new MappingForCommandNotFoundException(message, aCommand);

            target.Message.Should().Be(message);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_command()
        {
            String aMessage = "Hello world";
            ICommand theCommand = MockRepository.GenerateMock<ICommand>();

            var target = new MappingForCommandNotFoundException(aMessage, theCommand);

            target.Command.Should().Be(theCommand);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_inner_exception()
        {
            String aMessage = "Hello world";
            ICommand theCommand = MockRepository.GenerateMock<ICommand>();            
            var theInnerException = new Exception();

            var target = new MappingForCommandNotFoundException(aMessage, theCommand, theInnerException);

            target.InnerException.Should().Be(theInnerException);
        }

        [Test]
        public void It_should_be_serializable()
        {
            var aMessage = "Hello world";
            ICommand aCommand = MockRepository.GenerateMock<ICommand>();
            var theException = new MappingForCommandNotFoundException(aMessage, aCommand);
            MappingForCommandNotFoundException deserializedException = null;

            using (var buffer = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(buffer, theException);

                buffer.Seek(0, SeekOrigin.Begin);
                deserializedException = (MappingForCommandNotFoundException)formatter.Deserialize(buffer);
            }

            deserializedException.Should().NotBeNull();
        }
    }
}
