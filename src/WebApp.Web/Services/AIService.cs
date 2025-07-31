using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using System.ClientModel;
using System.Text.RegularExpressions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Client.Transports;

namespace WebApp.Web.Services;

public class AIService : IAIService
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<AIService> _logger;
    private McpClient? _mcpClient;

    public AIService(IConfigurationService configurationService, ILogger<AIService> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<AIResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var chatClient = await GetChatClientAsync();
            if (chatClient == null)
            {
                return new AIResponse 
                { 
                    IsError = true, 
                    ErrorMessage = "No configured AI provider available for chat" 
                };
            }

            var chatMessages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, message)
            };

            var chatOptions = new ChatOptions();
            
            // Add MCP tools if available
            var tools = await GetAvailableToolsAsync(cancellationToken);
            if (tools.Any())
            {
                var mcpTools = await GetMcpToolsAsync();
                if (mcpTools.Any())
                {
                    chatOptions.Tools = mcpTools.ToList();
                }
            }

            var response = await chatClient.CompleteAsync(chatMessages, chatOptions, cancellationToken);
            
            return ProcessResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to AI service");
            return new AIResponse 
            { 
                IsError = true, 
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<List<AITool>> GetAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var mcpClient = await GetMcpClientAsync();
            if (mcpClient == null) return new List<AITool>();

            var tools = await mcpClient.ListToolsAsync(cancellationToken);
            return tools.Select(tool => new AITool
            {
                Name = tool.Name,
                Description = tool.Description ?? string.Empty
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available tools");
            return new List<AITool>();
        }
    }

    private async Task<IChatClient?> GetChatClientAsync()
    {
        var config = await _configurationService.GetConfigurationAsync();
        
        // Prioritize Azure OpenAI
        if (!string.IsNullOrEmpty(config.AzureOpenAIEndpoint) && !string.IsNullOrEmpty(config.AzureOpenAIApiKey))
        {
            var azureClient = new AzureOpenAIClient(new Uri(config.AzureOpenAIEndpoint), new ApiKeyCredential(config.AzureOpenAIApiKey));
            var chatClient = azureClient.AsChatClient(config.ModelName);
            return chatClient.AsIChatClient().AsBuilder().UseFunctionInvocation().Build();
        }

        // Fallback to GitHub Models
        if (!string.IsNullOrEmpty(config.GitHubModelsToken))
        {
            var githubClient = new AzureOpenAIClient(new Uri("https://models.inference.ai.azure.com"), new ApiKeyCredential(config.GitHubModelsToken));
            var chatClient = githubClient.AsChatClient(config.ModelName);
            return chatClient.AsIChatClient().AsBuilder().UseFunctionInvocation().Build();
        }

        return null;
    }

    private async Task<McpClient?> GetMcpClientAsync()
    {
        if (_mcpClient != null) return _mcpClient;

        var config = await _configurationService.GetConfigurationAsync();
        if (string.IsNullOrEmpty(config.HuggingFaceToken) || string.IsNullOrEmpty(config.HuggingFaceMcpServer))
        {
            return null;
        }

        try
        {
            var hfHeaders = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {config.HuggingFaceToken}" }
            };

            var clientTransport = new SseClientTransport(new()
            {
                Name = "HF Server",
                Endpoint = new Uri(config.HuggingFaceMcpServer),
                AdditionalHeaders = hfHeaders
            });

            _mcpClient = await McpClientFactory.CreateAsync(clientTransport);
            return _mcpClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MCP client");
            return null;
        }
    }

    private async Task<IEnumerable<AITool>> GetMcpToolsAsync()
    {
        try
        {
            var mcpClient = await GetMcpClientAsync();
            if (mcpClient == null) return Enumerable.Empty<AITool>();

            var tools = await mcpClient.ListToolsAsync();
            return tools.Select(tool => new AITool
            {
                Name = tool.Name,
                Description = tool.Description ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP tools");
            return Enumerable.Empty<AITool>();
        }
    }

    private AIResponse ProcessResponse(ChatCompletion response)
    {
        var result = new AIResponse();
        var textContent = response.Message.Text ?? string.Empty;

        // Extract images from response
        var imageUrls = ExtractImageUrls(textContent);
        if (imageUrls.Any())
        {
            result.ImageUrls = imageUrls;
            // Remove markdown image syntax from text
            textContent = RemoveImageMarkdown(textContent);
        }

        result.TextContent = textContent.Trim();
        return result;
    }

    private List<string> ExtractImageUrls(string text)
    {
        var urls = new List<string>();
        
        // Pattern for markdown images: ![alt text](url)
        var markdownPattern = @"!\[.*?\]\((https?://[^\s\)]+(?:\.(?:jpg|jpeg|png|gif|webp|svg))?[^\s\)]*)\)";
        var markdownMatches = Regex.Matches(text, markdownPattern, RegexOptions.IgnoreCase);
        urls.AddRange(markdownMatches.Select(m => m.Groups[1].Value));

        // Pattern for direct HTTP/HTTPS URLs with image extensions
        var directPattern = @"https?://[^\s]+\.(?:jpg|jpeg|png|gif|webp|svg)(?:\?[^\s]*)?";
        var directMatches = Regex.Matches(text, directPattern, RegexOptions.IgnoreCase);
        urls.AddRange(directMatches.Select(m => m.Value));

        // Pattern for Hugging Face specific URLs
        var hfPattern = @"https?://[^\s]*\.hf\.space[^\s]*";
        var hfMatches = Regex.Matches(text, hfPattern, RegexOptions.IgnoreCase);
        urls.AddRange(hfMatches.Select(m => m.Value));

        return urls.Distinct().ToList();
    }

    private string RemoveImageMarkdown(string text)
    {
        var pattern = @"!\[.*?\]\([^\)]+\)";
        return Regex.Replace(text, pattern, string.Empty, RegexOptions.IgnoreCase).Trim();
    }

    public void Dispose()
    {
        _mcpClient?.Dispose();
    }
}