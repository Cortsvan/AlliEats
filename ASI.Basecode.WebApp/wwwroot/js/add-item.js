// Add Item functionality - Simple and clean

document.addEventListener('DOMContentLoaded', function () {
    initializeFormEnhancements();
});

function initializeFormEnhancements() {
    // Add input focus effects
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

function previewImage(input) {
    const preview = document.getElementById('preview');
    const previewDiv = document.getElementById('imagePreview');
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

function removeImage() {
    const input = document.getElementById('imageInput');
    const preview = document.getElementById('imagePreview');
    const uploadContainer = document.querySelector('.image-upload-container');

    input.value = '';
    preview.style.display = 'none';
    uploadContainer.style.display = 'block';
}

// Form validation enhancement
function validateForm() {
    const form = document.querySelector('.add-item-form');
    const requiredFields = form.querySelectorAll('[required]');
    let isValid = true;

    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        }
    });

    return isValid;
}

// Price formatting
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