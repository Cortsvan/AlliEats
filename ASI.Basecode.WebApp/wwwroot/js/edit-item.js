// Edit Item functionality - Extends add-item functionality

document.addEventListener('DOMContentLoaded', function () {
    initializeFormEnhancements();
    initializeImageToggle();
    initializeStatusToggle();
    initializeStockManagement();
    initializePriceFormatting();
});

// ==========================================
// 1. FORM ENHANCEMENTS
// ==========================================
function initializeFormEnhancements() {
    // Add input focus effects
    const formControls = document.querySelectorAll('.form-control-modern, .form-control-enhanced');
    formControls.forEach(control => {
        control.addEventListener('focus', function () {
            if (this.parentElement.classList.contains('input-group')) {
                this.parentElement.classList.add('focused');
            }
        });

        control.addEventListener('blur', function () {
            if (this.parentElement.classList.contains('input-group')) {
                this.parentElement.classList.remove('focused');
            }
        });
    });
}

function initializePriceFormatting() {
    const priceInput = document.querySelector('input[name="Price"]');
    if (priceInput) {
        priceInput.addEventListener('blur', function () {
            if (this.value) {
                this.value = parseFloat(this.value).toFixed(2);
            }
        });
    }
}

// ==========================================
// 2. IMAGE PREVIEW & TOGGLE LOGIC (THE FIX)
// ==========================================

function initializeImageToggle() {
    // Create backdrop for full-size image view if it doesn't exist
    if (!document.querySelector('.image-backdrop')) {
        const backdrop = document.createElement('div');
        backdrop.className = 'image-backdrop';
        backdrop.onclick = closeFullSizeImage;
        document.body.appendChild(backdrop);
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
        document.body.style.overflow = 'hidden'; // Prevent scrolling background
    }
}

function closeFullSizeImage() {
    const image = document.getElementById('currentImage');
    const backdrop = document.querySelector('.image-backdrop');

    if (image) image.classList.remove('full-size');
    if (backdrop) backdrop.classList.remove('show');
    document.body.style.overflow = '';
}

// FIXED: Handles the "display: none" toggle to prevent broken image icon
function previewNewImage(input) {
    const preview = document.getElementById('newPreview');
    const previewDiv = document.getElementById('newImagePreview');
    const uploadContainer = document.getElementById('uploadZone'); // Updated ID

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
            // Show the preview box
            previewDiv.style.display = 'block';
            // Hide the upload zone
            uploadContainer.style.display = 'none';
        };

        reader.readAsDataURL(file);
    } else {
        removeNewImage();
    }
}

function removeNewImage() {
    const input = document.getElementById('imageInput');
    const previewDiv = document.getElementById('newImagePreview');
    const uploadContainer = document.getElementById('uploadZone');

    input.value = '';
    // Hide the preview box
    previewDiv.style.display = 'none';
    // Show the upload zone
    uploadContainer.style.display = 'block'; // Or 'flex' if defined in CSS
}

// ==========================================
// 3. STATUS TOGGLE (UPDATED SELECTOR)
// ==========================================

function initializeStatusToggle() {
    const checkbox = document.getElementById('isActiveCheck'); // Updated ID from HTML
    if (checkbox) {
        // Add status indicator
        updateStatusIndicator(checkbox.checked);

        checkbox.addEventListener('change', function () {
            updateStatusIndicator(this.checked);
        });
    }
}

function updateStatusIndicator(isActive) {
    // Updated selector to match ".form-check-modern" from HTML
    const container = document.querySelector('.form-check-modern');

    if (!container) return;

    let indicator = container.querySelector('.status-indicator');

    // Create indicator if it doesn't exist (Optional, visual helper)
    if (!indicator) {
        indicator = document.createElement('div');
        indicator.className = 'status-indicator mt-2 small fw-bold';
        container.appendChild(indicator);
    }

    if (isActive) {
        indicator.className = 'status-indicator mt-2 small fw-bold text-success';
        indicator.innerHTML = '<i class="fas fa-check-circle me-1"></i> Item is visible to customers';
    } else {
        indicator.className = 'status-indicator mt-2 small fw-bold text-muted';
        indicator.innerHTML = '<i class="fas fa-eye-slash me-1"></i> Item is hidden from customers';
    }
}

