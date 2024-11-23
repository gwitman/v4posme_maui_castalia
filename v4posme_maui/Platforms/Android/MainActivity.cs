using Android.App;
using Android.Content.PM;
using Android.OS;


using Android;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Plugin.Permissions;
using v4posme_maui.Services;
using Android.Content;
using AndroidX.ConstraintLayout.Widget;


namespace v4posme_maui
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class MainActivity : MauiAppCompatActivity
	{
		private const int RequestLocationId = 0;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
			{
				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted)
				{
					ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.BluetoothConnect }, 1);
				}

				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
				{
					// Si el permiso no está concedido, solicitarlo
					ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, 1);
				}

				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
				{
					// Si el permiso no está concedido, solicitarlo
					ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessCoarseLocation }, 1);
				}

				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessBackgroundLocation) != Permission.Granted)
				{
					// Si el permiso no está concedido, solicitarlo
					ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessBackgroundLocation }, 1);
				}

				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ForegroundServiceLocation) != Permission.Granted)
				{
					// Si el permiso no está concedido, solicitarlo
					ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ForegroundServiceLocation }, 1);
				}

				var channel = new NotificationChannel("LocationServiceChannel", "Location Service Channel", NotificationImportance.Default);
				var manager = GetSystemService(NotificationService) as NotificationManager;
				manager?.CreateNotificationChannel(channel);
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			if (requestCode == 1)
			{
				if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
				{
					// Permiso concedido
				}
			}
		}
	}
}
