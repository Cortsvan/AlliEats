let currentCartItemId = null;
let removeModalInstance = null;
let clearModalInstance = null;

// Function to show the remove item confirmation modal
function confirmRemove(cartItemId, itemName) {
    currentCartItemId = cartItemId;
    document.getElementById('removeItemName').textContent = itemName;
    if (removeModalInstance) {
        removeModalInstance.show();
    }
}

// Function to show the clear cart confirmation modal
function confirmClearCart() {
    if (clearModalInstance) {
        clearModalInstance.show();
    }
}

// A helper function to process the fetch response
function processResponse(response) {
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.indexOf("application/json") !== -1) {
        return response.json();
    } else {
        // If not JSON, return a success object to proceed with UI updates,
        // assuming the action was successful if no server error occurred.
        return { success: true, message: "Operation completed successfully." };
    }
}

// Function to check stock availability
function checkStockAvailability() {
    const checkButton = document.getElementById('checkStockBtn');
    const originalText = checkButton.innerHTML;

    checkButton.disabled = true;
    checkButton.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Checking...';

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch('/Cart/CheckStockAvailability', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
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
                    toastr.success('All items are available!');
                }
            } else {
                toastr.error(data.message || 'Failed to check stock availability');
            }
        })
        .catch(error => {
            checkButton.disabled = false;
            checkButton.innerHTML = originalText;
            toastr.error('An error occurred while checking stock availability');
            console.error('Error:', error);
        });
}

// Function to update stock indicators
function updateStockIndicators(stockIssues) {
    // Reset all stock indicators first
    document.querySelectorAll('.cart-item-card').forEach(card => {
        const stockIndicator = card.querySelector('.stock-indicator');
        const stockText = stockIndicator.querySelector('.stock-text');

        stockIndicator.classList.add('d-none');
        card.classList.remove('stock-issue');
        stockText.textContent = '';
    });

    // Update items with stock issues
    stockIssues.forEach(issue => {
        const card = document.querySelector(`[data-item-id="${issue.cartItemId}"]`);
        if (card) {
            const stockIndicator = card.querySelector('.stock-indicator');
            const stockText = stockIndicator.querySelector('.stock-text');

            card.classList.add('stock-issue');
            stockIndicator.classList.remove('d-none');

            if (issue.issue === 'unavailable') {
                stockText.innerHTML = '<span class="text-danger">Unavailable</span>';
            } else if (issue.issue === 'out-of-stock') {
                stockText.innerHTML = '<span class="text-danger">Out of stock</span>';
            } else if (issue.issue === 'insufficient') {
                stockText.innerHTML = `<span class="text-warning">Only ${issue.availableStock} available</span>`;
            }
        }
    });
}

// Function to show stock issues alert
function showStockIssuesAlert(stockIssues) {
    const alert = document.getElementById('stockIssuesAlert');
    const content = document.getElementById('stockIssuesContent');

    let html = '<ul class="mb-2">';
    stockIssues.forEach(issue => {
        html += `<li>${issue.message}</li>`;
    });
    html += '</ul>';
    html += '<p class="mb-0 small"><strong>Action required:</strong> Please adjust quantities or remove unavailable items before proceeding to checkout.</p>';

    content.innerHTML = html;
    alert.classList.remove('d-none');
}

// Function to hide stock issues alert
function hideStockIssuesAlert() {
    const alert = document.getElementById('stockIssuesAlert');
    alert.classList.add('d-none');
}

// Function to disable checkout button
function disableCheckoutButton() {
    const checkoutBtn = document.getElementById('checkoutBtn');
    checkoutBtn.classList.add('disabled');
    checkoutBtn.style.pointerEvents = 'none';
    checkoutBtn.innerHTML = '<i class="fas fa-exclamation-triangle me-2"></i>Stock Issues - Cannot Checkout';
}

// Function to enable checkout button
function enableCheckoutButton() {
    const checkoutBtn = document.getElementById('checkoutBtn');
    checkoutBtn.classList.remove('disabled');
    checkoutBtn.style.pointerEvents = 'auto';
    checkoutBtn.innerHTML = '<i class="fas fa-credit-card me-2"></i>Proceed to Checkout';
}

// Function to auto-fix stock issues
function autoFixStockIssues() {
    const autoFixBtn = document.getElementById('autoFixStockBtn');
    const originalText = autoFixBtn.innerHTML;

    autoFixBtn.disabled = true;
    autoFixBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Fixing...';

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch('/Cart/AutoFixStockIssues', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                toastr.success(data.message);

                // Show what was fixed
                if (data.fixedItems && data.fixedItems.length > 0) {
                    let message = 'Quantities adjusted:\n';
                    data.fixedItems.forEach(item => {
                        message += `• ${item.name}: ${item.oldQuantity} → ${item.newQuantity}\n`;
                    });
                    toastr.info(message);
                }

                if (data.removedItems && data.removedItems.length > 0) {
                    let message = 'Items removed:\n';
                    data.removedItems.forEach(item => {
                        message += `• ${item.name} (${item.reason})\n`;
                    });
                    toastr.warning(message);
                }

                // Reload page to show updated cart
                setTimeout(() => {
                    location.reload();
                }, 2000);
            } else {
                autoFixBtn.disabled = false;
                autoFixBtn.innerHTML = originalText;
                toastr.error(data.message || 'Failed to fix stock issues');
            }
        })
        .catch(error => {
            autoFixBtn.disabled = false;
            autoFixBtn.innerHTML = originalText;
            toastr.error('An error occurred while fixing stock issues');
            console.error('Error:', error);
        });
}

