using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v4posme_maui.Models
{
	public class DtoVisita : INotifyPropertyChanged
	{

		private string _comentario;

		private DateTime _fechaVisita;

		private string _tipificacion;

		public string Comentario
		{
			get => _comentario;
			set
			{
				if (_comentario != value)
				{
					_comentario = value;
					OnPropertyChanged(nameof(_comentario));
				}
			}
		}

		public DateTime FechaVisita
		{
			get => _fechaVisita;
			set
			{

				if (_fechaVisita != value)
				{
					_fechaVisita = value;
					OnPropertyChanged(nameof(_fechaVisita));
				}
			}
		}

		public string Tipificacion
		{
			get => _tipificacion;
			set
			{
				if (_tipificacion != value)
				{
					_tipificacion = value;
					OnPropertyChanged(nameof(_tipificacion));
				}
			}
		}

		public Api_AppMobileApi_GetDataDownloadCustomerResponse Customer { get; set; }

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
