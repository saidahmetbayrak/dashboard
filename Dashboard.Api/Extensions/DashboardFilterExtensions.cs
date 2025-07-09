using Dashboard.Models.Filter;

namespace Dashboard.Api.Extensions
{
    public static class DashboardFilterExtensions
    {
        /// <summary>
        /// "string" placeholder değerlerini null'a çevirir
        /// </summary>
        public static DashboardFilter CleanStringPlaceholders(this DashboardFilter filter)
        {
            if (filter == null) return new DashboardFilter();

            return new DashboardFilter
            {
                MusteriNo = CleanStringValue(filter.MusteriNo),
                KullaniciKodu = CleanStringValue(filter.KullaniciKodu),
                MalzemeNo = CleanStringValue(filter.MalzemeNo),
                Il = CleanStringValue(filter.Il),
                Ilce = CleanStringValue(filter.Ilce),
                SevkiyatDepoKodu = CleanStringValue(filter.SevkiyatDepoKodu),
                DateRange = CleanDateRange(filter.DateRange)
            };
        }

        private static string? CleanStringValue(string? value)
        {
            if (string.IsNullOrEmpty(value) ||
                value.Trim() == "string" ||
                value.Trim() == "null" ||
                value.Trim() == "undefined")
            {
                return null;
            }
            return value.Trim();
        }

        private static DateRangeFilter? CleanDateRange(DateRangeFilter? dateRange)
        {
            if (dateRange == null) return null;

            var cleanedField = CleanStringValue(dateRange.Field);
            if (cleanedField == null) return null;

            return new DateRangeFilter
            {
                Start = dateRange.Start,
                End = dateRange.End,
                Field = cleanedField
            };
        }
    }
}