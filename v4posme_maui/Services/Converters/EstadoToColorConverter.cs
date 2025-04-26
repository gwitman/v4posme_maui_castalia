using System.Globalization;

namespace v4posme_maui.Services.Converters;

public class EstadoToColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Colors.Transparent;

        var asignado = values[0] is bool a && a;
        var facturado = values[1] is bool f && f;
        if (facturado)
            return Application.Current!.Resources["Salmon"]; // tiene prioridad
        return asignado ? Colors.Green : Application.Current!.Resources["Primary"];
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
