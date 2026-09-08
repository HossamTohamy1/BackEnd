using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using loxxking_backend_clean.Application.Features.Users.Commands.CreateSalesEmployee;
using loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;
using loxxking_backend_clean.Domain.Entities.Countries;
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
public class VerifyAuthEndpointsIntegrationTests : IAsyncLifetime
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

    private async Task SeedLegacyUser(string email, string password, Guid userId, UserRole role = UserRole.Customer)
    {
        var connStr = _container.GetConnectionString();
        var legacyHash = BCrypt.Net.BCrypt.HashPassword(password);

        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();

        var countryId = Guid.NewGuid();
        await using var cmd = new NpgsqlCommand($@"
            INSERT INTO ""Countries"" (""Id"", ""Name"", ""Currency"", ""DefaultLanguage"", ""IsDefault"", ""CreatedAt"", ""IsActive"", ""IsDeleted"")
            VALUES ('{countryId}', 'TestCountry', 'USD', 'en', true, NOW(), true, false);

            INSERT INTO ""Users"" (""Id"", ""Name"", ""Email"", ""UserName"", ""NormalizedEmail"", ""NormalizedUserName"",
                ""Phone"", ""PasswordHash"", ""CountryId"", ""Role"", ""CreatedAt"", ""IsActive"", ""IsDeleted"",
                ""SecurityStamp"", ""ConcurrencyStamp"", ""EmailConfirmed"", ""PhoneNumberConfirmed"",
                ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"")
            VALUES ('{userId}', 'Legacy User', '{email}', '{email}', '{email.ToUpper()}', '{email.ToUpper()}',
                '+1234567890', '{legacyHash}', '{countryId}', {(int)role}, NOW(), true, false,
                '{Guid.NewGuid()}', '{Guid.NewGuid()}', true, true,
                false, false, 0);
        ", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task Endpoint_Login_LegacyBCryptUser_ReturnsJwtAndUpgradesHash()
    {
        var email = "legacy@test.com";
        var password = "SecurePass123!";
        var userId = Guid.NewGuid();

        await SeedLegacyUser(email, password, userId);

        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = password });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var result = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginUserResponse>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Token);
        Assert.Equal(userId, result.Data.UserId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstAsync(u => u.Id == userId);
        
        Assert.True(user.PasswordHash!.StartsWith("AQAAAA"), "Hash was not upgraded to PBKDF2 Identity format");

        var wrongResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = "wrongpassword" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongResponse.StatusCode);
    }

    [Fact]
    public async Task Endpoint_GetProfile_DoesNotLeakIdentityProperties()
    {
        var email = "profile@test.com";
        var password = "SecurePass123!";
        var userId = Guid.NewGuid();

        await SeedLegacyUser(email, password, userId);

        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", new { Email = email, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginUserResponse>>();
        var token = loginResult!.Data!.Token;

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var profileResponse = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var rawJson = await profileResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PasswordHash", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SecurityStamp", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConcurrencyStamp", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NormalizedEmail", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NormalizedUserName", rawJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Endpoint_Inventories_AuthorizeRolesAdmin_WorksCorrectly()
    {
        var adminEmail = "admin@test.com";
        var customerEmail = "customer@test.com";
        var password = "SecurePass123!";

        await SeedLegacyUser(adminEmail, password, Guid.NewGuid(), UserRole.Admin);
        await SeedLegacyUser(customerEmail, password, Guid.NewGuid(), UserRole.Customer);

        var adminLogin = await _client.PostAsJsonAsync("/api/users/login", new { Email = adminEmail, Password = password });
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<ApiResponse<LoginUserResponse>>())!.Data!.Token;

        var customerLogin = await _client.PostAsJsonAsync("/api/users/login", new { Email = customerEmail, Password = password });
        var customerToken = (await customerLogin.Content.ReadFromJsonAsync<ApiResponse<LoginUserResponse>>())!.Data!.Token;

        var someProductId = Guid.NewGuid();

        var adminReq = new HttpRequestMessage(HttpMethod.Get, $"/api/inventories/{someProductId}");
        adminReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var adminRes = await _client.SendAsync(adminReq);
        Assert.NotEqual(HttpStatusCode.Unauthorized, adminRes.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, adminRes.StatusCode);

        var customerReq = new HttpRequestMessage(HttpMethod.Get, $"/api/inventories/{someProductId}");
        customerReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var customerRes = await _client.SendAsync(customerReq);
        Assert.Equal(HttpStatusCode.Forbidden, customerRes.StatusCode);

        var anonRes = await _client.GetAsync($"/api/inventories/{someProductId}");
        Assert.Equal(HttpStatusCode.Unauthorized, anonRes.StatusCode);
    }

    [Fact]
    public async Task Endpoint_CreateEmployee_ByAdmin_SucceedsAndCanLogin()
    {
        var adminEmail = "admin2@test.com";
        var password = "SecurePass123!";
        await SeedLegacyUser(adminEmail, password, Guid.NewGuid(), UserRole.Admin);

        var adminLogin = await _client.PostAsJsonAsync("/api/users/login", new { Email = adminEmail, Password = password });
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<ApiResponse<LoginUserResponse>>())!.Data!.Token;

        var cmd = new CreateSalesEmployeeCommand("New Employee", "new.employee@test.com", "+1234567899", "NewPass123!");

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/users/staff/create-employee");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        req.Content = JsonContent.Create(cmd);

        var res = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var employeeLogin = await _client.PostAsJsonAsync("/api/users/login", new { Email = "new.employee@test.com", Password = "NewPass123!" });
        Assert.Equal(HttpStatusCode.OK, employeeLogin.StatusCode);
        var loginResult = await employeeLogin.Content.ReadFromJsonAsync<ApiResponse<LoginUserResponse>>();
        Assert.True(loginResult!.Success);
        Assert.Equal("SalesEmployee", loginResult.Data!.Role);
    }
}
