using Avalonia.Data.Converters;

namespace Common.Avalonia.Converters;

public class ProgressConverter
{
    public static readonly IValueConverter ProgressToOpacityConverter =
        new FuncValueConverter<double, double>(progress =>
        {
            double MaxOpacity = 1;
            double MinOpacity = 0.2;
            double MaxProgress = 100;
            double range = MaxOpacity - MinOpacity;
            double opacity = MinOpacity + progress / MaxProgress * range;

            return Math.Clamp(opacity, MinOpacity, MaxOpacity);
        });
}