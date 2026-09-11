namespace Template.BlazorWasm.Components;

using Bunit;

using Microsoft.FluentUI.AspNetCore.Components;

public abstract class FluentUITestBase : BunitContext
{
    protected FluentUITestBase()
    {
        Services.AddFluentUIComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
