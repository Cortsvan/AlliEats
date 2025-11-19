document.addEventListener('DOMContentLoaded', function () {
    // Initialize Add to Cart from home page
    initializeHomeAddToCart();

    // Function to initialize Add to Cart from home page
    function initializeHomeAddToCart() {
        $('.home-add-to-cart').on('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            const button = $(this);
            const itemId = button.data('item-id');
            const itemName = button.data('item-name');
            const itemPrice = button.data('item-price');
            const itemStock = button.data('item-stock');

            // Check stock
            if (itemStock <= 0) {
                toastr.warning('This item is out of stock');
                return;
            }

            // Get CSRF token
            const token = $('input[name="__RequestVerificationToken"]').val();

            // Disable button and show loading
            const originalText = button.text();
            button.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Adding...');

            // Add to cart via AJAX
            $.ajax({
                url: '/Cart/AddItem',
                type: 'POST',
                data: {
                    menuItemId: itemId,
                    quantity: 1,
                    __RequestVerificationToken: token
                },
                success: function(response) {
                    if (response.success) {
                        toastr.success(`${itemName} added to cart!`);
                        
                        // Update cart count if cart badge exists
                        if (response.itemCount !== undefined) {
                            const cartBadge = $('#cartItemCount');
                            if (cartBadge.length) {
                                cartBadge.text(response.itemCount).show();
                            }
                        }

                        // Visual feedback - animate the button
                        button.html('<i class="fas fa-check"></i> Added!');
                        setTimeout(() => {
                            button.html(originalText).prop('disabled', false);
                        }, 2000);
                    } else {
                        toastr.error(response.message || 'Failed to add item to cart');
                        button.html(originalText).prop('disabled', false);
                    }
                },
                error: function(xhr, status, error) {
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
                    button.html(originalText).prop('disabled', false);
                    console.error('Error adding to cart:', xhr.responseText);
                }
            });
        });
    }
});

