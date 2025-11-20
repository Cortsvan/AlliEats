// ===================================
// CART.JS - Complete Working Version
// ===================================

let currentCartItemId = null;
let removeModalInstance = null;
let clearModalInstance = null;

// ===================================
// MODAL FUNCTIONS
// ===================================

function confirmRemove(cartItemId, itemName) {
    console.log('confirmRemove called:', cartItemId, itemName);
    currentCartItemId = cartItemId;
    const nameElement = document.getElementById('removeItemName');
    if (nameElement) {
        nameElement.textContent = itemName;
    }
    if (removeModalInstance) {
        removeModalInstance.show();
    }
}

function confirmClearCart() {
    console.log('confirmClearCart called');
    if (clearModalInstance) {
        clearModalInstance.show();
    }
}

// ===================================
// HELPER FUNCTIONS
// ===================================

function processResponse(response) {
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.indexOf("application/json") !== -1) {
        return response.json();
    }
    return { success: true, message: "Operation completed successfully." };
}

function showToast(message, type = 'success') {
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    } else {
        console.log(`[${type.toUpperCase()}] ${message}`);
    }
}

// ===================================
// STOCK MANAGEMENT
// ===================================

function checkStockAvailability() {
    const checkButton = document.getElementById('checkStockBtn');
    if (!checkButton) return;
    
    const originalText = checkButton.innerHTML;
    checkButton.disabled = true;
    checkButton.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Checking...';

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch('/Cart/CheckStockAvailability', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token,
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        checkButton.disabled = false;
        checkButton.innerHTML = originalText;

        if (data.success) {
            updateStockIndicators(data.stockIssues || []);
            if (data.hasIssues) {
                showStockIssuesAlert(data.stockIssues);
                disableCheckoutButton();
            } else {
                hideStockIssuesAlert();
                enableCheckoutButton();
                showToast('All items are available!', 'success');
            }
        } else {
            showToast(data.message || 'Failed to check stock', 'error');
        }
    })
    .catch(error => {
        checkButton.disabled = false;
        checkButton.innerHTML = originalText;
        showToast('Error checking stock', 'error');
        console.error('Stock check error:', error);
    });
}

function updateStockIndicators(stockIssues) {
    // Reset all indicators
    document.querySelectorAll('.cart-item-row').forEach(row => {
        const stockIndicator = row.querySelector('.stock-indicator');
        if (stockIndicator) {
            const stockText = stockIndicator.querySelector('.stock-text');
            stockIndicator.classList.add('d-none');
            row.classList.remove('stock-issue');
            if (stockText) stockText.textContent = '';
        }
    });

    // Update items with issues
    stockIssues.forEach(issue => {
        const row = document.querySelector(`[data-item-id="${issue.cartItemId}"]`);
        if (row) {
            const stockIndicator = row.querySelector('.stock-indicator');
            if (stockIndicator) {
                const stockText = stockIndicator.querySelector('.stock-text');
                row.classList.add('stock-issue');
                stockIndicator.classList.remove('d-none');

                if (issue.issue === 'unavailable') {
                    stockText.innerHTML = '<span class="text-danger fw-bold">Unavailable</span>';
                } else if (issue.issue === 'out-of-stock') {
                    stockText.innerHTML = '<span class="text-danger fw-bold">Out of stock</span>';
                } else if (issue.issue === 'insufficient') {
                    stockText.innerHTML = `<span class="text-warning fw-bold">Only ${issue.availableStock} available</span>`;
                }
            }
        }
    });
}

function showStockIssuesAlert(stockIssues) {
    const alert = document.getElementById('stockIssuesAlert');
    const content = document.getElementById('stockIssuesContent');
    if (!alert || !content) return;

    let html = '<ul class="mb-2 ps-3">';
    stockIssues.forEach(issue => {
        html += `<li class="small">${issue.message}</li>`;
    });
    html += '</ul>';
    html += '<p class="mb-0 small"><strong>Action:</strong> Adjust quantities or remove unavailable items.</p>';

    content.innerHTML = html;
    alert.classList.remove('d-none');
}

function hideStockIssuesAlert() {
    const alert = document.getElementById('stockIssuesAlert');
    if (alert) alert.classList.add('d-none');
}

