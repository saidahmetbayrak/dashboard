/**
 * Dashboard Ana JavaScript Dosyası
 * Elasticsearch tabanlı sepet analiz dashboard'u
 */

class CartDashboard {
    constructor() {
        // API endpoints
        this.apiBaseUrl = '/api/DashboardApi';

        // Filtreler
        this.filters = {};

        // Sayfalama bilgileri
        this.pagination = {
            customer: {
                page: 1,
                size: 50,
                total: { value: 0, relation: 'eq' },
                searchAfterHistory: [null],
                hasNextPage: true
            },
            cart: {
                page: 1,
                size: 50,
                total: { value: 0, relation: 'eq' },
                searchAfterHistory: [null],
                hasNextPage: true
            }
        };

        // Table managers
        this.customerTableManager = new TableManager(this, {
            idPrefix: 'customer',
            columns: [
                { field: 'firmaAdi', title: 'Firma Adı', sortable: true, type: 'text' },
                { field: 'musteriNo', title: 'Müşteri No', sortable: true, type: 'text' },
                { field: 'mail', title: 'Email', sortable: false, type: 'text' },
                { field: 'telefon', title: 'Telefon', sortable: false, type: 'text' },
                { field: 'il', title: 'İl', sortable: true, type: 'text' },
                { field: 'ilce', title: 'İlçe', sortable: true, type: 'text' },
                { field: 'bolgeKodu', title: 'Bölge', sortable: true, type: 'text' },
                { field: 'cariHesapBakiyesi', title: 'Bakiye', sortable: true, type: 'currency' }
            ],
            defaultSortField: 'firmaAdi'
        });

        this.cartTableManager = new TableManager(this, {
            idPrefix: 'cart',
            columns: [
                { field: 'musteriNo', title: 'Müşteri No', sortable: true, type: 'text' },
                { field: 'kullaniciKodu', title: 'Kullanıcı Kodu', sortable: true, type: 'text' },
                { field: 'malzemeNo', title: 'Malzeme No', sortable: true, type: 'text' },
                { field: 'adet', title: 'Adet', sortable: true, type: 'number' },
                { field: 'siparisNo', title: 'Sipariş No', sortable: true, type: 'text' },
                { field: 'sevkiyatDepoKodu', title: 'Sevkiyat Depo', sortable: true, type: 'text' },
                { field: 'sonIslemTarihi', title: 'Son İşlem Tarihi', sortable: true, type: 'datetime' }
            ],
            defaultSortField: 'sonIslemTarihi'
        });

        // Chart ve filter manager'lar
        this.chartManager = new ChartManager(this);
        this.filterManager = new FilterManager(this);

        // Başlangıç
        this.init();
    }

    /**
     * Dashboard'u başlatır
     */
    async init() {
        try {
            this.showLoading(true);
            this.setupEventListeners();
            this.setupPopupEventListeners();
            this.customerTableManager.init();
            this.cartTableManager.init();
            this.chartManager.init();
            this.filterManager.init();

            // İlk veri yüklemesi
            await this.applyFiltersAndLoadData();

            this.showLoading(false);
            this.showNotification('Dashboard başarıyla yüklendi', 'success');
        } catch (error) {
            console.error('Dashboard initialization error:', error);
            this.showLoading(false);
            this.showNotification('Dashboard yüklenirken hata oluştu: ' + error.message, 'error');
        }
    }

    /**
     * Event listener'ları kurar
     */
    setupEventListeners() {
        // Filtre butonları
        $('#apply-filters').on('click', (event) => {
            this.applyFiltersAndLoadData(event);
        });

        $('#clear-filters').on('click', () => {
            this.clearFiltersAndUI();
        });

        // Sayfalama butonları
        ['customer', 'cart'].forEach(prefix => {
            $(`#${prefix}-prev`).on('click', () => {
                if (this.pagination[prefix].page > 1) {
                    this.pagination[prefix].page--;
                    this.applyFiltersAndLoadData();
                }
            });

            $(`#${prefix}-next`).on('click', () => {
                if (this.pagination[prefix].hasNextPage) {
                    this.pagination[prefix].page++;
                    this.applyFiltersAndLoadData();
                }
            });
        });
    }

    /**
     * Modal popup event listener'ları kurar
     */
    setupPopupEventListeners() {
        // Bootstrap modal events are handled automatically
    }

