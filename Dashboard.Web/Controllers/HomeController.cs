using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Models;
using Dashboard.Web.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Controllers
{
    /// <summary>
    /// Ana sayfa controller'ý
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IApiService apiService, ILogger<HomeController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard ana sayfasý
        /// </summary>
        /// <returns>Dashboard view</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading dashboard index page");

                var viewModel = new DashboardViewModel();

                // Paralel olarak gerekli verileri yükle
                var healthCheckTask = _apiService.HealthCheckAsync(); // Artýk bool döner
                var provincesTask = _apiService.GetProvincesAsync();
                var depotTask = _apiService.GetDropdownDataAsync("properties.SevkiyatDepoKodu", "context-sepet");

                // Tüm görevlerin tamamlanmasýný bekle
                await Task.WhenAll(healthCheckTask, provincesTask, depotTask);

                // Görev sonuçlarýný ilgili deðiþkenlere ata
                var isHealthy = await healthCheckTask; // Sadece bool
                var provincesResult = await provincesTask;
                var depotResult = await depotTask;

                // Health check sonucu - Artýk direkt bool
                viewModel.IsElasticsearchConnected = isHealthy;

                if (!viewModel.IsElasticsearchConnected)
                {
                    viewModel.ErrorMessage = "Elasticsearch baðlantýsý kurulamadý";
                    _logger.LogWarning("Elasticsearch connection failed");
                }

                // Ýl listesi
                if (provincesResult.Success && provincesResult.Data != null)
                {
                    viewModel.Provinces = provincesResult.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to load provinces: {Message}", provincesResult.Message);
                }

                // Sevkiyat depo listesi
                if (depotResult.Success && depotResult.Data != null)
                {
                    viewModel.DepotList = depotResult.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to load depot list: {Message}", depotResult.Message);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard index page");

                var errorModel = new DashboardViewModel
                {
                    ErrorMessage = "Dashboard yüklenirken bir hata oluþtu",
                    IsElasticsearchConnected = false
                };

                return View(errorModel);
            }
        }

        /// <summary>
        /// Hakkýnda sayfasý
        /// </summary>
        /// <returns>About view</returns>
        public IActionResult About()
        {
            _logger.LogInformation("Loading about page");

            ViewData["Title"] = "Hakkýnda";
            ViewData["Message"] = "Dashboard Analytics - Elasticsearch tabanlý sepet analiz sistemi";

            return View();
        }

        /// <summary>
        /// Ýletiþim sayfasý
        /// </summary>
        /// <returns>Contact view</returns>
        public IActionResult Contact()
        {
            _logger.LogInformation("Loading contact page");

            ViewData["Title"] = "Ýletiþim";
            ViewData["Message"] = "Ýletiþim bilgileri ve destek";

            return View();
        }

        /// <summary>
        /// Gizlilik sayfasý
        /// </summary>
        /// <returns>Privacy view</returns>
        public IActionResult Privacy()
        {
            _logger.LogInformation("Loading privacy page");
            return View();
        }

        /// <summary>
        /// Hata sayfasý
        /// </summary>
        /// <returns>Error view</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error page requested with RequestId: {RequestId}", requestId);

            var errorModel = new ErrorViewModel
            {
                RequestId = requestId,
                ErrorMessage = "Beklenmeyen bir hata oluþtu",
                StatusCode = HttpContext.Response.StatusCode
            };

            return View(errorModel);
        }

        /// <summary>
        /// 404 Not Found sayfasý
        /// </summary>
        /// <returns>NotFound view</returns>
        public IActionResult NotFound()
        {
            _logger.LogWarning("404 Not Found requested");

            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = "Aradýðýnýz sayfa bulunamadý",
                StatusCode = 404,
                Title = "Sayfa Bulunamadý"
            };

            Response.StatusCode = 404;
            return View("Error", errorModel);
        }

        /// <summary>
        /// API durumu kontrolü - Detaylý response için
        /// </summary>
        /// <returns>JSON response</returns>
        [HttpGet]
        public async Task<IActionResult> ApiStatus()
        {
            try
            {
                _logger.LogInformation("Checking API status");

                // Detaylý health check kullan (JSON response için)
                var healthCheck = await _apiService.HealthCheckDetailedAsync();

                return Json(new
                {
                    success = healthCheck.Success,
                    connected = healthCheck.Data,
                    message = healthCheck.Message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking API status");

                return Json(new
                {
                    success = false,
                    connected = false,
                    message = "API durumu kontrol edilemedi",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Basit API baðlantý kontrolü
        /// </summary>
        /// <returns>JSON response</returns>
        [HttpGet]
        public async Task<IActionResult> QuickApiCheck()
        {
            try
            {
                _logger.LogInformation("Performing quick API check");

                // Basit health check kullan
                var isHealthy = await _apiService.HealthCheckAsync();

                return Json(new
                {
                    connected = isHealthy,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in quick API check");

                return Json(new
                {
                    connected = false,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}