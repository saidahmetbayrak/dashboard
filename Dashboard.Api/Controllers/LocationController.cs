using Microsoft.AspNetCore.Mvc;
using Dashboard.Api.Services;
using Dashboard.Models.Location;
using Dashboard.Models.Response;

namespace Dashboard.Api.Controllers
{
    /// <summary>
    /// Lokasyon API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ILocationService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm lokasyon mapping verilerini getirir
        /// </summary>
        /// <returns>Lokasyon mappings</returns>
        [HttpGet("mappings")]
        public async Task<ActionResult<ApiResponse<LocationMappings>>> GetLocationMappings()
        {
            try
            {
                _logger.LogInformation("Getting location mappings");

                var mappings = await _locationService.GetLocationMappingsAsync();

                return Ok(ApiResponse<LocationMappings>.SuccessResult(mappings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location mappings");
                return StatusCode(500, ApiResponse<LocationMappings>.ErrorResult("Lokasyon verileri alınırken hata oluştu"));
            }
        }

        /// <summary>
        /// Tüm illeri getirir
        /// </summary>
        /// <returns>İl listesi</returns>
        [HttpGet("provinces")]
        public async Task<ActionResult<ApiResponse<List<ProvinceItem>>>> GetProvinces()
        {
            try
            {
                _logger.LogInformation("Getting all provinces");

                var provinces = await _locationService.GetProvincesAsync();

                return Ok(ApiResponse<List<ProvinceItem>>.SuccessResult(provinces));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provinces");
                return StatusCode(500, ApiResponse<List<ProvinceItem>>.ErrorResult("İl verileri alınırken hata oluştu"));
            }
        }

        /// <summary>
        /// Belirtilen ile ait ilçeleri getirir
        /// </summary>
        /// <param name="provinceCode">İl kodu</param>
        /// <returns>İlçe listesi</returns>
        [HttpGet("districts/{provinceCode}")]
        public async Task<ActionResult<ApiResponse<List<DistrictItem>>>> GetDistricts(string provinceCode)
        {
            try
            {
                _logger.LogInformation("Getting districts for province: {ProvinceCode}", provinceCode);

                if (string.IsNullOrEmpty(provinceCode))
                {
                    return BadRequest(ApiResponse<List<DistrictItem>>.ErrorResult("İl kodu boş olamaz"));
                }

                var districts = await _locationService.GetDistrictsAsync(provinceCode);

                return Ok(ApiResponse<List<DistrictItem>>.SuccessResult(districts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting districts for province: {ProvinceCode}", provinceCode);
                return StatusCode(500, ApiResponse<List<DistrictItem>>.ErrorResult("İlçe verileri alınırken hata oluştu"));
            }
        }

        /// <summary>
        /// İl kodundan il adını getirir
        /// </summary>
        /// <param name="provinceCode">İl kodu</param>
        /// <returns>İl adı</returns>
        [HttpGet("province-name/{provinceCode}")]
        public async Task<ActionResult<ApiResponse<string>>> GetProvinceName(string provinceCode)
        {
            try
            {
                _logger.LogInformation("Getting province name for code: {ProvinceCode}", provinceCode);

                if (string.IsNullOrEmpty(provinceCode))
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("İl kodu boş olamaz"));
                }

                var provinceName = await _locationService.GetProvinceNameAsync(provinceCode);

                return Ok(ApiResponse<string>.SuccessResult(provinceName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting province name for code: {ProvinceCode}", provinceCode);
                return StatusCode(500, ApiResponse<string>.ErrorResult("İl adı alınırken hata oluştu"));
            }
        }

        /// <summary>
        /// İlçe kodundan ilçe adını getirir
        /// </summary>
        /// <param name="provinceCode">İl kodu</param>
        /// <param name="districtCode">İlçe kodu</param>
        /// <returns>İlçe adı</returns>
        [HttpGet("district-name/{provinceCode}/{districtCode}")]
        public async Task<ActionResult<ApiResponse<string>>> GetDistrictName(string provinceCode, string districtCode)
        {
            try
            {
                _logger.LogInformation("Getting district name for province: {ProvinceCode}, district: {DistrictCode}",
                    provinceCode, districtCode);

                if (string.IsNullOrEmpty(provinceCode) || string.IsNullOrEmpty(districtCode))
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("İl ve ilçe kodları boş olamaz"));
                }

                var districtName = await _locationService.GetDistrictNameAsync(provinceCode, districtCode);

                return Ok(ApiResponse<string>.SuccessResult(districtName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting district name for province: {ProvinceCode}, district: {DistrictCode}",
                    provinceCode, districtCode);
                return StatusCode(500, ApiResponse<string>.ErrorResult("İlçe adı alınırken hata oluştu"));
            }
        }
    }
}