namespace MinuteMind;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register non-tab routes for push/modal navigation
        Routing.RegisterRoute(nameof(Views.ProcessingPage), typeof(Views.ProcessingPage));
        Routing.RegisterRoute(nameof(Views.TranscriptPage), typeof(Views.TranscriptPage));
        Routing.RegisterRoute(nameof(Views.MinutesPage), typeof(Views.MinutesPage));
        Routing.RegisterRoute(nameof(Views.EditMinutesPage), typeof(Views.EditMinutesPage));
        Routing.RegisterRoute(nameof(Views.ExportPage), typeof(Views.ExportPage));
    }
}
