using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class MeetingsPage : ContentPage
{
    public MeetingsPage(MeetingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MeetingsViewModel vm)
            vm.LoadMeetingsCommand.Execute(null);
    }
}
