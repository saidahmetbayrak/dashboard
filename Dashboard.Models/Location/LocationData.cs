namespace Dashboard.Models.Location
{
    /// <summary>
    /// İl-İlçe veri modeli (sabitler.json için)
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// İl-İlçe listesi
        /// </summary>
        public List<LocationItem> IlIlce { get; set; } = new();
    }

    /// <summary>
    /// Lokasyon öğesi
    /// </summary>
    public class LocationItem
    {
        /// <summary>
        /// İlçe kodu
        /// </summary>
        public int IlceKodu { get; set; }

        /// <summary>
        /// İlçe adı
        /// </summary>
        public string IlceAdi { get; set; } = string.Empty;

        /// <summary>
        /// İl kodu
        /// </summary>
        public int IlKodu { get; set; }

        /// <summary>
        /// İl adı
        /// </summary>
        public string IlAdi { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lokasyon mapping verileri
    /// </summary>
    public class LocationMappings
    {
        /// <summary>
        /// İl kod-ad eşleştirmeleri
        /// </summary>
        public Dictionary<string, string> Provinces { get; set; } = new();

        /// <summary>
        /// İlçe kod-ad eşleştirmeleri (il koduna göre gruplu)
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Districts { get; set; } = new();
    }

    /// <summary>
    /// İl dropdown öğesi
    /// </summary>
    public class ProvinceItem
    {
        /// <summary>
        /// İl kodu
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// İl adı
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// İlçe dropdown öğesi
    /// </summary>
    public class DistrictItem
    {
        /// <summary>
        /// İlçe kodu
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// İlçe adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Bağlı olduğu il kodu
        /// </summary>
        public string ProvinceCode { get; set; } = string.Empty;
    }
}