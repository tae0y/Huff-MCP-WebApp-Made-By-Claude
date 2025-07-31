using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace WebApp.IntegrationTests;

[TestClass]
public class DebugTests : PageTest
{
    private const string BASE_URL = "https://localhost:7058";
    
    [TestMethod]
    public async Task Debug_HomePage_TakeScreenshot()
    {
        await Page.GotoAsync(BASE_URL);
        
        // 스크린샷 찍기
        await Page.ScreenshotAsync(new()
        {
            Path = "debug-homepage.png",
            FullPage = true
        });
        
        // DOM 구조 출력
        var html = await Page.ContentAsync();
        Console.WriteLine("=== HOME PAGE HTML ===");
        Console.WriteLine(html);
    }
    
    [TestMethod]
    public async Task Debug_SettingsPage_TakeScreenshot()
    {
        await Page.GotoAsync($"{BASE_URL}/settings");
        
        // 스크린샷 찍기
        await Page.ScreenshotAsync(new()
        {
            Path = "debug-settings.png",
            FullPage = true
        });
        
        // 패스워드 필드 주변 HTML 구조 출력
        var inputGroup = Page.Locator("input#hfToken").Locator("..");
        var html = await inputGroup.InnerHTMLAsync();
        Console.WriteLine("=== PASSWORD FIELD HTML ===");
        Console.WriteLine(html);
    }
    
    [TestMethod]
    public async Task Debug_ChatInput_Behavior()
    {
        await Page.GotoAsync(BASE_URL);
        
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        var sendButton = Page.Locator("button:has-text('Send')");
        
        // 초기 상태 확인
        Console.WriteLine($"Initial Send button disabled: {await sendButton.IsDisabledAsync()}");
        
        // 텍스트 입력
        await messageInput.FillAsync("Test message");
        
        // 입력 후 상태 확인 (약간 대기)
        await Page.WaitForTimeoutAsync(500);
        Console.WriteLine($"After input Send button disabled: {await sendButton.IsDisabledAsync()}");
        
        // Force change event trigger
        await messageInput.PressAsync("Space");
        await messageInput.PressAsync("Backspace");
        
        await Page.WaitForTimeoutAsync(500);
        Console.WriteLine($"After force change Send button disabled: {await sendButton.IsDisabledAsync()}");
    }
}