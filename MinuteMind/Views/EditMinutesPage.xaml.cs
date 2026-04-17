using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class EditMinutesPage : ContentPage
{
    public EditMinutesPage(EditMinutesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
