using Microsoft.AspNetCore.Mvc;
using Dashboard.Api.Services;
using Dashboard.Models.Filter;
using Dashboard.Models.Response;
using Dashboard.Models.Cart;
using Dashboard.Models.Customer;
using Dashboard.Api.Models;

namespace Dashboard.Api.Controllers
{
    /// <summary>
    /// Dashboard API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILocationService _locationService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IElasticsearchService elasticsearchService,
            ILocationService locationService,
            ILogger<DashboardController> logger)
        {
            _elasticsearchService = elasticsearchService;
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard chart verilerini getirir
        /// </summary>
        /// <param name="filter">Filtre parametreleri</param>
        /// <returns>Chart verileri</returns>
        [HttpPost("charts")]
        public async Task<ActionResult<ApiResponse<DashboardChartData>>> GetChartData([FromBody] DashboardFilter filter)
        {
            try
            {
                // ✅ String değerlerini temizle - Extension kullanmadan
                var cleanedFilter = CleanDashboardFilter(filter);

                _logger.LogInformation("=== CHART REQUEST DEBUG ===");
                _logger.LogInformation("Original filter: {@OriginalFilter}", filter);
                _logger.LogInformation("Cleaned filter: {@CleanedFilter}", cleanedFilter);

                var chartData = await _elasticsearchService.GetChartDataAsync(cleanedFilter);

                // İl bazında performans chart'ı için il kodlarını isimlerle değiştir
                var locationMappings = await _locationService.GetLocationMappingsAsync();
                foreach (var item in chartData.ProvincePerformance)
                {
                    if (locationMappings.Provinces.TryGetValue(item.Key, out var provinceName))
                    {
                        item.Label = provinceName;
                    }
                }

                _logger.LogInformation("Chart data retrieved successfully");
                return Ok(ApiResponse<DashboardChartData>.SuccessResult(chartData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data");
                return StatusCode(500, ApiResponse<DashboardChartData>.ErrorResult("Chart verileri alınırken hata oluştu: " + ex.Message));
            }
        }

        /// <summary>
        /// Sepet verilerini getirir
        /// </summary>
        /// <param name="request">Arama ve filtre parametreleri</param>
        /// <returns>Sayfalı sepet verileri</returns>
        [HttpPost("cart-items")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CartItem>>>> GetCartItems([FromBody] CartItemsRequest request)
        {
            try
            {
                // ✅ String değerlerini temizle
                var cleanedFilter = CleanDashboardFilter(request.Filter);

                _logger.LogInformation("Original filter: {@OriginalFilter}", request.Filter);
                _logger.LogInformation("Cleaned filter: {@CleanedFilter}", cleanedFilter);

                var result = await _elasticsearchService.GetCartItemsAsync(cleanedFilter, request.Pagination);

                return Ok(ApiResponse<PagedResponse<CartItem>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items");
                return StatusCode(500, ApiResponse<PagedResponse<CartItem>>.ErrorResult("Sepet verileri alınırken hata oluştu: " + ex.Message));
            }
        }

        /// <summary>
        /// Müşteri verilerini getirir
        /// </summary>
        /// <param name="request">Arama ve filtre parametreleri</param>
        /// <returns>Sayfalı müşteri verileri</returns>
        [HttpPost("customers")]
        public async Task<ActionResult<ApiResponse<PagedResponse<Customer>>>> GetCustomers([FromBody] CustomersRequest request)
        {
            try
            {
                // ✅ String değerlerini temizle
                var cleanedFilter = CleanDashboardFilter(request.Filter);

                _logger.LogInformation("Original filter: {@OriginalFilter}", request.Filter);
                _logger.LogInformation("Cleaned filter: {@CleanedFilter}", cleanedFilter);

                var result = await _elasticsearchService.GetCustomersAsync(cleanedFilter, request.Pagination);

                // İl ve ilçe kodlarını isimlerle değiştir
                var locationMappings = await _locationService.GetLocationMappingsAsync();
                foreach (var customer in result.Data)
                {
                    if (!string.IsNullOrEmpty(customer.Il))
                    {
                        customer.Il = await _locationService.GetProvinceNameAsync(customer.Il);
                    }

                    if (!string.IsNullOrEmpty(customer.Ilce) && !string.IsNullOrEmpty(customer.Il))
                    {
                        var provinceCode = locationMappings.Provinces.FirstOrDefault(p => p.Value == customer.Il).Key;
                        if (!string.IsNullOrEmpty(provinceCode))
                        {
                            customer.Ilce = await _locationService.GetDistrictNameAsync(provinceCode, customer.Ilce);
                        }
                    }
                }

                return Ok(ApiResponse<PagedResponse<Customer>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, ApiResponse<PagedResponse<Customer>>.ErrorResult("Müşteri verileri alınırken hata oluştu: " + ex.Message));
            }
        }

        /// <summary>
        /// Autocomplete önerileri getirir
        /// </summary>
        /// <param name="request">Autocomplete isteği</param>
        /// <returns>Öneri listesi</returns>
        [HttpPost("autocomplete")]
        public async Task<ActionResult<ApiResponse<List<AutocompleteItem>>>> GetAutocomplete([FromBody] AutocompleteRequest request)
        {
            try
            {
                // ✅ Query string'ini temizle
                var cleanedRequest = new AutocompleteRequest
                {
                    Field = request.Field,
                    Query = CleanStringValue(request.Query) ?? "",
                    Index = request.Index,
                    Size = request.Size
                };

                _logger.LogInformation("Original autocomplete: {@Original}, Cleaned: {@Cleaned}", request, cleanedRequest);

                var suggestions = await _elasticsearchService.GetAutocompleteAsync(cleanedRequest);

                return Ok(ApiResponse<List<AutocompleteItem>>.SuccessResult(suggestions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting autocomplete suggestions");
                return StatusCode(500, ApiResponse<List<AutocompleteItem>>.ErrorResult("Autocomplete verileri alınırken hata oluştu: " + ex.Message));
            }
        }

        /// <summary>
        /// Dropdown verilerini getirir
        /// </summary>
        /// <param name="field">Alan adı</param>
        /// <param name="index">Index adı</param>
        /// <returns>Dropdown öğeleri</returns>
        [HttpGet("dropdown")]
        public async Task<ActionResult<ApiResponse<List<DropdownItem>>>> GetDropdownData(
            [FromQuery] string field,
            [FromQuery] string index)
        {
            try
            {
                // ✅ Query parametrelerini temizle
                var cleanedField = CleanStringValue(field);
                var cleanedIndex = CleanStringValue(index);

                if (string.IsNullOrEmpty(cleanedField) || string.IsNullOrEmpty(cleanedIndex))
                {
                    return BadRequest(ApiResponse<List<DropdownItem>>.ErrorResult("Geçerli field ve index değerleri gereklidir"));
                }

                _logger.LogInformation("Getting dropdown data for field: {Field}, index: {Index}", cleanedField, cleanedIndex);

                var items = await _elasticsearchService.GetDropdownDataAsync(cleanedField, cleanedIndex);

                return Ok(ApiResponse<List<DropdownItem>>.SuccessResult(items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dropdown data");
                return StatusCode(500, ApiResponse<List<DropdownItem>>.ErrorResult("Dropdown verileri alınırken hata oluştu: " + ex.Message));
            }
        }

        /// <summary>
        /// Elasticsearch bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı durumu</returns>
        [HttpGet("health")]
        public async Task<ActionResult<ApiResponse<bool>>> HealthCheck()
        {
            try
            {
                var isConnected = await _elasticsearchService.TestConnectionAsync();

                if (isConnected)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Elasticsearch bağlantısı başarılı"));
                }
                else
                {
                    return StatusCode(503, ApiResponse<bool>.ErrorResult("Elasticsearch bağlantısı başarısız"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Elasticsearch connection");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Bağlantı testi sırasında hata oluştu: " + ex.Message));
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// DashboardFilter'ı temizler - "string" değerlerini null'a çevirir
        /// </summary>
        private DashboardFilter CleanDashboardFilter(DashboardFilter filter)
        {
            if (filter == null) return new DashboardFilter();

            return new DashboardFilter
            {
                MusteriNo = CleanStringValue(filter.MusteriNo),
                KullaniciKodu = CleanStringValue(filter.KullaniciKodu),
                MalzemeNo = CleanStringValue(filter.MalzemeNo),
                Il = CleanStringValue(filter.Il),
                Ilce = CleanStringValue(filter.Ilce),
                SevkiyatDepoKodu = CleanStringValue(filter.SevkiyatDepoKodu),
                DateRange = CleanDateRange(filter.DateRange)
            };
        }

        /// <summary>
        /// String değerini temizler
        /// </summary>
        private string? CleanStringValue(string? value)
        {
            if (string.IsNullOrEmpty(value) ||
                value.Trim() == "string" ||
                value.Trim() == "null" ||
                value.Trim() == "undefined")
            {
                return null;
            }
            return value.Trim();
        }

        /// <summary>
        /// DateRange'i temizler
        /// </summary>
        private DateRangeFilter? CleanDateRange(DateRangeFilter? dateRange)
        {
            if (dateRange == null) return null;

            var cleanedField = CleanStringValue(dateRange.Field);
            if (cleanedField == null) return null;

            return new DateRangeFilter
            {
                Start = dateRange.Start,
                End = dateRange.End,
                Field = cleanedField
            };
        }

        #endregion Private Helper Methods
    }
}