using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace WebApp.Web.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;
    private readonly IDataProtector _protector;

    public ConfigurationService(IDataProtectionProvider dataProtectionProvider, ILogger<ConfigurationService> logger, IWebHostEnvironment environment)
    {
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
        _configFilePath = Path.Combine(environment.ContentRootPath, "App_Data", "ai-config.json");
        _protector = _dataProtectionProvider.CreateProtector("AIConfiguration");
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath)!);
    }

    public async Task<AIConfiguration> GetConfigurationAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                return new AIConfiguration();
            }

            var encryptedJson = await File.ReadAllTextAsync(_configFilePath);
            if (string.IsNullOrEmpty(encryptedJson))
            {
                return new AIConfiguration();
            }

            var decryptedJson = _protector.Unprotect(encryptedJson);
            var config = JsonSerializer.Deserialize<AIConfiguration>(decryptedJson);
            
            return config ?? new AIConfiguration();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration");
            return new AIConfiguration();
        }
    }

    public async Task SaveConfigurationAsync(AIConfiguration configuration)
    {
        try
        {
            var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            var encryptedJson = _protector.Protect(json);
            await File.WriteAllTextAsync(_configFilePath, encryptedJson);
            
            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration");
            throw;
        }
    }

    public async Task<bool> ValidateConfigurationAsync(AIConfiguration configuration)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(configuration.HuggingFaceToken))
            {
                return false;
            }

            if (!configuration.HasValidAIProvider)
            {
                return false;
            }

            if (string.IsNullOrEmpty(configuration.ModelName))
            {
                return false;
            }

            // Validate URLs if provided
            if (!string.IsNullOrEmpty(configuration.AzureOpenAIEndpoint))
            {
                if (!Uri.TryCreate(configuration.AzureOpenAIEndpoint, UriKind.Absolute, out _))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(configuration.HuggingFaceMcpServer))
            {
                if (!Uri.TryCreate(configuration.HuggingFaceMcpServer, UriKind.Absolute, out _))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration");
            return false;
        }
    }
}