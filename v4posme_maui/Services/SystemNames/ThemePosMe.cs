using Microsoft.Maui.Graphics;

namespace v4posme_maui.Services.SystemNames;

/// <summary>
/// Administrador de temas maestros de la aplicacion. Cada tema define una paleta
/// de colores que sobreescribe las claves de <c>Colors.xaml</c> en tiempo de
/// ejecucion. De esta forma todas las pantallas que usan
/// <c>{DynamicResource ...}</c> se re-renderizan con el estilo seleccionado.
/// </summary>
public static class ThemePosMe
{
    // Nombres de los temas disponibles (los que se muestran en el combo).
    public const string PosMe  = "posMe";
    public const string Rosa   = "Rosa";
    public const string Azul   = "Azul";
    public const string Blanco = "Blanco";
    public const string Negro  = "Negro";
    public const string Verde  = "Verde";
    public const string Salmon = "Salmon";

    public static readonly List<string> Disponibles = new()
    {
        PosMe, Rosa, Azul, Blanco, Negro, Verde, Salmon
    };

    /// <summary>
    /// Define la paleta de cada tema. Solo se sobreescriben las claves de color
    /// que dan identidad visual (primarios, fondos, tarjetas, texto sobre
    /// primario). El resto de claves grises/utilitarias se mantienen.
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, Color>> Paletas = new()
    {
        [PosMe] = new()
        {
            ["Primary"]         = Color.FromArgb("#006E98"),
            ["PrimaryDark"]     = Color.FromArgb("#133C55"),
            ["PrimaryDarkText"] = Color.FromArgb("#386FA4"),
            ["Secondary"]       = Color.FromArgb("#00C868"),
            ["OnPrimary"]       = Colors.White,
            ["PageBackground"]  = Color.FromArgb("#F9F6FF"),
            ["CardColor"]       = Colors.White,
        },
        [Rosa] = new()
        {
            ["Primary"]         = Color.FromArgb("#D6336C"),
            ["PrimaryDark"]     = Color.FromArgb("#96143E"),
            ["PrimaryDarkText"] = Color.FromArgb("#E64980"),
            ["Secondary"]       = Color.FromArgb("#F06595"),
            ["OnPrimary"]       = Colors.White,
            ["PageBackground"]  = Color.FromArgb("#FFF0F5"),
            ["CardColor"]       = Colors.White,
        },
        [Azul] = new()
        {
            ["Primary"]         = Color.FromArgb("#1565C0"),
            ["PrimaryDark"]     = Color.FromArgb("#0D3C7A"),
            ["PrimaryDarkText"] = Color.FromArgb("#1E88E5"),
            ["Secondary"]       = Color.FromArgb("#42A5F5"),
            ["OnPrimary"]       = Colors.White,
            ["PageBackground"]  = Color.FromArgb("#EFF5FF"),
            ["CardColor"]       = Colors.White,
        },
        [Blanco] = new()
        {
            ["Primary"]         = Color.FromArgb("#5A5A5A"),
            ["PrimaryDark"]     = Color.FromArgb("#333333"),
            ["PrimaryDarkText"] = Color.FromArgb("#6E6E6E"),
            ["Secondary"]       = Color.FromArgb("#9E9E9E"),
            ["OnPrimary"]       = Colors.White,
            ["PageBackground"]  = Color.FromArgb("#FFFFFF"),
            ["CardColor"]       = Color.FromArgb("#F5F5F5"),
        },
        [Negro] = new()
        {
            ["Primary"]         = Color.FromArgb("#212121"),
            ["PrimaryDark"]     = Color.FromArgb("#000000"),
            ["PrimaryDarkText"] = Color.FromArgb("#424242"),
            ["Secondary"]       = Color.FromArgb("#616161"),
            ["OnPrimary"]       = Colors.White,
            ["PageBackground"]  = Color.FromArgb("#ECECEC"),
            ["CardColor"]       = Colors.White,
        },
        [Verde] = new()
        {
            ["Primary"]         = Color.FromArgb("#2E7D32"),
            ["PrimaryDark"]     = Color.FromArgb("#1B5E20"),
            ["PrimaryDarkText"] = Color.FromArgb("#43A047"),
            ["Secondary"]       = Color.FromArgb("#66BB6A"),
            ["OnPrimary"]       = Colors.White,
            ["PageBackground"]  = Color.FromArgb("#EFF7EF"),
            ["CardColor"]       = Colors.White,
        },
        [Salmon] = new()
        {
            ["Primary"]         = Color.FromArgb("#E9705B"),
            ["PrimaryDark"]     = Color.FromArgb("#B84A38"),
            ["PrimaryDarkText"] = Color.FromArgb("#F08A78"),
            ["Secondary"]       = Color.FromArgb("#FA8072"),
            ["OnPrimary"]       = Colors.White,
            ["PageBackground"]  = Color.FromArgb("#FFF3F0"),
            ["CardColor"]       = Colors.White,
        },
    };

    /// <summary>Tema aplicado actualmente.</summary>
    public static string Actual { get; private set; } = PosMe;

    /// <summary>Normaliza un nombre recibido; si no existe usa el tema por defecto.</summary>
    public static string Normalizar(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return PosMe;
        var encontrado = Disponibles
            .FirstOrDefault(t => string.Equals(t, nombre, StringComparison.OrdinalIgnoreCase));
        return encontrado ?? PosMe;
    }

    /// <summary>
    /// Aplica el tema indicado sobreescribiendo las claves de color en los
    /// recursos de la aplicacion. Debe llamarse en el hilo de UI.
    /// </summary>
    public static void Aplicar(string? nombre)
    {
        var tema = Normalizar(nombre);
        Actual = tema;

        var app = Application.Current;
        if (app?.Resources is null)
            return;

        if (!Paletas.TryGetValue(tema, out var paleta))
            return;

        var recursos = app.Resources;
        foreach (var (clave, color) in paleta)
        {
            recursos[clave] = color;
        }
    }
}
