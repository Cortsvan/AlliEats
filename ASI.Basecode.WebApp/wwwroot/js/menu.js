$(document).ready(function () {
    let currentItemId = null;
    let currentStock = 0;

    // Handle anchor links from external pages (like home page)
    if (window.location.hash) {
        setTimeout(function() {
            const targetId = window.location.hash;
            const targetElement = $(targetId);
            
            if (targetElement.length) {
                // Activate the corresponding category button
                const categoryBtn = $(`.category-btn[data-target="${targetId}"]`);
                if (categoryBtn.length) {
                    $('.category-btn').removeClass('active');
                    categoryBtn.addClass('active');
                }
                
                // Scroll to the category section
                const headerOffset = 100;
                const targetPosition = targetElement.offset().top - headerOffset;
                
                $('html, body').animate({
                    scrollTop: targetPosition
                }, 800, 'swing');
            }
        }, 300); // Small delay to ensure page is fully loaded
    }

    // Show modal when a menu item card is clicked
    $('.menu-item-card').on('click', function () {
        const card = $(this);

        // Check if item is out of stock before opening modal
        const stock = parseInt(card.data('stock')) || 0;
        if (stock === 0) {
            showStockAlert('This item is currently out of stock and cannot be ordered.', 'warning');
            return;
        }

        currentItemId = card.data('id');
        currentStock = stock;
        const name = card.data('name');
        const description = card.data('description');
        const price = parseFloat(card.data('price')).toFixed(2);
        let image = card.data('image');

        if (!image) {
            image = '/images/placeholder-food.jpg';
        }

        $('#modalItemName').text(name);
        $('#modalItemDescription').text(description);
        $('#modalItemPrice').text('P' + price);
        $('#modalItemImage').attr('src', image);

        // Reset quantity and update controls based on stock
        $('#quantity-input').val(1).attr('max', currentStock);
        updateQuantityControls();

        // Add stock indicator to modal
        updateStockDisplay();

        $('#addToCartModal').modal('show');
    });

    // Quantity controls - Updated with stock validation
    $('#quantity-plus').on('click', function () {
        let quantityInput = $('#quantity-input');
        let currentValue = parseInt(quantityInput.val());
        let maxQuantity = Math.min(currentStock, 10);

        if (currentValue < maxQuantity) {
            quantityInput.val(currentValue + 1);
            updateQuantityControls();
        }
    });

    $('#quantity-minus').on('click', function () {
        let quantityInput = $('#quantity-input');
        let currentValue = parseInt(quantityInput.val());
        if (currentValue > 1) {
            quantityInput.val(currentValue - 1);
            updateQuantityControls();
        }
    });

    // Update quantity controls based on stock
    function updateQuantityControls() {
        const quantityInput = $('#quantity-input');
        const currentQuantity = parseInt(quantityInput.val());
        const maxQuantity = Math.min(currentStock, 10);

        $('#quantity-plus').prop('disabled', currentQuantity >= maxQuantity);
        $('#quantity-minus').prop('disabled', currentQuantity <= 1);

        updateStockWarning(currentQuantity, maxQuantity);
    }

    // Update stock warning display
    function updateStockWarning(currentQuantity, maxQuantity) {
        const warningContainer = $('#stock-warning');

        if (currentQuantity >= maxQuantity && maxQuantity < 10) {
            if (warningContainer.length === 0) {
                const warningHtml = `
                    <div id="stock-warning" class="stock-warning mt-2">
                        <i class="fas fa-exclamation-triangle me-1"></i>
                        Maximum available quantity reached (${maxQuantity} in stock)
                    </div>`;
                $('.quantity-controls').after(warningHtml);
            }
        } else {
            warningContainer.remove();
        }
    }

    // Add stock display to modal
    function updateStockDisplay() {
        $('#stock-display').remove();

        let stockHtml = '';
        if (currentStock <= 5) {
            stockHtml = `
                <div id="stock-display" class="stock-display mb-3">
                    <span class="stock-indicator low-stock">
                        <i class="fas fa-box me-1"></i>
                        Only ${currentStock} left in stock!
                    </span>
                </div>`;
        } else {
            stockHtml = `
                <div id="stock-display" class="stock-display mb-3">
                    <span class="stock-indicator in-stock">
                        <i class="fas fa-check-circle me-1"></i>
                        ${currentStock} available
                    </span>
                </div>`;
        }

        $('#modalItemPrice').after(stockHtml);
    }

    // Add to cart confirmation - FIXED: No more visual stock updates
    $('#confirmAddToCart').on('click', function () {
        const quantity = parseInt($('#quantity-input').val());
        const token = $('input[name="__RequestVerificationToken"]').val();

        if (quantity > currentStock) {
            showStockAlert(`Cannot add ${quantity} items. Only ${currentStock} available.`, 'error');
            return;
        }

        const $button = $(this);
        const originalText = $button.text();
        $button.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Adding...');

        $.ajax({
            url: '/Cart/AddItem',
            type: 'POST',
            data: {
                menuItemId: currentItemId,
                quantity: quantity,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Item added to cart!');
                    $('#addToCartModal').modal('hide');

                    // REMOVED: updateCardStockDisplay(currentItemId, quantity);
                    // Stock should only visually update when page refreshes or orders are placed
                } else {
                    toastr.error(response.message || 'Failed to add item to cart.');
                }
            },
            error: function (xhr, status, error) {
                let errorMessage = 'An error occurred. Please try again.';
                try {
                    const errorResponse = JSON.parse(xhr.responseText);
                    if (errorResponse.message) {
                        errorMessage = errorResponse.message;
                    }
                } catch (e) {
                    // Use default message
                }

                toastr.error(errorMessage);
                console.error("Error adding to cart:", xhr.responseText);
            },
            complete: function () {
                $button.prop('disabled', false).text(originalText);
            }
        });
    });

    // REMOVED: updateCardStockDisplay function entirely
    // Stock visual updates should only happen when:
    // 1. Page is refreshed/reloaded
    // 2. Orders are actually placed (not just added to cart)

    // Show stock alert function
    function showStockAlert(message, type = 'warning') {
        const alertClass = type === 'error' ? 'alert-danger' : 'alert-warning';
        const icon = type === 'error' ? 'fas fa-times-circle' : 'fas fa-exclamation-triangle';

        const alertDiv = $(`
            <div class="alert ${alertClass} alert-dismissible fade show stock-alert" role="alert">
                <i class="${icon} me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `);

        $('.stock-alert').remove();
        $('.menu-container').prepend(alertDiv);

        setTimeout(() => {
            alertDiv.fadeOut('slow', function () {
                $(this).remove();
            });
        }, 5000);
    }

    // Category smooth scrolling logic
    $('.category-btn').on('click', function (e) {
        e.preventDefault();

        $('.category-btn').removeClass('active');
        $(this).addClass('active');

        const targetId = $(this).data('target');
        const targetElement = $(targetId);

        if (targetElement.length) {
            const headerOffset = 80;
            const targetPosition = targetElement.offset().top - headerOffset;

            $('html, body').animate({
                scrollTop: targetPosition
            }, 500);
        }

        $('.category-section').show();
    });
});