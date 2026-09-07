using TextCopy;

namespace ProjectTreeCli.Services;

public sealed class ClipboardManager
{
    public async Task CopyAsync(string text)
    {
        await ClipboardService.SetTextAsync(text);
    }
}