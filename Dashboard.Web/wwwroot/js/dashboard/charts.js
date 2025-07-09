/**
 * Chart Manager - Dashboard chart işlemlerini yönetir
 */
class ChartManager {
    constructor(dashboard) {
        this.dashboard = dashboard;
        this.charts = {};

        // Chart renkleri
        this.chartColors = [
            '#A93439', '#4A6D7C', '#F9A03F', '#5E8C61', '#7D5BA6', '#00A99D',
            '#E98074', '#3498db', '#FFC107', '#9B59B6', '#34495E', '#8BC34A'
        ];

        // Date adapter ayarlarını başlangıçta yap
        this.initializeDateAdapter();
    }

    /**
     * Date adapter'ı başlatır ve test eder
     */
    initializeDateAdapter() {
        try {
            // Chart.js date adapter konfigürasyonu
            if (typeof Chart !== 'undefined') {
                // Date adapter test
                Chart.defaults.adapters = {
                    date: {}
                };

                console.log('✅ Chart.js date adapter initialized');
            } else {
                console.error('❌ Chart.js not loaded');
            }
        } catch (error) {
            console.error('❌ Date adapter initialization failed:', error);
        }
    }

    /**
     * Chart manager'ı başlatır
     */
    init() {
        // Chart.js global ayarları
        Chart.defaults.font.family = "'Inter', 'Segoe UI', sans-serif";
        Chart.defaults.plugins.legend.position = 'bottom';
        Chart.defaults.maintainAspectRatio = false;

        // Date adapter final kontrolü
        this.verifyDateAdapter();

        this.setupAllCharts();
    }

    /**
     * Date adapter'ın doğru yüklendiğini kontrol eder
     */
    verifyDateAdapter() {
        try {
            // Time scale test
            const timeScale = Chart.registry.getScale('time');
            if (timeScale) {
                console.log('✅ Time scale is available');
            } else {
                console.warn('⚠️ Time scale not available');
            }
        } catch (error) {
            console.warn('⚠️ Date adapter verification failed:', error);
        }
    }

