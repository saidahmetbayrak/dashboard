/**
 * Table Manager - Dashboard tablo işlemlerini yönetir
 */
class TableManager {
    constructor(dashboard, config) {
        this.dashboard = dashboard;
        this.config = config;
        this.table = {
            data: [],
            sortField: config.defaultSortField,
            sortDirection: 'desc'
        };
    }

    /**
     * Table manager'ı başlatır
     */
    init() {
        this.setupTableHeader();
        this.setupEventListeners();
        this.showInfoMessage("Detaylı veri için Müşteri No, Kullanıcı Kodu veya Malzeme No ile filtreleyiniz.");
    }

    /**
     * Tablo header'ını kurar
     */
    setupTableHeader() {
        const headerContainer = $(`#${this.config.idPrefix}-table thead`);
        if (!headerContainer.length) return;

        const tr = $('<tr></tr>');

        this.config.columns.forEach(col => {
            const th = $('<th></th>').text(col.title);

            if (col.sortable) {
                th.attr('data-sort', col.field);
                th.addClass('sortable');
                th.css('cursor', 'pointer');
                th.attr('title', 'Sıralamak için tıklayın');
            }

            tr.append(th);
        });

        headerContainer.empty().append(tr);
    }

    /**
     * Event listener'ları kurar
     */
    setupEventListeners() {
        const tableElement = $(`#${this.config.idPrefix}-table`);

        // Sıralama için tıklama eventi
        tableElement.off('click.sorting').on('click.sorting', 'th[data-sort]', (e) => {
            const sortField = $(e.currentTarget).data('sort');
            this.sortTableByUI(sortField);
        });

        // Müşteri tablosu için satır tıklama eventi
        if (this.config.idPrefix === 'customer') {
            tableElement.off('click.rowSelect').on('click.rowSelect', 'tbody tr', (e) => {
                const tr = $(e.currentTarget);
                const rowIndex = parseInt(tr.data('row-index'), 10);

                if (!isNaN(rowIndex) && this.table.data[rowIndex]) {
                    const customerData = this.table.data[rowIndex];
                    this.dashboard.showCustomerPopup(customerData);
                }
            });
        }
    }

    /**
     * Tablo verilerini yükler
     */
    /**
 * Tablo verilerini yükler - IsSuccess property ile uyumlu
 */
    async loadTable(filters, pagination) {
        try {
            this.showTableLoading(true);

            const endpoint = this.config.idPrefix === 'customer' ? 'customers' : 'cart-items';
            const requestData = {
                ...filters,
                page: pagination.page,
                size: pagination.size,
                sortField: this.table.sortField,
                sortDirection: this.table.sortDirection,
                searchAfter: pagination.searchAfterHistory[pagination.page - 1]
            };

            const response = await $.ajax({
                url: `${this.dashboard.apiBaseUrl}/${endpoint}`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(requestData),
                headers: {
                    'X-CSRF-TOKEN': window.csrfToken
                }
            });

            console.log('📥 Table response:', response);

            // ✅ IsSuccess property'sini kontrol et (backend'deki gerçek property)
            const isSuccessful = response?.isSuccess || response?.IsSuccess || response?.success;

            if (response && isSuccessful && response.data) {
                const result = response.data;

                this.table.data = result.data || [];

                // Pagination bilgilerini güncelle
                pagination.total = result.total || { value: 0, relation: 'eq' };
                pagination.hasNextPage = result.hasNextPage || false;

                // Search after değerini kaydet
                if (result.hasNextPage && !pagination.searchAfterHistory[pagination.page]) {
                    pagination.searchAfterHistory[pagination.page] = result.searchAfter;
                }

                this.renderTable();
                this.updatePaginationControls(pagination);
                this.updateSortHeaders();
            } else {
                console.error('❌ Table load failed:', response);
                this.showTableError(response?.message || 'Veri yüklenemedi');
            }
        } catch (error) {
            console.error(`Table load error (${this.config.idPrefix}):`, error);
            this.showTableError('Veri yüklenirken hata oluştu: ' + error.message);
        } finally {
            this.showTableLoading(false);
        }
    }