    /**
     * Filtreleri temizler ve UI'ı sıfırlar
     */
    async clearFiltersAndUI() {
        try {
            // Form elemanlarını temizle
            $('#musteri-no').val('');
            $('#kullanici-kodu').val('');
            $('#malzeme-no').val('');
            $('#il-filter').val('');
            $('#ilce-filter').val('').prop('disabled', true);
            $('#sevkiyat-depo').val('');
            $('#date-field').val('3');

            // İlçe dropdown'ını sıfırla
            this.filterManager.resetIlceDropdown();

            // Filtreleri temizle
            this.filters = {};

            // Chart visibility'yi güncelle
            this.toggleConditionalCharts();

            // Tabloları gizle
            $('#customer-table-panel').addClass('d-none');
            $('#cart-table-panel').addClass('d-none');

            // Verileri yeniden yükle
            await this.applyFiltersAndLoadData();

            this.showNotification('Filtreler temizlendi', 'info');
        } catch (error) {
            console.error('Clear filters error:', error);
            this.showNotification('Filtreler temizlenirken hata oluştu', 'error');
        }
    }

    /**
     * Filtreleri toplar
     */
    collectFilters() {
        this.filters = {
            musteriNo: $('#musteri-no').val()?.trim() || null,
            kullaniciKodu: $('#kullanici-kodu').val()?.trim() || null,
            malzemeNo: $('#malzeme-no').val()?.trim() || null,
            il: $('#il-filter').val() || null,
            ilce: $('#ilce-filter').val() || null,
            sevkiyatDepoKodu: $('#sevkiyat-depo').val() || null,
            dateRangeMonths: parseInt($('#date-field').val() || '3', 10)
        };
    }

    /**
     * Chart'dan gelen filtreyi uygular
     */
    applyFilterFromChart(filterType, value) {
        let inputId;
        if (filterType === 'musteriNo') {
            inputId = 'musteri-no';
        } else if (filterType === 'malzemeNo') {
            inputId = 'malzeme-no';
        } else if (filterType === 'il') {
            inputId = 'il-filter';
        } else {
            return;
        }

        const inputElement = $(`#${inputId}`);
        if (inputElement.length > 0) {
            inputElement.val(value);
            if (filterType === 'il') {
                inputElement.trigger('change');
            }
        }

        this.applyFiltersAndLoadData();
    }

    /**
     * Müşteri popup'ını gösterir
     */
    showCustomerPopup(customerData) {
        try {
            const popupBody = $('#popup-body');
            if (!popupBody.length) return;

            const labelMap = {
                firmaAdi: "Firma Adı",
                musteriNo: "Müşteri No",
                mail: "E-posta",
                telefon: "Telefon",
                il: "İl",
                ilce: "İlçe",
                bolgeKodu: "Bölge Kodu",
                plasiyerAdi: "Plasiyer Adı",
                sonGirisTarihi: "Son Giriş Tarihi",
                aktif: "Aktif mi?",
                kullaniciTipi: "Kullanıcı Tipi",
                adres: "Adres",
                konumKodu: "Konum Kodu",
                dbsLimit: "DBS Limiti",
                vadeliDbsBorc: "Vadeli DBS Borcu",
                kalanDbs: "Kalan DBS",
                limitKullanimOrani: "Limit Kullanım Oranı (%)",
                cariHesapBakiyesi: "Cari Hesap Bakiyesi",
                krediLimiti: "Kredi Limiti",
                ipotekLimiti: "İpotek Limiti",
                sigortaLimiti: "Sigorta Limiti",
                cekRiski: "Çek Riski",
                acikSiparisTutar: "Açık Sipariş Tutarı",
                acikFaturaTutar: "Açık Fatura Tutarı",
                acikDagitimTutar: "Açık Dağıtım Tutarı",
                acikIrsaliye: "Açık İrsaliye",
                yaslandirma0: "Yaşlandırma (0-30 Gün)",
                yaslandirma30: "Yaşlandırma (31-60 Gün)",
                yaslandirma60: "Yaşlandırma (61-90 Gün)",
                yaslandirma90: "Yaşlandırma (91-120 Gün)",
                yaslandirma120: "Yaşlandırma (120+ Gün)",
                kalanLimit: "Kalan Limit",
                vadesiGecen: "Vadesi Geçen Bakiye",
                kullaniciAdi: "Kullanıcı Adı"
            };

            popupBody.empty();

            // Grid layout için container
            const container = $('<div class="row g-3"></div>');

            for (const key in customerData) {
                if (customerData.hasOwnProperty(key) && key !== 'id') {
                    const label = labelMap[key] || key;
                    let value = customerData[key];

                    // Değer formatlaması
                    if (typeof value === 'boolean') {
                        value = value ? 'Evet' : 'Hayır';
                    } else if (key.toLowerCase().includes('tarih') && value) {
                        value = new Date(value).toLocaleString('tr-TR');
                    } else if (typeof value === 'number' && key !== 'il' && key !== 'ilce' && key !== 'kullaniciTipi') {
                        value = this.formatNumber(value, 2);
                    }

                    const col = $(`
                        <div class="col-md-6 col-lg-4">
                            <div class="card h-100">
                                <div class="card-body p-3">
                                    <h6 class="card-subtitle mb-2 text-muted">${label}</h6>
                                    <p class="card-text">${value !== null && value !== undefined ? value : 'Tanımsız'}</p>
                                </div>
                            </div>
                        </div>
                    `);
                    container.append(col);
                }
            }

            popupBody.append(container);

            // Modal'ı göster
            $('#customer-popup').modal('show');
        } catch (error) {
            console.error('Show customer popup error:', error);
            this.showNotification('Müşteri detayları gösterilirken hata oluştu', 'error');
        }
    }

