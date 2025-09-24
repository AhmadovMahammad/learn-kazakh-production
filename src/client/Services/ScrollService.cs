using Microsoft.JSInterop;

namespace ClientSolution.Services;
public class ScrollService(IJSRuntime js)
{
    private readonly IJSRuntime _js = js;

    public async Task ScrollToJourney()
    {
        await _js.InvokeVoidAsync("ScrollTo", "ThreeMonthJourney");
    }
}