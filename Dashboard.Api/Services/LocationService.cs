using Dashboard.Models.Location;
using System.Text.Json;

namespace Dashboard.Api.Services
{
    /// <summary>
    /// Lokasyon servisi implementasyonu
    /// </summary>
    public class LocationService : ILocationService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<LocationService> _logger;
        private LocationMappings? _cachedMappings;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public LocationService(IWebHostEnvironment webHostEnvironment, ILogger<LocationService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Lokasyon mapping verilerini getirir
        /// </summary>
        public async Task<LocationMappings> GetLocationMappingsAsync()
        {
            if (_cachedMappings != null)
                return _cachedMappings;

            await _semaphore.WaitAsync();
            try
            {
                if (_cachedMappings != null)
                    return _cachedMappings;

                await LoadLocationDataAsync();
                return _cachedMappings ?? new LocationMappings();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Tüm illeri getirir
        /// </summary>
        public async Task<List<ProvinceItem>> GetProvincesAsync()
        {
            var mappings = await GetLocationMappingsAsync();

            return mappings.Provinces
                .Select(p => new ProvinceItem { Code = p.Key, Name = p.Value })
                .OrderBy(p => p.Name)
                .ToList();
        }

        /// <summary>
        /// Belirtilen ile ait ilçeleri getirir
        /// </summary>
        public async Task<List<DistrictItem>> GetDistrictsAsync(string provinceCode)
        {
            var mappings = await GetLocationMappingsAsync();

            if (!mappings.Districts.TryGetValue(provinceCode, out var districts))
                return new List<DistrictItem>();

            return districts
                .Select(d => new DistrictItem
                {
                    Code = d.Key,
                    Name = d.Value,
                    ProvinceCode = provinceCode
                })
                .OrderBy(d => d.Name)
                .ToList();
        }

        /// <summary>
        /// İl kodundan il adını getirir
        /// </summary>
        public async Task<string> GetProvinceNameAsync(string provinceCode)
        {
            var mappings = await GetLocationMappingsAsync();

            if (string.IsNullOrEmpty(provinceCode))
                return provinceCode;

            // Kod 2 haneli olmalı
            var formattedCode = provinceCode.PadLeft(2, '0');
            return mappings.Provinces.TryGetValue(formattedCode, out var name) ? name : provinceCode;
        }

        /// <summary>
        /// İlçe kodundan ilçe adını getirir
        /// </summary>
        public async Task<string> GetDistrictNameAsync(string provinceCode, string districtCode)
        {
            var mappings = await GetLocationMappingsAsync();

            if (string.IsNullOrEmpty(provinceCode) || string.IsNullOrEmpty(districtCode))
                return districtCode;

            var formattedProvinceCode = provinceCode.PadLeft(2, '0');

            if (mappings.Districts.TryGetValue(formattedProvinceCode, out var districts) &&
                districts.TryGetValue(districtCode, out var name))
            {
                return name;
            }

            return districtCode;
        }

        /// <summary>
        /// Lokasyon verilerini JSON dosyasından yükler
        /// </summary>
        private async Task LoadLocationDataAsync()
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Data", "sabitler.json");

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("sabitler.json file not found at {FilePath}", filePath);
                    _cachedMappings = new LocationMappings();
                    return;
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                var locationData = JsonSerializer.Deserialize<LocationData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (locationData?.IlIlce == null)
                {
                    _logger.LogWarning("Invalid location data format in sabitler.json");
                    _cachedMappings = new LocationMappings();
                    return;
                }

                var mappings = new LocationMappings();

                foreach (var item in locationData.IlIlce)
                {
                    var provinceCode = item.IlKodu.ToString().PadLeft(2, '0');
                    var provinceName = item.IlAdi?.Trim() ?? "";
                    var districtCode = item.IlceKodu.ToString();
                    var districtName = item.IlceAdi?.Trim() ?? "";

                    // İl mappings
                    if (!string.IsNullOrEmpty(provinceName))
                    {
                        mappings.Provinces[provinceCode] = provinceName;
                    }

                    // İlçe mappings
                    if (!string.IsNullOrEmpty(districtName))
                    {
                        if (!mappings.Districts.ContainsKey(provinceCode))
                        {
                            mappings.Districts[provinceCode] = new Dictionary<string, string>();
                        }
                        mappings.Districts[provinceCode][districtCode] = districtName;
                    }
                }

                _cachedMappings = mappings;
                _logger.LogInformation("Location data loaded successfully. Provinces: {ProvinceCount}, Districts: {DistrictCount}",
                    mappings.Provinces.Count,
                    mappings.Districts.Sum(d => d.Value.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading location data");
                _cachedMappings = new LocationMappings();
            }
        }
    }
}