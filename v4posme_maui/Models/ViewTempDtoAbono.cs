namespace v4posme_maui.Models;

public record ViewTempDtoAbono(
    string CodigoAbono,
    int EntityId,
    string FirstName,
    string LastName,
    string Identification,
    DateTime Fecha,
    string DocumentNumber,
    string CurrencyName,
    decimal MontoAplicar,
    decimal SaldoInicial,
    decimal SaldoFinal,
    string Description);