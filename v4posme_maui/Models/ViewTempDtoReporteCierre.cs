namespace v4posme_maui.Models
{
	public class ViewTempDtoReporteCierre
	{
		public string DocumentNumber { get; set; } = null!;
		public string CurrencyName { get; set; } = null!;
		public int CurrencyId { get; set; }
		public decimal Remaining { get; set; }
		public string ClientName { get; set; } = null!;
		public int ClientId { get; set; }
		public string Date { get; set; } = null!;
	}
}
