using kolekt.EventSourcing.Consumers;
using kolekt.EventSourcing.Messages;
using kolekt.EventSourcing.Providers;
using MassTransit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace kolekt.EventSourcing.UnitTests
{
    public class MessageBusTests
    {
        private readonly Mock<ISendEndpointProvider> _sendEndpointProvider;
        private readonly Mock<IClientFactory> _clientFactory;
        private readonly Mock<ConsumeContext> _context;
        private readonly MessageBus _target;

        private readonly Event _testEvent;
        private readonly Command _testCommand;

        public MessageBusTests()
        {
            _clientFactory = new Mock<IClientFactory>();
            _sendEndpointProvider = new Mock<ISendEndpointProvider>();
            _context = new Mock<ConsumeContext>();
            _target = new MessageBus(_clientFactory.Object, _sendEndpointProvider.Object);

            _testEvent = new MockEvent();
            _testCommand = new MockCommand();
        }

        [Fact]
        public async Task When_PublishingEvent_Then_ConsumeContextIsUsed()
        {
            _context.Setup(a => a.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _target.PublishEventAsync(_testEvent, _context.Object);
            _context.Verify(a => a.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task When_PublishingEvent_Then_ContextObjectMethodIsUsed()
        {
            _context.Setup(a => a.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _context.Setup(a => a.Publish(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _target.PublishEventAsync(_testEvent, _context.Object);
            _context.Verify(a => a.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once());
            _context.Verify(a => a.Publish(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task When_SendingCommand_And_ContextIsNull_Then_ClientFactoryWithoutContextIsUsed()
        {
            var requestClient = BuildRequestClient<MockCommand>(true);

            _clientFactory.Setup(a => a.CreateRequestClient<MockCommand>(It.IsAny<Uri>(), It.IsAny<RequestTimeout>()))
                .Returns(requestClient.Object)
                .Verifiable();

            var response = await _target.SendCommandAsync<MockCommand, MockAggregate>((MockCommand)_testCommand);

            _clientFactory.Verify(a => a.CreateRequestClient<MockCommand>(It.IsAny<Uri>(), It.IsAny<RequestTimeout>()), Times.Once());
        }

        [Fact]
        public async Task When_SendingCommand_And_ContextIsProvided_Then_ClientFactoryWithContextIsUsed()
        {
            var requestClient = BuildRequestClient<MockCommand>(true);

            _clientFactory.Setup(a => a.CreateRequestClient<MockCommand>(It.IsAny<ConsumeContext>(), It.IsAny<Uri>(), It.IsAny<RequestTimeout>()))
                .Returns(requestClient.Object)
                .Verifiable();

            var response = await _target.SendCommandAsync<MockCommand, MockAggregate>((MockCommand)_testCommand, _context.Object);

            _clientFactory.Verify(a => a.CreateRequestClient<MockCommand>(It.IsAny<ConsumeContext>(), It.IsAny<Uri>(), It.IsAny<RequestTimeout>()), Times.Once());
        }

        [Fact]
        public async Task When_SendingCommand_And_ContextIsNull_Then_ProvidedUriIsCorrect()
        {
            Uri actual = null;
            Uri expected = new Uri($"queue:{typeof(MockCommand).Name}Handler");

            var requestClient = BuildRequestClient<MockCommand>(true);

            _clientFactory.Setup(a => a.CreateRequestClient<MockCommand>(It.IsAny<Uri>(), It.IsAny<RequestTimeout>()))
                .Returns(requestClient.Object)
                .Callback((Uri u, RequestTimeout t) => actual = u);

            var response = await _target.SendCommandAsync<MockCommand, MockAggregate>((MockCommand)_testCommand);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task When_SendingCommand_Then_CommandResponseIsReturned()
        {
            var requestClient = BuildRequestClient<MockCommand>(true);

            _clientFactory.Setup(a => a.CreateRequestClient<MockCommand>(It.IsAny<Uri>(), It.IsAny<RequestTimeout>()))
                .Returns(requestClient.Object);

            var response = await _target.SendCommandAsync<MockCommand, MockAggregate>((MockCommand)_testCommand);

            Assert.IsAssignableFrom<CommandResponse<MockAggregate>>(response);
        }

        private Mock<IRequestClient<TCommand>> BuildRequestClient<TCommand>(bool succeeds) where TCommand : Command
        {
            var response = new CommandResponse<MockAggregate>()
            {
                Success = succeeds,
                Data = succeeds ? new MockAggregate(Guid.NewGuid()) : null,
                ErrorMessage = succeeds ? null : "Failure"
            };

            var responseWrapper = new Mock<Response<CommandResponse<MockAggregate>>>();
            responseWrapper.SetupGet(a => a.Message)
                .Returns(response);

            var requestHandle = new Mock<RequestHandle<TCommand>>();
            requestHandle.Setup(a => a.GetResponse<CommandResponse<MockAggregate>>(It.IsAny<bool>()))
                .ReturnsAsync(responseWrapper.Object);

            var requestClient = new Mock<IRequestClient<TCommand>>();
            requestClient.Setup(a => a.Create(It.IsAny<TCommand>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .Returns(requestHandle.Object);

            return requestClient;

        }
    }
}
