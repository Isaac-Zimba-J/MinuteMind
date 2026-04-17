using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class MinutesPage : ContentPage
{
    public MinutesPage(MinutesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
