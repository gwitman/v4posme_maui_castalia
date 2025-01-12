using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using v4posme_maui.Services;
using v4posme_maui.Services.SystemNames;


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
                StartLocationService();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        private void StartLocationService()
        {
            //esto ejecutará el servicio de gps
            //no importa si esta en segundo plano la app
            GpsServiceIntent = new Intent(this, typeof(GPSService));
            StartService(GpsServiceIntent);
        }
    }
}