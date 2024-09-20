namespace v4posme_maui.Models;

public record DtoCatalogItem(int Key, string Name, string Simbolo)
{
    public override string ToString() => Name;
}