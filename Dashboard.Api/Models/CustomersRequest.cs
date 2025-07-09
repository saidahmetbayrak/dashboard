using Dashboard.Models.Filter;

namespace Dashboard.Api.Models
{
    /// <summary>
    /// Müşteri verileri isteği
    /// </summary>
    public class CustomersRequest
    {
        /// <summary>
        /// Filtre parametreleri
        /// </summary>
        public DashboardFilter Filter { get; set; } = new();

        /// <summary>
        /// Sayfalama parametreleri
        /// </summary>
        public PaginationFilter Pagination { get; set; } = new();
    }
}