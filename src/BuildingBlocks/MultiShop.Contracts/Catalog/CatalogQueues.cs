namespace MultiShop.Contracts.Catalog;

/// <summary>
/// Catalog'un komut kuyrukları. Komutun tek alıcısı olduğu için gönderen (Order saga'sı) <c>Send</c> ile doğrudan bu
/// kuyruğa yazar (<c>queue:catalog-reserve-stock</c>); Catalog endpoint'lerini aynı adlarla açar.
/// </summary>
public static class CatalogQueues
{
    public const string ReserveStock = "catalog-reserve-stock";
    public const string CommitStock = "catalog-commit-stock";
    public const string ReleaseStock = "catalog-release-stock";
}
