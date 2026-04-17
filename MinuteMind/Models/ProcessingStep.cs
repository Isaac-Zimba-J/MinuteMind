using CommunityToolkit.Mvvm.ComponentModel;

namespace MinuteMind.Models;

public enum StepStatus { Pending, Active, Completed }

public partial class ProcessingStep : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _subtitle = string.Empty;

    [ObservableProperty]
    private StepStatus _status = StepStatus.Pending;
}
