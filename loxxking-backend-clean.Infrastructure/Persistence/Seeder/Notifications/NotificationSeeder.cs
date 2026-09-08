using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Notifications;

public class NotificationSeeder : IDataSeeder
{
    public int Order => 11;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<NotificationSeeder>>();

        var customer = context.CustomerUser;
        var admin = context.AdminUser;

        if (customer != null)
        {
            const string custMsg = "تم تأكيد وشحن طلبك رقم ORD-2026-0001 بنجاح.";
            var exists = await dbContext.Notifications.AnyAsync(
                n => n.UserId == customer.Id && n.Message == custMsg, 
                cancellationToken);

            if (!exists)
            {
                var notif = Notification.Create(
                    userId: customer.Id,
                    type: NotificationType.OrderStatusChanged,
                    message: custMsg);

                await dbContext.Notifications.AddAsync(notif, cancellationToken);
                logger.LogInformation("Seeded customer notification.");
            }
        }

        if (admin != null)
        {
            const string adminMsg = "إشعار نظام: تم استلام إيصال تحويل بنكي جديد للطلب رقم ORD-2026-0002 بانتظار المراجعة.";
            var exists = await dbContext.Notifications.AnyAsync(
                n => n.UserId == admin.Id && n.Message == adminMsg, 
                cancellationToken);

            if (!exists)
            {
                var notif = Notification.Create(
                    userId: admin.Id,
                    type: NotificationType.BankTransferSubmitted,
                    message: adminMsg);

                await dbContext.Notifications.AddAsync(notif, cancellationToken);
                logger.LogInformation("Seeded admin notification.");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
