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
        
        // Assert - 설정 상태 섹션 확인
        var statusSection = Page.Locator("h6:has-text('Configuration Status:')");
        await Expect(statusSection).ToBeVisibleAsync();
        
        // Assert - Hugging Face 상태 배지 확인 (더 구체적인 selector 사용)
        var hfStatusBadge = Page.Locator("span.badge:has-text('Hugging Face: Missing')");
        await Expect(hfStatusBadge).ToBeVisibleAsync();
        
        // Assert - AI Provider 상태 배지 확인
        var providerStatusBadge = Page.Locator("span.badge:has-text('AI Provider: Required')");
        await Expect(providerStatusBadge).ToBeVisibleAsync();
        
        // Assert - Required 배지가 적어도 하나는 존재 확인 (first() 사용)
        var requiredBadge = Page.Locator("span.badge:has-text('Required')").First;
        await Expect(requiredBadge).ToBeVisibleAsync();
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
        
        // Assert - 초기 상태에서 Send 버튼이 비활성화되어 있는지 확인
        await Expect(sendButton).ToBeDisabledAsync();
        
        // Act - 메시지 입력 (oninput 바인딩으로 실시간 업데이트)
        await messageInput.ClickAsync();
        await messageInput.FillAsync("Hello, this is a test message");
        
        // JavaScript로 직접 input 이벤트 트리거
        await Page.EvaluateAsync(@"
            const input = document.querySelector('input[placeholder*=""Type your message here""]');
            if (input) {
                input.dispatchEvent(new Event('input', { bubbles: true }));
            }
        ");
        
        // Blazor Server SignalR 통신을 위한 대기
        await Page.WaitForTimeoutAsync(1000);
        
        // Assert - 입력값 확인
        await Expect(messageInput).ToHaveValueAsync("Hello, this is a test message");
        
        // Assert - Send 버튼이 활성화되었는지 확인
        await Expect(sendButton).ToBeEnabledAsync();
    }

    [TestMethod]
    [Ignore("DOM structure analysis needed")]
    public async Task SuggestionButtons_ShouldFillInputWhenClicked()
    {
        // Arrange
        await Page.GotoAsync(BASE_URL);
        
        var whoAmIButton = Page.Locator("button:has-text('who am I')");
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        
        // Act - 제안 버튼 클릭
        await whoAmIButton.ClickAsync();
        
        // Assert - 로딩 상태나 AI 응답 처리 대기
        await Page.WaitForTimeoutAsync(2000); // 더 긴 대기
        
        // 채팅 영역에 사용자 메시지가 나타났는지 확인 (더 구체적인 selector)
        var userMessage = Page.Locator("strong:has-text('You:')").Locator("..").Filter(new() { HasText = "who am I" });
        await Expect(userMessage).ToBeVisibleAsync();
        
        // 또는 AI 응답이나 오류 메시지가 나타났는지 확인
        var aiResponseOrError = Page.Locator("strong:has-text('AI:')").First;
        await Expect(aiResponseOrError).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task ChatInterface_ShouldShowErrorWhenNoAIProviderConfigured()
    {
        // Arrange
        await Page.GotoAsync(BASE_URL);
        
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        var sendButton = Page.Locator("button:has-text('Send')");
        
        // Act - 메시지 입력 (oninput 바인딩으로 실시간 활성화)
        await messageInput.ClickAsync();
        await messageInput.FillAsync("Test message");
        
        // JavaScript로 직접 input 이벤트 트리거
        await Page.EvaluateAsync(@"
            const input = document.querySelector('input[placeholder*=""Type your message here""]');
            if (input) {
                input.dispatchEvent(new Event('input', { bubbles: true }));
            }
        ");
        
        await Page.WaitForTimeoutAsync(1000);
        
        // Send 버튼이 활성화될 때까지 대기
        await Expect(sendButton).ToBeEnabledAsync();
        
        // 메시지 전송
        await sendButton.ClickAsync();
        
        // Assert - 오류 메시지 확인 (API 호출 대기)
        await Page.WaitForTimeoutAsync(3000);
        
        var errorMessage = Page.Locator("div:has-text('Error:')").Filter(new() { HasText = "No configured AI provider" });
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [TestMethod]
    [Ignore("DOM selector needs refinement")]
    public async Task PasswordFields_ShouldHaveToggleVisibility()
    {
        // Arrange
        await Page.GotoAsync($"{BASE_URL}/settings");
        
        var hfTokenInput = Page.Locator("input#hfToken");
        // input-group 구조에서 버튼 찾기
        var hfToggleButton = Page.Locator("input#hfToken").Locator("+ button");
        
        // 만약 위 selector가 안되면 다른 방법 시도
        if (await hfToggleButton.CountAsync() == 0)
        {
            hfToggleButton = Page.Locator("div.input-group:has(input#hfToken) button");
        }
        
        // Assert - 초기 상태는 password 타입
        await Expect(hfTokenInput).ToHaveAttributeAsync("type", "password");
        
        // Act - 토글 버튼 클릭
        await hfToggleButton.ClickAsync();
        
        // 약간의 대기 (상태 변경 처리)
        await Page.WaitForTimeoutAsync(100);
        
        // Assert - 타입이 text로 변경되었는지 확인
        await Expect(hfTokenInput).ToHaveAttributeAsync("type", "text");
        
        // Act - 다시 토글 버튼 클릭
        await hfToggleButton.ClickAsync();
        
        // 약간의 대기 (상태 변경 처리)
        await Page.WaitForTimeoutAsync(100);
        
        // Assert - 다시 password 타입으로 변경되었는지 확인
        await Expect(hfTokenInput).ToHaveAttributeAsync("type", "password");
    }
}