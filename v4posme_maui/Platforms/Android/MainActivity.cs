using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using v4posme_maui.Services;
using v4posme_maui.Services.SystemNames;
using Microsoft.Maui.ApplicationModel;


namespace v4posme_maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : MauiAppCompatActivity
    {
        private Intent? GpsServiceIntent;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                var channel = new NotificationChannel(Constantes.GpsNameChangelNotification, Constantes.GpsDescriptionServices, NotificationImportance.Default);
                var manager = GetSystemService(NotificationService) as NotificationManager;
                manager?.CreateNotificationChannel(channel);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Solicitar permisos y luego iniciar el servicio, evita crash en Android 14+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                _ = RequestPermissionsAndStartServiceAsync();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async Task RequestPermissionsAndStartServiceAsync()
        {
            try
            {
                var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus == PermissionStatus.Granted)
                {
                    StartLocationService();
                }
            }
            catch (Exception)
            {
                // Si falla la solicitud de permisos, no iniciar el servicio
            }
        }

        private void StartLocationService()
        {
            if (GpsServiceIntent != null) return; // ya iniciado
            GpsServiceIntent = new Intent(this, typeof(GPSService));
            StartService(GpsServiceIntent);
        }
    }
}