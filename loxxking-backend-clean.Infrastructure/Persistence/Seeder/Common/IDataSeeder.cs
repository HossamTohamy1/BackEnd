namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}
