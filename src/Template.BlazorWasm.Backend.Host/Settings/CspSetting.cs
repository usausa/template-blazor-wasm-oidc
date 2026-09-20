namespace Template.BlazorWasm.Backend.Host.Settings;

public sealed class CspSetting
{
    // Report-Only: violations are reported in the browser console, nothing is blocked
    public bool ReportOnly { get; set; }
}
