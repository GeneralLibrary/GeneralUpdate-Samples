using System.ComponentModel;
using System.Reflection;
using Avalonia.Data.Converters;

namespace Common.Avalonia.Converters;

public static class EnumConverter
{
    public static readonly IValueConverter EnumToDescriptionConverter =
        new FuncValueConverter<object?, string>(obj =>
        {
            if (obj is not Enum value)
                return string.Empty;

            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        });
}