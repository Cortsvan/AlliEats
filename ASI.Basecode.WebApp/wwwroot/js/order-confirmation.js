document.addEventListener('DOMContentLoaded', function () {
    // Add animation to success icon
    const successIcon = document.querySelector('.success-icon i');
    if (successIcon) {
        successIcon.style.opacity = '0';
        successIcon.style.transform = 'scale(0.5)';

        setTimeout(() => {
            successIcon.style.transition = 'all 0.6s ease';
            successIcon.style.opacity = '1';
            successIcon.style.transform = 'scale(1)';
        }, 200);
    }

    // Add stagger animation to order items
    const orderItems = document.querySelectorAll('.confirmation-item-card');
    orderItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateY(20px)';

        setTimeout(() => {
            item.style.transition = 'all 0.4s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateY(0)';
        }, 300 + (index * 100));
    });

    // Add animation to action buttons
    const actionButtons = document.querySelectorAll('.action-buttons .btn');
    actionButtons.forEach((button, index) => {
        button.style.opacity = '0';
        button.style.transform = 'translateY(10px)';

        setTimeout(() => {
            button.style.transition = 'all 0.3s ease';
            button.style.opacity = '1';
            button.style.transform = 'translateY(0)';
        }, 800 + (index * 100));
    });

    // Auto-focus on track order button for accessibility
    const trackOrderBtn = document.querySelector('a[href*="TrackOrder"]');
    if (trackOrderBtn) {
        setTimeout(() => {
            trackOrderBtn.focus();
        }, 1000);
    }
});