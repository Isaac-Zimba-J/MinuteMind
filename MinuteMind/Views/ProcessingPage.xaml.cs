using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class ProcessingPage : ContentPage
{
    public ProcessingPage(ProcessingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
