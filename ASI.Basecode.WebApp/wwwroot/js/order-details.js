document.addEventListener('DOMContentLoaded', function () {
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

// Add CSS for ripple effect
const rippleCSS = `
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
`;

const style = document.createElement('style');
style.textContent = rippleCSS;
document.head.appendChild(style);