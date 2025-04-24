using System.Runtime.Versioning;
using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.Services
{
    [SupportedOSPlatform("android31.0")]
    [Service(Exported = false, Enabled = true, Name = "com.Services.GPSService", ForegroundServiceType = ForegroundService.TypeLocation)]
    public class GPSService : Service, ILocationListener
    {
        private const int SERVICE_ID = Constantes.GpsServicesId;
        private const string SERVICE_NOTIFICATION_CHANNEL_ID = Constantes.GpsServicesChanelId;
        private const string TAG = Constantes.TagGps;
        private LocationManager? _locationManager;
        private HelperCore? _helper;
        
        public override void OnCreate()
        {
            base.OnCreate();
            _locationManager = GetSystemService(LocationService) as LocationManager;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Task.Run(async () =>
            {
                var hasPermission = await PermissionsService.RequestLocationPermissionAsync();

                //  Build the notification for the foreground service
                var notification = BuildNotification();
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    if (hasPermission)
                    {
                        StartForeground(SERVICE_ID, notification, ForegroundService.TypeLocation);
                    }
                }

                RequestLocationUpdates();
            });

            //  Return a sticky result so that the service remains running
            return StartCommandResult.Sticky;
        }

        public override IBinder? OnBind(Intent? intent) => null;
        
        private void BackgroundWorkerOnWorkerStopped(object sender, EventArgs e)
        {
            StopForeground(removeNotification: true);
            StopSelf();
        }

        private Notification BuildNotification()
        {
            // Building intent
            var notificationBuilder = new Notification.Builder(this, Constantes.GpsNameChangelNotification)
                .SetContentTitle(Constantes.GpsTitleContentNotification)
                .SetContentText(Constantes.GpsTextContentNotification)
                .SetSmallIcon(ResourceConstant.Drawable.notification_action_background); // Cambia icon por tu recurso de ícono.

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
            var criteria                        = new Criteria { Accuracy = Accuracy.Fine };
            HelperCore _helperTemporal          = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
            var timesString                     = await _helperTemporal.GetValueParameter("MOBILE_SYNC_GPS", "86400000"); //1 dia
            var time                            = Convert.ToInt64(timesString);
            _locationManager.RequestLocationUpdates(time, 0, criteria, this, Looper.MainLooper);
        }

        private async void SendLocationToApi(double latitude, double longitude)
        {
            try
            {
                //Enviar a comercio gps
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
                    new("txtReference1", " "),
                    new("txtCompanyName", VariablesGlobales.CompanyKey!.ToString())
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


                //Enviar a PosMe Gps
                var httpClientPosme = new HttpClient();
                tempUrl = Constantes.UrlBasePosme + Constantes.UrlGpSShareOnly;

                if (string.IsNullOrEmpty(tempUrl))
                {
                    return;
                }

                nickname = VariablesGlobales.User!.Nickname!;
                password = VariablesGlobales.User!.Password!;

                var nvcPosme = new List<KeyValuePair<string, string>>
                {
                    new("txtNickname", nickname),
                    new("txtPassword", password),
                    new("txtLatituded", latitude.ToString()),
                    new("txtLongituded", longitude.ToString()),
                    new("txtReference1", " "),
                    new("txtCompanyName", VariablesGlobales.CompanyKey!.ToString())
                };
                var reqPosme = new HttpRequestMessage(HttpMethod.Post, tempUrl)
                {
                    Content = new FormUrlEncodedContent(nvcPosme)
                };

                try
                {
                    var response = await httpClientPosme.SendAsync(reqPosme);
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
            catch (Exception e)
            {
                Log.Error("Error sending location updates", e.Message);
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