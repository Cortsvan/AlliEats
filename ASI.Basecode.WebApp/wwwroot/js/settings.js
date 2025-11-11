document.addEventListener('DOMContentLoaded', function () {
    // Initialize settings animations
    initializeSettingsAnimations();

    // Initialize settings interactions
    initializeSettingsInteractions();

    // Handle success/error messages
    handleNotifications();

    // Function to initialize animations
    function initializeSettingsAnimations() {
        // Animate settings cards
        const settingsCards = document.querySelectorAll('.settings-card');
        settingsCards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(30px)';

            setTimeout(() => {
                card.style.transition = 'all 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, index * 150);
        });

        // Animate quick actions
        const quickActions = document.querySelectorAll('.quick-action-item');
        quickActions.forEach((action, index) => {
            action.style.opacity = '0';
            action.style.transform = 'scale(0.8)';

            setTimeout(() => {
                action.style.transition = 'all 0.4s ease';
                action.style.opacity = '1';
                action.style.transform = 'scale(1)';
            }, 800 + (index * 100));
        });
    }

    // Function to initialize interactions
    function initializeSettingsInteractions() {
        // Add click effects to settings action buttons
        const actionButtons = document.querySelectorAll('.settings-action-btn');
        actionButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                // Add click feedback
                this.style.transform = 'translateY(0) scale(0.95)';

                setTimeout(() => {
                    this.style.transform = 'translateY(-2px) scale(1)';
                }, 150);

                // Log analytics
                const buttonText = this.textContent.trim();
                console.log(`Settings: Action button clicked - ${buttonText}`);
            });
        });

        // Add hover effects to settings cards
        const settingsCards = document.querySelectorAll('.settings-card');
        settingsCards.forEach(card => {
            card.addEventListener('mouseenter', function () {
                const icon = this.querySelector('.settings-icon');
                if (icon) {
                    icon.style.transform = 'scale(1.1) rotate(5deg)';
                }
            });

            card.addEventListener('mouseleave', function () {
                const icon = this.querySelector('.settings-icon');
                if (icon) {
                    icon.style.transform = 'scale(1) rotate(0deg)';
                }
            });
        });

        // Add click effects to quick actions
        const quickActions = document.querySelectorAll('.quick-action-item');
        quickActions.forEach(action => {
            action.addEventListener('click', function (e) {
                // Add click feedback
                this.style.transform = 'translateY(-5px) scale(0.95)';

                setTimeout(() => {
                    this.style.transform = 'translateY(-5px) scale(1)';
                }, 150);

                // Log analytics
                const actionText = this.querySelector('.quick-action-text').textContent;
                console.log(`Settings: Quick action clicked - ${actionText}`);
            });
        });
    }

    // Function to handle notifications
    function handleNotifications() {
        // Check for success message
        const successMessage = document.querySelector('[data-success-message]');
        if (successMessage) {
            const message = successMessage.getAttribute('data-success-message');
            if (message && typeof toastr !== 'undefined') {
                toastr.success(message);
            }
        }

        // Check for error message
        const errorMessage = document.querySelector('[data-error-message]');
        if (errorMessage) {
            const message = errorMessage.getAttribute('data-error-message');
            if (message && typeof toastr !== 'undefined') {
                toastr.error(message);
            }
        }
    }

    // Add settings icon animations
    const settingsIcons = document.querySelectorAll('.settings-icon');
    settingsIcons.forEach(icon => {
        icon.style.transition = 'transform 0.3s ease';

        // Add periodic subtle animation
        setInterval(() => {
            icon.style.transform = 'scale(1.05)';
            setTimeout(() => {
                icon.style.transform = 'scale(1)';
            }, 200);
        }, 3000 + Math.random() * 2000);
    });

    // Add settings card pulse effect on hover
    settingsCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            const actionBtn = this.querySelector('.settings-action-btn');
            if (actionBtn) {
                actionBtn.style.boxShadow = '0 6px 20px rgba(139, 94, 60, 0.3)';
            }
        });

        card.addEventListener('mouseleave', function () {
            const actionBtn = this.querySelector('.settings-action-btn');
            if (actionBtn) {
                actionBtn.style.boxShadow = '';
            }
        });
    });
});