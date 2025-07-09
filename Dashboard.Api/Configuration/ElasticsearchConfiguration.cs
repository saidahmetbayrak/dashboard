using Elastic.Clients.Elasticsearch;

namespace Dashboard.Api.Configuration
{
    /// <summary>
    /// Elasticsearch yapılandırma ayarları
    /// </summary>
    public class ElasticsearchSettings
    {
        /// <summary>
        /// Elasticsearch URL
        /// </summary>
        public string Url { get; set; } = "http://192.168.200.54:9200";

        /// <summary>
        /// Sepet index adı
        /// </summary>
        public string CartIndex { get; set; } = "context-sepet";

        /// <summary>
        /// Profil index adı
        /// </summary>
        public string ProfileIndex { get; set; } = "context-profile";

        /// <summary>
        /// Bağlantı timeout süresi (saniye)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maksimum retry sayısı
        /// </summary>
        public int MaxRetries { get; set; } = 3;
    }

    /// <summary>
    /// Elasticsearch client yapılandırması
    /// </summary>
    public static class ElasticsearchExtensions
    {
        /// <summary>
        /// Elasticsearch client'ı DI container'a ekler
        /// </summary>
        /// <param name="services">Servis koleksiyonu</param>
        /// <param name="configuration">Yapılandırma</param>
        /// <returns>Servis koleksiyonu</returns>
        public static IServiceCollection AddElasticsearch(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Elasticsearch ayarlarını yükle
            var settings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>()
                          ?? new ElasticsearchSettings();

            services.Configure<ElasticsearchSettings>(
                configuration.GetSection("Elasticsearch"));

            // Elasticsearch client'ı yapılandır
            services.AddSingleton<ElasticsearchClient>(provider =>
            {
                var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Url))
                    .RequestTimeout(TimeSpan.FromSeconds(settings.TimeoutSeconds))
                    .MaximumRetries(settings.MaxRetries)
                    .ThrowExceptions(false)
                    .EnableDebugMode() // Debug modunu açın
                    .PrettyJson(); // JSON'ı daha okunabilir hale getirin

                // Development ortamında SSL sertifika kontrolünü kapat
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    clientSettings = clientSettings.ServerCertificateValidationCallback(
                        (sender, certificate, chain, errors) => true);
                }

                return new ElasticsearchClient(clientSettings);
            });

            return services;
        }
    }
}