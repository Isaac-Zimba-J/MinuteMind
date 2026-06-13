using Microsoft.Extensions.Logging;
using DotNet.Meteor.HotReload.Plugin;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using MinuteMind.Data;
using MinuteMind.Services.Contracts;
using MinuteMind.Services.Implementations;
using MinuteMind.ViewModels;
using MinuteMind.Views;

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

        // Remove bottom underline from Entry / Editor on Android
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });

        // Audio converter — platform-specific, handles AAC/M4A/MP3 → 16 kHz WAV for Whisper
#if ANDROID
        builder.Services.AddSingleton<IAudioConverter, AndroidAudioConverter>();
#else
        builder.Services.AddSingleton<IAudioConverter, NullAudioConverter>();
#endif

        // Database
        builder.Services.AddSingleton<MinuteMindDatabase>();

        // Services
        builder.Services.AddSingleton<IMeetingRepository, MeetingRepository>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IAudioRecorderService, AudioRecorderService>();
        builder.Services.AddSingleton<ITranscriptionService, LocalTranscriptionService>();
        builder.Services.AddTransient<IMinutesGeneratorService, GroqMinutesGeneratorService>();
        builder.Services.AddTransient<IPdfExportService, PdfExportService>();
        builder.Services.AddSingleton<Plugin.Maui.Audio.IAudioManager>(AudioManager.Current);
        builder.Services.AddHttpClient();

        // ViewModels
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<RecordingViewModel>();
        builder.Services.AddTransient<ProcessingViewModel>();
        builder.Services.AddTransient<TranscriptViewModel>();
        builder.Services.AddTransient<MinutesViewModel>();
        builder.Services.AddTransient<EditMinutesViewModel>();
        builder.Services.AddTransient<ExportViewModel>();
        builder.Services.AddTransient<MeetingsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<Dashboard>();
        builder.Services.AddTransient<RecordingPage>();
        builder.Services.AddTransient<ProcessingPage>();
        builder.Services.AddTransient<TranscriptPage>();
        builder.Services.AddTransient<MinutesPage>();
        builder.Services.AddTransient<EditMinutesPage>();
        builder.Services.AddTransient<ExportPage>();
        builder.Services.AddTransient<MeetingsPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
