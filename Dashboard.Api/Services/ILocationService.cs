using Dashboard.Models.Location;
using System.Text.Json;

namespace Dashboard.Api.Services
{
    /// <summary>
    /// Lokasyon servisi interface'i
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// Lokasyon mapping verilerini getirir
        /// </summary>
        Task<LocationMappings> GetLocationMappingsAsync();

        /// <summary>
        /// Tüm illeri getirir
        /// </summary>
        Task<List<ProvinceItem>> GetProvincesAsync();

        /// <summary>
        /// Belirtilen ile ait ilçeleri getirir
        /// </summary>
        Task<List<DistrictItem>> GetDistrictsAsync(string provinceCode);

        /// <summary>
        /// İl kodundan il adını getirir
        /// </summary>
        Task<string> GetProvinceNameAsync(string provinceCode);

        /// <summary>
        /// İlçe kodundan ilçe adını getirir
        /// </summary>
        Task<string> GetDistrictNameAsync(string provinceCode, string districtCode);
    }
}