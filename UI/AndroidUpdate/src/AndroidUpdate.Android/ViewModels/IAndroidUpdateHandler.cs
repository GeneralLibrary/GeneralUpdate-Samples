namespace AndroidUpdate.ViewModels;

/// <summary>
/// Abstraction for the platform-specific update download/install handler.
/// The Android project provides the implementation using GeneralUpdate.Avalonia.Android.
/// </summary>
public interface IAndroidUpdateHandler : IDisposable
{
    event EventHandler<double>? ProgressChanged;
    event EventHandler<string>? StatusChanged;

    Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
}
