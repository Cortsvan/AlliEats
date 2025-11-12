// Manage Orders functionality

document.addEventListener('DOMContentLoaded', function () {
    initializeSearchAndFilter();
    initializeTooltips();
    initializeRefresh();
});

function initializeSearchAndFilter() {
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const dateFilter = document.getElementById('dateFilter');
    const paymentFilter = document.getElementById('paymentFilter');
    const amountFilter = document.getElementById('amountFilter');
    const clearSearch = document.getElementById('clearSearch');
    const clearFilters = document.getElementById('clearFilters');

    // Search functionality
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const hasValue = this.value.trim().length > 0;
            clearSearch.style.display = hasValue ? 'block' : 'none';
            filterOrders();
        });
    }

    // Filter events
    [statusFilter, dateFilter, paymentFilter, amountFilter].forEach(filter => {
        if (filter) {
            filter.addEventListener('change', filterOrders);
        }
    });

    // Clear search
    if (clearSearch) {
        clearSearch.addEventListener('click', function () {
            searchInput.value = '';
            this.style.display = 'none';
            filterOrders();
        });
    }

    // Clear all filters
    if (clearFilters) {
        clearFilters.addEventListener('click', function () {
            searchInput.value = '';
            statusFilter.value = 'all';
            dateFilter.value = 'all';
            paymentFilter.value = 'all';
            amountFilter.value = 'all';
            clearSearch.style.display = 'none';
            filterOrders();
        });
    }
}

function filterOrders() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
    const statusFilter = document.getElementById('statusFilter').value;
    const dateFilter = document.getElementById('dateFilter').value;
    const paymentFilter = document.getElementById('paymentFilter').value;
    const amountFilter = document.getElementById('amountFilter').value;

    const rows = document.querySelectorAll('.order-row');
    const filterInfo = document.getElementById('filterInfo');
    const noResults = document.getElementById('noResults');
    const table = document.querySelector('.table-responsive');

    let visibleCount = 0;
    let hasActiveFilters = searchTerm || statusFilter !== 'all' || dateFilter !== 'all' ||
        paymentFilter !== 'all' || amountFilter !== 'all';

    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    const weekAgo = new Date(today);
    weekAgo.setDate(weekAgo.getDate() - 7);
    const monthAgo = new Date(today);
    monthAgo.setMonth(monthAgo.getMonth() - 1);

    rows.forEach(row => {
        const orderNumber = row.getAttribute('data-order-number') || '';
        const customer = row.getAttribute('data-customer') || '';
        const status = row.getAttribute('data-status');
        const payment = row.getAttribute('data-payment');
        const amount = parseFloat(row.getAttribute('data-amount')) || 0;
        const orderDate = new Date(row.getAttribute('data-date'));

        // Search match
        const searchMatch = !searchTerm ||
            orderNumber.includes(searchTerm) ||
            customer.includes(searchTerm);

        // Status match
        const statusMatch = statusFilter === 'all' || status === statusFilter;

        // Payment match
        const paymentMatch = paymentFilter === 'all' || payment === paymentFilter;

        // Amount match
        let amountMatch = true;
        if (amountFilter !== 'all') {
            const [min, max] = amountFilter.split('-');
            if (max === '+') {
                amountMatch = amount >= parseInt(min);
            } else {
                amountMatch = amount >= parseInt(min) && amount <= parseInt(max);
            }
        }

        // Date match
        let dateMatch = true;
        if (dateFilter !== 'all') {
            const orderDateString = orderDate.toDateString();
            switch (dateFilter) {
                case 'today':
                    dateMatch = orderDateString === today.toDateString();
                    break;
                case 'yesterday':
                    dateMatch = orderDateString === yesterday.toDateString();
                    break;
                case 'week':
                    dateMatch = orderDate >= weekAgo;
                    break;
                case 'month':
                    dateMatch = orderDate >= monthAgo;
                    break;
            }
        }

        // Show/hide row
        if (searchMatch && statusMatch && paymentMatch && amountMatch && dateMatch) {
            row.classList.remove('hidden');
            visibleCount++;
        } else {
            row.classList.add('hidden');
        }
    });

    // Update counters
    document.getElementById('orderCount').textContent = visibleCount;
    document.getElementById('visibleCount').textContent = visibleCount;

    // Show/hide filter info
    filterInfo.style.display = hasActiveFilters ? 'block' : 'none';

    // Show/hide no results
    if (visibleCount === 0 && hasActiveFilters) {
        noResults.style.display = 'block';
        table.style.display = 'none';
    } else {
        noResults.style.display = 'none';
        table.style.display = 'block';
    }
}

