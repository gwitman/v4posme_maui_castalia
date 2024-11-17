using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace v4posme_maui.Services
{
	public class LocationSevice
	{
		public string Message { get; private set; } = string.Empty;

		public Xamarin.Essentials.Location? Location { get; private set; }

		public async Task<LocationSevice> ObtenerUbicacionAsync()
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
	}
}