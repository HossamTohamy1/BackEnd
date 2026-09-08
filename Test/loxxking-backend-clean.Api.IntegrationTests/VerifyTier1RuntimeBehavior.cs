
using loxxking_backend_clean.Application.Features.SiteVisits.Commands.LogVisit;
using loxxking_backend_clean.Application.Features.Favorites.Commands.AddFavorite;
using loxxking_backend_clean.Domain.Entities.SiteVisits;
using loxxking_backend_clean.Domain.Entities.Favorites;
using loxxking_backend_clean.Domain.Entities.Countries;
using loxxking_backend_clean.Domain.Entities.Products;
using loxxking_backend_clean.Domain.Entities.Categories;
using loxxking_backend_clean.Domain.ValueObjects;
using loxxking_backend_clean.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class VerifyTier1RuntimeBehavior
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
    public async Task Verify_SiteVisit_Create_And_Validation()
    {
        using var db = GetDbContext();
        var handler = new LogVisitHandler(db);

        var country = Country.Create("TestCountry", "USD");
        db.Countries.Add(country);
        await db.SaveChangesAsync();

        var validCommand = new LogVisitCommand(country.Id, "/home");
        var invalidCommand = new LogVisitCommand(Guid.Empty, "/home");

        var invalidResult = await handler.Handle(invalidCommand, CancellationToken.None);

        var validResult = await handler.Handle(validCommand, CancellationToken.None);

        Assert.False(invalidResult.IsSuccess);

        Assert.True(validResult.IsSuccess);
        var visit = await db.SiteVisits.FirstOrDefaultAsync(v => v.CountryId == country.Id);
        Assert.NotNull(visit);
        Assert.Equal("/home", visit.Page);
        Assert.True((DateTime.UtcNow - visit.VisitedAt).TotalSeconds < 5);
    }

    [Fact]
    public async Task Verify_FavoriteItem_Create_And_Validation()
    {
        using var db = GetDbContext();
        var handler = new AddFavoriteHandler(db);

        var category = Category.Create("TestAr", "TestEn", "test", "url");
        db.Categories.Add(category);
        var product = Product.Create(
            category.Id, 
            "ProdAr", 
            "ProdEn", 
            "Desc", 
            "slug", 
            Money.FromDecimal(100), 
            null, 
            new List<string>(), 
            new List<string>(), 
            new List<string>());
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var validUserCommand = new AddFavoriteCommand(product.Id, Guid.NewGuid(), null);
        var validGuestCommand = new AddFavoriteCommand(product.Id, Guid.Empty, "guest-123");
        var invalidCommand = new AddFavoriteCommand(Guid.Empty, Guid.NewGuid(), null);

        var invalidResult = await handler.Handle(invalidCommand, CancellationToken.None);

        var validUserResult = await handler.Handle(validUserCommand, CancellationToken.None);

        var validGuestResult = await handler.Handle(validGuestCommand, CancellationToken.None);

        Assert.False(invalidResult.IsSuccess);

        Assert.True(validUserResult.IsSuccess);
        var userFavorite = await db.FavoriteItems.FirstOrDefaultAsync(f => f.Id == validUserResult.Value.FavoriteItemId);
        Assert.NotNull(userFavorite);
        Assert.Equal(validUserCommand.UserId, userFavorite.UserId);
        Assert.Null(userFavorite.GuestId);

        Assert.True(validGuestResult.IsSuccess);
        var guestFavorite = await db.FavoriteItems.FirstOrDefaultAsync(f => f.Id == validGuestResult.Value.FavoriteItemId);
        Assert.NotNull(guestFavorite);
        Assert.Equal("guest-123", guestFavorite.GuestId);
        Assert.Null(guestFavorite.UserId);
    }
}
