namespace WebApp.Web.Services;

public interface IConfigurationService
{
    Task<AIConfiguration> GetConfigurationAsync();
    Task SaveConfigurationAsync(AIConfiguration configuration);
    Task<bool> ValidateConfigurationAsync(AIConfiguration configuration);
}

public class AIConfiguration
{
    public string HuggingFaceToken { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gpt-4o-mini";
    public string GitHubModelsToken { get; set; } = string.Empty;
    public string AzureOpenAIEndpoint { get; set; } = string.Empty;
    public string AzureOpenAIApiKey { get; set; } = string.Empty;
    public string HuggingFaceMcpServer { get; set; } = "https://huggingface.co/mcp";
    
    public bool HasHuggingFaceConfig => !string.IsNullOrEmpty(HuggingFaceToken);
    public bool HasGitHubModelsConfig => !string.IsNullOrEmpty(GitHubModelsToken);
    public bool HasAzureOpenAIConfig => !string.IsNullOrEmpty(AzureOpenAIEndpoint) && !string.IsNullOrEmpty(AzureOpenAIApiKey);
    public bool HasValidAIProvider => HasGitHubModelsConfig || HasAzureOpenAIConfig;
}