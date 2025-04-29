using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services;

namespace v4posme_maui.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy=true;
        private Color _popupBackgroundColor = Colors.White;
        private string _title = string.Empty;
        private string _search = string.Empty;
        private bool _popUpShow;
        private string? _barCode;
        protected INavigationService NavigationService => DependencyService.Get<INavigationService>();

        public async void ShowToast(string mensaje, ToastDuration duration, double fontSize)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var toast = Toast.Make(mensaje, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }

        protected void ShowMensajePopUp(string mensaje, Color? color=null)
        {
            Mensaje = mensaje;
            PopupBackgroundColor = color ?? Colors.Red;
            PopUpShow = true;
        }
        public bool IsBusy
        {
            get => _isBusy;
            protected set => SetProperty(ref _isBusy, value);
        }

        public Color PopupBackgroundColor
        {
            get => _popupBackgroundColor;
            set => SetProperty(ref _popupBackgroundColor, value);
        }

        public bool PopUpShow
        {
            get => _popUpShow;
            set => SetProperty(ref _popUpShow, value);
        }
        
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string? BarCode
        {
            get => _barCode;
            set => SetProperty(ref _barCode, value);
        }

        public string Search
        {
            get => _search;
            set => SetProperty(ref _search, value);
        }

        private string? _mensaje;

        public string? Mensaje
        {
            get => _mensaje;
            set => SetProperty(ref _mensaje, value);
        }

        private INavigation? _navigation;

        protected INavigation? Navigation
        {
            get => _navigation;
            set => SetProperty(ref _navigation, value);
        }

        private TbCompany? _company;

        protected TbCompany? Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }
        
        public virtual Task InitializeAsync(object parameter)
        {
            return Task.CompletedTask;
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected Task OpenUrl(string url)
        {
            return Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
    }
}