function initializeTooltips() {
    const tooltipElements = document.querySelectorAll('[title]');
    tooltipElements.forEach(element => {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            new bootstrap.Tooltip(element);
        }
    });
}

function initializeRefresh() {
    // Optional: Add refresh button functionality
    // Can be extended to show last refresh time
}

function showUpdateStatusModal(orderId, orderNumber, currentStatus) {
    document.getElementById('orderId').value = orderId;
    document.getElementById('orderNumber').value = '#' + orderNumber;
    document.getElementById('currentStatus').value = currentStatus;

    // Clear the status dropdown
    const statusSelect = document.getElementById('status');
    statusSelect.value = '';

    // Populate status options based on current status and admin restrictions
    populateAdminStatusOptions(currentStatus);

    const modal = new bootstrap.Modal(document.getElementById('updateStatusModal'));
    modal.show();
}

function populateAdminStatusOptions(currentStatus) {
    const statusSelect = document.getElementById('status');

    // Clear existing options except the first one
    while (statusSelect.children.length > 1) {
        statusSelect.removeChild(statusSelect.lastChild);
    }

    // Define admin-allowed status transitions
    const allowedTransitions = getAdminAllowedTransitions(currentStatus);

    // Add allowed status options
    allowedTransitions.forEach(status => {
        const option = document.createElement('option');
        option.value = status.value;
        option.textContent = status.label;
        statusSelect.appendChild(option);
    });
}

function getAdminAllowedTransitions(currentStatus) {
    // Admin cannot set status to "Received" (only users can confirm receipt)
    // Admin cannot cancel orders (only users can cancel before confirmation)

    const allStatuses = [
        { value: 'Pending', label: 'Pending' },
        { value: 'Confirmed', label: 'Confirmed' },
        { value: 'Preparing', label: 'Preparing' },
        { value: 'Ready', label: 'Ready' },
        { value: 'On the Way', label: 'On the Way' }
        // Note: "Received" and "Cancelled" are excluded for admin
    ];

    switch (currentStatus) {
        case 'Pending':
            return [
                { value: 'Confirmed', label: 'Confirmed' },
                // Admin can move pending orders directly to confirmed
            ];

        case 'Confirmed':
            return [
                { value: 'Preparing', label: 'Preparing' },
                // Can go back to pending if needed
                { value: 'Pending', label: 'Pending' }
            ];

        case 'Preparing':
            return [
                { value: 'Ready', label: 'Ready' },
                // Can go back to confirmed if needed
                { value: 'Confirmed', label: 'Confirmed' }
            ];

        case 'Ready':
            return [
                { value: 'On the Way', label: 'On the Way' },
                // Can go back to preparing if needed
                { value: 'Preparing', label: 'Preparing' }
            ];

        case 'On the Way':
            return [
                // Can go back to ready if needed (e.g., delivery issue)
                { value: 'Ready', label: 'Ready' }
                // Cannot set to "Received" - only customer can confirm receipt
            ];

        case 'Received':
            return [
                // No transitions allowed from received status
            ];

        case 'Cancelled':
            return [
                // No transitions allowed from cancelled status
            ];

        default:
            return allStatuses.filter(s => s.value !== 'Received' && s.value !== 'Cancelled');
    }
}

function refreshOrders() {
    // Show loading state
    const refreshBtn = document.querySelector('[onclick="refreshOrders()"]');
    const originalContent = refreshBtn.innerHTML;

    refreshBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Refreshing...';
    refreshBtn.disabled = true;

    // Refresh page
    setTimeout(() => {
        location.reload();
    }, 1000);
}

// Simple notification function
function showNotification(message, type = 'success') {
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    }
}