$(document).ready(function () {
    let currentItemData = null;

    // Initialize Item Details Modal
    initializeItemDetailsModal();

    // Function to initialize item details modal
    function initializeItemDetailsModal() {
        // View details button click handler
        $('.view-details-btn').on('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            const button = $(this);
            currentItemData = {
                id: button.data('item-id'),
                name: button.data('item-name'),
                price: parseFloat(button.data('item-price')),
                stock: parseInt(button.data('item-stock')),
                description: button.data('item-description') || 'No description available.',
                image: button.data('item-image'),
                category: button.data('item-category'),
                rating: parseFloat(button.data('item-rating')),
                reviewCount: parseInt(button.data('item-review-count'))
            };

            // Populate modal
            $('#modalItemName').text(currentItemData.name);
            $('#modalItemDescription').text(currentItemData.description);
            $('#modalItemPrice').text('$' + currentItemData.price.toFixed(2));
            $('#modalItemCategory').html(`<i class="fas fa-tag me-2"></i>${currentItemData.category}`);

            // Set image or show placeholder icon
            if (currentItemData.image) {
                $('#modalItemImage').attr('src', currentItemData.image).show();
            } else {
                $('#modalItemImage').hide();
            }

            // Display rating
            displayModalRating(currentItemData.rating, currentItemData.reviewCount);

            // Reset quantity
            $('#quantity-input').val(1).attr('max', currentItemData.stock);
            updateQuantityButtons();

            // Show modal
            var modal = new bootstrap.Modal(document.getElementById('itemDetailsModal'));
            modal.show();
        });

        // Quantity control handlers
        $('#quantity-plus').on('click', function() {
            let input = $('#quantity-input');
            let currentValue = parseInt(input.val());
            let maxValue = parseInt(input.attr('max'));

            if (currentValue < maxValue && currentValue < 10) {
                input.val(currentValue + 1);
                updateQuantityButtons();
            }
        });

        $('#quantity-minus').on('click', function() {
            let input = $('#quantity-input');
            let currentValue = parseInt(input.val());

            if (currentValue > 1) {
                input.val(currentValue - 1);
                updateQuantityButtons();
            }
        });

        // Confirm Add to Cart button
        $('#confirmAddToCart').on('click', function() {
            if (!currentItemData) return;

            const quantity = parseInt($('#quantity-input').val());
            addToCart(currentItemData.id, currentItemData.name, quantity);
        });
    }

    function displayModalRating(rating, reviewCount) {
        const ratingContainer = $('#modalItemRating');
        ratingContainer.empty();

        if (reviewCount > 0) {
            let starsHtml = '<div class="rating-stars">';
            const fullStars = Math.floor(rating);
            const hasHalfStar = (rating - fullStars) >= 0.5;
            const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

            // Full stars
            for (let i = 0; i < fullStars; i++) {
                starsHtml += '<i class="fas fa-star"></i>';
            }

            // Half star
            if (hasHalfStar) {
                starsHtml += '<i class="fas fa-star-half-alt"></i>';
            }

            // Empty stars
            for (let i = 0; i < emptyStars; i++) {
                starsHtml += '<i class="far fa-star"></i>';
            }

            starsHtml += '</div>';
            starsHtml += `<span class="rating-count">${rating.toFixed(1)} (${reviewCount} review${reviewCount !== 1 ? 's' : ''})</span>`;
            ratingContainer.html(starsHtml);
        } else {
            ratingContainer.html('<div class="rating-stars">' +
                '<i class="far fa-star text-muted"></i>'.repeat(5) +
                '</div><span class="rating-count text-muted">(No reviews yet)</span>');
        }
    }

    function updateQuantityButtons() {
        const input = $('#quantity-input');
        const currentValue = parseInt(input.val());
        const maxValue = parseInt(input.attr('max'));

        $('#quantity-minus').prop('disabled', currentValue <= 1);
        $('#quantity-plus').prop('disabled', currentValue >= maxValue || currentValue >= 10);
    }

    function addToCart(itemId, itemName, quantity) {
        const token = $('input[name="__RequestVerificationToken"]').val();
        const button = $('#confirmAddToCart');

        // Disable button and show loading
        const originalText = button.html();
        button.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Adding...');

        // Add to cart via AJAX
        $.ajax({
            url: '/Cart/AddItem',
            type: 'POST',
            data: {
                menuItemId: itemId,
                quantity: quantity,
                __RequestVerificationToken: token
            },
            success: function(response) {
                if (response.success) {
                    toastr.success(`${itemName} added to cart!`);
                    
                    // Update cart count if cart badge exists
                    if (response.itemCount !== undefined) {
                        const cartBadge = $('#cartItemCount');
                        if (cartBadge.length) {
                            cartBadge.text(response.itemCount).show();
                        }
                    }

                    // Close modal
                    var modal = bootstrap.Modal.getInstance(document.getElementById('itemDetailsModal'));
                    if (modal) {
                        modal.hide();
                    }

                    // Reset button
                    button.prop('disabled', false).html(originalText);
                } else {
                    toastr.error(response.message || 'Failed to add item to cart');
                    button.prop('disabled', false).html(originalText);
                }
            },
            error: function(xhr, status, error) {
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
                button.prop('disabled', false).html(originalText);
                console.error('Error adding to cart:', xhr.responseText);
            }
        });
    }
});


// Add CSS for ripple effect
const rippleCSS = `
    .ripple {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.6);
        transform: scale(0);
        animation: ripple-animation 0.6s linear;
        pointer-events: none;
    }

    @keyframes ripple-animation {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    /* Glow effects */
    .admin-action-icon, .user-action-icon, .feature-icon {
        transition: all 0.3s ease, box-shadow 1s ease;
    }
`;

const style = document.createElement('style');
style.textContent = rippleCSS;
document.head.appendChild(style);