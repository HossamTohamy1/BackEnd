using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;
using loxxking_backend_clean.Domain.Entities.Countries;
using loxxking_backend_clean.Domain.Entities.Users;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class VerifyIdentityMigrationBackwardCompatibility
{
    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));
            
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        
        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();
        
        services.AddScoped<IPasswordHasher<User>, loxxking_backend_clean.Infrastructure.Authentication.LegacyBCryptPasswordHasher>();
        
        var mockJwtProvider = new Mock<IJwtProvider>();
        mockJwtProvider.Setup(x => x.Generate(It.IsAny<User>())).Returns("mock-token");
        services.AddSingleton(mockJwtProvider.Object);
        
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Verify_LegacyBCryptHash_LogsIn_And_HandlesRehash()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();

        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        var country = Country.Create("Test", "USD", "en", true);
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var plaintextPassword = "SecretPassword123!";
        var legacyBcryptHash = BCrypt.Net.BCrypt.HashPassword(plaintextPassword);

        var user = User.Create(
            "Legacy User",
            "legacy@example.com",
            "1234567890",
            legacyBcryptHash,
            country.Id,
            UserRole.SalesEmployee
        );

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loginHandler = new LoginUserHandler(context, jwtProvider, userManager);
        var loginResult = await loginHandler.Handle(new LoginUserQuery("legacy@example.com", plaintextPassword), CancellationToken.None);
        Assert.True(loginResult.IsSuccess);
        Assert.NotNull(loginResult.Value.Token);

        var updatedUser = await context.Users.FirstAsync(u => u.Id == user.Id);
        
        var verificationResult = userManager.PasswordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordHash, plaintextPassword);
        Assert.NotEqual(PasswordVerificationResult.Failed, verificationResult);
    }
}
