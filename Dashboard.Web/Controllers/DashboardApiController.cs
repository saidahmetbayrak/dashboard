using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;
using Dashboard.Web.Models;
using Dashboard.Models.Filter;

namespace Dashboard.Web.Controllers
{
    /// <summary>
    /// Dashboard AJAX API controller'ı
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly ILogger<DashboardApiController> _logger;

        public DashboardApiController(IApiService apiService, ILogger<DashboardApiController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// Chart verilerini getirir
        /// </summary>
        /// <param name="request">Chart veri isteği</param>
        /// <returns>Chart verileri</returns>
        [HttpPost("charts")]
        public async Task<ActionResult<AjaxDataResponse<object>>> GetChartData([FromBody] ChartDataRequest request)
        {
            try
            {
                _logger.LogInformation("=== REAL CHART DATA REQUEST ===");
                _logger.LogInformation("Getting chart data with request: {@Request}", request);

                // Request'i DashboardFilter'a dönüştür - safe şekilde
                var filter = MapChartRequestToFilterSafe(request);
                _logger.LogInformation("Mapped filter: {@Filter}", filter);

                var result = await _apiService.GetChartDataAsync(filter);
                _logger.LogInformation("API Service result - Success: {Success}, Data null: {IsNull}",
                    result.Success, result.Data == null);

                if (result.Success && result.Data != null)
                {
                    return Ok(AjaxDataResponse<object>.Success(result.Data, "Chart verileri başarıyla alındı"));
                }
                else
                {
                    _logger.LogWarning("Chart data request failed: {Message}", result.Message);

                    // Hata durumunda test verisi döndür (geçici)
                    var fallbackData = GetFallbackChartData();
                    return Ok(AjaxDataResponse<object>.Success(fallbackData, "Test verileri döndürüldü (API hatası)"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data");

                // Exception durumunda da test verisi döndür
                var fallbackData = GetFallbackChartData();
                return Ok(AjaxDataResponse<object>.Success(fallbackData, "Test verileri döndürüldü (Exception)"));
            }
        }

        /// <summary>
        /// Sepet verilerini getirir
        /// </summary>
        /// <param name="request">Tablo veri isteği</param>
        /// <returns>Sepet verileri</returns>
        [HttpPost("cart-items")]
        public async Task<ActionResult<AjaxDataResponse<object>>> GetCartItems([FromBody] TableDataRequest request)
        {
            try
            {
                _logger.LogInformation("Getting cart items with request: {@Request}", request);

                var filter = MapTableRequestToFilter(request);
                var pagination = MapToPagination(request);

                var result = await _apiService.GetCartItemsAsync(filter, pagination);

                if (result.Success && result.Data != null)
                {
                    return Ok(AjaxDataResponse<object>.Success(result.Data, "Sepet verileri başarıyla alındı"));
                }
                else
                {
                    _logger.LogWarning("Cart items request failed: {Message}", result.Message);
                    return Ok(AjaxDataResponse<object>.Error(result.Message ?? "Sepet verileri alınamadı"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items");
                return StatusCode(500, AjaxDataResponse<object>.Error("Sepet verileri alınırken hata oluştu"));
            }
        }

        /// <summary>
        /// Müşteri verilerini getirir
        /// </summary>
        /// <param name="request">Tablo veri isteği</param>
        /// <returns>Müşteri verileri</returns>
        [HttpPost("customers")]
        public async Task<ActionResult<AjaxDataResponse<object>>> GetCustomers([FromBody] TableDataRequest request)
        {
            try
            {
                _logger.LogInformation("Getting customers with request: {@Request}", request);

                var filter = MapTableRequestToFilter(request);
                var pagination = MapToPagination(request);

                var result = await _apiService.GetCustomersAsync(filter, pagination);

                if (result.Success && result.Data != null)
                {
                    return Ok(AjaxDataResponse<object>.Success(result.Data, "Müşteri verileri başarıyla alındı"));
                }
                else
                {
                    _logger.LogWarning("Customers request failed: {Message}", result.Message);
                    return Ok(AjaxDataResponse<object>.Error(result.Message ?? "Müşteri verileri alınamadı"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, AjaxDataResponse<object>.Error("Müşteri verileri alınırken hata oluştu"));
            }
        }

        /// <summary>
        /// Autocomplete önerileri getirir
        /// </summary>
        /// <param name="request">Autocomplete isteği</param>
        /// <returns>Öneri listesi</returns>
        [HttpPost("autocomplete")]
        public async Task<ActionResult<AjaxDataResponse<object>>> GetAutocomplete([FromBody] AutocompleteWebRequest request)
        {
            try
            {
                _logger.LogInformation("Getting autocomplete for field: {Field}, query: {Query}", request.Field, request.Query);

                var autocompleteRequest = new AutocompleteRequest
                {
                    Query = request.Query,
                    Field = request.Field,
                    Index = request.Index,
                    Size = request.Size
                };

                var result = await _apiService.GetAutocompleteAsync(autocompleteRequest);

                if (result.Success && result.Data != null)
                {
                    return Ok(AjaxDataResponse<object>.Success(result.Data, "Autocomplete verileri başarıyla alındı"));
                }
                else
                {
                    _logger.LogWarning("Autocomplete request failed: {Message}", result.Message);
                    return Ok(AjaxDataResponse<object>.Success(new List<object>(), "Öneri bulunamadı"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting autocomplete");
                return Ok(AjaxDataResponse<object>.Success(new List<object>(), "Autocomplete hatası"));
            }
        }

        /// <summary>
        /// İlçe listesi getirir
        /// </summary>
        /// <param name="provinceCode">İl kodu</param>
        /// <returns>İlçe listesi</returns>
        [HttpGet("districts/{provinceCode}")]
        public async Task<ActionResult<AjaxDataResponse<object>>> GetDistricts(string provinceCode)
        {
            try
            {
                _logger.LogInformation("Getting districts for province: {ProvinceCode}", provinceCode);

                if (string.IsNullOrEmpty(provinceCode))
                {
                    return BadRequest(AjaxDataResponse<object>.Error("İl kodu boş olamaz"));
                }

                var result = await _apiService.GetDistrictsAsync(provinceCode);

                if (result.Success && result.Data != null)
                {
                    return Ok(AjaxDataResponse<object>.Success(result.Data, "İlçe verileri başarıyla alındı"));
                }
                else
                {
                    _logger.LogWarning("Districts request failed: {Message}", result.Message);
                    return Ok(AjaxDataResponse<object>.Success(new List<object>(), "İlçe bulunamadı"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting districts");
                return Ok(AjaxDataResponse<object>.Success(new List<object>(), "İlçe verileri alınırken hata oluştu"));
            }
        }

        /// <summary>
        /// Dropdown verilerini getirir
        /// </summary>
        /// <param name="field">Alan adı</param>
        /// <param name="index">Index adı</param>
        /// <returns>Dropdown verileri</returns>
        [HttpGet("dropdown")]
        public async Task<ActionResult<AjaxDataResponse<object>>> GetDropdownData([FromQuery] string field, [FromQuery] string index)
        {
            try
            {
                _logger.LogInformation("Getting dropdown data for field: {Field}, index: {Index}", field, index);

                if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(index))
                {
                    return BadRequest(AjaxDataResponse<object>.Error("Alan adı ve index gereklidir"));
                }

                var result = await _apiService.GetDropdownDataAsync(field, index);

                if (result.Success && result.Data != null)
                {
                    return Ok(AjaxDataResponse<object>.Success(result.Data, "Dropdown verileri başarıyla alındı"));
                }
                else
                {
                    _logger.LogWarning("Dropdown request failed: {Message}", result.Message);
                    return Ok(AjaxDataResponse<object>.Success(new List<object>(), "Dropdown verisi bulunamadı"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dropdown data");
                return Ok(AjaxDataResponse<object>.Success(new List<object>(), "Dropdown verileri alınırken hata oluştu"));
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Table request'i DashboardFilter'a dönüştürür
        /// </summary>
        private DashboardFilter MapTableRequestToFilter(TableDataRequest? request)
        {
            if (request == null) return new DashboardFilter();

            var filter = new DashboardFilter
            {
                MusteriNo = string.IsNullOrWhiteSpace(request.MusteriNo) ? null : request.MusteriNo.Trim(),
                KullaniciKodu = string.IsNullOrWhiteSpace(request.KullaniciKodu) ? null : request.KullaniciKodu.Trim(),
                MalzemeNo = string.IsNullOrWhiteSpace(request.MalzemeNo) ? null : request.MalzemeNo.Trim(),
                Il = string.IsNullOrWhiteSpace(request.Il) ? null : request.Il.Trim(),
                Ilce = string.IsNullOrWhiteSpace(request.Ilce) ? null : request.Ilce.Trim(),
                SevkiyatDepoKodu = string.IsNullOrWhiteSpace(request.SevkiyatDepoKodu) ? null : request.SevkiyatDepoKodu.Trim()
            };

            // Tarih aralığı hesapla
            if (request.DateRangeMonths > 0)
            {
                var endDate = DateTime.Now;
                var startDate = endDate.AddMonths(-request.DateRangeMonths);

                filter.DateRange = new DateRangeFilter
                {
                    Start = startDate,
                    End = endDate,
                    Field = "SonIslemTarihi"
                };
            }

            return filter;
        }

        /// <summary>
        /// Web request'i PaginationFilter'a dönüştürür
        /// </summary>
        private PaginationFilter MapToPagination(TableDataRequest? request)
        {
            if (request == null)
            {
                return new PaginationFilter
                {
                    Page = 1,
                    Size = 50,
                    SortDirection = "desc"
                };
            }

            return new PaginationFilter
            {
                Page = Math.Max(1, request.Page),
                Size = Math.Min(Math.Max(1, request.Size), 100), // Max 100 kayıt
                SortField = string.IsNullOrWhiteSpace(request.SortField) ? null : request.SortField.Trim(),
                SortDirection = string.IsNullOrWhiteSpace(request.SortDirection) ? "desc" : request.SortDirection.ToLower(),
                SearchAfter = request.SearchAfter
            };
        }

        /// <summary>
        /// Safe filter mapping - crash'i önler
        /// </summary>
        private DashboardFilter MapChartRequestToFilterSafe(ChartDataRequest? request)
        {
            if (request == null) return new DashboardFilter();

            try
            {
                var filter = new DashboardFilter
                {
                    MusteriNo = CleanString(request.MusteriNo),
                    KullaniciKodu = CleanString(request.KullaniciKodu),
                    MalzemeNo = CleanString(request.MalzemeNo),
                    Il = CleanString(request.Il),
                    Ilce = CleanString(request.Ilce),
                    SevkiyatDepoKodu = CleanString(request.SevkiyatDepoKodu)
                };

                // Tarih aralığı hesapla
                if (request.DateRangeMonths > 0)
                {
                    var endDate = DateTime.Now;
                    var startDate = endDate.AddMonths(-request.DateRangeMonths);

                    filter.DateRange = new DateRangeFilter
                    {
                        Start = startDate,
                        End = endDate,
                        Field = "SonIslemTarihi"
                    };
                }

                return filter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping chart request to filter");
                return new DashboardFilter();
            }
        }

        /// <summary>
        /// String temizleme - güvenli
        /// </summary>
        private string? CleanString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "string")
                return null;
            return value.Trim();
        }

        /// <summary>
        /// Fallback chart data - API hata verirse
        /// </summary>
        private object GetFallbackChartData()
        {
            return new
            {
                DailyCartTrend = new[]
                {
                    new { Label = "Veri Yok", Value = 0, Key = DateTime.Now.ToString("yyyy-MM-dd"), Date = DateTime.Now }
                },
                MonthlyTrend = new[]
                {
                    new { Label = "Veri Yok", Value = 0, Key = DateTime.Now.ToString("yyyy-MM"), Date = DateTime.Now }
                },
                TopProducts = new[]
                {
                    new { Label = "Veri Yok", Value = 0, Key = "NODATA" }
                },
                TopCustomers = new[]
                {
                    new { Label = "Veri Yok", Value = 0, Key = "NODATA" }
                },
                DepotDistribution = new[]
                {
                    new { Label = "Veri Yok", Value = 0, Key = "NODATA" }
                },
                ProvincePerformance = new[]
                {
                    new { Label = "Veri Yok", Value = 0, Key = "NODATA" }
                }
            };
        }

        #endregion Private Helper Methods
    }
}