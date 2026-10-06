namespace MultiShop.Catalog.Exceptions
{
    public class CatalogDomainException : Exception
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