function disableCheckoutButton() {
    const checkoutBtn = document.getElementById('checkoutBtn');
    if (checkoutBtn) {
        checkoutBtn.classList.add('disabled', 'btn-secondary');
        checkoutBtn.classList.remove('btn-primary');
        checkoutBtn.style.pointerEvents = 'none';
        checkoutBtn.innerHTML = '<i class="fas fa-exclamation-triangle me-2"></i>Resolve Stock Issues';
    }
}

function enableCheckoutButton() {
    const checkoutBtn = document.getElementById('checkoutBtn');
    if (checkoutBtn) {
        checkoutBtn.classList.remove('disabled', 'btn-secondary');
        checkoutBtn.classList.add('btn-primary');
        checkoutBtn.style.pointerEvents = 'auto';
        checkoutBtn.innerHTML = 'Proceed to Checkout <i class="fas fa-chevron-right ms-2"></i>';
    }
}

function autoFixStockIssues() {
    const autoFixBtn = document.getElementById('autoFixStockBtn');
    if (!autoFixBtn) return;
    
    const originalText = autoFixBtn.innerHTML;
    autoFixBtn.disabled = true;
    autoFixBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Fixing...';

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch('/Cart/AutoFixStockIssues', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token,
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast('Stock issues fixed!', 'success');
            setTimeout(() => location.reload(), 1500);
        } else {
            autoFixBtn.disabled = false;
            autoFixBtn.innerHTML = originalText;
            showToast(data.message || 'Failed to fix issues', 'error');
        }
    })
    .catch(error => {
        autoFixBtn.disabled = false;
        autoFixBtn.innerHTML = originalText;
        showToast('Error fixing stock issues', 'error');
        console.error('Auto-fix error:', error);
    });
}

// ===================================
// FORM HANDLERS
// ===================================

function handleRemoveItem(e) {
    e.preventDefault();
    const form = e.target;
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const button = form.querySelector('button[type="submit"]');
    const actionUrl = form.getAttribute('action');
    
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

    fetch(actionUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token,
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({ 'cartItemId': currentCartItemId })
    })
    .then(response => {
        if (!response.ok) throw new Error('Network error');
        return processResponse(response);
    })
    .then(data => {
        if (data.success) {
            showToast('Item removed successfully', 'success');
            if (removeModalInstance) removeModalInstance.hide();
            setTimeout(() => location.reload(), 500);
        } else {
            throw new Error(data.message || 'Failed to remove item');
        }
    })
    .catch(error => {
        showToast(error.message || 'Error removing item', 'error');
        button.disabled = false;
        button.innerHTML = 'Remove';
    });
}

function handleClearCart(e) {
    e.preventDefault();
    const form = e.target;
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const button = form.querySelector('button[type="submit"]');
    const actionUrl = form.getAttribute('action');

    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

    fetch(actionUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token,
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams()
    })
    .then(response => {
        if (!response.ok) throw new Error('Network error');
        return processResponse(response);
    })
    .then(data => {
        if (data.success) {
            showToast('Cart cleared successfully', 'success');
            if (clearModalInstance) clearModalInstance.hide();
            setTimeout(() => location.reload(), 500);
        } else {
            throw new Error(data.message || 'Failed to clear cart');
        }
    })
    .catch(error => {
        showToast(error.message || 'Error clearing cart', 'error');
        button.disabled = false;
        button.innerHTML = 'Yes, Clear It';
    });
}

// ===================================
// QUANTITY STEPPER
// ===================================

