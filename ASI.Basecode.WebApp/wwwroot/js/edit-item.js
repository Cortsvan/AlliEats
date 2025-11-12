// Edit Item functionality - Extends add-item functionality

document.addEventListener('DOMContentLoaded', function () {
    initializeFormEnhancements();
    initializeImageToggle();
    initializeStatusToggle();
});

function initializeFormEnhancements() {
    // Add input focus effects (same as add-item)
    const formControls = document.querySelectorAll('.form-control-enhanced');
    formControls.forEach(control => {
        control.addEventListener('focus', function () {
            this.parentElement.classList.add('focused');
        });

        control.addEventListener('blur', function () {
            this.parentElement.classList.remove('focused');
        });
    });
}

function initializeImageToggle() {
    // Create backdrop for full-size image view
    const backdrop = document.createElement('div');
    backdrop.className = 'image-backdrop';
    backdrop.onclick = closeFullSizeImage;
    document.body.appendChild(backdrop);
}

function initializeStatusToggle() {
    const checkbox = document.getElementById('IsActive');
    if (checkbox) {
        // Add status indicator
        updateStatusIndicator(checkbox.checked);

        checkbox.addEventListener('change', function () {
            updateStatusIndicator(this.checked);
        });
    }
}

function updateStatusIndicator(isActive) {
    const container = document.querySelector('.form-check-container');
    let indicator = container.querySelector('.status-indicator');

    if (!indicator) {
        indicator = document.createElement('div');
        indicator.className = 'status-indicator';
        container.appendChild(indicator);
    }

    if (isActive) {
        indicator.className = 'status-indicator active';
        indicator.innerHTML = '<i class="fas fa-check-circle"></i> Item will be visible to customers';
    } else {
        indicator.className = 'status-indicator inactive';
        indicator.innerHTML = '<i class="fas fa-eye-slash"></i> Item will be hidden from customers';
    }
}

function toggleImageSize() {
    const image = document.getElementById('currentImage');
    const backdrop = document.querySelector('.image-backdrop');

    if (image.classList.contains('full-size')) {
        closeFullSizeImage();
    } else {
        image.classList.add('full-size');
        backdrop.classList.add('show');
        document.body.style.overflow = 'hidden';
    }
}

function closeFullSizeImage() {
    const image = document.getElementById('currentImage');
    const backdrop = document.querySelector('.image-backdrop');

    if (image) {
        image.classList.remove('full-size');
    }
    backdrop.classList.remove('show');
    document.body.style.overflow = '';
}

function previewNewImage(input) {
    const preview = document.getElementById('newPreview');
    const previewDiv = document.getElementById('newImagePreview');
    const uploadContainer = document.querySelector('.image-upload-container');

    if (input.files && input.files[0]) {
        const file = input.files[0];

        // Check file size (5MB limit)
        if (file.size > 5 * 1024 * 1024) {
            alert('File size must be less than 5MB');
            input.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            previewDiv.style.display = 'block';
            uploadContainer.style.display = 'none';
        };

        reader.readAsDataURL(file);
    } else {
        previewDiv.style.display = 'none';
        uploadContainer.style.display = 'block';
    }
}

function removeNewImage() {
    const input = document.getElementById('imageInput');
    const preview = document.getElementById('newImagePreview');
    const uploadContainer = document.querySelector('.image-upload-container');

    input.value = '';
    preview.style.display = 'none';
    uploadContainer.style.display = 'block';
}

// Price formatting (same as add-item)
document.addEventListener('DOMContentLoaded', function () {
    const priceInput = document.querySelector('input[name="Price"]');
    if (priceInput) {
        priceInput.addEventListener('blur', function () {
            if (this.value) {
                this.value = parseFloat(this.value).toFixed(2);
            }
        });
    }
});

// Keyboard shortcuts
document.addEventListener('keydown', function (e) {
    // Escape key to close full-size image
    if (e.key === 'Escape') {
        closeFullSizeImage();
    }
});