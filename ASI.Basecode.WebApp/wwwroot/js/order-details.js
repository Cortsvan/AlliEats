$(document).ready(function () {
    let currentOrderId = null;

    // Show confirm receipt modal when button is clicked
    $('.confirm-receipt-btn').on('click', function () {
        const button = $(this);
        currentOrderId = button.data('order-id');
        const orderNumber = button.data('order-number');

        $('#modalOrderNumber').text(orderNumber);
        $('#confirmReceiptModal').modal('show');
    });

    // Show cancel order modal when button is clicked
    $('.cancel-order-btn').on('click', function () {
        const button = $(this);
        currentOrderId = button.data('order-id');
        const orderNumber = button.data('order-number');

        $('#cancelModalOrderNumber').text(orderNumber);
        $('#cancelOrderModal').modal('show');
    });

    // Confirm receipt submission
    $('#confirmReceiptSubmit').on('click', function () {
        const token = $('input[name="__RequestVerificationToken"]').val();
        const submitButton = $(this);

        // Disable button and show loading
        submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Confirming...');

        $.ajax({
            url: '/Order/ConfirmReceipt',
            type: 'POST',
            data: {
                orderId: currentOrderId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Order receipt confirmed successfully!');
                    $('#confirmReceiptModal').modal('hide');
                    // Reload page to show updated status
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    toastr.error(response.message || 'Failed to confirm receipt.');
                    submitButton.prop('disabled', false).html('<i class="fas fa-check me-2"></i>Yes, I Received It');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred. Please try again.');
                console.error("Error confirming receipt:", xhr.responseText);
                submitButton.prop('disabled', false).html('<i class="fas fa-check me-2"></i>Yes, I Received It');
            }
        });
    });

    // Cancel order submission
    $('#cancelOrderSubmit').on('click', function () {
        const token = $('input[name="__RequestVerificationToken"]').val();
        const submitButton = $(this);

        // Disable button and show loading
        submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Cancelling...');

        $.ajax({
            url: '/Order/CancelOrder',
            type: 'POST',
            data: {
                orderId: currentOrderId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Order cancelled successfully!');
                    $('#cancelOrderModal').modal('hide');
                    // Reload page to show updated status
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    toastr.error(response.message || 'Failed to cancel order.');
                    submitButton.prop('disabled', false).html('<i class="fas fa-times me-2"></i>Yes, Cancel Order');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred. Please try again.');
                console.error("Error cancelling order:", xhr.responseText);
                submitButton.prop('disabled', false).html('<i class="fas fa-times me-2"></i>Yes, Cancel Order');
            }
        });
    });

    // Reset modals when hidden
    $('#confirmReceiptModal, #cancelOrderModal').on('hidden.bs.modal', function () {
        currentOrderId = null;
        $(this).find('button[type="submit"]').prop('disabled', false);
        $('#confirmReceiptSubmit').html('<i class="fas fa-check me-2"></i>Yes, I Received It');
        $('#cancelOrderSubmit').html('<i class="fas fa-times me-2"></i>Yes, Cancel Order');
    });

    // Add animation to order items
    const orderItems = document.querySelectorAll('.order-item-card');
    orderItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateY(20px)';

        setTimeout(() => {
            item.style.transition = 'all 0.4s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateY(0)';
        }, 200 + (index * 100));
    });

    // Add animation to status icon if order is active
    const statusIcon = document.querySelector('.status-icon i');
    const orderStatus = document.querySelector('.status-badge')?.textContent.trim();

    if (statusIcon && (orderStatus === 'Preparing' || orderStatus === 'Confirmed')) {
        statusIcon.style.animation = 'pulse 2s infinite';
    }

    // Add smooth scrolling for action buttons
    const actionButtons = document.querySelectorAll('.action-buttons-card .btn');
    actionButtons.forEach((button, index) => {
        button.style.opacity = '0';
        button.style.transform = 'translateX(20px)';

        setTimeout(() => {
            button.style.transition = 'all 0.3s ease';
            button.style.opacity = '1';
            button.style.transform = 'translateX(0)';
        }, 600 + (index * 100));
    });

    // Add click analytics for action buttons
    actionButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const buttonText = this.textContent.trim();
            console.log(`Order Details: ${buttonText} button clicked`);

            // Add ripple effect
            createRippleEffect(this, e);
        });
    });

    // Add ripple effect for cancel button
    $('.cancel-order-btn').on('click', function (e) {
        createRippleEffect(this, e.originalEvent);
    });

    // Helper function to create ripple effect
    function createRippleEffect(button, event) {
        const ripple = document.createElement('span');
        const rect = button.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = x + 'px';
        ripple.style.top = y + 'px';
        ripple.classList.add('ripple');

        button.style.position = 'relative';
        button.style.overflow = 'hidden';
        button.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    }
});

// Add CSS for ripple effect and cancel button styles
const additionalCSS = `
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

    .cancel-order-btn {
        transition: all 0.3s ease;
        position: relative;
        overflow: hidden;
    }

    .cancel-order-btn:hover {
        background-color: #c82333;
        border-color: #bd2130;
        transform: translateY(-2px);
        box-shadow: 0 4px 8px rgba(220, 53, 69, 0.3);
    }

    .cancel-order-btn:active {
        transform: translateY(0);
    }

    .modal-content {
        border-radius: 15px;
        overflow: hidden;
    }

    .modal-header.bg-danger {
        border-bottom: none;
    }

    .alert-warning {
        border-radius: 10px;
        border: none;
        background: linear-gradient(135deg, #fff3cd, #ffeeba);
        border-left: 4px solid #ffc107;
    }
`;

const style = document.createElement('style');
style.textContent = additionalCSS;
document.head.appendChild(style);