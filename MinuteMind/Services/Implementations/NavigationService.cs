using System;
using MinuteMind.Services.Contracts;

namespace MinuteMind.Services.Implementations;

public class NavigationService(IServiceProvider services) : INavigationService
{
    public Task GoToAsync(string route) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(route));

    public Task GoBackAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));

   public Task GoToAsync(string route, Dictionary<string, object> parameters) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(route, parameters));

   

     public Task NavigateToShellAsync() =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (Application.Current?.Windows.Count > 0)
                Application.Current.Windows[0].Page = services.GetRequiredService<AppShell>();
        });

    Task INavigationService.NavigateToLoginAsync()
    {
        throw new NotImplementedException();
    }
}
