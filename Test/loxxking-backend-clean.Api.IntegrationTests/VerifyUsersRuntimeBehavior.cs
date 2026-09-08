using loxxking_backend_clean.Application.Features.Users.Commands.CreateStoreManager;
using loxxking_backend_clean.Application.Features.Users.Commands.CreateSalesEmployee;
using loxxking_backend_clean.Domain.Entities.Countries;
using loxxking_backend_clean.Domain.Entities.Users;
using loxxking_backend_clean.Infrastructure.Persistence;
using loxxking_backend_clean.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Xunit;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Application.Features.Users.Commands.UpdateStaff;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class VerifyUsersRuntimeBehavior
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
        services.AddScoped<CreateStoreManagerHandler>();
        
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Verify_User_Create_Update_Validation()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        
        var country = Country.Create("TestCountry", "USD", "en", true);
        db.Countries.Add(country);
        await db.SaveChangesAsync();

        var createHandler = scope.ServiceProvider.GetRequiredService<CreateStoreManagerHandler>();
        var createCmd = new CreateStoreManagerCommand("John Doe", "john@test.com", "+1234567890", "Password123!");
        
        var createResult = await createHandler.Handle(createCmd, CancellationToken.None);
        Assert.True(createResult.IsSuccess);

        var createdUser = await db.Users.FindAsync(createResult.Value);
        Assert.NotNull(createdUser);
        Assert.Equal("John Doe", createdUser.Name);
        Assert.Equal("john@test.com", createdUser.Email);
        Assert.Equal("+1234567890", createdUser.Phone);
        Assert.Equal(UserRole.StoreManager, createdUser.Role);
        Assert.Equal(country.Id, createdUser.CountryId);

        var badCreateCmd = new CreateStoreManagerCommand("", "jane@test.com", "+1234567890", "Pass!");
        await Assert.ThrowsAsync<ArgumentException>(() => createHandler.Handle(badCreateCmd, CancellationToken.None));

        var updateHandler = new UpdateStaffHandler(db);
        var badUpdateCmd = new UpdateStaffCommand(createResult.Value, "Jane Doe", ""); // Empty phone
        await Assert.ThrowsAsync<ArgumentException>(() => updateHandler.Handle(badUpdateCmd, CancellationToken.None));

        var validUpdateCmd = new UpdateStaffCommand(createResult.Value, "John Updated", "+0987654321");
        var updateResult = await updateHandler.Handle(validUpdateCmd, CancellationToken.None);
        Assert.True(updateResult.IsSuccess);

        var updatedUser = await db.Users.FindAsync(createResult.Value);
        Assert.Equal("John Updated", updatedUser!.Name);
        Assert.Equal("+0987654321", updatedUser.Phone);
    }
}
