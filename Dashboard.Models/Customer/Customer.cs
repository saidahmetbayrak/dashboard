using System.Text.Json.Serialization;

namespace Dashboard.Models.Customer
{
    /// <summary>
    /// Müşteri model sınıfı - Elasticsearch müşteri/profil verilerini temsil eder
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Elasticsearch document ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Firma adı
        /// </summary>
        [JsonPropertyName("firmaAdi")]
        public string FirmaAdi { get; set; } = string.Empty;

        /// <summary>
        /// Müşteri numarası
        /// </summary>
        [JsonPropertyName("MusteriNo")]
        public string MusteriNo { get; set; } = string.Empty;

        /// <summary>
        /// E-posta adresi
        /// </summary>
        [JsonPropertyName("Mail")]
        public string Mail { get; set; } = string.Empty;

        /// <summary>
        /// Telefon numarası
        /// </summary>
        [JsonPropertyName("Telefon")]
        public string Telefon { get; set; } = string.Empty;

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

        /// <summary>
        /// Bölge kodu
        /// </summary>
        [JsonPropertyName("BolgeKodu")]
        public string BolgeKodu { get; set; } = string.Empty;

        /// <summary>
        /// Plasiyer adı
        /// </summary>
        [JsonPropertyName("PlasiyerAdi")]
        public string PlasiyerAdi { get; set; } = string.Empty;

        /// <summary>
        /// Son giriş tarihi
        /// </summary>
        [JsonPropertyName("sonGirisTarihi")]
        public DateTime? SonGirisTarihi { get; set; }

        /// <summary>
        /// Aktif durumu
        /// </summary>
        [JsonPropertyName("Aktif")]
        public bool Aktif { get; set; }

        /// <summary>
        /// Kullanıcı tipi
        /// </summary>
        [JsonPropertyName("KullaniciTipi")]
        public string KullaniciTipi { get; set; } = string.Empty;

        /// <summary>
        /// Adres bilgisi
        /// </summary>
        [JsonPropertyName("adres")]
        public string Adres { get; set; } = string.Empty;

        /// <summary>
        /// Konum kodu
        /// </summary>
        [JsonPropertyName("konumKodu")]
        public string KonumKodu { get; set; } = string.Empty;

        /// <summary>
        /// DBS limit
        /// </summary>
        [JsonPropertyName("DbsLimit")]
        public decimal DbsLimit { get; set; }

        /// <summary>
        /// Vadeli DBS borcu
        /// </summary>
        [JsonPropertyName("VadeliDbsBorc")]
        public decimal VadeliDbsBorc { get; set; }

        /// <summary>
        /// Kalan DBS
        /// </summary>
        [JsonPropertyName("KalanDbs")]
        public decimal KalanDbs { get; set; }

        /// <summary>
        /// Limit kullanım oranı (%)
        /// </summary>
        [JsonPropertyName("LimitKullanimOrani")]
        public decimal LimitKullanimOrani { get; set; }

        /// <summary>
        /// Cari hesap bakiyesi
        /// </summary>
        [JsonPropertyName("CariHesapBakiyesi")]
        public decimal CariHesapBakiyesi { get; set; }

        /// <summary>
        /// Kredi limiti
        /// </summary>
        [JsonPropertyName("KrediLimiti")]
        public decimal KrediLimiti { get; set; }

        /// <summary>
        /// İpotek limiti
        /// </summary>
        [JsonPropertyName("IpotekLimiti")]
        public decimal IpotekLimiti { get; set; }

        /// <summary>
        /// Sigorta limiti
        /// </summary>
        [JsonPropertyName("SigortaLimiti")]
        public decimal SigortaLimiti { get; set; }

        /// <summary>
        /// Çek riski
        /// </summary>
        [JsonPropertyName("CekRiski")]
        public decimal CekRiski { get; set; }

        /// <summary>
        /// Açık sipariş tutarı
        /// </summary>
        [JsonPropertyName("AcikSiparisTutar")]
        public decimal AcikSiparisTutar { get; set; }

        /// <summary>
        /// Açık fatura tutarı
        /// </summary>
        [JsonPropertyName("AcikFaturaTutar")]
        public decimal AcikFaturaTutar { get; set; }

        /// <summary>
        /// Açık dağıtım tutarı
        /// </summary>
        [JsonPropertyName("AcikDagitimTutar")]
        public decimal AcikDagitimTutar { get; set; }

        /// <summary>
        /// Açık irsaliye
        /// </summary>
        [JsonPropertyName("AcikIrsaliye")]
        public decimal AcikIrsaliye { get; set; }

        /// <summary>
        /// Yaşlandırma (0-30 gün)
        /// </summary>
        [JsonPropertyName("Yaslandirma0")]
        public decimal Yaslandirma0 { get; set; }

        /// <summary>
        /// Yaşlandırma (31-60 gün)
        /// </summary>
        [JsonPropertyName("Yaslandirma30")]
        public decimal Yaslandirma30 { get; set; }

        /// <summary>
        /// Yaşlandırma (61-90 gün)
        /// </summary>
        [JsonPropertyName("Yaslandirma60")]
        public decimal Yaslandirma60 { get; set; }

        /// <summary>
        /// Yaşlandırma (91-120 gün)
        /// </summary>
        [JsonPropertyName("Yaslandirma90")]
        public decimal Yaslandirma90 { get; set; }

        /// <summary>
        /// Yaşlandırma (120+ gün)
        /// </summary>
        [JsonPropertyName("Yaslandirma120")]
        public decimal Yaslandirma120 { get; set; }

        /// <summary>
        /// Kalan limit
        /// </summary>
        [JsonPropertyName("KalanLimit")]
        public decimal KalanLimit { get; set; }

        /// <summary>
        /// Vadesi geçen bakiye
        /// </summary>
        [JsonPropertyName("VadesiGecen")]
        public decimal VadesiGecen { get; set; }

        /// <summary>
        /// Kullanıcı adı (kod)
        /// </summary>
        [JsonPropertyName("kullaniciAdi")]
        public string KullaniciAdi { get; set; } = string.Empty;
    }
}