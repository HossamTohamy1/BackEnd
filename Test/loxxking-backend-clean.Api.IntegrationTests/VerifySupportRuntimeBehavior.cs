using loxxking_backend_clean.Application.Features.Support.Commands.SendMessage;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Persistence;
using loxxking_backend_clean.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class VerifySupportRuntimeBehavior
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task Verify_SupportConversation_Create_And_AddMessage()
    {
        var db = GetDbContext();
        
        var sendHandler = new SendMessageHandler(db, new Mock<ISupportNotificationService>().Object);
        var sendCmd = new SendMessageCommand(
            Guid.Empty,
            "Hello, I need help.",
            Guid.Empty,
            "John Doe",
            false
        );
        
        var sendResult = await sendHandler.Handle(sendCmd, CancellationToken.None);
        Assert.True(sendResult.IsSuccess);

        var conversationId = sendResult.Value.ConversationId;

        var createdConversation = await db.SupportConversations.Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id == conversationId);
        Assert.NotNull(createdConversation);
        Assert.Single(createdConversation.Messages);
        Assert.Equal("Hello, I need help.", createdConversation.Messages.First().Message);
        Assert.Equal("John Doe", createdConversation.Messages.First().GuestName);
    }
}
