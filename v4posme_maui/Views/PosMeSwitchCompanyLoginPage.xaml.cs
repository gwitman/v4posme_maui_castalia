namespace v4posme_maui.Views
{
    // Resultado del dialogo de credenciales para el cambio de compania.
    public class SwitchCompanyLoginResult
    {
        public bool Aceptado { get; init; }
        public string? Usuario { get; init; }
        public string? Clave { get; init; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SwitchCompanyLoginPage : ContentPage
    {
        private readonly TaskCompletionSource<SwitchCompanyLoginResult> _tcs = new();

        public SwitchCompanyLoginPage(string companyName)
        {
            InitializeComponent();
            SubtituloLabel.Text = $"Ingresa tus credenciales para \"{companyName}\"";
        }

        public Task<SwitchCompanyLoginResult> WaitForResultAsync() => _tcs.Task;

        private async void OnAceptarClicked(object? sender, EventArgs e)
        {
            var usuario = TextUsuario.Text?.Trim();
            var clave   = TextClave.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            {
                ErrorLabel.Text      = "Debe ingresar usuario y contraseña.";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (!_tcs.Task.IsCompleted)
            {
                _tcs.SetResult(new SwitchCompanyLoginResult
                {
                    Aceptado = true,
                    Usuario  = usuario,
                    Clave    = clave
                });
            }

            await Navigation.PopModalAsync();
        }

        private async void OnCancelarClicked(object? sender, EventArgs e)
        {
            if (!_tcs.Task.IsCompleted)
            {
                _tcs.SetResult(new SwitchCompanyLoginResult { Aceptado = false });
            }

            await Navigation.PopModalAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            if (!_tcs.Task.IsCompleted)
            {
                _tcs.SetResult(new SwitchCompanyLoginResult { Aceptado = false });
            }

            return base.OnBackButtonPressed();
        }
    }
}
