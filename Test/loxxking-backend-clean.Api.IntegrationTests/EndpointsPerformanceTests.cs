using loxxking_backend_clean.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class EndpointsPerformanceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EndpointsPerformanceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CrawlAllGetEndpoints_And_GenerateReport()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        
        // Find all controllers in the API assembly
        var apiAssembly = typeof(loxxking_backend_clean.Api.Controllers.ProductsController).Assembly;
        
        var controllers = apiAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract)
            .ToList();

        var report = new StringBuilder();
        report.AppendLine("# API Performance & Logic Report");
        report.AppendLine("This report simulates Postman requests to all endpoints, bypassing Authentication/Authorization, to measure logic response and performance.");
        report.AppendLine();
        report.AppendLine("## Automated GET Requests");
        report.AppendLine("| Controller | Route | Status Code | Time (ms) | Notes |");
        report.AppendLine("|---|---|---|---|---|");

        foreach (var controller in controllers)
        {
            // Get base route
            var routeAttr = controller.GetCustomAttribute<RouteAttribute>();
            var baseRoute = routeAttr?.Template.Replace("[controller]", controller.Name.Replace("Controller", "")) ?? "api/" + controller.Name.Replace("Controller", "");

            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            
            foreach (var method in methods)
            {
                var httpGetAttr = method.GetCustomAttribute<HttpGetAttribute>();
                if (httpGetAttr != null)
                {
                    var endpointTemplate = httpGetAttr.Template ?? "";
                    
                    // Construct final URL
                    var fullUrl = $"{baseRoute}/{endpointTemplate}".TrimEnd('/');
                    
                    // Replace obvious parameters with dummy values for testing
                    fullUrl = fullUrl.Replace("{id}", Guid.NewGuid().ToString());
                    fullUrl = fullUrl.Replace("{productId}", Guid.NewGuid().ToString());
                    fullUrl = fullUrl.Replace("{countryId}", Guid.NewGuid().ToString());
                    fullUrl = fullUrl.Replace("{orderId}", Guid.NewGuid().ToString());
                    fullUrl = fullUrl.Replace("{conversationId}", Guid.NewGuid().ToString());
                    // Add more generic replacements if needed

                    var sw = Stopwatch.StartNew();
                    var response = await client.GetAsync(fullUrl);
                    sw.Stop();

                    string note = response.IsSuccessStatusCode ? "Success" : $"Failed with {response.StatusCode}";
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        note = "Expected 404 (Dummy ID used)";

                    report.AppendLine($"| {controller.Name} | `GET /{fullUrl}` | {(int)response.StatusCode} {response.StatusCode} | {sw.ElapsedMilliseconds} | {note} |");
                }
            }
        }

        // Save report to the artifact directory
        var artifactDir = @"C:\Users\hossny D. monkey\.gemini\antigravity-ide\brain\bb2ba945-cef1-4d1b-9718-a34ee5e84a8b";
        var reportPath = Path.Combine(artifactDir, "api_test_report.md");
        
        await File.WriteAllTextAsync(reportPath, report.ToString());
        
        Assert.True(File.Exists(reportPath));
    }
}
