/**
 * Filter Manager - Dashboard filtre işlemlerini yönetir
 */
class FilterManager {
    constructor(dashboard) {
        this.dashboard = dashboard;
        this.autocompleteElements = {};
        this.debounceTimers = {};
    }

    /**
     * Filter manager'ı başlatır
     */
    init() {
        this.setupAutocomplete();
        this.setupLocationFilters();
    }

    /**
     * Autocomplete işlevselliğini kurar
     */
    setupAutocomplete() {
        const autocompleteFields = [
            {
                id: 'musteri-no',
                field: 'properties.MusteriNo',
                index: 'context-profile'
            },
            {
                id: 'kullanici-kodu',
                field: 'properties.kullaniciAdi',
                index: 'context-profile'
            },
            {
                id: 'malzeme-no',
                field: 'properties.MalzemeNo',
                index: 'context-sepet'
            }
        ];

        autocompleteFields.forEach(config => {
            this.initializeAutocompleteForField(config);
        });
    }

    /**
     * Lokasyon filtrelerini kurar
     */
    setupLocationFilters() {
        $('#il-filter').on('change', (e) => {
            this.populateIlceDropdown($(e.target).val());
        });
    }

    /**
     * İlçe dropdown'ını doldurur
     */
    async populateIlceDropdown(provinceCode) {
        const ilceDropdown = $('#ilce-filter');
        if (!ilceDropdown.length) return;

        // Dropdown'ı temizle ve devre dışı bırak
        ilceDropdown.html('<option value="">İlçe Seçiniz</option>').prop('disabled', true);

        if (!provinceCode) {
            return;
        }

        try {
            // İlçeleri API'den al
            const response = await $.ajax({
                url: `${this.dashboard.apiBaseUrl}/districts/${encodeURIComponent(provinceCode)}`,
                method: 'GET',
                headers: {
                    'X-CSRF-TOKEN': window.csrfToken
                }
            });

            if (response.success && response.data && response.data.length > 0) {
                // İlçeleri alfabetik sırala
                const sortedDistricts = response.data.sort((a, b) =>
                    a.name.localeCompare(b.name, 'tr'));

                sortedDistricts.forEach(district => {
                    const option = $('<option></option>')
                        .val(district.code)
                        .text(district.name);
                    ilceDropdown.append(option);
                });

                ilceDropdown.prop('disabled', false);
            }
        } catch (error) {
            console.error('Error loading districts:', error);
            this.dashboard.showNotification('İlçeler yüklenirken hata oluştu', 'error');
        }
    }

    /**
     * İlçe dropdown'ını sıfırlar
     */
    resetIlceDropdown() {
        const ilceDropdown = $('#ilce-filter');
        if (ilceDropdown.length) {
            ilceDropdown.html('<option value="">İlçe Seçiniz</option>').prop('disabled', true);
        }
    }

    /**
     * Belirli bir alan için autocomplete başlatır
     */
    initializeAutocompleteForField(config) {
        const inputElement = $(`#${config.id}`);
        if (!inputElement.length) return;

        // Suggestions container oluştur
        const suggestionsContainer = $('<div class="autocomplete-suggestions"></div>');
        inputElement.parent().append(suggestionsContainer);
        this.autocompleteElements[config.id] = suggestionsContainer;

        // Event listener'ları ekle
        inputElement.on('input', (e) => {
            this.handleAutocompleteInput(e, config);
        });

        inputElement.on('blur', () => {
            setTimeout(() => this.hideAutocompleteSuggestions(config.id), 200);
        });

        inputElement.on('keydown', (e) => {
            this.handleAutocompleteKeyNavigation(e, config.id);
        });

        // Focus olduğunda önerileri göster
        inputElement.on('focus', (e) => {
            if ($(e.target).val().trim().length >= 2) {
                this.handleAutocompleteInput(e, config);
            }
        });
    }

    /**
     * Autocomplete input'unu işler
     */
    handleAutocompleteInput(event, config) {
        const queryText = $(event.target).val().trim();

        if (queryText.length < 2) {
            this.hideAutocompleteSuggestions(config.id);
            return;
        }

        // Debounce timer'ı temizle
        clearTimeout(this.debounceTimers[config.id]);

        // Yeni timer başlat
        this.debounceTimers[config.id] = setTimeout(() => {
            this.fetchAndDisplayAutocompleteSuggestions(queryText, config);
        }, 300);
    }

    /**
     * Autocomplete önerilerini getirir ve gösterir
     */
    /**
 * Autocomplete önerilerini getirir ve gösterir - IsSuccess fix
 */
    async fetchAndDisplayAutocompleteSuggestions(queryText, config) {
        try {
            const requestData = {
                query: queryText,
                field: config.field,
                index: config.index,
                size: 10
            };

            const response = await $.ajax({
                url: `${this.dashboard.apiBaseUrl}/autocomplete`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(requestData),
                headers: {
                    'X-CSRF-TOKEN': window.csrfToken
                }
            });

            console.log('📥 Autocomplete response:', response);

            // ✅ IsSuccess property'sini kontrol et
            const isSuccessful = response?.isSuccess || response?.IsSuccess || response?.success;

            if (response && isSuccessful && response.data) {
                this.displayAutocompleteSuggestions(response.data, config.id);
            } else {
                console.warn('❌ Autocomplete failed:', response?.message);
                this.hideAutocompleteSuggestions(config.id);
            }
        } catch (error) {
            console.error('Autocomplete error:', error);
            this.hideAutocompleteSuggestions(config.id);
        }
    }

