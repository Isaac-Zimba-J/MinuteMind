using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class RecordingPage : ContentPage
{
    public RecordingPage(RecordingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
