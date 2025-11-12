document.addEventListener('DOMContentLoaded', function () {
    initializeStarRating();
    initializeCommentCounter();
    initializeFormValidation();
    initializeFormAnimations();
});

// Star Rating Functionality
function initializeStarRating() {
    const stars = document.querySelectorAll('.star');
    const ratingInput = document.getElementById('ratingInput');
    const ratingText = document.getElementById('ratingText');

    if (!stars.length) return;

    const ratingTexts = {
        1: 'Poor',
        2: 'Fair',
        3: 'Good',
        4: 'Very Good',
        5: 'Excellent'
    };

    const ratingClasses = {
        1: 'poor',
        2: 'fair',
        3: 'good',
        4: 'very-good',
        5: 'excellent'
    };

    stars.forEach((star, index) => {
        // Handle mouse hover
        star.addEventListener('mouseenter', function () {
            highlightStars(index + 1, true);
        });

        // Handle mouse leave
        star.addEventListener('mouseleave', function () {
            const currentRating = parseInt(ratingInput.value) || 0;
            highlightStars(currentRating, false);
        });

        // Handle click
        star.addEventListener('click', function () {
            const rating = index + 1;
            ratingInput.value = rating;

            // Update rating text
            if (ratingText) {
                ratingText.textContent = ratingTexts[rating];
                ratingText.className = 'rating-text ' + ratingClasses[rating];
            }

            highlightStars(rating, false);

            // Remove validation error if exists
            const errorSpan = document.querySelector('span[data-valmsg-for="Rating"]');
            if (errorSpan) {
                errorSpan.textContent = '';
            }
        });
    });

    function highlightStars(rating, isHover) {
        stars.forEach((star, index) => {
            const starIcon = star.querySelector('i');
            star.classList.remove('active', 'hover');

            if (index < rating) {
                star.classList.add(isHover ? 'hover' : 'active');
                starIcon.className = 'fas fa-star';
            } else {
                starIcon.className = 'far fa-star';
            }
        });
    }

    // Initialize with existing rating if any
    const currentRating = parseInt(ratingInput.value) || 0;
    if (currentRating > 0) {
        highlightStars(currentRating, false);
        if (ratingText) {
            ratingText.textContent = ratingTexts[currentRating];
            ratingText.className = 'rating-text ' + ratingClasses[currentRating];
        }
    }
}

// Comment Character Counter
function initializeCommentCounter() {
    const commentTextarea = document.getElementById('commentTextarea');
    const charCount = document.getElementById('charCount');

    if (!commentTextarea || !charCount) return;

    function updateCharCount() {
        const currentLength = commentTextarea.value.length;
        const maxLength = 1000;

        charCount.textContent = currentLength;

        // Update counter color based on remaining characters
        const counter = charCount.parentElement;
        counter.classList.remove('warning', 'danger');

        if (currentLength > maxLength * 0.9) {
            counter.classList.add('danger');
        } else if (currentLength > maxLength * 0.8) {
            counter.classList.add('warning');
        }
    }

    commentTextarea.addEventListener('input', updateCharCount);

    // Initialize count
    updateCharCount();
}

// Form Validation
function initializeFormValidation() {
    const form = document.getElementById('reviewForm');
    const submitBtn = document.getElementById('submitReviewBtn');

    if (!form || !submitBtn) return;

    form.addEventListener('submit', function (e) {
        const ratingInput = document.getElementById('ratingInput');
        const rating = parseInt(ratingInput.value);

        if (!rating || rating < 1 || rating > 5) {
            e.preventDefault();

            // Show error message
            const errorSpan = document.querySelector('span[data-valmsg-for="Rating"]');
            if (errorSpan) {
                errorSpan.textContent = 'Please provide a rating.';
            }

            // Shake the star rating
            const starRating = document.getElementById('starRating');
            if (starRating) {
                starRating.style.animation = 'shake 0.5s ease-in-out';
                setTimeout(() => {
                    starRating.style.animation = '';
                }, 500);
            }

            if (typeof toastr !== 'undefined') {
                toastr.error('Please provide a rating before submitting your review.');
            }

            return false;
        }

        // Show loading state
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Submitting Review...';

        // Let form submit normally
        return true;
    });
}

// Form Animations
function initializeFormAnimations() {
    const cards = document.querySelectorAll('.review-order-card, .review-form-card');

    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';

        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 200);
    });

    // Add subtle hover effects to order items
    const orderItems = document.querySelectorAll('.order-item-row');
    orderItems.forEach(item => {
        item.addEventListener('mouseenter', function () {
            this.style.transform = 'translateX(5px)';
            this.style.transition = 'transform 0.2s ease';
        });

        item.addEventListener('mouseleave', function () {
            this.style.transform = 'translateX(0)';
        });
    });
}

// Add shake animation CSS
const shakeCSS = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        25% { transform: translateX(-10px); }
        75% { transform: translateX(10px); }
    }
`;

// Inject CSS
const style = document.createElement('style');
style.textContent = shakeCSS;
document.head.appendChild(style);

// Toast notification helper (if toastr is not available)
if (typeof toastr === 'undefined') {
    window.toastr = {
        success: function (message) {
            console.log('Success: ' + message);
            alert('Success: ' + message);
        },
        error: function (message) {
            console.log('Error: ' + message);
            alert('Error: ' + message);
        },
        info: function (message) {
            console.log('Info: ' + message);
            alert('Info: ' + message);
        }
    };
}