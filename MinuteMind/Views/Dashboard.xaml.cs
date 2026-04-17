using MinuteMind.ViewModels;

namespace MinuteMind.Views;

public partial class Dashboard : ContentPage
{
    public Dashboard(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
