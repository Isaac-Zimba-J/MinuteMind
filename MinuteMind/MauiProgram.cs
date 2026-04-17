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
                // Plus Jakarta Sans — Headlines
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "PlusJakartaBold");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "PlusJakartaSemiBold");
                fonts.AddFont("PlusJakartaSans-Medium.ttf", "PlusJakartaMedium");
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaRegular");
                fonts.AddFont("PlusJakartaSans-Light.ttf", "PlusJakartaLight");

                // Inter — Body & Labels
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}