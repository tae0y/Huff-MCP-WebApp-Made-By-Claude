using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace WebApp.IntegrationTests;

[TestClass]
public class QuickDebugTest : PageTest
{
    private const string BASE_URL = "https://localhost:7058";
    
    [TestMethod]
    public async Task QuickDebug_SendButtonState()
    {
        await Page.GotoAsync(BASE_URL);
        
        var messageInput = Page.Locator("input[placeholder*='Type your message here']");
        var sendButton = Page.Locator("button:has-text('Send')");
        
        Console.WriteLine("=== 초기 상태 ===");
        Console.WriteLine($"Input value: '{await messageInput.InputValueAsync()}'");
        Console.WriteLine($"Send button disabled: {await sendButton.IsDisabledAsync()}");
        Console.WriteLine($"Send button HTML: {await sendButton.GetAttributeAsync("outerHTML")}");
        
        await messageInput.ClickAsync();
        await messageInput.FillAsync("Test");
        
        Console.WriteLine("\n=== FillAsync 후 ===");
        Console.WriteLine($"Input value: '{await messageInput.InputValueAsync()}'");
        Console.WriteLine($"Send button disabled: {await sendButton.IsDisabledAsync()}");
        
        // 수동으로 각 문자를 타이핑해보기
        await messageInput.ClearAsync();
        await messageInput.ClickAsync();
        
        await Page.Keyboard.TypeAsync("T");
        await Page.WaitForTimeoutAsync(100);
        Console.WriteLine($"\n=== 'T' 타이핑 후 ===");
        Console.WriteLine($"Input value: '{await messageInput.InputValueAsync()}'");
        Console.WriteLine($"Send button disabled: {await sendButton.IsDisabledAsync()}");
        
        await Page.Keyboard.TypeAsync("est");
        await Page.WaitForTimeoutAsync(500);
        Console.WriteLine($"\n=== 'Test' 완성 후 ===");
        Console.WriteLine($"Input value: '{await messageInput.InputValueAsync()}'");
        Console.WriteLine($"Send button disabled: {await sendButton.IsDisabledAsync()}");
        
        // 강제로 blur 이벤트 발생
        await Page.EvaluateAsync("document.querySelector('input[placeholder*=\"Type your message here\"]').blur()");
        await Page.WaitForTimeoutAsync(500);
        Console.WriteLine($"\n=== blur 이벤트 후 ===");
        Console.WriteLine($"Input value: '{await messageInput.InputValueAsync()}'");
        Console.WriteLine($"Send button disabled: {await sendButton.IsDisabledAsync()}");
    }
}