using System.Collections.Concurrent;
using System.Globalization;
using v4posme_maui.Services.Api;

namespace v4posme_maui.Services.Converters;

/// <summary>
/// Convierte el <c>ItemId</c> de un producto en un <see cref="ImageSource"/> para
/// mostrar la imagen del producto en la vista tipo "restaurante" (tarjetas).
///
/// La imagen se obtiene del servidor mediante <see cref="RestApiItemImage"/>
/// (POST que devuelve los bytes de la imagen).
///
/// Para evitar el "parpadeo" (cada celda descargaba su imagen cada vez que se
/// renderizaba, al hacer scroll o recargar), los bytes descargados se guardan en
/// una cache en memoria por <c>ItemId</c>:
/// - La primera vez se descarga del servidor y se guarda en cache.
/// - Las siguientes veces (reciclado de celdas al hacer scroll) se sirven al
///   instante desde la cache, sin volver a pedir al servidor, por lo que la imagen
///   ya no desaparece/reaparece.
/// Tambien se cachean los ItemId que no tienen imagen valida para no reintentar la
/// descarga una y otra vez.
///
/// Si el producto no tiene imagen, el servidor no responde o el binario no es una
/// imagen valida, se devuelve <c>null</c> para que el control muestre la imagen
/// por defecto definida en el XAML.
/// </summary>
public class ItemImageConverter : IValueConverter
{
    // Un unico cliente reutilizable para todas las celdas.
    private static readonly RestApiItemImage ApiItemImage = new();

    // Cache en memoria de las imagenes ya descargadas (bytes), por ItemId.
    private static readonly ConcurrentDictionary<int, byte[]> CacheImagenes = new();

    // ItemIds que ya se intentaron y no tienen imagen valida, para no reintentar.
    private static readonly ConcurrentDictionary<int, bool> SinImagen = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int itemId || itemId <= 0)
            return null;

        // Si ya se sabe que no tiene imagen, no se reintenta (muestra la imagen por defecto).
        if (SinImagen.ContainsKey(itemId))
            return null;

        return ImageSource.FromStream(async token =>
        {
            try
            {
                // Si ya esta en cache, se devuelve al instante (sin parpadeo ni descarga).
                if (CacheImagenes.TryGetValue(itemId, out var cacheados))
                    return new MemoryStream(cacheados);

                var bytes = await ApiItemImage.GetImageAsync(itemId);
                if (!RestApiItemImage.EsImagenValida(bytes))
                {
                    SinImagen.TryAdd(itemId, true);
                    return null;
                }

                CacheImagenes.TryAdd(itemId, bytes!);
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
