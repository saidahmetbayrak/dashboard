namespace Dashboard.Models.Filter
{
    /// <summary>
    /// Dashboard filtre parametreleri
    /// </summary>
    public class DashboardFilter
    {
        /// <summary>
        /// Müşteri numarası filtresi
        /// </summary>
        public string? MusteriNo { get; set; }

        /// <summary>
        /// Kullanıcı kodu filtresi
        /// </summary>
        public string? KullaniciKodu { get; set; }

        /// <summary>
        /// Malzeme numarası filtresi
        /// </summary>
        public string? MalzemeNo { get; set; }

        /// <summary>
        /// İl filtresi
        /// </summary>
        public string? Il { get; set; }

        /// <summary>
        /// İlçe filtresi
        /// </summary>
        public string? Ilce { get; set; }

        /// <summary>
        /// Sevkiyat depo kodu filtresi
        /// </summary>
        public string? SevkiyatDepoKodu { get; set; }

        /// <summary>
        /// Tarih aralığı filtresi
        /// </summary>
        public DateRangeFilter? DateRange { get; set; }
    }

    /// <summary>
    /// Tarih aralığı filtre modeli
    /// </summary>
    public class DateRangeFilter
    {
        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Bitiş tarihi
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Tarih alanı adı
        /// </summary>
        public string Field { get; set; } = "SonIslemTarihi";
    }

    /// <summary>
    /// Sayfalama parametreleri
    /// </summary>
    public class PaginationFilter
    {
        /// <summary>
        /// Sayfa numarası (1-based)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Sayfa başına kayıt sayısı
        /// </summary>
        public int Size { get; set; } = 50;

        /// <summary>
        /// Sıralama alanı
        /// </summary>
        public string? SortField { get; set; }

        /// <summary>
        /// Sıralama yönü (asc/desc)
        /// </summary>
        public string SortDirection { get; set; } = "desc";

        /// <summary>
        /// Search after değeri (Elasticsearch pagination için)
        /// </summary>
        public object[]? SearchAfter { get; set; }
    }

    /// <summary>
    /// Autocomplete arama parametreleri
    /// </summary>
    public class AutocompleteRequest
    {
        /// <summary>
        /// Arama terimi
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Arama yapılacak alan
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Index adı
        /// </summary>
        public string Index { get; set; } = string.Empty;

        /// <summary>
        /// Maksimum sonuç sayısı
        /// </summary>
        public int Size { get; set; } = 10;
    }

    /// <summary>
    /// Autocomplete sonuç öğesi
    /// </summary>
    public class AutocompleteItem
    {
        /// <summary>
        /// Metin değeri
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Döküman sayısı
        /// </summary>
        public long Count { get; set; }
    }
}