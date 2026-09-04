using System.Globalization;

namespace v4posme_maui.Services.Converters;

/// <summary>
/// Convierte un porcentaje (0-100) al valor 0.0-1.0 que espera un ProgressBar.
/// Valores fuera de rango se recortan a [0,1].
/// </summary>
public class PorcentajeProgresoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var porcentaje = value switch
        {
            decimal d => (double)d,
            double db => db,
            float f   => f,
            int i     => i,
            _         => 0d
        };

        var progreso = porcentaje / 100d;
        return Math.Clamp(progreso, 0d, 1d);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