    /**
     * Tüm chart'ları kurar
     */
    setupAllCharts() {
        // Günlük sepet chart'ı - Time scale ile
        this.createChart('daily-cart-chart', 'line', 'Günlük Sepet Sayısı', {
            plugins: {
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        title: function (context) {
                            // Moment.js ile tarih formatla
                            return moment(context[0].label).format('DD MMMM YYYY');
                        }
                    }
                }
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'day',
                        displayFormats: {
                            day: 'DD MMM'
                        },
                        tooltipFormat: 'DD MMMM YYYY'
                    },
                    title: {
                        display: true,
                        text: 'Tarih'
                    }
                },
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Sepet Sayısı'
                    },
                    ticks: {
                        callback: function (value) {
                            return new Intl.NumberFormat('tr-TR').format(value);
                        }
                    }
                }
            }
        });

        // Aylık trend chart'ı - Time scale ile
        this.createChart('monthly-trend-chart', 'bar', 'Aylık Sepet Trendi', {
            plugins: {
                tooltip: {
                    callbacks: {
                        title: function (context) {
                            // Aylık format için
                            return moment(context[0].label).format('MMMM YYYY');
                        },
                        label: function (context) {
                            return `${context.formattedValue} sepet`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'month',
                        displayFormats: {
                            month: 'MMM YY'
                        },
                        tooltipFormat: 'MMMM YYYY'
                    },
                    title: {
                        display: true,
                        text: 'Ay'
                    }
                },
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Sepet Sayısı'
                    },
                    ticks: {
                        callback: function (value) {
                            return new Intl.NumberFormat('tr-TR').format(value);
                        }
                    }
                }
            }
        });

        this.createChart('top-products-chart', 'bar', 'En Çok Eklenen Ürünler', {
            indexAxis: 'y',
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    const label = this.charts['top-products-chart'].data.labels[index];
                    if (label && label !== 'Veri Yok') {
                        this.dashboard.applyFilterFromChart('malzemeNo', label);
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return `${context.formattedValue} sepet`;
                        }
                    }
                }
            }
        });

        this.createChart('customer-analysis-chart', 'bar', 'En Aktif Müşteriler', {
            indexAxis: 'y',
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    const label = this.charts['customer-analysis-chart'].data.labels[index];
                    if (label && label !== 'Veri Yok') {
                        this.dashboard.applyFilterFromChart('musteriNo', label);
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return `${context.formattedValue} işlem`;
                        }
                    }
                }
            }
        });

        this.createChart('depot-distribution-chart', 'doughnut', 'Depo Dağılımı', {
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((context.raw / total) * 100).toFixed(1);
                            return `${context.label}: ${context.formattedValue} (${percentage}%)`;
                        }
                    }
                }
            }
        });

        this.createChart('il-bazinda-performans-chart', 'bar', 'İl Bazında Performans', {
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    const provinceName = this.charts['il-bazinda-performans-chart'].data.labels[index];

                    // İl adından il kodunu bul (reverse mapping gerekebilir)
                    // Şu an basitçe il adını kullanıyoruz
                    if (provinceName && provinceName !== 'Veri Yok') {
                        this.dashboard.applyFilterFromChart('il', provinceName);
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return `${context.formattedValue} müşteri`;
                        }
                    }
                }
            }
        });
    }

    /**
     * Chart oluşturur
     */
    createChart(canvasId, type, label, customOptions = {}) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.warn(`Canvas element not found: ${canvasId}`);
            return;
        }

        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            scales: (type === 'bar' || type === 'line') && !customOptions.scales ? {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return new Intl.NumberFormat('tr-TR').format(value);
                        }
                    }
                }
            } : {},
            plugins: {
                legend: {
                    display: type !== 'bar' || customOptions.indexAxis === 'y'
                }
            }
        };

        // Deep merge options
        const options = this.mergeDeep(defaultOptions, customOptions);

        this.charts[canvasId] = new Chart(ctx, {
            type: type,
            data: {
                labels: [],
                datasets: [{
                    label: label,
                    data: [],
                    backgroundColor: type === 'doughnut' || type === 'pie' ?
                        this.chartColors :
                        this.chartColors[0] + '80', // Alpha transparency
                    borderColor: this.chartColors[0],
                    borderWidth: 2
                }]
            },
            options: options
        });
    }

    /**
     * Tüm chart'ları günceller - Debug ve error handling ile
     */
    /**
 * Tüm chart'ları günceller - IsSuccess property ile uyumlu
 */
    async updateAllCharts(filters) {
        try {
            console.log('🔄 Updating charts with filters:', filters);

            const response = await $.ajax({
                url: `${this.dashboard.apiBaseUrl}/charts`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(filters),
                headers: {
                    'X-CSRF-TOKEN': window.csrfToken
                }
            });

            console.log('📥 Raw API Response:', response);

            // ✅ C# naming convention'ına göre kontrol et
            const isSuccessful = response?.IsSuccess || response?.isSuccess || response?.success;
            const responseData = response?.Data || response?.data;
            const responseMessage = response?.Message || response?.message;

            console.log('✅ IsSuccess:', response?.IsSuccess);
            console.log('📈 Response Data:', responseData);
            console.log('💬 Response Message:', responseMessage);

            if (response && isSuccessful && responseData) {
                const chartData = responseData;
                console.log('📊 Chart data received:', chartData);

                // Her chart'ı güncelle
                this.updateTimeSeriesChart('daily-cart-chart', chartData.DailyCartTrend);
                this.updateTimeSeriesChart('monthly-trend-chart', chartData.MonthlyTrend);
                this.updateChart('top-products-chart', chartData.TopProducts);
                this.updateChart('customer-analysis-chart', chartData.TopCustomers);
                this.updateChart('depot-distribution-chart', chartData.DepotDistribution);
                this.updateChart('il-bazinda-performans-chart', chartData.ProvincePerformance);

                console.log('✅ All charts updated successfully');
            } else {
                console.error('❌ Chart data update failed.');
                console.error('   - IsSuccess:', isSuccessful);
                console.error('   - Data exists:', !!responseData);
                console.error('   - Message:', responseMessage);
                this.showEmptyCharts();

                if (this.dashboard && this.dashboard.showNotification) {
                    this.dashboard.showNotification(
                        responseMessage || 'Chart verileri alınamadı',
                        'warning'
                    );
                }
            }
        } catch (error) {
            console.error('❌ Ajax error in updateAllCharts:', error);
            this.showEmptyCharts();

            if (this.dashboard && this.dashboard.showNotification) {
                this.dashboard.showNotification('Chart verileri güncellenirken hata oluştu', 'error');
            }
        }
    }

    /**
     * Time series chart'ı günceller (günlük/aylık trendler için)
     */
    updateTimeSeriesChart(chartId, data) {
        const chart = this.charts[chartId];
        if (!chart) {
            console.warn(`Chart not found: ${chartId}`);
            return;
        }

        console.log(`📊 Updating time series chart ${chartId} with data:`, data);

        if (!data || data.length === 0) {
            console.log(`⚠️ No data for chart ${chartId}`);
            chart.data.labels = [];
            chart.data.datasets[0].data = [];
            chart.update('none');
            return;
        }

        // Time series data format: {x: date, y: value}
        const timeSeriesData = data.map(item => {
            const dateValue = new Date(item.date || item.key || item.label);
            console.log(`📅 Processing date: ${item.date || item.key || item.label} -> ${dateValue}`);
            return {
                x: dateValue,
                y: item.value || 0
            };
        });

        console.log(`📈 Time series data for ${chartId}:`, timeSeriesData);

        chart.data.datasets[0].data = timeSeriesData;
        chart.data.labels = []; // Time scale doesn't need labels

        // Renk ayarları
        const color = this.chartColors[0];
        chart.data.datasets[0].backgroundColor = chart.config.type === 'line' ?
            color + '20' : color + '80';
        chart.data.datasets[0].borderColor = color;

        chart.update('none');
    }

    /**
     * Tek bir chart'ı günceller
     */
    updateChart(chartId, data) {
        const chart = this.charts[chartId];
        if (!chart) {
            console.warn(`Chart not found: ${chartId}`);
            return;
        }

        console.log(`📊 Updating chart ${chartId} with data:`, data);

        if (!data || data.length === 0) {
            console.log(`⚠️ No data for chart ${chartId}`);
            this.updateChartData(chartId, ['Veri Yok'], [0]);
            return;
        }

        const labels = data.map(item => item.label || item.key || 'Tanımsız');
        const values = data.map(item => item.value || 0);

        console.log(`📊 Chart ${chartId} labels:`, labels);
        console.log(`📊 Chart ${chartId} values:`, values);

        this.updateChartData(chartId, labels, values);
    }

    /**
     * Chart verilerini günceller
     */
    updateChartData(chartId, labels, data) {
        const chart = this.charts[chartId];
        if (!chart) return;

        chart.data.labels = labels;
        chart.data.datasets.forEach((dataset, index) => {
            dataset.data = data;

            // Renk ayarları
            if (chart.config.type === 'doughnut' || chart.config.type === 'pie') {
                dataset.backgroundColor = Array.from({ length: data.length }, (_, i) =>
                    this.chartColors[i % this.chartColors.length]);
            } else {
                const color = this.chartColors[index % this.chartColors.length];
                dataset.backgroundColor = chart.config.type === 'line' ?
                    color + '20' : color + '80';
                dataset.borderColor = color;
            }
        });

        chart.update('none');
    }

    /**
     * Boş chart'ları gösterir
     */
    showEmptyCharts() {
        console.log('📊 Showing empty charts');
        Object.keys(this.charts).forEach(chartId => {
            // Time series chart'lar için özel işlem
            if (chartId === 'daily-cart-chart' || chartId === 'monthly-trend-chart') {
                const chart = this.charts[chartId];
                if (chart) {
                    chart.data.datasets[0].data = [];
                    chart.data.labels = [];
                    chart.update('none');
                }
            } else {
                this.updateChartData(chartId, ['Veri Yok'], [0]);
            }
        });
    }

    /**
     * Tüm chart'ları temizler
     */
    clearAllCharts() {
        Object.keys(this.charts).forEach(chartId => {
            const chart = this.charts[chartId];
            if (chart) {
                chart.data.datasets[0].data = [];
                chart.data.labels = [];
                chart.update('none');
            }
        });
    }

    /**
     * Deep merge utility
     */
    mergeDeep(target, source) {
        const output = Object.assign({}, target);
        if (this.isObject(target) && this.isObject(source)) {
            Object.keys(source).forEach(key => {
                if (this.isObject(source[key])) {
                    if (!(key in target))
                        Object.assign(output, { [key]: source[key] });
                    else
                        output[key] = this.mergeDeep(target[key], source[key]);
                } else {
                    Object.assign(output, { [key]: source[key] });
                }
            });
        }
        return output;
    }

    /**
     * Object kontrolü
     */
    isObject(item) {
        return item && typeof item === 'object' && !Array.isArray(item);
    }

    /**
     * Chart'ları yok eder (cleanup için)
     */
    destroy() {
        Object.values(this.charts).forEach(chart => {
            if (chart && typeof chart.destroy === 'function') {
                chart.destroy();
            }
        });
        this.charts = {};
    }
}