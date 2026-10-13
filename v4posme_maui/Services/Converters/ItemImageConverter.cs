using System.Globalization;
using v4posme_maui.Services.Api;

namespace v4posme_maui.Services.Converters;

/// <summary>
/// Convierte el <c>ItemId</c> de un producto en un <see cref="ImageSource"/> para
/// mostrar la imagen del producto en la vista tipo "restaurante" (tarjetas).
///
/// La imagen se obtiene del servidor mediante <see cref="RestApiItemImage"/>
/// (POST que devuelve los bytes de la imagen). La descarga se hace de forma
/// perezosa dentro del <see cref="ImageSource.FromStream(Func{Stream})"/>, por lo
/// que no bloquea la UI: cada celda solicita su imagen cuando se renderiza.
///
/// Si el producto no tiene imagen, el servidor no responde o el binario no es una
/// imagen valida, se devuelve <c>null</c> para que el control muestre la imagen
/// por defecto definida en el XAML.
/// </summary>
public class ItemImageConverter : IValueConverter
{
    // Un unico cliente reutilizable para todas las celdas.
    private static readonly RestApiItemImage ApiItemImage = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int itemId || itemId <= 0)
            return null;

        return ImageSource.FromStream(async token =>
        {
            try
            {
                var bytes = await ApiItemImage.GetImageAsync(itemId);
                if (!RestApiItemImage.EsImagenValida(bytes))
                    return null;

                return new MemoryStream(bytes!);
            }
            catch
            {
                // Ante cualquier error se devuelve null para que el control use la
                // imagen por defecto. No se propaga la excepcion para no romper la lista.
                return null;
            }
        });
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