    /**
     * İlçe dropdown'ını doldurur - IsSuccess fix
     */
    async populateIlceDropdown(provinceCode) {
        const ilceDropdown = $('#ilce-filter');
        if (!ilceDropdown.length) return;

        // Dropdown'ı temizle ve devre dışı bırak
        ilceDropdown.html('<option value="">İlçe Seçiniz</option>').prop('disabled', true);

        if (!provinceCode) {
            return;
        }

        try {
            // İlçeleri API'den al
            const response = await $.ajax({
                url: `${this.dashboard.apiBaseUrl}/districts/${encodeURIComponent(provinceCode)}`,
                method: 'GET',
                headers: {
                    'X-CSRF-TOKEN': window.csrfToken
                }
            });

            console.log('📥 Districts response:', response);

            // ✅ IsSuccess property'sini kontrol et
            const isSuccessful = response?.isSuccess || response?.IsSuccess || response?.success;

            if (response && isSuccessful && response.data && response.data.length > 0) {
                // İlçeleri alfabetik sırala
                const sortedDistricts = response.data.sort((a, b) =>
                    a.name.localeCompare(b.name, 'tr'));

                sortedDistricts.forEach(district => {
                    const option = $('<option></option>')
                        .val(district.code)
                        .text(district.name);
                    ilceDropdown.append(option);
                });

                ilceDropdown.prop('disabled', false);
            } else {
                console.warn('❌ Districts load failed:', response?.message);
            }
        } catch (error) {
            console.error('Error loading districts:', error);
            this.dashboard.showNotification('İlçeler yüklenirken hata oluştu', 'error');
        }
    }

    /**
     * Autocomplete önerilerini görüntüler
     */
    displayAutocompleteSuggestions(suggestions, inputId) {
        const suggestionsContainer = this.autocompleteElements[inputId];
        const inputElement = $(`#${inputId}`);

        if (!suggestionsContainer || !inputElement.length || !suggestions || suggestions.length === 0) {
            this.hideAutocompleteSuggestions(inputId);
            return;
        }

        // Container'ı temizle
        suggestionsContainer.empty();

        suggestions.forEach(suggestion => {
            const suggestionElement = $(`
                <div class="autocomplete-suggestion" data-value="${suggestion.text}">
                    <strong>${suggestion.text}</strong>
                    <span class="text-muted ms-2">(${suggestion.count})</span>
                </div>
            `);

            suggestionElement.on('mousedown', (e) => {
                e.preventDefault(); // Blur event'ını engelle
                inputElement.val($(e.currentTarget).data('value'));
                this.hideAutocompleteSuggestions(inputId);
                inputElement.focus();
            });

            suggestionsContainer.append(suggestionElement);
        });

        this.showAutocompleteSuggestions(inputId);
    }

    /**
     * Autocomplete önerilerini gösterir
     */
    showAutocompleteSuggestions(inputId) {
        const container = this.autocompleteElements[inputId];
        if (container) {
            container.show();
        }
    }

    /**
     * Autocomplete önerilerini gizler
     */
    hideAutocompleteSuggestions(inputId) {
        const container = this.autocompleteElements[inputId];
        if (container) {
            container.hide();
        }
    }

    /**
     * Autocomplete keyboard navigation'ını işler
     */
    handleAutocompleteKeyNavigation(event, inputId) {
        const container = this.autocompleteElements[inputId];
        if (!container || !container.is(':visible')) return;

        const items = container.find('.autocomplete-suggestion');
        if (items.length === 0) return;

        let activeItem = container.find('.autocomplete-suggestion.active');
        let nextItem;

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                nextItem = activeItem.length ?
                    (activeItem.next('.autocomplete-suggestion').length ?
                        activeItem.next('.autocomplete-suggestion') : items.first()) :
                    items.first();
                break;

            case 'ArrowUp':
                event.preventDefault();
                nextItem = activeItem.length ?
                    (activeItem.prev('.autocomplete-suggestion').length ?
                        activeItem.prev('.autocomplete-suggestion') : items.last()) :
                    items.last();
                break;

            case 'Enter':
                event.preventDefault();
                if (activeItem.length) {
                    $(`#${inputId}`).val(activeItem.data('value'));
                    this.hideAutocompleteSuggestions(inputId);
                }
                return;

            case 'Escape':
                this.hideAutocompleteSuggestions(inputId);
                return;

            default:
                return;
        }

        if (nextItem && nextItem.length) {
            // Aktif class'ı güncelle
            items.removeClass('active');
            nextItem.addClass('active');

            // Scroll into view
            const containerHeight = container.height();
            const itemTop = nextItem.position().top;
            const itemHeight = nextItem.outerHeight();

            if (itemTop < 0) {
                container.scrollTop(container.scrollTop() + itemTop);
            } else if (itemTop + itemHeight > containerHeight) {
                container.scrollTop(container.scrollTop() + itemTop + itemHeight - containerHeight);
            }
        }
    }

    /**
     * Tüm autocomplete'leri temizler
     */
    clearAllAutocomplete() {
        Object.keys(this.autocompleteElements).forEach(inputId => {
            $(`#${inputId}`).val('');
            this.hideAutocompleteSuggestions(inputId);
        });
    }

    /**
     * Cleanup - filter manager'ı temizler
     */
    destroy() {
        // Debounce timer'larını temizle
        Object.values(this.debounceTimers).forEach(timer => {
            clearTimeout(timer);
        });
        this.debounceTimers = {};

        // Event listener'ları kaldır
        $('#il-filter').off('change');
        Object.keys(this.autocompleteElements).forEach(inputId => {
            $(`#${inputId}`).off('input blur keydown focus');
        });

        // Autocomplete container'larını kaldır
        Object.values(this.autocompleteElements).forEach(container => {
            container.remove();
        });
        this.autocompleteElements = {};
    }
}