function setupQuantityStepper() {
    console.log('=== Setting up quantity steppers ===');
    
    const stepperButtons = document.querySelectorAll('.stepper-btn');
    console.log(`Found ${stepperButtons.length} stepper buttons`);
    
    stepperButtons.forEach((button, index) => {
        // Remove any existing listeners
        const newButton = button.cloneNode(true);
        button.parentNode.replaceChild(newButton, button);
        
        newButton.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            console.log(`Button ${index + 1} clicked`);
            
            const action = this.getAttribute('data-action');
            const form = this.closest('form');
            const input = form.querySelector('.quantity-input');
            
            if (!input) {
                console.error('No input found');
                return;
            }
            
            let currentValue = parseInt(input.value) || 1;
            const min = parseInt(input.getAttribute('min')) || 1;
            const max = parseInt(input.getAttribute('max')) || 50;

            console.log(`Action: ${action}, Current: ${currentValue}, Min: ${min}, Max: ${max}`);

            let newValue = currentValue;
            
            if (action === 'increase' && currentValue < max) {
                newValue = currentValue + 1;
            } else if (action === 'decrease' && currentValue > min) {
                newValue = currentValue - 1;
            }

            if (newValue !== currentValue) {
                console.log(`Changing from ${currentValue} to ${newValue}`);
                input.value = newValue;
                
                // Add loading state
                const row = this.closest('.cart-item-row');
                if (row) {
                    row.classList.add('is-loading');
                }
                
                // Submit the form
                console.log('Submitting form...');
                form.submit();
            } else {
                console.log('Value unchanged - limit reached');
            }
        });
    });

    // Handle direct input changes
    document.querySelectorAll('.quantity-input').forEach(input => {
        input.addEventListener('change', function() {
            const min = parseInt(this.getAttribute('min')) || 1;
            const max = parseInt(this.getAttribute('max')) || 50;
            let value = parseInt(this.value) || 1;

            if (value < min) value = min;
            if (value > max) value = max;
            
            this.value = value;

            const row = this.closest('.cart-item-row');
            if (row) row.classList.add('is-loading');

            this.closest('form').submit();
        });

        // Prevent invalid input
        input.addEventListener('keypress', function(e) {
            if (e.key === 'e' || e.key === '-' || e.key === '+' || e.key === '.') {
                e.preventDefault();
            }
        });
    });
    
    console.log('=== Stepper setup complete ===');
}

// ===================================
// INITIALIZATION
// ===================================

document.addEventListener('DOMContentLoaded', function() {
    console.log('🛒 Cart page loading...');

    // Initialize modals
    const removeModalEl = document.getElementById('removeModal');
    if (removeModalEl) {
        removeModalInstance = new bootstrap.Modal(removeModalEl);
        console.log('✓ Remove modal initialized');
    }

    const clearModalEl = document.getElementById('clearModal');
    if (clearModalEl) {
        clearModalInstance = new bootstrap.Modal(clearModalEl);
        console.log('✓ Clear modal initialized');
    }

    // Setup quantity steppers
    setupQuantityStepper();

    // Auto-check stock on load
    const cartItems = document.querySelectorAll('.cart-item-row');
    if (cartItems.length > 0) {
        console.log(`✓ Found ${cartItems.length} cart items`);
        setTimeout(() => {
            console.log('Running auto stock check...');
            checkStockAvailability();
        }, 500);
    }

    // Setup event listeners
    const checkStockBtn = document.getElementById('checkStockBtn');
    if (checkStockBtn) {
        checkStockBtn.addEventListener('click', checkStockAvailability);
        console.log('✓ Check stock button ready');
    }

    const autoFixStockBtn = document.getElementById('autoFixStockBtn');
    if (autoFixStockBtn) {
        autoFixStockBtn.addEventListener('click', autoFixStockIssues);
        console.log('✓ Auto-fix button ready');
    }

    const checkoutBtn = document.getElementById('checkoutBtn');
    if (checkoutBtn) {
        checkoutBtn.addEventListener('click', function(e) {
            if (this.classList.contains('disabled')) {
                e.preventDefault();
                showToast('Please resolve stock issues first', 'warning');
                return false;
            }
        });
        console.log('✓ Checkout button ready');
    }

    const removeForm = document.getElementById('removeForm');
    if (removeForm) {
        removeForm.addEventListener('submit', handleRemoveItem);
        console.log('✓ Remove form ready');
    }

    const clearForm = document.getElementById('clearForm');
    if (clearForm) {
        clearForm.addEventListener('submit', handleClearCart);
        console.log('✓ Clear form ready');
    }

    console.log('🎉 Cart initialization complete!');
});

// Make functions globally available
window.confirmRemove = confirmRemove;
window.confirmClearCart = confirmClearCart;