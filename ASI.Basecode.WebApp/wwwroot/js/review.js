document.addEventListener('DOMContentLoaded', function () {
    initializeStarRating();
    initializeCommentCounter();
    initializeFormValidation();
    initializeFormAnimations();
    initializeEditReviewFunctionality();
});

// Star Rating Functionality (for Create page)
function initializeStarRating() {
    const stars = document.querySelectorAll('#starRating .star');
    const ratingInput = document.getElementById('ratingInput');
    const ratingText = document.getElementById('ratingText');

    if (!stars.length || !ratingInput) return;

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
        star.addEventListener('mouseenter', function () {
            highlightStars(index + 1, true);
        });

        star.addEventListener('mouseleave', function () {
            const currentRating = parseInt(ratingInput.value) || 0;
            highlightStars(currentRating, false);
        });

        star.addEventListener('click', function () {
            const rating = index + 1;
            ratingInput.value = rating;

            if (ratingText) {
                ratingText.textContent = ratingTexts[rating];
                ratingText.className = 'rating-text ' + ratingClasses[rating];
            }

            highlightStars(rating, false);

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

    const currentRating = parseInt(ratingInput.value) || 0;
    if (currentRating > 0) {
        highlightStars(currentRating, false);
        if (ratingText) {
            ratingText.textContent = ratingTexts[currentRating];
            ratingText.className = 'rating-text ' + ratingClasses[currentRating];
        }
    }
}

// Comment Character Counter (for Create page)
function initializeCommentCounter() {
    const commentTextarea = document.getElementById('commentTextarea');
    const charCount = document.getElementById('charCount');

    if (!commentTextarea || !charCount) return;

    function updateCharCount() {
        const currentLength = commentTextarea.value.length;
        const maxLength = 1000;

        charCount.textContent = currentLength;

        const counter = charCount.parentElement;
        counter.classList.remove('warning', 'danger');

        if (currentLength > maxLength * 0.9) {
            counter.classList.add('danger');
        } else if (currentLength > maxLength * 0.8) {
            counter.classList.add('warning');
        }
    }

    commentTextarea.addEventListener('input', updateCharCount);
    updateCharCount();
}

// Form Validation (for Create page)
function initializeFormValidation() {
    const form = document.getElementById('reviewForm');
    const submitBtn = document.getElementById('submitReviewBtn');

    if (!form || !submitBtn) return;

    form.addEventListener('submit', function (e) {
        const ratingInput = document.getElementById('ratingInput');
        const rating = parseInt(ratingInput.value);

        if (!rating || rating < 1 || rating > 5) {
            e.preventDefault();

            const errorSpan = document.querySelector('span[data-valmsg-for="Rating"]');
            if (errorSpan) {
                errorSpan.textContent = 'Please provide a rating.';
            }

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

        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Submitting Review...';
        return true;
    });
}

// Form Animations
function initializeFormAnimations() {
    const cards = document.querySelectorAll('.review-order-card, .review-form-card, .review-card');

    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';

        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 200);
    });

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

// Edit Review Functionality (for MyReviews page)
function initializeEditReviewFunctionality() {
    const editButtons = document.querySelectorAll('.edit-review-btn');
    const editModal = document.getElementById('editReviewModal');

    if (!editButtons.length || !editModal) return;

    let currentEditRating = 0;

    // Edit button click handlers
    editButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const orderId = this.getAttribute('data-order-id');
            loadReviewForEdit(orderId);
        });
    });

    // Initialize edit star rating
    const editStars = document.querySelectorAll('#editStarRating .star');
    const editRatingInput = document.getElementById('editRatingInput');
    const editRatingText = document.getElementById('editRatingText');

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

    editStars.forEach((star, index) => {
        star.addEventListener('mouseenter', function () {
            highlightEditStars(index + 1, true);
        });

        star.addEventListener('mouseleave', function () {
            highlightEditStars(currentEditRating, false);
        });

        star.addEventListener('click', function () {
            currentEditRating = index + 1;
            editRatingInput.value = currentEditRating;

            if (editRatingText) {
                editRatingText.textContent = ratingTexts[currentEditRating];
                editRatingText.className = 'rating-text ' + ratingClasses[currentEditRating];
            }

            highlightEditStars(currentEditRating, false);

            const errorDiv = document.getElementById('editRatingError');
            if (errorDiv) {
                errorDiv.textContent = '';
                errorDiv.style.display = 'none';
            }
        });
    });

    function highlightEditStars(rating, isHover) {
        editStars.forEach((star, index) => {
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

    function setEditRating(rating) {
        currentEditRating = rating;
        editRatingInput.value = rating;
        highlightEditStars(rating, false);
        if (editRatingText) {
            editRatingText.textContent = ratingTexts[rating];
            editRatingText.className = 'rating-text ' + ratingClasses[rating];
        }
    }

    // Edit comment character counter
    const editComment = document.getElementById('editComment');
    const editCharCount = document.getElementById('editCharCount');

    if (editComment && editCharCount) {
        function updateEditCharCount() {
            const currentLength = editComment.value.length;
            editCharCount.textContent = currentLength;

            const counter = editCharCount.parentElement;
            counter.classList.remove('warning', 'danger');

            if (currentLength > 900) {
                counter.classList.add('danger');
            } else if (currentLength > 800) {
                counter.classList.add('warning');
            }
        }

        editComment.addEventListener('input', updateEditCharCount);
    }

    // Save button click handler
    const saveBtn = document.getElementById('saveReviewBtn');
    if (saveBtn) {
        saveBtn.addEventListener('click', function () {
            saveReview();
        });
    }

    function loadReviewForEdit(orderId) {
        const saveBtn = document.getElementById('saveReviewBtn');
        if (saveBtn) {
            saveBtn.disabled = true;
            saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Loading...';
        }

        // Changed URL to use new action name
        fetch(`/Review/GetReviewForEdit?orderId=${orderId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    document.getElementById('editOrderId').value = data.review.orderId;
                    editComment.value = data.review.comment || '';
                    updateEditCharCount();

                    setEditRating(data.review.rating);

                    const modal = new bootstrap.Modal(editModal);
                    modal.show();
                } else {
                    showToast('error', data.message);
                }
            })
            .catch(error => {
                console.error('Error loading review:', error);
                showToast('error', 'An error occurred while loading the review.');
            })
            .finally(() => {
                if (saveBtn) {
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = '<i class="fas fa-save me-2"></i>Save Changes';
                }
            });
    }

    function updateEditCharCount() {
        if (editComment && editCharCount) {
            const currentLength = editComment.value.length;
            editCharCount.textContent = currentLength;
        }
    }

    function saveReview() {
        const orderId = document.getElementById('editOrderId').value;
        const rating = parseInt(editRatingInput.value);
        const comment = editComment.value;

        if (!rating || rating < 1 || rating > 5) {
            const errorDiv = document.getElementById('editRatingError');
            if (errorDiv) {
                errorDiv.textContent = 'Please provide a rating.';
                errorDiv.style.display = 'block';
            }
            return;
        }

        if (comment && comment.length > 1000) {
            const errorDiv = document.getElementById('editCommentError');
            if (errorDiv) {
                errorDiv.textContent = 'Comment cannot exceed 1000 characters.';
                errorDiv.style.display = 'block';
            }
            return;
        }

        const saveBtn = document.getElementById('saveReviewBtn');
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Saving...';

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const formData = new FormData();
        formData.append('orderId', orderId);
        formData.append('rating', rating);
        formData.append('comment', comment);
        formData.append('__RequestVerificationToken', token);

        // Changed URL to use new action name
        fetch('/Review/UpdateReview', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showToast('success', data.message);
                    const modal = bootstrap.Modal.getInstance(editModal);
                    if (modal) {
                        modal.hide();
                    }
                    setTimeout(() => {
                        location.reload();
                    }, 1000);
                } else {
                    showToast('error', data.message);
                }
            })
            .catch(error => {
                console.error('Error saving review:', error);
                showToast('error', 'An error occurred while saving the review.');
            })
            .finally(() => {
                saveBtn.disabled = false;
                saveBtn.innerHTML = '<i class="fas fa-save me-2"></i>Save Changes';
            });
    }
}

function showToast(type, message) {
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    } else {
        alert(type.charAt(0).toUpperCase() + type.slice(1) + ': ' + message);
    }
}

// Add shake animation CSS
const shakeCSS = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        25% { transform: translateX(-10px); }
        75% { transform: translateX(10px); }
    }
`;

const style = document.createElement('style');
style.textContent = shakeCSS;
document.head.appendChild(style);

// Toast notification helper
if (typeof toastr === 'undefined') {
    window.toastr = {
        success: function (message) {
            console.log('Success: ' + message);
        },
        error: function (message) {
            console.log('Error: ' + message);
        },
        info: function (message) {
            console.log('Info: ' + message);
        }
    };
}