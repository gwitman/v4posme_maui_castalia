namespace v4posme_maui.Models;

public record ViewTempDtoAbono(
    string CodigoAbono,
    int EntityId,
    string FirstName,
    string LastName,
    string Identification,
    DateTime Fecha,
    string? DocumentNumber,
    string CurrencyName,
    decimal MontoAplicar,
    decimal SaldoInicial,
    decimal SaldoFinal,
    string Description)
{
    public decimal MoraPagada { get; set; }
    public string? Documentos { get; set; }
    public int DiasMora { get; set; }
    public decimal MontoMora { get; set; }
    public int CuotasPendientes { get; set; }
}