using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using KnightBus.Core;
using KnightBus.Host.DefaultMiddlewares;
using KnightBus.Messages;
using Moq;
using NUnit.Framework;

namespace KnightBus.Host.Tests.Unit.Middleware
{
    [TestFixture]
    public class AttachmentMiddlewareTests
    {
        [Test]
        public async Task Should_attach_attachment_when_command_have_attachment()
        {
            //arrange
            var message = new AttachmentCommand();
            var nextProcessor = new Mock<IMessageProcessor>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("this is a stream"));
            var attachment = new TestAttachment
            {
                ContentType = "text/plain",
                Filename = "test.txt",
                Stream = stream
            };
            var stateHandler = new Mock<IMessageStateHandler<AttachmentCommand>>();
            stateHandler.Setup(x => x.GetMessageAsync()).ReturnsAsync(message);
            stateHandler.Setup(x=> x.MessageProperties).Returns(new Dictionary<string, string>{ {AttachmentUtility.AttachmentKey, "89BDF3DB-896C-448D-A84E-872CBA8DBC9F" }});
            var attachmentProvider = new Mock<IMessageAttachmentProvider>();
            attachmentProvider.Setup(x => x.GetAttachmentAsync("attachmentcommand", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(attachment);
            var middleware = new AttachmentMiddleware(attachmentProvider.Object);
            //act
            await middleware.ProcessAsync(stateHandler.Object, nextProcessor.Object, CancellationToken.None);
            //assert
            stream.CanRead.Should().BeFalse("It should have been disposed");
            message.Attachment.Filename.Should().Be("test.txt");
            message.Attachment.ContentType.Should().Be("text/plain");
            nextProcessor.Verify(x=> x.ProcessAsync(stateHandler.Object, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Should_not_attach_attachments_for_other_commands()
        {
            //arrange
            var message = new TestCommand();
            var nextProcessor = new Mock<IMessageProcessor>();
            var stateHandler = new Mock<IMessageStateHandler<TestCommand>>();
            stateHandler.Setup(x => x.GetMessageAsync()).ReturnsAsync(message);
            var attachmentProvider = new Mock<IMessageAttachmentProvider>();
            attachmentProvider.Setup(x => x.GetAttachmentAsync("testcommand", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(IMessageAttachment));
            var middleware = new AttachmentMiddleware(attachmentProvider.Object);
            //act
            await middleware.ProcessAsync(stateHandler.Object, nextProcessor.Object, CancellationToken.None);
            //assert
            attachmentProvider.Verify(x=> x.GetAttachmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            nextProcessor.Verify(x => x.ProcessAsync(stateHandler.Object, CancellationToken.None), Times.Once);
        }

        private class TestAttachment : IMessageAttachment
        {
            public string Filename { get; set; }
            public string ContentType { get; set; }
            public long Length { get; }
            public Stream Stream { get; set; }
        }
    }
}