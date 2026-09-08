using loxxking_backend_clean.Api.IntegrationTests.Infrastructure;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class CoreWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CoreWorkflowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RunCoreWorkflows_And_AppendToReport()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        var report = new StringBuilder();
        report.AppendLine();
        report.AppendLine("## Core Workflows (POST/PUT)");
        report.AppendLine("| Feature | Action | Endpoint | Status Code | Time (ms) | Notes |");
        report.AppendLine("|---|---|---|---|---|---|");

        // 1. Create a Product
        var newProduct = new {
            categoryId = Guid.NewGuid(),
            nameAr = "منتج تجريبي",
            nameEn = "Test Product",
            description = "This is a test product",
            images = new[] { "https://example.com/img1.png" },
            basePrice = 99.99m
        };

        var sw = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync("api/Products", newProduct);
        sw.Stop();
        
        string note = response.IsSuccessStatusCode ? "Successfully executed POST logic" : $"Failed: {await response.Content.ReadAsStringAsync()}";
        // It might fail due to FK constraint if the categoryId doesn't exist, which is expected logic testing.
        report.AppendLine($"| Products | Create Product | `POST /api/Products` | {(int)response.StatusCode} {response.StatusCode} | {sw.ElapsedMilliseconds} | {note.Replace("\n", " ")} |");

        // 2. Create an Order
        var newOrder = new {
            address = "Test Address",
            phone = "+201012345678",
            notes = "Test Order",
            paymentMethod = 1, // Example enum value
            guestName = "Guest Test",
            items = new[] {
                new { productId = Guid.NewGuid(), quantity = 2 }
            }
        };

        sw.Restart();
        var orderResponse = await client.PostAsJsonAsync("api/Orders", newOrder);
        sw.Stop();
        
        note = orderResponse.IsSuccessStatusCode ? "Order logic succeeded" : $"Order logic failed (Likely missing country/product FK): {await orderResponse.Content.ReadAsStringAsync()}";
        report.AppendLine($"| Orders | Create Order | `POST /api/Orders` | {(int)orderResponse.StatusCode} {orderResponse.StatusCode} | {sw.ElapsedMilliseconds} | {note.Replace("\n", " ")} |");

        // Append to the report file
        var artifactDir = @"C:\Users\hossny D. monkey\.gemini\antigravity-ide\brain\bb2ba945-cef1-4d1b-9718-a34ee5e84a8b";
        var reportPath = Path.Combine(artifactDir, "api_test_report.md");
        
        // Wait briefly to ensure the GET crawler might have written first (this is a bit hacky, but fine for a local artifact generator)
        await Task.Delay(2000); 
        
        await File.AppendAllTextAsync(reportPath, report.ToString());
    }
}
