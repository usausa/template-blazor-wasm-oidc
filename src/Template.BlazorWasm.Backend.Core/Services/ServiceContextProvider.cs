namespace Template.BlazorWasm.Services;

public abstract class ServiceContextProvider
{
    public abstract ServiceContext Current { get; }
}
