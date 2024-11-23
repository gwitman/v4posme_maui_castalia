using Android.Content;
using Android.OS;

namespace v4posme_maui.Services
{
	public class BackgroundWorker : IBackgroundWorker
	{
		/// <summary>
		/// Worker Stopped Event
		/// </summary>
		public event EventHandler WorkerStopped;

		/// <summary>
		/// Actual Background Work
		/// </summary>
		public Func<Task> BackgroundWork { get; private set; }

		public BackgroundWorker()
		{
			StartWorker(BackgroundWork);

		}
		/// <summary>
		/// Start Native Background or Foreground Service
		/// </summary>
		public void StartWorker(Func<Task> backgroundWork)
		{
			BackgroundWork = backgroundWork;
			var intent = new Intent(Android.App.Application.Context, typeof(GPSService));

			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				Android.App.Application.Context?.StartForegroundService(intent);
			}
			else
			{
				Android.App.Application.Context.StartService(intent);
			}
		}

		/// <summary>
		/// Force stop worked foreground or background service
		/// </summary>
		public void StopWorker()
		{
			WorkerStopped?.Invoke(this, EventArgs.Empty);
		}

	}
}
