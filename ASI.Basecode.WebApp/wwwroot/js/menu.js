$(document).ready(function () {
    let currentItemData = null;
    let carouselPositions = {}; // Track position of each category carousel

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
        }, 300);
    }

    // Initialize carousels
    initializeCarousels();

    // Initialize Item Details Modal (Same as home.js)
    initializeItemDetailsModal();

    function initializeCarousels() {
        // Initialize position tracking for each category
        $('.category-carousel-container').each(function() {
            const category = $(this).data('category');
            carouselPositions[category] = 0;
            updateCarouselButtons(category);
        });

        // Previous button click handler
        $('.carousel-prev').on('click', function() {
            const category = $(this).data('category');
            scrollCarousel(category, 'prev');
        });

        // Next button click handler
        $('.carousel-next').on('click', function() {
            const category = $(this).data('category');
            scrollCarousel(category, 'next');
        });
    }

    function scrollCarousel(category, direction) {
        const container = $(`.category-carousel-container[data-category="${category}"]`);
        const track = container.find('.category-carousel-track');
        const items = track.find('.carousel-item-wrapper');
        
        if (items.length === 0) return;

        const itemWidth = items.first().outerWidth(true); // includes margin
        const containerWidth = container.width();
        const visibleItems = Math.floor(containerWidth / itemWidth);
        const maxScroll = Math.max(0, items.length - visibleItems);

        if (direction === 'next' && carouselPositions[category] < maxScroll) {
            carouselPositions[category]++;
        } else if (direction === 'prev' && carouselPositions[category] > 0) {
            carouselPositions[category]--;
        }

        const translateX = -(carouselPositions[category] * itemWidth);
        track.css('transform', `translateX(${translateX}px)`);
        
        updateCarouselButtons(category);
    }

    function updateCarouselButtons(category) {
        const container = $(`.category-carousel-container[data-category="${category}"]`);
        const track = container.find('.category-carousel-track');
        const items = track.find('.carousel-item-wrapper');
        
        if (items.length === 0) return;

        const itemWidth = items.first().outerWidth(true);
        const containerWidth = container.width();
        const visibleItems = Math.floor(containerWidth / itemWidth);
        const maxScroll = Math.max(0, items.length - visibleItems);

        const prevBtn = $(`.carousel-prev[data-category="${category}"]`);
        const nextBtn = $(`.carousel-next[data-category="${category}"]`);

        // Disable/enable buttons based on position
        prevBtn.prop('disabled', carouselPositions[category] <= 0);
        nextBtn.prop('disabled', carouselPositions[category] >= maxScroll);
    }

    // Recalculate carousel positions on window resize
    $(window).on('resize', function() {
        Object.keys(carouselPositions).forEach(category => {
            carouselPositions[category] = 0;
            const track = $(`.category-carousel-container[data-category="${category}"] .category-carousel-track`);
            track.css('transform', 'translateX(0)');
            updateCarouselButtons(category);
        });
    });

    function initializeItemDetailsModal() {
        // Show modal when a menu item card is clicked
        $('.menu-item-card').on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const card = $(this);
            const stock = parseInt(card.data('stock')) || 0;

            // Check if item is out of stock
            if (stock === 0) {
                toastr.warning('This item is currently out of stock and cannot be ordered.');
                return;
            }

            // Gather item data
            currentItemData = {
                id: card.data('id'),
                name: card.data('name'),
                price: parseFloat(card.data('price')),
                stock: stock,
                description: card.data('description') || 'No description available.',
                image: card.data('image'),
                category: card.data('category'),
                rating: parseFloat(card.data('rating')) || 0,
                reviewCount: parseInt(card.data('review-count')) || 0
            };

            // Populate modal
            $('#modalItemName').text(currentItemData.name);
            $('#modalItemDescription').text(currentItemData.description);
            $('#modalItemPrice').text('P' + currentItemData.price.toFixed(2));
            $('#modalItemCategory').html(`<i class="fas fa-tag me-2"></i>${currentItemData.category}`);

            // Set image or show placeholder
            if (currentItemData.image) {
                $('#modalItemImage').attr('src', currentItemData.image).show();
            } else {
                $('#modalItemImage').attr('src', '/img/placeholder-food.png').show();
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