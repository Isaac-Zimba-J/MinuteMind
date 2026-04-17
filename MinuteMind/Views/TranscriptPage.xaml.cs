using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class TranscriptPage : ContentPage
{
    public TranscriptPage(TranscriptViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
