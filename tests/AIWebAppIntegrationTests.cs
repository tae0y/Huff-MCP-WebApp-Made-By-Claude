using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace WebApp.IntegrationTests;

[TestClass]
public class AIWebAppIntegrationTests : PageTest
{
    private const string BASE_URL = "https://localhost:7058";
    
    [TestInitialize]
    public async Task TestInitialize()
    {
        // 브라우저 설정
        await Context.Tracing.StartAsync(new()
        {
            Title = TestContext.TestName,
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await Context.Tracing.StopAsync(new()
        {
            Path = Path.Combine(
                Environment.CurrentDirectory,
                "playwright-traces",
                $"{TestContext.TestName}.zip"
            )
        });
    }

    [TestMethod]
    public async Task HomePage_ShouldLoadAndDisplayAIChatInterface()
    {
        // Arrange & Act
        await Page.GotoAsync(BASE_URL);
        
        // Assert - 페이지 제목 확인
        await Expect(Page).ToHaveTitleAsync("AI Chat");
        
        // Assert - AI Chat Assistant 헤더 확인
        var header = Page.Locator("h3:has-text('🤖 AI Chat Assistant')");
        await Expect(header).ToBeVisibleAsync();
        
        // Assert - 제안 버튼들 확인
        var whoAmIButton = Page.Locator("button:has-text('who am I')");
        var beaverButton = Page.Locator("button:has-text('create a pixelated image of a beaver')");
        
        await Expect(whoAmIButton).ToBeVisibleAsync();
        await Expect(beaverButton).ToBeVisibleAsync();
        
        // Assert - 채팅 입력 요소들 확인
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        var sendButton = Page.Locator("button:has-text('Send')");
        
        await Expect(messageInput).ToBeVisibleAsync();
        await Expect(sendButton).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task NavigationMenu_ShouldContainAIChatAndSettings()
    {
        // Arrange & Act
        await Page.GotoAsync(BASE_URL);
        
        // Assert - 네비게이션 브랜드 확인
        var brand = Page.Locator("a.navbar-brand:has-text('🤖 AI Chat App')");
        await Expect(brand).ToBeVisibleAsync();
        
        // Assert - AI Chat 네비게이션 링크 확인 (홈페이지)
        var aiChatLink = Page.Locator("a.nav-link:has-text('AI Chat')");
        await Expect(aiChatLink).ToBeVisibleAsync();
        
        // Assert - Settings 네비게이션 링크 확인
        var settingsLink = Page.Locator("a.nav-link:has-text('Settings')");
        await Expect(settingsLink).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task SettingsPage_ShouldLoadAndDisplayConfigurationForm()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BASE_URL}/settings");
        
        // Assert - 페이지 제목 확인
        await Expect(Page).ToHaveTitleAsync("Settings");
        
        // Assert - 설정 헤더 확인
        var header = Page.Locator("h3:has-text('AI Configuration Settings')");
        await Expect(header).ToBeVisibleAsync();
        
        // Assert - 필수 설정 필드들 확인
        var hfTokenInput = Page.Locator("input#hfToken");
        var modelNameInput = Page.Locator("input#modelName");
        var hfMcpServerInput = Page.Locator("input#hfMcpServer");
        
        await Expect(hfTokenInput).ToBeVisibleAsync();
        await Expect(modelNameInput).ToBeVisibleAsync();
        await Expect(hfMcpServerInput).ToBeVisibleAsync();
        
        // Assert - AI 제공자 설정 필드들 확인
        var githubTokenInput = Page.Locator("input#githubToken");
        var azureEndpointInput = Page.Locator("input#azureEndpoint");
        var azureApiKeyInput = Page.Locator("input#azureApiKey");
        
        await Expect(githubTokenInput).ToBeVisibleAsync();
        await Expect(azureEndpointInput).ToBeVisibleAsync();
        await Expect(azureApiKeyInput).ToBeVisibleAsync();
        
        // Assert - 저장 버튼 확인
        var saveButton = Page.Locator("button:has-text('Save Settings')");
        await Expect(saveButton).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task SettingsPage_ShouldShowConfigurationStatusBadges()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BASE_URL}/settings");
        
        // Assert - Hugging Face 설정 상태 배지 확인
        var hfBadge = Page.Locator("span.badge:has-text('Required')");
        await Expect(hfBadge).ToBeVisibleAsync();
        
        // Assert - 설정 상태 섹션 확인
        var statusSection = Page.Locator("h6:has-text('Configuration Status:')");
        await Expect(statusSection).ToBeVisibleAsync();
        
        // Assert - Hugging Face 상태 배지 확인
        var hfStatusBadge = Page.Locator("span.badge:has-text('Hugging Face: Missing')");
        await Expect(hfStatusBadge).ToBeVisibleAsync();
        
        // Assert - AI Provider 상태 배지 확인
        var providerStatusBadge = Page.Locator("span.badge:has-text('AI Provider: Required')");
        await Expect(providerStatusBadge).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task SettingsPage_ShouldShowQuickSetupCards()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BASE_URL}/settings");
        
        // Assert - Quick Setup 섹션 확인
        var quickSetupHeader = Page.Locator("h5:has-text('Quick Setup')");
        await Expect(quickSetupHeader).ToBeVisibleAsync();
        
        // Assert - GitHub Models 카드 확인
        var githubCard = Page.Locator("h6:has-text('GitHub Models')");
        var githubLink = Page.Locator("a[href='https://github.com/marketplace/models']:has-text('Get Token')");
        
        await Expect(githubCard).ToBeVisibleAsync();
        await Expect(githubLink).ToBeVisibleAsync();
        
        // Assert - Azure OpenAI 카드 확인
        var azureCard = Page.Locator("h6:has-text('Azure OpenAI')");
        var azureLink = Page.Locator("a[href='https://portal.azure.com/']:has-text('Azure Portal')");
        
        await Expect(azureCard).ToBeVisibleAsync();
        await Expect(azureLink).ToBeVisibleAsync();
        
        // Assert - Hugging Face 카드 확인
        var hfCard = Page.Locator("h6:has-text('Hugging Face')");
        var hfLink = Page.Locator("a[href='https://huggingface.co/settings/tokens']:has-text('Get Token')");
        
        await Expect(hfCard).ToBeVisibleAsync();
        await Expect(hfLink).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task ChatInterface_ShouldAcceptUserInput()
    {
        // Arrange
        await Page.GotoAsync(BASE_URL);
        
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        var sendButton = Page.Locator("button:has-text('Send')");
        
        // Act - 메시지 입력
        await messageInput.FillAsync("Hello, this is a test message");
        
        // Assert - 입력값 확인
        await Expect(messageInput).ToHaveValueAsync("Hello, this is a test message");
        
        // Assert - Send 버튼이 활성화되었는지 확인
        await Expect(sendButton).ToBeEnabledAsync();
    }

    [TestMethod]
    public async Task SuggestionButtons_ShouldFillInputWhenClicked()
    {
        // Arrange
        await Page.GotoAsync(BASE_URL);
        
        var whoAmIButton = Page.Locator("button:has-text('who am I')");
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        
        // Act - 제안 버튼 클릭
        await whoAmIButton.ClickAsync();
        
        // Assert - 입력 필드에 제안 텍스트가 채워졌는지 확인
        // 참고: 실제 AI 호출이 일어나므로 로딩 상태나 오류 메시지가 나타날 수 있음
        await Page.WaitForTimeoutAsync(1000); // 잠시 대기
        
        // 채팅 영역에 사용자 메시지가 나타났는지 확인
        var userMessage = Page.Locator("div:has-text('You: who am I')");
        await Expect(userMessage).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task ChatInterface_ShouldShowErrorWhenNoAIProviderConfigured()
    {
        // Arrange
        await Page.GotoAsync(BASE_URL);
        
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        var sendButton = Page.Locator("button:has-text('Send')");
        
        // Act - 메시지 전송
        await messageInput.FillAsync("Test message");
        await sendButton.ClickAsync();
        
        // Assert - 오류 메시지 확인
        await Page.WaitForTimeoutAsync(2000); // API 호출 대기
        
        var errorMessage = Page.Locator("div:has-text('Error: No configured AI provider available for chat')");
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task PasswordFields_ShouldHaveToggleVisibility()
    {
        // Arrange
        await Page.GotoAsync($"{BASE_URL}/settings");
        
        var hfTokenInput = Page.Locator("input#hfToken");
        var hfToggleButton = hfTokenInput.Locator("..").Locator("button");
        
        // Assert - 초기 상태는 password 타입
        await Expect(hfTokenInput).ToHaveAttributeAsync("type", "password");
        
        // Act - 토글 버튼 클릭
        await hfToggleButton.ClickAsync();
        
        // Assert - 타입이 text로 변경되었는지 확인
        await Expect(hfTokenInput).ToHaveAttributeAsync("type", "text");
        
        // Act - 다시 토글 버튼 클릭
        await hfToggleButton.ClickAsync();
        
        // Assert - 다시 password 타입으로 변경되었는지 확인
        await Expect(hfTokenInput).ToHaveAttributeAsync("type", "password");
    }
}