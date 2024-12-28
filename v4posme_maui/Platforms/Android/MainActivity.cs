using Android.App;
using Android.Content.PM;
using Android.OS;
using Android;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Plugin.Permissions;
using v4posme_maui.Services.SystemNames;


namespace v4posme_maui
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var permisosFaltantes = new List<string>();

			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
			{
				// Comprobamos cada permiso y añadimos los que faltan a la lista
				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted)
				{
					permisosFaltantes.Add(Manifest.Permission.BluetoothConnect);
				}

				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ForegroundServiceLocation) != Permission.Granted)
				{
					permisosFaltantes.Add(Manifest.Permission.ForegroundServiceLocation);
				}

				var channel = new NotificationChannel(Constantes.GpsNameChangelNotification, Constantes.GpsDescriptionServices, NotificationImportance.Default);
				var manager = GetSystemService(NotificationService) as NotificationManager;
				manager?.CreateNotificationChannel(channel);

				// Si hay permisos faltantes, solicitarlos todos de una vez
				if (permisosFaltantes.Count > 0)
				{
					ActivityCompat.RequestPermissions(this, [.. permisosFaltantes], 1);
				}
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			if (requestCode == 1)
			{
				for (int i = 0; i < permissions.Length; i++)
				{
					if (grantResults[i] == Permission.Granted)
					{
						// El permiso ha sido concedido
						Console.WriteLine($"{permissions[i]} fue concedido.");
					}
					else
					{
						// El permiso ha sido denegado
						Console.WriteLine($"{permissions[i]} fue denegado.");
					}
				}
			}
		}
	}
}
