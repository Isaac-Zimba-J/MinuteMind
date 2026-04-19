using System;

namespace MinuteMind.Services.Contracts;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync(string route, Dictionary<string, object> parameters);
    Task GoBackAsync();
    Task NavigateToShellAsync();
}
