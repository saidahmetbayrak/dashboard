using Dashboard.Models.Response;
using Dashboard.Models.Filter;
using Dashboard.Models.Cart;
using Dashboard.Models.Customer;
using Dashboard.Models.Location;
using Dashboard.Web.Models;

namespace Dashboard.Web.Services
{
    /// <summary>
    /// API servis interface'i
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// Chart verilerini getirir
        /// </summary>
        /// <param name="filter">Filtre parametreleri</param>
        /// <returns>Chart verileri</returns>
        Task<WebApiResponse<DashboardChartData>> GetChartDataAsync(DashboardFilter filter);

        /// <summary>
        /// Sepet verilerini getirir
        /// </summary>
        /// <param name="filter">Filtre parametreleri</param>
        /// <param name="pagination">Sayfalama parametreleri</param>
        /// <returns>Sepet verileri</returns>
        Task<WebApiResponse<PagedResponse<CartItem>>> GetCartItemsAsync(DashboardFilter filter, PaginationFilter pagination);

        /// <summary>
        /// Müşteri verilerini getirir
        /// </summary>
        /// <param name="filter">Filtre parametreleri</param>
        /// <param name="pagination">Sayfalama parametreleri</param>
        /// <returns>Müşteri verileri</returns>
        Task<WebApiResponse<PagedResponse<Customer>>> GetCustomersAsync(DashboardFilter filter, PaginationFilter pagination);

        /// <summary>
        /// Autocomplete önerileri getirir
        /// </summary>
        /// <param name="request">Autocomplete isteği</param>
        /// <returns>Öneri listesi</returns>
        Task<WebApiResponse<List<AutocompleteItem>>> GetAutocompleteAsync(AutocompleteRequest request);

        /// <summary>
        /// Dropdown verilerini getirir
        /// </summary>
        /// <param name="field">Alan adı</param>
        /// <param name="index">Index adı</param>
        /// <returns>Dropdown öğeleri</returns>
        Task<WebApiResponse<List<DropdownItem>>> GetDropdownDataAsync(string field, string index);

        /// <summary>
        /// Lokasyon mapping verilerini getirir
        /// </summary>
        /// <returns>Lokasyon mappings</returns>
        Task<WebApiResponse<LocationMappings>> GetLocationMappingsAsync();

        /// <summary>
        /// Tüm illeri getirir
        /// </summary>
        /// <returns>İl listesi</returns>
        Task<WebApiResponse<List<ProvinceItem>>> GetProvincesAsync();

        /// <summary>
        /// Belirtilen ile ait ilçeleri getirir
        /// </summary>
        /// <param name="provinceCode">İl kodu</param>
        /// <returns>İlçe listesi</returns>
        Task<WebApiResponse<List<DistrictItem>>> GetDistrictsAsync(string provinceCode);

        /// <summary>
        /// API sağlık kontrolü yapar (basit boolean response)
        /// Program.cs startup health check için kullanılır
        /// </summary>
        /// <returns>API sağlıklı ise true, değilse false</returns>
        Task<bool> HealthCheckAsync();

        /// <summary>
        /// API sağlık kontrolü yapar (detaylı response)
        /// Controller'lar ve detaylı health check için kullanılır
        /// </summary>
        /// <returns>Detaylı sağlık durumu bilgisi</returns>
        Task<WebApiResponse<bool>> HealthCheckDetailedAsync();
    }
}