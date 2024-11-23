using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Runtime.Versioning;
using Unity;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using Debug = System.Diagnostics.Debug;

namespace v4posme_maui.Services
{
	[SupportedOSPlatform("android31.0")]
	[Service(Exported = false, Enabled = true, Name = "com.Services.GPSService", ForegroundServiceType = Android.Content.PM.ForegroundService.TypeLocation)]
	public class GPSService : Service, ILocationListener
	{
		private const int SERVICE_ID = 7070;

		private const string SERVICE_NOTIFICATION_CHANNEL_ID = "7071";

		private LocationManager? _locationManager;

		private HelperCore? _helper;

		private const string TAG = "LocationService";

		public string Message { get; private set; } = string.Empty;

		public Xamarin.Essentials.Location? Location { get; private set; }

		public override void OnCreate()
		{
			base.OnCreate();
			_locationManager = GetSystemService(LocationService) as LocationManager;
		}

		[return: GeneratedEnum]
		public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
		{
			//  Build the notification for the foreground service
			var notification = BuildNotification();
			if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
			{
				StartForeground(SERVICE_ID, notification, Android.Content.PM.ForegroundService.TypeLocation);
			}
			else
			{
				StartForeground(SERVICE_ID, notification);
			}

			RequestLocationUpdates();

			//  Return a sticky result so that the service remains running
			return StartCommandResult.Sticky;
		}

		public override Android.OS.IBinder? OnBind(Intent? intent) => null;

		public async Task<GPSService> ObtenerUbicacionAsync()
		{
			try
			{
				var ubicacion = await Xamarin.Essentials.Geolocation.GetLastKnownLocationAsync();

				if (ubicacion is null)
				{
					ubicacion = await Xamarin.Essentials.Geolocation.GetLocationAsync(
						new Xamarin.Essentials.GeolocationRequest
						{
							DesiredAccuracy = Xamarin.Essentials.GeolocationAccuracy.Medium,
							Timeout = TimeSpan.FromSeconds(30)
						}
					);
				}

				return new()
				{
					Location = ubicacion,
					Message = "",
				};
			}
			catch (Xamarin.Essentials.FeatureNotSupportedException fnsEx)
			{
				Console.WriteLine("GPS no soportado en este dispositivo");
				Message = fnsEx.Message;
				Debug.WriteLine(fnsEx);
			}
			catch (Xamarin.Essentials.FeatureNotEnabledException fneEx)
			{
				Console.WriteLine("GPS no habilitado");
				Message = fneEx.Message;
				Debug.WriteLine(fneEx);
			}
			catch (Xamarin.Essentials.PermissionException pEx)
			{
				Console.WriteLine("Permisos denegados"); // Manejar excepción
				Message = pEx.Message;
				Debug.WriteLine(pEx);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ocurrió un error al obtener la ubicación: {ex.Message}"); // Manejar excepción
				Message = ex.Message;
				Debug.Write(ex);
			}

			return new()
			{
				Location = null,
				Message = Message
			};
		}

		public async Task<bool> HasLocationPermission()
		{
			try
			{
				var status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationPermission>();

				if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
				{
					if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
					{
						return false;
					}

					status = await CrossPermissions.Current.RequestPermissionAsync<LocationPermission>();
				}

				if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
				{
					return true;
				}
				else if (status != Plugin.Permissions.Abstractions.PermissionStatus.Unknown)
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ocurrió un error al obtener la ubicación: {ex.Message}"); // Manejar excepción
				Message = ex.Message;
				Debug.Write(ex);
			}

			return false;
		}

		private void BackgroundWorkerOnWorkerStopped(object sender, EventArgs e)
		{
			StopForeground(removeNotification: true);
			StopSelf();
		}

		private Notification BuildNotification()
		{
			// Building intent
			var notificationBuilder = new Notification.Builder(this, "LocationServiceChannel")
		 .SetContentTitle("Tracking Location")
		 .SetContentText("Sending location data every 20 minutes")
		 .SetSmallIcon(Resource.Drawable.notification_action_background); // Cambia icon por tu recurso de ícono.

			return notificationBuilder.Build();
		}

		public void OnLocationChanged(Android.Locations.Location location)
		{
			Log.Debug(TAG, $"Location: {location.Latitude}, {location.Longitude}");
			SendLocationToApi(location.Latitude, location.Longitude);
		}

		public void OnProviderDisabled(string provider)
		{
		}

		public void OnProviderEnabled(string provider)
		{
		}

		public void OnStatusChanged(string? provider, [GeneratedEnum] Availability status, Bundle? extras)
		{
		}

		private async void RequestLocationUpdates()
		{
			var criteria = new Criteria { Accuracy = Accuracy.Fine };
			//var valueTime = await _helper.GetValueParameter("MOBILE_SYNC_GPS", "20");

			_locationManager.RequestLocationUpdates(60000 * 3, 0, criteria, this, Looper.MainLooper);
		}

		private async void SendLocationToApi(double latitude, double longitude)
		{
			var httpClient = new HttpClient();
			
			var tempUrl = Constantes.UrlGPSShare.Replace("{UrlBase}", VariablesGlobales.CompanyKey);

			if (string.IsNullOrEmpty(tempUrl))
			{
				return;
			}

			var nickname = VariablesGlobales.User!.Nickname!;
			var password = VariablesGlobales.User!.Password!;

			var nvc = new List<KeyValuePair<string, string>>
			{
				new("txtNickname", nickname),
				new("txtPassword", password),
				new("txtLatituded", latitude.ToString()),
				new("txtLongituded", longitude.ToString()),
				new("txtReference1", " ")
			};
			var req = new HttpRequestMessage(HttpMethod.Post, tempUrl)
			{
				Content = new FormUrlEncodedContent(nvc)
			};

			try
			{
				var response = await httpClient.SendAsync(req);
				if (!response.IsSuccessStatusCode)
				{
					Log.Debug(TAG, $"API Response: {response.StatusCode}");
					return;
				}

				Log.Debug(TAG, $"API Response: {response.StatusCode}");
			}
			catch (Exception ex)
			{
				Log.Error(TAG, $"Error sending location: {ex.Message}");
			}

		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			_locationManager.RemoveUpdates(this);
			Log.Debug(TAG, "Service destroyed");
		}

	}
}