using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;
using Common.Avalonia.Models;
using Common.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sandbox.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;

    public ObservableCollection<string> ThemeOptions { get; } = ["浅色", "深色", "跟随系统"];

    public string CurrentVersion => "12.1.0";

    [ObservableProperty] public partial int SelectedThemeIndex { get; set; } = GetInitialThemeIndex();

    [ObservableProperty] public partial bool IsCheckUpdatePopupOpen { get; set; }

    [ObservableProperty] public partial bool IsDownloadPopupOpen { get; set; }

    [ObservableProperty] public partial bool IsInstallPopupOpen { get; set; }

    [ObservableProperty] public partial DownloadStatistics Statistics { get; set; }

    public SettingsPageViewModel(IDownloadService downloadService)
    {
        _downloadService = downloadService;

        _downloadService.ProgressChanged += stats => Statistics = stats;
        _downloadService.StatusChanged += status =>
        {
            if (status == DownloadStatus.Completed)
                OnDownloadCompleted();
        };

        Statistics = _downloadService.CurrentStatistics;
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var app = Application.Current;

        app?.RequestedThemeVariant = value switch
        {
            0 => ThemeVariant.Light,
            1 => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    private static int GetInitialThemeIndex()
    {
        var variant = Application.Current?.RequestedThemeVariant;
        if (variant == ThemeVariant.Light) return 0;
        if (variant == ThemeVariant.Dark) return 1;
        return 2;
    }

    private void OnDownloadCompleted()
    {
        IsDownloadPopupOpen = false;
        IsInstallPopupOpen = true;
    }

    [RelayCommand]
    private void CheckUpdate()
    {
        IsCheckUpdatePopupOpen = true;
    }

    [RelayCommand]
    private void ConfirmUpdate()
    {
        IsCheckUpdatePopupOpen = false;
        IsDownloadPopupOpen = true;
        _downloadService.Restart();
    }

    [RelayCommand]
    private void CancelUpdate()
    {
        _downloadService.Stop();
        IsCheckUpdatePopupOpen = false;
        IsDownloadPopupOpen = false;
    }

    [RelayCommand]
    private void InstallConfirm()
    {
        IsInstallPopupOpen = false;
    }

    [RelayCommand]
    private void InstallCancel()
    {
        IsInstallPopupOpen = false;
    }
}