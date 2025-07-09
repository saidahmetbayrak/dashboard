namespace Dashboard.Models.Response
{
    /// <summary>
    /// API genel response modeli
    /// </summary>
    /// <typeparam name="T">Response data tipi</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response verisi
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Hata detayları (varsa)
        /// </summary>
        public object? Errors { get; set; }

        /// <summary>
        /// Başarılı response oluşturur
        /// </summary>
        public static ApiResponse<T> SuccessResult(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Hatalı response oluşturur
        /// </summary>
        public static ApiResponse<T> ErrorResult(string message, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Sayfalı veri response modeli
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// Veri listesi
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Toplam kayıt sayısı
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// Toplam sayı tahmini mi?
        /// </summary>
        public string Relation { get; set; } = "eq"; // "eq" or "gte"

        /// <summary>
        /// Sayfa numarası
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Sayfa boyutu
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Sonraki sayfa var mı?
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Son kaydın sort değeri (pagination için)
        /// </summary>
        public object[]? SearchAfter { get; set; }
    }

    /// <summary>
    /// Chart veri noktası
    /// </summary>
    public class ChartDataPoint
    {
        /// <summary>
        /// Etiket/kategori
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Değer
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// Anahtar (orijinal değer)
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Tarih bilgisi (zaman serisi için)
        /// </summary>
        public DateTime? Date { get; set; }
    }

    /// <summary>
    /// Dashboard chart verileri
    /// </summary>
    public class DashboardChartData
    {
        /// <summary>
        /// Günlük sepet trendi
        /// </summary>
        public List<ChartDataPoint> DailyCartTrend { get; set; } = new();

        /// <summary>
        /// Aylık sepet trendi
        /// </summary>
        public List<ChartDataPoint> MonthlyTrend { get; set; } = new();

        /// <summary>
        /// En çok eklenen ürünler
        /// </summary>
        public List<ChartDataPoint> TopProducts { get; set; } = new();

        /// <summary>
        /// En aktif müşteriler
        /// </summary>
        public List<ChartDataPoint> TopCustomers { get; set; } = new();

        /// <summary>
        /// Depo dağılımı
        /// </summary>
        public List<ChartDataPoint> DepotDistribution { get; set; } = new();

        /// <summary>
        /// İl bazında performans
        /// </summary>
        public List<ChartDataPoint> ProvincePerformance { get; set; } = new();
    }

    /// <summary>
    /// Dropdown öğesi
    /// </summary>
    public class DropdownItem
    {
        /// <summary>
        /// Değer
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Görüntülenecek text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Kayıt sayısı
        /// </summary>
        public long Count { get; set; }
    }
}