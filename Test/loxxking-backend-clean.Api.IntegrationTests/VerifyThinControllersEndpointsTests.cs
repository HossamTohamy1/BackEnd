using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;
using loxxking_backend_clean.Domain.Entities.Users;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Persistence;
using loxxking_backend_clean.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

[Collection("PostgresSequential")]
public class VerifyThinControllersEndpointsTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private Guid _customerUserId;
    private Guid _adminUserId;
    private string _customerToken = "";
    private string _adminToken = "";

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

        _customerUserId = Guid.NewGuid();
        _adminUserId = Guid.NewGuid();
        
        await SeedUser("customer.test@test.com", "Pass123!", _customerUserId, UserRole.Customer);
        await SeedUser("admin.test@test.com", "Pass123!", _adminUserId, UserRole.Admin);

        _customerToken = await GetToken("customer.test@test.com", "Pass123!");
        _adminToken = await GetToken("admin.test@test.com", "Pass123!");
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory != null) await _factory.DisposeAsync();
        if (_container != null) await _container.DisposeAsync();
    }

    private async Task SeedUser(string email, string password, Guid userId, UserRole role)
    {
        var connStr = _container.GetConnectionString();
        var legacyHash = BCrypt.Net.BCrypt.HashPassword(password);

        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();

        var countryId = Guid.NewGuid();
        await using var cmd = new NpgsqlCommand($@"
            INSERT INTO ""Countries"" (""Id"", ""Name"", ""Currency"", ""DefaultLanguage"", ""IsDefault"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
            VALUES ('{countryId}', 'TestCountry', 'USD', 'en', true, NOW(), true, false) ON CONFLICT DO NOTHING;

            INSERT INTO ""Users"" (""Id"", ""Name"", ""Email"", ""UserName"", ""NormalizedEmail"", ""NormalizedUserName"",
                ""Phone"", ""PasswordHash"", ""CountryId"", ""Role"", ""CreatedAt"", ""IsActive"", ""IsDeleted"",
                ""SecurityStamp"", ""ConcurrencyStamp"", ""EmailConfirmed"", ""PhoneNumberConfirmed"",
                ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"")
            VALUES ('{userId}', 'Test User', '{email}', '{email}', '{email.ToUpper()}', '{email.ToUpper()}',
                '+1234567890', '{legacyHash}', '{countryId}', {(int)role}, NOW(), true, false,
                '{Guid.NewGuid()}', '{Guid.NewGuid()}', true, true,
                false, false, 0);
        ", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<string> GetToken(string email, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = password });
        var result = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginUserResponse>>();
        return result!.Data!.Token;
    }

    [Fact]
    public async Task Endpoint_BankTransfers_Upload_FailsWithoutFile()
    {
        var orderId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/bank-transfers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _customerToken);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(orderId.ToString()), "orderId");

        request.Content = content;

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":false", json);
    }

    [Fact]
    public async Task Endpoint_Products_SubmitReview_FailsInvalidRating()
    {
        var productId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/products/{productId}/reviews");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _customerToken);
        
        request.Content = JsonContent.Create(new { Rating = 6, Comment = "Good" });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":false", json);
    }
    
    [Fact]
    public async Task Endpoint_Favorites_AddAndGet_UsesCurrentUser()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var cat = loxxking_backend_clean.Domain.Entities.Categories.Category.Create("Cat", "cat", "cat-slug", "img");
        db.Categories.Add(cat);
        var prod = loxxking_backend_clean.Domain.Entities.Products.Product.Create(
            cat.Id, 
            "Prod", 
            "ProdEn",
            "Desc", 
            "prod-slug", 
            loxxking_backend_clean.Domain.ValueObjects.Money.FromDecimal(10m), 
            null, 
            new List<string>(), 
            new List<string>(), 
            new List<string>()
        );
        db.Products.Add(prod);
        await db.SaveChangesAsync();

        var addRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/favorites/{prod.Id}");
        addRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _customerToken);
        var addResponse = await _client.SendAsync(addRequest);
        
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/favorites");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _customerToken);
        var getResponse = await _client.SendAsync(getRequest);
        
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var json = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains(prod.Id.ToString(), json);
    }

    [Fact]
    public async Task Endpoint_Users_GetProfile_ReturnsCurrentUserProfile()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _customerToken);
        var response = await _client.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains(_customerUserId.ToString(), json);
        Assert.Contains("customer.test@test.com", json);
    }

    [Fact]
    public async Task Endpoint_SupportChat_SendMessage_SetsSenderCorrectly()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/support-chat/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _customerToken);
        request.Content = JsonContent.Create(new { Message = "Hello support!" });
        var response = await _client.SendAsync(request);
        var error = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Error: {error}");

        var json = System.Text.Json.Nodes.JsonNode.Parse(error);
        var returnedConvoId = json?["data"]?["conversationId"]?.ToString();
        Assert.NotNull(returnedConvoId);

        var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/support-chat/messages/{returnedConvoId}");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _customerToken);
        var getRes = await _client.SendAsync(getReq);

        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        var getResponseJson = await getRes.Content.ReadAsStringAsync();
        Assert.Contains("Hello support!", getResponseJson);
        Assert.Contains("\"senderType\":\"Customer\"", getResponseJson);
    }
}
