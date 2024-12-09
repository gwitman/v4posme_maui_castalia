using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v4posme_maui.ViewModels.More
{
	public class MoreViewModel : BaseViewModel
	{

		public void OnAppearing(INavigation navigation)
		{
			Navigation = navigation;
			IsBusy = false;
		}
	}
}
