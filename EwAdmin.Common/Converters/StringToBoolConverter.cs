using System.Globalization;
using Avalonia.Data.Converters;

namespace EwAdmin.Common.Converters;

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string strParameter && bool.TryParse(strParameter, out bool result))
        {
            return result;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return booleanValue.ToString();
        }
        return "False";
    }
}