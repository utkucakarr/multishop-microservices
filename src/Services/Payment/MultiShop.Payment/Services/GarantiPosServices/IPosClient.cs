namespace MultiShop.Payment.Services.GarantiPosServices
{
    /// <summary>
    /// Sanal POS'a satış isteği. Uygulaması <see cref="GarantiPosClient"/>; arayüz, ödeme akışının testte sahte bir bankayla
    /// çalışabilmesi için (KT-16).
    /// </summary>
    public interface IPosClient
    {
        /// <param name="orderId">Bankadaki sipariş kimliği; her deneme için farklı olmalı.</param>
        /// <param name="amount">TL.</param>
        /// <exception cref="GarantiPosException">Banka cevap vermedi ya da cevap okunamadı (sonuç belirsiz).</exception>
        Task<GarantiPosResult> SaleAsync(string orderId, decimal amount, CardDetails card);
    }
}
