using Dashboard.Models.Filter;
using Dashboard.Models.Response;
using Dashboard.Models.Cart;
using Dashboard.Models.Customer;

namespace Dashboard.Api.Services
{
    /// <summary>
    /// Elasticsearch servis interface'i
    /// </summary>
    public interface IElasticsearchService
    {
        /// <summary>
        /// Sepet verilerini filtreli olarak getirir
        /// </summary>
        /// <param name="filter">Filtre parametreleri</param>
        /// <param name="pagination">Sayfalama parametreleri</param>
        /// <returns>Sayfalı sepet verileri</returns>
        Task<PagedResponse<CartItem>> GetCartItemsAsync(DashboardFilter filter, PaginationFilter pagination);

        /// <summary>
        /// Müşteri verilerini filtreli olarak getirir
        /// </summary>
        /// <param name="filter">Filtre parametreleri</param>
        /// <param name="pagination">Sayfalama parametreleri</param>
        /// <returns>Sayfalı müşteri verileri</returns>
        Task<PagedResponse<Customer>> GetCustomersAsync(DashboardFilter filter, PaginationFilter pagination);

        /// <summary>
        /// Dashboard chart verilerini getirir
        /// </summary>
        /// <param name="filter">Filtre parametreleri</param>
        /// <returns>Chart verileri</returns>
        Task<DashboardChartData> GetChartDataAsync(DashboardFilter filter);

        /// <summary>
        /// Autocomplete önerileri getirir
        /// </summary>
        /// <param name="request">Autocomplete isteği</param>
        /// <returns>Öneri listesi</returns>
        Task<List<AutocompleteItem>> GetAutocompleteAsync(AutocompleteRequest request);

        /// <summary>
        /// Dropdown verilerini getirir
        /// </summary>
        /// <param name="field">Alan adı</param>
        /// <param name="index">Index adı</param>
        /// <returns>Dropdown öğeleri</returns>
        Task<List<DropdownItem>> GetDropdownDataAsync(string field, string index);

        /// <summary>
        /// Elasticsearch bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı durumu</returns>
        Task<bool> TestConnectionAsync();
    }
}