using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v4posme_maui.Services
{
	public interface IBackgroundWorker
	{
		/// <summary>
		/// Worker Stopped Event
		/// </summary>
		public event EventHandler WorkerStopped;

		/// <summary>
		/// Actual Background Work
		/// </summary>
		public Func<Task> BackgroundWork { get; }

		/// <summary>
		/// Start Native Background or Foreground Service
		/// </summary>
		void StartWorker(Func<Task> backgroundWork);

		/// <summary>
		/// Force stop worked foreground or background service
		/// </summary>
		void StopWorker();
	}
}
