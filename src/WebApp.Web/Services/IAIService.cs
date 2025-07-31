using Microsoft.Extensions.AI;

namespace WebApp.Web.Services;

public interface IAIService
{
    Task<AIResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default);
    Task<List<AITool>> GetAvailableToolsAsync(CancellationToken cancellationToken = default);
}

public class AIResponse
{
    public string? TextContent { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AITool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}