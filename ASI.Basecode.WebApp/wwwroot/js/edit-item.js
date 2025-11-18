// Edit Item functionality - Extends add-item functionality

document.addEventListener('DOMContentLoaded', function () {
    initializeFormEnhancements();
    initializeImageToggle();
    initializeStatusToggle();
    initializeStockManagement(); // Add this new initialization
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

// NEW: Stock management functions
function initializeStockManagement() {
    const stockInput = document.getElementById('Stock');
    if (stockInput) {
        updateStockPreview(stockInput.value);

        // Add input event listener for real-time updates
        stockInput.addEventListener('input', function () {
            updateStockPreview(this.value);
        });
    }
}

function adjustStock(amount) {
    const stockInput = document.getElementById('Stock');
    const currentValue = parseInt(stockInput.value) || 0;
    const newValue = Math.max(0, currentValue + amount);
    stockInput.value = newValue;
    updateStockPreview(newValue);

    // Trigger validation
    stockInput.dispatchEvent(new Event('change'));

    // Add visual feedback for the button clicked
    const buttons = document.querySelectorAll('.stock-quick-actions .btn');
    buttons.forEach(btn => {
        if (btn.textContent.includes(amount > 0 ? '+' + amount : amount.toString())) {
            btn.classList.add('btn-feedback');
            setTimeout(() => btn.classList.remove('btn-feedback'), 200);
        }
    });
}

function updateStockPreview(stockValue) {
    const stock = parseInt(stockValue) || 0;
    const previewText = document.getElementById('stock-preview-text');
    const currentStockDisplay = document.getElementById('current-stock-display');
    const stockStatusDisplay = document.getElementById('stock-status-display');
    const stockWarnings = document.getElementById('stock-warnings');
    const statusBadge = document.querySelector('.stock-status-badge');

    // Update preview text
    if (previewText) {
        previewText.textContent = `New value: ${stock} items available for ordering`;
    }

    // Update summary display
    if (currentStockDisplay) currentStockDisplay.textContent = stock;

    // Update status display
    let status = stock === 0 ? 'Out of Stock' : stock <= 5 ? 'Low Stock' : 'In Stock';
    if (stockStatusDisplay) stockStatusDisplay.textContent = status;

    // Update header badge
    if (statusBadge) {
        let badgeClass = 'badge ';
        let badgeIcon = '';
        let badgeText = '';

        if (stock === 0) {
            badgeClass += 'bg-danger';
            badgeIcon = 'fas fa-exclamation-circle';
            badgeText = 'Out of Stock';
        } else if (stock <= 5) {
            badgeClass += 'bg-warning text-dark';
            badgeIcon = 'fas fa-exclamation-triangle';
            badgeText = `Low Stock (${stock})`;
        } else {
            badgeClass += 'bg-success';
            badgeIcon = 'fas fa-check-circle';
            badgeText = `In Stock (${stock})`;
        }

        statusBadge.innerHTML = `
            <span class="${badgeClass}">
                <i class="${badgeIcon} me-1"></i>${badgeText}
            </span>
        `;
    }

    // Update warnings
    if (stockWarnings) {
        stockWarnings.innerHTML = '';
        if (stock === 0) {
            stockWarnings.innerHTML = `
                <div class="alert alert-warning alert-sm">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>Warning:</strong> This item will be out of stock. Consider restocking or setting it to inactive.
                </div>
            `;
        } else if (stock <= 5) {
            stockWarnings.innerHTML = `
                <div class="alert alert-info alert-sm">
                    <i class="fas fa-info-circle me-2"></i>
                    <strong>Notice:</strong> Low stock level. Consider restocking soon.
                </div>
            `;
        }
    }
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

    // Stock adjustment shortcuts (Ctrl + Arrow keys)
    if (e.ctrlKey && document.getElementById('Stock') === document.activeElement) {
        switch (e.key) {
            case 'ArrowUp':
                e.preventDefault();
                adjustStock(5);
                break;
            case 'ArrowDown':
                e.preventDefault();
                adjustStock(-5);
                break;
            case 'PageUp':
                e.preventDefault();
                adjustStock(10);
                break;
            case 'PageDown':
                e.preventDefault();
                adjustStock(-10);
                break;
        }
    }
});