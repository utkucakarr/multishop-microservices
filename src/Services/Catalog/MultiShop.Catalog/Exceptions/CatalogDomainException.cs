using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Catalog.Exceptions
{
    // DomainException'dan türediği için ortak hata middleware'i 400 Bad Request döner.
    public class CatalogDomainException : DomainException
    {
        // 1. Parametresiz kurucu (Sadece hata fırlatmak istendiğinde)
        public CatalogDomainException()
        { }

        // 2. Sadece mesaj alan kurucu
        public CatalogDomainException(string message) 
            : base(message)
        { }

        // 3. Mesaj ve iç hata (Inner Exception) alan kurucu
        // (Başka bir hatayı sarmalayıp yukarı fırlatmak gerektiğinde kullanır)
        public CatalogDomainException(string message, Exception innerException)
            : base (message, innerException)
        { }
    }
}
