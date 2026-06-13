namespace MinuteMind.Controls;

public class WaveformView : GraphicsView
{
    private readonly WaveformDrawable _drawable = new();

    public static readonly BindableProperty LevelsProperty =
        BindableProperty.Create(
            nameof(Levels),
            typeof(float[]),
            typeof(WaveformView),
            new float[13],
            propertyChanged: OnLevelsChanged);

    public static readonly BindableProperty BarColorProperty =
        BindableProperty.Create(
            nameof(BarColor),
            typeof(Color),
            typeof(WaveformView),
            Color.FromArgb("#005FAA"),
            propertyChanged: OnBarColorChanged);

    public float[] Levels
    {
        get => (float[])GetValue(LevelsProperty);
        set => SetValue(LevelsProperty, value);
    }

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    public WaveformView()
    {
        Drawable = _drawable;
    }

    private static void OnLevelsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is WaveformView view && newValue is float[] levels)
        {
            view._drawable.Levels = levels;
            view.Invalidate();
        }
    }

    private static void OnBarColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is WaveformView view && newValue is Color color)
        {
            view._drawable.BarColor = color;
            view.Invalidate();
        }
    }
}