    /**
     * Tabloyu render eder
     */
    renderTable() {
        const tbody = $(`#${this.config.idPrefix}-table-body`);
        if (!tbody.length) return;

        tbody.empty();

        if (this.table.data.length === 0) {
            const shouldLoadTables = this.dashboard.filters.musteriNo ||
                this.dashboard.filters.kullaniciKodu ||
                this.dashboard.filters.malzemeNo;

            if (!shouldLoadTables) {
                this.showInfoMessage("Detaylı veri için Müşteri No, Kullanıcı Kodu veya Malzeme No ile filtreleyiniz.");
            } else {
                this.showTableEmpty();
            }
        } else {
            this.table.data.forEach((row, index) => {
                const tr = $('<tr></tr>');
                tr.attr('data-row-index', index);

                if (this.config.idPrefix === 'customer') {
                    tr.css('cursor', 'pointer');
                    tr.attr('title', 'Detayları görmek için tıklayın');
                }

                this.config.columns.forEach(col => {
                    const td = $('<td></td>');
                    const cellValue = this.formatCellValue(row[col.field], col, row);
                    td.html(cellValue);
                    tr.append(td);
                });

                tbody.append(tr);
            });
        }

        this.updateTableInfo(this.dashboard.pagination[this.config.idPrefix]);
    }

    /**
     * Tabloyu temizler
     */
    clearTable() {
        this.table.data = [];
        this.renderTable();

        const pagination = this.dashboard.pagination[this.config.idPrefix];
        if (pagination) {
            pagination.total = { value: 0, relation: 'eq' };
            pagination.page = 1;
            this.updatePaginationControls(pagination);
        }
    }

    /**
     * Hücre değerini formatlar
     */
    formatCellValue(value, col, rowData) {
        if (value === null || value === undefined || value === '' || Number.isNaN(value)) {
            return '<span class="text-muted">Tanımsız</span>';
        }

        switch (col.type) {
            case 'currency':
                return this.dashboard.formatNumber(value, 2) + ' ₺';
            case 'number':
                return this.dashboard.formatNumber(value, 0);
            case 'datetime':
                try {
                    return new Date(value).toLocaleString('tr-TR');
                } catch {
                    return value;
                }
            case 'date':
                try {
                    return new Date(value).toLocaleDateString('tr-TR');
                } catch {
                    return value;
                }
            case 'boolean':
                return value ?
                    '<i class="fas fa-check text-success"></i>' :
                    '<i class="fas fa-times text-danger"></i>';
            default:
                return String(value);
        }
    }

    /**
     * Bilgi mesajı gösterir
     */
    showInfoMessage(message) {
        const tbody = $(`#${this.config.idPrefix}-table-body`);
        if (!tbody.length) return;

        const colspan = this.config.columns.length;
        tbody.html(`
            <tr>
                <td colspan="${colspan}" class="text-center py-4">
                    <div class="text-muted">
                        <i class="fas fa-info-circle fa-2x mb-2"></i>
                        <div>${message}</div>
                    </div>
                </td>
            </tr>
        `);

        const infoElement = $(`#${this.config.idPrefix}-count`);
        if (infoElement.length) {
            infoElement.text('Filtreleme bekleniyor...');
        }

        const pagination = this.dashboard.pagination[this.config.idPrefix];
        if (pagination) {
            pagination.total = { value: 0, relation: 'eq' };
            pagination.page = 1;
            this.updatePaginationControls(pagination);
        }
    }

    /**
     * UI'dan sıralama yapar
     */
    sortTableByUI(sortField) {
        if (this.table.sortField === sortField) {
            this.table.sortDirection = this.table.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.table.sortField = sortField;
            this.table.sortDirection = 'desc';
        }

        const shouldLoadTables = this.dashboard.filters.musteriNo ||
            this.dashboard.filters.kullaniciKodu ||
            this.dashboard.filters.malzemeNo;

        if (shouldLoadTables) {
            const pagination = this.dashboard.pagination[this.config.idPrefix];
            pagination.page = 1;
            pagination.searchAfterHistory = [null];
            pagination.hasNextPage = true;

            this.dashboard.applyFiltersAndLoadData();
        }
    }

