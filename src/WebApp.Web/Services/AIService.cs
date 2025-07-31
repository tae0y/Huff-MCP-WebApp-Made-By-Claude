using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace WebApp.Web.Services;

public class AIService : IAIService
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<AIService> _logger;

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
                    ErrorMessage = "No configured AI provider available for chat. Please configure your AI settings." 
                };
            }

            var chatMessages = new List<OpenAI.Chat.ChatMessage>
            {
                new OpenAI.Chat.UserChatMessage(message)
            };

            var chatResponse = await chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);
            
            return ProcessResponse(chatResponse.Value.Content[0].Text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to AI service");
            return new AIResponse 
            { 
                IsError = true, 
                ErrorMessage = $"Error: {ex.Message}" 
            };
        }
    }

    public Task<List<AITool>> GetAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        // Temporarily return empty list until MCP integration is completed
        return Task.FromResult(new List<AITool>());
    }

    private async Task<ChatClient?> GetChatClientAsync()
    {
        var config = await _configurationService.GetConfigurationAsync();
        
        // Prioritize Azure OpenAI
        if (!string.IsNullOrEmpty(config.AzureOpenAIEndpoint) && !string.IsNullOrEmpty(config.AzureOpenAIApiKey))
        {
            try
            {
                var azureClient = new AzureOpenAIClient(new Uri(config.AzureOpenAIEndpoint), new ApiKeyCredential(config.AzureOpenAIApiKey));
                return azureClient.GetChatClient(config.ModelName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Azure OpenAI client");
            }
        }

        // Fallback to GitHub Models
        if (!string.IsNullOrEmpty(config.GitHubModelsToken))
        {
            try
            {
                var githubClient = new AzureOpenAIClient(new Uri("https://models.inference.ai.azure.com"), new ApiKeyCredential(config.GitHubModelsToken));
                return githubClient.GetChatClient(config.ModelName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GitHub Models client");
            }
        }

        return null;
    }

    private AIResponse ProcessResponse(string textContent)
    {
        var result = new AIResponse();
        textContent = textContent ?? string.Empty;

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
}