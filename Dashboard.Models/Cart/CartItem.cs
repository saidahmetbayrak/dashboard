using System.Text.Json.Serialization;

namespace Dashboard.Models.Cart
{
    /// <summary>
    /// Sepet öğesi model sınıfı - Elasticsearch sepet verilerini temsil eder
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// Elasticsearch document ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Müşteri numarası
        /// </summary>
        [JsonPropertyName("MusteriNo")]
        public string MusteriNo { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcı kodu/adı
        /// </summary>
        [JsonPropertyName("KullaniciKodu")]
        public string KullaniciKodu { get; set; } = string.Empty;

        /// <summary>
        /// Malzeme numarası
        /// </summary>
        [JsonPropertyName("MalzemeNo")]
        public string MalzemeNo { get; set; } = string.Empty;

        /// <summary>
        /// Ürün adeti
        /// </summary>
        [JsonPropertyName("Adet")]
        public int Adet { get; set; }

        /// <summary>
        /// Sipariş numarası
        /// </summary>
        [JsonPropertyName("SiparisNo")]
        public string SiparisNo { get; set; } = string.Empty;

        /// <summary>
        /// Sevkiyat depo kodu
        /// </summary>
        [JsonPropertyName("SevkiyatDepoKodu")]
        public string SevkiyatDepoKodu { get; set; } = string.Empty;

        /// <summary>
        /// Son işlem tarihi
        /// </summary>
        [JsonPropertyName("SonIslemTarihi")]
        public DateTime SonIslemTarihi { get; set; }

        /// <summary>
        /// İl kodu
        /// </summary>
        [JsonPropertyName("Il")]
        public string Il { get; set; } = string.Empty;

        /// <summary>
        /// İlçe kodu
        /// </summary>
        [JsonPropertyName("Ilce")]
        public string Ilce { get; set; } = string.Empty;
    }
}