document.addEventListener('DOMContentLoaded', function () {
    // Initialize modals once the DOM is loaded
    const removeModalEl = document.getElementById('removeModal');
    if (removeModalEl) {
        removeModalInstance = new bootstrap.Modal(removeModalEl);
    }
    const clearModalEl = document.getElementById('clearModal');
    if (clearModalEl) {
        clearModalInstance = new bootstrap.Modal(clearModalEl);
    }

    // Automatically check stock when page loads (if cart has items)
    const cartItems = document.querySelectorAll('.cart-item-card');
    if (cartItems.length > 0) {
        setTimeout(() => {
            checkStockAvailability();
        }, 500); // Small delay to ensure page is fully loaded
    }

    // Add event listener for manual stock check button
    const checkStockBtn = document.getElementById('checkStockBtn');
    if (checkStockBtn) {
        checkStockBtn.addEventListener('click', checkStockAvailability);
    }

    // Add event listener for auto-fix button
    const autoFixStockBtn = document.getElementById('autoFixStockBtn');
    if (autoFixStockBtn) {
        autoFixStockBtn.addEventListener('click', autoFixStockIssues);
    }

    // Prevent checkout if there are stock issues
    const checkoutBtn = document.getElementById('checkoutBtn');
    if (checkoutBtn) {
        checkoutBtn.addEventListener('click', function (e) {
            if (this.classList.contains('disabled')) {
                e.preventDefault();
                toastr.error('Please resolve stock issues before proceeding to checkout');
                return false;
            }
        });
    }

    // Handle remove item form submission
    const removeForm = document.getElementById('removeForm');
    if (removeForm) {
        removeForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const token = this.querySelector('input[name="__RequestVerificationToken"]').value;
            const button = this.querySelector('button[type="submit"]');
            const actionUrl = this.getAttribute('action');
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Removing...';

            fetch(actionUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams({
                    'cartItemId': currentCartItemId
                })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return processResponse(response);
                })
                .then(data => {
                    if (data.success) {
                        toastr.success(data.message || 'Item removed successfully.');
                        if (removeModalInstance) {
                            removeModalInstance.hide();
                        }
                        removeModalEl.addEventListener('hidden.bs.modal', function () {
                            location.reload();
                        }, { once: true });
                    } else {
                        toastr.error(data.message || 'Failed to remove item.');
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-trash me-2"></i>Remove';
                    }
                })
                .catch(() => {
                    toastr.error('An error occurred while removing the item.');
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-trash me-2"></i>Remove';
                });
        });
    }

    // Handle clear cart form submission
    const clearForm = document.getElementById('clearForm');
    if (clearForm) {
        clearForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const token = this.querySelector('input[name="__RequestVerificationToken"]').value;
            const button = this.querySelector('button[type="submit"]');
            const actionUrl = this.getAttribute('action');

            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Clearing...';

            fetch(actionUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams() // No body needed for clear
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return processResponse(response);
                })
                .then(data => {
                    if (data.success) {
                        toastr.success(data.message || 'Cart cleared successfully.');
                        if (clearModalInstance) {
                            clearModalInstance.hide();
                        }
                        clearModalEl.addEventListener('hidden.bs.modal', function () {
                            location.reload();
                        }, { once: true });
                    } else {
                        toastr.error(data.message || 'Failed to clear cart.');
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-trash me-2"></i>Clear Cart';
                    }
                })
                .catch(() => {
                    toastr.error('An error occurred while clearing the cart.');
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-trash me-2"></i>Clear Cart';
                });
        });
    }

    document.querySelectorAll('.btn-quantity').forEach(button => {
        button.addEventListener('click', function () {
            const action = this.dataset.action;
            const form = this.closest('form');
            const input = form.querySelector('.quantity-input');
            let currentValue = parseInt(input.value);
            const min = parseInt(input.min);
            const max = parseInt(input.max);

            let shouldSubmit = false;

            if (action === 'increase' && currentValue < max) {
                input.value = currentValue + 1;
                shouldSubmit = true;
            } else if (action === 'decrease' && currentValue > min) {
                input.value = currentValue - 1;
                shouldSubmit = true;
            }

            if (shouldSubmit) {
                // This submits the form and reloads the page,
                // which is what your controller expects.
                form.submit();
            }
        });
    });

    // Handle quantity updates
    document.querySelectorAll('.quantity-input').forEach(input => {
        input.addEventListener('change', function () {
            this.closest('form').submit();
        });
    });
});