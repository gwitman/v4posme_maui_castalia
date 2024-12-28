using Android.OS;
using Plugin.Permissions;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services
{
	public class RequestPermissionResult
	{
		public bool Exito { get; set; }
		public string Mensaje { get; set; }
	}

	public class RequestPermissionService
	{
		public static async Task<RequestPermissionResult> SolicitarPermisoAsync<TPermission>(Plugin.Permissions.Abstractions.Permission permission)
			where TPermission : BasePermission, new()
		{
			try
			{
				// Verificar el estado actual del permiso
				var status = await CrossPermissions.Current.CheckPermissionStatusAsync<TPermission>();

				// Si el permiso no está concedido
				if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
				{
					// Solicitar el permiso
					status = await CrossPermissions.Current.RequestPermissionAsync<TPermission>();
				}

				// Retornar true si el permiso fue concedido
				var result = status == Plugin.Permissions.Abstractions.PermissionStatus.Granted;
				return new RequestPermissionResult()
				{
					Exito = result,
					Mensaje = result ? string.Empty : Mensajes.MessagePermissionShouldAccept
				};
			}
			catch (Exception ex)
			{
				// Capturar cualquier excepción y mostrarla en los registros
				System.Diagnostics.Debug.WriteLine("Error solicitando permiso: " + ex.Message);

				return new RequestPermissionResult()
				{
					Exito = false,
					Mensaje = ex.Message
				};
			}
		}

		public static async Task<RequestPermissionResult> ShouldShowRequestPermission<TPermission>(Plugin.Permissions.Abstractions.Permission permission)
			where TPermission : BasePermission, new()
		{
			var canRequest = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(permission);

			if (!canRequest)
			{
				return new RequestPermissionResult()
				{
					Exito = false,
					Mensaje = Mensajes.MessagePermissionShouldAccept
				};
			}

			return new RequestPermissionResult()
			{
				Exito = true,
				Mensaje = string.Empty
			};
		}

		public static void OpenSettings() => CrossPermissions.Current.OpenAppSettings();

		public static async Task<bool> CheckStatusPermission<TPermission>(PermissionStatus status) 
			where TPermission : Permissions.BasePermission, new()
		{
			if (status != PermissionStatus.Granted)
			{
				status = await Permissions.RequestAsync<TPermission>();
			}

			if(status == PermissionStatus.Denied)
			{
				return Permissions.ShouldShowRationale<TPermission>();
			}

			return status == PermissionStatus.Granted;
		}
	}
}
