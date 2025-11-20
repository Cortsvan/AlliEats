document.addEventListener('DOMContentLoaded', function () {
    const checkoutForm = document.getElementById('checkoutForm');
    const placeOrderBtn = document.getElementById('placeOrderBtn');
    const paymentMethodSelect = document.getElementById('PaymentMethod');
    const savedCardsSection = document.getElementById('savedCardsSection');
    const selectedCardIdInput = document.getElementById('SelectedCardId');

    // Payment method validation and button styling
    if (paymentMethodSelect) {
        paymentMethodSelect.addEventListener('change', function () {
            const method = this.value;
            
            // Show/hide saved cards section
            if (savedCardsSection) {
                if (method === 'Credit or Debit Card') {
                    savedCardsSection.style.display = 'block';
                    
                    // Check if there are any saved cards
                    const hasSavedCards = document.querySelectorAll('input[name="SelectedCardId"]').length > 0;
                    
                    if (!hasSavedCards) {
                        // No saved cards - show notification and redirect
                        toastr.info('You need to add a payment card first. Redirecting...', 'No Saved Cards', {
                            timeOut: 2000,
                            closeButton: true
                        });
                        
                        setTimeout(function() {
                            // Pass returnUrl so user can come back to checkout after adding card
                            window.location.href = '/Settings/AddPaymentCard?returnUrl=' + encodeURIComponent('/Checkout/Index');
                        }, 2000);
                        return;
                    }
                } else {
                    savedCardsSection.style.display = 'none';
                }
            }

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
        } else if (paymentMethodSelect.value === 'Credit or Debit Card' && savedCardsSection) {
            savedCardsSection.style.display = 'block';
        }
    }

    // Handle saved card selection
    if (selectedCardIdInput) {
        const cardRadios = document.querySelectorAll('input[name="SelectedCardId"]');
        cardRadios.forEach(radio => {
            radio.addEventListener('change', function () {
                selectedCardIdInput.value = this.value;
            });
        });

        // Set initial value if a card is checked
        const checkedCard = document.querySelector('input[name="SelectedCardId"]:checked');
        if (checkedCard) {
            selectedCardIdInput.value = checkedCard.value;
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

            // Validate card selection if Credit or Debit Card is selected
            if (paymentMethod === 'Credit or Debit Card') {
                const selectedCard = document.querySelector('input[name="SelectedCardId"]:checked');
                const hasSavedCards = document.querySelectorAll('input[name="SelectedCardId"]').length > 0;
                
                if (!hasSavedCards) {
                    // No saved cards available
                    toastr.error('You need to add a payment card first.');
                    setTimeout(function() {
                        window.location.href = '/Settings/AddPaymentCard?returnUrl=' + encodeURIComponent('/Checkout/Index');
                    }, 1500);
                    return;
                }
                
                if (!selectedCard || !selectedCard.value) {
                    toastr.error('Please select a payment card.');
                    
                    // Scroll to the saved cards section
                    if (savedCardsSection) {
                        savedCardsSection.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        savedCardsSection.classList.add('highlight-error');
                        setTimeout(() => savedCardsSection.classList.remove('highlight-error'), 2000);
                    }
                    return;
                }

                // Update the hidden input with selected card ID
                if (selectedCardIdInput) {
                    selectedCardIdInput.value = selectedCard.value;
                }
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
                        // Handle redirect to add card if needed
                        if (data.redirectAddCard && data.addCardUrl) {
                            toastr.error(data.message, 'Payment Card Required');
                            setTimeout(function() {
                                window.location.href = data.addCardUrl;
                            }, 2000);
                        } else {
                            throw new Error(data.message || 'Failed to place order');
                        }
                    }
                })
                .catch(error => {
                    console.error('Checkout Error:', error);
                    toastr.error(error.message || 'An error occurred while processing your order.');

                    // Reset button state
                    placeOrderBtn.disabled = false;
                    placeOrderBtn.innerHTML = '<i class="fas fa-lock me-2"></i>Place Order';

                    // Remove loading state
                    if (summaryCard) {
                        summaryCard.classList.remove('is-loading');
                    }
                });
        });
    }
});