// ==========================================
// 4. STOCK MANAGEMENT
// ==========================================

function initializeStockManagement() {
    const stockInput = document.getElementById('Stock');
    if (stockInput) {
        // Initialize view
        updateStockPreview(stockInput.value);

        // Real-time updates
        stockInput.addEventListener('input', function () {
            updateStockPreview(this.value);
        });

        // Ensure only positive numbers
        stockInput.addEventListener('change', function () {
            if (this.value < 0) this.value = 0;
            updateStockPreview(this.value);
        });
    }
}

function adjustStock(amount) {
    const stockInput = document.getElementById('Stock');
    const currentValue = parseInt(stockInput.value) || 0;
    const newValue = Math.max(0, currentValue + amount); // Prevent negative

    stockInput.value = newValue;
    updateStockPreview(newValue);

    // Add visual feedback for the button clicked
    // Note: This looks for the exact text content in your buttons
    const buttons = document.querySelectorAll('.stock-quick-adjust .btn');
    buttons.forEach(btn => {
        if (btn.innerHTML.includes(amount > 0 ? amount : Math.abs(amount))) {
            btn.classList.add('active');
            setTimeout(() => btn.classList.remove('active'), 200);
        }
    });
}

function updateStockPreview(stockValue) {
    const stock = parseInt(stockValue) || 0;

    // 1. Update Form Text Hint
    const previewText = document.getElementById('stock-preview-text');
    if (previewText) {
        previewText.innerHTML = `<i class="fas fa-info-circle me-1"></i> New value: <strong>${stock}</strong> items available`;
    }

    // 2. Update Summary Card Values
    const currentStockDisplay = document.getElementById('current-stock-display');
    if (currentStockDisplay) {
        currentStockDisplay.textContent = stock;
        currentStockDisplay.classList.add('updated');
        setTimeout(() => currentStockDisplay.classList.remove('updated'), 500);
    }

    // 3. Update Status Text in Card
    const stockStatusDisplay = document.getElementById('stock-status-display');
    if (stockStatusDisplay) {
        let status = stock === 0 ? 'Out of Stock' : stock <= 5 ? 'Low Stock' : 'In Stock';
        stockStatusDisplay.textContent = status;

        // Optional: Colorize the text
        stockStatusDisplay.className = 'stock-info-value updated';
        if (stock === 0) stockStatusDisplay.style.color = '#dc3545';
        else if (stock <= 5) stockStatusDisplay.style.color = '#ffc107';
        else stockStatusDisplay.style.color = '#28a745';
    }

    // 4. Update Alerts (Warnings)
    const stockWarnings = document.getElementById('stock-warnings');
    if (stockWarnings) {
        stockWarnings.innerHTML = ''; // Clear existing

        if (stock === 0) {
            stockWarnings.innerHTML = `
                <div class="alert alert-warning alert-compact">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>Warning:</strong> This item is out of stock.
                </div>
            `;
        } else if (stock <= 5) {
            stockWarnings.innerHTML = `
                <div class="alert alert-info alert-compact">
                    <i class="fas fa-info-circle me-2"></i>
                    <strong>Notice:</strong> Low stock level.
                </div>
            `;
        }
    }
}

// ==========================================
// 5. KEYBOARD SHORTCUTS
// ==========================================
document.addEventListener('keydown', function (e) {
    // Escape key to close full-size image
    if (e.key === 'Escape') {
        closeFullSizeImage();
    }

    // Stock adjustment shortcuts (Ctrl + Arrow keys) when Stock input is focused
    if (e.ctrlKey && document.getElementById('Stock') === document.activeElement) {
        switch (e.key) {
            case 'ArrowUp': e.preventDefault(); adjustStock(5); break;
            case 'ArrowDown': e.preventDefault(); adjustStock(-5); break;
        }
    }
});