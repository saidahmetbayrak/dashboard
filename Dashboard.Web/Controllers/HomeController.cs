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
    /// Ana sayfa controller'�
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
        /// Dashboard ana sayfas�
        /// </summary>
        /// <returns>Dashboard view</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading dashboard index page");

                var viewModel = new DashboardViewModel();

                // Paralel olarak gerekli verileri y�kle
                var healthCheckTask = _apiService.HealthCheckAsync(); // Art�k bool d�ner
                var provincesTask = _apiService.GetProvincesAsync();
                var depotTask = _apiService.GetDropdownDataAsync("properties.SevkiyatDepoKodu", "context-sepet");

                // T�m g�revlerin tamamlanmas�n� bekle
                await Task.WhenAll(healthCheckTask, provincesTask, depotTask);

                // G�rev sonu�lar�n� ilgili de�i�kenlere ata
                var isHealthy = await healthCheckTask; // Sadece bool
                var provincesResult = await provincesTask;
                var depotResult = await depotTask;

                // Health check sonucu - Art�k direkt bool
                viewModel.IsElasticsearchConnected = isHealthy;

                if (!viewModel.IsElasticsearchConnected)
                {
                    viewModel.ErrorMessage = "Elasticsearch ba�lant�s� kurulamad�";
                    _logger.LogWarning("Elasticsearch connection failed");
                }

                // �l listesi
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
                    ErrorMessage = "Dashboard y�klenirken bir hata olu�tu",
                    IsElasticsearchConnected = false
                };

                return View(errorModel);
            }
        }

        /// <summary>
        /// Hakk�nda sayfas�
        /// </summary>
        /// <returns>About view</returns>
        public IActionResult About()
        {
            _logger.LogInformation("Loading about page");

            ViewData["Title"] = "Hakk�nda";
            ViewData["Message"] = "Dashboard Analytics - Elasticsearch tabanl� sepet analiz sistemi";

            return View();
        }

        /// <summary>
        /// �leti�im sayfas�
        /// </summary>
        /// <returns>Contact view</returns>
        public IActionResult Contact()
        {
            _logger.LogInformation("Loading contact page");

            ViewData["Title"] = "�leti�im";
            ViewData["Message"] = "�leti�im bilgileri ve destek";

            return View();
        }

        /// <summary>
        /// Gizlilik sayfas�
        /// </summary>
        /// <returns>Privacy view</returns>
        public IActionResult Privacy()
        {
            _logger.LogInformation("Loading privacy page");
            return View();
        }

        /// <summary>
        /// Hata sayfas�
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
                ErrorMessage = "Beklenmeyen bir hata olu�tu",
                StatusCode = HttpContext.Response.StatusCode
            };

            return View(errorModel);
        }

        /// <summary>
        /// 404 Not Found sayfas�
        /// </summary>
        /// <returns>NotFound view</returns>
        public IActionResult NotFound()
        {
            _logger.LogWarning("404 Not Found requested");

            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = "Arad���n�z sayfa bulunamad�",
                StatusCode = 404,
                Title = "Sayfa Bulunamad�"
            };

            Response.StatusCode = 404;
            return View("Error", errorModel);
        }

        /// <summary>
        /// API durumu kontrol� - Detayl� response i�in
        /// </summary>
        /// <returns>JSON response</returns>
        [HttpGet]
        public async Task<IActionResult> ApiStatus()
        {
            try
            {
                _logger.LogInformation("Checking API status");

                // Detayl� health check kullan (JSON response i�in)
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
        /// Basit API ba�lant� kontrol�
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