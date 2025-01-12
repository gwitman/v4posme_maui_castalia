using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services;

public class PermissionsService
{
    public static async Task<bool> RequestLocationPermissionAsync()
    {
        // Verifica el estado del permiso FINE_LOCATION
        var fineLocationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (fineLocationStatus != PermissionStatus.Granted)
        {
            // Solicita el permiso
            fineLocationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        // Verifica si el permiso fue otorgado
        return fineLocationStatus == PermissionStatus.Granted;
    }
    
    public static async Task<bool> CheckAndRequestPermissionsAsync()
    {
        var permissions = new Permissions.BasePlatformPermission[]
        {
            new Permissions.LocationWhenInUse(),
            new Permissions.LocationAlways(),
            new Permissions.Camera(),
            new Permissions.StorageRead(),
            new Permissions.StorageWrite(),
            new Permissions.Bluetooth(),
            new Permissions.Media(),
            new Permissions.Photos(),
            new Permissions.PhotosAddOnly()
        };

        var allPermissionsGranted = true;

        foreach (var permission in permissions)
        {
            var status = await permission.CheckStatusAsync();
            if (status == PermissionStatus.Granted) continue;
            // Solicitar el permiso si no está otorgado
            status = await permission.RequestAsync();
            if (status == PermissionStatus.Granted) continue;
            allPermissionsGranted = false;
            await Application.Current?.MainPage?.DisplayAlert(
                "Permiso denegado",
                Mensajes.MessagePermission.Replace("{name}", permission.GetType().Name),
                "OK")!;
        }

        return allPermissionsGranted;
    }
}