    /**
     * Koşullu chart'ları gösterir/gizler
     */
    toggleConditionalCharts() {
        const customerChart = $('#customer-analysis-chart-container');
        const productChart = $('#top-products-chart-container');

        if (this.filters.musteriNo || this.filters.kullaniciKodu) {
            customerChart.addClass('d-none');
        } else {
            customerChart.removeClass('d-none');
        }

        if (this.filters.malzemeNo) {
            productChart.addClass('d-none');
        } else {
            productChart.removeClass('d-none');
        }
    }

    /**
     * Filtreleri uygular ve verileri yükler
     */
    async applyFiltersAndLoadData(event) {
        try {
            this.showLoading(true);
            this.collectFilters();

            // Eğer apply-filters butonuna tıklanmışsa sayfalamayı sıfırla
            if (event && event.currentTarget && event.currentTarget.id === 'apply-filters') {
                for (const key in this.pagination) {
                    this.pagination[key].page = 1;
                    this.pagination[key].searchAfterHistory = [null];
                    this.pagination[key].hasNextPage = true;
                    this.pagination[key].total = { value: 0, relation: 'eq' };
                }
            }

            // Chart'ları güncelle
            await this.chartManager.updateAllCharts(this.filters);
            this.toggleConditionalCharts();

            // Tablo verilerini yükle (sadece filtre varsa)
            const shouldLoadTables = this.filters.musteriNo || this.filters.kullaniciKodu || this.filters.malzemeNo;
            const customerPanel = $('#customer-table-panel');
            const cartPanel = $('#cart-table-panel');

            if (shouldLoadTables) {
                customerPanel.removeClass('d-none');
                cartPanel.removeClass('d-none');

                await Promise.all([
                    this.customerTableManager.loadTable(this.filters, this.pagination.customer),
                    this.cartTableManager.loadTable(this.filters, this.pagination.cart)
                ]);
            } else {
                customerPanel.addClass('d-none');
                cartPanel.addClass('d-none');
            }

            this.showLoading(false);
        } catch (error) {
            console.error('Apply filters error:', error);
            this.showLoading(false);
            this.showNotification("Veriler yüklenirken bir hata oluştu: " + error.message, 'error');
        }
    }

    /**
     * Sayı formatlaması
     */
    formatNumber(num, decimals = 0) {
        if (num === null || num === undefined) return '0';

        const options = {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
            useGrouping: true
        };

        return new Intl.NumberFormat('tr-TR', options).format(num);
    }

    /**
     * Loading overlay'i gösterir/gizler
     */
    showLoading(show = true) {
        const overlay = $('#loading-overlay');
        if (show) {
            overlay.removeClass('d-none');
        } else {
            overlay.addClass('d-none');
        }
    }

    /**
     * Toast notification gösterir
     */
    showNotification(message, type = 'info') {
        const toast = $('#notification-toast');
        const toastBody = toast.find('.toast-body');
        const toastHeader = toast.find('.toast-header');

        // Icon ve renk ayarla
        let icon, bgClass;
        switch (type) {
            case 'success':
                icon = 'fas fa-check-circle text-success';
                bgClass = 'bg-success text-white';
                break;
            case 'error':
                icon = 'fas fa-exclamation-triangle text-danger';
                bgClass = 'bg-danger text-white';
                break;
            case 'warning':
                icon = 'fas fa-exclamation-triangle text-warning';
                bgClass = 'bg-warning text-dark';
                break;
            default:
                icon = 'fas fa-info-circle text-primary';
                bgClass = 'bg-primary text-white';
        }

        toastHeader.find('i').attr('class', icon);
        toastHeader.removeClass().addClass(`toast-header ${bgClass}`);
        toastBody.text(message);

        // Toast'ı göster
        const bsToast = new bootstrap.Toast(toast[0]);
        bsToast.show();
    }
}

// Global fonksiyonlar
window.refreshAllData = function () {
    if (window.dashboard) {
        window.dashboard.applyFiltersAndLoadData();
    }
};

window.exportData = function (format) {
    console.log(`Export to ${format} - Feature coming soon`);
    if (window.dashboard) {
        window.dashboard.showNotification(`${format.toUpperCase()} export özelliği yakında eklenecek`, 'info');
    }
};

// Dashboard'u başlat
$(document).ready(function () {
    window.dashboard = new CartDashboard();
});