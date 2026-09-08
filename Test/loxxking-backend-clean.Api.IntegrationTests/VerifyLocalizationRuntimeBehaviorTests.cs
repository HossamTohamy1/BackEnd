using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using loxxking_backend_clean.Application.Features.Reviews.Commands.CreateReview;
using loxxking_backend_clean.Application.Features.Support.Commands.SendMessage;
using loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;
using loxxking_backend_clean.Infrastructure.Persistence;
using loxxking_backend_clean.Shared;
using loxxking_backend_clean.Shared.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

[Collection("PostgresSequential")]
public class VerifyLocalizationRuntimeBehaviorTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
        await _container.StartAsync();

        var connStr = _container.GetConnectionString();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connStr,
                    ["Jwt:Secret"] = "SuperSecretKeyThatIsLongEnoughForHS256Algorithm!!",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience"
                };
                config.AddInMemoryCollection(dict);
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory != null) await _factory.DisposeAsync();
        if (_container != null) await _container.DisposeAsync();
    }



    [Fact]
    public async Task Endpoint1_LoginUser_ReturnsLocalizedResponses_InEnglishAndArabic()
    {
        var enReq = new HttpRequestMessage(HttpMethod.Post, "/api/users/login");
        enReq.Headers.Add("Accept-Language", "en");
        enReq.Content = JsonContent.Create(new LoginUserQuery("invalid_user@test.com", "WrongPassword123!"));
        var enResp = await _client.SendAsync(enReq);
        var enContent = await enResp.Content.ReadAsStringAsync();

        var arReq = new HttpRequestMessage(HttpMethod.Post, "/api/users/login");
        arReq.Headers.Add("Accept-Language", "ar");
        arReq.Content = JsonContent.Create(new LoginUserQuery("invalid_user@test.com", "WrongPassword123!"));
        var arResp = await _client.SendAsync(arReq);
        var arContent = await arResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, enResp.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, arResp.StatusCode);

        Assert.NotEqual(enContent, arContent);

        Assert.Contains("Invalid credentials", enContent);

        Assert.Matches(@"[\u0600-\u06FF]", arContent);
        Assert.Contains("Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ø§Ø¹ØªÙ…Ø§Ø¯ ØºÙŠØ± ØµØ§Ù„Ø­Ø©", arContent);
    }

    [Fact]
    public async Task Endpoint2_GetProductById_ReturnsLocalizedNotFound_InEnglishAndArabic()
    {
        var nonExistentId = Guid.NewGuid();

        var enReq = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{nonExistentId}");
        enReq.Headers.Add("Accept-Language", "en");
        var enResp = await _client.SendAsync(enReq);
        var enContent = await enResp.Content.ReadAsStringAsync();

        var arReq = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{nonExistentId}");
        arReq.Headers.Add("Accept-Language", "ar");
        var arResp = await _client.SendAsync(arReq);
        var arContent = await arResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, enResp.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, arResp.StatusCode);

        Assert.NotEqual(enContent, arContent);

        Assert.Contains("Product not found", enContent);

        Assert.Matches(@"[\u0600-\u06FF]", arContent);
        Assert.Contains("Ø§Ù„Ù…Ù†ØªØ¬ ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯", arContent);
    }

    [Fact]
    public async Task Endpoint3_CreateReview_ReturnsLocalizedValidationError_InEnglishAndArabic()
    {
        var invalidReview = new CreateReviewCommand(
            ProductId: Guid.NewGuid(),
            Rating: 10,
            Comment: "Great product",
            GuestName: "Guest User",
            UserId: Guid.Empty
        );

        var enReq = new HttpRequestMessage(HttpMethod.Post, "/api/reviews");
        enReq.Headers.Add("Accept-Language", "en");
        enReq.Content = JsonContent.Create(invalidReview);
        var enResp = await _client.SendAsync(enReq);
        var enContent = await enResp.Content.ReadAsStringAsync();

        var arReq = new HttpRequestMessage(HttpMethod.Post, "/api/reviews");
        arReq.Headers.Add("Accept-Language", "ar");
        arReq.Content = JsonContent.Create(invalidReview);
        var arResp = await _client.SendAsync(arReq);
        var arContent = await arResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, enResp.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, arResp.StatusCode);

        Assert.NotEqual(enContent, arContent);

        Assert.Contains("Rating must be between 1 and 5", enContent);

        Assert.Matches(@"[\u0600-\u06FF]", arContent);
        Assert.Contains("ÙŠØ¬Ø¨ Ø£Ù† ÙŠÙƒÙˆÙ† Ø§Ù„ØªÙ‚ÙŠÙŠÙ… Ø¨ÙŠÙ† 1 Ùˆ 5", arContent);
    }

    [Fact]
    public async Task Endpoint4_SupportChatSend_ReturnsLocalizedEmptyMessageError_InEnglishAndArabic()
    {
        var invalidCmd = new SendMessageCommand(Guid.NewGuid(), "", Guid.NewGuid(), "Guest", false);

        var enReq = new HttpRequestMessage(HttpMethod.Post, "/api/support-chat/send");
        enReq.Headers.Add("Accept-Language", "en");
        enReq.Content = JsonContent.Create(invalidCmd);
        var enResp = await _client.SendAsync(enReq);
        var enContent = await enResp.Content.ReadAsStringAsync();

        var arReq = new HttpRequestMessage(HttpMethod.Post, "/api/support-chat/send");
        arReq.Headers.Add("Accept-Language", "ar");
        arReq.Content = JsonContent.Create(invalidCmd);
        var arResp = await _client.SendAsync(arReq);
        var arContent = await arResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, enResp.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, arResp.StatusCode);

        Assert.NotEqual(enContent, arContent);

        Assert.Contains("Message cannot be empty", enContent);

        Assert.Matches(@"[\u0600-\u06FF]", arContent);
        Assert.Contains("Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø£Ù† ØªÙƒÙˆÙ† Ø§Ù„Ø±Ø³Ø§Ù„Ø© ÙØ§Ø±ØºØ©", arContent);
    }

    [Fact]
    public void VerifyAllResourceKeys_HaveHighQualityTranslations_AndValidPlaceholders()
    {
        using var scope = _factory.Services.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>>();

        var enCulture = new System.Globalization.CultureInfo("en");
        var arCulture = new System.Globalization.CultureInfo("ar");

        var assembly = typeof(SharedResource).Assembly;
        var rm = new System.Resources.ResourceManager("loxxking_backend_clean.Shared.Resources.SharedResource", assembly);

        var enResourceSet = rm.GetResourceSet(enCulture, true, true);
        var arResourceSet = rm.GetResourceSet(arCulture, true, true);

        Assert.NotNull(enResourceSet);
        Assert.NotNull(arResourceSet);

        var enDict = new Dictionary<string, string>();
        foreach (System.Collections.DictionaryEntry entry in enResourceSet)
        {
            enDict[entry.Key.ToString()!] = entry.Value?.ToString() ?? string.Empty;
        }

        var arDict = new Dictionary<string, string>();
        foreach (System.Collections.DictionaryEntry entry in arResourceSet)
        {
            arDict[entry.Key.ToString()!] = entry.Value?.ToString() ?? string.Empty;
        }

        Assert.True(enDict.Count >= 120, $"Expected at least 120 keys, found {enDict.Count}");
        Assert.Equal(enDict.Count, arDict.Count);

        foreach (var (key, enVal) in enDict)
        {
            Assert.True(arDict.ContainsKey(key), $"Key '{key}' is missing in Arabic resources");
            var arVal = arDict[key];

            Assert.False(string.IsNullOrWhiteSpace(enVal), $"Key '{key}' has empty English value");
            Assert.False(string.IsNullOrWhiteSpace(arVal), $"Key '{key}' has empty Arabic value");

            Assert.Matches(@"[\u0600-\u06FF]", arVal);

            var enPlaceholders = Regex.Matches(enVal, @"\{(\d+)\}")
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var arPlaceholders = Regex.Matches(arVal, @"\{(\d+)\}")
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(enPlaceholders, arPlaceholders);

            if (enPlaceholders.Count > 0)
            {
                var dummyArgs = enPlaceholders.Select(p => (object)$"TestVal_{p}").ToArray();
                var formattedEn = string.Format(enCulture, enVal, dummyArgs);
                var formattedAr = string.Format(arCulture, arVal, dummyArgs);

                Assert.False(string.IsNullOrWhiteSpace(formattedEn));
                Assert.False(string.IsNullOrWhiteSpace(formattedAr));
                Assert.Matches(@"[\u0600-\u06FF]", formattedAr);
            }

            System.Threading.Thread.CurrentThread.CurrentUICulture = enCulture;
            var locEn = localizer[key];
            Assert.False(locEn.ResourceNotFound, $"IStringLocalizer failed to resolve key '{key}' in culture 'en'");
            Assert.Equal(enVal, locEn.Value);

            System.Threading.Thread.CurrentThread.CurrentUICulture = arCulture;
            var locAr = localizer[key];
            Assert.False(locAr.ResourceNotFound, $"IStringLocalizer failed to resolve key '{key}' in culture 'ar'");
            Assert.Equal(arVal, locAr.Value);
        }
    }

    [Fact]
    public async Task VerifyFavoritesPageConfig_WhenNoDbConfigSeeded_ReturnsLocalizedFallbackContent()
    {
        var enReq = new HttpRequestMessage(HttpMethod.Get, "/api/favorites-config");
        enReq.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
        var enResp = await _client.SendAsync(enReq);
        var enJson = await enResp.Content.ReadAsStringAsync();

        var arReq = new HttpRequestMessage(HttpMethod.Get, "/api/favorites-config");
        arReq.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ar"));
        var arResp = await _client.SendAsync(arReq);
        var arJson = await arResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, enResp.StatusCode);
        Assert.Equal(HttpStatusCode.OK, arResp.StatusCode);
        Assert.NotEqual(enJson, arJson);

        Assert.Contains("Favorites", enJson);
        Assert.Contains("List of products you liked", enJson);
        Assert.Contains("Add all products to cart", enJson);
        Assert.Contains("Favorites list is empty", enJson);
        Assert.Contains("You haven't added any products to your favorites list yet", enJson);
        Assert.Contains("Explore Shopping", enJson);

        Assert.Contains("Ø§Ù„Ù…ÙØ¶Ù„Ø©", arJson);
        Assert.Contains("Ù‚Ø§Ø¦Ù…Ø© Ø¨Ø§Ù„Ù…Ù†ØªØ¬Ø§Øª Ø§Ù„ØªÙŠ ØªÙ… Ø§Ù„Ø¥Ø¹Ø¬Ø§Ø¨ Ø¨Ù‡Ø§", arJson);
        Assert.Contains("Ø¥Ø¶Ø§ÙØ© Ø¬Ù…ÙŠØ¹ Ø§Ù„Ù…Ù†ØªØ¬Ø§Øª Ø¥Ù„Ù‰ Ø§Ù„Ø³Ù„Ø©", arJson);
        Assert.Contains("Ù‚Ø§Ø¦Ù…Ø© Ø§Ù„Ù…ÙØ¶Ù„Ø© ÙØ§Ø±ØºØ©", arJson);
        Assert.Contains("Ù„Ù… ØªÙ‚Ù… Ø¨Ø¥Ø¶Ø§ÙØ© Ø£ÙŠ Ù…Ù†ØªØ¬ Ø¥Ù„Ù‰ Ù‚Ø§Ø¦Ù…Ø© Ø§Ù„Ù…ÙØ¶Ù„Ø© Ø¨Ø¹Ø¯", arJson);
        Assert.Contains("Ø§ÙƒØªØ´Ù Ø§Ù„ØªØ³ÙˆÙ‚", arJson);
    }

    [Fact]
    public async Task VerifyOffersPageConfig_WhenNoDbConfigSeeded_ReturnsLocalizedFallbackContent()
    {
        var enReq = new HttpRequestMessage(HttpMethod.Get, "/api/offers-page-config");
        enReq.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
        var enResp = await _client.SendAsync(enReq);
        var enJson = await enResp.Content.ReadAsStringAsync();

        var arReq = new HttpRequestMessage(HttpMethod.Get, "/api/offers-page-config");
        arReq.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ar"));
        var arResp = await _client.SendAsync(arReq);
        var arJson = await arResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, enResp.StatusCode);
        Assert.Equal(HttpStatusCode.OK, arResp.StatusCode);
        Assert.NotEqual(enJson, arJson);

        Assert.Contains("Special Offers", enJson);
        Assert.Contains("Best prices for a limited time", enJson);
        Assert.Contains("Current Discounts", enJson);
        Assert.Contains("Save more with bundles", enJson);
        Assert.Contains("Choose the bundle that best suits you at discounted prices", enJson);

        Assert.Contains("Ø¹Ø±ÙˆØ¶ Ø®Ø§ØµØ©", arJson);
        Assert.Contains("Ø£ÙØ¶Ù„ Ø§Ù„Ø£Ø³Ø¹Ø§Ø± Ù„ÙØªØ±Ø© Ù…Ø­Ø¯ÙˆØ¯Ø©", arJson);
        Assert.Contains("Ø§Ù„ØªØ®ÙÙŠØ¶Ø§Øª Ø§Ù„Ø­Ø§Ù„ÙŠØ©", arJson);
        Assert.Contains("ÙˆÙØ± Ø£ÙƒØ«Ø± Ù…Ø¹ Ø§Ù„Ø¨Ø§Ù‚Ø§Øª", arJson);
        Assert.Contains("Ø§Ø®ØªØ§Ø± Ø§Ù„Ø¨Ø§Ù‚Ø© Ø§Ù„Ø£Ù†Ø³Ø¨ Ù„Ùƒ Ø¨Ø£Ø³Ø¹Ø§Ø± Ù…Ø®ÙØ¶Ø©", arJson);
    }

    [Fact]
    public async Task VerifyAddFavorite_WhenSuccess_ReturnsLocalizedMessage()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cat = loxxking_backend_clean.Domain.Entities.Categories.Category.Create("CatAr", "CatEn", $"cat-fav-{Guid.NewGuid():N}", "");
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        var prod = loxxking_backend_clean.Domain.Entities.Products.Product.Create(
            cat.Id, "ProdAr", "ProdEn", "Desc", $"slug-fav-{Guid.NewGuid():N}",
            loxxking_backend_clean.Domain.ValueObjects.Money.FromDecimal(50),
            null, new List<string>(), new List<string>(), new List<string>(),
            null, null, null, null, false, false, null);
        db.Products.Add(prod);
        await db.SaveChangesAsync();

        var guestId = Guid.NewGuid().ToString();

        var enReq = new HttpRequestMessage(HttpMethod.Post, $"/api/favorites/{prod.Id}");
        enReq.Headers.Add("X-Guest-Id", guestId);
        enReq.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
        var enResp = await _client.SendAsync(enReq);
        var enJson = await enResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, enResp.StatusCode);
        Assert.Contains("Added to Favorite", enJson);

        var arReq = new HttpRequestMessage(HttpMethod.Post, $"/api/favorites/{prod.Id}");
        arReq.Headers.Add("X-Guest-Id", guestId);
        arReq.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ar"));
        var arResp = await _client.SendAsync(arReq);
        var arJson = await arResp.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, arResp.StatusCode);
        Assert.Contains("Ù…ÙˆØ¬ÙˆØ¯ Ø¨Ø§Ù„ÙØ¹Ù„ ÙÙŠ Ø§Ù„Ù…ÙØ¶Ù„Ø©", arJson);
    }
}
