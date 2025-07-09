using Dashboard.Models.Response;
using Dashboard.Models.Location;

namespace Dashboard.Web.Models
{
    /// <summary>
    /// Dashboard ana sayfa view model
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Elasticsearch bağlantı durumu
        /// </summary>
        public bool IsElasticsearchConnected { get; set; }

        /// <summary>
        /// İl listesi
        /// </summary>
        public List<ProvinceItem> Provinces { get; set; } = new();

        /// <summary>
        /// Sevkiyat depo listesi
        /// </summary>
        public List<DropdownItem> DepotList { get; set; } = new();

        /// <summary>
        /// Sayfa başlığı
        /// </summary>
        public string Title { get; set; } = "Sepet Analytics Dashboard";

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Başarı mesajı (varsa)
        /// </summary>
        public string? SuccessMessage { get; set; }
    }

    /// <summary>
    /// Hata sayfası view model
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Request ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Request ID gösterilecek mi?
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Hata mesajı
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Sayfa başlığı
        /// </summary>
        public string Title { get; set; } = "Hata";
    }

    /// <summary>
    /// API response wrapper
    /// </summary>
    /// <typeparam name="T">Response data tipi</typeparam>
    public class WebApiResponse<T>
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
        /// Hata mesajı
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// AJAX request base model
    /// </summary>
    public class AjaxRequestBase
    {
        /// <summary>
        /// Request timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request ID (tracking için)
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// AJAX response base model
    /// </summary>
    public class AjaxResponseBase
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Mesaj
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Response timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request ID
        /// </summary>
        public string? RequestId { get; set; }
    }

    /// <summary>
    /// AJAX data response
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    public class AjaxDataResponse<T> : AjaxResponseBase
    {
        /// <summary>
        /// Response verisi
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Başarılı response oluşturur
        /// </summary>
        public static AjaxDataResponse<T> Success(T data, string? message = null)
        {
            return new AjaxDataResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Hatalı response oluşturur
        /// </summary>
        public static AjaxDataResponse<T> Error(string message)
        {
            return new AjaxDataResponse<T>
            {
                IsSuccess = false,
                Message = message
            };
        }
    }

    /// <summary>
    /// Chart data request model
    /// </summary>
    public class ChartDataRequest : AjaxRequestBase
    {
        /// <summary>
        /// Müşteri numarası
        /// </summary>
        public string? MusteriNo { get; set; }

        /// <summary>
        /// Kullanıcı kodu
        /// </summary>
        public string? KullaniciKodu { get; set; }

        /// <summary>
        /// Malzeme numarası
        /// </summary>
        public string? MalzemeNo { get; set; }

        /// <summary>
        /// İl kodu
        /// </summary>
        public string? Il { get; set; }

        /// <summary>
        /// İlçe kodu
        /// </summary>
        public string? Ilce { get; set; }

        /// <summary>
        /// Sevkiyat depo kodu
        /// </summary>
        public string? SevkiyatDepoKodu { get; set; }

        /// <summary>
        /// Tarih aralığı (ay olarak)
        /// </summary>
        public int DateRangeMonths { get; set; } = 3;
    }

    /// <summary>
    /// Table data request model
    /// </summary>
    public class TableDataRequest : ChartDataRequest
    {
        /// <summary>
        /// Sayfa numarası
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Sayfa boyutu
        /// </summary>
        public int Size { get; set; } = 50;

        /// <summary>
        /// Sıralama alanı
        /// </summary>
        public string? SortField { get; set; }

        /// <summary>
        /// Sıralama yönü
        /// </summary>
        public string SortDirection { get; set; } = "desc";

        /// <summary>
        /// Search after değeri
        /// </summary>
        public object[]? SearchAfter { get; set; }
    }

    /// <summary>
    /// Autocomplete request model
    /// </summary>
    public class AutocompleteWebRequest : AjaxRequestBase
    {
        /// <summary>
        /// Arama terimi
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Alan adı
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
}