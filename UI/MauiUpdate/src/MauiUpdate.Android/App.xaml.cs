using MauiUpdate.Views;

namespace MauiUpdate;

public partial class App : Application
{
    public App(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
    }

    private readonly MainPage _mainPage;

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_mainPage)
        {
            Title = "MauiUpdate",
        };
    }
}
