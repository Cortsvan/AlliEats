// Admin Order Details functionality

let currentQuickAction = {
    orderId: null,
    status: null,
    button: null
};

document.addEventListener('DOMContentLoaded', function () {
    initializeTooltips();
    initializeAnimations();
    initializeCountdown();
});

function initializeTooltips() {
    const tooltipElements = document.querySelectorAll('[title]');
    tooltipElements.forEach(element => {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            new bootstrap.Tooltip(element);
        }
    });
}

function initializeAnimations() {
    // Animate cards on load
    const cards = document.querySelectorAll('.order-summary-card, .order-items-card, .admin-actions-card');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';

        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);
    });

    // Animate order items
    const orderItems = document.querySelectorAll('.order-item-row');
    orderItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateX(-20px)';

        setTimeout(() => {
            item.style.transition = 'all 0.4s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateX(0)';
        }, 500 + (index * 50));
    });

    // Animate quick action buttons
    const actionButtons = document.querySelectorAll('.btn-quick-action');
    actionButtons.forEach((button, index) => {
        button.style.opacity = '0';
        button.style.transform = 'translateX(20px)';

        setTimeout(() => {
            button.style.transition = 'all 0.3s ease';
            button.style.opacity = '1';
            button.style.transform = 'translateX(0)';
        }, 800 + (index * 100));
    });
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

    switch (currentStatus) {
        case 'Pending':
            return [
                { value: 'Confirmed', label: 'Confirmed' }
            ];

        case 'Confirmed':
            return [
                { value: 'Preparing', label: 'Preparing' },
                { value: 'Pending', label: 'Pending' }
            ];

        case 'Preparing':
            return [
                { value: 'Ready', label: 'Ready' },
                { value: 'Confirmed', label: 'Confirmed' }
            ];

        case 'Ready':
            return [
                { value: 'On the Way', label: 'On the Way' },
                { value: 'Preparing', label: 'Preparing' }
            ];

        case 'On the Way':
            return [
                { value: 'Ready', label: 'Ready' }
                // Cannot set to "Received" - only customer can confirm receipt
            ];

        case 'Received':
        case 'Cancelled':
            return [
                // No transitions allowed from received or cancelled status
            ];

        default:
            return [
                { value: 'Pending', label: 'Pending' },
                { value: 'Confirmed', label: 'Confirmed' },
                { value: 'Preparing', label: 'Preparing' },
                { value: 'Ready', label: 'Ready' },
                { value: 'On the Way', label: 'On the Way' }
            ];
    }
}

function getStatusInfo(status) {
    const statusMap = {
        'Confirmed': {
            icon: 'fa-check-circle',
            color: '#17a2b8',
            bgColor: 'rgba(23, 162, 184, 0.1)',
            description: 'The order will be marked as confirmed and the kitchen will be notified.',
            action: 'Confirm this order'
        },
        'Preparing': {
            icon: 'fa-utensils',
            color: 'var(--primary-color)',
            bgColor: 'rgba(139, 94, 60, 0.1)',
            description: 'The order status will change to "Preparing" and the kitchen will start working on it.',
            action: 'Start preparing this order'
        },
        'Ready': {
            icon: 'fa-bell',
            color: '#28a745',
            bgColor: 'rgba(40, 167, 69, 0.1)',
            description: 'The order will be marked as ready for pickup or delivery.',
            action: 'Mark this order as ready'
        },
        'On the Way': {
            icon: 'fa-shipping-fast',
            color: '#fd7e14',
            bgColor: 'rgba(253, 126, 20, 0.1)',
            description: 'The order will be marked as out for delivery. Customer will have 2 hours to confirm receipt.',
            action: 'Send this order out for delivery'
        }
    };

    return statusMap[status] || {
        icon: 'fa-question-circle',
        color: '#6c757d',
        bgColor: 'rgba(108, 117, 125, 0.1)',
        description: 'Update the order status.',
        action: 'Update this order'
    };
}

function updateStatus(orderId, status) {
    const button = event.target.closest('button');
    const statusInfo = getStatusInfo(status);
    
    // Store current action details
    currentQuickAction = {
        orderId: orderId,
        status: status,
        button: button
    };

    // Populate modal with status information
    document.getElementById('quickActionOrderId').value = orderId;
    document.getElementById('quickActionStatus').value = status;
    
    const modalIcon = document.getElementById('quickActionIcon');
    const modalTitle = document.getElementById('quickActionTitle');
    const modalDescription = document.getElementById('quickActionDescription');
    const modalHeader = document.querySelector('#quickActionModal .modal-header');
    
    modalIcon.className = `fas ${statusInfo.icon}`;
    modalIcon.style.color = statusInfo.color;
    modalTitle.textContent = statusInfo.action;
    modalDescription.textContent = statusInfo.description;
    modalHeader.style.background = `linear-gradient(135deg, ${statusInfo.bgColor}, ${statusInfo.bgColor})`;
    modalHeader.style.borderBottom = `3px solid ${statusInfo.color}`;

    // Show the modal
    const modal = new bootstrap.Modal(document.getElementById('quickActionModal'));
    modal.show();
}

