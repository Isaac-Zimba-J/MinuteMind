using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class MeetingsPage : ContentPage
{
    public MeetingsPage(MeetingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
