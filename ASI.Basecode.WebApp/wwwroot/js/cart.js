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

    // Handle quantity updates
    document.querySelectorAll('.quantity-input').forEach(input => {
        input.addEventListener('change', function () {
            this.closest('form').submit();
        });
    });
});