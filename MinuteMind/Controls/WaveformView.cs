namespace MinuteMind.Controls;

public class WaveformView : GraphicsView
{
    private readonly WaveformDrawable _drawable = new();

    // Bindable property so XAML/VM can set Levels and trigger a redraw
    public static readonly BindableProperty LevelsProperty =
        BindableProperty.Create(
            nameof(Levels),
            typeof(float[]),
            typeof(WaveformView),
            new float[13],
            propertyChanged: OnLevelsChanged);

    public float[] Levels
    {
        get => (float[])GetValue(LevelsProperty);
        set => SetValue(LevelsProperty, value);
    }

    public WaveformView()
    {
        // Tell GraphicsView which object does the drawing
        Drawable = _drawable;
    }

    private static void OnLevelsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is WaveformView view && newValue is float[] levels)
        {
            view._drawable.Levels = levels;
            // Invalidate tells MAUI to call Draw() again on the next frame
            view.Invalidate();
        }
    }
}
