using loxxking_backend_clean.Application.Features.Notifications.Commands.MarkAsRead;
using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class VerifyTier2RuntimeBehavior
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys=OFF;");
        return context;
    }

    [Fact]
    public async Task Verify_Notification_Create_And_Validation()
    {
        var userId = Guid.NewGuid();
        var relatedId = Guid.NewGuid();
        
        var notification = Notification.Create(userId, NotificationType.SystemAlert, "Test message", relatedId);
        
        Assert.NotNull(notification);
        Assert.Equal(userId, notification.UserId);
        Assert.Equal(NotificationType.SystemAlert, notification.Type);
        Assert.Equal("Test message", notification.Message);
        Assert.Equal(relatedId, notification.RelatedEntityId);
        Assert.False(notification.IsRead);
        
        Assert.Throws<ArgumentException>(() => Notification.Create(Guid.Empty, NotificationType.SystemAlert, "Test"));
        Assert.Throws<ArgumentException>(() => Notification.Create(userId, NotificationType.SystemAlert, "   "));
    }

    [Fact]
    public async Task Verify_Notification_MarkAsRead_Handler()
    {
        using var db = GetDbContext();
        var handler = new MarkAsReadHandler(db);
        var notification = Notification.Create(Guid.NewGuid(), NotificationType.SystemAlert, "Test");
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        var command = new MarkAsReadCommand(notification.Id, notification.UserId);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Notifications.FirstAsync();
        Assert.True(updated.IsRead);
        Assert.True((DateTime.UtcNow - updated.UpdatedAt).Value.TotalSeconds < 5);
    }
}
