using loxxking_backend_clean.Domain.Entities.Support;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Support;

public class SupportSeeder : IDataSeeder
{
    public int Order => 12;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<SupportSeeder>>();

        var customer = context.CustomerUser;
        var salesEmployee = context.SalesEmployeeUser;

        const string orderNumber = "ORD-2026-0001";
        var exists = await dbContext.SupportConversations.AnyAsync(c => c.OrderNumber == orderNumber, cancellationToken);
        if (!exists)
        {
            var conversation = SupportConversation.Create(
                orderNumber: orderNumber,
                customerName: customer.Name,
                customerPhone: customer.Phone,
                customerEmail: customer.Email);

            conversation.AssignTo(salesEmployee.Id);

            conversation.AddMessage(
                senderId: customer.Id,
                recipientId: salesEmployee.Id,
                message: "مرحباً، أود التأكد من موعد وصول المندوب غداً لتسليم الطلب؟",
                attachmentUrl: null,
                relatedOrderId: null,
                relatedReviewId: null,
                guestName: null);

            conversation.AddMessage(
                senderId: salesEmployee.Id,
                recipientId: customer.Id,
                message: "أهلاً بك يا فندم! المندوب سيتواصل معك هاتفيًا غداً بين الساعة 2 ظهراً و 5 مساءً.",
                attachmentUrl: null,
                relatedOrderId: null,
                relatedReviewId: null,
                guestName: null);

            await dbContext.SupportConversations.AddAsync(conversation, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded SupportConversation for order '{OrderNumber}' with messages.", orderNumber);
        }
    }
}
