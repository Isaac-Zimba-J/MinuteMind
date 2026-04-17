using Microsoft.Extensions.Logging;
using DotNet.Meteor.HotReload.Plugin;
using CommunityToolkit.Maui;

namespace MinuteMind;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .EnableHotReload()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "PlusJakartaBold");
                fonts.AddFont("PlusJakartaSans-Light.ttf", "PlusJakartaLight");
                fonts.AddFont("PlusJakartaSans-Medium.ttf", "PlusJakartaMedium");
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaRegular");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "PlusJakartaSemiBold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}