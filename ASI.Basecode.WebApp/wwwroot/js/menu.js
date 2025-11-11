$(document).ready(function () {
    let currentItemId = null;

    // Show modal when a menu item card is clicked
    $('.menu-item-card').on('click', function () {
        const card = $(this);
        currentItemId = card.data('id');
        const name = card.data('name');
        const description = card.data('description');
        const price = parseFloat(card.data('price')).toFixed(2);
        let image = card.data('image');

        // Use placeholder if image path is empty
        if (!image) {
            image = '/images/placeholder-food.jpg'; // Fallback to the placeholder
        }

        $('#modalItemName').text(name);
        $('#modalItemDescription').text(description);
        $('#modalItemPrice').text('P' + price);
        $('#modalItemImage').attr('src', image);

        // Reset quantity
        $('#quantity-input').val(1);

        $('#addToCartModal').modal('show');
    });

    // Quantity controls
    $('#quantity-plus').on('click', function () {
        let quantityInput = $('#quantity-input');
        let currentValue = parseInt(quantityInput.val());
        if (currentValue < 10) {
            quantityInput.val(currentValue + 1);
        }
    });

    $('#quantity-minus').on('click', function () {
        let quantityInput = $('#quantity-input');
        let currentValue = parseInt(quantityInput.val());
        if (currentValue > 1) {
            quantityInput.val(currentValue - 1);
        }
    });

    // Add to cart confirmation
    $('#confirmAddToCart').on('click', function () {
        const quantity = parseInt($('#quantity-input').val());
        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Cart/AddItem', // Correct URL from your old code
            type: 'POST',
            // Send data as form-urlencoded, not JSON
            data: {
                menuItemId: currentItemId,
                quantity: quantity,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Item added to cart!');
                    $('#addToCartModal').modal('hide');
                } else {
                    toastr.error(response.message || 'Failed to add item to cart.');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred. Please try again.');
                console.error("Error adding to cart:", xhr.responseText);
            }
        });
    });

    // --- UPDATED: Category smooth scrolling logic ---
    $('.category-btn').on('click', function (e) {
        e.preventDefault();

        // Update active button state
        $('.category-btn').removeClass('active');
        $(this).addClass('active');

        const targetId = $(this).data('target');
        const targetElement = $(targetId);

        if (targetElement.length) {
            // Calculate position, accounting for a fixed header if you have one (optional)
            const headerOffset = 80; // Adjust this value based on your site's header height
            const targetPosition = targetElement.offset().top - headerOffset;

            $('html, body').animate({
                scrollTop: targetPosition
            }, 500); // 500ms scroll speed
        }

        // Ensure all category sections are visible for scrolling
        $('.category-section').show();
    });
});