using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v4posme_maui.Services
{
	/*[Service(ForegroundServiceType = ForegroundService.TypeLocation)]
	public class BackgroundGeolocationService : Service
	{
		const int _channelId = 785123754;
		const int _serviceId = 19461923;
		bool _isStarted;
		Handler _handler;
		Action _runnable;
		private readonly LocationSevice _locationSevice;

		NotificationChannel? _channel;
		PowerManager.WakeLock partialWakeLock;

		public BackgroundGeolocationService(LocationSevice locationSevice)
		{
			_locationSevice = locationSevice;
		}

		public override void OnCreate()
		{
			_handler = new Handler(Looper.MainLooper!);
			_runnable = new Action(() =>
			{
				if (_isStarted)
				{
					Execute();
					_handler.PostDelayed(_runnable, 1000);
				}
			});
			base.OnCreate();
		}

		public override IBinder? OnBind(Intent? intent)
		{
			return null;
		}

		void Execute()
		{

		}

		public override StartCommandResult OnStartCommand(
	   Intent? intent,
	   StartCommandFlags flags,
	   int startId
   )
		{
			// Acquire a partial WakeLock
			var wakeLock = (PowerManager)GetSystemService(PowerService);
			partialWakeLock = wakeLock.NewWakeLock(WakeLockFlags.Partial, "BackgroundGeolocationService");
			partialWakeLock.Acquire();

			if (intent == null)
			{
				*//*intent = new Intent(GooglePlayLocationService.Context, typeof(BackgroundGeolocationService))
					.AddFlags(ActivityFlags.ReceiverForeground);*//*

				// TODO add logging

			}

			if (_isStarted)
			{
				// Do Nothing
			}
			else
			{
				StartInForeground(null, intent);
				_handler.PostDelayed(_runnable, 1000);
				_isStarted = true;
			}

			return StartCommandResult.Sticky;
		}

		void StartInForeground(Context? context, Intent? intent)
		{
			var pendingIntent = PendingIntent
				.GetActivity(context, 0, intent, PendingIntentFlags.Mutable);

			var builder = new NotificationCompat.Builder(context, _channelId.ToString())
				.SetContentTitle("DSP mobile is Tracking You")
				.SetContentText("Your location is being tracked")
				.SetOngoing(true)
				.SetContentIntent(pendingIntent);

			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				_channel ??=
					new NotificationChannel(_channelId.ToString(), "Title", NotificationImportance.High);
				_channel.Importance = NotificationImportance.High;
				_channel.EnableLights(true);
				_channel.EnableVibration(true);
				_channel.SetShowBadge(true);
				_channel.SetVibrationPattern(new long[] { 100, 200, 300 });

				var notificationManager = GetSystemService(NotificationService) as NotificationManager;

				if (notificationManager != null)
				{
					builder.SetChannelId(_channelId.ToString());
					notificationManager.CreateNotificationChannel(_channel);
				}
			}

			StartForeground(_serviceId, builder.Build());
		}

		public override ComponentName? StartForegroundService(Intent? service)
		{
			return base.StartForegroundService(service);
		}

		public override void OnTaskRemoved(Intent rootIntent)
		{
			StopForeground(StopForegroundFlags.Remove);
			// Release the partial WakeLock
			if (partialWakeLock != null && partialWakeLock.IsHeld)
			{
				partialWakeLock.Release();
			}
			base.OnTaskRemoved(rootIntent);
		}
	}*/
}
