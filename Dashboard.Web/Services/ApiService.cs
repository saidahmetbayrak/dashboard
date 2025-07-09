using Dashboard.Models.Cart;
using Dashboard.Models.Customer;
using Dashboard.Models.Filter;
using Dashboard.Models.Location;
using Dashboard.Models.Response;
using Dashboard.Web.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dashboard.Web.Services
{
    /// <summary>
    /// API servis implementasyonu
    /// </summary>
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _logger.LogInformation("ApiService initialized with BaseAddress: {BaseAddress}",
                _httpClient.BaseAddress?.ToString() ?? "NULL");

            // ✅ Elasticsearch uyumlu JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null, // Keep original property names
                WriteIndented = false, // ✅ false yapın
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                // ✅ Elasticsearch uyumluluk için ekleyin:
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }

        /// <summary>
        /// Chart verilerini getirir
        /// </summary>
        public async Task<WebApiResponse<DashboardChartData>> GetChartDataAsync(DashboardFilter filter)
        {
            try
            {
                _logger.LogInformation("=== API SERVICE REQUEST ===");
                _logger.LogInformation("Making request to: {BaseUrl}/charts", _httpClient.BaseAddress);
                _logger.LogInformation("Filter: {@Filter}", filter);

                var response = await _httpClient.PostAsJsonAsync("api/dashboard/charts", filter);

                _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("API Response Success: {IsSuccess}", response.IsSuccessStatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Raw API Response Content: {Content}", responseContent);

                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardChartData>>();
                    _logger.LogInformation("API Response parsed - Success: {Success}, Data null: {IsNull}",
                        result?.Success, result?.Data == null);

                    return new WebApiResponse<DashboardChartData>
                    {
                        Success = result?.Success ?? false,
                        Data = result?.Data,
                        Message = result?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Request failed - Status: {Status}, Content: {Content}",
                        response.StatusCode, errorContent);

                    return new WebApiResponse<DashboardChartData>
                    {
                        Success = false,
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== API SERVICE EXCEPTION ===");
                _logger.LogError("Exception Type: {Type}", ex.GetType().Name);
                _logger.LogError("Exception Message: {Message}", ex.Message);
                _logger.LogError("Inner Exception: {InnerException}", ex.InnerException?.Message);

                return new WebApiResponse<DashboardChartData>
                {
                    Success = false,
                    Message = $"API exception: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Sepet verilerini getirir
        /// </summary>
        public async Task<WebApiResponse<PagedResponse<CartItem>>> GetCartItemsAsync(DashboardFilter filter, PaginationFilter pagination)
        {
            try
            {
                _logger.LogInformation("Getting cart items from API");

                var request = new { Filter = filter, Pagination = pagination };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/dashboard/cart-items", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResponse<CartItem>>>(responseContent, _jsonOptions);

                    return new WebApiResponse<PagedResponse<CartItem>>
                    {
                        Success = apiResponse?.Success ?? false,
                        Data = apiResponse?.Data,
                        Message = apiResponse?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API cart items request failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<PagedResponse<CartItem>>
                    {
                        Success = false,
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items from API");
                return new WebApiResponse<PagedResponse<CartItem>>
                {
                    Success = false,
                    Message = "API iletişim hatası",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Müşteri verilerini getirir
        /// </summary>
        public async Task<WebApiResponse<PagedResponse<Customer>>> GetCustomersAsync(DashboardFilter filter, PaginationFilter pagination)
        {
            try
            {
                _logger.LogInformation("Getting customers from API");

                var request = new { Filter = filter, Pagination = pagination };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/dashboard/customers", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResponse<Customer>>>(responseContent, _jsonOptions);

                    return new WebApiResponse<PagedResponse<Customer>>
                    {
                        Success = apiResponse?.Success ?? false,
                        Data = apiResponse?.Data,
                        Message = apiResponse?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API customers request failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<PagedResponse<Customer>>
                    {
                        Success = false,
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers from API");
                return new WebApiResponse<PagedResponse<Customer>>
                {
                    Success = false,
                    Message = "API iletişim hatası",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Autocomplete önerileri getirir
        /// </summary>
        public async Task<WebApiResponse<List<AutocompleteItem>>> GetAutocompleteAsync(AutocompleteRequest request)
        {
            try
            {
                _logger.LogInformation("Getting autocomplete suggestions from API");

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/dashboard/autocomplete", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AutocompleteItem>>>(responseContent, _jsonOptions);

                    return new WebApiResponse<List<AutocompleteItem>>
                    {
                        Success = apiResponse?.Success ?? false,
                        Data = apiResponse?.Data ?? new List<AutocompleteItem>(),
                        Message = apiResponse?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API autocomplete request failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<List<AutocompleteItem>>
                    {
                        Success = false,
                        Data = new List<AutocompleteItem>(),
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting autocomplete from API");
                return new WebApiResponse<List<AutocompleteItem>>
                {
                    Success = false,
                    Data = new List<AutocompleteItem>(),
                    Message = "API iletişim hatası",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Dropdown verilerini getirir
        /// </summary>
        public async Task<WebApiResponse<List<DropdownItem>>> GetDropdownDataAsync(string field, string index)
        {
            try
            {
                _logger.LogInformation("Getting dropdown data from API for field: {Field}", field);

                var response = await _httpClient.GetAsync($"api/dashboard/dropdown?field={Uri.EscapeDataString(field)}&index={Uri.EscapeDataString(index)}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<DropdownItem>>>(responseContent, _jsonOptions);

                    return new WebApiResponse<List<DropdownItem>>
                    {
                        Success = apiResponse?.Success ?? false,
                        Data = apiResponse?.Data ?? new List<DropdownItem>(),
                        Message = apiResponse?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API dropdown request failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<List<DropdownItem>>
                    {
                        Success = false,
                        Data = new List<DropdownItem>(),
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dropdown data from API");
                return new WebApiResponse<List<DropdownItem>>
                {
                    Success = false,
                    Data = new List<DropdownItem>(),
                    Message = "API iletişim hatası",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Lokasyon mapping verilerini getirir
        /// </summary>
        public async Task<WebApiResponse<LocationMappings>> GetLocationMappingsAsync()
        {
            try
            {
                _logger.LogInformation("Getting location mappings from API");

                var response = await _httpClient.GetAsync("api/location/mappings");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<LocationMappings>>(responseContent, _jsonOptions);

                    return new WebApiResponse<LocationMappings>
                    {
                        Success = apiResponse?.Success ?? false,
                        Data = apiResponse?.Data,
                        Message = apiResponse?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API location mappings request failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<LocationMappings>
                    {
                        Success = false,
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location mappings from API");
                return new WebApiResponse<LocationMappings>
                {
                    Success = false,
                    Message = "API iletişim hatası",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Tüm illeri getirir
        /// </summary>
        public async Task<WebApiResponse<List<ProvinceItem>>> GetProvincesAsync()
        {
            try
            {
                _logger.LogInformation("Getting provinces from API");

                var response = await _httpClient.GetAsync("api/location/provinces");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProvinceItem>>>(responseContent, _jsonOptions);

                    return new WebApiResponse<List<ProvinceItem>>
                    {
                        Success = apiResponse?.Success ?? false,
                        Data = apiResponse?.Data ?? new List<ProvinceItem>(),
                        Message = apiResponse?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API provinces request failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<List<ProvinceItem>>
                    {
                        Success = false,
                        Data = new List<ProvinceItem>(),
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provinces from API");
                return new WebApiResponse<List<ProvinceItem>>
                {
                    Success = false,
                    Data = new List<ProvinceItem>(),
                    Message = "API iletişim hatası",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Belirtilen ile ait ilçeleri getirir
        /// </summary>
        public async Task<WebApiResponse<List<DistrictItem>>> GetDistrictsAsync(string provinceCode)
        {
            try
            {
                _logger.LogInformation("Getting districts from API for province: {ProvinceCode}", provinceCode);

                var response = await _httpClient.GetAsync($"api/location/districts/{Uri.EscapeDataString(provinceCode)}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<DistrictItem>>>(responseContent, _jsonOptions);

                    return new WebApiResponse<List<DistrictItem>>
                    {
                        Success = apiResponse?.Success ?? false,
                        Data = apiResponse?.Data ?? new List<DistrictItem>(),
                        Message = apiResponse?.Message,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API districts request failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<List<DistrictItem>>
                    {
                        Success = false,
                        Data = new List<DistrictItem>(),
                        Message = $"API request failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting districts from API");
                return new WebApiResponse<List<DistrictItem>>
                {
                    Success = false,
                    Data = new List<DistrictItem>(),
                    Message = "API iletişim hatası",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// API sağlık kontrolü yapar - Basit boolean döner (Program.cs ile uyumlu)
        /// </summary>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                _logger.LogInformation("Performing API health check to {BaseAddress}", _httpClient.BaseAddress);

                // /health endpoint'ini kullan - bu middleware tarafından sağlanır
                var response = await _httpClient.GetAsync("/health");

                var isHealthy = response.IsSuccessStatusCode;
                _logger.LogInformation("API health check result: {IsHealthy} (Status: {StatusCode})",
                    isHealthy, response.StatusCode);

                if (!isHealthy)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Health check failed. Response: {Response}", content);
                }

                return isHealthy;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during API health check. Is the API running at {BaseAddress}?",
                    _httpClient.BaseAddress);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout during API health check to {BaseAddress}", _httpClient.BaseAddress);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error performing API health check to {BaseAddress}",
                    _httpClient.BaseAddress);
                return false;
            }
        }

        /// <summary>
        /// API sağlık kontrolü yapar - Detaylı response döner (backward compatibility için)
        /// </summary>
        public async Task<WebApiResponse<bool>> HealthCheckDetailedAsync()
        {
            try
            {
                _logger.LogInformation("Performing detailed API health check");

                var response = await _httpClient.GetAsync("/health");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new WebApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "API is healthy",
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    _logger.LogWarning("API health check failed with status: {StatusCode}", response.StatusCode);
                    return new WebApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = $"Health check failed: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing detailed API health check");
                return new WebApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "API bağlantı hatası",
                    StatusCode = 500
                };
            }
        }
    }
}