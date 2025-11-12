// Simple admin menu functionality with search and filter

document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips for action buttons
    initializeTooltips();
    // Initialize search and filter
    initializeSearchAndFilter();
});

function initializeTooltips() {
    // Simple tooltip initialization
    const tooltipElements = document.querySelectorAll('[title]');
    tooltipElements.forEach(element => {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            new bootstrap.Tooltip(element);
        }
    });
}

function initializeSearchAndFilter() {
    const searchInput = document.getElementById('searchInput');
    const categoryFilter = document.getElementById('categoryFilter');
    const statusFilter = document.getElementById('statusFilter');
    const clearSearch = document.getElementById('clearSearch');
    const clearFilters = document.getElementById('clearFilters');

    // Search functionality
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const hasValue = this.value.trim().length > 0;
            clearSearch.style.display = hasValue ? 'block' : 'none';
            filterItems();
        });
    }

    // Category filter
    if (categoryFilter) {
        categoryFilter.addEventListener('change', filterItems);
    }

    // Status filter  
    if (statusFilter) {
        statusFilter.addEventListener('change', filterItems);
    }

    // Clear search
    if (clearSearch) {
        clearSearch.addEventListener('click', function () {
            searchInput.value = '';
            this.style.display = 'none';
            filterItems();
        });
    }

    // Clear all filters
    if (clearFilters) {
        clearFilters.addEventListener('click', function () {
            searchInput.value = '';
            categoryFilter.value = 'all';
            statusFilter.value = 'all';
            clearSearch.style.display = 'none';
            filterItems();
        });
    }
}

function filterItems() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    const categoryFilter = document.getElementById('categoryFilter').value;
    const statusFilter = document.getElementById('statusFilter').value;

    const rows = document.querySelectorAll('.menu-item-row');
    const filterInfo = document.getElementById('filterInfo');
    const noResults = document.getElementById('noResults');
    const table = document.querySelector('.table-responsive');

    let visibleCount = 0;
    let hasActiveFilters = searchTerm || categoryFilter !== 'all' || statusFilter !== 'all';

    rows.forEach(row => {
        const name = row.getAttribute('data-name') || '';
        const description = row.getAttribute('data-description') || '';
        const category = row.getAttribute('data-category');
        const status = row.getAttribute('data-status');

        // Check search match
        const searchMatch = !searchTerm ||
            name.includes(searchTerm) ||
            description.includes(searchTerm);

        // Check category match
        const categoryMatch = categoryFilter === 'all' || category === categoryFilter;

        // Check status match
        const statusMatch = statusFilter === 'all' || status === statusFilter;

        // Show/hide row
        if (searchMatch && categoryMatch && statusMatch) {
            row.classList.remove('hidden');
            visibleCount++;
        } else {
            row.classList.add('hidden');
        }
    });

    // Update counter
    document.getElementById('itemCount').textContent = visibleCount;
    document.getElementById('visibleCount').textContent = visibleCount;

    // Show/hide filter info
    filterInfo.style.display = hasActiveFilters ? 'block' : 'none';

    // Show/hide no results message
    if (visibleCount === 0 && hasActiveFilters) {
        noResults.style.display = 'block';
        table.style.display = 'none';
    } else {
        noResults.style.display = 'none';
        table.style.display = 'block';
    }
}

function confirmDelete(itemId, itemName) {
    // Simple delete confirmation
    document.getElementById('itemName').textContent = itemName;
    document.getElementById('deleteForm').action = '/AdminMenu/DeleteItem/' + itemId;

    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}

// Simple notification function
function showNotification(message, type = 'success') {
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    }
}