    /**
     * Sıralama header'larını günceller
     */
    updateSortHeaders() {
        $(`#${this.config.idPrefix}-table th[data-sort]`).each((index, th) => {
            const $th = $(th);
            $th.removeClass('sort-asc sort-desc');

            if ($th.data('sort') === this.table.sortField) {
                $th.addClass(this.table.sortDirection === 'asc' ? 'sort-asc' : 'sort-desc');
            }
        });
    }

    /**
     * Tablo bilgilerini günceller
     */
    updateTableInfo(pagination) {
        const infoElement = $(`#${this.config.idPrefix}-count`);
        if (!infoElement.length || !pagination || pagination.total.value === 0) {
            if (infoElement.length) {
                infoElement.text('Filtreleme bekleniyor...');
            }
            return;
        }

        const { page, size, total } = pagination;
        const startRecord = (page - 1) * size + 1;
        const endRecord = startRecord + this.table.data.length - 1;

        let totalText;
        if (total.relation === 'gte') {
            totalText = `Toplam ${this.dashboard.formatNumber(total.value)}+ sonuç arasından`;
        } else {
            totalText = `Toplam ${this.dashboard.formatNumber(total.value)} kayıttan`;
        }

        infoElement.text(
            `${totalText} ${this.dashboard.formatNumber(startRecord)}-${this.dashboard.formatNumber(endRecord)} arası gösteriliyor.`
        );
    }

    /**
     * Sayfalama kontrollerini günceller
     */
    updatePaginationControls(pagination) {
        if (!pagination) return;

        const { page, hasNextPage } = pagination;
        const prevBtn = $(`#${this.config.idPrefix}-prev`);
        const nextBtn = $(`#${this.config.idPrefix}-next`);
        const pageInfo = $(`#${this.config.idPrefix}-page-info`);

        if (prevBtn.length) prevBtn.prop('disabled', page <= 1);
        if (nextBtn.length) nextBtn.prop('disabled', !hasNextPage);
        if (pageInfo.length) pageInfo.text(`Sayfa ${this.dashboard.formatNumber(page)}`);
    }

    /**
     * Tablo yükleniyor durumunu gösterir
     */
    showTableLoading(show) {
        const tbody = $(`#${this.config.idPrefix}-table-body`);
        if (!tbody.length) return;

        if (show) {
            const colspan = this.config.columns.length;
            tbody.html(`
                <tr>
                    <td colspan="${colspan}" class="text-center py-4">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Yükleniyor...</span>
                        </div>
                        <div class="mt-2">Veriler yükleniyor...</div>
                    </td>
                </tr>
            `);
        }
    }

    /**
     * Boş tablo mesajı gösterir
     */
    showTableEmpty() {
        const tbody = $(`#${this.config.idPrefix}-table-body`);
        if (!tbody.length) return;

        const colspan = this.config.columns.length;
        tbody.html(`
            <tr>
                <td colspan="${colspan}" class="text-center py-4">
                    <div class="text-muted">
                        <i class="fas fa-search fa-2x mb-2"></i>
                        <div>Filtreyle eşleşen veri bulunamadı.</div>
                    </div>
                </td>
            </tr>
        `);
    }

    /**
     * Tablo hata mesajı gösterir
     */
    showTableError(message) {
        const tbody = $(`#${this.config.idPrefix}-table-body`);
        if (!tbody.length) return;

        const colspan = this.config.columns.length;
        tbody.html(`
            <tr>
                <td colspan="${colspan}" class="text-center py-4">
                    <div class="text-danger">
                        <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                        <div>Hata: ${message}</div>
                    </div>
                </td>
            </tr>
        `);
    }
}