function confirmQuickAction() {
    const orderId = document.getElementById('quickActionOrderId').value;
    const status = document.getElementById('quickActionStatus').value;
    const button = currentQuickAction.button;
    const submitBtn = document.getElementById('confirmQuickActionBtn');

    // Show loading state on submit button
    const originalBtnContent = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
    submitBtn.disabled = true;

    // Show loading state on original button
    if (button) {
        button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Updating...';
        button.disabled = true;
    }

    // Create and submit form
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/AdminOrder/UpdateStatus';

    // Add CSRF token
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const tokenInput = document.createElement('input');
    tokenInput.type = 'hidden';
    tokenInput.name = '__RequestVerificationToken';
    tokenInput.value = token;
    form.appendChild(tokenInput);

    // Add order ID
    const orderIdInput = document.createElement('input');
    orderIdInput.type = 'hidden';
    orderIdInput.name = 'orderId';
    orderIdInput.value = orderId;
    form.appendChild(orderIdInput);

    // Add status
    const statusInput = document.createElement('input');
    statusInput.type = 'hidden';
    statusInput.name = 'status';
    statusInput.value = status;
    form.appendChild(statusInput);

    document.body.appendChild(form);
    form.submit();
}

function refreshPage() {
    // Show loading state
    const refreshBtn = document.querySelector('[onclick="refreshPage()"]');
    if (refreshBtn) {
        const originalContent = refreshBtn.innerHTML;
        refreshBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Refreshing...';
        refreshBtn.disabled = true;
    }

    // Refresh page
    setTimeout(() => {
        location.reload();
    }, 1000);
}

// Add ripple effect to buttons
function addRippleEffect(button, event) {
    const ripple = document.createElement('span');
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    ripple.style.width = ripple.style.height = size + 'px';
    ripple.style.left = x + 'px';
    ripple.style.top = y + 'px';
    ripple.classList.add('admin-ripple');

    button.style.position = 'relative';
    button.style.overflow = 'hidden';
    button.appendChild(ripple);

    setTimeout(() => {
        ripple.remove();
    }, 600);
}

// Add event listeners for ripple effects
document.addEventListener('click', function (event) {
    if (event.target.closest('.btn-quick-action') || event.target.closest('.btn-admin-primary')) {
        addRippleEffect(event.target.closest('button'), event);
    }
});

// Simple notification function
function showNotification(message, type = 'success') {
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    }
}

// Admin-specific analytics
function trackAdminAction(action, orderId) {
    console.log(`Admin Action: ${action} on Order ${orderId}`);
    // This can be extended to send analytics data
}

// Add admin ripple effect CSS
const adminRippleCSS = `
    .admin-ripple {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.6);
        transform: scale(0);
        animation: admin-ripple-animation 0.6s linear;
        pointer-events: none;
    }

    @keyframes admin-ripple-animation {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
`;

const style = document.createElement('style');
style.textContent = adminRippleCSS;
document.head.appendChild(style);

// Countdown timer for "On the Way" orders
function initializeCountdown() {
    const countdownDisplay = document.querySelector('.countdown-display');
    if (countdownDisplay) {
        const remainingSeconds = parseInt(countdownDisplay.getAttribute('data-remaining'));
        if (remainingSeconds > 0) {
            startCountdown(remainingSeconds);
        }
    }
}

function startCountdown(seconds) {
    const countdownElement = document.querySelector('.countdown-time');
    if (!countdownElement) return;

    const timer = setInterval(function () {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const secs = seconds % 60;

        if (seconds <= 0) {
            clearInterval(timer);
            // Show overdue message and refresh page
            const alert = document.querySelector('.timer-alert');
            alert.className = 'alert alert-warning timer-alert';
            alert.innerHTML = `
                <div class="d-flex align-items-center">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <div>
                        <strong>Auto-Receipt Timer</strong>
                        <div>⚠️ This order is now overdue and will be automatically marked as "Received" shortly.</div>
                        <button onclick="location.reload()" class="btn btn-sm btn-warning mt-2">
                            <i class="fas fa-sync-alt me-1"></i>Refresh Page
                        </button>
                    </div>
                </div>
            `;
            return;
        }

        if (hours > 0) {
            countdownElement.textContent = `${hours}h ${minutes}m`;
        } else if (minutes > 0) {
            countdownElement.textContent = `${minutes}m ${secs}s`;
        } else {
            countdownElement.textContent = `${secs}s`;
        }

        seconds--;
    }, 1000);
}

// Reset modal when closed
document.addEventListener('DOMContentLoaded', function() {
    const quickActionModal = document.getElementById('quickActionModal');
    if (quickActionModal) {
        quickActionModal.addEventListener('hidden.bs.modal', function () {
            const submitBtn = document.getElementById('confirmQuickActionBtn');
            if (submitBtn) {
                submitBtn.innerHTML = '<i class="fas fa-check me-2"></i>Yes, Update Status';
                submitBtn.disabled = false;
            }
            
            // Reset the stored action
            currentQuickAction = {
                orderId: null,
                status: null,
                button: null
            };
        });
    }
});