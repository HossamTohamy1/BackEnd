using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Nodes;
using loxxking_backend_clean.Application.Common.Interfaces;

namespace loxxking_backend_clean.Application.Features.HomePageConfig.Commands.UpdateHomePageConfig;

public record UpdateHomePageConfigCommand(
    string SectionsJson,
    string? ModifiedBy = null
) : IRequest<Result>;

public class UpdateHomePageConfigHandler : IRequestHandler<UpdateHomePageConfigCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "PageConfig_HomePage";

    public UpdateHomePageConfigHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(UpdateHomePageConfigCommand request, CancellationToken cancellationToken)
    {
        // Validate JSON format
        JsonNode? incomingNode;
        try
        {
            incomingNode = JsonNode.Parse(request.SectionsJson);
            if (incomingNode is not JsonArray incomingArray)
            {
                return Result.Failure(new Error("HomePageConfig.InvalidPayload", "SectionsJson must be a JSON array."));
            }
        }
        catch (JsonException ex)
        {
            return Result.Failure(new Error("HomePageConfig.InvalidJson", $"Invalid JSON: {ex.Message}"));
        }

        var config = await _context.HomePageConfigs
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (config == null)
        {
            config = new Domain.Entities.HomePage.HomePageConfig
            {
                SectionsJson = incomingNode.ToJsonString(),
                Version = 1,
                LastModifiedBy = request.ModifiedBy,
                UpdatedAt = DateTime.UtcNow
            };
            _context.HomePageConfigs.Add(config);
        }
        else
        {
            // Intelligent Partial Update: Merge by id / type into existing configuration
            string mergedJson = MergeSections(config.SectionsJson, (JsonArray)incomingNode);
            config.SectionsJson = mergedJson;
            config.Version++;
            config.LastModifiedBy = request.ModifiedBy;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKey, cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Performs an id-based partial update on sections without destroying nested data.
    /// </summary>
    private static string MergeSections(string existingJson, JsonArray incomingArray)
    {
        JsonArray? existingArray = null;
        try
        {
            existingArray = JsonNode.Parse(existingJson) as JsonArray;
        }
        catch
        {
            // fallback if existing was invalid
        }

        if (existingArray == null || existingArray.Count == 0)
        {
            return incomingArray.ToJsonString();
        }

        var resultMap = new List<JsonObject>();

        foreach (var incomingItem in incomingArray)
        {
            if (incomingItem is not JsonObject incObj) continue;

            string? id = incObj["id"]?.ToString();
            string? type = incObj["type"]?.ToString();

            // Find matching existing section by ID, or by type
            JsonObject? match = null;
            foreach (var extItem in existingArray)
            {
                if (extItem is not JsonObject extObj) continue;
                if (!string.IsNullOrEmpty(id) && extObj["id"]?.ToString() == id)
                {
                    match = extObj;
                    break;
                }
                if (match == null && !string.IsNullOrEmpty(type) && extObj["type"]?.ToString() == type)
                {
                    match = extObj;
                }
            }

            if (match == null)
            {
                // New section: clone and keep exactly as requested
                resultMap.Add((JsonObject)JsonNode.Parse(incObj.ToJsonString())!);
            }
            else
            {
                // Merge: start with existing, overlay with incoming fields
                var merged = (JsonObject)JsonNode.Parse(match.ToJsonString())!;
                foreach (var prop in incObj)
                {
                    if (prop.Value != null)
                    {
                        merged[prop.Key] = JsonNode.Parse(prop.Value.ToJsonString());
                    }
                }
                resultMap.Add(merged);
            }
        }

        var finalArray = new JsonArray();
        foreach (var obj in resultMap)
        {
            finalArray.Add(obj);
        }

        return finalArray.ToJsonString();
    }
}
