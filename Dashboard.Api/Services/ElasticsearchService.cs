using Dashboard.Api.Configuration;
using Dashboard.Models.Cart;
using Dashboard.Models.Customer;
using Dashboard.Models.Filter;
using Dashboard.Models.Response;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace Dashboard.Api.Services
{
    /// <summary>
    /// Elasticsearch servis implementasyonu
    /// </summary>
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ElasticsearchSettings _settings;
        private readonly ILogger<ElasticsearchService> _logger;

        public ElasticsearchService(
            ElasticsearchClient client,
            IOptions<ElasticsearchSettings> settings,
            ILogger<ElasticsearchService> logger)
        {
            _client = client;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sepet verilerini filtreli olarak getirir
        /// </summary>
        public async Task<PagedResponse<CartItem>> GetCartItemsAsync(DashboardFilter filter, PaginationFilter pagination)
        {
            try
            {
                // Query oluştur
                var queries = BuildMustQueries(filter, _settings.CartIndex);

                // Search request oluştur
                var searchRequest = new SearchRequest(_settings.CartIndex)
                {
                    Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                    Size = pagination.Size,
                    Sort = GetSortDescriptors(pagination, "properties.SonIslemTarihi"),
                    TrackTotalHits = new TrackHits(true)
                };

                // Search after kullanılıyorsa ekle
                if (pagination.SearchAfter != null && pagination.SearchAfter.Length > 0)
                {
                    searchRequest.SearchAfter = pagination.SearchAfter
                        .Select(value => FieldValue.String(value?.ToString() ?? ""))
                        .ToArray();
                }

                // Elasticsearch sorgusu çalıştır
                var response = await _client.SearchAsync<object>(searchRequest);

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Elasticsearch cart search failed: {Error}", response.DebugInformation);
                    throw new Exception($"Elasticsearch search failed: {response.DebugInformation}");
                }

                // Sonuçları dönüştür
                var items = new List<CartItem>();
                foreach (var hit in response.Documents)
                {
                    var cartItem = ConvertToCartItem(hit, hit.GetType().GetProperty("Id")?.GetValue(hit)?.ToString() ?? "");
                    if (cartItem != null)
                        items.Add(cartItem);
                }

                // Search after değerini al
                FieldValue[]? searchAfter = null;
                if (response.Hits.Count > 0)
                {
                    var lastHit = response.Hits.Last();
                    searchAfter = lastHit.Sort != null ? lastHit.Sort.ToArray() : null;
                }

                // Total hits bilgisini al
                var totalHits = response.HitsMetadata?.Total;
                long total = 0;
                var relation = "eq";

                if (totalHits != null)
                {
                    if (totalHits.Match(
                        first => { total = first.Value; relation = first.Relation.ToString().ToLower(); return true; },
                        second => { total = second; return true; }))
                    {
                        // Match successful
                    }
                }

                return new PagedResponse<CartItem>
                {
                    Data = items,
                    Total = total,
                    Relation = relation,
                    Page = pagination.Page,
                    Size = pagination.Size,
                    HasNextPage = items.Count == pagination.Size,
                    SearchAfter = searchAfter != null ? searchAfter.Select(fv => fv.ToString()).ToArray() : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items");
                throw;
            }
        }

        /// <summary>
        /// Müşteri verilerini filtreli olarak getirir
        /// </summary>
        public async Task<PagedResponse<Customer>> GetCustomersAsync(DashboardFilter filter, PaginationFilter pagination)
        {
            try
            {
                // Müşteri için sadece belirli filtreleri kullan
                var customerFilter = new DashboardFilter
                {
                    MusteriNo = filter.MusteriNo,
                    KullaniciKodu = filter.KullaniciKodu,
                    Il = filter.Il,
                    Ilce = filter.Ilce
                };

                var queries = BuildMustQueries(customerFilter, _settings.ProfileIndex);

                var searchRequest = new SearchRequest(_settings.ProfileIndex)
                {
                    Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                    Size = pagination.Size,
                    Sort = GetSortDescriptors(pagination, "properties.firmaAdi"),
                    TrackTotalHits = new TrackHits(true)
                };

                if (pagination.SearchAfter != null && pagination.SearchAfter.Length > 0)
                {
                    searchRequest.SearchAfter = pagination.SearchAfter
                        .Select(value => FieldValue.String(value?.ToString() ?? ""))
                        .ToArray();
                }

                var response = await _client.SearchAsync<object>(searchRequest);

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Elasticsearch customer search failed: {Error}", response.DebugInformation);
                    throw new Exception($"Elasticsearch search failed: {response.DebugInformation}");
                }

                var customers = new List<Customer>();
                foreach (var hit in response.Documents)
                {
                    var customer = ConvertToCustomer(hit, hit.GetType().GetProperty("Id")?.GetValue(hit)?.ToString() ?? "");
                    if (customer != null)
                        customers.Add(customer);
                }

                FieldValue[]? searchAfter = null;
                if (response.Hits.Count > 0)
                {
                    var lastHit = response.Hits.Last();
                    searchAfter = lastHit.Sort != null ? lastHit.Sort.ToArray() : null;
                }

                // Total hits bilgisini al
                var totalHits = response.HitsMetadata?.Total;
                long total = 0;
                var relation = "eq";

                if (totalHits != null)
                {
                    if (totalHits.Match(
                        first => { total = first.Value; relation = first.Relation.ToString().ToLower(); return true; },
                        second => { total = second; return true; }))
                    {
                        // Match successful
                    }
                }

                return new PagedResponse<Customer>
                {
                    Data = customers,
                    Total = total,
                    Relation = relation,
                    Page = pagination.Page,
                    Size = pagination.Size,
                    HasNextPage = customers.Count == pagination.Size,
                    SearchAfter = searchAfter != null ? searchAfter.Select(fv => fv.ToString()).ToArray() : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                throw;
            }
        }

        /// <summary>
        /// Dashboard chart verilerini getirir
        /// </summary>
        public async Task<DashboardChartData> GetChartDataAsync(DashboardFilter filter)
        {
            try
            {
                var chartData = new DashboardChartData();

                // Paralel olarak tüm chart verilerini al
                var tasks = new[]
                {
                    GetDailyCartTrendAsync(filter),
                    GetMonthlyTrendAsync(filter),
                    GetTopProductsAsync(filter),
                    GetTopCustomersAsync(filter),
                    GetDepotDistributionAsync(filter),
                    GetProvincePerformanceAsync(filter)
                };

                var results = await Task.WhenAll(tasks);

                chartData.DailyCartTrend = results[0];
                chartData.MonthlyTrend = results[1];
                chartData.TopProducts = results[2];
                chartData.TopCustomers = results[3];
                chartData.DepotDistribution = results[4];
                chartData.ProvincePerformance = results[5];

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data");
                throw;
            }
        }

        /// <summary>
        /// Autocomplete önerileri getirir
        /// </summary>
        public async Task<List<AutocompleteItem>> GetAutocompleteAsync(AutocompleteRequest request)
        {
            try
            {
                var searchRequest = new SearchRequest(request.Index)
                {
                    Size = 0,
                    Aggregations = new Dictionary<string, Aggregation>
                    {
                        ["suggestions"] = new TermsAggregation
                        {
                            Field = request.Field,
                            Size = request.Size,
                            Include = new TermsInclude($".*{request.Query.ToLower()}.*")
                        }
                    }
                };

                var response = await _client.SearchAsync<object>(searchRequest);

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Elasticsearch autocomplete failed: {Error}", response.DebugInformation);
                    return new List<AutocompleteItem>();
                }

                var suggestions = new List<AutocompleteItem>();

                if (response.Aggregations?.TryGetValue("suggestions", out var suggestionsAgg) == true &&
                    suggestionsAgg is StringTermsAggregate termsAgg)
                {
                    foreach (var bucket in termsAgg.Buckets)
                    {
                        suggestions.Add(new AutocompleteItem
                        {
                            Text = bucket.Key.ToString() ?? "",
                            Count = bucket.DocCount
                        });
                    }
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting autocomplete suggestions");
                return new List<AutocompleteItem>();
            }
        }

        /// <summary>
        /// Dropdown verilerini getirir
        /// </summary>
        public async Task<List<DropdownItem>> GetDropdownDataAsync(string field, string index)
        {
            try
            {
                var searchRequest = new SearchRequest(index)
                {
                    Size = 0,
                    Aggregations = new Dictionary<string, Aggregation>
                    {
                        ["values"] = new TermsAggregation
                        {
                            Field = field,
                            Size = 200
                            // Order kısmını şimdilik kaldırıyorum
                        }
                    }
                };

                var response = await _client.SearchAsync<object>(searchRequest);

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Elasticsearch dropdown data failed: {Error}", response.DebugInformation);
                    return new List<DropdownItem>();
                }

                var items = new List<DropdownItem>();

                if (response.Aggregations?.TryGetValue("values", out var valuesAgg) == true &&
                    valuesAgg is StringTermsAggregate termsAgg)
                {
                    foreach (var bucket in termsAgg.Buckets)
                    {
                        var key = bucket.Key.ToString() ?? "";
                        items.Add(new DropdownItem
                        {
                            Value = key,
                            Text = key,
                            Count = bucket.DocCount
                        });
                    }
                }

                // Listeyi alfabetik olarak sırala
                return items.OrderBy(x => x.Text).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dropdown data");
                return new List<DropdownItem>();
            }
        }

        /// <summary>
        /// Elasticsearch bağlantısını test eder
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _client.PingAsync();
                return response.IsValidResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch connection test failed");
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// Filtreden Must query listesi oluşturur
        /// </summary>
        private List<Query> BuildMustQueries(DashboardFilter filter, string indexType)
        {
            var queries = new List<Query>();

            // ✅ DETAYLI DEBUG LOGLARI EKLEYİN
            _logger.LogInformation("=== BuildMustQueries Debug ===");
            _logger.LogInformation("Filter: {@Filter}", filter);
            _logger.LogInformation("IndexType: {IndexType}", indexType);

            // Müşteri no filtresi
            _logger.LogInformation("MusteriNo: '{MusteriNo}', IsNull: {IsNull}, IsString: {IsString}",
                filter.MusteriNo, string.IsNullOrEmpty(filter.MusteriNo), filter.MusteriNo == "string");

            if (!string.IsNullOrEmpty(filter.MusteriNo) && filter.MusteriNo != "string")
            {
                _logger.LogInformation("Adding MusteriNo query");
                queries.Add(new TermQuery
                {
                    Field = "properties.MusteriNo",
                    Value = filter.MusteriNo
                });
            }

            // Kullanıcı kodu filtresi
            _logger.LogInformation("KullaniciKodu: '{KullaniciKodu}', IsNull: {IsNull}, IsString: {IsString}",
                filter.KullaniciKodu, string.IsNullOrEmpty(filter.KullaniciKodu), filter.KullaniciKodu == "string");

            if (!string.IsNullOrEmpty(filter.KullaniciKodu) && filter.KullaniciKodu != "string")
            {
                var field = indexType == _settings.ProfileIndex ? "properties.kullaniciAdi" : "properties.KullaniciKodu";
                _logger.LogInformation("Adding KullaniciKodu query with field: {Field}", field);
                queries.Add(new TermQuery
                {
                    Field = field,
                    Value = filter.KullaniciKodu
                });
            }

            // İl filtresi (hem cart hem de profile için)
            _logger.LogInformation("Il: '{Il}', IsNull: {IsNull}, IsString: {IsString}",
                filter.Il, string.IsNullOrEmpty(filter.Il), filter.Il == "string");

            if (!string.IsNullOrEmpty(filter.Il) && filter.Il != "string")
            {
                _logger.LogInformation("Adding Il query");
                queries.Add(new TermQuery
                {
                    Field = "properties.Il",
                    Value = filter.Il
                });
            }

            // İlçe filtresi (hem cart hem de profile için)
            _logger.LogInformation("Ilce: '{Ilce}', IsNull: {IsNull}, IsString: {IsString}",
                filter.Ilce, string.IsNullOrEmpty(filter.Ilce), filter.Ilce == "string");

            if (!string.IsNullOrEmpty(filter.Ilce) && filter.Ilce != "string")
            {
                _logger.LogInformation("Adding Ilce query");
                queries.Add(new TermQuery
                {
                    Field = "properties.Ilce",
                    Value = filter.Ilce
                });
            }

            // Malzeme no filtresi (sadece sepet için)
            _logger.LogInformation("MalzemeNo: '{MalzemeNo}', IsNull: {IsNull}, IsString: {IsString}, IsCartIndex: {IsCartIndex}",
                filter.MalzemeNo, string.IsNullOrEmpty(filter.MalzemeNo), filter.MalzemeNo == "string", indexType == _settings.CartIndex);

            if (!string.IsNullOrEmpty(filter.MalzemeNo) &&
                filter.MalzemeNo != "string" &&
                indexType == _settings.CartIndex)
            {
                _logger.LogInformation("Adding MalzemeNo query");
                queries.Add(new TermQuery
                {
                    Field = "properties.MalzemeNo",
                    Value = filter.MalzemeNo
                });
            }

            // Sevkiyat depo filtresi
            _logger.LogInformation("SevkiyatDepoKodu: '{SevkiyatDepoKodu}', IsNull: {IsNull}, IsString: {IsString}",
                filter.SevkiyatDepoKodu, string.IsNullOrEmpty(filter.SevkiyatDepoKodu), filter.SevkiyatDepoKodu == "string");

            if (!string.IsNullOrEmpty(filter.SevkiyatDepoKodu) &&
                filter.SevkiyatDepoKodu != "string" &&
                indexType == _settings.CartIndex)
            {
                _logger.LogInformation("Adding SevkiyatDepoKodu query");
                queries.Add(new TermQuery
                {
                    Field = "properties.SevkiyatDepoKodu",
                    Value = filter.SevkiyatDepoKodu
                });
            }

            // DateRange filtresi
            _logger.LogInformation("DateRange: {@DateRange}", filter.DateRange);

            if (filter.DateRange != null && indexType == _settings.CartIndex)
            {
                _logger.LogInformation("DateRange is not null, checking field");
                _logger.LogInformation("DateRange.Field: '{Field}', IsNull: {IsNull}, IsString: {IsString}",
                    filter.DateRange.Field,
                    string.IsNullOrEmpty(filter.DateRange.Field),
                    filter.DateRange.Field == "string");

                if (!string.IsNullOrEmpty(filter.DateRange.Field) && filter.DateRange.Field != "string")
                {
                    _logger.LogInformation("Adding DateRange query with field: properties.{Field}", filter.DateRange.Field);
                    queries.Add(new DateRangeQuery($"properties.{filter.DateRange.Field}")
                    {
                        Gte = DateMath.Anchored(filter.DateRange.Start),
                        Lte = DateMath.Anchored(filter.DateRange.End),
                        Format = "yyyy-MM-dd'T'HH:mm:ss.SSSSSS'Z'"
                    });
                }
                else
                {
                    _logger.LogInformation("DateRange field is null, empty, or 'string' - skipping DateRange query");
                }
            }

            _logger.LogInformation("=== BuildMustQueries Result: {Count} queries ===", queries.Count);

            // ✅ ÇOK BASİT VE GÜVENLİ QUERY LOGGING
            for (int i = 0; i < queries.Count; i++)
            {
                _logger.LogInformation("Query {Index}: {QueryType}", i, queries[i].GetType().Name);
            }

            return queries;
        }

        /// <summary>
        /// Sort descriptors oluşturur
        /// </summary>
        private SortOptions[] GetSortDescriptors(PaginationFilter pagination, string defaultField)
        {
            var sorts = new List<SortOptions>();

            if (!string.IsNullOrEmpty(pagination.SortField))
            {
                var field = $"properties.{pagination.SortField}";
                var order = pagination.SortDirection.ToLower() == "asc" ? SortOrder.Asc : SortOrder.Desc;

                sorts.Add(new FieldSort(field) { Order = order });
            }
            else
            {
                sorts.Add(new FieldSort(defaultField) { Order = SortOrder.Desc });
            }

            // _id alanını da ekle (unique sorting için)
            sorts.Add(new FieldSort("_id") { Order = SortOrder.Asc });

            return sorts.ToArray();
        }

        /// <summary>
        /// Object'i CartItem'a dönüştürür
        /// </summary>
        private CartItem? ConvertToCartItem(object hit, string id)
        {
            try
            {
                var json = JsonSerializer.Serialize(hit);
                var doc = JsonSerializer.Deserialize<JsonElement>(json);

                _logger.LogDebug("Converting document: {Json}", json);

                // Document yapısını kontrol et
                JsonElement properties;
                JsonElement systemProperties;

                // _source içinde mi yoksa direkt mi?
                var rootElement = doc;
                if (doc.TryGetProperty("_source", out var source))
                {
                    rootElement = source;
                }

                // properties field'ını bul
                if (!rootElement.TryGetProperty("properties", out properties))
                {
                    _logger.LogWarning("Properties field not found in document");
                    return null;
                }

                // systemProperties field'ını bul (opsiyonel)
                rootElement.TryGetProperty("systemProperties", out systemProperties);

                var cartItem = new CartItem
                {
                    Id = id,
                    MusteriNo = GetStringProperty(properties, "MusteriNo"),
                    KullaniciKodu = GetStringProperty(properties, "KullaniciKodu"),
                    MalzemeNo = GetStringProperty(properties, "MalzemeNo"),
                    Adet = GetIntProperty(properties, "Adet"),
                    SiparisNo = GetStringProperty(properties, "SiparisNo"),
                    SevkiyatDepoKodu = GetStringProperty(properties, "SevkiyatDepoKodu"),

                    // ✅ DOĞRU: properties.SonIslemTarihi kullan
                    SonIslemTarihi = GetDateTimeProperty(properties, "SonIslemTarihi"),

                    Il = GetStringProperty(properties, "Il"), // Eğer mapping'de yoksa boş gelir
                    Ilce = GetStringProperty(properties, "Ilce") // Eğer mapping'de yoksa boş gelir
                };

                _logger.LogDebug("Converted CartItem successfully: {@CartItem}", cartItem);
                return cartItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting to CartItem. Document: {Document}", JsonSerializer.Serialize(hit));
                return null;
            }
        }

        /// <summary>
        /// Object'i Customer'a dönüştürür
        /// </summary>
        private Customer? ConvertToCustomer(object hit, string id)
        {
            try
            {
                var json = JsonSerializer.Serialize(hit);
                var doc = JsonSerializer.Deserialize<JsonElement>(json);

                if (!doc.TryGetProperty("properties", out var properties))
                    return null;

                return new Customer
                {
                    Id = id,
                    FirmaAdi = GetStringProperty(properties, "firmaAdi"),
                    MusteriNo = GetStringProperty(properties, "MusteriNo"),
                    Mail = GetStringProperty(properties, "Mail"),
                    Telefon = GetStringProperty(properties, "Telefon"),
                    Il = GetStringProperty(properties, "Il"),
                    Ilce = GetStringProperty(properties, "Ilce"),
                    BolgeKodu = GetStringProperty(properties, "BolgeKodu"),
                    PlasiyerAdi = GetStringProperty(properties, "PlasiyerAdi"),
                    SonGirisTarihi = GetNullableDateTimeProperty(properties, "sonGirisTarihi"),
                    Aktif = GetBoolProperty(properties, "Aktif"),
                    KullaniciTipi = GetStringProperty(properties, "KullaniciTipi"),
                    Adres = GetStringProperty(properties, "adres"),
                    KonumKodu = GetStringProperty(properties, "konumKodu"),
                    DbsLimit = GetDecimalProperty(properties, "DbsLimit"),
                    VadeliDbsBorc = GetDecimalProperty(properties, "VadeliDbsBorc"),
                    KalanDbs = GetDecimalProperty(properties, "KalanDbs"),
                    LimitKullanimOrani = GetDecimalProperty(properties, "LimitKullanimOrani"),
                    CariHesapBakiyesi = GetDecimalProperty(properties, "CariHesapBakiyesi"),
                    KrediLimiti = GetDecimalProperty(properties, "KrediLimiti"),
                    IpotekLimiti = GetDecimalProperty(properties, "IpotekLimiti"),
                    SigortaLimiti = GetDecimalProperty(properties, "SigortaLimiti"),
                    CekRiski = GetDecimalProperty(properties, "CekRiski"),
                    AcikSiparisTutar = GetDecimalProperty(properties, "AcikSiparisTutar"),
                    AcikFaturaTutar = GetDecimalProperty(properties, "AcikFaturaTutar"),
                    AcikDagitimTutar = GetDecimalProperty(properties, "AcikDagitimTutar"),
                    AcikIrsaliye = GetDecimalProperty(properties, "AcikIrsaliye"),
                    Yaslandirma0 = GetDecimalProperty(properties, "Yaslandirma0"),
                    Yaslandirma30 = GetDecimalProperty(properties, "Yaslandirma30"),
                    Yaslandirma60 = GetDecimalProperty(properties, "Yaslandirma60"),
                    Yaslandirma90 = GetDecimalProperty(properties, "Yaslandirma90"),
                    Yaslandirma120 = GetDecimalProperty(properties, "Yaslandirma120"),
                    KalanLimit = GetDecimalProperty(properties, "KalanLimit"),
                    VadesiGecen = GetDecimalProperty(properties, "VadesiGecen"),
                    KullaniciAdi = GetStringProperty(properties, "kullaniciAdi")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting to Customer");
                return null;
            }
        }

        // Helper methods for JSON property extraction
        private string GetStringProperty(JsonElement element, string property) =>
            element.TryGetProperty(property, out var prop) ? prop.GetString() ?? "" : "";

        private int GetIntProperty(JsonElement element, string property) =>
            element.TryGetProperty(property, out var prop) ? prop.GetInt32() : 0;

        private decimal GetDecimalProperty(JsonElement element, string property) =>
            element.TryGetProperty(property, out var prop) ? prop.GetDecimal() : 0m;

        private bool GetBoolProperty(JsonElement element, string property) =>
            element.TryGetProperty(property, out var prop) && prop.GetBoolean();

        private DateTime GetDateTimeProperty(JsonElement element, string property)
        {
            if (element.TryGetProperty(property, out var prop) && prop.TryGetDateTime(out var date))
                return date;
            return DateTime.MinValue;
        }

        private DateTime? GetNullableDateTimeProperty(JsonElement element, string property)
        {
            if (element.TryGetProperty(property, out var prop) && prop.TryGetDateTime(out var date))
                return date;
            return null;
        }

        #endregion Private Methods

        #region Chart Data Methods

        private async Task<List<ChartDataPoint>> GetDailyCartTrendAsync(DashboardFilter filter)
        {
            var queries = BuildMustQueries(filter, _settings.CartIndex);
            var searchRequest = new SearchRequest(_settings.CartIndex)
            {
                Size = 0,
                Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                Aggregations = new Dictionary<string, Aggregation>
                {
                    ["data"] = new DateHistogramAggregation
                    {
                        Field = "properties.SonIslemTarihi",
                        CalendarInterval = CalendarInterval.Day,
                        Format = "yyyy-MM-dd"
                    }
                }
            };

            var response = await _client.SearchAsync<object>(searchRequest);
            var result = new List<ChartDataPoint>();

            if (response.IsValidResponse &&
                response.Aggregations?.TryGetValue("data", out var dataAgg) == true &&
                dataAgg is DateHistogramAggregate dateHistAgg)
            {
                foreach (var bucket in dateHistAgg.Buckets)
                {
                    result.Add(new ChartDataPoint
                    {
                        Label = bucket.KeyAsString ?? "",
                        Value = bucket.DocCount,
                        Key = bucket.KeyAsString ?? "",
                        Date = bucket.Key.DateTime
                    });
                }
            }

            return result;
        }

        private async Task<List<ChartDataPoint>> GetMonthlyTrendAsync(DashboardFilter filter)
        {
            var queries = BuildMustQueries(filter, _settings.CartIndex);
            var searchRequest = new SearchRequest(_settings.CartIndex)
            {
                Size = 0,
                Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                Aggregations = new Dictionary<string, Aggregation>
                {
                    ["data"] = new DateHistogramAggregation
                    {
                        Field = "properties.SonIslemTarihi",
                        CalendarInterval = CalendarInterval.Month,
                        Format = "yyyy-MM"
                    }
                }
            };

            var response = await _client.SearchAsync<object>(searchRequest);
            var result = new List<ChartDataPoint>();

            if (response.IsValidResponse &&
                response.Aggregations?.TryGetValue("data", out var dataAgg) == true &&
                dataAgg is DateHistogramAggregate dateHistAgg)
            {
                foreach (var bucket in dateHistAgg.Buckets)
                {
                    result.Add(new ChartDataPoint
                    {
                        Label = bucket.KeyAsString ?? "",
                        Value = bucket.DocCount,
                        Key = bucket.KeyAsString ?? "",
                        Date = bucket.Key.DateTime
                    });
                }
            }

            return result;
        }

        private async Task<List<ChartDataPoint>> GetTopProductsAsync(DashboardFilter filter)
        {
            var queries = BuildMustQueries(filter, _settings.CartIndex);
            var searchRequest = new SearchRequest(_settings.CartIndex)
            {
                Size = 0,
                Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                Aggregations = new Dictionary<string, Aggregation>
                {
                    ["data"] = new TermsAggregation
                    {
                        Field = "properties.MalzemeNo",
                        Size = 50
                    }
                }
            };

            var response = await _client.SearchAsync<object>(searchRequest);
            var result = new List<ChartDataPoint>();

            if (response.IsValidResponse &&
                response.Aggregations?.TryGetValue("data", out var dataAgg) == true &&
                dataAgg is StringTermsAggregate termsAgg)
            {
                foreach (var bucket in termsAgg.Buckets)
                {
                    var key = bucket.Key.ToString();
                    result.Add(new ChartDataPoint
                    {
                        Label = key,
                        Value = bucket.DocCount,
                        Key = key
                    });
                }
            }

            return result;
        }

        private async Task<List<ChartDataPoint>> GetTopCustomersAsync(DashboardFilter filter)
        {
            var queries = BuildMustQueries(filter, _settings.CartIndex);
            var searchRequest = new SearchRequest(_settings.CartIndex)
            {
                Size = 0,
                Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                Aggregations = new Dictionary<string, Aggregation>
                {
                    ["data"] = new TermsAggregation
                    {
                        Field = "properties.MusteriNo",
                        Size = 50
                    }
                }
            };

            var response = await _client.SearchAsync<object>(searchRequest);
            var result = new List<ChartDataPoint>();

            if (response.IsValidResponse &&
                response.Aggregations?.TryGetValue("data", out var dataAgg) == true &&
                dataAgg is StringTermsAggregate termsAgg)
            {
                foreach (var bucket in termsAgg.Buckets)
                {
                    var key = bucket.Key.ToString();
                    result.Add(new ChartDataPoint
                    {
                        Label = key,
                        Value = bucket.DocCount,
                        Key = key
                    });
                }
            }

            return result;
        }

        private async Task<List<ChartDataPoint>> GetDepotDistributionAsync(DashboardFilter filter)
        {
            var queries = BuildMustQueries(filter, _settings.CartIndex);
            var searchRequest = new SearchRequest(_settings.CartIndex)
            {
                Size = 0,
                Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                Aggregations = new Dictionary<string, Aggregation>
                {
                    ["data"] = new TermsAggregation
                    {
                        Field = "properties.SevkiyatDepoKodu",
                        Size = 50
                    }
                }
            };

            var response = await _client.SearchAsync<object>(searchRequest);
            var result = new List<ChartDataPoint>();

            if (response.IsValidResponse &&
                response.Aggregations?.TryGetValue("data", out var dataAgg) == true &&
                dataAgg is StringTermsAggregate termsAgg)
            {
                foreach (var bucket in termsAgg.Buckets)
                {
                    var key = bucket.Key.ToString();
                    result.Add(new ChartDataPoint
                    {
                        Label = key,
                        Value = bucket.DocCount,
                        Key = key
                    });
                }
            }

            return result;
        }

        private async Task<List<ChartDataPoint>> GetProvincePerformanceAsync(DashboardFilter filter)
        {
            // Profil index'inden il bazında performans verisi
            var queries = BuildMustQueries(filter, _settings.ProfileIndex);
            var searchRequest = new SearchRequest(_settings.ProfileIndex)
            {
                Size = 0,
                Query = queries.Any() ? new BoolQuery { Must = queries } : new MatchAllQuery(),
                Aggregations = new Dictionary<string, Aggregation>
                {
                    ["data"] = new TermsAggregation
                    {
                        Field = "properties.Il",
                        Size = 50
                    }
                }
            };

            var response = await _client.SearchAsync<object>(searchRequest);
            var result = new List<ChartDataPoint>();

            if (response.IsValidResponse &&
                response.Aggregations?.TryGetValue("data", out var dataAgg) == true &&
                dataAgg is StringTermsAggregate termsAgg)
            {
                foreach (var bucket in termsAgg.Buckets)
                {
                    var key = bucket.Key.ToString();
                    result.Add(new ChartDataPoint
                    {
                        Label = key, // Burada il kodunu il adına çevirmek için location service kullanılabilir
                        Value = bucket.DocCount,
                        Key = key
                    });
                }
            }

            return result;
        }

        #endregion Chart Data Methods
    }
}