using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;
using loxxking_backend_clean.Domain.Entities.Users;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Authentication;
using loxxking_backend_clean.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

/// <summary>
/// Simulates the exact production migration pre-flight checklist against a
/// disposable PostgreSQL Testcontainer. Each test spins up its own fresh
/// container so there is zero shared state.
///
/// PURPOSE: If any of these tests need to be skipped or deleted in the future,
/// that is a signal that a critical safety check has been removed â€” investigate
/// before doing so.
/// </summary>
[Collection("PostgresSequential")]
public class VerifyProductionMigrationChecklist : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private ServiceProvider BuildDiProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IPasswordHasher<User>, LegacyBCryptPasswordHasher>();

        var mockJwtProvider = new Mock<IJwtProvider>();
        mockJwtProvider.Setup(x => x.Generate(It.IsAny<User>())).Returns("mock-jwt-token");
        services.AddSingleton(mockJwtProvider.Object);

        services.AddLogging();

        return services.BuildServiceProvider();
    }

    private async Task MigrateUpTo(string connectionString, string? targetMigration)
    {
        using var provider = BuildDiProvider(connectionString);
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var migrator = ((IInfrastructure<IServiceProvider>)ctx).Instance.GetRequiredService<IMigrator>();
        await migrator.MigrateAsync(targetMigration);
    }

    private const string PreIdentityMigration = "20260904023128_Phase3_Tier4";
    private const string IdentityMigration = "20260904033624_IdentityOptionA";

    [Fact]
    public async Task Test_DuplicateEmailDetection_BlocksUnsafeMigration()
    {
        var connStr = _container.GetConnectionString();

        await MigrateUpTo(connStr, PreIdentityMigration);

        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync();

            var countryId = Guid.NewGuid();
            var hash = BCrypt.Net.BCrypt.HashPassword("Password123!");

            await using var cmd = new NpgsqlCommand($@"
                INSERT INTO ""Countries"" (""Id"", ""Name"", ""Currency"", ""DefaultLanguage"", ""IsDefault"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{countryId}', 'TestCountry', 'USD', 'en', true, NOW(), true, false);

                INSERT INTO ""Users"" (""Id"", ""Name"", ""Email"", ""Phone"", ""PasswordHash"", ""CountryId"", ""Role"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{Guid.NewGuid()}', 'User One', 'dup@test.com', '+111', '{hash}', '{countryId}', 0, NOW(), true, false);

                INSERT INTO ""Users"" (""Id"", ""Name"", ""Email"", ""Phone"", ""PasswordHash"", ""CountryId"", ""Role"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{Guid.NewGuid()}', 'User Two', 'DUP@test.com', '+222', '{hash}', '{countryId}', 0, NOW(), true, false);
            ", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        var ex = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await MigrateUpTo(connStr, IdentityMigration);
        });

        var fullMsg = $"{ex.Message} | Inner: {ex.InnerException?.Message}";
        Assert.True(
            fullMsg.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
            fullMsg.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
            fullMsg.Contains("UserNameIndex", StringComparison.OrdinalIgnoreCase) ||
            fullMsg.Contains("23505", StringComparison.OrdinalIgnoreCase), // PostgreSQL unique_violation error code
            $"Expected unique index violation but got: {fullMsg}");
    }

    [Fact]
    public async Task Test_CleanMigration_AppliesSuccessfully()
    {
        var connStr = _container.GetConnectionString();

        await MigrateUpTo(connStr, PreIdentityMigration);

        var countryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var favoriteId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var bcryptHash = BCrypt.Net.BCrypt.HashPassword("TestPass123!");

        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand($@"
                INSERT INTO ""Countries"" (""Id"", ""Name"", ""Currency"", ""DefaultLanguage"", ""IsDefault"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{countryId}', 'TestCountry', 'USD', 'en', true, NOW(), true, false);

                INSERT INTO ""Users"" (""Id"", ""Name"", ""Email"", ""Phone"", ""PasswordHash"", ""CountryId"", ""Role"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{userId}', 'Admin User', 'admin@test.com', '+1234567890', '{bcryptHash}', '{countryId}', 0, NOW(), true, false);

                INSERT INTO ""Categories"" (""Id"", ""NameEn"", ""NameAr"", ""Slug"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{categoryId}', 'TestCat', 'ØªØ³Øª', 'test-cat', NOW(), true, false);

                INSERT INTO ""Products"" (""Id"", ""NameEn"", ""NameAr"", ""Description"", ""Images"", ""Colors"", ""Sizes"", ""BasePrice"", ""CategoryId"", ""Slug"", ""Stock"", ""Rating"", ""ReviewCount"", ""IsNew"", ""IsBestSeller"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{productId}', 'TestProd', 'Ù…Ù†ØªØ¬', 'test description', '{{}}', '{{}}', '{{}}', 10.00, '{categoryId}', 'test-prod', 100, 0, 0, false, false, NOW(), true, false);

                INSERT INTO ""Orders"" (""Id"", ""OrderNumber"", ""CustomerId"", ""CountryId"", ""Status"", ""PaymentMethod"", ""PaymentStatus"", ""Address"", ""Phone"", ""TotalAmount"", ""City"", ""Area"", ""DeliveryCompany"", ""Subtotal"", ""Shipping"", ""Discount"", ""ViewCount"", ""ProcessedCount"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{orderId}', 'ORD-001', '{userId}', '{countryId}', 0, 0, 0, '123 Main St', '+1234567890', 10.00, 'NYC', 'Manhattan', 'FedEx', 10.00, 0.00, 0.00, 0, 0, NOW(), true, false);

                INSERT INTO ""Reviews"" (""Id"", ""ProductId"", ""UserId"", ""Rating"", ""Comment"", ""Status"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{reviewId}', '{productId}', '{userId}', 5, 'Great product', 0, NOW(), true, false);

                INSERT INTO ""FavoriteItems"" (""Id"", ""UserId"", ""ProductId"", ""AddedAt"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{favoriteId}', '{userId}', '{productId}', NOW(), NOW(), true, false);

                INSERT INTO ""Notifications"" (""Id"", ""UserId"", ""Type"", ""Message"", ""IsRead"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{notificationId}', '{userId}', 0, 'Welcome!', false, NOW(), true, false);
            ", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        await MigrateUpTo(connStr, IdentityMigration);

        using var provider = BuildDiProvider(connStr);
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Id == userId);
        Assert.Equal("Admin User", user.Name);
        Assert.Equal("admin@test.com", user.Email);
        Assert.Equal("+1234567890", user.PhoneNumber);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.True(user.PasswordHash!.StartsWith("$2a$") || user.PasswordHash.StartsWith("$2b$"));

        Assert.Equal("ADMIN@TEST.COM", user.NormalizedEmail);
        Assert.Equal("ADMIN@TEST.COM", user.NormalizedUserName);
        Assert.NotNull(user.SecurityStamp);
        Assert.NotNull(user.ConcurrencyStamp);

        var order = await dbContext.Orders.FirstAsync(o => o.Id == orderId);
        Assert.Equal(userId, order.CustomerId);

        var review = await dbContext.Reviews.FirstAsync(r => r.Id == reviewId);
        Assert.Equal(userId, review.UserId);

        var favorite = await dbContext.FavoriteItems.FirstAsync(f => f.Id == favoriteId);
        Assert.Equal(userId, favorite.UserId);

        var notification = await dbContext.Notifications.FirstAsync(n => n.Id == notificationId);
        Assert.Equal(userId, notification.UserId);
    }

    [Fact]
    public async Task Test_LegacyBCryptUser_CanLoginAndGetsRehashedToPBKDF2()
    {
        var connStr = _container.GetConnectionString();

        await MigrateUpTo(connStr, null);

        var password = "SecurePass123!";
        var legacyHash = BCrypt.Net.BCrypt.HashPassword(password);
        var userId = Guid.NewGuid();

        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync();
            var countryId = Guid.NewGuid();
            await using var cmd = new NpgsqlCommand($@"
                INSERT INTO ""Countries"" (""Id"", ""Name"", ""Currency"", ""DefaultLanguage"", ""IsDefault"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{countryId}', 'TestCountry', 'USD', 'en', true, NOW(), true, false);

                INSERT INTO ""Users"" (""Id"", ""Name"", ""Email"", ""UserName"", ""NormalizedEmail"", ""NormalizedUserName"",
                    ""Phone"", ""PasswordHash"", ""CountryId"", ""Role"", ""CreatedAt"", ""IsActive"", ""IsDeleted"",
                    ""SecurityStamp"", ""ConcurrencyStamp"", ""EmailConfirmed"", ""PhoneNumberConfirmed"",
                    ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"")
                VALUES ('{userId}', 'BCrypt User', 'bcrypt@test.com', 'bcrypt@test.com', 'BCRYPT@TEST.COM', 'BCRYPT@TEST.COM',
                    '+9876543210', '{legacyHash}', '{countryId}', 0, NOW(), true, false,
                    '{Guid.NewGuid()}', '{Guid.NewGuid()}', true, true,
                    false, false, 0);
            ", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        using var provider = BuildDiProvider(connStr);
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();

        var handler = new LoginUserHandler(dbContext, jwtProvider, userManager);
        var result = await handler.Handle(new LoginUserQuery("bcrypt@test.com", password), CancellationToken.None);

        Assert.True(result.IsSuccess, $"Login failed: {result.Error}");
        Assert.Equal("mock-jwt-token", result.Value.Token);

        var updatedUser = await dbContext.Users.FirstAsync(u => u.Id == userId);
        Assert.True(
            updatedUser.PasswordHash!.StartsWith("AQAAAA"),
            $"Expected PBKDF2 hash but got: {updatedUser.PasswordHash[..Math.Min(20, updatedUser.PasswordHash.Length)]}...");

        var verifyResult = userManager.PasswordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, password);
        Assert.Equal(PasswordVerificationResult.Success, verifyResult);
    }

    [Fact]
    public async Task Test_NewUserRegistration_ThroughIdentityPipeline()
    {
        var connStr = _container.GetConnectionString();
        await MigrateUpTo(connStr, null);

        var countryId = Guid.NewGuid();
        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand($@"
                INSERT INTO ""Countries"" (""Id"", ""Name"", ""Currency"", ""DefaultLanguage"", ""IsDefault"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{countryId}', 'TestCountry', 'USD', 'en', true, NOW(), true, false);
            ", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        using var provider = BuildDiProvider(connStr);
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = User.Create("New Employee", "employee@test.com", "+5555555555", "#PENDING_HASH#", countryId, UserRole.SalesEmployee);
        var createResult = await userManager.CreateAsync(user, "NewSecurePass1!");

        Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var dbUser = await dbContext.Users.FirstAsync(u => u.Email == "employee@test.com");
        Assert.Equal("New Employee", dbUser.Name);
        Assert.Equal(UserRole.SalesEmployee, dbUser.Role);

        Assert.NotEqual("#PENDING_HASH#", dbUser.PasswordHash);

        Assert.True(await userManager.CheckPasswordAsync(dbUser, "NewSecurePass1!"));
        Assert.False(await userManager.CheckPasswordAsync(dbUser, "WrongPassword"));
    }

    [Fact]
    public async Task Test_RoleBasedAuthorization_StillWorksAfterMigration()
    {
        var connStr = _container.GetConnectionString();
        await MigrateUpTo(connStr, null);

        var countryId = Guid.NewGuid();
        var hash = BCrypt.Net.BCrypt.HashPassword("AdminPass1!");
        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand($@"
                INSERT INTO ""Countries"" (""Id"", ""Name"", ""Currency"", ""DefaultLanguage"", ""IsDefault"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
                VALUES ('{countryId}', 'TestCountry', 'USD', 'en', true, NOW(), true, false);

                INSERT INTO ""Users"" (""Id"", ""Name"", ""Email"", ""UserName"", ""NormalizedEmail"", ""NormalizedUserName"",
                    ""Phone"", ""PasswordHash"", ""CountryId"", ""Role"", ""CreatedAt"", ""IsActive"", ""IsDeleted"",
                    ""SecurityStamp"", ""ConcurrencyStamp"", ""EmailConfirmed"", ""PhoneNumberConfirmed"",
                    ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"")
                VALUES ('{Guid.NewGuid()}', 'Admin User', 'admin@test.com', 'admin@test.com', 'ADMIN@TEST.COM', 'ADMIN@TEST.COM',
                    '+1111111111', '{hash}', '{countryId}', 0, NOW(), true, false,
                    '{Guid.NewGuid()}', '{Guid.NewGuid()}', true, true,
                    false, false, 0);
            ", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        using var provider = BuildDiProvider(connStr);
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Email == "admin@test.com");
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal("Admin", user.Role.ToString());

        var configManager = new Microsoft.Extensions.Configuration.ConfigurationManager();
        configManager["Jwt:Secret"] = "SuperSecretKeyThatIsLongEnoughForHS256Algorithm!!";
        configManager["Jwt:Issuer"] = "TestIssuer";
        configManager["Jwt:Audience"] = "TestAudience";
        Microsoft.Extensions.Configuration.IConfiguration config = configManager;

        var realJwtProvider = new JwtProvider(config);
        var token = realJwtProvider.Generate(user);

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value); // PascalCase, NOT lowercase
    }
}

/// <summary>
/// Ensures Testcontainer-based tests run sequentially to avoid port conflicts.
/// </summary>
[CollectionDefinition("PostgresSequential")]
public class PostgresSequentialCollection : ICollectionFixture<PostgresSequentialFixture> { }

public class PostgresSequentialFixture { }
