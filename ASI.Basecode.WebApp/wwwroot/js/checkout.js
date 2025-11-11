document.addEventListener('DOMContentLoaded', function () {
    const checkoutForm = document.getElementById('checkoutForm');
    const placeOrderBtn = document.getElementById('placeOrderBtn');
    const paymentMethodSelect = document.getElementById('PaymentMethod');

    // Payment method validation and button styling
    if (paymentMethodSelect) {
        paymentMethodSelect.addEventListener('change', function () {
            const method = this.value;
            if (method) {
                placeOrderBtn.classList.remove('btn-secondary');
                placeOrderBtn.classList.add('btn-checkout');
                placeOrderBtn.disabled = false;
            } else {
                placeOrderBtn.classList.remove('btn-checkout');
                placeOrderBtn.classList.add('btn-secondary');
                placeOrderBtn.disabled = true;
            }
        });

        // Initialize button state
        if (!paymentMethodSelect.value) {
            placeOrderBtn.classList.remove('btn-checkout');
            placeOrderBtn.classList.add('btn-secondary');
            placeOrderBtn.disabled = true;
        }
    }

    // Handle form submission
    if (checkoutForm) {
        checkoutForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const formData = new FormData(this);
            const paymentMethod = formData.get('PaymentMethod');

            // Validate payment method
            if (!paymentMethod) {
                toastr.error('Please select a payment method.');
                return;
            }

            // Show loading state
            placeOrderBtn.disabled = true;
            placeOrderBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing Order...';

            // Add loading class to summary card
            const summaryCard = document.querySelector('.checkout-summary-card');
            if (summaryCard) {
                summaryCard.classList.add('is-loading');
            }

            // Submit form
            fetch(this.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(response => {
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.indexOf('application/json') !== -1) {
                        return response.json();
                    }
                    return { success: true, message: 'Order placed successfully!' };
                })
                .then(data => {
                    if (data.success) {
                        toastr.success(data.message || 'Order placed successfully!');
                        // Redirect to confirmation page
                        setTimeout(function () {
                            if (data.redirectUrl) {
                                window.location.href = data.redirectUrl;
                            } else {
                                // Fallback redirect
                                window.location.href = '/Checkout/OrderConfirmation';
                            }
                        }, 1500);
                    } else {
                        throw new Error(data.message || 'Failed to place order');
                    }
                })
                .catch(error => {
                    console.error('Checkout Error:', error);
                    toastr.error(error.message || 'An error occurred while processing your order.');

                    // Reset button state
                    placeOrderBtn.disabled = false;
                    placeOrderBtn.innerHTML = '<i class="fas fa-shopping-bag me-2"></i>Place Order';

                    // Remove loading state
                    if (summaryCard) {
                        summaryCard.classList.remove('is-loading');
                    }
                });
        });
    }
});